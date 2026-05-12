using System.Net.Http.Json;
using KyInfo.Contracts.Recommendations;

namespace KyInfo.Blazor.ApiClient;

public sealed class RecommendationsApiClient
{
    private readonly HttpClient _http;

    public RecommendationsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<RecommendationResponseDto> GetAsync(int userId, int? year, int top = 30, CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"userId={userId}", $"top={top}" };
        if (year.HasValue) query.Add($"year={year.Value}");

        var url = "api/recommendations?" + string.Join("&", query);
        var response = await _http.GetAsync(url, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("获取推荐失败。");

        var dto = await response.Content.ReadFromJsonAsync<RecommendationResponseDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "未获取到推荐结果。");
        }

        return dto;
    }
}

