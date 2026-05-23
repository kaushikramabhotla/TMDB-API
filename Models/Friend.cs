using System.ComponentModel.DataAnnotations;

namespace TMDB_API.Models
{
    public class Friend
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid FriendUserId { get; set; }

        public DateTime CreatedAt = DateTime.UtcNow;

        // Navigation
        public User User { get; set; }

        public User FriendUser { get; set; }
    }
}
