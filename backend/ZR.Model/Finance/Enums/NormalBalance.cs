namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// Normal Balance - الطبيعة الطبيعية للحساب
    /// Defines whether an account normally has a debit or credit balance
    /// Based on fundamental accounting principles
    /// </summary>
    public enum NormalBalance
    {
        /// <summary>
        /// Debit - مدين
        /// Account balance increases with debits (Assets, Expenses, Contra Liabilities)
        /// </summary>
        Debit = 0,

        /// <summary>
        /// Credit - دائن
        /// Account balance increases with credits (Liabilities, Equity, Revenue)
        /// </summary>
        Credit = 1
    }
}
