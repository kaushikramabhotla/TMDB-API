using System.ComponentModel.DataAnnotations;

namespace TMDB_API.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Username { get; set; }

        [Required]
        public string Provider { get; set; }

        [Required]
        public string ProviderUserId { get; set; }

        public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();

        public ICollection<Friend> Friends { get; set; } = new List<Friend>();

        public ICollection<FriendRequest> SentRequests { get; set; } = new List<FriendRequest>();

        public ICollection<FriendRequest> ReceivedRequests { get; set; } = new List<FriendRequest>();
    }
}
