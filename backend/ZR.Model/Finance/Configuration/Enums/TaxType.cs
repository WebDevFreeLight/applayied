namespace ZR.Model.Finance.Configuration.Enums
{
    /// <summary>
    /// Tax Type - نوع الضريبة
    /// </summary>
    public enum TaxType
    {
        /// <summary>
        /// VAT - ضريبة القيمة المضافة
        /// </summary>
        VAT = 0,

        /// <summary>
        /// Withholding Tax - ضريبة الاستقطاع
        /// </summary>
        Withholding = 1,

        /// <summary>
        /// Sales Tax - ضريبة المبيعات
        /// </summary>
        SalesTax = 2,

        /// <summary>
        /// Income Tax - ضريبة الدخل
        /// </summary>
        IncomeTax = 3,

        /// <summary>
        /// Other - أخرى
        /// </summary>
        Other = 4
    }
}


