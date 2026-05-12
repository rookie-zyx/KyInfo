namespace KyInfo.Contracts.Recommendations;

public class RecommendationRequestDto
{
    public int UserId { get; set; }
    public int? Year { get; set; }
    public int Top { get; set; } = 30;
}

public class RecommendationItemDto
{
    public int Year { get; set; }
    public int UserTotalScore { get; set; }
    public int ScoreLine { get; set; }
    public int ScoreDiff => UserTotalScore - ScoreLine;

    public string Level { get; set; } = default!; // 冲刺 / 匹配 / 保底

    public int SchoolId { get; set; }
    public string SchoolName { get; set; } = default!;
    public string? SchoolShortName { get; set; }
    public string Province { get; set; } = default!;
    public string City { get; set; } = default!;
    public string LevelTag { get; set; } = default!;

    public int MajorId { get; set; }
    public string MajorName { get; set; } = default!;
    public string MajorCode { get; set; } = default!;
    public string DisciplineCategory { get; set; } = default!;
    public string DegreeType { get; set; } = default!;
    public string StudyType { get; set; } = default!;
    public int DurationYears { get; set; }
    public decimal? TuitionPerYear { get; set; }
}

public class RecommendationResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = default!;
    public int Year { get; set; }
    public int UserTotalScore { get; set; }
    public List<RecommendationItemDto> Items { get; set; } = new();
}

