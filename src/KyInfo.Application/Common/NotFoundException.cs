namespace KyInfo.Application.Common;

/// <summary>
/// 应用层的“未找到”异常（由 Controller 映射到 HTTP 404）。
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

