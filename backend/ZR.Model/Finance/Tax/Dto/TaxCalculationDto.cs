using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model.Finance.Enums;

namespace ZR.Model.Finance.Tax.Dto
{
    #region Calculation Request DTOs

    /// <summary>
    /// Tax Calculation Request DTO
    /// Used for both preview and actual calculation
    /// </summary>
    public class TaxCalculationRequestDto
    {
        /// <summary>
        /// Company ID for tenant isolation
        /// </summary>
        [Required]
        public long CompanyId { get; set; }

        /// <summary>
        /// Currency code (ISO 4217)
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Currency { get; set; }

        /// <summary>
        /// Exchange rate to base currency
        /// If not provided, will be fetched from exchange_rates table
        /// </summary>
        public decimal? ExchangeRate { get; set; }

        /// <summary>
        /// Date for tax rate lookup and exchange rate
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Are prices tax-inclusive?
        /// true = prices include tax (back-calculate tax)
        /// false = prices exclude tax (add tax on top)
        /// </summary>
        public bool TaxInclusive { get; set; } = false;

        /// <summary>
        /// Rounding mode
        /// </summary>
        public TaxRoundingMode RoundingMode { get; set; } = TaxRoundingMode.RoundPerLine;

        /// <summary>
        /// Number of decimal places for rounding
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// Line items to calculate tax for
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one line item is required")]
        public List<TaxCalculationLineDto> Lines { get; set; } = new List<TaxCalculationLineDto>();

        /// <summary>
        /// Source module (AR, AP, MANUAL, etc.)
        /// </summary>
        [StringLength(50)]
        public string SourceModule { get; set; }

        /// <summary>
        /// Source document ID (invoice, journal, etc.)
        /// </summary>
        public Guid? SourceDocumentId { get; set; }

        /// <summary>
        /// Source document number
        /// </summary>
        [StringLength(100)]
        public string SourceDocumentNumber { get; set; }
    }

    /// <summary>
    /// Tax Calculation Line DTO
    /// Individual line item for tax calculation
    /// </summary>
    public class TaxCalculationLineDto
    {
        /// <summary>
        /// Line identifier (for matching results)
        /// </summary>
        [Required]
        public string LineId { get; set; }

        /// <summary>
        /// GL Account ID for the line
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Amount in foreign currency (before tax if exclusive, total if inclusive)
        /// </summary>
        [Required]
        public decimal AmountForeign { get; set; }

        /// <summary>
        /// Tax code (e.g., "VAT5", "EXEMPT")
        /// Can be a single tax code or a tax group code
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TaxCode { get; set; }

        /// <summary>
        /// Override tax rate (optional, uses effective dated rate if not provided)
        /// </summary>
        public decimal? OverrideRate { get; set; }

        /// <summary>
        /// Line description (for audit)
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Quantity (for unit price calculations)
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// Unit price (for reference)
        /// </summary>
        public decimal? UnitPrice { get; set; }
    }

    #endregion

    #region Calculation Result DTOs

    /// <summary>
    /// Tax Calculation Result DTO
    /// Complete result of tax calculation
    /// </summary>
    public class TaxCalculationResultDto
    {
        /// <summary>
        /// Was calculation successful?
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error messages if failed
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Warning messages (calculation succeeded but with warnings)
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Input parameters echo
        /// </summary>
        public TaxCalculationInputEchoDto Input { get; set; }

        /// <summary>
        /// Calculated line results
        /// </summary>
        public List<TaxCalculationLineResultDto> LineResults { get; set; } = new List<TaxCalculationLineResultDto>();

        /// <summary>
        /// Summary by tax code
        /// </summary>
        public List<TaxSummaryByCodeDto> TaxSummaryByCode { get; set; } = new List<TaxSummaryByCodeDto>();

        /// <summary>
        /// Overall totals
        /// </summary>
        public TaxCalculationTotalsDto Totals { get; set; }

        /// <summary>
        /// Exchange rate used
        /// </summary>
        public decimal ExchangeRateUsed { get; set; }

        /// <summary>
        /// Rounding mode used
        /// </summary>
        public TaxRoundingMode RoundingModeUsed { get; set; }

        /// <summary>
        /// Decimal places used
        /// </summary>
        public int DecimalPlacesUsed { get; set; }

