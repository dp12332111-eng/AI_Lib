using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryRecommendation.Api.Models;

public class Rating
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; }

    public DateTime RatedDate { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public User? User { get; set; }

    [JsonIgnore]
    public Book? Book { get; set; }
}
