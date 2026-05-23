namespace TMDB_API.DTO
{
    public class UserSearchDto
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public bool RequestSent { get; set; }

        public bool AlreadyFriends {get; set;}
    }
}