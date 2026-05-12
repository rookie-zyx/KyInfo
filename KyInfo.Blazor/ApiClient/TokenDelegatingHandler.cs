using System.Net.Http.Headers;
using Microsoft.JSInterop;
using KyInfo.Blazor.Auth;

namespace KyInfo.Blazor.ApiClient;

public sealed class TokenDelegatingHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private readonly TokenStore _tokenStore;

    public TokenDelegatingHandler(IJSRuntime js, TokenStore tokenStore)
    {
        _js = js;
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Blazor Server 的首次水合阶段可能导致 localStorage 读取偶发失败：
        // - 不带 token 会触发 401
        // - 组件收到 401 又会重定向到 /login，形成循环
        //
        // 另外，某些情况下 HttpRequestMessage 可能已经带了 Authorization 头对象，
        // 但 Parameter 为空或不是我们预期的 token，因此这里以 "缺少 parameter" 为准注入 token。
        var token = _tokenStore.Token;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await TryReadTokenWithRetryAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                _tokenStore.Token = token;
            }
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            var hasValidAuth =
                request.Headers.Authorization is not null &&
                !string.IsNullOrWhiteSpace(request.Headers.Authorization.Parameter);

            if (!hasValidAuth)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        // 对 GET 请求在 401 时仅再尝试一次：刷新 Authorization 后重放请求。
        // 避免影响 POST/DELETE 等写操作（可能造成重复提交）。
        if (request.Method == HttpMethod.Get && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var retryToken = _tokenStore.Token;
            if (string.IsNullOrWhiteSpace(retryToken))
            {
                retryToken = await TryReadTokenWithRetryAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(retryToken))
                {
                    _tokenStore.Token = retryToken;
                }
            }
            if (!string.IsNullOrWhiteSpace(retryToken))
            {
                // GET 401 仅再尝试一次：强制覆盖 Authorization，避免仍携带错误/空 parameter。
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", retryToken);
                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }

    private async Task<string?> TryReadTokenWithRetryAsync(CancellationToken cancellationToken)
    {
        // 小概率 JS 未就绪，简单重试能显著降低首次水合触发 401 的概率
        for (var i = 0; i < 3; i++)
        {
            try
            {
                return await _js.InvokeAsync<string?>("localStorage.getItem", cancellationToken, "kyinfo_token");
            }
            catch
            {
                if (i == 2)
                {
                    return null;
                }

                await Task.Delay(120, cancellationToken);
            }
        }

        return null;
    }
}

