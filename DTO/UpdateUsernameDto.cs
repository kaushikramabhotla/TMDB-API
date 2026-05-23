using System.ComponentModel.DataAnnotations;

namespace TMDB_API.DTO
{
    public class UpdateUsernameDto
    {
        [Required]
        [MinLength(6)]
        [MaxLength(15)]
        public string Username
        { get; set; }
    }
}