using System.Net;
using System.Text.RegularExpressions;

namespace VibeVoice.Application.Common;

/// <summary>Helpers for the rich-text (HTML) bodies coming from the TipTap editor.</summary>
public static partial class HtmlText
{
    [GeneratedRegex(@"<(br|/p|/h[1-6]|/li|/blockquote|/div)\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockEnd();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Tag();

    [GeneratedRegex(@"[ \t]+")]
    private static partial Regex Spaces();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex BlankLines();

    /// <summary>Converts HTML to readable plain text (block elements become line breaks).</summary>
    public static string ToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";
        var text = BlockEnd().Replace(html, "\n");
        text = Tag().Replace(text, "");
        text = WebUtility.HtmlDecode(text);
        text = Spaces().Replace(text, " ");
        text = BlankLines().Replace(text, "\n\n");
        return text.Trim();
    }

    public static string Excerpt(string? html, int maxLength = 200)
    {
        var plain = ToPlainText(html).Replace('\n', ' ');
        if (plain.Length <= maxLength) return plain;
        var cut = plain.LastIndexOf(' ', maxLength);
        return plain[..(cut > maxLength / 2 ? cut : maxLength)].TrimEnd() + "…";
    }

    /// <summary>A short window of <paramref name="text"/> around the first match of <paramref name="term"/>.</summary>
    public static string Snippet(string text, string term, int radius = 70)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var normText = TextNormalizer.Normalize(text);
        var normTerm = TextNormalizer.Normalize(term);
        // Normalization can change length (dropped harakat), so map the match back approximately.
        var idx = normTerm.Length == 0 ? -1 : normText.IndexOf(normTerm, StringComparison.Ordinal);
        if (idx < 0) return Excerpt(text, radius * 2);
        var ratio = (double)text.Length / Math.Max(1, normText.Length);
        var center = (int)(idx * ratio);
        var start = Math.Max(0, center - radius);
        var end = Math.Min(text.Length, center + normTerm.Length + radius);
        return (start > 0 ? "…" : "") + text[start..end].Replace('\n', ' ').Trim() + (end < text.Length ? "…" : "");
    }
}
