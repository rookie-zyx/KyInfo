using System.Net.Http.Json;
using KyInfo.Contracts.ExamScores;

namespace KyInfo.Blazor.ApiClient;

public sealed class ExamScoresApiClient
{
    private readonly HttpClient _http;

    public ExamScoresApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ExamScoreListItemDto>> SearchAsync(
        int userId,
        int? year,
        int? schoolId,
        int? majorId,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"userId={userId}" };
        if (year.HasValue) query.Add($"year={year.Value}");
        if (schoolId.HasValue) query.Add($"schoolId={schoolId.Value}");
        if (majorId.HasValue) query.Add($"majorId={majorId.Value}");

        var url = "api/examscores?" + string.Join("&", query);
        var response = await _http.GetAsync(url, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("成绩查询失败。");

        var items = await response.Content.ReadFromJsonAsync<List<ExamScoreListItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<ExamScoreListItemDto>();
    }

    public async Task<int> CreateAsync(ExamScoreCreateDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/examscores", dto, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("新增成绩失败。");

        // Api 返回的是 int id
        var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        return id;
    }
}

