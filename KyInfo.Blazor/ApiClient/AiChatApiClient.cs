using System.Net.Http.Json;
using System.Net.Http.Headers;
using KyInfo.Contracts.AiChat;

namespace KyInfo.Blazor.ApiClient;

public sealed class AiChatApiClient
{
    private readonly HttpClient _http;

    public AiChatApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AiChatResponseDto> ChatAsync(AiChatRequestDto request, string? token = null, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/ai/chat")
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
        }

        using var response = await _http.SendAsync(httpRequest, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("AI 对话请求失败。");

        var dto = await response.Content.ReadFromJsonAsync<AiChatResponseDto>(cancellationToken: cancellationToken);
        if (dto is null || string.IsNullOrWhiteSpace(dto.Reply))
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "未收到有效回复。");
        }

        return dto;
    }
}

