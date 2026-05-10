using TMDB_API.DTO;
using TMDB_API.Models;

namespace TMDB_API.Services
{
    public interface IMovieService
    {
        Task<List<MovieDto>> GetTop10Movies(Guid userId);
        Task<MovieDto> GetMovie(int id, Guid userId);
        Task<List<Movie>> GetMovies(int page, int pageSize);
        Task<List<MovieDto>> Search(string query, Guid userId);
        Task ToggleFavorite(Guid userId, int movieId);
    }
}
