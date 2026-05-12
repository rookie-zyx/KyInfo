using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Contracts.Auth;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Enums;
using KyInfo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Tests;

public sealed class AdminSecurityIntegrationTests : IClassFixture<KyInfoApiFactory>
{
    private readonly KyInfoApiFactory _factory;

    public AdminSecurityIntegrationTests(KyInfoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_token_cannot_access_root_only_administrators_list_returns_403()
    {
        var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            db.Users.Add(new User
            {
                UserName = "admintest",
                Email = "admintest@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var loginResp = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UserNameOrEmail = "admintest",
            Password = "Test123!"
        });
        loginResp.EnsureSuccessStatusCode();
        var auth = await loginResp.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth?.Token);

        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/admin/administrators");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        var resp = await client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }
}
