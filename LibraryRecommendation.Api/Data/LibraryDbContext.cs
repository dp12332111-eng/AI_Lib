using LibraryRecommendation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryRecommendation.Api.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Rating> Ratings => Set<Rating>();

    public DbSet<ReadingHistory> ReadingHistory => Set<ReadingHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => b.Genre);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Ignore(u => u.FavouriteGenreList);
            entity.Ignore(u => u.FavouriteAuthorList);
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Book)
                .WithMany(b => b.Ratings)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // One rating per user per book.
            entity.HasIndex(r => new { r.UserId, r.BookId }).IsUnique();
        });

        modelBuilder.Entity<ReadingHistory>(entity =>
        {
            entity.HasKey(h => h.Id);

            entity.HasOne(h => h.User)
                .WithMany(u => u.ReadingHistory)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.Book)
                .WithMany(b => b.ReadingHistory)
                .HasForeignKey(h => h.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(h => new { h.UserId, h.BookId }).IsUnique();
        });
    }
}
