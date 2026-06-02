namespace KyInfo.Domain.Entities;

public class DiscussionComment
{
    public int Id { get; set; }

    public int DiscussionId { get; set; }
    public Discussion Discussion { get; set; } = default!;

    public int AuthorUserId { get; set; }
    public User Author { get; set; } = default!;

    public string Content { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
}
