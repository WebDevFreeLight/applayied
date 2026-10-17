using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// Vendor Repository Interface - واجهة مستودع الموردين
    /// </summary>
    public interface IVendorRepository : IBaseRepository<Vendor>
    {
        /// <summary>
        /// Get paginated list of vendors - الحصول على قائمة الموردين بالترقيم
        /// </summary>
        PagedInfo<Vendor> GetList(Vendor parm, PagerInfo pager);

        /// <summary>
        /// Get vendor by ID - الحصول على المورد بالمعرف
        /// </summary>
        Vendor GetById(Guid id);

        /// <summary>
        /// Get vendor by code - الحصول على المورد بالرمز
        /// </summary>
        Vendor GetByCode(string code);

        /// <summary>
        /// Check if vendor code exists - التحقق من وجود رمز المورد
        /// </summary>
        bool CodeExists(string code, Guid? excludeId = null);

        /// <summary>
        /// Get all active vendors - الحصول على جميع الموردين النشطين
        /// </summary>
        List<Vendor> GetActiveVendors();
    }
}


