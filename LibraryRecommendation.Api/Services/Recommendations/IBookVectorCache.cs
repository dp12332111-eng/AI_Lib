namespace LibraryRecommendation.Api.Services.Recommendations;

/// <summary>
/// Holds the TF-IDF model for the whole library. Registered as a singleton so the vectors are
/// computed once, not per request; <see cref="Invalidate"/> marks them stale after books change.
/// </summary>
public interface IBookVectorCache
{
    Task<TfIdfModel> GetModelAsync(CancellationToken cancellationToken = default);

    /// <summary>Call after any book insert/update/delete so the next request rebuilds the model.</summary>
    void Invalidate();
}
