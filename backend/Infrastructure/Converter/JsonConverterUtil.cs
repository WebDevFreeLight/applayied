using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Converter
{
    public class JsonConverterUtil
    {
        // Saudi Arabia timezone offset: UTC+3 = 180 minutes
        private static readonly TimeSpan SaudiOffset = TimeSpan.FromHours(3);

        public class DateTimeNullConverter : JsonConverter<DateTime?>
        {
            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => string.IsNullOrEmpty(reader.GetString()) ? default : ParseDateTime(reader.GetString());

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
                => writer.WriteStringValue(value?.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public class DateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var dateTime = ParseDateTime(reader.GetString());
                return dateTime == null ? DateTime.MinValue : dateTime.Value;
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Parse datetime string preserving the original time regardless of server timezone.
        /// When datetime has timezone offset (e.g., +03:00), extract the LOCAL time from that timezone.
        /// This ensures 15:00+03:00 is stored as 15:00, not converted to server time.
        /// </summary>
        public static DateTime? ParseDateTime(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return null;

            // Check if it's a valid date format
            if (!System.Text.RegularExpressions.Regex.IsMatch(dateStr, @"^\d{4}[/-]"))
                return null;

            // Try parsing with DateTimeOffset first to handle timezone-aware strings
            // This preserves the LOCAL time from the original timezone
            if (DateTimeOffset.TryParse(dateStr, null, System.Globalization.DateTimeStyles.None, out var dateTimeOffset))
            {
                // Return the DateTime component (the local time as specified)
                // This is the key fix: we take the DateTime part which represents
                // the time in the ORIGINAL timezone, ignoring server timezone conversion
                return dateTimeOffset.DateTime;
            }

            // Fallback: parse as regular DateTime
            if (DateTime.TryParse(dateStr, null, System.Globalization.DateTimeStyles.None, out var dateVal))
                return dateVal;

            return null;
        }
    }
}

