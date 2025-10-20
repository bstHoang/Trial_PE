using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Server.Model;

public partial class LibraryContext : DbContext
{
    public LibraryContext()
    {
    }

    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookCopy> BookCopies { get; set; }

    public virtual DbSet<BorrowHistory> BorrowHistories { get; set; }

    public virtual DbSet<Borrower> Borrowers { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string connectionString = configuration.GetConnectionString("MyCnn")!;
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__Authors__70DAFC3468E3B846");

            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C2077C52B94C");

            entity.Property(e => e.Title).HasMaxLength(300);

            entity.HasOne(d => d.Genre).WithMany(p => p.Books)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Books_Genres");

            entity.HasMany(d => d.Authors).WithMany(p => p.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "BookAuthor",
                    r => r.HasOne<Author>().WithMany()
                        .HasForeignKey("AuthorId")
                        .HasConstraintName("FK_BookAuthors_Authors"),
                    l => l.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .HasConstraintName("FK_BookAuthors_Books"),
                    j =>
                    {
                        j.HasKey("BookId", "AuthorId");
                        j.ToTable("BookAuthors");
                    });
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(e => e.CopyId).HasName("PK__BookCopi__C26CCCC5DF8D7C7B");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("available");

            entity.HasOne(d => d.Book).WithMany(p => p.BookCopies)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_BookCopies_Books");
        });

        modelBuilder.Entity<BorrowHistory>(entity =>
        {
            entity.HasKey(e => e.BorrowId).HasName("PK__BorrowHi__4295F83F0A4F7077");

            entity.ToTable("BorrowHistory");

            entity.HasOne(d => d.Borrower).WithMany(p => p.BorrowHistories)
                .HasForeignKey(d => d.BorrowerId)
                .HasConstraintName("FK_BorrowHistory_Borrower");

            entity.HasOne(d => d.Copy).WithMany(p => p.BorrowHistories)
                .HasForeignKey(d => d.CopyId)
                .HasConstraintName("FK_BorrowHistory_Copy");
        });

        modelBuilder.Entity<Borrower>(entity =>
        {
            entity.HasKey(e => e.BorrowerId).HasName("PK__Borrower__568EDB575F92CC3D");

            entity.HasIndex(e => e.Email, "UX_Borrowers_Email")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("PK__Genres__0385057E54B6B80C");

            entity.Property(e => e.GenreName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
