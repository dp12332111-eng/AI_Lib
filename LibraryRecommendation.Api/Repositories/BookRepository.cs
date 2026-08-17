using LibraryRecommendation.Api.Data;
using LibraryRecommendation.Api.Models;
using LibraryRecommendation.Api.Services.Recommendations;
using Microsoft.EntityFrameworkCore;

namespace LibraryRecommendation.Api.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;
    private readonly IBookVectorCache _vectorCache;

    public BookRepository(LibraryDbContext context, IBookVectorCache vectorCache)
    {
        _context = context;
        _vectorCache = vectorCache;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);
        _vectorCache.Invalidate();
        return book;
    }

    public async Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Books
            .AsNoTracking()
            .AnyAsync(b => b.Id == book.Id, cancellationToken);

        if (!exists)
        {
            return false;
        }

        _context.Books.Update(book);
        await _context.SaveChangesAsync(cancellationToken);
        _vectorCache.Invalidate();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (book is null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);
        _vectorCache.Invalidate();
        return true;
    }

    public async Task<IEnumerable<Book>> SearchAsync(
        string? term,
        string? genre = null,
        double? minRating = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Books.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(b =>
                b.Title.Contains(term) ||
                b.Author.Contains(term) ||
                b.Genre.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(b => b.Genre == genre);
        }

        if (minRating.HasValue)
        {
            query = query.Where(b => b.Rating >= minRating.Value);
        }

        return await query
            .OrderByDescending(b => b.Rating)
            .ThenBy(b => b.Title)
            .ToListAsync(cancellationToken);
    }
}
