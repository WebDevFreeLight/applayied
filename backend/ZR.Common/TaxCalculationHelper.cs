using System;

namespace ZR.Common
{
    /// <summary>
    /// Tax Calculation Helper - مساعد حساب الضريبة
    /// Provides standardized tax calculations for tax-inclusive and tax-exclusive invoices
    /// يوفر حسابات ضريبية موحدة للفواتير الشاملة والمستثناة من الضريبة
    /// </summary>
    public static class TaxCalculationHelper
    {
        /// <summary>
        /// Calculate tax amounts based on tax-inclusive or tax-exclusive mode
        /// حساب مبالغ الضريبة بناءً على وضع شامل أو مستثنى من الضريبة
        /// </summary>
        /// <param name="amount">The amount (inclusive or exclusive based on isTaxInclusive)</param>
        /// <param name="taxRate">The tax rate as a percentage (e.g., 15 for 15%)</param>
        /// <param name="isTaxInclusive">True if amount includes tax, False if tax is added on top</param>
        /// <returns>Tuple of (TaxableAmount, TaxAmount, TotalAmount)</returns>
        /// <example>
        /// Tax-Inclusive: CalculateTax(115, 15, true) => (100, 15, 115)
        /// Tax-Exclusive: CalculateTax(100, 15, false) => (100, 15, 115)
        /// </example>
        public static (decimal TaxableAmount, decimal TaxAmount, decimal TotalAmount) CalculateTax(
            decimal amount, decimal taxRate, bool isTaxInclusive)
        {
            if (taxRate < 0)
                throw new ArgumentException("Tax rate cannot be negative", nameof(taxRate));

            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            decimal taxRateDecimal = taxRate / 100m;

            if (isTaxInclusive)
            {
                // Tax-inclusive: amount already includes tax
                // taxable_amount = total_amount / (1 + tax_rate)
                var totalAmount = amount;
                var taxableAmount = Math.Round(totalAmount / (1 + taxRateDecimal), 4, MidpointRounding.AwayFromZero);
                var taxAmount = Math.Round(totalAmount - taxableAmount, 4, MidpointRounding.AwayFromZero);
                return (taxableAmount, taxAmount, totalAmount);
            }
            else
            {
                // Tax-exclusive: tax is added on top
                // tax_amount = base_amount * tax_rate
                var taxableAmount = amount;
                var taxAmount = Math.Round(taxableAmount * taxRateDecimal, 4, MidpointRounding.AwayFromZero);
                var totalAmount = taxableAmount + taxAmount;
                return (taxableAmount, taxAmount, totalAmount);
            }
        }

        /// <summary>
        /// Calculate tax amount only (useful when you just need the tax portion)
        /// حساب مبلغ الضريبة فقط
        /// </summary>
        /// <param name="amount">The base/taxable amount</param>
        /// <param name="taxRate">The tax rate as a percentage</param>
        /// <returns>The tax amount</returns>
        public static decimal CalculateTaxAmount(decimal amount, decimal taxRate)
        {
            if (taxRate < 0 || amount < 0)
                return 0;

            return Math.Round(amount * (taxRate / 100m), 4, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Extract taxable amount from a tax-inclusive amount
        /// استخراج المبلغ الخاضع للضريبة من مبلغ شامل الضريبة
        /// </summary>
        /// <param name="taxInclusiveAmount">The amount that includes tax</param>
        /// <param name="taxRate">The tax rate as a percentage</param>
        /// <returns>The taxable (pre-tax) amount</returns>
        /// <example>
        /// ExtractTaxableAmount(115, 15) => 100
        /// </example>
        public static decimal ExtractTaxableAmount(decimal taxInclusiveAmount, decimal taxRate)
        {
            if (taxRate < 0 || taxInclusiveAmount < 0)
                return taxInclusiveAmount;

            decimal taxRateDecimal = taxRate / 100m;
            return Math.Round(taxInclusiveAmount / (1 + taxRateDecimal), 4, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Extract tax amount from a tax-inclusive amount
        /// استخراج مبلغ الضريبة من مبلغ شامل الضريبة
        /// </summary>
        /// <param name="taxInclusiveAmount">The amount that includes tax</param>
        /// <param name="taxRate">The tax rate as a percentage</param>
        /// <returns>The tax amount</returns>
        /// <example>
        /// ExtractTaxAmount(115, 15) => 15
        /// </example>
        public static decimal ExtractTaxAmount(decimal taxInclusiveAmount, decimal taxRate)
        {
            if (taxRate < 0 || taxInclusiveAmount < 0)
                return 0;

            var taxableAmount = ExtractTaxableAmount(taxInclusiveAmount, taxRate);
            return Math.Round(taxInclusiveAmount - taxableAmount, 4, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculate line total with tax
        /// حساب إجمالي السطر مع الضريبة
        /// </summary>
        /// <param name="quantity">Quantity of items</param>
        /// <param name="unitPrice">Unit price (tax-inclusive or exclusive based on isTaxInclusive)</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        /// <param name="isTaxInclusive">Whether unit price includes tax</param>
        /// <returns>Tuple of (LineAmount, TaxableAmount, TaxAmount, TotalAmount)</returns>
        public static (decimal LineAmount, decimal TaxableAmount, decimal TaxAmount, decimal TotalAmount) CalculateLineTotal(
            decimal quantity, decimal unitPrice, decimal taxRate, bool isTaxInclusive)
        {
            var lineAmount = Math.Round(quantity * unitPrice, 4, MidpointRounding.AwayFromZero);
            var (taxableAmount, taxAmount, totalAmount) = CalculateTax(lineAmount, taxRate, isTaxInclusive);
            return (lineAmount, taxableAmount, taxAmount, totalAmount);
        }

        /// <summary>
        /// Validate tax calculation (ensure tax amounts are consistent)
        /// التحقق من صحة حساب الضريبة
        /// </summary>
        /// <param name="taxableAmount">The base amount before tax</param>
        /// <param name="taxAmount">The calculated tax amount</param>
        /// <param name="totalAmount">The total including tax</param>
        /// <param name="tolerance">Acceptable rounding difference (default 0.01)</param>
        /// <returns>True if the amounts are consistent</returns>
        public static bool ValidateTaxCalculation(decimal taxableAmount, decimal taxAmount, decimal totalAmount, decimal tolerance = 0.01m)
        {
            var expectedTotal = taxableAmount + taxAmount;
            return Math.Abs(expectedTotal - totalAmount) <= tolerance;
        }

        /// <summary>
        /// Calculate effective tax rate from amounts
        /// حساب معدل الضريبة الفعلي من المبالغ
        /// </summary>
        /// <param name="taxableAmount">The base amount</param>
        /// <param name="taxAmount">The tax amount</param>
        /// <returns>The effective tax rate as a percentage</returns>
        public static decimal CalculateEffectiveTaxRate(decimal taxableAmount, decimal taxAmount)
        {
            if (taxableAmount == 0)
                return 0;

            return Math.Round((taxAmount / taxableAmount) * 100m, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Saudi Arabia VAT rate constant
        /// ثابت معدل ضريبة القيمة المضافة في المملكة العربية السعودية
        /// </summary>
        public const decimal SaudiVATRate = 15m;

        /// <summary>
        /// Zero tax rate for exempted items
        /// معدل الضريبة الصفري للسلع المعفاة
        /// </summary>
        public const decimal ZeroRate = 0m;
    }
}
