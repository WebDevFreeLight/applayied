using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;

namespace ZR.Service.IService.Finance.Configuration
{
    /// <summary>
    /// Currency Service Interface - واجهة خدمة العملات
    /// </summary>
    public interface ICurrencyService : IBaseService<Currency>
    {
        /// <summary>
        /// Get paginated list of currencies - الحصول على قائمة العملات بالترقيم
        /// </summary>
        PagedInfo<CurrencyListDto> GetList(CurrencyQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get currency by ID - الحصول على العملة بالمعرف
        /// </summary>
        CurrencyDetailDto GetById(Guid id);

        /// <summary>
        /// Create currency - إنشاء عملة
        /// </summary>
        Currency Create(CreateCurrencyDto dto, string userName);

        /// <summary>
        /// Update currency - تحديث عملة
        /// </summary>
        int Update(UpdateCurrencyDto dto, string userName);

        /// <summary>
        /// Delete currencies - حذف عملات
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Set base currency - تعيين العملة الأساسية
        /// </summary>
        int SetBaseCurrency(Guid currencyId, string userName);

        /// <summary>
        /// Get base currency - الحصول على العملة الأساسية
        /// </summary>
        CurrencyDetailDto GetBaseCurrency();

        /// <summary>
        /// Get all active currencies - الحصول على جميع العملات النشطة
        /// </summary>
        List<CurrencyListDto> GetActiveCurrencies();

        /// <summary>
        /// Check if currency code exists - التحقق من وجود رمز العملة
        /// </summary>
        bool CodeExists(string currencyCode, Guid? excludeId = null);
    }
}


