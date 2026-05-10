namespace TMDB_API.Models
{
    public class User
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string Provider { get; set; }
        public string ProviderUserId { get; set; }

        // Navigation => what movies this user likes
        public ICollection<UserFavorite> Favorites { get; set; }
    }
}
