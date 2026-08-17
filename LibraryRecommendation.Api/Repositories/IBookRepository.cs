using LibraryRecommendation.Api.Models;

namespace LibraryRecommendation.Api.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Free-text query over title/author/genre, optionally filtered by genre and minimum rating.
    /// </summary>
    Task<IEnumerable<Book>> SearchAsync(
        string? term,
        string? genre = null,
        double? minRating = null,
        CancellationToken cancellationToken = default);
}
