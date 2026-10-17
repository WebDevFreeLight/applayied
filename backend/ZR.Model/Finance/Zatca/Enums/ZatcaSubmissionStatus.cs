namespace ZR.Model.Finance.Zatca.Enums
{
    /// <summary>
    /// ZATCA Submission Status - حالة الإرسال لزاتكا
    /// </summary>
    public enum ZatcaSubmissionStatus
    {
        /// <summary>
        /// Not Submitted - لم يتم الإرسال
        /// Invoice created but not yet submitted to ZATCA
        /// </summary>
        NotSubmitted = 0,

        /// <summary>
        /// Pending - قيد الانتظار
        /// Submitted but awaiting response
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Reported - تم الإبلاغ
        /// Successfully reported (for simplified invoices)
        /// </summary>
        Reported = 2,

        /// <summary>
        /// Reported with Warnings - تم الإبلاغ مع تحذيرات
        /// Reported but with validation warnings
        /// </summary>
        ReportedWithWarnings = 3,

        /// <summary>
        /// Cleared - تمت المقاصة
        /// Successfully cleared (for standard invoices)
        /// </summary>
        Cleared = 4,

        /// <summary>
        /// Cleared with Warnings - تمت المقاصة مع تحذيرات
        /// Cleared but with validation warnings
        /// </summary>
        ClearedWithWarnings = 5,

        /// <summary>
        /// Rejected - مرفوض
        /// Rejected by ZATCA
        /// </summary>
        Rejected = 6,

        /// <summary>
        /// Failed - فشل
        /// Technical failure during submission
        /// </summary>
        Failed = 7,

        /// <summary>
        /// Cancelled - ملغي
        /// Invoice was cancelled in ZATCA
        /// </summary>
        Cancelled = 8
    }
}
