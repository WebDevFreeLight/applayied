using System;
using System.Text.Json.Serialization;

namespace Infrastructure.Models
{
    /// <summary>
    /// Standardized API response wrapper
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("details")]
        public object Details { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Code = "SUCCESS",
                Message = message ?? "Operation completed successfully",
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string code, string message, object details = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Code = code,
                Message = message,
                Details = details
            };
        }
    }

    /// <summary>
    /// Non-generic API response for operations without data
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
    }
}
