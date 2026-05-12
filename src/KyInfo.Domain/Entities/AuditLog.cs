namespace KyInfo.Domain.Entities;

/// <summary>
/// 管理类操作审计记录（不含密码、完整 JWT）。
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int? ActorUserId { get; set; }

    public string ActorRole { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? ResourceType { get; set; }

    public int? ResourceId { get; set; }

    public string? Summary { get; set; }
}
