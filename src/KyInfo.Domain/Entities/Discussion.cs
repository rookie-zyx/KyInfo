namespace KyInfo.Domain.Entities;

public class Discussion
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string Content { get; set; } = default!;

    public int AuthorUserId { get; set; }
    public User Author { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsTeacherZone { get; set; }

    public List<DiscussionComment> Comments { get; set; } = new();
}
