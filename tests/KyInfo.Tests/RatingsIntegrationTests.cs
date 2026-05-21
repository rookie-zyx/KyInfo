using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Contracts.Auth;
using KyInfo.Contracts.Ratings;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Enums;
using KyInfo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Tests;

public sealed class RatingsIntegrationTests : IClassFixture<KyInfoApiFactory>
{
    private readonly KyInfoApiFactory _factory;

    public RatingsIntegrationTests(KyInfoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task First_submit_then_update_keeps_total_and_adjusts_avg_and_distribution()
    {
        var client = _factory.CreateClient();
        await EnsureUserAsync("rating-user-a", "rating-a@local.test");

        var token = await LoginAsync(client, "rating-user-a");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var first = await client.PostAsJsonAsync("/api/ratings", new RatingSubmitDto
        {
            SubjectType = "school",
            SubjectId = 1001,
            Score = 5,
            Comment = "很好"
        });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstResult = await first.Content.ReadFromJsonAsync<RatingSubmitResultDto>();
        Assert.NotNull(firstResult);
        Assert.False(firstResult!.IsUpdate);
        Assert.Equal(1, firstResult.TotalRatings);
        Assert.Equal(5m, firstResult.AvgScore);
        Assert.Equal(1, firstResult.Distribution[5]);

        var second = await client.PostAsJsonAsync("/api/ratings", new RatingSubmitDto
        {
            SubjectType = "school",
            SubjectId = 1001,
            Score = 3,
            Comment = "改评"
        });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondResult = await second.Content.ReadFromJsonAsync<RatingSubmitResultDto>();
        Assert.NotNull(secondResult);
        Assert.True(secondResult!.IsUpdate);
        Assert.Equal(1, secondResult.TotalRatings);
        Assert.Equal(3m, secondResult.AvgScore);
        Assert.False(secondResult.Distribution.ContainsKey(5));
        Assert.Equal(1, secondResult.Distribution[3]);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var subject = db.RatingSubjects.Single(s => s.SubjectType == "school" && s.SubjectId == 1001);
        Assert.Equal(1, db.UserRatings.Count(r => r.RatingSubjectId == subject.Id));
        Assert.Equal(3m, subject.AvgScore);
        Assert.Equal(1, subject.TotalRatings);
    }

    [Fact]
    public async Task Two_users_average_and_distribution_are_correct()
    {
        var clientA = _factory.CreateClient();
        var clientB = _factory.CreateClient();
        await EnsureUserAsync("rating-user-b1", "rating-b1@local.test");
        await EnsureUserAsync("rating-user-b2", "rating-b2@local.test");

        var tokenA = await LoginAsync(clientA, "rating-user-b1");
        var tokenB = await LoginAsync(clientB, "rating-user-b2");
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var r1 = await clientA.PostAsJsonAsync("/api/ratings", new RatingSubmitDto
        {
            SubjectType = "major",
            SubjectId = 42,
            Score = 4
        });
        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);

        var r2 = await clientB.PostAsJsonAsync("/api/ratings", new RatingSubmitDto
        {
            SubjectType = "major",
            SubjectId = 42,
            Score = 2
        });
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);

        var detail = await clientA.GetFromJsonAsync<RatingDetailDto>("/api/ratings/major/42");
        Assert.NotNull(detail);
        Assert.Equal(2, detail!.TotalRatings);
        Assert.Equal(3m, detail.AvgScore);
        Assert.Equal(1, detail.Distribution[4]);
        Assert.Equal(1, detail.Distribution[2]);
        Assert.Equal(4, detail.MyRating!.Score);
    }

    [Fact]
    public async Task Submit_without_token_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/ratings", new RatingSubmitDto
        {
            SubjectType = "school",
            SubjectId = 1,
            Score = 5
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task EnsureUserAsync(string userName, string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Users.Any(u => u.UserName == userName))
        {
            return;
        }

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new User
        {
            UserName = userName,
            Email = email,
            PasswordHash = hasher.HashPassword("Test123!"),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task<string> LoginAsync(HttpClient client, string userName)
    {
        var login = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UserNameOrEmail = userName,
            Password = "Test123!"
        });
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        return auth!.Token;
    }
}
