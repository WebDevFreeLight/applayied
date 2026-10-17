namespace ZR.Model.Finance.ARAP.Enums
{
    /// <summary>
    /// Invoice Status - حالة الفاتورة
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>
        /// Draft - مسودة (not posted yet)
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Posted - مرحّل (posted to GL but not paid)
        /// </summary>
        Posted = 1,

        /// <summary>
        /// Partially Paid - مدفوعة جزئياً
        /// </summary>
        PartiallyPaid = 2,

        /// <summary>
        /// Paid - مدفوعة
        /// </summary>
        Paid = 3,

        /// <summary>
        /// Cancelled - ملغية
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Overdue - متأخرة
        /// </summary>
        Overdue = 5 

  
    }
}


