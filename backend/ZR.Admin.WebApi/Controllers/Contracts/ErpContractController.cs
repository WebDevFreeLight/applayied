using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Service.Contracts.IService;

namespace ZR.Admin.WebApi.Controllers.Contracts
{
    /// <summary>
    /// Contract Management Controller - إدارة العقود
    /// </summary>
    [Authorize]
    [Route("erp/contracts")]
    [ApiController]
    public class ErpContractController : BaseController
    {
        private readonly IContractService _contractService;
        private readonly IWebHostEnvironment _env;

        public ErpContractController(IContractService contractService, IWebHostEnvironment env)
        {
            _contractService = contractService;
            _env = env;
        }

        private string GetContractPdfDirectory()
        {
            var dir = global::System.IO.Path.Combine(_env.ContentRootPath, "secure-uploads", "contracts", "pdfs");
            if (!global::System.IO.Directory.Exists(dir))
            {
                global::System.IO.Directory.CreateDirectory(dir);
            }
            return dir;
        }

        /// <summary>
        /// Get contracts list with pagination
        /// الحصول على قائمة العقود مع الترقيم
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ErpContractQueryDto query, [FromQuery] PagerInfo pager)
        {
            var result = _contractService.GetList(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get contract by ID
        /// الحصول على العقد بالمعرف
        /// </summary>
        [HttpGet("{contractId}")]
        public IActionResult GetById(Guid contractId)
        {
            var result = _contractService.GetById(contractId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Contract not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create contract
        /// إنشاء عقد جديد
        /// </summary>
        [HttpPost]
        [Log(Title = "Contract", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateErpContractDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var contractId = _contractService.Create(dto, HttpContext.GetName());
                return SUCCESS(new { ContractId = contractId });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update contract
        /// تحديث العقد
        /// Returns ContractUpdateResultDto with warning info for pricing changes on active contracts
        /// </summary>
        [HttpPut]
        [Log(Title = "Contract", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateErpContractDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var result = _contractService.Update(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete contracts
        /// حذف العقود
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Contract", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            try
            {
                var idArray = ids.Split(',').Select(x => Guid.Parse(x.Trim())).ToArray();
                var result = _contractService.Delete(idArray);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Activate contract
        /// تفعيل العقد
        /// </summary>
        [HttpPost("activate")]
        [Log(Title = "Contract", BusinessType = BusinessType.UPDATE)]
        public IActionResult Activate([FromBody] ActivateContractDto dto)
        {
            try
            {
                var result = _contractService.Activate(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Suspend contract
        /// تعليق العقد
        /// </summary>
        [HttpPost("suspend")]
        [Log(Title = "Contract", BusinessType = BusinessType.UPDATE)]
        public IActionResult Suspend([FromBody] SuspendContractDto dto)
        {
            try
            {
                var result = _contractService.Suspend(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Resume contract
        /// استئناف العقد
        /// </summary>
        [HttpPost("resume/{contractId}")]
        [Log(Title = "Contract", BusinessType = BusinessType.UPDATE)]
        public IActionResult Resume(Guid contractId)
        {
            try
            {
                var result = _contractService.Resume(contractId, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Terminate contract
        /// إنهاء العقد
        /// </summary>
        [HttpPost("terminate")]
        [Log(Title = "Contract", BusinessType = BusinessType.UPDATE)]
        public IActionResult Terminate([FromBody] TerminateContractDto dto)
        {
            try
            {
                var result = _contractService.Terminate(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Cancel contract
        /// إلغاء العقد
        /// </summary>
        [HttpPost("cancel/{contractId}")]
        [Log(Title = "Contract", BusinessType = BusinessType.UPDATE)]
        public IActionResult Cancel(Guid contractId, [FromQuery] string reason)
        {
            try
            {
                var result = _contractService.Cancel(contractId, HttpContext.GetName(), reason);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Renew contract
        /// تجديد العقد
        /// </summary>
        [HttpPost("renew")]
        [Log(Title = "Contract", BusinessType = BusinessType.INSERT)]
        public IActionResult Renew([FromBody] RenewContractDto dto)
        {
            try
            {
                var newContractId = _contractService.Renew(dto.ContractId, dto.NewEndDate, HttpContext.GetName());
                return SUCCESS(new { ContractId = newContractId });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get expiring contracts
        /// الحصول على العقود المنتهية قريباً
        /// </summary>
        [HttpGet("expiring")]
        public IActionResult GetExpiringContracts([FromQuery] int daysFromNow = 30)
        {
            var result = _contractService.GetExpiringContracts(daysFromNow);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get contracts for selection dropdown
        /// الحصول على العقود للقائمة المنسدلة
        /// </summary>
        [HttpGet("select")]
        public IActionResult GetSelectList([FromQuery] ContractStatus? status = null)
        {
            var result = _contractService.GetSelectList(status);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get contracts by customer
        /// الحصول على عقود العميل
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public IActionResult GetByCustomerId(Guid customerId)
        {
            var result = _contractService.GetByCustomerId(customerId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get contract statistics
        /// الحصول على إحصائيات العقود
        /// </summary>
        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var result = _contractService.GetStatistics();
            return SUCCESS(result);
        }

        /// <summary>
        /// Get contract info by shop IDs (for boulevard reports)
        /// الحصول على معلومات العقود حسب معرفات المحلات
        /// </summary>
        [HttpGet("shop-info")]
        public IActionResult GetContractInfoByShops([FromQuery] string shopIds)
        {
            if (string.IsNullOrWhiteSpace(shopIds))
            {
                return SUCCESS(new List<object>());
            }
            var ids = shopIds.Split(',').Select(x => long.Parse(x.Trim())).ToArray();
            var result = _contractService.GetContractInfoByShopIds(ids);
            return SUCCESS(result);
        }

        /// <summary>
        /// Check if contract number exists
        /// التحقق من وجود رقم العقد
        /// </summary>
        [HttpGet("check-number")]
        public IActionResult ContractNumberExists([FromQuery] string contractNumber, [FromQuery] Guid? excludeId = null)
        {
            var result = _contractService.ContractNumberExists(contractNumber, excludeId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get schedule impact preview for a contract
        /// Shows how many billing schedules would be affected by a pricing change
        /// معاينة تأثير التغيير على جداول الفوترة
        /// </summary>
        [HttpGet("{contractId}/schedule-impact")]
        public IActionResult GetScheduleImpact(Guid contractId)
        {
            try
            {
                var result = _contractService.GetScheduleImpact(contractId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Export contracts
        /// تصدير العقود
        /// </summary>
        [HttpGet("export")]
        [Log(Title = "Contract", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpContractQueryDto query)
        {
            var result = _contractService.GetExportList(query);
            return SUCCESS(result);
        }

        // ============================================================================
        // PDF DOCUMENT OPERATIONS - عمليات مستندات PDF
        // ============================================================================

        /// <summary>
        /// Upload contract PDF document
        /// رفع مستند PDF للعقد
        /// </summary>
        [HttpPost("{contractId}/upload-pdf")]
        [Log(Title = "Contract PDF", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UploadPdf(Guid contractId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return ToResponse(ResultCode.PARAM_ERROR, "File cannot be empty");
                }

                // Validate it's a PDF
                var ext = global::System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                {
                    return ToResponse(ResultCode.PARAM_ERROR, "Only PDF files are allowed");
                }

                // Max 25MB
                if (file.Length > 25 * 1024 * 1024)
                {
                    return ToResponse(ResultCode.PARAM_ERROR, "File size must not exceed 25MB");
                }

                // Verify contract exists
                var contract = _contractService.GetById(contractId);
                if (contract == null)
                {
                    return ToResponse(ResultCode.FAIL, "Contract not found");
                }

                var dir = GetContractPdfDirectory();

                // Delete existing PDF if any
                var existingFiles = global::System.IO.Directory.GetFiles(dir, $"{contractId}.*");
                foreach (var f in existingFiles)
                {
                    global::System.IO.File.Delete(f);
                }

                // Save new PDF
                var fileName = $"{contractId}.pdf";
                var filePath = global::System.IO.Path.Combine(dir, fileName);

                using (var stream = new global::System.IO.FileStream(filePath, global::System.IO.FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return SUCCESS(new
                {
                    fileName = file.FileName,
                    fileSize = file.Length,
                    uploadedAt = DateTime.Now,
                    uploadedBy = HttpContext.GetName()
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get contract PDF info
        /// الحصول على معلومات مستند PDF للعقد
        /// </summary>
        [HttpGet("{contractId}/pdf-info")]
        public IActionResult GetPdfInfo(Guid contractId)
        {
            try
            {
                var dir = GetContractPdfDirectory();
                var filePath = global::System.IO.Path.Combine(dir, $"{contractId}.pdf");

                if (global::System.IO.File.Exists(filePath))
                {
                    var fileInfo = new global::System.IO.FileInfo(filePath);
                    return SUCCESS(new
                    {
                        hasPdf = true,
                        fileName = $"{contractId}.pdf",
                        fileSize = fileInfo.Length,
                        uploadedAt = fileInfo.LastWriteTimeUtc,
                    });
                }

                return SUCCESS(new { hasPdf = false });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Download contract PDF
        /// تحميل مستند PDF للعقد
        /// </summary>
        [HttpGet("{contractId}/download-pdf")]
        public IActionResult DownloadPdf(Guid contractId)
        {
            try
            {
                var dir = GetContractPdfDirectory();
                var filePath = global::System.IO.Path.Combine(dir, $"{contractId}.pdf");

                if (!global::System.IO.File.Exists(filePath))
                {
                    return ToResponse(ResultCode.FAIL, "PDF document not found");
                }

                var fileBytes = global::System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf", $"contract-{contractId}.pdf");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// View contract PDF in browser
        /// عرض مستند PDF في المتصفح
        /// </summary>
        [HttpGet("{contractId}/view-pdf")]
        [AllowAnonymous]
        public IActionResult ViewPdf(Guid contractId)
        {
            try
            {
                var dir = GetContractPdfDirectory();
                var filePath = global::System.IO.Path.Combine(dir, $"{contractId}.pdf");

                if (!global::System.IO.File.Exists(filePath))
                {
                    return NotFound("PDF document not found");
                }

                var fileBytes = global::System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete contract PDF
        /// حذف مستند PDF للعقد
        /// </summary>
        [HttpDelete("{contractId}/delete-pdf")]
        [Log(Title = "Contract PDF", BusinessType = BusinessType.DELETE)]
        public IActionResult DeletePdf(Guid contractId)
        {
            try
            {
                var dir = GetContractPdfDirectory();
                var filePath = global::System.IO.Path.Combine(dir, $"{contractId}.pdf");

                if (!global::System.IO.File.Exists(filePath))
                {
                    return ToResponse(ResultCode.FAIL, "PDF document not found");
                }

                global::System.IO.File.Delete(filePath);
                return SUCCESS("PDF deleted successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }
    }

    /// <summary>
    /// DTO for renew contract
    /// </summary>
    public class RenewContractDto
    {
        public Guid ContractId { get; set; }
        public DateTime NewEndDate { get; set; }
    }
}
