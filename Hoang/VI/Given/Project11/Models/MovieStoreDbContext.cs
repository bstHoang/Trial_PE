using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Project11.Models;

public partial class MovieStoreDbContext : DbContext
{
    public MovieStoreDbContext()
    {
    }

    public MovieStoreDbContext(DbContextOptions<MovieStoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Director> Directors { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<MovieStar> MovieStars { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Star> Stars { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Đọc file appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("MyCnn");

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Director>(entity =>
        {
            entity.HasKey(e => e.DirectorId).HasName("PK__Director__26C69E26171AF19B");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("PK__Movies__4BD2943A545278BE");

            entity.Property(e => e.AvailableCopies).HasDefaultValue(0);

            entity.HasOne(d => d.Director).WithMany(p => p.Movies).HasConstraintName("FK__Movies__Director__3C69FB99");
        });

        modelBuilder.Entity<MovieStar>(entity =>
        {
            entity.HasKey(e => new { e.MovieId, e.StarId }).HasName("PK__MovieSta__2BB8285EEE676A58");

            entity.HasOne(d => d.Movie).WithMany(p => p.MovieStars)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MovieStar__Movie__3F466844");

            entity.HasOne(d => d.Star).WithMany(p => p.MovieStars)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MovieStar__StarI__403A8C7D");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AE28450651");

            entity.Property(e => e.ReviewDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Movie).WithMany(p => p.Reviews).HasConstraintName("FK__Reviews__MovieID__4316F928");
        });

        modelBuilder.Entity<Star>(entity =>
        {
            entity.HasKey(e => e.StarId).HasName("PK__Stars__06ABC647D3B2AD3A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
