namespace KyInfo.Contracts.Discussions;

public static class DiscussionSortOptions
{
    public const string Latest = "latest";
    public const string MostCommented = "mostCommented";
    public const string RecentlyActive = "recentlyActive";
}

public static class DiscussionZoneOptions
{
    public const string Student = "student";
    public const string Teacher = "teacher";
    public const string All = "all";
}

public class DiscussionListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string AuthorUserName { get; set; } = default!;
    public int AuthorUserId { get; set; }
    public int CommentCount { get; set; }
    public string ContentPreview { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class DiscussionCommentDto
{
    public int Id { get; set; }
    public int AuthorUserId { get; set; }
    public string AuthorUserName { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class DiscussionDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public int AuthorUserId { get; set; }
    public string AuthorUserName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DiscussionCommentDto> Comments { get; set; } = new();
}

public class DiscussionCreateDto
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public bool IsTeacherZone { get; set; }
}

public class DiscussionCommentCreateDto
{
    public string Content { get; set; } = default!;
}

public class DiscussionListResponseDto
{
    public IReadOnlyList<DiscussionListItemDto> Items { get; set; } = Array.Empty<DiscussionListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class StickerDto
{
    public string Code { get; set; } = default!;
    public string Label { get; set; } = default!;
    public string Url { get; set; } = default!;
}
