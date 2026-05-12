using KyInfo.Blazor.Components;
using KyInfo.Blazor.ApiClient;
using KyInfo.Blazor.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KyInfo.Contracts.Account;
using KyInfo.Contracts.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenStore>();

builder.Services.AddKyInfoApiClients(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapPost("/bff/auth/login", async (
    LoginRequest request,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var client = httpClientFactory.CreateClient("KyInfoApi");
    using var response = await client.PostAsJsonAsync("api/Auth/login", request, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        var message = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? "登录失败，请检查用户名或密码。";
        return Results.Json(new { message }, statusCode: (int)response.StatusCode);
    }

    var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
    if (auth is null || string.IsNullOrWhiteSpace(auth.Token))
    {
        return Results.Json(new { message = "登录响应异常，请稍后再试。" }, statusCode: StatusCodes.Status500InternalServerError);
    }

    httpContext.Response.Cookies.Append(
        TokenDelegatingHandler.AuthCookieName,
        auth.Token,
        CreateAuthCookieOptions(httpContext, ResolveCookieLifetimeMinutes(configuration)));

    var session = await TryLoadSessionAsync(httpClientFactory, auth.Token, cancellationToken)
        ?? new BrowserSessionDto
        {
            IsAuthenticated = true,
            Token = auth.Token,
            UserName = auth.UserName,
            Role = auth.Role
        };

    return Results.Ok(new
    {
        session
    });
});

app.MapPost("/bff/auth/logout", (HttpContext httpContext) =>
{
    httpContext.Response.Cookies.Delete(TokenDelegatingHandler.AuthCookieName, CreateDeleteCookieOptions(httpContext));
    return Results.Ok(new { message = "已退出登录" });
});

app.MapGet("/bff/auth/session", async (
    IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var token = httpContext.Request.Cookies[TokenDelegatingHandler.AuthCookieName];
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.Ok(new { session = new BrowserSessionDto { IsAuthenticated = false } });
    }

    var client = httpClientFactory.CreateClient("KyInfoApi");
    var session = await TryLoadSessionAsync(httpClientFactory, token, cancellationToken);
    if (session is null)
    {
        httpContext.Response.Cookies.Delete(TokenDelegatingHandler.AuthCookieName, CreateDeleteCookieOptions(httpContext));
        return Results.Ok(new { session = new BrowserSessionDto { IsAuthenticated = false } });
    }
    return Results.Ok(new { session });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static CookieOptions CreateAuthCookieOptions(HttpContext httpContext, int expireMinutes)
{
    return new CookieOptions
    {
        HttpOnly = true,
        Secure = httpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/",
        Expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes > 0 ? expireMinutes : 60)
    };
}

static int ResolveCookieLifetimeMinutes(IConfiguration configuration)
{
    var configured = configuration.GetValue<int?>("Jwt:ExpireMinutes");
    return configured is > 0 ? configured.Value : 60;
}

static CookieOptions CreateDeleteCookieOptions(HttpContext httpContext)
{
    return new CookieOptions
    {
        HttpOnly = true,
        Secure = httpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/"
    };
}

static async Task<string?> TryReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
{
    try
    {
        var payload = await response.Content.ReadFromJsonAsync<JsonObjectEnvelope>(cancellationToken: cancellationToken);
        return payload?.Message;
    }
    catch
    {
        return null;
    }
}

static async Task<BrowserSessionDto?> TryLoadSessionAsync(
    IHttpClientFactory httpClientFactory,
    string token,
    CancellationToken cancellationToken)
{
    var client = httpClientFactory.CreateClient("KyInfoApi");
    using var request = new HttpRequestMessage(HttpMethod.Get, "api/Account/me");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        return null;
    }

    var profile = await response.Content.ReadFromJsonAsync<AccountProfileDto>(cancellationToken: cancellationToken);
    if (profile is null)
    {
        return null;
    }

    return new BrowserSessionDto
    {
        IsAuthenticated = true,
        Token = token,
        UserId = profile.Id,
        UserName = profile.UserName,
        Role = profile.Role
    };
}

file sealed class JsonObjectEnvelope
{
    public string? Message { get; set; }
}
