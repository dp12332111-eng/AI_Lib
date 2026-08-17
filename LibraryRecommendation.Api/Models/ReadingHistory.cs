using System.Text.Json.Serialization;

namespace LibraryRecommendation.Api.Models;

public enum ReadingStatus
{
    Read = 0,
    Reading = 1,
    WantToRead = 2
}

public class ReadingHistory
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public ReadingStatus Status { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    [JsonIgnore]
    public Book? Book { get; set; }
}
