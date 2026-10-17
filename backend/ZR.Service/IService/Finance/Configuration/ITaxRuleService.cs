using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;

namespace ZR.Service.IService.Finance.Configuration
{
    /// <summary>
    /// Tax Rule Service Interface - واجهة خدمة قواعد الضريبة
    /// </summary>
    public interface ITaxRuleService : IBaseService<TaxRule>
    {
        /// <summary>
        /// Get paginated list of tax rules - الحصول على قائمة قواعد الضريبة بالترقيم
        /// </summary>
        PagedInfo<TaxRuleListDto> GetList(TaxRuleQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get tax rule by ID - الحصول على قاعدة الضريبة بالمعرف
        /// </summary>
        TaxRuleDetailDto GetById(Guid id);

        /// <summary>
        /// Create tax rule - إنشاء قاعدة ضريبة
        /// </summary>
        TaxRule Create(CreateTaxRuleDto dto, string userName);

        /// <summary>
        /// Update tax rule - تحديث قاعدة ضريبة
        /// </summary>
        int Update(UpdateTaxRuleDto dto, string userName);

        /// <summary>
        /// Delete tax rules - حذف قواعد ضريبة
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Calculate tax - حساب الضريبة
        /// </summary>
        TaxCalculationResultDto CalculateTax(CalculateTaxDto dto);

        /// <summary>
        /// Get active tax rules - الحصول على قواعد الضريبة النشطة
        /// </summary>
        List<TaxRuleListDto> GetActiveTaxRules();
    }
}


