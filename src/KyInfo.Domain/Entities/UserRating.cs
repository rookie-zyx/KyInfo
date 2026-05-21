namespace KyInfo.Domain.Entities;

public class UserRating
{
    public int Id { get; set; }

    public int RatingSubjectId { get; set; }
    public RatingSubject RatingSubject { get; set; } = default!;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public byte Score { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
