namespace KyInfo.Domain.Entities;

public class ScoreLine
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int Score { get; set; }

    public bool IsNational { get; set; }

    public int? SchoolId { get; set; }
    public School? School { get; set; }

    public int? MajorId { get; set; }
    public Major? Major { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

