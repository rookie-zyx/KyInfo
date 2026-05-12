using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Common;
using KyInfo.Contracts.Recommendations;
using KyInfo.Domain.Entities;
using KyInfo.Domain.Rules;

namespace KyInfo.Application.Services.Recommendations;

public class RecommendationAppService : IRecommendationAppService
{
    private readonly IExamScoreRepository _examScoreRepository;
    private readonly IScoreLineRepository _scoreLineRepository;

    private sealed class ScoredItem
    {
        public required RecommendationItemDto Dto { get; init; }
        public required KyInfo.Domain.Enums.RecommendationLevel LevelEnum { get; init; }
        public required int ScoreDiff { get; init; }
    }

    public RecommendationAppService(
        IExamScoreRepository examScoreRepository,
        IScoreLineRepository scoreLineRepository)
    {
        _examScoreRepository = examScoreRepository;
        _scoreLineRepository = scoreLineRepository;
    }

    public async Task<RecommendationResponseDto> GetRecommendationsAsync(
        RecommendationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.UserId <= 0)
        {
            throw new ArgumentException("userId 必填", nameof(request));
        }

        var top = request.Top;
        if (top <= 0) top = 30;
        if (top > 100) top = 100;

        var examScore = await _examScoreRepository
            .GetLatestByUserIdAsync(request.UserId, request.Year, cancellationToken);

        if (examScore is null)
        {
            throw new NotFoundException("未找到该用户的成绩记录");
        }

        var targetYear = examScore.Year;

        var scoreLines = await _scoreLineRepository
            .GetProfessionalScoreLinesAsync(targetYear, cancellationToken);

        if (scoreLines.Count == 0)
        {
            throw new NotFoundException("当前年份尚未录入专业分数线");
        }

        var scored = scoreLines
            .Select(sl =>
            {
                var diff = examScore.TotalScore - sl.Score;
                var levelEnum = RecommendationLevelRule.FromScoreDiff(diff);
                if (!RecommendationLevelRule.ShouldInclude(levelEnum))
                {
                    return null;
                }

                if (sl.Major is not { } major || major.School is not { } school)
                {
                    return null;
                }

                var dto = new RecommendationItemDto
                {
                    Year = targetYear,
                    UserTotalScore = examScore.TotalScore,
                    ScoreLine = sl.Score,
                    Level = RecommendationLevelRule.ToDisplayString(levelEnum),

                    SchoolId = school.Id,
                    SchoolName = school.Name,
                    SchoolShortName = school.ShortName,
                    Province = school.Province,
                    City = school.City,
                    LevelTag = school.LevelTag,

                    MajorId = major.Id,
                    MajorName = major.Name,
                    MajorCode = major.Code,
                    DisciplineCategory = major.DisciplineCategory,
                    DegreeType = major.DegreeType,
                    StudyType = major.StudyType,
                    DurationYears = major.DurationYears,
                    TuitionPerYear = major.TuitionPerYear
                };

                return new ScoredItem
                {
                    Dto = dto,
                    LevelEnum = levelEnum,
                    ScoreDiff = diff
                };
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderBy(x => RecommendationLevelRule.SortOrder(x.LevelEnum))
            .ThenByDescending(x => x.ScoreDiff)
            .ThenBy(x => x.Dto.SchoolName)
            .ThenBy(x => x.Dto.MajorName)
            .Take(top)
            .Select(x => x.Dto)
            .ToList();

        return new RecommendationResponseDto
        {
            UserId = request.UserId,
            UserName = examScore.User.UserName,
            Year = targetYear,
            UserTotalScore = examScore.TotalScore,
            Items = scored
        };
    }
}

