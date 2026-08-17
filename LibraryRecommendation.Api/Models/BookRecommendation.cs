namespace LibraryRecommendation.Api.Models;

/// <summary>How a recommendation was produced — surfaced so the UI can be honest about cold starts.</summary>
public enum RecommendationStrategy
{
    /// <summary>Content-based: matched against a profile built from books the user rated 4+.</summary>
    ContentBased = 0,

    /// <summary>Cold start: no ratings, so matched against stated favourite genres/authors.</summary>
    StatedPreferences = 1,

    /// <summary>Cold start: nothing known about the user, so the library's highest-rated books.</summary>
    TopRated = 2
}

public class BookRecommendation
{
    public required Book Book { get; init; }

    /// <summary>Raw cosine similarity against the user profile (0–1). Zero for the TopRated strategy.</summary>
    public double Score { get; init; }

    /// <summary>
    /// Score expressed relative to the strongest match in this result set, so the list reads
    /// sensibly in the UI. Raw cosine values are inherently small and mean little on their own.
    /// </summary>
    public int MatchPercentage { get; set; }

    /// <summary>Terms that contributed most to the score — the "recommended because" list.</summary>
    public IReadOnlyList<string> Reasons { get; init; } = [];

    public RecommendationStrategy Strategy { get; init; }

    /// <summary>Ready-made sentence for the UI.</summary>
    public string Explanation =>
        Reasons.Count > 0
            ? $"Recommended because you liked: {string.Join(", ", Reasons)}"
            : Strategy switch
            {
                RecommendationStrategy.TopRated => "One of the highest-rated books in the library",
                RecommendationStrategy.StatedPreferences => "Matches the genres and authors on your profile",
                _ => "Similar to books you have rated highly"
            };
}
