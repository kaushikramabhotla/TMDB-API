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
        public DateOnly? ReleaseDate { get; internal set; }
        public int? Runtime { get; internal set; }
        public double? VoteAverage { get; internal set; }
        public string? Genres { get; internal set; }

        public List<CreditDto> Credits { get; set; }
    }
}
