using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Service;

namespace ZR.Service.IService.Finance.Configuration
{
    /// <summary>
    /// Auto Posting Service Interface - واجهة خدمة الترحيل التلقائي
    /// </summary>
    public interface IAutoPostingService : IBaseService<AutoPostingRule>
    {
        /// <summary>
        /// Get paginated list of rules - الحصول على قائمة القواعد بالترقيم
        /// </summary>
        PagedInfo<AutoPostingRuleListDto> GetList(AutoPostingRuleQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get rule by ID - الحصول على القاعدة بالمعرف
        /// </summary>
        AutoPostingRuleDetailDto GetById(Guid id);

        /// <summary>
        /// Create rule - إنشاء قاعدة
        /// </summary>
        AutoPostingRule Create(CreateAutoPostingRuleDto dto, string userName);

        /// <summary>
        /// Update rule - تحديث قاعدة
        /// </summary>
        int Update(UpdateAutoPostingRuleDto dto, string userName);

        /// <summary>
        /// Delete rules - حذف قواعد
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Get rules for transaction type - الحصول على القواعد لنوع المعاملة
        /// </summary>
        List<AutoPostingRuleListDto> GetRulesForTransactionType(TransactionType transactionType);

        /// <summary>
        /// Apply auto posting rule - تطبيق قاعدة الترحيل التلقائي
        /// </summary>
        object ApplyAutoPostingRule(TransactionType transactionType, decimal amount, string description = null);
    }
}


