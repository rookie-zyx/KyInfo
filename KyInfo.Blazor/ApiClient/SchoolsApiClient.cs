using System.Net.Http.Json;
using KyInfo.Contracts.Schools;

namespace KyInfo.Blazor.ApiClient;

public sealed class SchoolsApiClient
{
    private readonly HttpClient _http;

    public SchoolsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<SchoolListItemDto>> SearchAsync(string? keyword, string? province, string? levelTag, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword)) query.Add($"keyword={Uri.EscapeDataString(keyword)}");
        if (!string.IsNullOrWhiteSpace(province)) query.Add($"province={Uri.EscapeDataString(province)}");
        if (!string.IsNullOrWhiteSpace(levelTag)) query.Add($"levelTag={Uri.EscapeDataString(levelTag)}");

        var url = "api/schools" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var response = await _http.GetAsync(url, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("院校查询失败。");

        var items = await response.Content.ReadFromJsonAsync<List<SchoolListItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<SchoolListItemDto>();
    }

    public async Task<SchoolDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"api/schools/{id}", cancellationToken);
        await response.EnsureSuccessOrThrowAsync("院校详情加载失败。");

        var dto = await response.Content.ReadFromJsonAsync<SchoolDetailDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "院校详情解析失败。");
        }

        return dto;
    }
}

