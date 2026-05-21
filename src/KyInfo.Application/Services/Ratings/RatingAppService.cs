using KyInfo.Application.Abstractions.Caching;
using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Ratings;
using KyInfo.Contracts.Ratings;

namespace KyInfo.Application.Services.Ratings;

public sealed class RatingAppService : IRatingAppService
{
    private const int MaxSubjectTypeLength = 64;
    private const int MaxCommentLength = 500;

    private readonly IRatingRepository _repository;
    private readonly IRatingCache _cache;

    public RatingAppService(IRatingRepository repository, IRatingCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<RatingSubmitResultDto> SubmitAsync(
        int userId,
        RatingSubmitDto dto,
        CancellationToken cancellationToken)
    {
        var subjectType = NormalizeSubjectType(dto.SubjectType);
        ValidateScore(dto.Score);
        ValidateComment(dto.Comment);

        if (dto.SubjectId <= 0)
        {
            throw new ArgumentException("SubjectId 必须为正整数。");
        }

        var outcome = await _repository.SubmitRatingAsync(
            subjectType,
            dto.SubjectId,
            userId,
            dto.Score,
            dto.Comment,
            cancellationToken);

        var snapshot = await _repository.GetSnapshotAsync(
            subjectType,
            dto.SubjectId,
            userId,
            cancellationToken);

        await _cache.SetSnapshotAsync(snapshot, userId, cancellationToken);

        return new RatingSubmitResultDto
        {
            SubjectType = outcome.SubjectType,
            SubjectId = outcome.SubjectId,
            IsUpdate = outcome.IsUpdate,
            AvgScore = outcome.AvgScore,
            TotalRatings = outcome.TotalRatings,
            Distribution = outcome.Distribution
        };
    }

    public async Task<RatingDetailDto> GetDetailAsync(
        string subjectType,
        int subjectId,
        int? userId,
        CancellationToken cancellationToken)
    {
        subjectType = NormalizeSubjectType(subjectType);

        if (subjectId <= 0)
        {
            throw new ArgumentException("SubjectId 必须为正整数。");
        }

        var cached = await _cache.GetSnapshotAsync(subjectType, subjectId, userId, cancellationToken);
        if (cached is not null)
        {
            return MapToDetail(cached);
        }

        var snapshot = await _repository.GetSnapshotAsync(subjectType, subjectId, userId, cancellationToken);
        await _cache.SetSnapshotAsync(snapshot, userId, cancellationToken);

        return MapToDetail(snapshot);
    }

    private static RatingDetailDto MapToDetail(RatingSnapshot snapshot)
    {
        MyRatingDto? mine = null;
        if (snapshot.MyScore.HasValue)
        {
            mine = new MyRatingDto
            {
                Score = snapshot.MyScore.Value,
                Comment = snapshot.MyComment,
                CreatedAt = snapshot.MyCreatedAt ?? DateTime.UtcNow,
                UpdatedAt = snapshot.MyUpdatedAt
            };
        }

        return new RatingDetailDto
        {
            SubjectType = snapshot.SubjectType,
            SubjectId = snapshot.SubjectId,
            AvgScore = snapshot.AvgScore,
            TotalRatings = snapshot.TotalRatings,
            Distribution = snapshot.Distribution,
            MyRating = mine
        };
    }

    private static string NormalizeSubjectType(string subjectType)
    {
        if (string.IsNullOrWhiteSpace(subjectType))
        {
            throw new ArgumentException("SubjectType 不能为空。");
        }

        var normalized = subjectType.Trim().ToLowerInvariant();
        if (normalized.Length > MaxSubjectTypeLength)
        {
            throw new ArgumentException($"SubjectType 长度不能超过 {MaxSubjectTypeLength}。");
        }

        return normalized;
    }

    private static void ValidateScore(byte score)
    {
        if (score is < 1 or > 5)
        {
            throw new ArgumentException("Score 必须在 1 到 5 之间。");
        }
    }

    private static void ValidateComment(string? comment)
    {
        if (comment is not null && comment.Length > MaxCommentLength)
        {
            throw new ArgumentException($"Comment 长度不能超过 {MaxCommentLength}。");
        }
    }
}
