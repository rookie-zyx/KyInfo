using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Contracts.Auth;
using KyInfo.Contracts.ExamScores;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Enums;
using KyInfo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Tests;

public sealed class ExamScoresSecurityIntegrationTests : IClassFixture<KyInfoApiFactory>
{
    private readonly KyInfoApiFactory _factory;

    public ExamScoresSecurityIntegrationTests(KyInfoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_exam_scores_without_token_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/examscores?userId=1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User_cannot_create_score_for_another_user()
    {
        var client = _factory.CreateClient();
        int userAId;
        int userBId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var userA = new User
            {
                UserName = "score-user-a",
                Email = "score-a@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };
            var userB = new User
            {
                UserName = "score-user-b",
                Email = "score-b@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.AddRange(userA, userB);
            await db.SaveChangesAsync();
            userAId = userA.Id;
            userBId = userB.Id;
        }

        var login = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UserNameOrEmail = "score-user-a",
            Password = "Test123!"
        });
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth?.Token);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PostAsJsonAsync("/api/examscores", new ExamScoreCreateDto
        {
            UserId = userBId,
            Year = 2025,
            TotalScore = 360
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User_cannot_query_another_users_scores()
    {
        var client = _factory.CreateClient();
        int userBId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            db.Users.Add(new User
            {
                UserName = "score-query-a",
                Email = "score-qa@local.test",
                PasswordHash = hasher.HashPassword("Test123!"),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            });
            var userB = new User
            {
                UserName = "score-query-b",
                Email = "score-qb@local.test",
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
            UserNameOrEmail = "score-query-a",
            Password = "Test123!"
        });
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        var response = await client.GetAsync($"/api/examscores?userId={userBId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
