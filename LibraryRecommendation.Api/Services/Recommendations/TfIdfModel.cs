using LibraryRecommendation.Api.Models;

namespace LibraryRecommendation.Api.Services.Recommendations;

/// <summary>
/// TF-IDF vector space over the book corpus, implemented from scratch (no ML libraries).
/// Vectors are sparse — a term only appears in the dictionary if the book actually uses it.
/// </summary>
public class TfIdfModel
{
    /// <summary>Contribution below this share of the strongest term is noise, not an explanation.</summary>
    private const double ExplanationCutoff = 0.15;

    private readonly Dictionary<string, string> _phraseLabels;

    private TfIdfModel(
        Dictionary<string, double> idf,
        Dictionary<int, Dictionary<string, double>> vectors,
        Dictionary<string, string> phraseLabels,
        DateTime builtAtUtc)
    {
        Idf = idf;
        Vectors = vectors;
        _phraseLabels = phraseLabels;
        BuiltAtUtc = builtAtUtc;
    }

    /// <summary>Inverse document frequency per term.</summary>
    public IReadOnlyDictionary<string, double> Idf { get; }

    /// <summary>L2-normalised TF-IDF vector per book id.</summary>
    public IReadOnlyDictionary<int, Dictionary<string, double>> Vectors { get; }

    public DateTime BuiltAtUtc { get; }

    public int DocumentCount => Vectors.Count;

    public static TfIdfModel Build(IEnumerable<Book> books, IReadOnlySet<string>? stopWords = null)
    {
        var documents = new Dictionary<int, IReadOnlyList<string>>();
        var documentFrequency = new Dictionary<string, int>();
        var phraseLabels = new Dictionary<string, string>();

        foreach (var book in books)
        {
            var tokens = TextTokenizer.Tokenize(BookCorpus.Build(book), stopWords);
            documents[book.Id] = tokens;

            foreach (var term in tokens.Distinct())
            {
                documentFrequency[term] = documentFrequency.GetValueOrDefault(term) + 1;
            }

            // Remember how to print phrase tokens back as words ("sciencefiction" -> "science fiction").
            RegisterPhraseLabel(phraseLabels, book.Genre);
            RegisterPhraseLabel(phraseLabels, book.Author);
        }

        var documentCount = documents.Count;
        var idf = new Dictionary<string, double>(documentFrequency.Count);
        foreach (var (term, frequency) in documentFrequency)
        {
            // Smoothed IDF: always positive, so a term shared by every book still counts a little.
            idf[term] = Math.Log((double)documentCount / (1 + frequency)) + 1.0;
        }

        var vectors = new Dictionary<int, Dictionary<string, double>>(documentCount);
        foreach (var (bookId, tokens) in documents)
        {
            vectors[bookId] = BuildVector(tokens, idf);
        }

        return new TfIdfModel(idf, vectors, phraseLabels, DateTime.UtcNow);
    }

    private static void RegisterPhraseLabel(Dictionary<string, string> labels, string? phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return;
        }

