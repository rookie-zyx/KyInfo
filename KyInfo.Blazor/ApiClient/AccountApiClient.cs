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

    public async Task<AccountProfileDto> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync("api/Account/me", cancellationToken);
        await response.EnsureSuccessOrThrowAsync("无法加载账户信息。");

        var dto = await response.Content.ReadFromJsonAsync<AccountProfileDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "账户数据解析失败。");
        }

        return dto;
    }

    public async Task<AccountProfileDto> UpdateProfileAsync(UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsJsonAsync("api/Account/profile", request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("保存失败。");

        var dto = await response.Content.ReadFromJsonAsync<AccountProfileDto>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "账户数据解析失败。");
        }

        return dto;
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("api/Account/change-password", request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("修改失败。");
    }
}

