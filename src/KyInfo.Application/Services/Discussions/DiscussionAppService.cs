using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Application.Discussions;
using KyInfo.Contracts.Discussions;
using KyInfo.Domain.Entities;

namespace KyInfo.Application.Services.Discussions;

public class DiscussionAppService : IDiscussionAppService
{
    private readonly IDiscussionRepository _repository;

    public DiscussionAppService(IDiscussionRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<StickerDto> GetStickers() => StickerCatalog.GetAll();

    public async Task<DiscussionListResponseDto> SearchAsync(
        int page,
        int pageSize,
        string? keyword,
        string? sort,
        int? authorUserId,
        bool? isTeacherZone,
        CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 20;
        }

        if (pageSize > 50)
        {
            pageSize = 50;
        }

        var normalizedSort = NormalizeSort(sort);
        var normalizedAuthorUserId = authorUserId.HasValue && authorUserId.Value > 0 ? authorUserId : null;
        var (items, total) = await _repository.SearchAsync(page, pageSize, keyword, normalizedSort, normalizedAuthorUserId, isTeacherZone, cancellationToken);
        var ids = items.Select(d => d.Id).ToList();
        var commentCounts = await _repository.GetCommentCountsByDiscussionIdsAsync(ids, cancellationToken);

        return new DiscussionListResponseDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items.Select(d => new DiscussionListItemDto
            {
                Id = d.Id,
                Title = d.Title,
                AuthorUserId = d.AuthorUserId,
                AuthorUserName = d.Author.UserName,
                CommentCount = commentCounts.GetValueOrDefault(d.Id),
                ContentPreview = DiscussionContentValidator.BuildPreview(d.Content),
                CreatedAt = d.CreatedAt
            }).ToList()
        };
    }

    private static string NormalizeSort(string? sort)
    {
        if (string.Equals(sort, DiscussionSortOptions.MostCommented, StringComparison.OrdinalIgnoreCase))
        {
            return DiscussionSortOptions.MostCommented;
        }

        if (string.Equals(sort, DiscussionSortOptions.RecentlyActive, StringComparison.OrdinalIgnoreCase))
        {
            return DiscussionSortOptions.RecentlyActive;
        }

        return DiscussionSortOptions.Latest;
    }

    public async Task<DiscussionDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var d = await _repository.GetByIdWithCommentsAsync(id, cancellationToken);
        if (d is null)
        {
            throw new NotFoundException("讨论帖不存在或已删除");
        }

        return new DiscussionDetailDto
        {
            Id = d.Id,
            Title = d.Title,
            Content = d.Content,
            AuthorUserId = d.AuthorUserId,
            AuthorUserName = d.Author.UserName,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
            Comments = d.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new DiscussionCommentDto
                {
                    Id = c.Id,
                    AuthorUserId = c.AuthorUserId,
                    AuthorUserName = c.Author.UserName,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<int> CreateDiscussionAsync(
        DiscussionCreateDto dto,
        int authorUserId,
        CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (authorUserId <= 0)
        {
            throw new ArgumentException("无效的用户 Id");
        }

        var title = DiscussionContentValidator.NormalizeTitle(dto.Title);
        var content = DiscussionContentValidator.NormalizeAndValidate(
            dto.Content,
            DiscussionContentLimits.MaxDiscussionContentLength);

        var entity = new Discussion
        {
            Title = title,
            Content = content,
            AuthorUserId = authorUserId,
            CreatedAt = DateTime.UtcNow,
            IsTeacherZone = dto.IsTeacherZone
        };

        return await _repository.CreateDiscussionAsync(entity, cancellationToken);
    }

    public async Task<int> CreateCommentAsync(
        int discussionId,
        DiscussionCommentCreateDto dto,
        int authorUserId,
        CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (authorUserId <= 0)
        {
            throw new ArgumentException("无效的用户 Id");
        }

        var discussion = await _repository.GetByIdWithCommentsAsync(discussionId, cancellationToken);
        if (discussion is null)
        {
            throw new NotFoundException("讨论帖不存在或已删除");
        }

        var content = DiscussionContentValidator.NormalizeAndValidate(
            dto.Content,
            DiscussionContentLimits.MaxCommentContentLength);

        var entity = new DiscussionComment
        {
            DiscussionId = discussionId,
            AuthorUserId = authorUserId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CreateCommentAsync(entity, cancellationToken);
    }

    public async Task DeleteDiscussionAsync(
        int id,
        int actorUserId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        var d = await _repository.GetDiscussionForDeleteAsync(id, cancellationToken);
        if (d is null)
        {
            throw new NotFoundException("讨论帖不存在或已删除");
        }

        if (!CanDelete(d.AuthorUserId, actorUserId, actorRole))
        {
            throw new ForbiddenException("无权删除该讨论帖");
        }

        await _repository.SoftDeleteDiscussionAsync(d, cancellationToken);
    }

    public async Task DeleteCommentAsync(
        int commentId,
        int actorUserId,
        string actorRole,
        CancellationToken cancellationToken)
    {
        var c = await _repository.GetCommentForDeleteAsync(commentId, cancellationToken);
        if (c is null)
        {
            throw new NotFoundException("评论不存在或已删除");
        }

        if (!CanDelete(c.AuthorUserId, actorUserId, actorRole))
        {
            throw new ForbiddenException("无权删除该评论");
        }

        await _repository.SoftDeleteCommentAsync(c, cancellationToken);
    }

    private static bool CanDelete(int authorUserId, int actorUserId, string actorRole)
    {
        if (authorUserId == actorUserId)
        {
            return true;
        }

        return string.Equals(actorRole, "Admin", StringComparison.OrdinalIgnoreCase)
               || string.Equals(actorRole, "Root", StringComparison.OrdinalIgnoreCase);
    }
}
