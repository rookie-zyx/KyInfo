namespace KyInfo.Domain.Entities;

public class School
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
    public string? ShortName { get; set; }

    public string Province { get; set; } = default!;
    public string City { get; set; } = default!;

    public string LevelTag { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Property { get; set; } = default!;

    public string? Website { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性：一个学校有多个专业（领域层仅保留 POCO 关系，不依赖 EF Core）
    public List<Major> Majors { get; set; } = new();
}

