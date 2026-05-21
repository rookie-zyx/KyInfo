namespace KyInfo.Domain.Entities;

public class RatingScoreDistribution
{
    public int RatingSubjectId { get; set; }
    public RatingSubject RatingSubject { get; set; } = default!;

    public byte Score { get; set; }

    public int Count { get; set; }
}
