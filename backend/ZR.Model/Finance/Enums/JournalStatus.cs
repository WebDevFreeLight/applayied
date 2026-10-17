namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// Journal Status - حالة القيد اليومي
    /// Replaces VoucherStatus as GLVoucher is removed
    /// </summary>
    public enum JournalStatus
    {
        /// <summary>
        /// Draft - مسودة
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Submitted - مقدم للمراجعة
        /// </summary>
        Submitted = 1,

        /// <summary>
        /// Approved - معتمد
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Posted - مرحّل
        /// </summary>
        Posted = 3,

        /// <summary>
        /// Rejected - مرفوض
        /// </summary>
        Rejected = 4,

        /// <summary>
        /// Cancelled - ملغى
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// Reversed - معكوس
        /// </summary>
        Reversed = 6
    }
}

