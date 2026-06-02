using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Discussions;
using KyInfo.Contracts.Discussions;
using KyInfo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.Discussions;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly AppDbContext _db;

    public DiscussionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Discussion> Items, int TotalCount)> SearchAsync(
        int page,
        int pageSize,
        string? keyword,
        string sort,
        int? authorUserId,
        bool? isTeacherZone,
        CancellationToken cancellationToken)
    {
        var comments = _db.DiscussionComments.Where(c => !c.IsDeleted);
        var query = _db.Discussions
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Include(d => d.Author)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(d => d.Title.Contains(k) || d.Content.Contains(k));
        }

        if (authorUserId.HasValue)
        {
            query = query.Where(d => d.AuthorUserId == authorUserId.Value);
        }

        if (isTeacherZone.HasValue)
        {
            query = query.Where(d => d.IsTeacherZone == isTeacherZone.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        query = sort switch
        {
            DiscussionSortOptions.MostCommented => query
                .OrderByDescending(d => comments.Count(c => c.DiscussionId == d.Id))
                .ThenByDescending(d => d.CreatedAt),
            DiscussionSortOptions.RecentlyActive => query
                .OrderByDescending(d =>
                    comments.Where(c => c.DiscussionId == d.Id)
                        .Select(c => (DateTime?)c.CreatedAt)
                        .Max() ?? d.CreatedAt)
                .ThenByDescending(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        var items = await query.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Discussion?> GetByIdWithCommentsAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Discussions
            .AsNoTracking()
            .Where(d => d.Id == id && !d.IsDeleted)
            .Include(d => d.Author)
            .Include(d => d.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateDiscussionAsync(Discussion entity, CancellationToken cancellationToken)
    {
        _db.Discussions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<int> CreateCommentAsync(DiscussionComment entity, CancellationToken cancellationToken)
    {
        _db.DiscussionComments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public Task<Discussion?> GetDiscussionForDeleteAsync(int id, CancellationToken cancellationToken)
    {
        return _db.Discussions.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public Task<DiscussionComment?> GetCommentForDeleteAsync(int commentId, CancellationToken cancellationToken)
    {
        return _db.DiscussionComments
            .Include(c => c.Discussion)
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted, cancellationToken);
    }

    public async Task SoftDeleteDiscussionAsync(Discussion entity, CancellationToken cancellationToken)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteCommentAsync(DiscussionComment entity, CancellationToken cancellationToken)
    {
        entity.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetCommentCountsByDiscussionIdsAsync(
        IReadOnlyList<int> discussionIds,
        CancellationToken cancellationToken)
    {
        if (discussionIds.Count == 0)
        {
            return new Dictionary<int, int>();
        }

        return await _db.DiscussionComments
            .AsNoTracking()
            .Where(c => discussionIds.Contains(c.DiscussionId) && !c.IsDeleted)
            .GroupBy(c => c.DiscussionId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
    }

    public async Task<int> StripStickerTokensFromAllContentAsync(CancellationToken cancellationToken)
    {
        var updated = 0;

        var discussions = await _db.Discussions.ToListAsync(cancellationToken);
        foreach (var discussion in discussions)
        {
            var cleaned = DiscussionContentValidator.StripStickerTokens(discussion.Content);
            if (!string.Equals(discussion.Content, cleaned, StringComparison.Ordinal))
            {
                discussion.Content = cleaned;
                discussion.UpdatedAt = DateTime.UtcNow;
                updated++;
            }
        }

        var comments = await _db.DiscussionComments.ToListAsync(cancellationToken);
        foreach (var comment in comments)
        {
            var cleaned = DiscussionContentValidator.StripStickerTokens(comment.Content);
            if (!string.Equals(comment.Content, cleaned, StringComparison.Ordinal))
            {
                comment.Content = cleaned;
                updated++;
            }
        }

        if (updated > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return updated;
    }
}
