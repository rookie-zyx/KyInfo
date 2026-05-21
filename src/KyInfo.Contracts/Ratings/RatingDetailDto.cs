namespace KyInfo.Contracts.Ratings;

public sealed class RatingDetailDto
{
    public string SubjectType { get; set; } = default!;

    public int SubjectId { get; set; }

    public decimal AvgScore { get; set; }

    public int TotalRatings { get; set; }

    public IReadOnlyDictionary<byte, int> Distribution { get; set; } = new Dictionary<byte, int>();

    public MyRatingDto? MyRating { get; set; }
}
