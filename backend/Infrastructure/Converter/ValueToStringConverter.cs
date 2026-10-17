using System;
using Newtonsoft.Json;

namespace Infrastructure.Converter
{
    /// <summary>
    /// Converts long / long? values to JSON strings (and back) to avoid JavaScript precision loss.
    ///
    /// IMPORTANT:
    /// SqlSugar already provides a type named <c>ValueToStringConverter</c>. To avoid ambiguity across the
    /// codebase, we intentionally use a different name here.
    /// </summary>
    public sealed class NewtonsoftLongToStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(long) || objectType == typeof(long?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (objectType == typeof(long?)) return null;
                return 0L;
            }

            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                return Convert.ToInt64(reader.Value);
            }

            if (reader.TokenType == JsonToken.String)
            {
                var s = reader.Value?.ToString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    if (objectType == typeof(long?)) return null;
                    return 0L;
                }

                if (long.TryParse(s, out var parsed))
                {
                    return parsed;
                }

                throw new JsonSerializationException($"Unable to convert \"{s}\" to {objectType}.");
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing {objectType}.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            // Ensure large integers are emitted as JSON strings to preserve precision in JS clients.
            writer.WriteValue(value.ToString());
        }
    }
}
