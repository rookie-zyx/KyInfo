namespace KyInfo.Contracts.Schools;

/// <summary>
/// 学校列表项 DTO，用于在列表/搜索结果中返回学校的简要信息。
/// </summary>
public class SchoolListItemDto
{
    /// <summary>学校主键 Id。</summary>
    public int Id { get; set; }

    /// <summary>学校全称。</summary>
    public string Name { get; set; } = default!;

    /// <summary>学校简称（可空）。</summary>
    public string? ShortName { get; set; }

    /// <summary>所在省份。</summary>
    public string Province { get; set; } = default!;

    /// <summary>所在城市。</summary>
    public string City { get; set; } = default!;

    /// <summary>层次标签（例如“双一流”“985/211”等）。</summary>
    public string LevelTag { get; set; } = default!;

    /// <summary>学校类型（例如“公办/民办”或院校类别）。</summary>
    public string Type { get; set; } = default!;

    /// <summary>学校属性（例如“理工/文史/综合”等描述）。</summary>
    public string Property { get; set; } = default!;
}

/// <summary>
/// 学校详情 DTO，扩展自 <see cref="SchoolListItemDto"/>，包含更多展示/详情字段。
/// </summary>
public class SchoolDetailDto : SchoolListItemDto
{
    /// <summary>学校官网（可空）。</summary>
    public string? Website { get; set; }

    // 备注：将来可在此处添加该校下的专业列表（List<MajorDto>）或其它详情字段。
}

