using KyInfo.Infrastructure.Persistence;
using KyInfo.Infrastructure.Ai;
using KyInfo.Application.Abstractions.Repositories;
using KyInfo.Application.Abstractions.Identity;
using KyInfo.Infrastructure.Identity;
using KyInfo.Infrastructure.Persistence.Repositories.Recommendations;
using KyInfo.Infrastructure.Persistence.Repositories.Schools;
using KyInfo.Infrastructure.Persistence.Repositories.RecruitInfos;
using KyInfo.Infrastructure.Persistence.Repositories.Majors;
using KyInfo.Infrastructure.Persistence.Repositories.Audit;
using KyInfo.Application.Abstractions.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddKyInfoInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        });

        // AI 服务注册（仍由 Api 层提供 HttpClient/配置，Infrastructure 只负责服务本体）
        services.AddScoped<AiGroundingService>();
        services.AddScoped<AiChatGateway>();

        // Recommendations 仓储（用于应用层用例编排）
        services.AddScoped<IExamScoreRepository, ExamScoreRepository>();
        services.AddScoped<IScoreLineRepository, ScoreLineRepository>();

        // Auth 仓储与密码/Token 能力
        services.AddScoped<IUserRepository, Persistence.Repositories.Auth.UserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();

        // Schools 仓储
        services.AddScoped<ISchoolRepository, SchoolRepository>();

        // RecruitInfos 仓储
        services.AddScoped<IRecruitInfoRepository, RecruitInfoRepository>();

        // Majors 仓储
        services.AddScoped<IMajorRepository, MajorRepository>();

        // JWT 令牌签发
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}

