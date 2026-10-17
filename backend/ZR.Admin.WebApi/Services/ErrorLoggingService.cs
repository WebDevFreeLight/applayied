using System.Collections.Concurrent;
using System.Text.Json;
using NLog;

namespace ZR.Admin.WebApi.Services
{
    /// <summary>
    /// Service for logging and tracking errors
    /// </summary>
    public interface IErrorLoggingService
    {
        Task LogErrorAsync(ErrorLog errorLog);
        Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 100);
        Task<ErrorStatistics> GetErrorStatisticsAsync();
        void ClearErrorLogs();
    }

    public class ErrorLoggingService : IErrorLoggingService
    {
        private readonly ConcurrentQueue<ErrorLog> _errorLogs = new();
        private readonly ILogger<ErrorLoggingService> _logger;
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();
        private readonly int _maxLogs = 1000;

        public ErrorLoggingService(ILogger<ErrorLoggingService> logger)
        {
            _logger = logger;
        }

        public Task LogErrorAsync(ErrorLog errorLog)
        {
            try
            {
                // Add to queue
                _errorLogs.Enqueue(errorLog);
                
                // Maintain max size
                while (_errorLogs.Count > _maxLogs)
                {
                    _errorLogs.TryDequeue(out _);
                }

                // Log to NLog
                var logMessage = $"[{errorLog.RequestId}] {errorLog.Controller}/{errorLog.Action} - {errorLog.ErrorMessage}";
                
                switch (errorLog.Severity)
                {
                    case ErrorSeverity.Critical:
                        nlog.Fatal(logMessage);
                        break;
                    case ErrorSeverity.High:
                        nlog.Error(logMessage);
                        break;
                    case ErrorSeverity.Medium:
                        nlog.Warn(logMessage);
                        break;
                    case ErrorSeverity.Low:
                        nlog.Info(logMessage);
                        break;
                }

                // Log to ILogger
                _logger.LogError($"{logMessage}\nDetails: {JsonSerializer.Serialize(errorLog)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log error");
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 100)
        {
            var errors = _errorLogs.Reverse().Take(count);
            return Task.FromResult(errors);
        }

        public Task<ErrorStatistics> GetErrorStatisticsAsync()
        {
            var logs = _errorLogs.ToList();
            var stats = new ErrorStatistics
            {
                TotalErrors = logs.Count,
                CriticalErrors = logs.Count(e => e.Severity == ErrorSeverity.Critical),
                HighErrors = logs.Count(e => e.Severity == ErrorSeverity.High),
                MediumErrors = logs.Count(e => e.Severity == ErrorSeverity.Medium),
                LowErrors = logs.Count(e => e.Severity == ErrorSeverity.Low),
                ErrorsByController = logs.GroupBy(e => e.Controller)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ErrorsByType = logs.GroupBy(e => e.ErrorType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentErrors = logs.OrderByDescending(e => e.Timestamp).Take(10).ToList()
            };

            return Task.FromResult(stats);
        }

        public void ClearErrorLogs()
        {
            while (_errorLogs.TryDequeue(out _)) { }
        }
    }

    /// <summary>
    /// Error log model
    /// </summary>
    public class ErrorLog
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Controller { get; set; }
        public string Action { get; set; }
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string UserId { get; set; }
        public string UserIp { get; set; }
        public string RequestPath { get; set; }
        public string RequestMethod { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
        public string RequestBody { get; set; }
        public int StatusCode { get; set; }
        public ErrorSeverity Severity { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }
    }

    /// <summary>
    /// Error severity levels
    /// </summary>
    public enum ErrorSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Error statistics model
    /// </summary>
    public class ErrorStatistics
    {
        public int TotalErrors { get; set; }
        public int CriticalErrors { get; set; }
        public int HighErrors { get; set; }
        public int MediumErrors { get; set; }
        public int LowErrors { get; set; }
        public Dictionary<string, int> ErrorsByController { get; set; }
        public Dictionary<string, int> ErrorsByType { get; set; }
        public List<ErrorLog> RecentErrors { get; set; }
    }
}
