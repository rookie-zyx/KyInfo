using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=KyInfoDb;Username=postgres;Password=KyInfo@2026"));
        
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<AppDbContext>();
    }
}
