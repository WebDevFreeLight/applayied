using System;
using System.Threading.Tasks;
using ZR.Model.Finance.Tax.Dto;

namespace ZR.Service.IService.Finance.Tax
{
    /// <summary>
    /// Tax Calculation Service Interface
    /// The core tax calculation engine
    /// واجهة خدمة حساب الضرائب - محرك الحساب الأساسي
    /// </summary>
    public interface ITaxCalculationService
    {
        #region Calculation Operations

        /// <summary>
        /// Calculate tax for a request (preview or actual)
        /// This is the MAIN calculation method
        /// </summary>
        /// <param name="request">Tax calculation request with lines</param>
        /// <returns>Complete tax calculation result</returns>
        TaxCalculationResultDto Calculate(TaxCalculationRequestDto request);

        /// <summary>
        /// Calculate tax for a request (async)
        /// </summary>
        Task<TaxCalculationResultDto> CalculateAsync(TaxCalculationRequestDto request);

        /// <summary>
        /// Calculate tax for a single line item
        /// </summary>
        TaxCalculationLineResultDto CalculateLine(TaxCalculationLineDto line, TaxCalculationRequestDto context);

        /// <summary>
        /// Calculate tax for a single line item (async)
        /// </summary>
        Task<TaxCalculationLineResultDto> CalculateLineAsync(TaxCalculationLineDto line, TaxCalculationRequestDto context);

        #endregion

        #region Tax-Inclusive Calculations

        /// <summary>
        /// Extract tax from a tax-inclusive amount
        /// Formula: tax = amount - (amount / (1 + rate/100))
        /// </summary>
        /// <param name="inclusiveAmount">Amount including tax</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        /// <param name="decimalPlaces">Decimal places for rounding</param>
        /// <returns>Tax amount</returns>
        decimal ExtractTaxFromInclusive(decimal inclusiveAmount, decimal taxRate, int decimalPlaces = 2);

        /// <summary>
        /// Get taxable amount from tax-inclusive amount
        /// Formula: taxable = amount / (1 + rate/100)
        /// </summary>
        decimal GetTaxableFromInclusive(decimal inclusiveAmount, decimal taxRate, int decimalPlaces = 2);

        #endregion

        #region Tax-Exclusive Calculations

        /// <summary>
        /// Calculate tax on a tax-exclusive amount
        /// Formula: tax = amount * (rate/100)
        /// </summary>
        /// <param name="exclusiveAmount">Amount excluding tax</param>
        /// <param name="taxRate">Tax rate as percentage</param>
        /// <param name="decimalPlaces">Decimal places for rounding</param>
        /// <returns>Tax amount</returns>
        decimal CalculateTaxOnExclusive(decimal exclusiveAmount, decimal taxRate, int decimalPlaces = 2);

        /// <summary>
        /// Get total from tax-exclusive amount
        /// Formula: total = amount * (1 + rate/100)
        /// </summary>
        decimal GetTotalFromExclusive(decimal exclusiveAmount, decimal taxRate, int decimalPlaces = 2);

        #endregion

        #region Rounding Operations

        /// <summary>
        /// Apply rounding based on mode
        /// </summary>
        /// <param name="amount">Amount to round</param>
        /// <param name="decimalPlaces">Decimal places</param>
        /// <returns>Rounded amount</returns>
        decimal Round(decimal amount, int decimalPlaces);

        /// <summary>
        /// Calculate rounding delta
        /// </summary>
        decimal CalculateRoundingDelta(decimal original, decimal rounded);

        #endregion

        #region Exchange Rate Operations

        /// <summary>
        /// Convert amount to base currency
        /// </summary>
        /// <param name="foreignAmount">Amount in foreign currency</param>
        /// <param name="exchangeRate">Exchange rate (foreign to base)</param>
        /// <param name="decimalPlaces">Decimal places for rounding</param>
        /// <returns>Amount in base currency</returns>
        decimal ConvertToBase(decimal foreignAmount, decimal exchangeRate, int decimalPlaces = 2);

        /// <summary>
        /// Get exchange rate for currency on date
        /// </summary>
        Task<decimal> GetExchangeRateAsync(string currency, DateTime date, long companyId);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Validate calculation request
        /// </summary>
        TaxValidationResult ValidateRequest(TaxCalculationRequestDto request);

        /// <summary>
        /// Validate calculation request (async)
        /// </summary>
        Task<TaxValidationResult> ValidateRequestAsync(TaxCalculationRequestDto request);

        #endregion

        #region Apply to Documents

        /// <summary>
        /// Apply tax calculation to an invoice
        /// Creates tax journal lines and optionally GL entries
        /// </summary>
        ApplyTaxToInvoiceResponseDto ApplyToInvoice(ApplyTaxToInvoiceRequestDto request);

        /// <summary>
        /// Apply tax calculation to an invoice (async)
        /// </summary>
        Task<ApplyTaxToInvoiceResponseDto> ApplyToInvoiceAsync(ApplyTaxToInvoiceRequestDto request);

        #endregion

        #region Hash/Verification

        /// <summary>
        /// Generate calculation hash for audit
        /// </summary>
        string GenerateCalculationHash(TaxCalculationResultDto result);

        /// <summary>
        /// Verify calculation hash
        /// </summary>
        bool VerifyCalculationHash(TaxCalculationResultDto result, string hash);

        #endregion
    }
}
