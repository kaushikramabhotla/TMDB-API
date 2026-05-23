using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TMDB_API.DTO;
using TMDB_API.Repository;
using TMDB_API.Services;
using TMDB_API.Utils;

namespace TMDB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService service)
        {
            _userService = service;
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> Getfavorites()
        {
            Guid userId = ClaimsExtensions.GetUserId(User);
            return Ok(await _userService.ShowFavorites(userId));
        }

        [Authorize]
        [HttpPut("username")]
        public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameDto dto)
        {
            bool success = await _userService.UpdateUsername(dto,User);
            if (!success)
            {
                return BadRequest("Username already taken");
            }
            return Ok();
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(string query)
        {
            var users = await _userService.SearchUsers(query, User);
            return Ok(users);
        }

        [Authorize]
        [HttpPost("friend-request")]
        public async Task<IActionResult> SendFriendrequest(Guid receiverId)
        {
            bool success = await _userService.SendFriendrequest(receiverId, User);

            if(!success)
            {
                return BadRequest("Unable to Send Request");
            }

            return Ok();
        }

        [Authorize]
        [HttpGet("friend-requests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            var requests = await _userService.GetFriendRequests(User);

            return Ok(requests);
        }

        [Authorize]
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptRequest(Guid requestId)
        {
            var requests = await _userService.AcceptFriendRequest(requestId, User);

            return Ok(requests);
        }

        [Authorize]
        [HttpPost("reject")]
        public async Task<IActionResult> RejectRequest([FromBody] Guid requestId)
        {
            bool success = await _userService.RejectRequest(requestId, User);

            if (!success)
                return BadRequest();

            return Ok();
        }
    }
}
