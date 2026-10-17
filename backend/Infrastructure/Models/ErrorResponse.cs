using System;
using System.Text.Json.Serialization;
using Infrastructure.Exceptions;

namespace Infrastructure.Models
{
    /// <summary>
    /// Detailed error response for exceptions
    /// </summary>
    public class ErrorResponse
    {
        [JsonPropertyName("success")]
        public bool Success => false;

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("details")]
        public object Details { get; set; }

        [JsonPropertyName("severity")]
        public string Severity { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        // Only include in development
        [JsonPropertyName("stackTrace")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string StackTrace { get; set; }

        [JsonPropertyName("innerException")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string InnerException { get; set; }

        public static ErrorResponse FromException(
            AppException exception,
            string path,
            string traceId,
            bool isDevelopment = false)
        {
            return new ErrorResponse
            {
                Code = exception.Code,
                Message = exception.Message,
                Details = exception.Details,
                Severity = exception.Severity.ToString(),
                Path = path,
                TraceId = traceId,
                StackTrace = isDevelopment ? exception.StackTrace : null,
                InnerException = isDevelopment ? exception.InnerException?.Message : null
            };
        }

        public static ErrorResponse FromGenericException(
            Exception exception,
            string path,
            string traceId,
            bool isDevelopment = false)
        {
            return new ErrorResponse
            {
                Code = ExceptionCode.INTERNAL_ERROR,
                Message = isDevelopment
                    ? exception.Message
                    : "An internal error occurred. Please contact support.",
                Severity = ExceptionSeverity.Critical.ToString(),
                Path = path,
                TraceId = traceId,
                StackTrace = isDevelopment ? exception.StackTrace : null,
                InnerException = isDevelopment ? exception.InnerException?.Message : null
            };
        }
    }
}
