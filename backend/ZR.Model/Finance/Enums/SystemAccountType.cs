namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// System Account Type - نوع حساب النظام
    /// Predefined account types for IFRS compliant chart of accounts
    /// </summary>
    public enum SystemAccountType
    {
        // Assets - الأصول
        Assets = 1000,
        CurrentAssets = 1100,
        CashAndCashEquivalents = 1110,
        AccountsReceivable = 1120,
        Inventory = 1130,
        PrepaidExpenses = 1140,
        NonCurrentAssets = 1200,
        PropertyPlantEquipment = 1210,
        IntangibleAssets = 1220,
        Investments = 1230,

        // Liabilities - الالتزامات
        Liabilities = 2000,
        CurrentLiabilities = 2100,
        AccountsPayable = 2110,
        AccruedExpenses = 2120,
        ShortTermLoans = 2130,
        NonCurrentLiabilities = 2200,
        LongTermLoans = 2210,
        DeferredTaxLiabilities = 2220,

        // Equity - حقوق الملكية
        Equity = 3000,
        ShareCapital = 3100,
        RetainedEarnings = 3200,
        OtherReserves = 3300,

        // Revenue - الإيرادات
        Revenue = 4000,
        OperatingRevenue = 4100,
        OtherIncome = 4200,

        // Expenses - المصروفات
        Expenses = 5000,
        CostOfGoodsSold = 5100,
        OperatingExpenses = 5200,
        AdministrativeExpenses = 5300,
        FinancialExpenses = 5400
    }
}
