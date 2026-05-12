namespace KyInfo.Infrastructure.Options;

public class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>OpenAI 兼容 API 根路径，例如 https://api.openai.com/v1 或 Azure/OpenRouter 等。</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>API Key（勿提交到仓库，可用 User Secrets 或环境变量）。</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>模型名称，例如 gpt-4o-mini、gpt-4o。</summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>系统提示词，可覆盖默认考研助手人设。</summary>
    public string? SystemPrompt { get; set; }
}

