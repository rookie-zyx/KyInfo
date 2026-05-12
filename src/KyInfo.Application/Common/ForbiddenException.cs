namespace KyInfo.Application.Common;

/// <summary>
/// 已认证但无权执行该操作（HTTP 403）。
/// </summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
