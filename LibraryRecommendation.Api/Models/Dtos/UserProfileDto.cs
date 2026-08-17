namespace LibraryRecommendation.Api.Models.Dtos;

public class RatedBookDto
{
    public required Book Book { get; init; }

    public int Stars { get; init; }

    public DateTime RatedDate { get; init; }
}

public class ReadingHistoryItemDto
{
    public required Book Book { get; init; }

    public ReadingStatus Status { get; init; }

    public string StatusName => Status.ToString();
}

public class UserProfileDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<string> FavouriteGenres { get; init; } = [];

    public IReadOnlyList<string> FavouriteAuthors { get; init; } = [];

    public IReadOnlyList<RatedBookDto> RatedBooks { get; init; } = [];

    public IReadOnlyList<ReadingHistoryItemDto> ReadingHistory { get; init; } = [];
}
