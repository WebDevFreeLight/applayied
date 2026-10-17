using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Auto Posting Rule Repository Interface - واجهة مستودع قواعد الترحيل التلقائي
    /// </summary>
    public interface IAutoPostingRuleRepository : IBaseRepository<AutoPostingRule>
    {
        /// <summary>
        /// Get paginated list of rules - الحصول على قائمة القواعد بالترقيم
        /// </summary>
        PagedInfo<AutoPostingRule> GetList(AutoPostingRule parm, PagerInfo pager);

        /// <summary>
        /// Get rule by ID - الحصول على القاعدة بالمعرف
        /// </summary>
        AutoPostingRule GetById(Guid id);

        /// <summary>
        /// Get active rules by transaction type - الحصول على القواعد النشطة بنوع المعاملة
        /// </summary>
        List<AutoPostingRule> GetActiveRulesByTransactionType(ZR.Model.Finance.Configuration.Enums.TransactionType transactionType);

        /// <summary>
        /// Get all active rules - الحصول على جميع القواعد النشطة
        /// </summary>
        List<AutoPostingRule> GetActiveRules();
    }
}


