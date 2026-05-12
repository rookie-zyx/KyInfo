namespace KyInfo.Contracts.Majors;

public class MajorListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string DisciplineCategory { get; set; } = default!;
    public string DegreeType { get; set; } = default!;
    public string StudyType { get; set; } = default!;
    public int DurationYears { get; set; }

    public int SchoolId { get; set; }
    public string SchoolName { get; set; } = default!;
}

public class MajorDetailDto : MajorListItemDto
{
    public decimal? TuitionPerYear { get; set; }
    public string? SchoolDepartment { get; set; }
    public string? Description { get; set; }
}

