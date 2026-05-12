using System.Collections.Generic;
using KyInfo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Tests;

public sealed class KyInfoApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"KyInfoIntegrationTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting 确保在 Program 读取 Jwt:Key 与 JwtTokenService 签发时配置一致（早于部分 ConfigureAppConfiguration 合并时序问题）。
        builder.UseSetting("Jwt:Key", "unit-test-jwt-key-at-least-32-characters!!");
        builder.UseSetting("Jwt:Issuer", "KyInfoApp");
        builder.UseSetting("Jwt:Audience", "KyInfoUsers");
        builder.UseSetting("Jwt:ExpireMinutes", "60");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "unused");
        builder.UseSetting("Seed:Admin:Enabled", "false");

        builder.ConfigureServices(services =>
        {
            var toRemove = new List<ServiceDescriptor>();
            foreach (var d in services)
            {
                if (d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                {
                    toRemove.Add(d);
                }
                else if (d.ServiceType == typeof(AppDbContext))
                {
                    toRemove.Add(d);
                }
            }

            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
