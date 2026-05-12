using System.Net.Http.Headers;
using System.Net.Http.Json;
using KyInfo.Contracts.Admin;

namespace KyInfo.Blazor.ApiClient;

public sealed class AdminApiClient
{
    private readonly HttpClient _http;

    public AdminApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AdminUserListItemDto>> GetUsersAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/admin/users", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("加载用户列表失败。");

        var list = await response.Content.ReadFromJsonAsync<List<AdminUserListItemDto>>(cancellationToken: cancellationToken);
        return list ?? new List<AdminUserListItemDto>();
    }

    public async Task CreateUserAsync(AdminCreateUserRequest request, string? token = null, CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, "api/admin/users", token);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(httpRequest, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("创建用户失败。");
    }

    public async Task DeleteUserAsync(int id, string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, $"api/admin/users/{id}", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("删除用户失败。");
    }

    public async Task UpdateUserAsync(int id, AdminUpdateUserRequest request, string? token = null, CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Put, $"api/admin/users/{id}", token);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(httpRequest, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("修改用户失败。");
    }

    public async Task<List<AdminUserListItemDto>> GetAdministratorsAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/admin/administrators", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("加载管理员列表失败。");

        var list = await response.Content.ReadFromJsonAsync<List<AdminUserListItemDto>>(cancellationToken: cancellationToken);
        return list ?? new List<AdminUserListItemDto>();
    }

    public async Task CreateAdministratorAsync(
        AdminCreateAdministratorRequest request,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, "api/admin/administrators", token);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(httpRequest, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("创建管理员失败。");
    }

    public async Task RemoveAdministratorAsync(int id, string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, $"api/admin/administrators/{id}", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("撤权管理员失败。");
    }

    public async Task<byte[]> DownloadExamScoreTemplateAsync(string? token = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/admin/exam-scores/import-template", token);
        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("下载模板失败。");
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<ExcelImportResultDto> ImportExamScoresAsync(
        Stream excelStream,
        string fileName,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/admin/exam-scores/import", token);
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(excelStream);
        content.Add(streamContent, "file", string.IsNullOrWhiteSpace(fileName) ? "import.xlsx" : fileName);
        request.Content = content;

        var response = await _http.SendAsync(request, cancellationToken);
        await response.EnsureSuccessOrThrowAsync("导入失败。");

        var result = await response.Content.ReadFromJsonAsync<ExcelImportResultDto>(cancellationToken: cancellationToken);
        return result ?? new ExcelImportResultDto();
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
