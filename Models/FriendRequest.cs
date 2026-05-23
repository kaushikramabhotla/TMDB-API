using System.ComponentModel.DataAnnotations;

namespace TMDB_API.Models
{
    public class FriendRequest
    {
        public Guid Id { get; set; }

        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt = DateTime.UtcNow;

        // Navigation
        public User Sender { get; set; }

        public User Receiver { get; set; }
    }
}
