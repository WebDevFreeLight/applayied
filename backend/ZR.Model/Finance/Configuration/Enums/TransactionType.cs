namespace ZR.Model.Finance.Configuration.Enums
{
    /// <summary>
    /// Transaction Type - نوع المعاملة
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Payment - دفع
        /// </summary>
        Payment = 0,

        /// <summary>
        /// Receipt - إيصال استلام
        /// </summary>
        Receipt = 1,

        /// <summary>
        /// Invoice - فاتورة
        /// </summary>
        Invoice = 2,

        /// <summary>
        /// Journal Entry - قيد محاسبي
        /// </summary>
        JournalEntry = 3,

        /// <summary>
        /// Credit Note - إشعار دائن
        /// </summary>
        CreditNote = 4,

        /// <summary>
        /// Debit Note - إشعار مدين
        /// </summary>
        DebitNote = 5,

        /// <summary>
        /// Adjustment - تعديل
        /// </summary>
        Adjustment = 6,

        /// <summary>
        /// Other - أخرى
        /// </summary>
        Other = 7
    }
}


