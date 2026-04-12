using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TMDB_API.Models;

namespace TMDB_API.Repository;

public partial class TmdbContext : DbContext
{

    private readonly IConfiguration _config;
    public TmdbContext()
    {
    }

    public TmdbContext(DbContextOptions<TmdbContext> options, IConfiguration configuration)
        : base(options)
    {
        _config = configuration;
    }

    public virtual DbSet<Credit> Credits { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserFavorite> UserFavorites { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<UserFavorite>()
            .HasKey(x => new { x.UserId, x.MovieId });

        modelBuilder.Entity<User>();
        modelBuilder.Entity<UserFavorite>();

        //User -> UserFavorite
        modelBuilder.Entity<UserFavorite>()
            .HasOne(x => x.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(x => x.UserId);

        //Movie -> UserFavorite
        modelBuilder.Entity<UserFavorite>()
            .HasOne(x => x.Movie)
            .WithMany(u => u.FavoritedBy)
            .HasForeignKey(x => x.MovieId);

        modelBuilder.Entity<Credit>(entity =>
        {
            entity.HasKey(e => e.MovieId);

            entity.Property(e => e.MovieId)
                .ValueGeneratedNever()
                .HasColumnName("movie_id");
            entity.Property(e => e.Cast).HasColumnName("cast");
            entity.Property(e => e.Crew).HasColumnName("crew");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Budget).HasColumnName("budget");
            entity.Property(e => e.Genres).HasColumnName("genres");
            entity.Property(e => e.Homepage)
                .HasMaxLength(450)
                .HasColumnName("homepage");
            entity.Property(e => e.Keywords).HasColumnName("keywords");
            entity.Property(e => e.OriginalLanguage)
                .HasMaxLength(50)
                .HasColumnName("original_language");
            entity.Property(e => e.OriginalTitle)
                .HasMaxLength(100)
                .HasColumnName("original_title");
            entity.Property(e => e.Overview).HasColumnName("overview");
            entity.Property(e => e.Popularity).HasColumnName("popularity");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Revenue).HasColumnName("revenue");
            entity.Property(e => e.Runtime).HasColumnName("runtime");
            entity.Property(e => e.SpokenLanguages).HasColumnName("spoken_languages");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Tagline).HasColumnName("tagline");
            entity.Property(e => e.Title)
                .HasMaxLength(450)
                .HasColumnName("title");
            entity.Property(e => e.VoteAverage).HasColumnName("vote_average");
            entity.Property(e => e.VoteCount).HasColumnName("vote_count");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
