using System.Net.Http.Json;
using KyInfo.Contracts.RecruitInfos;

namespace KyInfo.Blazor.ApiClient;

public sealed class RecruitInfosApiClient
{
    private readonly HttpClient _http;

    public RecruitInfosApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RecruitInfoListItemDto>> SearchAsync(int? schoolId, int? majorId, int? year, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (schoolId.HasValue) query.Add($"schoolId={schoolId.Value}");
        if (majorId.HasValue) query.Add($"majorId={majorId.Value}");
        if (year.HasValue) query.Add($"year={year.Value}");

        var url = "api/recruitinfos" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var response = await _http.GetAsync(url, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("简章查询失败。");

        var items = await response.Content.ReadFromJsonAsync<List<RecruitInfoListItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<RecruitInfoListItemDto>();
    }

    public async Task<RecruitInfoDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"api/recruitinfos/{id}", cancellationToken);
        await response.EnsureSuccessOrThrowAsync("简章详情加载失败。");

        var dto = await response.Content.ReadFromJsonAsync<RecruitInfoDetailDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "简章详情解析失败。");
        }

        return dto;
    }
}

