using LibraryRecommendation.Api.Models;
using LibraryRecommendation.Api.Repositories;
using LibraryRecommendation.Api.Services.Recommendations;

namespace LibraryRecommendation.Api.Services;

/// <summary>
/// Content-based recommender written in plain C#: TF-IDF vectors over each book's
/// Title + Author + Genre + Description, compared to a user profile with cosine similarity.
/// </summary>
public class RecommendationService : IRecommendationService
{
    /// <summary>A book rated 4 counts once, a book rated 5 counts twice.</summary>
    private const int LikedRatingThreshold = 4;

    /// <summary>Weight of stated genres/authors once the rating-based profile exists (unit vectors).</summary>
    private const double StatedPreferenceBoost = 0.5;

    private const int MaxReasons = 4;

    private readonly IBookVectorCache _vectorCache;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;

    public RecommendationService(
        IBookVectorCache vectorCache,
        IBookRepository bookRepository,
        IUserRepository userRepository)
    {
        _vectorCache = vectorCache;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<BookRecommendation>> GetRecommendationsForUserAsync(
        int userId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count < 1)
        {
            count = 1;
        }

        var model = await _vectorCache.GetModelAsync(cancellationToken);
        var books = (await _bookRepository.GetAllAsync(cancellationToken)).ToList();

        if (books.Count == 0)
        {
            return [];
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        var ratings = user is null
            ? []
            : (await _userRepository.GetRatingsAsync(userId, cancellationToken)).ToList();
        var history = user is null
            ? []
            : (await _userRepository.GetReadingHistoryAsync(userId, cancellationToken)).ToList();

        // Books the user has already rated, read or is currently reading are not candidates.
        // "Want to read" stays eligible — they haven't read it yet.
        var excluded = new HashSet<int>(ratings.Select(r => r.BookId));
        foreach (var entry in history.Where(h => h.Status is ReadingStatus.Read or ReadingStatus.Reading))
        {
            excluded.Add(entry.BookId);
        }

        var (profile, strategy) = BuildProfile(user, ratings, model);

        var recommendations = profile.Count > 0
            ? ScoreCandidates(books, excluded, profile, model, strategy, count)
            : [];

        // Cold start and top-up: never hand the UI an empty list.
        if (recommendations.Count < count)
        {
            var alreadyChosen = new HashSet<int>(recommendations.Select(r => r.Book.Id));
            recommendations.AddRange(
                TopRatedFallback(books, excluded, alreadyChosen, count - recommendations.Count));
        }

        if (recommendations.Count == 0)
        {
            // Every book is excluded (the user has read the entire library) — fall back to
            // the best books overall rather than returning nothing.
            recommendations.AddRange(TopRatedFallback(books, [], [], count));
        }

        ApplyMatchPercentages(recommendations);
        return recommendations;
    }

    /// <summary>
    /// Rating-weighted average of the vectors of books the user liked, nudged towards their stated
    /// favourites. Falls back to stated favourites alone when there are no ratings to learn from.
    /// </summary>
    private static (Dictionary<string, double> Profile, RecommendationStrategy Strategy) BuildProfile(
        User? user,
        IReadOnlyList<Rating> ratings,
        TfIdfModel model)
    {
        var profile = new Dictionary<string, double>();

        var liked = ratings.Where(r => r.Stars >= LikedRatingThreshold).ToList();
        foreach (var rating in liked)
        {
            if (model.Vectors.TryGetValue(rating.BookId, out var vector))
            {
                // 4 stars -> weight 1, 5 stars -> weight 2.
                TfIdfModel.AddScaled(profile, vector, rating.Stars - (LikedRatingThreshold - 1));
            }
        }

        TfIdfModel.Normalize(profile);

        var preferences = BuildPreferenceVector(user, model);

        if (profile.Count > 0)
        {
            if (preferences.Count > 0)
            {
                TfIdfModel.AddScaled(profile, preferences, StatedPreferenceBoost);
                TfIdfModel.Normalize(profile);
            }

            return (profile, RecommendationStrategy.ContentBased);
        }

        // No usable ratings: stated genres and authors are all we know.
        return (preferences, RecommendationStrategy.StatedPreferences);
    }

    private static Dictionary<string, double> BuildPreferenceVector(User? user, TfIdfModel model)
    {
        var vector = new Dictionary<string, double>();
        if (user is null)
        {
            return vector;
        }

        // Vectorise each favourite the same way a book's genre/author is written into its corpus,
        // so a stated "Science Fiction" lands on the same phrase token the sci-fi books carry.
        foreach (var genre in user.FavouriteGenreList)
        {
            TfIdfModel.AddScaled(vector, model.Vectorize(BookCorpus.BuildPreferenceText(genre)), 1.0);
        }

        foreach (var author in user.FavouriteAuthorList)
        {
            TfIdfModel.AddScaled(vector, model.Vectorize(BookCorpus.BuildPreferenceText(author)), 1.0);
        }

        TfIdfModel.Normalize(vector);
        return vector;
    }

    private static List<BookRecommendation> ScoreCandidates(
        IEnumerable<Book> books,
        HashSet<int> excluded,
        Dictionary<string, double> profile,
        TfIdfModel model,
        RecommendationStrategy strategy,
        int count)
    {
        var scored = new List<BookRecommendation>();

        foreach (var book in books)
        {
            if (excluded.Contains(book.Id) || !model.Vectors.TryGetValue(book.Id, out var vector))
            {
                continue;
            }

            var score = TfIdfModel.CosineSimilarity(profile, vector);
            if (score <= 0.0)
            {
                continue;
            }

            scored.Add(new BookRecommendation
            {
                Book = book,
                Score = score,
                Strategy = strategy,
                Reasons = model.ExplainMatch(profile, vector, MaxReasons)
            });
        }

        return scored
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.Book.Rating)
            .Take(count)
            .ToList();
    }

    private static IEnumerable<BookRecommendation> TopRatedFallback(
        IEnumerable<Book> books,
        HashSet<int> excluded,
        HashSet<int> alreadyChosen,
        int count)
    {
        return books
            .Where(b => !excluded.Contains(b.Id) && !alreadyChosen.Contains(b.Id))
            .OrderByDescending(b => b.Rating)
            .ThenBy(b => b.Title)
            .Take(count)
            .Select(b => new BookRecommendation
            {
                Book = b,
                Score = 0.0,
                Strategy = RecommendationStrategy.TopRated
            });
    }

    /// <summary>
    /// Turns raw cosine scores into a percentage relative to the best match in the set. Cosine
    /// values over a sparse corpus are small in absolute terms (0.1 is a strong match), so showing
    /// them raw would read as "10% match" for a genuinely good recommendation.
    /// </summary>
    private static void ApplyMatchPercentages(List<BookRecommendation> recommendations)
    {
        var best = recommendations.Max(r => r.Score);

        foreach (var recommendation in recommendations)
        {
            recommendation.MatchPercentage = best > 0.0 && recommendation.Score > 0.0
                ? Math.Clamp((int)Math.Round(recommendation.Score / best * 100), 1, 100)
                // Top-rated fallback entries have no similarity score; use the book's own rating.
                : Math.Clamp((int)Math.Round(recommendation.Book.Rating / 5.0 * 100), 1, 100);
        }
    }
}
