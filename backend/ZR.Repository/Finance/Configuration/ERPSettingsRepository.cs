using System;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// ERP Settings Repository - مستودع إعدادات النظام
    /// </summary>
    [AppService(ServiceType = typeof(IERPSettingsRepository), ServiceLifetime = LifeTime.Transient)]
    public class ERPSettingsRepository : BaseRepository<ERPSettings>, IERPSettingsRepository
    {
        /// <summary>
        /// Get paginated list of settings
        /// </summary>
        public PagedInfo<ERPSettings> GetList(ERPSettings parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ERPSettings>();
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage(pager);
        }

        /// <summary>
        /// Get settings by ID
        /// </summary>
        public ERPSettings GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get settings by company
        /// </summary>
        public ERPSettings GetByCompanyId(long? companyId)
        {
            return GetFirst(x => x.CompanyID == companyId);
        }
    }
}


