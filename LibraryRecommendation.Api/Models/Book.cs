using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryRecommendation.Api.Models;

// PROVISIONAL: property set is a placeholder until the assignment brief is released.
// Expect the brief to rename/add/remove properties — change here, then re-run
// `dotnet ef migrations add <Name>` and `dotnet ef database update`.
public class Book
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Genre { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0, 5)]
    public double Rating { get; set; }

    [Range(1000, 2100)]
    public int PublishedYear { get; set; }

    [JsonIgnore]
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [JsonIgnore]
    public ICollection<ReadingHistory> ReadingHistory { get; set; } = new List<ReadingHistory>();
}
