namespace ZR.Model.Finance.Zatca.Enums
{
    /// <summary>
    /// ZATCA Invoice Sub Type - النوع الفرعي للفاتورة
    /// KSA-2 field in ZATCA specifications
    /// </summary>
    public enum ZatcaInvoiceSubType
    {
        /// <summary>
        /// Standard Invoice (B2B) - فاتورة قياسية
        /// Binary: 0100000 = 01 (Standard)
        /// </summary>
        StandardInvoice = 1,

        /// <summary>
        /// Simplified Invoice (B2C) - فاتورة مبسطة
        /// Binary: 0200000 = 02 (Simplified)
        /// </summary>
        SimplifiedInvoice = 2,

        /// <summary>
        /// Standard Credit Note - إشعار دائن قياسي
        /// </summary>
        StandardCreditNote = 3,

        /// <summary>
        /// Simplified Credit Note - إشعار دائن مبسط
        /// </summary>
        SimplifiedCreditNote = 4,

        /// <summary>
        /// Standard Debit Note - إشعار مدين قياسي
        /// </summary>
        StandardDebitNote = 5,

        /// <summary>
        /// Simplified Debit Note - إشعار مدين مبسط
        /// </summary>
        SimplifiedDebitNote = 6
    }
}
