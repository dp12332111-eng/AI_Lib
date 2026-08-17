using System.Text;
using LibraryRecommendation.Api.Models;

namespace LibraryRecommendation.Api.Services.Recommendations;

/// <summary>
/// Builds the text a book is vectorised from: Title + Author + Genre + Description.
/// </summary>
/// <remarks>
/// Genre and Author are additionally emitted as single "phrase tokens" ("Science Fiction" ->
/// "sciencefiction"). Without them the tokenizer splits multi-word values into ordinary words, and
/// those words collide across unrelated books: "Science Fiction" and "Popular Science" share
/// "science", and Stephen King matches Stephen Hawking on "stephen". A phrase token only matches
/// the exact same genre or author, so it carries the signal those loose words cannot.
/// </remarks>
public static class BookCorpus
{
    /// <summary>How many times a phrase token is repeated, i.e. its weight relative to a plain word.</summary>
    public const int PhraseTokenWeight = 2;

    public static string Build(Book book)
    {
        var builder = new StringBuilder();
        builder.Append(book.Title).Append(' ');
        builder.Append(book.Author).Append(' ');
        builder.Append(book.Genre).Append(' ');
        builder.Append(book.Description).Append(' ');
        builder.Append(WeightedPhrase(book.Genre)).Append(' ');
        builder.Append(WeightedPhrase(book.Author));
        return builder.ToString();
    }

    /// <summary>"Science Fiction" -> "sciencefiction". Returns empty for null/blank input.</summary>
    public static string PhraseToken(string? phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return string.Empty;
        }

        var token = new StringBuilder(phrase.Length);
        foreach (var ch in phrase)
        {
            if (char.IsLetterOrDigit(ch))
            {
                token.Append(char.ToLowerInvariant(ch));
            }
        }

        return token.ToString();
    }

    /// <summary>The phrase token repeated <see cref="PhraseTokenWeight"/> times, space separated.</summary>
    public static string WeightedPhrase(string? phrase)
    {
        var token = PhraseToken(phrase);
        if (token.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(' ', Enumerable.Repeat(token, PhraseTokenWeight));
    }

    /// <summary>Free text plus its own phrase token — used to vectorise a stated favourite genre/author.</summary>
    public static string BuildPreferenceText(string phrase) => $"{phrase} {WeightedPhrase(phrase)}";
}
