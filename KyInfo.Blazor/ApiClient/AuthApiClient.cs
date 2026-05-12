using System.Net.Http.Json;
using KyInfo.Contracts.Auth;

namespace KyInfo.Blazor.ApiClient;

public sealed class AuthApiClient
{
    private readonly HttpClient _http;

    public AuthApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/Auth/login", request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("登录失败，请检查用户名或密码。");

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
        if (auth is null || string.IsNullOrWhiteSpace(auth.Token))
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "登录响应异常，请稍后再试。");
        }

        return auth;
    }

    public async Task RegisterAsync(string userName, string email, string password, CancellationToken cancellationToken = default)
    {
        var body = new { userName, email, password };
        var response = await _http.PostAsJsonAsync("api/Auth/register", body, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("注册失败，请检查输入或稍后再试。");
    }
}

