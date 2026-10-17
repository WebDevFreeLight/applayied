using System;
using System.Collections.Generic;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Customers;

namespace ZR.Repository.Customers
{
    /// <summary>
    /// Customer Repository - مستودع العملاء
    /// </summary>
    [AppService(ServiceType = typeof(ICustomerRepository), ServiceLifetime = LifeTime.Transient)]
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        /// <summary>
        /// Get paginated list of customers
        /// </summary>
        public PagedInfo<Customer> GetList(Customer parm, PagerInfo pager, bool? isActive = null)
        {
            var predicate = Expressionable.Create<Customer>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CustomerCode), it => it.CustomerCode.Contains(parm.CustomerCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NameAr), it => it.NameAr.Contains(parm.NameAr));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NameEn), it => it.NameEn.Contains(parm.NameEn));
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);
            predicate = predicate.AndIF(isActive.HasValue, it => it.IsActive == isActive.Value);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.CustomerCode)
                .ToPage(pager);
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        public Customer GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get customer by code
        /// </summary>
        public Customer GetByCode(string code)
        {
            return GetFirst(x => x.CustomerCode == code && x.IsActive);
        }

        /// <summary>
        /// Check if customer code exists
        /// </summary>
        public bool CodeExists(string code, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<Customer>();
            predicate = predicate.And(it => it.CustomerCode == code);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Get all active customers
        /// </summary>
        public List<Customer> GetActiveCustomers()
        {
            return Queryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.CustomerCode)
                .ToList();
        }
    }
}
