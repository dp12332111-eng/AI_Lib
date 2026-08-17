using LibraryRecommendation.Api.Models;

namespace LibraryRecommendation.Api.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>User with Ratings and ReadingHistory eagerly loaded (read-only).</summary>
    Task<User?> GetWithHistoryAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Rating>> GetRatingsAsync(int userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ReadingHistory>> GetReadingHistoryAsync(int userId, CancellationToken cancellationToken = default);
}
