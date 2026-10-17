using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Contracts.Dto;
using ZR.Service.Contracts.IService;

namespace ZR.Admin.WebApi.Controllers.Contracts
{
    /// <summary>
    /// Contract Unit Controller - وحدات العقد
    /// </summary>
    [Authorize]
    [Route("erp/contracts/units")]
    [ApiController]
    public class ErpContractUnitController : BaseController
    {
        private readonly IContractUnitService _unitService;

        public ErpContractUnitController(IContractUnitService unitService)
        {
            _unitService = unitService;
        }

        /// <summary>
        /// Get units by contract ID
        /// الحصول على الوحدات بمعرف العقد
        /// </summary>
        [HttpGet("contract/{contractId}")]
        public IActionResult GetByContractId(Guid contractId, [FromQuery] bool? isActive = null)
        {
            var result = _unitService.GetByContractId(contractId, isActive);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get unit by ID
        /// الحصول على الوحدة بالمعرف
        /// </summary>
        [HttpGet("{unitId}")]
        public IActionResult GetById(Guid unitId)
        {
            var result = _unitService.GetById(unitId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Unit not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create unit
        /// إنشاء وحدة
        /// </summary>
        [HttpPost]
        [Log(Title = "Contract Unit", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateErpContractUnitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var unitId = _unitService.Create(dto, HttpContext.GetName());
                return SUCCESS(new { UnitId = unitId });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update unit (recalculates contract + billing if price changed on active contract)
        /// تحديث الوحدة (يعيد حساب العقد والفواتير عند تغيير السعر)
        /// </summary>
        [HttpPut]
        [Log(Title = "Contract Unit", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateErpContractUnitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var result = _unitService.Update(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete unit
        /// حذف الوحدة
        /// </summary>
        [HttpDelete("{unitId}")]
        [Log(Title = "Contract Unit", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid unitId)
        {
            try
            {
                var result = _unitService.Delete(unitId);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Toggle unit active status
        /// تبديل حالة نشاط الوحدة
        /// </summary>
        [HttpPost("toggle-active/{unitId}")]
        [Log(Title = "Contract Unit", BusinessType = BusinessType.UPDATE)]
        public IActionResult ToggleActive(Guid unitId)
        {
            try
            {
                var result = _unitService.ToggleActive(unitId);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Check if unit code exists in contract
        /// التحقق من وجود رمز الوحدة
        /// </summary>
        [HttpGet("check-code")]
        public IActionResult UnitCodeExists([FromQuery] Guid contractId, [FromQuery] string unitCode, [FromQuery] Guid? excludeId = null)
        {
            var result = _unitService.UnitCodeExists(contractId, unitCode, excludeId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Calculate total price for contract units
        /// حساب السعر الإجمالي لوحدات العقد
        /// </summary>
        [HttpGet("total-price/{contractId}")]
        public IActionResult CalculateTotalPrice(Guid contractId)
        {
            var result = _unitService.CalculateTotalPrice(contractId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get active units count
        /// الحصول على عدد الوحدات النشطة
        /// </summary>
        [HttpGet("active-count/{contractId}")]
        public IActionResult GetActiveUnitsCount(Guid contractId)
        {
            var result = _unitService.GetActiveUnitsCount(contractId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Change the boulevard shop assigned to a contract unit
        /// تغيير المحل المخصص لوحدة العقد
        /// </summary>
        [HttpPost("change-shop")]
        [Log(Title = "Contract Unit Shop Change", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ChangeShop([FromBody] ChangeContractUnitShopDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var result = await _unitService.ChangeShopAsync(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get available shops for a contract unit (for shop change)
        /// الحصول على المحلات المتاحة لوحدة العقد
        /// </summary>
        [HttpGet("{unitId}/available-shops")]
        public async Task<IActionResult> GetAvailableShops(Guid unitId)
        {
            try
            {
                var result = await _unitService.GetAvailableShopsForUnitAsync(unitId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }
    }
}
