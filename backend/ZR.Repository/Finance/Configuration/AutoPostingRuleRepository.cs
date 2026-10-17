using System;
using System.Collections.Generic;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Enums;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Auto Posting Rule Repository - مستودع قواعد الترحيل التلقائي
    /// </summary>
    [AppService(ServiceType = typeof(IAutoPostingRuleRepository), ServiceLifetime = LifeTime.Transient)]
    public class AutoPostingRuleRepository : BaseRepository<AutoPostingRule>, IAutoPostingRuleRepository
    {
        /// <summary>
        /// Get paginated list of rules
        /// </summary>
        public PagedInfo<AutoPostingRule> GetList(AutoPostingRule parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<AutoPostingRule>();
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.Priority)
                .OrderBy(x => x.TransactionType)
                .ToPage(pager);
        }

        /// <summary>
        /// Get rule by ID
        /// </summary>
        public AutoPostingRule GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get active rules by transaction type
        /// </summary>
        public List<AutoPostingRule> GetActiveRulesByTransactionType(TransactionType transactionType)
        {
            return Queryable()
                .Where(x => x.TransactionType == transactionType && x.Active == true)
                .OrderBy(x => x.Priority)
                .ToList();
        }

        /// <summary>
        /// Get all active rules
        /// </summary>
        public List<AutoPostingRule> GetActiveRules()
        {
            return Queryable()
                .Where(x => x.Active == true)
                .OrderBy(x => x.Priority)
                .OrderBy(x => x.TransactionType)
                .ToList();
        }
    }
}


