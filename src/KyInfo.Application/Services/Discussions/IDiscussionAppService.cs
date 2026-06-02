using KyInfo.Contracts.Discussions;

namespace KyInfo.Application.Services.Discussions;

public interface IDiscussionAppService
{
    Task<DiscussionListResponseDto> SearchAsync(
        int page,
        int pageSize,
        string? keyword,
        string? sort,
        int? authorUserId,
        bool? isTeacherZone,
        CancellationToken cancellationToken);

    Task<DiscussionDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<int> CreateDiscussionAsync(DiscussionCreateDto dto, int authorUserId, CancellationToken cancellationToken);

    Task<int> CreateCommentAsync(
        int discussionId,
        DiscussionCommentCreateDto dto,
        int authorUserId,
        CancellationToken cancellationToken);

    Task DeleteDiscussionAsync(int id, int actorUserId, string actorRole, CancellationToken cancellationToken);

    Task DeleteCommentAsync(int commentId, int actorUserId, string actorRole, CancellationToken cancellationToken);

    IReadOnlyList<StickerDto> GetStickers();
}
