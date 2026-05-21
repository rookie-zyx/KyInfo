namespace KyInfo.Domain.Entities;

public class RatingSubject
{
    public int Id { get; set; }

    public string SubjectType { get; set; } = default!;

    public int SubjectId { get; set; }

    public decimal AvgScore { get; set; }

    public int TotalRatings { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<UserRating> UserRatings { get; set; } = new();

    public List<RatingScoreDistribution> ScoreDistributions { get; set; } = new();
}
