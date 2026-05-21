namespace KyInfo.Contracts.Ratings;

public sealed class RatingSubmitDto
{
    public string SubjectType { get; set; } = default!;

    public int SubjectId { get; set; }

    public byte Score { get; set; }

    public string? Comment { get; set; }
}
