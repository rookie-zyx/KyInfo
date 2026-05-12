using System.Net;
using System.Net.Http.Json;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Contracts.Auth;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Enums;
using KyInfo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Tests;

public sealed class AuthIntegrationTests : IClassFixture<KyInfoApiFactory>
{
    private readonly KyInfoApiFactory _factory;

    public AuthIntegrationTests(KyInfoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_returns_token_for_valid_credentials()
    {
        var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            db.Users.Add(new User
            {
                UserName = "login-user",
                Email = "login-user@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UserNameOrEmail = "login-user",
            Password = "Test123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.Token));
        Assert.Equal("login-user", payload.UserName);
        Assert.Equal("User", payload.Role);
    }

    [Fact]
    public async Task GetMe_without_token_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Account/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
