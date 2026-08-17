using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryRecommendation.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Comma-separated list, e.g. "Science Fiction, Fantasy".</summary>
    [MaxLength(500)]
    public string FavouriteGenres { get; set; } = string.Empty;

    /// <summary>Comma-separated list, e.g. "Frank Herbert, Ursula K. Le Guin".</summary>
    [MaxLength(500)]
    public string FavouriteAuthors { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [JsonIgnore]
    public ICollection<ReadingHistory> ReadingHistory { get; set; } = new List<ReadingHistory>();

    public IEnumerable<string> FavouriteGenreList => SplitList(FavouriteGenres);

    public IEnumerable<string> FavouriteAuthorList => SplitList(FavouriteAuthors);

    private static IEnumerable<string> SplitList(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
