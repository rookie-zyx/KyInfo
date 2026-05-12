namespace KyInfo.Domain.Entities;

public class RecruitInfo
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int? PlanCount { get; set; }

    public string? ExamSubjects { get; set; }
    public string? ExtraRequirements { get; set; }

    public string? SourceUrl { get; set; }
    public DateTime? PublishedAt { get; set; }

    public int SchoolId { get; set; }
    public School School { get; set; } = default!;

    public int MajorId { get; set; }
    public Major Major { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

