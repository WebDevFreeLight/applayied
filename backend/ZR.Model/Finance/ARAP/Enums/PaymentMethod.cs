namespace ZR.Model.Finance.ARAP.Enums
{
    /// <summary>
    /// Payment Method - طريقة الدفع
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Cash - نقدي
        /// </summary>
        Cash = 0,

        /// <summary>
        /// Bank Transfer - تحويل بنكي
        /// </summary>
        BankTransfer = 1,

        /// <summary>
        /// Cheque - شيك
        /// </summary>
        Cheque = 2,

        /// <summary>
        /// Card - بطاقة
        /// </summary>
        Card = 3,

        /// <summary>
        /// Other - أخرى
        /// </summary>
        Other = 4
    }
}


