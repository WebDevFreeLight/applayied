using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Admin.WebApi.Controllers.Finance.Treasury
{
    /// <summary>
    /// Cheque Controller - تحكم الشيك
    /// </summary>
    [Verify]
    [Route("erp/finance/treasury/[controller]")]
    [ApiController]
    public class ChequeController : BaseController
    {
        private readonly IChequeService _chequeService;

        public ChequeController(IChequeService chequeService)
        {
            _chequeService = chequeService;
        }

        /// <summary>
        /// Get cheque list with paging - الحصول على قائمة الشيكات مع ترقيم الصفحات
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ChequeQueryDto parm)
        {
            var response = _chequeService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get cheque by ID - الحصول على شيك بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _chequeService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Cheque not found - الشيك غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Create cheque - إنشاء شيك
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Cheque", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateChequeDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var userId = HttpContext.GetName();
                var result = _chequeService.Create(dto, userId);
                return SUCCESS(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Update cheque - تحديث شيك
        /// </summary>
        [HttpPut]
        [Log(Title = "Update Cheque", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateChequeDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var userId = HttpContext.GetName();
                var result = _chequeService.Update(dto, userId);
                return ToResponse(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Delete cheques - حذف شيكات
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Cheques", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();
            var result = _chequeService.Delete(idArray);
            return ToResponse(result);
        }

        /// <summary>
        /// Present cheque - تقديم شيك
        /// </summary>
        [HttpPut("{id}/present")]
        [Log(Title = "Present Cheque", BusinessType = BusinessType.UPDATE)]
        public IActionResult Present(Guid id)
        {
            try
            {
                var userId = HttpContext.GetName();
                var result = _chequeService.Present(id, userId);
                return ToResponse(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Clear cheque - تسوية شيك
        /// </summary>
        [HttpPut("{id}/clear")]
        [Log(Title = "Clear Cheque", BusinessType = BusinessType.UPDATE)]
        public IActionResult Clear(Guid id)
        {
            try
            {
                var userId = HttpContext.GetName();
                var result = _chequeService.Clear(id, userId);
                return ToResponse(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Bounce cheque - ارتداد شيك
        /// Accepts BounceChequeDto with reason and optional bank fees
        /// </summary>
        [HttpPut("{id}/bounce")]
        [Log(Title = "Bounce Cheque", BusinessType = BusinessType.UPDATE)]
        public IActionResult Bounce(Guid id, [FromBody] BounceChequeDto dto)
        {
            try
            {
                var userId = HttpContext.GetName();
                int result;

                // Support both simple string reason and BounceChequeDto
                if (dto != null && dto.BankFees > 0)
                {
                    result = _chequeService.BounceWithFees(id, userId, dto.Reason, dto.BankFees);
                }
                else
                {
                    result = _chequeService.Bounce(id, userId, dto?.Reason ?? "");
                }

                return ToResponse(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Cancel cheque - إلغاء شيك
        /// </summary>
        [HttpPut("{id}/cancel")]
        [Log(Title = "Cancel Cheque", BusinessType = BusinessType.UPDATE)]
        public IActionResult Cancel(Guid id, [FromBody] CancelChequeDto dto)
        {
            try
            {
                var userId = HttpContext.GetName();
                var result = _chequeService.Cancel(id, userId, dto.Reason);
                return ToResponse(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Get pending cheques - الحصول على الشيكات المعلقة
        /// </summary>
        [HttpGet("pending")]
        public IActionResult GetPendingCheques([FromQuery] Guid? bankAccountId = null, [FromQuery] long? tenantId = null)
        {
            var response = _chequeService.GetPendingCheques(bankAccountId, tenantId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get cheques due in period - الحصول على الشيكات المستحقة في الفترة
        /// </summary>
        [HttpGet("due")]
        public IActionResult GetChequesDueInPeriod(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? bankAccountId = null,
            [FromQuery] long? tenantId = null)
        {
            var response = _chequeService.GetChequesDueInPeriod(startDate, endDate, bankAccountId, tenantId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get cheque status history - الحصول على سجل تغييرات الحالة
        /// </summary>
        [HttpGet("{id}/history")]
        public IActionResult GetHistory(Guid id)
        {
            try
            {
                var response = _chequeService.GetChequeHistory(id);
                return SUCCESS(response);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Get cheque timeline for UI display - الحصول على الجدول الزمني للعرض
        /// </summary>
        [HttpGet("{id}/timeline")]
        public IActionResult GetTimeline(Guid id)
        {
            try
            {
                var response = _chequeService.GetChequeTimeline(id);
                return SUCCESS(response);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Get cheque statistics - الحصول على إحصائيات الشيكات
        /// </summary>
        [HttpGet("statistics")]
        public IActionResult GetStatistics([FromQuery] Guid? bankAccountId = null, [FromQuery] long? companyId = null)
        {
            try
            {
                var response = _chequeService.GetStatistics(bankAccountId, companyId);
                return SUCCESS(response);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Generate next cheque number - توليد رقم الشيك التالي
        /// </summary>
        [HttpGet("generate-number")]
        public IActionResult GenerateNumber([FromQuery] Guid bankAccountId, [FromQuery] int chequeType = 0)
        {
            try
            {
                if (bankAccountId == Guid.Empty)
                {
                    return ToResponse(ResultCode.PARAM_ERROR, "Bank account ID is required - معرف الحساب البنكي مطلوب");
                }

                var type = (ChequeType)chequeType;
                var chequeNumber = _chequeService.GenerateNextChequeNumber(bankAccountId, type);
                return SUCCESS(new { chequeNumber });
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }
    }
}






























