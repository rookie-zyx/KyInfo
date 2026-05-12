using System.Net.Http.Headers;
using KyInfo.Blazor.Auth;

namespace KyInfo.Blazor.ApiClient;

public sealed class TokenDelegatingHandler : DelegatingHandler
{
    public const string AuthCookieName = "kyinfo_auth";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenStore _tokenStore;

    public TokenDelegatingHandler(IHttpContextAccessor httpContextAccessor, TokenStore tokenStore)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = ResolveToken();
        if (!string.IsNullOrWhiteSpace(token) &&
            (request.Headers.Authorization is null || string.IsNullOrWhiteSpace(request.Headers.Authorization.Parameter)))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (request.Method == HttpMethod.Get &&
            response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var retryToken = ResolveToken();
            if (!string.IsNullOrWhiteSpace(retryToken))
            {
                response.Dispose();
                using var retryRequest = await CloneRequestAsync(request, cancellationToken);
                retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", retryToken);
                response = await base.SendAsync(retryRequest, cancellationToken);
            }
        }

        return response;
    }

    private string? ResolveToken()
    {
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            return _tokenStore.Token;
        }

        var token = _httpContextAccessor.HttpContext?.Request.Cookies[AuthCookieName];
        if (!string.IsNullOrWhiteSpace(token))
        {
            _tokenStore.Token = token;
        }

        return token;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var buffer = new MemoryStream();
            await request.Content.CopyToAsync(buffer, cancellationToken);
            buffer.Position = 0;

            var contentClone = new StreamContent(buffer);
            foreach (var header in request.Content.Headers)
            {
                contentClone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = contentClone;
        }

        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        return clone;
    }
}
