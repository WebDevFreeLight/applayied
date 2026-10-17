using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Tax Rule Repository Interface - واجهة مستودع قواعد الضريبة
    /// </summary>
    public interface ITaxRuleRepository : IBaseRepository<TaxRule>
    {
        /// <summary>
        /// Get paginated list of tax rules - الحصول على قائمة قواعد الضريبة بالترقيم
        /// </summary>
        PagedInfo<TaxRule> GetList(TaxRule parm, PagerInfo pager);

        /// <summary>
        /// Get tax rule by ID - الحصول على قاعدة الضريبة بالمعرف
        /// </summary>
        TaxRule GetById(Guid id);

        /// <summary>
        /// Get active tax rules - الحصول على قواعد الضريبة النشطة
        /// </summary>
        List<TaxRule> GetActiveTaxRules();

        /// <summary>
        /// Get tax rules by type - الحصول على قواعد الضريبة بالنوع
        /// </summary>
        List<TaxRule> GetByTaxType(ZR.Model.Finance.Configuration.Enums.TaxType taxType);
    }
}


