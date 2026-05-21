using KyInfo.Application.Ratings;

namespace KyInfo.Application.Abstractions.Repositories;

public interface IRatingRepository
{
    Task<RatingSubmitOutcome> SubmitRatingAsync(
        string subjectType,
        int subjectId,
        int userId,
        byte score,
        string? comment,
        CancellationToken cancellationToken);

    Task<RatingSnapshot> GetSnapshotAsync(
        string subjectType,
        int subjectId,
        int? userId,
        CancellationToken cancellationToken);
}
