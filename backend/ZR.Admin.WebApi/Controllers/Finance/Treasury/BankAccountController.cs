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
    /// Bank Account Controller - تحكم الحساب البنكي
    /// </summary>
    [Verify]
    [Route("erp/finance/treasury/[controller]")]
    [ApiController]
    public class BankAccountController : BaseController
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Get bank account list with paging - الحصول على قائمة الحسابات البنكية مع ترقيم الصفحات
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] BankAccountQueryDto parm)
        {
            var response = _bankAccountService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get bank account by ID - الحصول على حساب بنكي بالمعرف
        /// </summary>
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            var response = _bankAccountService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Bank account not found - الحساب البنكي غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Create bank account with automatic GL account creation - إنشاء حساب بنكي مع إنشاء تلقائي لحساب دفتر الأستاذ
        /// If GLAccountId is not provided and AutoCreateGLAccount is true (default),
        /// a new GL account will be automatically created under the specified parent (or default Bank Accounts parent).
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Bank Account", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> Create([FromBody] CreateBankAccountDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = await _bankAccountService.CreateWithGLAccountAsync(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get available parent GL accounts for bank account placement - الحصول على حسابات الأصل المتاحة لوضع الحسابات البنكية
        /// Returns GL accounts suitable for placing new bank GL accounts under.
        /// </summary>
        [HttpGet("gl-parent-options")]
        public async Task<IActionResult> GetGLParentOptions([FromQuery] long? companyId)
        {
            var response = await _bankAccountService.GetBankParentAccountOptionsAsync(companyId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update bank account - تحديث حساب بنكي
        /// </summary>
        [HttpPut]
        [Log(Title = "Update Bank Account", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateBankAccountDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _bankAccountService.Update(dto, userId);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete bank accounts - حذف حسابات بنكية
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Bank Accounts", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();
            var result = _bankAccountService.Delete(idArray);
            return ToResponse(result);
        }

        /// <summary>
        /// Get active bank accounts for dropdowns - الحصول على الحسابات البنكية النشطة للقوائم
        /// </summary>
        [HttpGet("active")]
        public IActionResult GetActive([FromQuery] long? companyId)
        {
            var response = _bankAccountService.GetActiveAccounts(companyId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get bank account balance - الحصول على رصيد الحساب البنكي
        /// </summary>
        [HttpGet("{id:guid}/balance")]
        public IActionResult GetBalance(Guid id)
        {
            var response = _bankAccountService.GetBalance(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Bank account not found - الحساب البنكي غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Get pending reconciliations - الحصول على التسويات المعلقة
        /// </summary>
        [HttpGet("{id:guid}/pending-reconciliations")]
        public IActionResult GetPendingReconciliations(Guid id)
        {
            var response = _bankAccountService.GetPendingReconciliations(id);
            return SUCCESS(response);
        }
    }
}






























