using System.Globalization;
using System.Text;

namespace VibeVoice.Application.Common;

/// <summary>
/// Folds text into a canonical form so a plain <c>LIKE '%term%'</c> works for English and
/// Arabic on every database provider. The same function runs on the stored text and on the query.
/// </summary>
public static class TextNormalizer
{
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var sb = new StringBuilder(input.Length);
        var lastWasSpace = false;

        foreach (var raw in input.Normalize(NormalizationForm.FormD))
        {
            // Latin accents and Arabic harakat are both non-spacing marks once decomposed.
            if (CharUnicodeInfo.GetUnicodeCategory(raw) == UnicodeCategory.NonSpacingMark) continue;

            var c = FoldArabic(raw);
            if (c == '\0') continue;

            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace && sb.Length > 0) sb.Append(' ');
                lastWasSpace = true;
                continue;
            }

            sb.Append(char.ToLowerInvariant(c));
            lastWasSpace = false;
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Maps Arabic letter variants that users type interchangeably to one form.
    /// Returns <c>'\0'</c> to drop a character entirely.
    /// </summary>
    private static char FoldArabic(char c) => c switch
    {
        'ـ' => '\0',                                   // tatweel (kashida) ـ
        'ٰ' => '\0',                                   // superscript alef
        'أ' or 'إ' or 'آ' or 'ٱ' => 'ا',                    // hamza/madda/wasla alef → bare alef
        'ى' => 'ي',                                         // alef maqsura → yeh
        'ة' => 'ه',                                         // teh marbuta → heh
        'ؤ' => 'و',
        'ئ' => 'ي',
        >= '٠' and <= '٩' => (char)('0' + (c - '٠')), // Arabic-Indic digits ٠-٩
        _ => c,
    };
}
