using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KyInfo.Blazor.ApiClient;

public static class DependencyInjection
{
    public static IServiceCollection AddKyInfoApiClients(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? "https://localhost:7233");
            client.Timeout = TimeSpan.FromSeconds(60);
        }

        var kyInfoApiBuilder = services.AddHttpClient("KyInfoApi", ConfigureClient);
        if (environment.IsDevelopment())
        {
            // 本地开发证书可能未信任，允许调用 https://localhost:7233
            kyInfoApiBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        }

        services.AddScoped<TokenDelegatingHandler>();

        IHttpClientBuilder AddApiClient<TClient>() where TClient : class
        {
            var builder = services.AddHttpClient<TClient>((sp, http) =>
            {
                ConfigureClient(http);
            }).AddHttpMessageHandler<TokenDelegatingHandler>();

            if (environment.IsDevelopment())
            {
                builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            }

            return builder;
        }

        AddApiClient<AuthApiClient>();
        AddApiClient<SchoolsApiClient>();
        AddApiClient<AccountApiClient>();
        AddApiClient<RecommendationsApiClient>();
        AddApiClient<ScoreLinesApiClient>();
        AddApiClient<ExamScoresApiClient>();
        AddApiClient<MajorsApiClient>();
        AddApiClient<RecruitInfosApiClient>();
        AddApiClient<AiChatApiClient>().ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(120));
        AddApiClient<AdminApiClient>().ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(120));
        AddApiClient<DiscussionsApiClient>();

        return services;
    }
}

