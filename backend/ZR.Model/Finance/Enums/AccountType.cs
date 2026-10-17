namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// Account Type - نوع الحساب (IFRS Compliant)
    /// Based on International Financial Reporting Standards (IFRS)
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Asset - أصول
        /// Resources controlled by the entity as a result of past events
        /// IFRS: IAS 1, IFRS 9
        /// </summary>
        Asset = 0,

        /// <summary>
        /// Liability - خصوم / التزامات
        /// Present obligations arising from past events
        /// IFRS: IAS 1, IAS 37
        /// </summary>
        Liability = 1,

        /// <summary>
        /// Equity - حقوق الملكية
        /// Residual interest in the assets after deducting liabilities
        /// IFRS: IAS 1, IAS 32
        /// </summary>
        Equity = 2,

        /// <summary>
        /// Revenue - إيرادات
        /// Increases in economic benefits during the accounting period
        /// IFRS: IFRS 15
        /// </summary>
        Revenue = 3,

        /// <summary>
        /// Expense - مصروفات
        /// Decreases in economic benefits during the accounting period
        /// IFRS: IAS 1
        /// </summary>
        Expense = 4,

        /// <summary>
        /// Tax - ضرائب
        /// Tax-related accounts (VAT, Income Tax, etc.)
        /// </summary>
        Tax = 5,

        /// <summary>
        /// Contra Asset - مقابل الأصول
        /// Accounts that offset asset accounts (e.g., Accumulated Depreciation)
        /// </summary>
        ContraAsset = 6,

        /// <summary>
        /// Contra Liability - مقابل الالتزامات
        /// Accounts that offset liability accounts
        /// </summary>
        ContraLiability = 7,

        /// <summary>
        /// Contra Equity - مقابل حقوق الملكية
        /// Accounts that offset equity accounts (e.g., Treasury Stock)
        /// </summary>
        ContraEquity = 8,

        /// <summary>
        /// Contra Revenue - مقابل الإيرادات
        /// Accounts that offset revenue accounts (e.g., Sales Returns)
        /// </summary>
        ContraRevenue = 9,

        /// <summary>
        /// Other Comprehensive Income - الدخل الشامل الآخر
        /// Items that bypass the income statement (IFRS: IAS 1)
        /// </summary>
        OtherComprehensiveIncome = 10
    }
}
