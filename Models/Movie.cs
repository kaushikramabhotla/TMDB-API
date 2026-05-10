using System;
using System.Collections.Generic;

namespace TMDB_API.Models;

public partial class Movie
{
    public int? Budget { get; set; }

    public string? Genres { get; set; }

    public string? Homepage { get; set; }

    public int Id { get; set; }

    public string? Keywords { get; set; }

    public string? OriginalLanguage { get; set; }

    public string? OriginalTitle { get; set; }

    public string? Overview { get; set; }

    public double? Popularity { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public long? Revenue { get; set; }

    public int? Runtime { get; set; }

    public string? SpokenLanguages { get; set; }

    public string? Status { get; set; }

    public string? Tagline { get; set; }

    public string? Title { get; set; }

    public double? VoteAverage { get; set; }

    public short? VoteCount { get; set; }

    public string? PosterPath { get; set; }

    public string? BackdropPath { get; set; }

    // Navigation => which users like this movie
    public ICollection<UserFavorite> FavoritedBy { get; set; }
}
