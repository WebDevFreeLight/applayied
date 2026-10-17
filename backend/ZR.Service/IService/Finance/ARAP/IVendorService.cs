using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;

namespace ZR.Service.IService.Finance.ARAP
{
    /// <summary>
    /// Vendor Service Interface - واجهة خدمة الموردين
    /// </summary>
    public interface IVendorService : IBaseService<Vendor>
    {
        /// <summary>
        /// Get paginated list of vendors - الحصول على قائمة الموردين بالترقيم
        /// </summary>
        PagedInfo<VendorListDto> GetList(VendorQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get vendor by ID - الحصول على المورد بالمعرف
        /// </summary>
        VendorDetailDto GetById(Guid id);

        /// <summary>
        /// Create vendor - إنشاء مورد
        /// </summary>
        Vendor Create(CreateVendorDto dto, string userName);

        /// <summary>
        /// Update vendor - تحديث مورد
        /// </summary>
        int Update(UpdateVendorDto dto, string userName);

        /// <summary>
        /// Delete vendors - حذف موردين
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Check if vendor code exists - التحقق من وجود رمز المورد
        /// </summary>
        bool CodeExists(string code, Guid? excludeId = null);

        /// <summary>
        /// Generate next vendor code - توليد رمز المورد التالي
        /// </summary>
        string GenerateNextVendorCode();

        /// <summary>
        /// Get active vendors for dropdown - الحصول على الموردين النشطين للقائمة المنسدلة
        /// </summary>
        List<VendorSelectDto> GetActiveVendors();
    }
}


