using KyInfo.Contracts.Auth;

namespace KyInfo.Application.Services.Auth;

public interface IAuthAppService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// 登录失败时返回 null（由 Controller 映射成 401）。
    /// </summary>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

