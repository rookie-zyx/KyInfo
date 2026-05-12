namespace KyInfo.Contracts.RecruitInfos;

public class RecruitInfoListItemDto
{
    public int Id { get; set; }
    public int Year { get; set; }

    public int SchoolId { get; set; }
    public string SchoolName { get; set; } = default!;

    public int MajorId { get; set; }
    public string MajorName { get; set; } = default!;

    public int? PlanCount { get; set; }
}

public class RecruitInfoDetailDto : RecruitInfoListItemDto
{
    public string? ExamSubjects { get; set; }
    public string? ExtraRequirements { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class RecruitInfoCreateDto
{
    public int Year { get; set; }
    public int SchoolId { get; set; }
    public int MajorId { get; set; }

    public int? PlanCount { get; set; }
    public string? ExamSubjects { get; set; }
    public string? ExtraRequirements { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
}

