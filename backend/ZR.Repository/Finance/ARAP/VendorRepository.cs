using System;
using System.Collections.Generic;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// Vendor Repository - مستودع الموردين
    /// </summary>
    [AppService(ServiceType = typeof(IVendorRepository), ServiceLifetime = LifeTime.Transient)]
    public class VendorRepository : BaseRepository<Vendor>, IVendorRepository
    {
        /// <summary>
        /// Get paginated list of vendors
        /// </summary>
        public PagedInfo<Vendor> GetList(Vendor parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<Vendor>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.VendorCode), it => it.VendorCode.Contains(parm.VendorCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NameAr), it => it.NameAr.Contains(parm.NameAr));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NameEn), it => it.NameEn.Contains(parm.NameEn));
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.VendorCode)
                .ToPage(pager);
        }

        /// <summary>
        /// Get vendor by ID
        /// </summary>
        public Vendor GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get vendor by code
        /// </summary>
        public Vendor GetByCode(string code)
        {
            return GetFirst(x => x.VendorCode == code && x.IsActive);
        }

        /// <summary>
        /// Check if vendor code exists
        /// </summary>
        public bool CodeExists(string code, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<Vendor>();
            predicate = predicate.And(it => it.VendorCode == code);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Get all active vendors
        /// </summary>
        public List<Vendor> GetActiveVendors()
        {
            return Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.VendorCode)
                .ToList();
        }
    }
}


