using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Exchange Rate Repository Interface
    /// مستودع أسعار الصرف
    /// </summary>
    public interface IExchangeRateRepository
    {
        /// <summary>
        /// Get paginated list of exchange rates
        /// الحصول على قائمة أسعار الصرف
        /// </summary>
        PagedInfo<ExchangeRate> GetList(ExchangeRate searchModel, PagerInfo pager);

        /// <summary>
        /// Get exchange rate by ID
        /// الحصول على سعر الصرف بالمعرف
        /// </summary>
        ExchangeRate GetById(Guid id);

        /// <summary>
        /// Get exchange rate by ID (async)
        /// </summary>
        Task<ExchangeRate> GetByIdAsync(Guid id);

        /// <summary>
        /// Get exchange rate for specific currency and date
        /// الحصول على سعر الصرف لعملة وتاريخ محددين
        /// RULE: Returns the rate for exact date or latest rate before that date
        /// </summary>
        ExchangeRate GetRateForDate(Guid currencyId, DateTime date);

        /// <summary>
        /// Get exchange rate for specific currency and date (async)
        /// </summary>
        Task<ExchangeRate> GetRateForDateAsync(Guid currencyId, DateTime date);

        /// <summary>
        /// Get exchange rate by currency code and date
        /// الحصول على سعر الصرف برمز العملة والتاريخ
        /// </summary>
        ExchangeRate GetRateByCodeAndDate(string currencyCode, DateTime date);

        /// <summary>
        /// Get exchange rate by currency code and date (async)
        /// </summary>
        Task<ExchangeRate> GetRateByCodeAndDateAsync(string currencyCode, DateTime date);

        /// <summary>
        /// Get exchange rates for date range
        /// الحصول على أسعار الصرف لفترة زمنية
        /// </summary>
        List<ExchangeRate> GetRatesForDateRange(Guid currencyId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get exchange rates for date range (async)
        /// </summary>
        Task<List<ExchangeRate>> GetRatesForDateRangeAsync(Guid currencyId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get latest exchange rate for currency
        /// الحصول على أحدث سعر صرف للعملة
        /// </summary>
        ExchangeRate GetLatestRate(Guid currencyId);

        /// <summary>
        /// Get latest exchange rate for currency (async)
        /// </summary>
        Task<ExchangeRate> GetLatestRateAsync(Guid currencyId);

        /// <summary>
        /// Get all rates for a specific date
        /// الحصول على جميع الأسعار لتاريخ محدد
        /// </summary>
        List<ExchangeRate> GetRatesForDate(DateTime date);

        /// <summary>
        /// Get all rates for a specific date (async)
        /// </summary>
        Task<List<ExchangeRate>> GetRatesForDateAsync(DateTime date);

        /// <summary>
        /// Check if rate exists for currency and date
        /// التحقق من وجود سعر للعملة والتاريخ
        /// </summary>
        bool RateExistsForDate(Guid currencyId, DateTime date);

        /// <summary>
        /// Check if rate exists for currency and date (async)
        /// </summary>
        Task<bool> RateExistsForDateAsync(Guid currencyId, DateTime date);

        /// <summary>
        /// Get rate history for currency
        /// الحصول على تاريخ الأسعار للعملة
        /// </summary>
        List<ExchangeRate> GetRateHistory(Guid currencyId, int days = 30);

        /// <summary>
        /// Get rate history for currency (async)
        /// </summary>
        Task<List<ExchangeRate>> GetRateHistoryAsync(Guid currencyId, int days = 30);

        /// <summary>
        /// Create exchange rate
        /// إنشاء سعر صرف
        /// </summary>
        int CreateRate(ExchangeRate rate);

        /// <summary>
        /// Create exchange rate (async)
        /// </summary>
        Task<int> CreateRateAsync(ExchangeRate rate);

        /// <summary>
        /// Create multiple exchange rates (bulk insert)
        /// إنشاء عدة أسعار صرف
        /// </summary>
        int CreateRates(List<ExchangeRate> rates);

        /// <summary>
        /// Create multiple exchange rates (bulk insert, async)
        /// </summary>
        Task<int> CreateRatesAsync(List<ExchangeRate> rates);

        /// <summary>
        /// Update exchange rate
        /// تحديث سعر الصرف
        /// </summary>
        int UpdateRate(ExchangeRate rate);

        /// <summary>
        /// Update exchange rate (async)
        /// </summary>
        Task<int> UpdateRateAsync(ExchangeRate rate);

        /// <summary>
        /// Approve exchange rate
        /// الموافقة على سعر الصرف
        /// </summary>
        int ApproveRate(Guid id, Guid approvedBy);

        /// <summary>
        /// Approve exchange rate (async)
        /// </summary>
        Task<int> ApproveRateAsync(Guid id, Guid approvedBy);

        /// <summary>
        /// Delete exchange rate
        /// حذف سعر الصرف
        /// RULE: Cannot delete if used in journals
        /// </summary>
        int DeleteRate(Guid id);

        /// <summary>
        /// Delete exchange rate (async)
        /// </summary>
        Task<int> DeleteRateAsync(Guid id);

        /// <summary>
        /// Check if rate is used in journals
        /// التحقق من استخدام السعر في القيود
        /// </summary>
        bool IsRateUsedInJournals(Guid rateId);

        /// <summary>
        /// Check if rate is used in journals (async)
        /// </summary>
        Task<bool> IsRateUsedInJournalsAsync(Guid rateId);

        /// <summary>
        /// Find gaps in rate history
        /// البحث عن فجوات في تاريخ الأسعار
        /// Returns list of dates where rate is missing
        /// </summary>
        List<DateTime> FindRateGaps(Guid currencyId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Find gaps in rate history (async)
        /// </summary>
        Task<List<DateTime>> FindRateGapsAsync(Guid currencyId, DateTime startDate, DateTime endDate);
    }
}
