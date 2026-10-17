namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// Period Status - حالة الفترة المالية
    /// </summary>
    public enum PeriodStatus
    {
        /// <summary>
        /// Open - مفتوحة
        /// </summary>
        Open = 0,

        /// <summary>
        /// Closed - مغلقة
        /// </summary>
        Closed = 1,

        /// <summary>
        /// Adjusted - معدّلة
        /// </summary>
        Adjusted = 2,

        /// <summary>
        /// Locked - مقفلة نهائياً
        /// </summary>
        Locked = 3
    }

    /// <summary>
    /// Period Type - نوع الفترة المالية
    /// </summary>
    public enum PeriodType
    {
        /// <summary>
        /// Month - شهر
        /// </summary>
        Month = 0,

        /// <summary>
        /// Quarter - ربع سنة
        /// </summary>
        Quarter = 1,

        /// <summary>
        /// Year - سنة
        /// </summary>
        Year = 2
    }
}

