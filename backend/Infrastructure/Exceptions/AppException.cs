using System;
using System.Collections.Generic;

namespace Infrastructure.Exceptions
{
    /// <summary>
    /// Base application exception with support for codes, localization, and severity
    /// </summary>
    public class AppException : Exception
    {
        /// <summary>
        /// Exception code for identification and localization
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Exception severity level
        /// </summary>
        public ExceptionSeverity Severity { get; }

        /// <summary>
        /// Additional details or context (optional)
        /// </summary>
        public object Details { get; set; }

        /// <summary>
        /// HTTP status code to return
        /// </summary>
        public int HttpStatusCode { get; }

        /// <summary>
        /// Additional parameters for message formatting
        /// </summary>
        public Dictionary<string, object> Parameters { get; }

        /// <summary>
        /// Constructor with exception code only
        /// Message will be auto-localized by middleware
        /// </summary>
        public AppException(string code)
            : base(code)
        {
            Code = code;
            Severity = ExceptionMetadata.GetSeverity(code);
            HttpStatusCode = ExceptionMetadata.GetHttpStatusCode(code);
            Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor with exception code and custom message
        /// Use this when you need to override the localized message
        /// </summary>
        public AppException(string code, string message)
            : base(message)
        {
            Code = code;
            Severity = ExceptionMetadata.GetSeverity(code);
            HttpStatusCode = ExceptionMetadata.GetHttpStatusCode(code);
            Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor with exception code, message, and details
        /// </summary>
        public AppException(string code, string message, object details)
            : base(message)
        {
            Code = code;
            Severity = ExceptionMetadata.GetSeverity(code);
            HttpStatusCode = ExceptionMetadata.GetHttpStatusCode(code);
            Details = details;
            Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor with exception code and parameters for message formatting
        /// Example: throw new AppException("INVOICE_NOT_FOUND", new Dictionary&lt;string, object&gt; { { "InvoiceNumber", "INV-001" } });
        /// </summary>
        public AppException(string code, Dictionary<string, object> parameters)
            : base(code)
        {
            Code = code;
            Severity = ExceptionMetadata.GetSeverity(code);
            HttpStatusCode = ExceptionMetadata.GetHttpStatusCode(code);
            Parameters = parameters ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor with exception code, custom severity, and HTTP status
        /// Use this for custom exceptions not registered in metadata
        /// </summary>
        public AppException(string code, ExceptionSeverity severity, int httpStatusCode)
            : base(code)
        {
            Code = code;
            Severity = severity;
            HttpStatusCode = httpStatusCode;
            Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Full constructor with all parameters
        /// </summary>
        public AppException(
            string code,
            string message,
            object details,
            ExceptionSeverity severity,
            int httpStatusCode,
            Exception innerException = null)
            : base(message, innerException)
        {
            Code = code;
            Severity = severity;
            Details = details;
            HttpStatusCode = httpStatusCode;
            Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Add a parameter for message formatting
        /// </summary>
        public AppException WithParameter(string key, object value)
        {
            Parameters[key] = value;
            return this;
        }
    }
}
