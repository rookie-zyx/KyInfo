using KyInfo.Application.Abstractions.Audit;
using KyInfo.Contracts.Admin;
using KyInfo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.Audit;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(
        int? actorUserId,
        string actorRole,
        string action,
        string? resourceType,
        int? resourceId,
        string? summary,
        CancellationToken cancellationToken = default)
    {
        var row = new AuditLog
        {
            CreatedAtUtc = DateTime.UtcNow,
            ActorUserId = actorUserId,
            ActorRole = actorRole.Trim(),
            Action = action.Trim(),
            ResourceType = string.IsNullOrWhiteSpace(resourceType) ? null : resourceType.Trim(),
            ResourceId = resourceId,
            Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim()
        };

        _db.AuditLogs.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<AuditLogListItemDto> Items, int TotalCount)> GetPagedAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var q = _db.AuditLogs.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc);
        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .Skip(skip)
            .Take(take)
            .Select(x => new AuditLogListItemDto
            {
                Id = x.Id,
                CreatedAtUtc = x.CreatedAtUtc,
                ActorUserId = x.ActorUserId,
                ActorRole = x.ActorRole,
                Action = x.Action,
                ResourceType = x.ResourceType,
                ResourceId = x.ResourceId,
                Summary = x.Summary
            })
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
