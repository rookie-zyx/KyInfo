using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace KyInfo.Blazor.ApiClient;

public static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonReadOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task EnsureSuccessOrThrowAsync(this HttpResponseMessage response, string? fallbackMessage = null)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var status = response.StatusCode;
        var message = await TryReadMessageAsync(response).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = fallbackMessage ?? "请求失败。";
        }

        throw new ApiException(status, message);
    }

    private static async Task<string?> TryReadMessageAsync(HttpResponseMessage response)
    {
        try
        {
            if (response.Content.Headers.ContentType?.MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
            {
                var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonReadOptions).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(payload?.Message))
                {
                    return payload!.Message;
                }
            }

            var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            // 兼容旧接口直接返回字符串（可能是 JSON string）
            if (raw.Length >= 2 && raw[0] == '"' && raw[^1] == '"')
            {
                return raw.Trim().Trim('"');
            }

            return raw.Trim();
        }
        catch
        {
            return null;
        }
    }
}

