using System;

namespace ZR.Common
{
    /// <summary>
    /// Currency Helper - مساعد العملات
    /// Provides standardized rounding for multi-currency calculations
    /// يوفر تقريب موحد لحسابات العملات المتعددة
    /// </summary>
    public static class CurrencyHelper
    {
        /// <summary>
        /// Default decimal places for base currency calculations (4 decimals for exchange rate precision)
        /// عدد الخانات العشرية الافتراضي لحسابات العملة الأساسية
        /// </summary>
        public const int BaseCurrencyDecimals = 4;

        /// <summary>
        /// Default decimal places for display amounts (2 decimals)
        /// عدد الخانات العشرية الافتراضي للمبالغ المعروضة
        /// </summary>
        public const int DisplayDecimals = 2;

        /// <summary>
        /// Convert amount to base currency with standardized rounding
        /// Uses 4 decimal places with MidpointRounding.AwayFromZero
        /// تحويل المبلغ إلى العملة الأساسية مع تقريب موحد
        /// </summary>
        /// <param name="amount">The original amount in foreign currency</param>
        /// <param name="exchangeRate">The exchange rate (foreign to base)</param>
        /// <returns>Amount in base currency, rounded to 4 decimal places</returns>
        /// <example>
        /// // Example: 100.005 USD at rate 3.7852 = 378.5421 (not 378.5419 due to MidpointRounding)
        /// var baseCurrency = CurrencyHelper.ToBaseCurrency(100.005m, 3.7852m);
        /// </example>
        public static decimal ToBaseCurrency(decimal amount, decimal exchangeRate)
        {
            return Math.Round(amount * exchangeRate, BaseCurrencyDecimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Round amount for display (2 decimal places)
        /// Uses MidpointRounding.AwayFromZero for consistent behavior
        /// تقريب المبلغ للعرض (خانتان عشريتان)
        /// </summary>
        /// <param name="amount">The amount to round</param>
        /// <returns>Amount rounded to 2 decimal places</returns>
        public static decimal RoundForDisplay(decimal amount)
        {
            return Math.Round(amount, DisplayDecimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Round amount with custom decimal places
        /// Uses MidpointRounding.AwayFromZero for consistent behavior
        /// تقريب المبلغ بعدد خانات عشرية مخصص
        /// </summary>
        /// <param name="amount">The amount to round</param>
        /// <param name="decimals">Number of decimal places</param>
        /// <returns>Rounded amount</returns>
        public static decimal Round(decimal amount, int decimals)
        {
            return Math.Round(amount, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculate debit amount in base currency
        /// حساب مبلغ المدين بالعملة الأساسية
        /// </summary>
        /// <param name="debit">Debit amount in foreign currency</param>
        /// <param name="exchangeRate">Exchange rate</param>
        /// <returns>Base debit amount, rounded to 4 decimal places</returns>
        public static decimal CalculateBaseDebit(decimal debit, decimal exchangeRate)
        {
            return ToBaseCurrency(debit, exchangeRate);
        }

        /// <summary>
        /// Calculate credit amount in base currency
        /// حساب مبلغ الدائن بالعملة الأساسية
        /// </summary>
        /// <param name="credit">Credit amount in foreign currency</param>
        /// <param name="exchangeRate">Exchange rate</param>
        /// <returns>Base credit amount, rounded to 4 decimal places</returns>
        public static decimal CalculateBaseCredit(decimal credit, decimal exchangeRate)
        {
            return ToBaseCurrency(credit, exchangeRate);
        }

        /// <summary>
        /// Calculate exchange rate difference (gain/loss)
        /// حساب فرق سعر الصرف (ربح/خسارة)
        /// </summary>
        /// <param name="originalAmount">Original amount in base currency</param>
        /// <param name="currentAmount">Current amount in base currency</param>
        /// <returns>Exchange difference (positive = gain, negative = loss)</returns>
        public static decimal CalculateExchangeDifference(decimal originalAmount, decimal currentAmount)
        {
            return Math.Round(currentAmount - originalAmount, BaseCurrencyDecimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Check if two amounts are equal within rounding tolerance
        /// التحقق من تساوي مبلغين ضمن هامش التقريب
        /// </summary>
        /// <param name="amount1">First amount</param>
        /// <param name="amount2">Second amount</param>
        /// <param name="tolerance">Tolerance for comparison (default: 0.0001)</param>
        /// <returns>True if amounts are equal within tolerance</returns>
        public static bool AreEqual(decimal amount1, decimal amount2, decimal tolerance = 0.0001m)
        {
            return Math.Abs(amount1 - amount2) <= tolerance;
        }

        /// <summary>
        /// Check if debit and credit are balanced within rounding tolerance
        /// التحقق من توازن المدين والدائن ضمن هامش التقريب
        /// </summary>
        /// <param name="totalDebit">Total debit amount</param>
        /// <param name="totalCredit">Total credit amount</param>
        /// <param name="tolerance">Tolerance for comparison (default: 0.0001)</param>
        /// <returns>True if balanced</returns>
        public static bool IsBalanced(decimal totalDebit, decimal totalCredit, decimal tolerance = 0.0001m)
        {
            return AreEqual(totalDebit, totalCredit, tolerance);
        }
    }
}
