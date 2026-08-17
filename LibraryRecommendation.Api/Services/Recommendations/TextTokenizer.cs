using System.Text;

namespace LibraryRecommendation.Api.Services.Recommendations;

/// <summary>
/// Lowercases text, strips punctuation, splits on whitespace and drops stopwords.
/// Pure and deterministic so it can be unit tested without any infrastructure.
/// </summary>
public static class TextTokenizer
{
    private const int MinimumTokenLength = 2;

    public static IReadOnlyList<string> Tokenize(string? text, IReadOnlySet<string>? stopWords = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        stopWords ??= StopWords.Default;

        // Replace every non-alphanumeric character with a space, so "sci-fi," -> "sci fi ".
        var cleaned = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            cleaned.Append(char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : ' ');
        }

        var tokens = new List<string>();
        foreach (var token in cleaned.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length < MinimumTokenLength || stopWords.Contains(token))
            {
                continue;
            }

            tokens.Add(token);
        }

        return tokens;
    }
}
