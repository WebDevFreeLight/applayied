using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;

namespace ZR.Service.Contracts.IService
{
    /// <summary>
    /// Contract Service Interface - واجهة خدمة العقود
    /// </summary>
    public interface IContractService
    {
        /// <summary>
        /// Get contracts list with pagination
        /// الحصول على قائمة العقود مع الترقيم
        /// </summary>
        PagedInfo<ErpContractListDto> GetList(ErpContractQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get contract by ID
        /// الحصول على العقد بالمعرف
        /// </summary>
        ErpContractDetailDto GetById(Guid contractId);

        /// <summary>
        /// Create contract
        /// إنشاء عقد
        /// </summary>
        Guid Create(CreateErpContractDto dto, string createdBy);

        /// <summary>
        /// Update contract
        /// تحديث العقد
        /// </summary>
        ContractUpdateResultDto Update(UpdateErpContractDto dto, string updatedBy);

        /// <summary>
        /// Get schedule impact for a contract (how many schedules affected by pricing change)
        /// الحصول على تأثير التغيير على جداول الفوترة
        /// </summary>
        ScheduleImpactDto GetScheduleImpact(Guid contractId);

        /// <summary>
        /// Delete contracts
        /// حذف العقود
        /// </summary>
        int Delete(Guid[] contractIds);

        /// <summary>
        /// Activate contract
        /// تفعيل العقد
        /// </summary>
        int Activate(ActivateContractDto dto, string activatedBy);

        /// <summary>
        /// Suspend contract
        /// تعليق العقد
        /// </summary>
        int Suspend(SuspendContractDto dto, string suspendedBy);

        /// <summary>
        /// Resume contract
        /// استئناف العقد
        /// </summary>
        int Resume(Guid contractId, string resumedBy);

        /// <summary>
        /// Terminate contract
        /// إنهاء العقد
        /// </summary>
        int Terminate(TerminateContractDto dto, string terminatedBy);

        /// <summary>
        /// Cancel contract
        /// إلغاء العقد
        /// </summary>
        int Cancel(Guid contractId, string cancelledBy, string reason);

        /// <summary>
        /// Renew contract
        /// تجديد العقد
        /// </summary>
        Guid Renew(Guid contractId, DateTime newEndDate, string renewedBy);

        /// <summary>
        /// Get expiring contracts
        /// الحصول على العقود المنتهية قريباً
        /// </summary>
        List<ErpContractListDto> GetExpiringContracts(int daysFromNow);

        /// <summary>
        /// Get contracts for selection
        /// الحصول على العقود للاختيار
        /// </summary>
        List<ErpContractSelectDto> GetSelectList(ContractStatus? status = null);

        /// <summary>
        /// Get contracts by customer
        /// الحصول على عقود العميل
        /// </summary>
        List<ErpContractListDto> GetByCustomerId(Guid customerId);

        /// <summary>
        /// Get contract statistics
        /// الحصول على إحصائيات العقود
        /// </summary>
        ContractStatisticsDto GetStatistics();

        /// <summary>
        /// Check if contract number exists
        /// التحقق من وجود رقم العقد
        /// </summary>
        bool ContractNumberExists(string contractNumber, Guid? excludeId = null);

        /// <summary>
        /// Export contracts to Excel
        /// تصدير العقود إلى Excel
        /// </summary>
        List<ErpContractListDto> GetExportList(ErpContractQueryDto query);

        /// <summary>
        /// Get contract info by shop IDs for boulevard reports
        /// الحصول على معلومات العقود حسب معرفات المحلات
        /// </summary>
        List<ShopContractInfoDto> GetContractInfoByShopIds(long[] shopIds);
    }
}
