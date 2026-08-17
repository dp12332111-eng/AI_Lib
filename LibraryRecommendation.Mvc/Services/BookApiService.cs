using System.Net;
using System.Net.Http.Json;
using LibraryRecommendation.Mvc.Models;

namespace LibraryRecommendation.Mvc.Services;

/// <summary>
/// Typed HttpClient client for the Books API. Base address is configured in Program.cs
/// from ApiSettings:BaseUrl in appsettings.json.
/// </summary>
public class BookApiService : IBookApiService
{
    private readonly HttpClient _httpClient;

    public BookApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<BookViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var books = await _httpClient.GetFromJsonAsync<IEnumerable<BookViewModel>>(
            "api/books", cancellationToken);
        return books ?? [];
    }

    public async Task<BookViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/books/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BookViewModel>(cancellationToken);
    }

    public async Task<IEnumerable<BookViewModel>> SearchAsync(
        string? term,
        string? genre = null,
        double? minRating = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(term))
        {
            query.Add($"term={Uri.EscapeDataString(term)}");
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query.Add($"genre={Uri.EscapeDataString(genre)}");
        }

        if (minRating.HasValue)
        {
            query.Add($"minRating={minRating.Value}");
        }

        var url = "api/books/search" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        var books = await _httpClient.GetFromJsonAsync<IEnumerable<BookViewModel>>(url, cancellationToken);
        return books ?? [];
    }

    public async Task<BookViewModel?> CreateAsync(BookViewModel book, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/books", book, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BookViewModel>(cancellationToken);
    }

    public async Task<bool> UpdateAsync(BookViewModel book, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/books/{book.Id}", book, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/books/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }
}
