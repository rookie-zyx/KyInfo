using KyInfo.Application.Services.Account;
using KyInfo.Application.Services.Admin;
using KyInfo.Application.Services.Auth;
using KyInfo.Application.Services.ExamScores;
using KyInfo.Application.Services.Majors;
using KyInfo.Application.Services.Recommendations;
using KyInfo.Application.Services.RecruitInfos;
using KyInfo.Application.Services.Schools;
using KyInfo.Application.Services.ScoreLines;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddKyInfoApplication(this IServiceCollection services)
    {
        services.AddScoped<IRecommendationAppService, RecommendationAppService>();
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<ISchoolAppService, SchoolAppService>();
        services.AddScoped<IExamScoreAppService, ExamScoreAppService>();
        services.AddScoped<IScoreLineAppService, ScoreLineAppService>();
        services.AddScoped<IRecruitInfoAppService, RecruitInfoAppService>();
        services.AddScoped<IMajorAppService, MajorAppService>();
        services.AddScoped<IAccountAppService, AccountAppService>();
        services.AddScoped<IAdminAppService, AdminAppService>();

        return services;
    }
}

