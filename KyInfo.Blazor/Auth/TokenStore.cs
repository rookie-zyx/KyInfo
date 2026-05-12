namespace KyInfo.Blazor.Auth;

/// <summary>
/// 保存当前用户会话内的 Bearer token（避免每次请求都依赖 localStorage/JS）。
/// </summary>
public sealed class TokenStore
{
    public string? Token { get; set; }
}