        /// <summary>
        /// Calculation timestamp
        /// </summary>
        public DateTime CalculatedAt { get; set; }

        /// <summary>
        /// Calculation hash (for verification)
        /// </summary>
        public string CalculationHash { get; set; }
    }

    /// <summary>
    /// Echo of input parameters (for audit/verification)
    /// </summary>
    public class TaxCalculationInputEchoDto
    {
        public long CompanyId { get; set; }
        public string Currency { get; set; }
        public DateTime Date { get; set; }
        public bool TaxInclusive { get; set; }
        public TaxRoundingMode RoundingMode { get; set; }
        public int DecimalPlaces { get; set; }
        public int LineCount { get; set; }
    }

    /// <summary>
    /// Tax Calculation Line Result DTO
    /// Result for individual line item
    /// </summary>
    public class TaxCalculationLineResultDto
    {
        /// <summary>
        /// Original line ID (for matching)
        /// </summary>
        public string LineId { get; set; }

        /// <summary>
        /// GL Account ID
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Tax code applied
        /// </summary>
        public string TaxCode { get; set; }

        /// <summary>
        /// Tax code ID
        /// </summary>
        public Guid TaxCodeId { get; set; }

        /// <summary>
        /// Tax rate applied (percentage)
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Was tax inclusive?
        /// </summary>
        public bool IsTaxInclusive { get; set; }

        #region Foreign Currency Amounts

        /// <summary>
        /// Original amount in foreign currency
        /// </summary>
        public decimal OriginalAmountForeign { get; set; }

        /// <summary>
        /// Taxable amount in foreign currency (net of tax if inclusive)
        /// </summary>
        public decimal TaxableAmountForeign { get; set; }

        /// <summary>
        /// Tax amount in foreign currency
        /// </summary>
        public decimal TaxAmountForeign { get; set; }

        /// <summary>
        /// Total amount in foreign currency (taxable + tax)
        /// </summary>
        public decimal TotalAmountForeign { get; set; }

        #endregion

        #region Base Currency Amounts

        /// <summary>
        /// Taxable amount in base currency
        /// </summary>
        public decimal TaxableAmountBase { get; set; }

        /// <summary>
        /// Tax amount in base currency
        /// </summary>
        public decimal TaxAmountBase { get; set; }

        /// <summary>
        /// Total amount in base currency
        /// </summary>
        public decimal TotalAmountBase { get; set; }

        #endregion

        /// <summary>
        /// Rounding delta (difference due to rounding)
        /// </summary>
        public decimal RoundingDelta { get; set; }

        /// <summary>
        /// Tax GL Account ID (payable or recoverable)
        /// </summary>
        public Guid? TaxAccountId { get; set; }

        /// <summary>
        /// Is recoverable tax (input VAT)?
        /// </summary>
        public bool IsRecoverable { get; set; }

        /// <summary>
        /// Is reverse charge?
        /// </summary>
        public bool IsReverseCharge { get; set; }

        /// <summary>
        /// Line-level error messages
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Line-level warnings
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Tax Summary By Code DTO
    /// Aggregated tax amounts per tax code
    /// </summary>
    public class TaxSummaryByCodeDto
    {
        public Guid TaxCodeId { get; set; }
        public string TaxCode { get; set; }
        public string TaxCodeNameEn { get; set; }
        public string TaxCodeNameAr { get; set; }
        public decimal TaxRate { get; set; }
        public int LineCount { get; set; }
        public decimal TaxableAmountForeign { get; set; }
        public decimal TaxAmountForeign { get; set; }
        public decimal TaxableAmountBase { get; set; }
        public decimal TaxAmountBase { get; set; }
        public Guid? TaxAccountId { get; set; }
        public string TaxAccountNumber { get; set; }
        public string TaxAccountName { get; set; }
    }

    /// <summary>
    /// Tax Calculation Totals DTO
    /// Overall calculation totals
    /// </summary>
    public class TaxCalculationTotalsDto
    {
        #region Foreign Currency

        /// <summary>
        /// Total taxable amount (foreign)
        /// </summary>
        public decimal SubtotalForeign { get; set; }

        /// <summary>
        /// Total tax amount (foreign)
        /// </summary>
        public decimal TotalTaxForeign { get; set; }

        /// <summary>
        /// Grand total (foreign)
        /// </summary>
        public decimal GrandTotalForeign { get; set; }

