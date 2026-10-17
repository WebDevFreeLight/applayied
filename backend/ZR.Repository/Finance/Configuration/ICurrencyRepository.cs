using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Currency Repository Interface
    /// مستودع العملات
    /// </summary>
    public interface ICurrencyRepository
    {
        /// <summary>
        /// Get paginated list of currencies
        /// الحصول على قائمة العملات
        /// </summary>
        PagedInfo<Currency> GetList(Currency searchModel, PagerInfo pager);

        /// <summary>
        /// Get currency by ID
        /// الحصول على العملة بالمعرف
        /// </summary>
        Currency GetById(Guid id);

        /// <summary>
        /// Get currency by ID (async)
        /// </summary>
        Task<Currency> GetByIdAsync(Guid id);

        /// <summary>
        /// Get currency by code
        /// الحصول على العملة بالرمز
        /// </summary>
        Currency GetByCode(string code);

        /// <summary>
        /// Get currency by code (async)
        /// </summary>
        Task<Currency> GetByCodeAsync(string code);

        /// <summary>
        /// Get base currency
        /// الحصول على العملة الأساسية
        /// RULE: Only one base currency can exist
        /// </summary>
        Currency GetBaseCurrency();

        /// <summary>
        /// Get base currency (async)
        /// </summary>
        Task<Currency> GetBaseCurrencyAsync();

        /// <summary>
        /// Get all active currencies
        /// الحصول على جميع العملات النشطة
        /// </summary>
        List<Currency> GetActiveCurrencies();

        /// <summary>
        /// Get all active currencies (async)
        /// </summary>
        Task<List<Currency>> GetActiveCurrenciesAsync();

        /// <summary>
        /// Check if currency code exists
        /// التحقق من وجود رمز العملة
        /// </summary>
        bool CurrencyCodeExists(string code, Guid? excludeId = null);

        /// <summary>
        /// Check if currency code exists (async)
        /// </summary>
        Task<bool> CurrencyCodeExistsAsync(string code, Guid? excludeId = null);

        /// <summary>
        /// Check if base currency exists
        /// التحقق من وجود عملة أساسية
        /// </summary>
        bool BaseCurrencyExists();

        /// <summary>
        /// Check if base currency exists (async)
        /// </summary>
        Task<bool> BaseCurrencyExistsAsync();

        /// <summary>
        /// Create currency
        /// إنشاء عملة
        /// </summary>
        int CreateCurrency(Currency currency);

        /// <summary>
        /// Create currency (async)
        /// </summary>
        Task<int> CreateCurrencyAsync(Currency currency);

        /// <summary>
        /// Update currency
        /// تحديث العملة
        /// RULE: Code cannot be updated
        /// </summary>
        int UpdateCurrency(Currency currency);

        /// <summary>
        /// Update currency (async)
        /// </summary>
        Task<int> UpdateCurrencyAsync(Currency currency);

        /// <summary>
        /// Set base currency
        /// تعيين العملة الأساسية
        /// RULE: Must unset previous base currency first
        /// </summary>
        int SetBaseCurrency(Guid currencyId);

        /// <summary>
        /// Set base currency (async)
        /// </summary>
        Task<int> SetBaseCurrencyAsync(Guid currencyId);

        /// <summary>
        /// Activate currency
        /// تفعيل العملة
        /// </summary>
        int ActivateCurrency(Guid id);

        /// <summary>
        /// Activate currency (async)
        /// </summary>
        Task<int> ActivateCurrencyAsync(Guid id);

        /// <summary>
        /// Deactivate currency
        /// إلغاء تفعيل العملة
        /// RULE: Cannot deactivate base currency
        /// </summary>
        int DeactivateCurrency(Guid id);

        /// <summary>
        /// Deactivate currency (async)
        /// </summary>
        Task<int> DeactivateCurrencyAsync(Guid id);

        /// <summary>
        /// Delete currency
        /// حذف العملة
        /// RULE: Cannot delete base currency
        /// RULE: Cannot delete if used in journals
        /// </summary>
        int DeleteCurrency(Guid id);

        /// <summary>
        /// Delete currency (async)
        /// </summary>
        Task<int> DeleteCurrencyAsync(Guid id);

        /// <summary>
        /// Check if currency is used in journals
        /// التحقق من استخدام العملة في القيود
        /// </summary>
        bool IsUsedInJournals(Guid currencyId);

        /// <summary>
        /// Check if currency is used in journals (async)
        /// </summary>
        Task<bool> IsUsedInJournalsAsync(Guid currencyId);
    }
}
