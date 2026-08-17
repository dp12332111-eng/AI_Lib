namespace LibraryRecommendation.Api.Services.Recommendations;

/// <summary>
/// Common English words that carry no topical signal. Removing them keeps the TF-IDF
/// vectors focused on words that actually distinguish one book from another.
/// </summary>
public static class StopWords
{
    public static readonly IReadOnlySet<string> Default = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "as", "at",
        "be", "because", "been", "before", "being", "below", "between", "both", "but", "by",
        "can", "cannot", "could",
        "did", "do", "does", "doing", "down", "during",
        "each", "even", "ever", "every",
        "few", "for", "from", "further",
        "had", "has", "have", "having", "he", "her", "here", "hers", "herself", "him", "himself", "his", "how",
        "i", "if", "in", "into", "is", "it", "its", "itself",
        "just",
        "me", "more", "most", "must", "my", "myself",
        "no", "nor", "not", "now",
        "of", "off", "on", "once", "one", "only", "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own",
        "same", "she", "should", "so", "some", "still", "such",
        "than", "that", "the", "their", "theirs", "them", "themselves", "then", "there", "these", "they", "this",
        "those", "through", "to", "too",
        "under", "until", "up", "upon",
        "very",
        "was", "we", "were", "what", "when", "where", "which", "while", "who", "whom", "why", "will", "with", "would",
        "you", "your", "yours", "yourself", "yourselves",
        // Counting words carry no topic signal but match freely across unrelated blurbs.
        "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
        "first", "second", "third", "another", "many", "much",
        // Words that show up in almost every blurb and therefore separate nothing.
        "book", "books", "novel", "story", "stories", "tale", "tales", "read", "reader", "readers", "author",
        "written", "writes", "page", "pages", "chapter", "series"
    };
}