        var token = BookCorpus.PhraseToken(phrase);
        if (token.Length > 0)
        {
            labels[token] = phrase.Trim();
        }
    }

    /// <summary>
    /// The "recommended because" list for one book: the terms driving the similarity, printed as
    /// words rather than internal tokens.
    /// </summary>
    public IReadOnlyList<string> ExplainMatch(
        IReadOnlyDictionary<string, double> profile,
        IReadOnlyDictionary<string, double> candidate,
        int count)
    {
        return TopContributingTerms(profile, candidate, count)
            .Select(term => _phraseLabels.TryGetValue(term, out var label) ? label.ToLowerInvariant() : term)
            .Distinct()
            .ToList();
    }

    /// <summary>Turns free text into a vector in this model's term space (unknown terms are ignored).</summary>
    public Dictionary<string, double> Vectorize(string? text, IReadOnlySet<string>? stopWords = null)
    {
        var tokens = TextTokenizer.Tokenize(text, stopWords);
        return BuildVector(tokens, Idf);
    }

    private static Dictionary<string, double> BuildVector(
        IReadOnlyList<string> tokens,
        IReadOnlyDictionary<string, double> idf)
    {
        var vector = new Dictionary<string, double>();
        if (tokens.Count == 0)
        {
            return vector;
        }

        var counts = new Dictionary<string, int>();
        foreach (var token in tokens)
        {
            counts[token] = counts.GetValueOrDefault(token) + 1;
        }

        foreach (var (term, count) in counts)
        {
            // Term frequency normalised by document length, weighted by IDF.
            // Terms unknown to the model contribute nothing.
            if (idf.TryGetValue(term, out var termIdf))
            {
                vector[term] = (double)count / tokens.Count * termIdf;
            }
        }

        Normalize(vector);
        return vector;
    }

    /// <summary>Cosine similarity of two sparse vectors. 1 = identical direction, 0 = orthogonal.</summary>
    public static double CosineSimilarity(
        IReadOnlyDictionary<string, double> a,
        IReadOnlyDictionary<string, double> b)
    {
        if (a.Count == 0 || b.Count == 0)
        {
            return 0.0;
        }

        // Walk the smaller vector: only shared terms contribute to the dot product.
        var (smaller, larger) = a.Count <= b.Count ? (a, b) : (b, a);

        var dot = 0.0;
        foreach (var (term, weight) in smaller)
        {
            if (larger.TryGetValue(term, out var otherWeight))
            {
                dot += weight * otherWeight;
            }
        }

        if (dot == 0.0)
        {
            return 0.0;
        }

        var magnitudeA = Magnitude(a);
        var magnitudeB = Magnitude(b);
        if (magnitudeA == 0.0 || magnitudeB == 0.0)
        {
            return 0.0;
        }

        var cosine = dot / (magnitudeA * magnitudeB);

        // Guard against floating point drift pushing an identical pair just past 1.
        return Math.Clamp(cosine, -1.0, 1.0);
    }

    /// <summary>
    /// The terms that contributed most to the similarity between two vectors, highest first.
    /// This is what makes a recommendation explainable: each term's share of the dot product.
    /// </summary>
    public static IReadOnlyList<string> TopContributingTerms(
        IReadOnlyDictionary<string, double> profile,
        IReadOnlyDictionary<string, double> candidate,
        int count)
    {
        var contributions = new List<KeyValuePair<string, double>>();
        var (smaller, larger) = profile.Count <= candidate.Count ? (profile, candidate) : (candidate, profile);

        foreach (var (term, weight) in smaller)
        {
            if (larger.TryGetValue(term, out var otherWeight))
            {
                var contribution = weight * otherWeight;
                if (contribution > 0.0)
                {
                    contributions.Add(new KeyValuePair<string, double>(term, contribution));
                }
            }
        }

        if (contributions.Count == 0)
        {
            return [];
        }

        // Drop terms that barely moved the score — they read as noise in the UI.
        var threshold = contributions.Max(c => c.Value) * ExplanationCutoff;

        return contributions
            .Where(c => c.Value >= threshold)
            .OrderByDescending(c => c.Value)
            .ThenBy(c => c.Key, StringComparer.Ordinal)
            .Take(count)
            .Select(c => c.Key)
            .ToList();
    }

    /// <summary>Adds <paramref name="source"/> * <paramref name="weight"/> into <paramref name="target"/>.</summary>
    public static void AddScaled(
        Dictionary<string, double> target,
        IReadOnlyDictionary<string, double> source,
        double weight)
    {
        foreach (var (term, value) in source)
        {
            target[term] = target.GetValueOrDefault(term) + (value * weight);
        }
    }

    public static void Normalize(Dictionary<string, double> vector)
    {
        var magnitude = Magnitude(vector);
        if (magnitude == 0.0)
        {
            return;
        }

        foreach (var term in vector.Keys.ToList())
        {
            vector[term] /= magnitude;
        }
    }

    private static double Magnitude(IReadOnlyDictionary<string, double> vector)
    {
        var sum = 0.0;
        foreach (var value in vector.Values)
        {
            sum += value * value;
        }

        return Math.Sqrt(sum);
    }
}
