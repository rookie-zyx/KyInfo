namespace KyInfo.Domain.Entities;

public class ExamScore
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int TotalScore { get; set; }

    public int? PoliticsScore { get; set; }
    public int? EnglishScore { get; set; }
    public int? MathScore { get; set; }
    public int? MajorSubjectScore { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int? SchoolId { get; set; }
    public School? School { get; set; }

    public int? MajorId { get; set; }
    public Major? Major { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

