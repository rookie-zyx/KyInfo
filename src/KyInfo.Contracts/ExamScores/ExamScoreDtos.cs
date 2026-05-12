namespace KyInfo.Contracts.ExamScores;

public class ExamScoreListItemDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int TotalScore { get; set; }

    public int UserId { get; set; }
    public string UserName { get; set; } = default!;

    public int? SchoolId { get; set; }
    public string? SchoolName { get; set; }

    public int? MajorId { get; set; }
    public string? MajorName { get; set; }
}

public class ExamScoreDetailDto : ExamScoreListItemDto
{
    public int? PoliticsScore { get; set; }
    public int? EnglishScore { get; set; }
    public int? MathScore { get; set; }
    public int? MajorSubjectScore { get; set; }
}

public class ExamScoreCreateDto
{
    public int Year { get; set; }
    public int TotalScore { get; set; }

    public int? PoliticsScore { get; set; }
    public int? EnglishScore { get; set; }
    public int? MathScore { get; set; }
    public int? MajorSubjectScore { get; set; }

    // 当前简化处理：直接传 UserId
    // 后续可以改为从 JWT 中读取当前用户 Id
    public int UserId { get; set; }

    public int? SchoolId { get; set; }
    public int? MajorId { get; set; }
}

