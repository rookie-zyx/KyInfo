using System.Text.Json;
using KyInfo.Application.Abstractions.Caching;
using KyInfo.Application.Ratings;
using KyInfo.Infrastructure.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace KyInfo.Infrastructure.Caching;

public sealed class RatingCacheService : IRatingCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DistributedCacheEntryOptions CacheTtl = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };

    private readonly IDistributedCache _cache;
    private readonly RatingOptions _options;

    public RatingCacheService(IDistributedCache cache, IOptions<RatingOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public async Task<RatingSnapshot?> GetSnapshotAsync(
        string subjectType,
        int subjectId,
        int? userId,
        CancellationToken cancellationToken)
    {
        if (!_options.UseRedis)
        {
            return null;
        }

        var bytes = await _cache.GetAsync(BuildSummaryKey(subjectType, subjectId), cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        var cached = JsonSerializer.Deserialize<CachedRatingSnapshot>(bytes, JsonOptions);
        if (cached is null)
        {
            return null;
        }

        MyRatingCacheEntry? mine = null;
        if (userId.HasValue)
        {
            var mineBytes = await _cache.GetAsync(
                BuildMineKey(subjectType, subjectId, userId.Value),
                cancellationToken);
            if (mineBytes is not null && mineBytes.Length > 0)
            {
                mine = JsonSerializer.Deserialize<MyRatingCacheEntry>(mineBytes, JsonOptions);
            }
        }

        return new RatingSnapshot
        {
            SubjectType = cached.SubjectType,
            SubjectId = cached.SubjectId,
            AvgScore = cached.AvgScore,
            TotalRatings = cached.TotalRatings,
            Distribution = cached.Distribution,
            MyScore = mine?.Score,
            MyComment = mine?.Comment,
            MyCreatedAt = mine?.CreatedAt,
            MyUpdatedAt = mine?.UpdatedAt
        };
    }

    public async Task SetSnapshotAsync(
        RatingSnapshot snapshot,
        int? userIdForMineKey,
        CancellationToken cancellationToken)
    {
        if (!_options.UseRedis)
        {
            return;
        }

        var summary = new CachedRatingSnapshot
        {
            SubjectType = snapshot.SubjectType,
            SubjectId = snapshot.SubjectId,
            AvgScore = snapshot.AvgScore,
            TotalRatings = snapshot.TotalRatings,
            Distribution = snapshot.Distribution.ToDictionary(x => x.Key, x => x.Value)
        };

        await _cache.SetStringAsync(
            BuildSummaryKey(snapshot.SubjectType, snapshot.SubjectId),
            JsonSerializer.Serialize(summary, JsonOptions),
            CacheTtl,
            cancellationToken);

        if (!userIdForMineKey.HasValue)
        {
            return;
        }

        if (snapshot.MyScore.HasValue)
        {
            var mine = new MyRatingCacheEntry
            {
                Score = snapshot.MyScore.Value,
                Comment = snapshot.MyComment,
                CreatedAt = snapshot.MyCreatedAt ?? DateTime.UtcNow,
                UpdatedAt = snapshot.MyUpdatedAt
            };

            await _cache.SetStringAsync(
                BuildMineKey(snapshot.SubjectType, snapshot.SubjectId, userIdForMineKey.Value),
                JsonSerializer.Serialize(mine, JsonOptions),
                CacheTtl,
                cancellationToken);
        }
        else
        {
            await _cache.RemoveAsync(
                BuildMineKey(snapshot.SubjectType, snapshot.SubjectId, userIdForMineKey.Value),
                cancellationToken);
        }
    }

    public Task InvalidateSnapshotAsync(
        string subjectType,
        int subjectId,
        int? userId,
        CancellationToken cancellationToken)
    {
        if (!_options.UseRedis)
        {
            return Task.CompletedTask;
        }

        var tasks = new List<Task>
        {
            _cache.RemoveAsync(BuildSummaryKey(subjectType, subjectId), cancellationToken)
        };

        if (userId.HasValue)
        {
            tasks.Add(_cache.RemoveAsync(BuildMineKey(subjectType, subjectId, userId.Value), cancellationToken));
        }

        return Task.WhenAll(tasks);
    }

    private static string BuildSummaryKey(string subjectType, int subjectId) =>
        $"rating:summary:{subjectType}:{subjectId}";

    private static string BuildMineKey(string subjectType, int subjectId, int userId) =>
        $"rating:mine:{subjectType}:{subjectId}:{userId}";

    private sealed class CachedRatingSnapshot
    {
        public string SubjectType { get; set; } = default!;

        public int SubjectId { get; set; }

        public decimal AvgScore { get; set; }

        public int TotalRatings { get; set; }

        public Dictionary<byte, int> Distribution { get; set; } = new();
    }

    private sealed class MyRatingCacheEntry
    {
        public byte Score { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
