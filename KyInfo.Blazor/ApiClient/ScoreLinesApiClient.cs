using System.Net.Http.Json;
using KyInfo.Contracts.ScoreLines;

namespace KyInfo.Blazor.ApiClient;

public sealed class ScoreLinesApiClient
{
    private readonly HttpClient _http;

    public ScoreLinesApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ScoreLineListItemDto>> SearchAsync(
        int? schoolId,
        int? majorId,
        int? year,
        bool? isNational,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (schoolId.HasValue) query.Add($"schoolId={schoolId.Value}");
        if (majorId.HasValue) query.Add($"majorId={majorId.Value}");
        if (year.HasValue) query.Add($"year={year.Value}");
        if (isNational.HasValue) query.Add($"isNational={isNational.Value.ToString().ToLowerInvariant()}");

        var url = "api/scorelines" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var response = await _http.GetAsync(url, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("分数线查询失败。");

        var items = await response.Content.ReadFromJsonAsync<List<ScoreLineListItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<ScoreLineListItemDto>();
    }

    public async Task<List<ScoreLineTrendPointDto>> GetTrendAsync(
        int? schoolId,
        int? majorId,
        bool? isNational,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (schoolId.HasValue) query.Add($"schoolId={schoolId.Value}");
        if (majorId.HasValue) query.Add($"majorId={majorId.Value}");
        if (isNational.HasValue) query.Add($"isNational={isNational.Value.ToString().ToLowerInvariant()}");

        var url = "api/scorelines/trend" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var response = await _http.GetAsync(url, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("趋势数据加载失败。");

        var points = await response.Content.ReadFromJsonAsync<List<ScoreLineTrendPointDto>>(cancellationToken: cancellationToken);
        return points ?? new List<ScoreLineTrendPointDto>();
    }
}

