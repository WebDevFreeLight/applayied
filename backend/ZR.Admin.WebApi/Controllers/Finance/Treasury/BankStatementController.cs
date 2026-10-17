using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Admin.WebApi.Controllers.Finance.Treasury
{
    /// <summary>
    /// Bank Statement Controller - تحكم كشف الحساب البنكي
    /// </summary>
    [Verify]
    [Route("erp/finance/treasury/[controller]")]
    [ApiController]
    public class BankStatementController : BaseController
    {
        private readonly IBankStatementService _bankStatementService;

        public BankStatementController(IBankStatementService bankStatementService)
        {
            _bankStatementService = bankStatementService;
        }

        /// <summary>
        /// Get bank statement list with paging - الحصول على قائمة كشوف الحساب مع ترقيم الصفحات
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] BankStatementQueryDto parm)
        {
            var response = _bankStatementService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get bank statement by ID - الحصول على كشف حساب بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _bankStatementService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Bank statement not found - كشف الحساب غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Create bank statement manually - إنشاء كشف حساب يدوياً
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Bank Statement", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateBankStatementDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _bankStatementService.Create(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Import bank statement from file - استيراد كشف حساب من ملف
        /// Supports CSV and MT940 formats
        /// </summary>
        [HttpPost("import")]
        [Log(Title = "Import Bank Statement", BusinessType = BusinessType.IMPORT)]
        public async Task<IActionResult> Import([FromForm] UploadBankStatementDto dto, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "File is required - الملف مطلوب");
            }

            dto.FileName = file.FileName;

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileContent = memoryStream.ToArray();

            var userId = HttpContext.GetName();
            var result = _bankStatementService.ImportBankStatement(dto, fileContent, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get statement lines - الحصول على بنود الكشف
        /// </summary>
        [HttpGet("{id}/lines")]
        public IActionResult GetStatementLines(Guid id)
        {
            var response = _bankStatementService.GetStatementLines(id);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get unmatched lines for a bank account - الحصول على البنود غير المطابقة لحساب بنكي
        /// </summary>
        [HttpGet("unmatched")]
        public IActionResult GetUnmatchedLines([FromQuery] Guid bankAccountId, [FromQuery] long? companyId = null)
        {
            var response = _bankStatementService.GetUnmatchedLines(bankAccountId, companyId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Auto-match statement lines - مطابقة تلقائية لبنود الكشف
        /// </summary>
        [HttpPost("{id}/auto-match")]
        [Log(Title = "Auto Match Statement Lines", BusinessType = BusinessType.UPDATE)]
        public IActionResult AutoMatchLines(Guid id)
        {
            var result = _bankStatementService.AutoMatchLines(id);
            return SUCCESS(new { MatchedCount = result });
        }

        /// <summary>
        /// Match a statement line manually - مطابقة بند كشف يدوياً
        /// </summary>
        [HttpPost("match")]
        [Log(Title = "Match Statement Line", BusinessType = BusinessType.UPDATE)]
        public IActionResult MatchLine([FromBody] MatchBankStatementLineDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _bankStatementService.MatchLine(dto, userId);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete bank statement - حذف كشف حساب
        /// </summary>
        [HttpDelete("{id}")]
        [Log(Title = "Delete Bank Statement", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid id)
        {
            var result = _bankStatementService.Delete(new[] { id });
            return ToResponse(result);
        }
    }
}
