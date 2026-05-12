using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KyInfo.Contracts.AiChat;
using KyInfo.Infrastructure.Options;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace KyInfo.Infrastructure.Ai;

/// <summary>
/// AI 对话网关（OpenAI 兼容接口）。
/// Infrastructure 层负责对外部 AI 服务的调用细节。
/// </summary>
public class AiChatGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<AiOptions> _options;

    public AiChatGateway(IHttpClientFactory httpClientFactory, IOptionsMonitor<AiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task<string> ChatAsync(IReadOnlyList<AiChatMessageDto> messages, CancellationToken cancellationToken)
    {
        var opts = _options.CurrentValue;
        if (string.IsNullOrWhiteSpace(opts.ApiKey))
        {
            throw new InvalidOperationException("未配置 Ai:ApiKey，请在 appsettings、User Secrets 或环境变量中设置。");
        }

        var client = _httpClientFactory.CreateClient("OpenAi");
        var model = string.IsNullOrWhiteSpace(opts.Model) ? "gpt-4o-mini" : opts.Model;

        var systemPrompt = string.IsNullOrWhiteSpace(opts.SystemPrompt)
            ? "你是 KyInfo 考研信息助手，帮助用户解答院校、专业、分数线、志愿填报等相关问题。回答简洁、准确，涉及录取与政策时提醒用户以官方招生简章为准。"
            : opts.SystemPrompt;

        var payloadMessages = new List<OpenAiChatMessagePayload>
        {
            new() { Role = "system", Content = systemPrompt! }
        };

        foreach (var m in messages)
        {
            if (string.IsNullOrWhiteSpace(m.Content))
            {
                continue;
            }

            var role = (m.Role ?? "user").Trim().ToLowerInvariant();
            if (role != "user" && role != "assistant")
            {
                role = "user";
            }

            payloadMessages.Add(new OpenAiChatMessagePayload { Role = role, Content = m.Content.Trim() });
        }

        if (payloadMessages.Count <= 1)
        {
            throw new ArgumentException("至少需要一条有效的用户或助手消息。");
        }

        var body = new OpenAiChatRequestPayload
        {
            Model = model,
            Messages = payloadMessages
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opts.ApiKey.Trim());
        request.Content = JsonContent.Create(body);

        using var response = await client.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"上游 AI 返回 {(int)response.StatusCode}：{raw}");
        }

        var parsed = JsonSerializer.Deserialize<OpenAiChatCompletionResponse>(raw, JsonOptions);
        var text = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("AI 返回内容为空。");
        }

        return text.Trim();
    }
}

