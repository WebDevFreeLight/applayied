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
    /// Tax Rule Repository - مستودع قواعد الضريبة
    /// </summary>
    [AppService(ServiceType = typeof(ITaxRuleRepository), ServiceLifetime = LifeTime.Transient)]
    public class TaxRuleRepository : BaseRepository<TaxRule>, ITaxRuleRepository
    {
        /// <summary>
        /// Get paginated list of tax rules
        /// </summary>
        public PagedInfo<TaxRule> GetList(TaxRule parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<TaxRule>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TaxName), it => it.TaxName.Contains(parm.TaxName));
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.TaxName)
                .ToPage(pager);
        }

        /// <summary>
        /// Get tax rule by ID
        /// </summary>
        public TaxRule GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get active tax rules
        /// </summary>
        public List<TaxRule> GetActiveTaxRules()
        {
            var now = DateTime.Now;
            return Queryable()
                .Where(x => x.Status == true 
                    && (x.EffectiveFrom == null || x.EffectiveFrom <= now)
                    && (x.EffectiveTo == null || x.EffectiveTo >= now))
                .OrderBy(x => x.TaxName)
                .ToList();
        }

        /// <summary>
        /// Get tax rules by type
        /// </summary>
        public List<TaxRule> GetByTaxType(TaxType taxType)
        {
            var now = DateTime.Now;
            return Queryable()
                .Where(x => x.TaxType == taxType 
                    && x.Status == true
                    && (x.EffectiveFrom == null || x.EffectiveFrom <= now)
                    && (x.EffectiveTo == null || x.EffectiveTo >= now))
                .OrderBy(x => x.TaxName)
                .ToList();
        }
    }
}


