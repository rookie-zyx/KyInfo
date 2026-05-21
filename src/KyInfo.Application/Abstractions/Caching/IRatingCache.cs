using KyInfo.Application.Ratings;

namespace KyInfo.Application.Abstractions.Caching;

public interface IRatingCache
{
    Task<RatingSnapshot?> GetSnapshotAsync(
        string subjectType,
        int subjectId,
        int? userId,
        CancellationToken cancellationToken);

    Task SetSnapshotAsync(
        RatingSnapshot snapshot,
        int? userIdForMineKey,
        CancellationToken cancellationToken);

    Task InvalidateSnapshotAsync(
        string subjectType,
        int subjectId,
        int? userId,
        CancellationToken cancellationToken);
}
