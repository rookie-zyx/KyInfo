namespace KyInfo.Blazor.Auth;

/// <summary>
/// 保存当前用户会话内的认证状态，避免每次请求都依赖浏览器存储。
/// </summary>
public sealed class TokenStore
{
    public string? Token { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Role { get; set; }

    public void SetAuthenticated(string token, string? userName = null, string? role = null, int? userId = null)
    {
        Token = token;
        UserName = userName;
        Role = role;
        UserId = userId;
    }

    public void SetProfile(int userId, string userName, string role)
    {
        UserId = userId;
        UserName = userName;
        Role = role;
    }

    public void Clear()
    {
        Token = null;
        UserId = null;
        UserName = null;
        Role = null;
    }
}

