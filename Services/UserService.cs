using Microsoft.EntityFrameworkCore;
using TMDB_API.Models;
using TMDB_API.Repository;

namespace TMDB_API.Services
{
    public class UserService
    {
        private TmdbContext _context;

        public UserService(TmdbContext context)
        {
            _context = context;
        }
        public async Task<List<Movie>> ShowFavorites(int userId)
        {
            return await _context.UserFavorites
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.Movie)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
