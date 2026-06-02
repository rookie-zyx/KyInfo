using System.Text.RegularExpressions;

namespace KyInfo.Blazor.Components.Discussions;

public static partial class DiscussionContentParser
{
    [GeneratedRegex(@"\[s:(?<code>[a-zA-Z0-9_]+)\]", RegexOptions.CultureInvariant)]
    private static partial Regex StickerRegex();

    public static string ToPlainText(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        var stripped = StickerRegex().Replace(content, " ");
        return CollapseWhitespaceRegex().Replace(stripped, " ").Trim();
    }

    [GeneratedRegex(@"\s{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex CollapseWhitespaceRegex();
}
