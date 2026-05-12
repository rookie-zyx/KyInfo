namespace KyInfo.Contracts.ScoreLines;

public class ScoreLineListItemDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Score { get; set; }
    public bool IsNational { get; set; }

    public int? SchoolId { get; set; }
    public string? SchoolName { get; set; }

    public int? MajorId { get; set; }
    public string? MajorName { get; set; }
}

public class ScoreLineDetailDto : ScoreLineListItemDto
{
    public string? Note { get; set; }
}

public class ScoreLineTrendPointDto
{
    public int Year { get; set; }
    public int Score { get; set; }
}

public class ScoreLineCreateDto
{
    public int Year { get; set; }
    public int Score { get; set; }
    public bool IsNational { get; set; }

    public int? SchoolId { get; set; }
    public int? MajorId { get; set; }

    public string? Note { get; set; }
}

