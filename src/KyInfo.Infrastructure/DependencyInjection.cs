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
using KyInfo.Infrastructure.Persistence.Repositories.Discussions;
using KyInfo.Infrastructure.Persistence.Repositories.Ratings;
using KyInfo.Application.Abstractions.Audit;
using KyInfo.Application.Abstractions.Caching;
using KyInfo.Infrastructure.Caching;
using KyInfo.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddKyInfoInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RatingOptions>(config.GetSection(RatingOptions.SectionName));

        var ratingOptions = config.GetSection(RatingOptions.SectionName).Get<RatingOptions>() ?? new RatingOptions();
        var redisConnection = config.GetConnectionString("Redis");
        if (ratingOptions.UseRedis && !string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<IRatingCache, RatingCacheService>();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<AiGroundingService>();
        services.AddScoped<AiChatGateway>();

        services.AddScoped<IExamScoreRepository, ExamScoreRepository>();
        services.AddScoped<IScoreLineRepository, ScoreLineRepository>();

        services.AddScoped<IUserRepository, Persistence.Repositories.Auth.UserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();

        services.AddScoped<ISchoolRepository, SchoolRepository>();

        services.AddScoped<IRecruitInfoRepository, RecruitInfoRepository>();

        services.AddScoped<IMajorRepository, MajorRepository>();

        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<IDiscussionRepository, DiscussionRepository>();

        services.AddScoped<IRatingRepository, RatingRepository>();

        return services;
    }
}
