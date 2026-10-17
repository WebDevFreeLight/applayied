namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// Tax Rounding Mode - نمط تقريب الضريبة
    /// Determines how tax amounts are rounded during calculation
    /// </summary>
    public enum TaxRoundingMode
    {
        /// <summary>
        /// Round per line - round tax amount for each line item
        /// تقريب لكل سطر - تقريب مبلغ الضريبة لكل بند
        /// </summary>
        RoundPerLine = 0,

        /// <summary>
        /// Round per invoice - sum all tax amounts then round once
        /// تقريب لكل فاتورة - جمع كل مبالغ الضريبة ثم تقريب مرة واحدة
        /// </summary>
        RoundPerInvoice = 1
    }

    /// <summary>
    /// Tax Date Source - مصدر تاريخ الضريبة
    /// Determines which date to use for tax rate lookup and exchange rate
    /// </summary>
    public enum TaxDateSource
    {
        /// <summary>
        /// Invoice date - use the invoice/document date
        /// تاريخ الفاتورة
        /// </summary>
        InvoiceDate = 0,

        /// <summary>
        /// Payment date - use the payment date (for cash basis)
        /// تاريخ الدفع
        /// </summary>
        PaymentDate = 1,

        /// <summary>
        /// Posting date - use the GL posting date
        /// تاريخ الترحيل
        /// </summary>
        PostingDate = 2
    }

    /// <summary>
    /// Tax Type - نوع الضريبة
    /// </summary>
    public enum TaxType
    {
        /// <summary>
        /// Standard VAT/Sales Tax
        /// ضريبة القيمة المضافة القياسية
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Zero-rated (rate = 0, but still reportable)
        /// صفرية التصنيف
        /// </summary>
        ZeroRated = 1,

        /// <summary>
        /// Exempt (no tax applies)
        /// معفاة
        /// </summary>
        Exempt = 2,

        /// <summary>
        /// Reverse charge (buyer accounts for tax)
        /// الاحتساب العكسي
        /// </summary>
        ReverseCharge = 3,

        /// <summary>
        /// Withholding tax
        /// ضريبة الاستقطاع
        /// </summary>
        Withholding = 4
    }

    /// <summary>
    /// Tax Calculation Method
    /// </summary>
    public enum TaxCalculationMethod
    {
        /// <summary>
        /// Tax exclusive - tax is added on top of line amount
        /// حصري - الضريبة تُضاف على المبلغ
        /// </summary>
        Exclusive = 0,

        /// <summary>
        /// Tax inclusive - line amount includes tax
        /// شامل - المبلغ يشمل الضريبة
        /// </summary>
        Inclusive = 1
    }
}
