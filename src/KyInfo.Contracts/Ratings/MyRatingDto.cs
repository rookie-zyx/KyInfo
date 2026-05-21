namespace KyInfo.Contracts.Ratings;

public sealed class MyRatingDto
{
    public byte Score { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
