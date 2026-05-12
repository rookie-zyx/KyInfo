using System.Net.Http.Headers;
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
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"userId={userId}" };
        if (year.HasValue) query.Add($"year={year.Value}");
        if (schoolId.HasValue) query.Add($"schoolId={schoolId.Value}");
        if (majorId.HasValue) query.Add($"majorId={majorId.Value}");

        var url = "api/examscores?" + string.Join("&", query);
        using var request = CreateRequest(HttpMethod.Get, url, token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("成绩查询失败。");

        var items = await response.Content.ReadFromJsonAsync<List<ExamScoreListItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<ExamScoreListItemDto>();
    }

    public async Task<int> CreateAsync(ExamScoreCreateDto dto, string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/examscores", token);
        request.Content = JsonContent.Create(dto);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("新增成绩失败。");

        // Api 返回的是 int id
        var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        return id;
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string? token)
    {
        var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
        }

        return request;
    }
}

