namespace KyInfo.Blazor.Auth;

public sealed class BrowserSessionDto
{
    public bool IsAuthenticated { get; set; }

    // Temporary bridge for Blazor Server circuits: lets the server-side TokenStore
    // reacquire the JWT after login/refresh without persisting it in browser storage.
    public string? Token { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Role { get; set; }
}

public sealed class BrowserAuthResult
{
    public bool Ok { get; set; }

    public int Status { get; set; }

    public string? Message { get; set; }

    public BrowserSessionDto? Session { get; set; }
}
