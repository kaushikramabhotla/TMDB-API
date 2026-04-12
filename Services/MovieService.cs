using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TMDB_API.Models;
using TMDB_API.Repository;

namespace TMDB_API.Services
{
    public class MovieService : IMovieService
    {
        private TmdbContext _context;
        private IMemoryCache _cache;

        public MovieService(TmdbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<Movie> GetMovie(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        public async Task<List<Movie>> GetMovies(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            /*
             * Page 1 will have :  00 Skip, Take 10
             * page 2 will have :  10 Skip, Take 10
             */
            return await _context.Movies
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Movie>> GetTop10Movies()
        {
            string cacheKey = "top10_movies";
            if(!_cache.TryGetValue(cacheKey, out List<Movie> movies))
            {
                 movies = await _context.Movies
                    .AsNoTracking()
                    .OrderByDescending(x => x.VoteCount)
                    .Take(10).ToListAsync();
                _cache.Set(cacheKey, movies);
            }
            return movies;
        }

        public async Task<List<Movie>> SearchMovies(string query)
        {
            var likeQuery = $"%{query}%";

            var result = await _context.Movies
                .AsNoTracking()
                .Where(m =>
                    EF.Functions.Like(m.Title ?? "", likeQuery) ||
                    EF.Functions.Like(m.Tagline ?? "", likeQuery) ||
                    EF.Functions.Like(m.Overview ?? "", likeQuery)
                )
                .Select(m => new
                {
                    Movie = m,

                    Score =
                        (EF.Functions.Like(m.Title ?? "", likeQuery) ? 3 : 0) +
                        (EF.Functions.Like(m.Tagline ?? "", likeQuery) ? 2 : 0) +
                        (EF.Functions.Like(m.Overview ?? "", likeQuery) ? 1 : 0)
                })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Movie)
                .Take(20)

                .ToListAsync();

            return result;
        }

        public async Task ToggleFavorite(int userId, int movieId)
        {
            var existing = await _context.UserFavorites
                .FirstOrDefaultAsync(x => x.UserId == userId && x.MovieId == movieId);

            if (existing != null)
            {
                _context.UserFavorites.Remove(existing);
            }
            else
            {
                _context.UserFavorites.Add(new UserFavorite
                {
                    UserId = userId,
                    MovieId = movieId
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
