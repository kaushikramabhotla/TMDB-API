using TMDB_API.Models;

namespace TMDB_API.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetTop10Movies();
        Task<Movie> GetMovie(int id);
        Task<List<Movie>> GetMovies(int page, int pageSize);
        Task<List<Movie>> SearchMovies(string query);
        Task ToggleFavorite(int userId, int movieId);
    }
}
