using System.Net.Http.Headers;
using System.Net.Http.Json;
using KyInfo.Contracts.Account;

namespace KyInfo.Blazor.ApiClient;

public sealed class AccountApiClient
{
    private readonly HttpClient _http;

    public AccountApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AccountProfileDto> GetMeAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/Account/me", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("无法加载账户信息。");

        var dto = await response.Content.ReadFromJsonAsync<AccountProfileDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "账户数据解析失败。");
        }

        return dto;
    }

    public async Task<AccountProfileDto> UpdateProfileAsync(
        UpdateAccountRequest request,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Put, "api/Account/profile", token);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(httpRequest, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("保存失败。");

        var dto = await response.Content.ReadFromJsonAsync<AccountProfileDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "账户数据解析失败。");
        }

        return dto;
    }

    public async Task ChangePasswordAsync(
        ChangePasswordRequest request,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, "api/Account/change-password", token);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(httpRequest, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("修改失败。");
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string? token)
    {
        var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
        }

        return request;
    }
}

