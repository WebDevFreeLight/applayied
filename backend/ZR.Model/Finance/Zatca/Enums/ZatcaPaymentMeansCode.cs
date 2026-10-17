namespace ZR.Model.Finance.Zatca.Enums
{
    /// <summary>
    /// ZATCA Payment Means Code - كود وسيلة الدفع
    /// Based on UN/ECE 4461 code list
    /// </summary>
    public enum ZatcaPaymentMeansCode
    {
        /// <summary>
        /// In Cash - نقدي
        /// </summary>
        Cash = 10,

        /// <summary>
        /// Cheque - شيك
        /// </summary>
        Cheque = 20,

        /// <summary>
        /// Credit Transfer - تحويل ائتماني
        /// </summary>
        CreditTransfer = 30,

        /// <summary>
        /// Payment Card - بطاقة دفع
        /// </summary>
        PaymentCard = 48,

        /// <summary>
        /// Direct Debit - خصم مباشر
        /// </summary>
        DirectDebit = 49,

        /// <summary>
        /// Bank Card - بطاقة بنكية
        /// </summary>
        BankCard = 42,

        /// <summary>
        /// Standing Agreement - اتفاقية دائمة
        /// </summary>
        StandingAgreement = 1,

        /// <summary>
        /// Other - أخرى
        /// </summary>
        Other = 97
    }
}
