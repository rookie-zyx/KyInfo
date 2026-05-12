namespace KyInfo.Contracts.Admin;

public class AuditLogListItemDto
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int? ActorUserId { get; set; }

    public string ActorRole { get; set; } = default!;

    public string Action { get; set; } = default!;

    public string? ResourceType { get; set; }

    public int? ResourceId { get; set; }

    public string? Summary { get; set; }
}

public class AuditLogListResponseDto
{
    public IReadOnlyList<AuditLogListItemDto> Items { get; set; } = Array.Empty<AuditLogListItemDto>();

    public int TotalCount { get; set; }
}
