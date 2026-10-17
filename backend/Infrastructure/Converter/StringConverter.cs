using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Converter
{
    /// <summary>
    /// Json any type read to string property
    /// Because System.Text.Json must strictly adhere to type consistency, when non-string is read to string property it throws error:
    /// The JSON value could not be converted to System.String.
    /// </summary>
    public class StringConverter : System.Text.Json.Serialization.JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else
            {
                //Non-string type, return raw content
                return GetRawPropertyValue(reader);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
        /// <summary>
        /// Non-string type, return raw content
        /// </summary>
        /// <param name="jsonReader"></param>
        /// <returns></returns>
        private static string GetRawPropertyValue(Utf8JsonReader jsonReader)
        {
            ReadOnlySpan<byte> utf8Bytes = jsonReader.HasValueSequence ?
            jsonReader.ValueSequence.ToArray() :
            jsonReader.ValueSpan;
            return Encoding.UTF8.GetString(utf8Bytes);
        }
    }
}

