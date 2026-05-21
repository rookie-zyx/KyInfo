namespace KyInfo.Application.Ratings;

public sealed class RatingSubmitOutcome
{
    public string SubjectType { get; init; } = default!;

    public int SubjectId { get; init; }

    public bool IsUpdate { get; init; }

    public decimal AvgScore { get; init; }

    public int TotalRatings { get; init; }

    public IReadOnlyDictionary<byte, int> Distribution { get; init; } = new Dictionary<byte, int>();
}