        #endregion

        #region Base Currency

        /// <summary>
        /// Total taxable amount (base)
        /// </summary>
        public decimal SubtotalBase { get; set; }

        /// <summary>
        /// Total tax amount (base)
        /// </summary>
        public decimal TotalTaxBase { get; set; }

        /// <summary>
        /// Grand total (base)
        /// </summary>
        public decimal GrandTotalBase { get; set; }

        #endregion

        /// <summary>
        /// Total rounding delta
        /// </summary>
        public decimal TotalRoundingDelta { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Base currency code
        /// </summary>
        public string BaseCurrency { get; set; }

        /// <summary>
        /// Exchange rate used
        /// </summary>
        public decimal ExchangeRate { get; set; }
    }

    #endregion

    #region Apply to Invoice DTOs

    /// <summary>
    /// Apply Tax to Invoice Request
    /// Used when posting an invoice
    /// </summary>
    public class ApplyTaxToInvoiceRequestDto
    {
        [Required]
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Force recalculation even if already calculated
        /// </summary>
        public bool ForceRecalculate { get; set; } = false;

        /// <summary>
        /// Create GL journal entries?
        /// </summary>
        public bool CreateJournalEntries { get; set; } = true;

        /// <summary>
        /// User making the request (for audit)
        /// </summary>
        public string UserName { get; set; }
    }

    /// <summary>
    /// Apply Tax to Invoice Response
    /// </summary>
    public class ApplyTaxToInvoiceResponseDto
    {
        public bool Success { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid? JournalId { get; set; }
        public string JournalNumber { get; set; }
        public List<TaxCalculationLineResultDto> TaxLines { get; set; } = new List<TaxCalculationLineResultDto>();
        public TaxCalculationTotalsDto Totals { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    #endregion

    #region Tax Journal Line DTOs

    /// <summary>
    /// Tax Journal Line Query DTO
    /// </summary>
    public class TaxJournalLineQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public long? CompanyId { get; set; }
        public Guid? JournalId { get; set; }
        public Guid? InvoiceId { get; set; }
        public string SourceModule { get; set; }
        public Guid? TaxCodeId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsPosted { get; set; }
        public bool? IsRecoverable { get; set; }
    }

    /// <summary>
    /// Tax Journal Line Response DTO
    /// </summary>
    public class TaxJournalLineResponseDto
    {
        public Guid Id { get; set; }
        public long? CompanyId { get; set; }
        public Guid? JournalId { get; set; }
        public string JournalNumber { get; set; }
        public Guid? InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string SourceModule { get; set; }
        public string SourceDocumentNumber { get; set; }
        public Guid TaxCodeId { get; set; }
        public string TaxCode { get; set; }
        public string TaxCodeNameEn { get; set; }
        public string TaxCodeNameAr { get; set; }
        public decimal TaxRate { get; set; }
        public bool IsTaxInclusive { get; set; }
        public decimal TaxableAmountBase { get; set; }
        public decimal? TaxableAmountForeign { get; set; }
        public decimal TaxAmountBase { get; set; }
        public decimal? TaxAmountForeign { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime TaxDate { get; set; }
        public Guid? TaxAccountId { get; set; }
        public string TaxAccountNumber { get; set; }
        public string TaxAccountName { get; set; }
        public bool IsRecoverable { get; set; }
        public bool IsReverseCharge { get; set; }
        public decimal RoundingDelta { get; set; }
        public string RoundingMode { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedAt { get; set; }
        public string PostedBy { get; set; }
        public bool IsManualOverride { get; set; }
        public string OverrideReason { get; set; }
        public string Notes { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateBy { get; set; }
    }

    #endregion

    #region Validation DTOs

    /// <summary>
    /// Tax Validation Error
    /// </summary>
    public class TaxValidationError
    {
        public string Code { get; set; }
        public string MessageEn { get; set; }
        public string MessageAr { get; set; }
        public string Field { get; set; }
        public string LineId { get; set; }
        public object AdditionalData { get; set; }
    }

    /// <summary>
    /// Tax Validation Result
    /// </summary>
    public class TaxValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<TaxValidationError> Errors { get; set; } = new List<TaxValidationError>();
        public List<TaxValidationError> Warnings { get; set; } = new List<TaxValidationError>();
    }

    #endregion
}
