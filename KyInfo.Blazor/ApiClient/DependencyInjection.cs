using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KyInfo.Blazor.ApiClient;

public static class DependencyInjection
{
    public static IServiceCollection AddKyInfoApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("KyInfoApi", client =>
        {
            client.BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? "https://localhost:7233");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<TokenDelegatingHandler>();

        services.AddHttpClient<AuthApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<SchoolsApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<AccountApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<RecommendationsApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<ScoreLinesApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<ExamScoresApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<MajorsApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<RecruitInfosApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(60);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<AiChatApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(120);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        services.AddHttpClient<AdminApiClient>((sp, http) =>
        {
            http.BaseAddress = sp.GetRequiredService<IHttpClientFactory>().CreateClient("KyInfoApi").BaseAddress;
            http.Timeout = TimeSpan.FromSeconds(120);
        }).AddHttpMessageHandler<TokenDelegatingHandler>();

        return services;
    }
}

