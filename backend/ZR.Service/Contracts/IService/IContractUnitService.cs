using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Sales.Dto;

namespace ZR.Service.Contracts.IService
{
    /// <summary>
    /// Contract Unit Service Interface - واجهة خدمة وحدات العقد
    /// </summary>
    public interface IContractUnitService
    {
        /// <summary>
        /// Get units by contract ID
        /// الحصول على الوحدات بمعرف العقد
        /// </summary>
        List<ErpContractUnitListDto> GetByContractId(Guid contractId, bool? isActive = null);

        /// <summary>
        /// Get unit by ID
        /// الحصول على الوحدة بالمعرف
        /// </summary>
        ErpContractUnit GetById(Guid unitId);

        /// <summary>
        /// Create unit
        /// إنشاء وحدة
        /// </summary>
        Guid Create(CreateErpContractUnitDto dto, string createdBy);

        /// <summary>
        /// Update unit (recalculates contract amounts and billing if price changed on active contract)
        /// تحديث الوحدة (يعيد حساب مبالغ العقد والفواتير إذا تغير السعر على عقد نشط)
        /// </summary>
        UnitPriceUpdateResultDto Update(UpdateErpContractUnitDto dto, string updatedBy);

        /// <summary>
        /// Delete unit
        /// حذف الوحدة
        /// </summary>
        int Delete(Guid unitId);

        /// <summary>
        /// Toggle unit active status
        /// تبديل حالة نشاط الوحدة
        /// </summary>
        int ToggleActive(Guid unitId);

        /// <summary>
        /// Check if unit code exists in contract
        /// التحقق من وجود رمز الوحدة
        /// </summary>
        bool UnitCodeExists(Guid contractId, string unitCode, Guid? excludeId = null);

        /// <summary>
        /// Calculate total price for contract units
        /// حساب السعر الإجمالي لوحدات العقد
        /// </summary>
        decimal CalculateTotalPrice(Guid contractId);

        /// <summary>
        /// Get active units count
        /// الحصول على عدد الوحدات النشطة
        /// </summary>
        int GetActiveUnitsCount(Guid contractId);

        /// <summary>
        /// Change the boulevard shop assigned to a contract unit
        /// تغيير المحل المخصص لوحدة العقد
        /// </summary>
        Task<int> ChangeShopAsync(ChangeContractUnitShopDto dto, string updatedBy);

        /// <summary>
        /// Get available shops for a contract unit (for shop change)
        /// الحصول على المحلات المتاحة لوحدة العقد
        /// </summary>
        Task<List<AvailableShopDto>> GetAvailableShopsForUnitAsync(Guid unitId);
    }
}
