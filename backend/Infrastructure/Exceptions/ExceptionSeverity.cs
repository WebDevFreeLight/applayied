namespace Infrastructure.Exceptions
{
    /// <summary>
    /// Exception severity levels for logging and alerting
    /// </summary>
    public enum ExceptionSeverity
    {
        /// <summary>
        /// Informational - No action required
        /// </summary>
        Info = 0,

        /// <summary>
        /// Warning - Should be monitored
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Error - Requires attention
        /// </summary>
        Error = 2,

        /// <summary>
        /// Critical - Immediate action required
        /// </summary>
        Critical = 3
    }
}
