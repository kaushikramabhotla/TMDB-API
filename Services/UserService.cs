using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;
using TMDB_API.DTO;
using TMDB_API.Models;
using TMDB_API.Repository;
using TMDB_API.Utils;

namespace TMDB_API.Services
{
    public class UserService
    {
        private TmdbContext _context;

        private readonly IConnectionMultiplexer _redis;

        public UserService(TmdbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }
        public async Task<List<MovieDto>> ShowFavorites(Guid userId)
        {
            return await _context.UserFavorites
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new MovieDto
                {
                    Id = x.Movie.Id,
                    Title = x.Movie.Title,
                    PosterPath = x.Movie.PosterPath,
                    VoteCount = x.Movie.VoteCount,
                    IsFavorite = true
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateUsername(UpdateUsernameDto dto, ClaimsPrincipal principal)
        {
            Guid userId = Guid.Parse
                (principal.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

            string username = dto.Username.Trim().ToLower();
            bool exists = await _context.Users.AnyAsync(u => u.Username == username);

            if (exists)
            {
                return false;
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.Username = username;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserSearchDto>> SearchUsers(string query, ClaimsPrincipal principal)
        {
            Guid currentUserId = ClaimsExtensions.GetUserId(principal);

            query = query
                .Trim()
                .ToLower();

            return await _context.Users
                .Where(u =>
                    u.Username != null &&
                    u.Username
                        .ToLower()
                        .Contains(query))
                .Select(u =>
                    new UserSearchDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Name = u.Name,
                        RequestSent =
                            _context.FriendRequest
                                .Any(fr =>
                                    fr.SenderId == currentUserId &&
                                    fr.ReceiverId== u.Id &&
                                    fr.Status == "Pending"),
                        AlreadyFriends =
                                _context.Friend.Any(f =>
                                    (f.UserId == currentUserId && f.FriendUserId == u.Id)
                                    ||
                                    (f.UserId == u.Id && f.FriendUserId == currentUserId)
                                )
                    })
                .Take(10)
                .ToListAsync();
        }

        public async Task<bool> SendFriendrequest(Guid receiverId, ClaimsPrincipal principal)
        {
            Guid senderId = ClaimsExtensions.GetUserId(principal);
            if(receiverId == senderId)
            {
                return false;
            }

            bool alreadyExists = await _context.FriendRequest
                .AnyAsync
                (x => x.ReceiverId == receiverId
                && x.SenderId == senderId
                && x.Status == "Pending");

            if (alreadyExists)
                return false;

            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
            };

            _context.FriendRequest.Add(request);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserSearchDto>> GetFriendRequests(ClaimsPrincipal principal)
        {
            Guid currentUserId = Guid.Parse(
                principal.FindFirst(
                    ClaimTypes.NameIdentifier)!
                    .Value);

            return await _context.FriendRequest
                .Where(fr => fr.ReceiverId == currentUserId && fr.Status == "Pending")
                .Select(fr => new UserSearchDto
                {
                    Id = fr.Sender.Id,
                    Username = fr.Sender.Username,
                    Name = fr.Sender.Name
                }).ToListAsync();
        }

        public async Task<bool> AcceptFriendRequest(Guid senderId, ClaimsPrincipal principal)
        {
            Guid receiverId = ClaimsExtensions.GetUserId(principal);

            // BUG FIX: was "pending" lowercase — now matches "Pending"
            var request = await _context.FriendRequest
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId
                && fr.ReceiverId == receiverId
                && fr.Status == "Pending");

            if (request == null) return false;

            // Create bidirectional friendship
            var friendship = new Friend { UserId = senderId, FriendUserId = receiverId };
            var reverseFriendship = new Friend { UserId = receiverId, FriendUserId = senderId };

            _context.Friend.Add(friendship);
            _context.Friend.Add(reverseFriendship);

            // Delete the request from DB after accepting
            _context.FriendRequest.Remove(request);

            await _context.SaveChangesAsync();

            // Publish notification to Redis
            // The sender (senderId) needs to be notified
            var subscriber = _redis.GetSubscriber();
            var payload = JsonSerializer.Serialize(new NotificationPayload
            {
                TargetUserId = senderId.ToString(),
                Type = "accepted",
                Message = "Your friend request was accepted!"
            });
            await subscriber.PublishAsync("notifications", payload);

            return true;
        }

        public async Task<bool> RejectRequest(Guid requestId, ClaimsPrincipal user)
        {
            var userId = ClaimsExtensions.GetUserId(user);

            var request = await _context.FriendRequest
                .FirstOrDefaultAsync(fr =>
                    fr.Id == requestId &&
                    fr.ReceiverId == userId &&
                    fr.Status == "Pending");

            if (request == null) return false;

            Guid senderId = request.SenderId;

            // Delete request from DB on rejection
            _context.FriendRequest.Remove(request);
            await _context.SaveChangesAsync();

            // Notify sender of rejection via Redis
            var subscriber = _redis.GetSubscriber();
            var payload = JsonSerializer.Serialize(new NotificationPayload
            {
                TargetUserId = senderId.ToString(),
                Type = "rejected",
                Message = "Your friend request was declined."
            });
            await subscriber.PublishAsync("notifications", payload);

            return true;
        }

        public async Task<List<UserSearchDto>> GetFriends(ClaimsPrincipal principal)
        {
            Guid userId = ClaimsExtensions.GetUserId(principal);

            return await _context.Friend
                .Where(f => f.UserId == userId)
                .Select(f => new UserSearchDto
                {
                    Id = f.FriendUser.Id,
                    Username = f.FriendUser.Username,
                    Name = f.FriendUser.Name
                })
                .ToListAsync();
        }
    }
}
