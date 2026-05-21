namespace KyInfo.Application.Ratings;

public sealed class RatingSnapshot
{
    public string SubjectType { get; init; } = default!;

    public int SubjectId { get; init; }

    public decimal AvgScore { get; init; }

    public int TotalRatings { get; init; }

    public IReadOnlyDictionary<byte, int> Distribution { get; init; } = new Dictionary<byte, int>();

    public byte? MyScore { get; init; }

    public string? MyComment { get; init; }

    public DateTime? MyCreatedAt { get; init; }

    public DateTime? MyUpdatedAt { get; init; }
}
