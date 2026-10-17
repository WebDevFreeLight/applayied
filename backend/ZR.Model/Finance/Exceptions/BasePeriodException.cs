using System;

namespace ZR.Model.Finance.Exceptions
{
    /// <summary>
    /// Base exception for period-related errors
    /// الفئة الأساسية لاستثناءات الفترات المالية
    /// </summary>
    public abstract class BasePeriodException : Exception
    {
        public abstract string ErrorCode { get; }
        public string MessageAr { get; protected set; }
        public string MessageEn { get; protected set; }
        public DateTime Date { get; protected set; }

        protected BasePeriodException(string message) : base(message) { }
        protected BasePeriodException(string message, Exception innerException) : base(message, innerException) { }
    }
}
