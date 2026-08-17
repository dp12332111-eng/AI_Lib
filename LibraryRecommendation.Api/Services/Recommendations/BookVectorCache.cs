using LibraryRecommendation.Api.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryRecommendation.Api.Services.Recommendations;

public class BookVectorCache : IBookVectorCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookVectorCache> _logger;
    private readonly SemaphoreSlim _rebuildLock = new(1, 1);

    private TfIdfModel? _model;
    private volatile bool _stale = true;

    public BookVectorCache(IServiceScopeFactory scopeFactory, ILogger<BookVectorCache> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<TfIdfModel> GetModelAsync(CancellationToken cancellationToken = default)
    {
        var cached = _model;
        if (cached is not null && !_stale)
        {
            return cached;
        }

        await _rebuildLock.WaitAsync(cancellationToken);
        try
        {
            // Another request may have rebuilt it while we waited for the lock.
            if (_model is not null && !_stale)
            {
                return _model;
            }

            // The cache is a singleton but the repository is scoped, so open a scope to read books.
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
            var books = await repository.GetAllAsync(cancellationToken);

            var model = TfIdfModel.Build(books);
            _model = model;
            _stale = false;

            _logger.LogInformation(
                "Rebuilt TF-IDF model: {DocumentCount} books, {TermCount} distinct terms.",
                model.DocumentCount,
                model.Idf.Count);

            return model;
        }
        finally
        {
            _rebuildLock.Release();
        }
    }

    public void Invalidate() => _stale = true;
}
