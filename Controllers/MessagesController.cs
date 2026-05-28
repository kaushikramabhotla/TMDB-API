using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMDB_API.Services;
using TMDB_API.Utils;

namespace TMDB_API.Controllers
{
    [ApiController]

    [Route("api/[controller]")]

    public class MessagesController: ControllerBase
    {
        private readonly MessageService _messageService;

        public MessagesController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [Authorize]
        [HttpGet("{friendId}")]
        public async Task<IActionResult> GetConversation(string friendId)
        {
            var currentUserId = ClaimsExtensions.GetUserId(User).ToString();

            var messages =
                await _messageService.GetConversation(currentUserId,friendId);

            return Ok(messages);
        }
    }
}