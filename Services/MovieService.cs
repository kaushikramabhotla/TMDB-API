using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using TMDB_API.DTO;
using TMDB_API.Models;
using TMDB_API.Repository;

namespace TMDB_API.Services
{
    public class MovieService : IMovieService
    {
        private TmdbContext _context;
        private IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public MovieService(TmdbContext context, IMemoryCache cache, IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<MovieDto?> GetMovie(int id, Guid userId)
        {
            var favoriteIds = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.MovieId)
                .ToHashSetAsync();

            var movie = await _context.Movies
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new MovieDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Overview = m.Overview,
                    VoteCount = m.VoteCount,
                    IsFavorite = favoriteIds.Contains(m.Id)
                })
                .FirstOrDefaultAsync();

            return movie;
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

        public async Task<List<MovieDto>> GetTop10Movies(Guid userId)
        {
            string cacheKey = "top10_movies";

            // Step 1: Get cached movies (no user data)
            if (!_cache.TryGetValue(cacheKey, out List<Movie> movies))
            {
                movies = await _context.Movies
                    .AsNoTracking()
                    .OrderByDescending(x => x.VoteCount)
                    .Take(10)
                    .ToListAsync();

                _cache.Set(cacheKey, movies, TimeSpan.FromMinutes(10));
            }

            foreach (var movie in movies)
            {
                await GetPicturePath(movie.Id);
            }
            //var user = await _context.Users.Where(u => u.Id == userId);
            // Step 2: Get user's favorite IDs
            var favoriteIds = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.MovieId)
                .ToListAsync();

            // Step 3: Map to DTO with isFavorite
            var result = movies.Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                VoteCount = m.VoteCount,
                Overview = m.Overview,
                PosterPath = m.PosterPath,
                BackdropPath = m.BackdropPath,
                IsFavorite = favoriteIds.Contains(m.Id)
            }).ToList();

            return result;
        }

        public async Task<List<MovieDto>> Search(string query, Guid userId)
        {
            var favoriteIds = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.MovieId)
                .ToListAsync();


            var movies = await _context.Movies
                .Where(m => m.Title.Contains(query))
                .Select(m => new MovieDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    PosterPath = m.PosterPath,
                    BackdropPath= m.BackdropPath,
                    IsFavorite = favoriteIds.Contains(m.Id)
                }).Take(10)
                .ToListAsync();

            foreach (var movie in movies)
            {
                await GetPicturePath(movie.Id);
            }

            return movies;
        }

        public async Task ToggleFavorite(Guid userId, int movieId)
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

        private async Task<string?> GetPicturePath(int movieId)
        {
            if (movieId <= 0)
                return null;

            var picturePath = await _context.Movies
                .AsNoTracking()
                .Where(m => m.Id == movieId)
                .Select(m => m.PosterPath)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(picturePath))
            {
                return picturePath;
            }

            var client = new HttpClient();

            string tmdbApiKey = _configuration["TMDB:ApiKey"];

            string url =
                $"https://api.themoviedb.org/3/movie/{movieId}?api_key={tmdbApiKey}";

            var response = await client.GetAsync(url);

            var json = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(json);

            string posterPath =
                doc.RootElement.GetProperty("poster_path").GetString();

            string backdropPath =
                doc.RootElement.GetProperty("backdrop_path").GetString();

            if (string.IsNullOrEmpty(posterPath))
                return null;

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == movieId);

            movie.PosterPath = posterPath;

            movie.BackdropPath = backdropPath;

            await _context.SaveChangesAsync();

            return posterPath;
        }

        public async Task<List<MovieDto>> GetFavorites(Guid userId)
        {
            return await _context.UserFavorites

                .Where(f => f.UserId == userId)

                .Select(f => new MovieDto
                {
                    Id = f.Movie.Id,
                    Title = f.Movie.Title,
                    PosterPath = f.Movie.PosterPath,
                    VoteCount = f.Movie.VoteCount,
                    IsFavorite = true
                })

                .ToListAsync();
        }
    }
}
