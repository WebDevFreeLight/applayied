using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Customers;
using ZR.Model.Customers.Dto;

namespace ZR.Service.IService.Customers
{
    /// <summary>
    /// Customer Service Interface - واجهة خدمة العملاء
    /// </summary>
    public interface ICustomerService : IBaseService<Customer>
    {
        /// <summary>
        /// Get paginated list of customers - الحصول على قائمة العملاء بالترقيم
        /// </summary>
        PagedInfo<CustomerListDto> GetList(CustomerQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get customer by ID - الحصول على العميل بالمعرف
        /// </summary>
        CustomerDetailDto GetById(Guid id);

        /// <summary>
        /// Create customer - إنشاء عميل
        /// </summary>
        Customer Create(CreateCustomerDto dto, string userName);

        /// <summary>
        /// Update customer - تحديث عميل
        /// </summary>
        int Update(UpdateCustomerDto dto, string userName);

        /// <summary>
        /// Delete customers - حذف عملاء
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Check if customer code exists - التحقق من وجود رمز العميل
        /// </summary>
        bool CodeExists(string code, Guid? excludeId = null);

        /// <summary>
        /// Generate next customer code - توليد رمز العميل التالي
        /// </summary>
        string GenerateNextCustomerCode();

        /// <summary>
        /// Validate credit limit - التحقق من الحد الائتماني
        /// </summary>
        bool ValidateCreditLimit(Guid customerId, decimal invoiceAmount);

        /// <summary>
        /// Get active customers for dropdown - الحصول على العملاء النشطين للقائمة المنسدلة
        /// </summary>
        List<CustomerSelectDto> GetActiveCustomers();

        /// <summary>
        /// Check if customer profile is complete (100%) - التحقق من اكتمال ملف العميل
        /// </summary>
        bool IsProfileComplete(Guid customerId);

        /// <summary>
        /// Get customer profile completion percentage - الحصول على نسبة اكتمال ملف العميل
        /// </summary>
        int GetProfileCompletionPercentage(Guid customerId);

        /// <summary>
        /// Get customer by LeadId - الحصول على العميل بمعرف العميل المحتمل
        /// </summary>
        Customer GetByLeadId(long leadId);
    }
}
