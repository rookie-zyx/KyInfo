using System.Net.Http.Json;
using KyInfo.Contracts.Majors;

namespace KyInfo.Blazor.ApiClient;

public sealed class MajorsApiClient
{
    private readonly HttpClient _http;

    public MajorsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MajorListItemDto>> SearchAsync(string? keyword, int? schoolId, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword)) query.Add($"keyword={Uri.EscapeDataString(keyword)}");
        if (schoolId.HasValue) query.Add($"schoolId={schoolId.Value}");

        var url = "api/majors" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var response = await _http.GetAsync(url, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("专业查询失败。");

        var items = await response.Content.ReadFromJsonAsync<List<MajorListItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<MajorListItemDto>();
    }

    public async Task<MajorDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"api/majors/{id}", cancellationToken);
        await response.EnsureSuccessOrThrowAsync("专业详情加载失败。");

        var dto = await response.Content.ReadFromJsonAsync<MajorDetailDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "专业详情解析失败。");
        }

        return dto;
    }
}

