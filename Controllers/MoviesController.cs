using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TMDB_API.Repository;
using TMDB_API.Services;
using TMDB_API.Utils;

namespace TMDB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {

        private readonly IMovieService _movieService;
        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet("top10")]
        [Authorize]
        [EnableRateLimiting("UserPolicy")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> getTop10Movies()
        {
            Guid userId = ClaimsExtensions.GetUserId(User);

            return Ok(await _movieService.GetTop10Movies(userId));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [EnableRateLimiting("UserPolicy")]
        public async Task<IActionResult> getMovieById(int id)
        {
            Guid userId = ClaimsExtensions.GetUserId(User);
            return Ok(await _movieService.GetMovie(id, userId));
        }

        [HttpGet]
        [Authorize]
        [EnableRateLimiting("UserPolicy")]
        public async Task<IActionResult> GetMovies(int page = 1, int pageSize = 10)
        {
            var movies = await _movieService.GetMovies(page, pageSize);
            return Ok(movies);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchMovies([FromQuery] string query)
        {
            Guid userid = ClaimsExtensions.GetUserId(User);
            var movies = await _movieService.Search(query, userid);
            return Ok(movies);
        }

        [HttpPut]
        [Authorize]
        [EnableRateLimiting("UserPolicy")]
        public async Task<IActionResult> ToggleFavorite([FromBody] int movieId)
        {
            Guid userId = ClaimsExtensions.GetUserId(User);

            await _movieService.ToggleFavorite(userId, movieId);

            return Ok();
        }
    }
}
