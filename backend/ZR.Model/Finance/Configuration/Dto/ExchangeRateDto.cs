using System;

namespace ZR.Model.Finance.Configuration.Dto
{
    /// <summary>
    /// Exchange Rate Query DTO
    /// </summary>
    public class ExchangeRateQueryDto
    {
        public Guid? CurrencyId { get; set; }
        public DateTime? RateDateFrom { get; set; }
        public DateTime? RateDateTo { get; set; }
        public bool? IsOverride { get; set; }
        public bool? IsActive { get; set; }
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Exchange Rate Response DTO
    /// </summary>
    public class ExchangeRateResponseDto
    {
        public Guid Id { get; set; }
        public Guid CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public DateTime RateDate { get; set; }
        public decimal Rate { get; set; }
        public decimal? InverseRate { get; set; }
        public bool IsOverride { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// Create Exchange Rate DTO
    /// </summary>
    public class CreateExchangeRateDto
    {
        /// <summary>
        /// Currency ID
        /// معرف العملة
        /// </summary>
        public Guid CurrencyId { get; set; }

        /// <summary>
        /// Rate Date
        /// تاريخ السعر
        /// </summary>
        public DateTime RateDate { get; set; }

        /// <summary>
        /// Exchange Rate
        /// سعر الصرف
        /// RULE: Must be > 0
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Is Manual Override?
        /// تعديل يدوي؟
        /// </summary>
        public bool IsOverride { get; set; }

        /// <summary>
        /// Source
        /// المصدر
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Notes
        /// ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update Exchange Rate DTO
    /// </summary>
    public class UpdateExchangeRateDto
    {
        public Guid Id { get; set; }
        public decimal Rate { get; set; }
        public bool IsOverride { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Bulk Import Exchange Rates DTO
    /// </summary>
    public class BulkImportExchangeRatesDto
    {
        public DateTime RateDate { get; set; }
        public ExchangeRateImportItem[] Rates { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// Exchange Rate Import Item
    /// </summary>
    public class ExchangeRateImportItem
    {
        public string CurrencyCode { get; set; }
        public decimal Rate { get; set; }
    }

    /// <summary>
    /// Get Exchange Rate for Date DTO
    /// </summary>
    public class GetExchangeRateDto
    {
        public Guid? CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Approve Exchange Rate DTO
    /// </summary>
    public class ApproveExchangeRateDto
    {
        public Guid Id { get; set; }
    }
}
