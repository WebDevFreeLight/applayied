using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Customers;

namespace ZR.Repository.Customers
{
    /// <summary>
    /// Customer Repository Interface - واجهة مستودع العملاء
    /// </summary>
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        /// <summary>
        /// Get paginated list of customers - الحصول على قائمة العملاء بالترقيم
        /// </summary>
        PagedInfo<Customer> GetList(Customer parm, PagerInfo pager, bool? isActive = null);

        /// <summary>
        /// Get customer by ID - الحصول على العميل بالمعرف
        /// </summary>
        Customer GetById(Guid id);

        /// <summary>
        /// Get customer by code - الحصول على العميل بالرمز
        /// </summary>
        Customer GetByCode(string code);

        /// <summary>
        /// Check if customer code exists - التحقق من وجود رمز العميل
        /// </summary>
        bool CodeExists(string code, Guid? excludeId = null);

        /// <summary>
        /// Get all active customers - الحصول على جميع العملاء النشطين
        /// </summary>
        List<Customer> GetActiveCustomers();
    }
}
