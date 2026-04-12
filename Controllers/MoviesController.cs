using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TMDB_API.Repository;
using TMDB_API.Services;

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
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> getTop10Movies()
        {
            return Ok(await _movieService.GetTop10Movies());
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [EnableRateLimiting("UserPolicy")]
        public async Task<IActionResult> getMovieById(int id)
        {
            return Ok(await _movieService.GetMovie(id));
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
        public async Task<IActionResult> SearchMovies([FromQuery] string query)
        {
            var movies = await _movieService.SearchMovies(query);
            return Ok(movies);
        }

        [HttpPut]
        [Authorize]
        [EnableRateLimiting("UserPolicy")]
        public async Task<IActionResult> ToggleFavorite([FromBody] int movieId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _movieService.ToggleFavorite(userId, movieId);

            return Ok();
        }
    }
}
