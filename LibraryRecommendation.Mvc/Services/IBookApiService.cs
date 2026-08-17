using LibraryRecommendation.Mvc.Models;

namespace LibraryRecommendation.Mvc.Services;

public interface IBookApiService
{
    Task<IEnumerable<BookViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<BookViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<BookViewModel>> SearchAsync(
        string? term,
        string? genre = null,
        double? minRating = null,
        CancellationToken cancellationToken = default);

    Task<BookViewModel?> CreateAsync(BookViewModel book, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(BookViewModel book, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
