using LibraryRecommendation.Api.Models;

namespace LibraryRecommendation.Api.Services;

public interface IRecommendationService
{
    /// <summary>
    /// Top <paramref name="count"/> books for a user, ranked by TF-IDF cosine similarity against a
    /// profile built from their ratings and stated preferences. Never returns an empty list: users
    /// with no ratings fall back to their favourite genres/authors, and users with neither fall
    /// back to the library's highest-rated books.
    /// </summary>
    Task<IReadOnlyList<BookRecommendation>> GetRecommendationsForUserAsync(
        int userId,
        int count = 10,
        CancellationToken cancellationToken = default);
}
