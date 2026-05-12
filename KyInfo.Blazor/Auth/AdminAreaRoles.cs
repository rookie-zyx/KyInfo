namespace KyInfo.Blazor.Auth;

/// <summary>
/// 可进入管理后台（与 API 的 Root,Admin 授权一致）的角色。
/// </summary>
public static class AdminAreaRoles
{
    public static bool IsStaff(string? role) =>
        string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, "Root", StringComparison.OrdinalIgnoreCase);
}
