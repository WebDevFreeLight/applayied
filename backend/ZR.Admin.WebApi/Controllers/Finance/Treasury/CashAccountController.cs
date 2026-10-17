using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Admin.WebApi.Controllers.Finance.Treasury
{
    /// <summary>
    /// Cash Account Controller - تحكم الحساب النقدي
    /// </summary>
    [Verify]
    [Route("erp/finance/treasury/[controller]")]
    [ApiController]
    public class CashAccountController : BaseController
    {
        private readonly ICashAccountService _cashAccountService;

        public CashAccountController(ICashAccountService cashAccountService)
        {
            _cashAccountService = cashAccountService;
        }

        /// <summary>
        /// Get cash account list with paging - الحصول على قائمة الحسابات النقدية مع ترقيم الصفحات
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] CashAccountQueryDto parm)
        {
            var response = _cashAccountService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get cash account by ID - الحصول على حساب نقدي بالمعرف
        /// </summary>
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            var response = _cashAccountService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Cash account not found - الحساب النقدي غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Create cash account - إنشاء حساب نقدي
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Cash Account", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateCashAccountDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _cashAccountService.Create(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update cash account - تحديث حساب نقدي
        /// </summary>
        [HttpPut]
        [Log(Title = "Update Cash Account", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateCashAccountDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _cashAccountService.Update(dto, userId);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete cash accounts - حذف حسابات نقدية
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Cash Accounts", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();
            var result = _cashAccountService.Delete(idArray);
            return ToResponse(result);
        }

        /// <summary>
        /// Get cash account balance - الحصول على رصيد الحساب النقدي
        /// </summary>
        [HttpGet("{id:guid}/balance")]
        public IActionResult GetBalance(Guid id)
        {
            var balance = _cashAccountService.GetBalance(id);
            return SUCCESS(balance);
        }

        /// <summary>
        /// Adjust cash account balance - تعديل رصيد الحساب النقدي
        /// </summary>
        [HttpPost("{id:guid}/adjust-balance")]
        [Log(Title = "Adjust Cash Account Balance", BusinessType = BusinessType.UPDATE)]
        public IActionResult AdjustBalance(Guid id, [FromBody] AdjustBalanceDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _cashAccountService.AdjustBalance(id, dto.AdjustmentAmount, dto.Reason, userId);
            return ToResponse(result);
        }

        /// <summary>
        /// Get GL parent account options for cash account creation
        /// الحصول على خيارات حسابات الأصل لإنشاء الحسابات النقدية
        /// </summary>
        [HttpGet("gl-parent-options")]
        public async Task<IActionResult> GetGLParentOptions([FromQuery] long? companyId = null)
        {
            var options = await _cashAccountService.GetCashParentAccountOptionsAsync(companyId);

            // Map to simplified response DTO
            var result = options.Select(a => new
            {
                AccountId = a.AccountId,
                AccountNumber = a.AccountNumber,
                AccountNameAr = a.AccountNameAr,
                AccountNameEn = a.AccountNameEn,
                Level = a.Level
            }).ToList();

            return SUCCESS(result);
        }
    }

    /// <summary>
    /// Adjust Balance DTO - DTO لتعديل الرصيد
    /// </summary>
    public class AdjustBalanceDto
    {
        public decimal AdjustmentAmount { get; set; }
        public string Reason { get; set; }
    }
}






























