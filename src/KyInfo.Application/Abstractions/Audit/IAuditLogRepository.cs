using KyInfo.Contracts.Admin;

namespace KyInfo.Application.Abstractions.Audit;

public interface IAuditLogRepository
{
    Task AddAsync(
        int? actorUserId,
        string actorRole,
        string action,
        string? resourceType,
        int? resourceId,
        string? summary,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AuditLogListItemDto> Items, int TotalCount)> GetPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
