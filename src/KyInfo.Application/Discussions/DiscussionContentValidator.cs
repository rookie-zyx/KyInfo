using System.Text;
using System.Text.RegularExpressions;

namespace KyInfo.Application.Discussions;

public static partial class DiscussionContentValidator
{
    private static readonly Regex StickerTokenRegex = StickerRegex();

    public static string NormalizeAndValidate(string content, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("内容不能为空。");
        }

        var trimmed = content.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"内容不能超过 {maxLength} 个字符。");
        }

        if (trimmed.Contains('<') || trimmed.Contains('>'))
        {
            throw new ArgumentException("内容不能包含 HTML 标签。");
        }

        ValidateStickerTokens(trimmed);

        return trimmed;
    }

    private static void ValidateStickerTokens(string content)
    {
        foreach (Match match in StickerTokenRegex.Matches(content))
        {
            var code = match.Groups["code"].Value;
            if (!StickerCatalog.IsValidCode(code))
            {
                throw new ArgumentException($"不支持的表情：{code}");
            }
        }
    }

    public static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("标题不能为空。");
        }

        var trimmed = title.Trim();
        if (trimmed.Length > DiscussionContentLimits.MaxTitleLength)
        {
            throw new ArgumentException($"标题不能超过 {DiscussionContentLimits.MaxTitleLength} 个字符。");
        }

        if (trimmed.Contains('<') || trimmed.Contains('>'))
        {
            throw new ArgumentException("标题不能包含 HTML 标签。");
        }

        return trimmed;
    }

    public static string StripStickerTokens(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var stripped = StickerTokenRegex.Replace(content, " ");
        stripped = CollapseWhitespaceRegex().Replace(stripped, " ").Trim();
        return stripped;
    }

    public static string BuildPreview(string content, int maxLen = 120)
    {
        var normalized = StripStickerTokens(content);
        if (string.IsNullOrEmpty(normalized) || normalized.Length <= maxLen)
        {
            return normalized;
        }

        return normalized[..maxLen] + "…";
    }

    [GeneratedRegex(@"\[s:(?<code>[a-zA-Z0-9_]+)\]", RegexOptions.CultureInvariant)]
    private static partial Regex StickerRegex();

    [GeneratedRegex(@"\s{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex CollapseWhitespaceRegex();
}
