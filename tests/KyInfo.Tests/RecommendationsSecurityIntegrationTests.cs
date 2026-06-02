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

public sealed class RecommendationsSecurityIntegrationTests : IClassFixture<KyInfoApiFactory>
{
    private readonly KyInfoApiFactory _factory;

    public RecommendationsSecurityIntegrationTests(KyInfoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_recommendations_without_token_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/recommendations?userId=1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User_cannot_get_recommendations_for_another_user()
    {
        var client = _factory.CreateClient();
        int userBId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            db.Users.Add(new User
            {
                UserName = "reco-user-a",
                Email = "reco-a@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            });
            var userB = new User
            {
                UserName = "reco-user-b",
                Email = "reco-b@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(userB);
            await db.SaveChangesAsync();
            userBId = userB.Id;
        }

        var login = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UserNameOrEmail = "reco-user-a",
            Password = "Test123!"
        });
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        var response = await client.GetAsync($"/api/recommendations?userId={userBId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
