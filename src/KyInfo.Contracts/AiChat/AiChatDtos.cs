using System.Text.Json.Serialization;

namespace KyInfo.Contracts.AiChat;

public class AiChatRequestDto
{
    /// <summary>单轮快捷输入：与 Messages 二选一（或同时存在时以 Messages 为准）。</summary>
    public string? Message { get; set; }

    /// <summary>多轮对话消息列表。</summary>
    public List<AiChatMessageDto>? Messages { get; set; }
}

public class AiChatMessageDto
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = "";
}

public class AiChatResponseDto
{
    public string Reply { get; set; } = "";
}

// 供 AI 网关实现内部使用（需要跨程序集可见，服务端程序集依赖它来构造/解析 OpenAI 请求）。
public sealed class OpenAiChatRequestPayload
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("messages")]
    public List<OpenAiChatMessagePayload> Messages { get; set; } = new();
}

public sealed class OpenAiChatMessagePayload
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public sealed class OpenAiChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public List<OpenAiChoice>? Choices { get; set; }
}

public sealed class OpenAiChoice
{
    [JsonPropertyName("message")]
    public OpenAiAssistantMessage? Message { get; set; }
}

public sealed class OpenAiAssistantMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

