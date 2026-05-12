namespace KyInfo.Domain.Entities;

public class Major
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;

    public string DisciplineCategory { get; set; } = default!;
    public string DegreeType { get; set; } = default!;
    public string StudyType { get; set; } = "全日制";
    public int DurationYears { get; set; } = 3;

    public decimal? TuitionPerYear { get; set; }
    public string? SchoolDepartment { get; set; }
    public string? Description { get; set; }

    public int SchoolId { get; set; }
    public School School { get; set; } = default!;
}

