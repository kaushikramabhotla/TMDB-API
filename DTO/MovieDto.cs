namespace TMDB_API.DTO
{
    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public short? VoteCount { get; set; }

        public bool IsFavorite { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }
    }
}
