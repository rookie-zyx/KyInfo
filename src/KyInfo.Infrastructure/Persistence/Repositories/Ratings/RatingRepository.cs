using System.Data;
using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Ratings;
using KyInfo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence.Repositories.Ratings;

public sealed class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _db;

    public RatingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RatingSubmitOutcome> SubmitRatingAsync(
        string subjectType,
        int subjectId,
        int userId,
        byte score,
        string? comment,
        CancellationToken cancellationToken)
    {
        if (IsInMemoryProvider())
        {
            return await SubmitRatingCoreAsync(subjectType, subjectId, userId, score, comment, cancellationToken);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var outcome = await SubmitRatingCoreAsync(subjectType, subjectId, userId, score, comment, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return outcome;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<RatingSubmitOutcome> SubmitRatingCoreAsync(
        string subjectType,
        int subjectId,
        int userId,
        byte score,
        string? comment,
        CancellationToken cancellationToken)
    {
            var subject = await GetOrCreateSubjectLockedAsync(subjectType, subjectId, cancellationToken);
            var existing = await _db.UserRatings
                .FirstOrDefaultAsync(
                    r => r.RatingSubjectId == subject.Id && r.UserId == userId,
                    cancellationToken);

            var now = DateTime.UtcNow;
            var isUpdate = existing is not null;

            if (existing is null)
            {
                _db.UserRatings.Add(new UserRating
                {
                    RatingSubjectId = subject.Id,
                    UserId = userId,
                    Score = score,
                    Comment = comment,
                    CreatedAt = now
                });

                await AdjustDistributionAsync(subject.Id, score, 1, cancellationToken);

                var oldTotal = subject.TotalRatings;
                var newTotal = oldTotal + 1;
                subject.TotalRatings = newTotal;
                subject.AvgScore = newTotal == 0
                    ? 0
                    : Math.Round((subject.AvgScore * oldTotal + score) / newTotal, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                var oldScore = existing.Score;
                if (oldScore != score)
                {
                    await AdjustDistributionAsync(subject.Id, oldScore, -1, cancellationToken);
                    await AdjustDistributionAsync(subject.Id, score, 1, cancellationToken);
                }

                existing.Score = score;
                existing.Comment = comment;
                existing.UpdatedAt = now;

                var total = subject.TotalRatings;
                if (total > 0)
                {
                    subject.AvgScore = Math.Round(
                        (subject.AvgScore * total - oldScore + score) / total,
                        2,
                        MidpointRounding.AwayFromZero);
                }
            }

            subject.UpdatedAt = now;
            await _db.SaveChangesAsync(cancellationToken);

            var distribution = await LoadDistributionAsync(subject.Id, cancellationToken);

            return new RatingSubmitOutcome
            {
                SubjectType = subject.SubjectType,
                SubjectId = subject.SubjectId,
                IsUpdate = isUpdate,
                AvgScore = subject.AvgScore,
                TotalRatings = subject.TotalRatings,
                Distribution = distribution
            };
    }

    public async Task<RatingSnapshot> GetSnapshotAsync(
        string subjectType,
        int subjectId,
        int? userId,
        CancellationToken cancellationToken)
    {
        var subject = await _db.RatingSubjects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.SubjectType == subjectType && s.SubjectId == subjectId,
                cancellationToken);

        if (subject is null)
        {
            return new RatingSnapshot
            {
                SubjectType = subjectType,
                SubjectId = subjectId,
                AvgScore = 0,
                TotalRatings = 0,
                Distribution = new Dictionary<byte, int>()
            };
        }

        var distribution = await LoadDistributionAsync(subject.Id, cancellationToken);

        UserRating? myRating = null;
        if (userId.HasValue)
        {
            myRating = await _db.UserRatings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.RatingSubjectId == subject.Id && r.UserId == userId.Value,
                    cancellationToken);
        }

        return new RatingSnapshot
        {
            SubjectType = subject.SubjectType,
            SubjectId = subject.SubjectId,
            AvgScore = subject.AvgScore,
            TotalRatings = subject.TotalRatings,
            Distribution = distribution,
            MyScore = myRating?.Score,
            MyComment = myRating?.Comment,
            MyCreatedAt = myRating?.CreatedAt,
            MyUpdatedAt = myRating?.UpdatedAt
        };
    }

    private async Task<RatingSubject> GetOrCreateSubjectLockedAsync(
        string subjectType,
        int subjectId,
        CancellationToken cancellationToken)
    {
        if (IsInMemoryProvider())
        {
            var subject = await _db.RatingSubjects
                .FirstOrDefaultAsync(
                    s => s.SubjectType == subjectType && s.SubjectId == subjectId,
                    cancellationToken);

            if (subject is not null)
            {
                return subject;
            }

            subject = new RatingSubject
            {
                SubjectType = subjectType,
                SubjectId = subjectId,
                AvgScore = 0,
                TotalRatings = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _db.RatingSubjects.Add(subject);
            await _db.SaveChangesAsync(cancellationToken);
            return subject;
        }

        var locked = await _db.RatingSubjects
            .FromSqlInterpolated($"""
                SELECT Id, SubjectType, SubjectId, AvgScore, TotalRatings, UpdatedAt
                FROM RatingSubjects WITH (UPDLOCK, ROWLOCK)
                WHERE SubjectType = {subjectType} AND SubjectId = {subjectId}
                """)
            .AsTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (locked is not null)
        {
            return locked;
        }

        var created = new RatingSubject
        {
            SubjectType = subjectType,
            SubjectId = subjectId,
            AvgScore = 0,
            TotalRatings = 0,
            UpdatedAt = DateTime.UtcNow
        };
        _db.RatingSubjects.Add(created);
        await _db.SaveChangesAsync(cancellationToken);
        return created;
    }

    private async Task AdjustDistributionAsync(
        int ratingSubjectId,
        byte score,
        int delta,
        CancellationToken cancellationToken)
    {
        if (delta == 0)
        {
            return;
        }

        var row = await _db.RatingScoreDistributions
            .FirstOrDefaultAsync(
                d => d.RatingSubjectId == ratingSubjectId && d.Score == score,
                cancellationToken);

        if (row is null)
        {
            if (delta > 0)
            {
                _db.RatingScoreDistributions.Add(new RatingScoreDistribution
                {
                    RatingSubjectId = ratingSubjectId,
                    Score = score,
                    Count = delta
                });
            }

            return;
        }

        row.Count = Math.Max(0, row.Count + delta);
        if (row.Count == 0)
        {
            _db.RatingScoreDistributions.Remove(row);
        }
    }

    private async Task<IReadOnlyDictionary<byte, int>> LoadDistributionAsync(
        int ratingSubjectId,
        CancellationToken cancellationToken)
    {
        var rows = await _db.RatingScoreDistributions
            .AsNoTracking()
            .Where(d => d.RatingSubjectId == ratingSubjectId && d.Count > 0)
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(d => d.Score, d => d.Count);
    }

    private bool IsInMemoryProvider() =>
        _db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
}
