namespace ZR.Model.Finance.Zatca.Enums
{
    /// <summary>
    /// ZATCA Invoice Type - نوع فاتورة زاتكا
    /// Based on UN/CEFACT code list 1001
    /// </summary>
    public enum ZatcaInvoiceType
    {
        /// <summary>
        /// Standard Tax Invoice (B2B) - فاتورة ضريبية قياسية
        /// Code: 388, requires clearance
        /// </summary>
        Standard = 388,

        /// <summary>
        /// Simplified Tax Invoice (B2C) - فاتورة ضريبية مبسطة
        /// Code: 388 with subtype 02, requires reporting only
        /// </summary>
        Simplified = 3880,

        /// <summary>
        /// Credit Note - إشعار دائن
        /// Code: 381
        /// </summary>
        CreditNote = 381,

        /// <summary>
        /// Debit Note - إشعار مدين
        /// Code: 383
        /// </summary>
        DebitNote = 383
    }
}
