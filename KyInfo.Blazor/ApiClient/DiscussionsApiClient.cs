using System.Net.Http.Headers;
using System.Net.Http.Json;
using KyInfo.Contracts.Discussions;

namespace KyInfo.Blazor.ApiClient;

public sealed class DiscussionsApiClient
{
    private readonly HttpClient _http;

    public DiscussionsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<DiscussionListResponseDto> SearchAsync(
        int page,
        int pageSize,
        string? keyword,
        string? sort = null,
        int? authorUserId = null,
        string? zone = null,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query.Add($"keyword={Uri.EscapeDataString(keyword.Trim())}");
        }
        if (!string.IsNullOrWhiteSpace(sort))
        {
            query.Add($"sort={Uri.EscapeDataString(sort.Trim())}");
        }
        if (authorUserId.HasValue && authorUserId.Value > 0)
        {
            query.Add($"authorUserId={authorUserId.Value}");
        }
        if (!string.IsNullOrWhiteSpace(zone))
        {
            query.Add($"zone={Uri.EscapeDataString(zone.Trim())}");
        }

        var url = "api/discussions?" + string.Join("&", query);
        using var request = CreateRequest(HttpMethod.Get, url, token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("讨论区列表加载失败。");

        var result = await response.Content.ReadFromJsonAsync<DiscussionListResponseDto>(cancellationToken: cancellationToken)
                     ?? new DiscussionListResponseDto();
        if (result.Items is null)
        {
            result.Items = Array.Empty<DiscussionListItemDto>();
        }

        return result;
    }

    public async Task<DiscussionDetailDto> GetByIdAsync(
        int id,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"api/discussions/{id}", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("讨论帖加载失败。");

        var dto = await response.Content.ReadFromJsonAsync<DiscussionDetailDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "讨论帖解析失败。");
        }

        return dto;
    }

    public async Task<int> CreateDiscussionAsync(
        DiscussionCreateDto dto,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/discussions", token);
        request.Content = JsonContent.Create(dto);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("发帖失败。");
        return await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
    }

    public async Task<int> CreateCommentAsync(
        int discussionId,
        DiscussionCommentCreateDto dto,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, $"api/discussions/{discussionId}/comments", token);
        request.Content = JsonContent.Create(dto);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("评论失败。");
        return await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
    }

    public async Task DeleteDiscussionAsync(int id, string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, $"api/discussions/{id}", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("删除讨论帖失败。");
    }

    public async Task DeleteCommentAsync(int commentId, string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, $"api/discussions/comments/{commentId}", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("删除评论失败。");
    }

    public async Task<List<StickerDto>> GetStickersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync("api/discussions/stickers", cancellationToken);
        await response.EnsureSuccessOrThrowAsync("贴纸列表加载失败。");
        var items = await response.Content.ReadFromJsonAsync<List<StickerDto>>(cancellationToken: cancellationToken);
        return items ?? new List<StickerDto>();
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
