namespace KyInfo.Infrastructure.Options;

public sealed class RatingOptions
{
    public const string SectionName = "Rating";

    public bool UseRedis { get; set; } = true;
}
