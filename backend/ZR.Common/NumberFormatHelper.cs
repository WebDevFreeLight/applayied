using System;
using System.Globalization;

namespace ZR.Common
{
    /// <summary>
    /// Number Format Helper - مساعد تنسيق الأرقام
    /// Provides standardized number formatting using English digits (0-9)
    /// يوفر تنسيق موحد للأرقام باستخدام الأرقام الإنجليزية
    ///
    /// IMPORTANT: All numeric formatting in the system should use these methods
    /// to ensure consistent display with English digits regardless of user locale.
    /// </summary>
    public static class NumberFormatHelper
    {
        /// <summary>
        /// Invariant culture for consistent number formatting
        /// Always uses: . as decimal separator, , as thousands separator
        /// </summary>
        public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// English (US) culture for number formatting
        /// Uses: . as decimal separator, , as thousands separator
        /// </summary>
        public static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");

        /// <summary>
        /// Standard decimal format for display (2 decimal places)
        /// </summary>
        public const string DecimalFormat = "N2";

        /// <summary>
        /// Standard currency format (2 decimal places)
        /// </summary>
        public const string CurrencyFormat = "C2";

        /// <summary>
        /// Integer format (no decimal places)
        /// </summary>
        public const string IntegerFormat = "N0";

        /// <summary>
        /// Percentage format (2 decimal places)
        /// </summary>
        public const string PercentFormat = "P2";

        /// <summary>
        /// Format a decimal number with 2 decimal places using English digits
        /// تنسيق رقم عشري بخانتين عشريتين باستخدام الأرقام الإنجليزية
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>Formatted string with English digits (e.g., "1,234.56")</returns>
        public static string FormatDecimal(decimal value)
        {
            return value.ToString(DecimalFormat, EnglishCulture);
        }

        /// <summary>
        /// Format a decimal number with custom decimal places using English digits
        /// تنسيق رقم عشري بعدد خانات مخصص باستخدام الأرقام الإنجليزية
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <param name="decimalPlaces">Number of decimal places</param>
        /// <returns>Formatted string with English digits</returns>
        public static string FormatDecimal(decimal value, int decimalPlaces)
        {
            return value.ToString($"N{decimalPlaces}", EnglishCulture);
        }

        /// <summary>
        /// Format a number as currency with English digits
        /// Note: Currency symbol depends on culture but digits will always be English
        /// تنسيق رقم كعملة باستخدام الأرقام الإنجليزية
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <param name="currencyCode">Currency code (e.g., "SAR", "USD")</param>
        /// <returns>Formatted currency string with English digits</returns>
        public static string FormatCurrency(decimal value, string currencyCode = "SAR")
        {
            return $"{currencyCode} {FormatDecimal(value)}";
        }

        /// <summary>
        /// Format a double number with 2 decimal places using English digits
        /// </summary>
        public static string FormatDecimal(double value)
        {
            return value.ToString(DecimalFormat, EnglishCulture);
        }

        /// <summary>
        /// Format an integer with thousands separator using English digits
        /// تنسيق عدد صحيح مع فاصل الآلاف باستخدام الأرقام الإنجليزية
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>Formatted string with English digits (e.g., "1,234")</returns>
        public static string FormatInteger(int value)
        {
            return value.ToString(IntegerFormat, EnglishCulture);
        }

        /// <summary>
        /// Format a long integer with thousands separator using English digits
        /// </summary>
        public static string FormatInteger(long value)
        {
            return value.ToString(IntegerFormat, EnglishCulture);
        }

        /// <summary>
        /// Format a percentage using English digits
        /// تنسيق نسبة مئوية باستخدام الأرقام الإنجليزية
        /// </summary>
        /// <param name="value">The value to format (0.5 = 50%)</param>
        /// <returns>Formatted percentage string (e.g., "50.00%")</returns>
        public static string FormatPercentage(decimal value)
        {
            return value.ToString(PercentFormat, EnglishCulture);
        }

        /// <summary>
        /// Format a percentage with custom decimal places using English digits
        /// </summary>
        public static string FormatPercentage(decimal value, int decimalPlaces)
        {
            return value.ToString($"P{decimalPlaces}", EnglishCulture);
        }

        /// <summary>
        /// Format a number for accounting display (parentheses for negative)
        /// Uses English digits only
        /// تنسيق رقم للعرض المحاسبي (أقواس للسالب)
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>Formatted string with negative values in parentheses</returns>
        public static string FormatAccounting(decimal value)
        {
            if (value < 0)
            {
                return $"({FormatDecimal(Math.Abs(value))})";
            }
            return FormatDecimal(value);
        }

        /// <summary>
        /// Parse a formatted number string back to decimal
        /// Handles various formats including parentheses for negative
        /// تحليل سلسلة رقم منسقة إلى عدد عشري
        /// </summary>
        /// <param name="value">The formatted string to parse</param>
        /// <returns>Parsed decimal value</returns>
        public static decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "-")
                return 0;

            // Handle accounting format (negative in parentheses)
            var isNegative = value.StartsWith("(") && value.EndsWith(")");
            var cleanValue = isNegative ? value[1..^1] : value;

            // Remove currency symbols and thousand separators
            cleanValue = cleanValue
                .Replace(",", "")
                .Replace("SAR", "")
                .Replace("USD", "")
                .Replace("EUR", "")
                .Replace("$", "")
                .Trim();

            if (decimal.TryParse(cleanValue, NumberStyles.Any, EnglishCulture, out var result))
            {
                return isNegative ? -result : result;
            }

            return 0;
        }

        /// <summary>
        /// Try parse a formatted number string to decimal
        /// </summary>
        public static bool TryParseDecimal(string value, out decimal result)
        {
            result = 0;
            try
            {
                result = ParseDecimal(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
