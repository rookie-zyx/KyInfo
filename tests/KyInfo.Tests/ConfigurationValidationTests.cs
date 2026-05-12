using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;

namespace KyInfo.Tests;

public sealed class ConfigurationValidationTests
{
    [Fact]
    public void Missing_cors_origins_in_production_fails_startup()
    {
        using var factory = new InvalidConfigurationFactory(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("Jwt:Key", "unit-test-jwt-key-at-least-32-characters!!");
            builder.UseSetting("Jwt:Issuer", "KyInfoApp");
            builder.UseSetting("Jwt:Audience", "KyInfoUsers");
            builder.UseSetting("Jwt:ExpireMinutes", "60");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "unused");
            builder.UseSetting("Seed:Admin:Enabled", "false");
        });

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
        Assert.Contains("Cors:Origins", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Invalid_jwt_expire_minutes_fails_startup()
    {
        using var factory = new InvalidConfigurationFactory(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Key", "unit-test-jwt-key-at-least-32-characters!!");
            builder.UseSetting("Jwt:Issuer", "KyInfoApp");
            builder.UseSetting("Jwt:Audience", "KyInfoUsers");
            builder.UseSetting("Jwt:ExpireMinutes", "0");
            builder.UseSetting("ConnectionStrings:DefaultConnection", "unused");
            builder.UseSetting("Seed:Admin:Enabled", "false");
        });

        var exception = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
        Assert.Contains("ExpireMinutes", exception.Message, StringComparison.Ordinal);
    }

    private sealed class InvalidConfigurationFactory : WebApplicationFactory<Program>
    {
        private readonly Action<IWebHostBuilder> _configureBuilder;

        public InvalidConfigurationFactory(Action<IWebHostBuilder> configureBuilder)
        {
            _configureBuilder = configureBuilder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _configureBuilder(builder);
        }
    }
}
