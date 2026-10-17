using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;

namespace ZR.Service.IService.Finance.Configuration
{
    /// <summary>
    /// Exchange Rate Service Interface
    /// خدمة أسعار الصرف
    /// </summary>
    public interface IExchangeRateService
    {
        /// <summary>
        /// Get paginated list of exchange rates
        /// الحصول على قائمة أسعار الصرف
        /// </summary>
        PagedInfo<ExchangeRateResponseDto> GetList(ExchangeRateQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get exchange rate by ID
        /// الحصول على سعر الصرف بالمعرف
        /// </summary>
        ExchangeRateResponseDto GetById(Guid id);

        /// <summary>
        /// Get exchange rate by ID (async)
        /// </summary>
        Task<ExchangeRateResponseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Get exchange rate for specific date
        /// الحصول على سعر الصرف لتاريخ محدد
        /// RULE: Returns rate for exact date or latest rate before that date
        /// </summary>
        ExchangeRateResponseDto GetRateForDate(Guid currencyId, DateTime date);

        /// <summary>
        /// Get exchange rate for specific date (async)
        /// </summary>
        Task<ExchangeRateResponseDto> GetRateForDateAsync(Guid currencyId, DateTime date);

        /// <summary>
        /// Get exchange rate by currency code and date
        /// الحصول على سعر الصرف برمز العملة والتاريخ
        /// </summary>
        ExchangeRateResponseDto GetRateByCodeAndDate(string currencyCode, DateTime date);

        /// <summary>
        /// Get exchange rate by currency code and date (async)
        /// </summary>
        Task<ExchangeRateResponseDto> GetRateByCodeAndDateAsync(string currencyCode, DateTime date);

        /// <summary>
        /// Get latest exchange rate for currency
        /// الحصول على أحدث سعر صرف
        /// </summary>
        ExchangeRateResponseDto GetLatestRate(Guid currencyId);

        /// <summary>
        /// Get latest exchange rate for currency (async)
        /// </summary>
        Task<ExchangeRateResponseDto> GetLatestRateAsync(Guid currencyId);

        /// <summary>
        /// Get all rates for a specific date
        /// الحصول على جميع الأسعار لتاريخ محدد
        /// </summary>
        List<ExchangeRateResponseDto> GetRatesForDate(DateTime date);

        /// <summary>
        /// Get all rates for a specific date (async)
        /// </summary>
        Task<List<ExchangeRateResponseDto>> GetRatesForDateAsync(DateTime date);

        /// <summary>
        /// Get rate history for currency
        /// الحصول على تاريخ الأسعار
        /// </summary>
        List<ExchangeRateResponseDto> GetRateHistory(Guid currencyId, int days = 30);

        /// <summary>
        /// Get rate history for currency (async)
        /// </summary>
        Task<List<ExchangeRateResponseDto>> GetRateHistoryAsync(Guid currencyId, int days = 30);

        /// <summary>
        /// Create exchange rate
        /// إنشاء سعر صرف
        /// VALIDATION RULES:
        /// - Rate must be > 0
        /// - Currency must exist and be active
        /// - Auto-calculate InverseRate = 1 / Rate
        /// - If !AllowManualExchangeRate, IsOverride must be false
        /// </summary>
        ExchangeRateResponseDto Create(CreateExchangeRateDto dto, string createdBy);

        /// <summary>
        /// Create exchange rate (async)
        /// </summary>
        Task<ExchangeRateResponseDto> CreateAsync(CreateExchangeRateDto dto, string createdBy);

        /// <summary>
        /// Bulk import exchange rates
        /// استيراد مجموعة أسعار صرف
        /// VALIDATION RULES:
        /// - All currency codes must exist
        /// - All rates must be > 0
        /// - Auto-calculate inverse rates
        /// </summary>
        List<ExchangeRateResponseDto> BulkImport(BulkImportExchangeRatesDto dto, string createdBy);

        /// <summary>
        /// Bulk import exchange rates (async)
        /// </summary>
        Task<List<ExchangeRateResponseDto>> BulkImportAsync(BulkImportExchangeRatesDto dto, string createdBy);

        /// <summary>
        /// Update exchange rate
        /// تحديث سعر الصرف
        /// VALIDATION RULES:
        /// - Rate must be > 0
        /// - Auto-update InverseRate
        /// - Log the update
        /// </summary>
        int Update(UpdateExchangeRateDto dto, string updatedBy);

        /// <summary>
        /// Update exchange rate (async)
        /// </summary>
        Task<int> UpdateAsync(UpdateExchangeRateDto dto, string updatedBy);

        /// <summary>
        /// Approve exchange rate
        /// الموافقة على سعر الصرف
        /// </summary>
        int Approve(Guid id, string approvedBy);

        /// <summary>
        /// Approve exchange rate (async)
        /// </summary>
        Task<int> ApproveAsync(Guid id, string approvedBy);

        /// <summary>
        /// Delete exchange rate
        /// حذف سعر الصرف
        /// VALIDATION RULES:
        /// - Cannot delete if used in journals
        /// </summary>
        int Delete(Guid id);

        /// <summary>
        /// Delete exchange rate (async)
        /// </summary>
        Task<int> DeleteAsync(Guid id);

        /// <summary>
        /// Validate exchange rate for journal
        /// التحقق من صحة سعر الصرف للقيد
        /// VALIDATION RULES:
        /// - Rate must exist for journal date
        /// - Rate must not be too old (configurable MaxRateAgeDays)
        /// - If RequireRateApproval = true, rate must be approved
        /// </summary>
        void ValidateRateForJournal(Guid currencyId, DateTime journalDate);

        /// <summary>
        /// Validate exchange rate for journal (async)
        /// </summary>
        Task ValidateRateForJournalAsync(Guid currencyId, DateTime journalDate);

        /// <summary>
        /// Calculate base amount from foreign amount
        /// حساب المبلغ بالعملة الأساسية
        /// Formula: BaseAmount = Round(ForeignAmount × ExchangeRate, 2)
        /// </summary>
        decimal CalculateBaseAmount(decimal foreignAmount, decimal exchangeRate);

        /// <summary>
        /// Calculate foreign amount from base amount
        /// حساب المبلغ بالعملة الأجنبية
        /// Formula: ForeignAmount = Round(BaseAmount / ExchangeRate, CurrencyDecimals)
        /// </summary>
        decimal CalculateForeignAmount(decimal baseAmount, decimal exchangeRate, int decimals = 2);

        /// <summary>
        /// Find gaps in exchange rate history
        /// البحث عن فجوات في تاريخ الأسعار
        /// </summary>
        List<DateTime> FindRateGaps(Guid currencyId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Find gaps in exchange rate history (async)
        /// </summary>
        Task<List<DateTime>> FindRateGapsAsync(Guid currencyId, DateTime startDate, DateTime endDate);
    }
}
