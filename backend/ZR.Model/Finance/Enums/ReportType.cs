namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// Financial Report Type - نوع التقرير المالي
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Trial Balance - ميزان المراجعة
        /// </summary>
        TrialBalance = 0,

        /// <summary>
        /// Balance Sheet - الميزانية العمومية
        /// </summary>
        BalanceSheet = 1,

        /// <summary>
        /// Income Statement - قائمة الدخل
        /// </summary>
        IncomeStatement = 2,

        /// <summary>
        /// Cash Flow Statement - قائمة التدفقات النقدية
        /// </summary>
        CashFlow = 3,

        /// <summary>
        /// General Ledger - دفتر الأستاذ العام
        /// </summary>
        GeneralLedger = 4,

        /// <summary>
        /// Account Statement - كشف حساب
        /// </summary>
        AccountStatement = 5
    }
}

