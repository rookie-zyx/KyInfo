using KyInfo.Domain.Entities;

namespace KyInfo.Application.Abstractions.Repositories;

public interface IDiscussionRepository
{
    Task<(List<Discussion> Items, int TotalCount)> SearchAsync(
        int page,
        int pageSize,
        string? keyword,
        string sort,
        int? authorUserId,
        bool? isTeacherZone,
        CancellationToken cancellationToken);

    Task<Discussion?> GetByIdWithCommentsAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateDiscussionAsync(Discussion entity, CancellationToken cancellationToken);

    Task<int> CreateCommentAsync(DiscussionComment entity, CancellationToken cancellationToken);

    Task<Discussion?> GetDiscussionForDeleteAsync(int id, CancellationToken cancellationToken);

    Task<DiscussionComment?> GetCommentForDeleteAsync(int commentId, CancellationToken cancellationToken);

    Task SoftDeleteDiscussionAsync(Discussion entity, CancellationToken cancellationToken);

    Task SoftDeleteCommentAsync(DiscussionComment entity, CancellationToken cancellationToken);

    Task<Dictionary<int, int>> GetCommentCountsByDiscussionIdsAsync(
        IReadOnlyList<int> discussionIds,
        CancellationToken cancellationToken);

    Task<int> StripStickerTokensFromAllContentAsync(CancellationToken cancellationToken);
}
