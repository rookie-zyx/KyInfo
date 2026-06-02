using System.Net;
using System.Net.Http.Json;
using KyInfo.Contracts.ScoreLines;
using KyInfo.Domain.Entities;
using KyInfo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Tests;

public sealed class ScoreLinesIntegrationTests : IClassFixture<KyInfoApiFactory>
{
    private readonly KyInfoApiFactory _factory;

    public ScoreLinesIntegrationTests(KyInfoApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTrend_aggregates_by_year()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ScoreLines.AddRange(
                new ScoreLine { Year = 2023, Score = 320, IsNational = true },
                new ScoreLine { Year = 2023, Score = 340, IsNational = true },
                new ScoreLine { Year = 2024, Score = 350, IsNational = true });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var points = await client.GetFromJsonAsync<List<ScoreLineTrendPointDto>>("/api/scorelines/trend?isNational=true");

        Assert.NotNull(points);
        Assert.Equal(2, points!.Count);
        Assert.Equal(2023, points[0].Year);
        Assert.Equal(330, points[0].Score);
        Assert.Equal(2024, points[1].Year);
        Assert.Equal(350, points[1].Score);
    }

    [Fact]
    public async Task GetTrend_with_schoolName_filter_returns_matching_rows()
    {
        int schoolId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var school = new School
            {
                Name = "清华大学",
                Province = "北京",
                City = "北京",
                LevelTag = "985",
                Type = "综合",
                Property = "公办"
            };
            db.Schools.Add(school);
            await db.SaveChangesAsync();
            schoolId = school.Id;

            db.ScoreLines.AddRange(
                new ScoreLine { Year = 2024, Score = 380, IsNational = false, SchoolId = schoolId },
                new ScoreLine { Year = 2024, Score = 360, IsNational = false, SchoolId = null });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/scorelines/trend?schoolName=清华");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var points = await response.Content.ReadFromJsonAsync<List<ScoreLineTrendPointDto>>();
        Assert.NotNull(points);
        Assert.Single(points!);
        Assert.Equal(380, points![0].Score);
    }

    [Fact]
    public async Task GetTrend_with_no_data_returns_empty_list_not_error()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/scorelines/trend?schoolName=__no_such_school__");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var points = await response.Content.ReadFromJsonAsync<List<ScoreLineTrendPointDto>>();
        Assert.NotNull(points);
        Assert.Empty(points!);
    }
}
