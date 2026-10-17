
using Infrastructure.Converter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;
using Infrastructure.Controllers;
using Infrastructure.Model;
using Infrastructure.Extensions;
using Newtonsoft.Json;
using SqlSugar;

namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Document Management Controller
    /// </summary>
    [Route("api/hr/documents")]
    [ApiExplorerSettings(GroupName = "hr")]

    [ApiController]
    public class DocumentController : BaseController
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }




        #region Document Types

        /// <summary>
        /// Get document types
        /// الحصول على أنواع المستندات
        /// </summary>
        [HttpGet("types")]
        [ActionPermissionFilter(Permission = "hr:documents:types")]
        public async Task<IActionResult> GetDocumentTypes([FromQuery] bool? isActive = null)
        {
            var types = await _documentService.GetDocumentTypesAsync(isActive);
            return SUCCESS(types);
        }

        /// <summary>
        /// Get document type by ID
        /// </summary>
        [HttpGet("types/{docTypeId}")]
        [ActionPermissionFilter(Permission = "hr:documents:types")]
        public async Task<IActionResult> GetDocumentType(long docTypeId)
        {
            var type = await _documentService.GetDocumentTypeByIdAsync(docTypeId);
            if (type == null)
                return ToResponse(ResultCode.CUSTOM_ERROR, "Document type not found");
            
            return SUCCESS(type);
        }

        /// <summary>
        /// Create document type
        /// </summary>
        [HttpPost("types")]
        [ActionPermissionFilter(Permission = "hr:documents:types:add")]
        public async Task<IActionResult> CreateDocumentType([FromBody] DocumentTypeDto docType)
        {
            if (!ModelState.IsValid){
                return ToResponse(ResultCode.PARAM_ERROR, "Make fill Fields");
            }
            var result = await _documentService.CreateDocumentTypeAsync(docType);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update document type
        /// </summary>
        [HttpPut("types/{docTypeId}")]
        [ActionPermissionFilter(Permission = "hr:documents:types:edit")]
        public async Task<IActionResult> UpdateDocumentType(long docTypeId, [FromBody] DocumentTypeDto docType)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");

            docType.DocTypeId = docTypeId;
            var result = await _documentService.UpdateDocumentTypeAsync(docType);
            
            return result ? SUCCESS("Document type updated successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to update document type");
        }

        /// <summary>
        /// Delete document type
        /// </summary>
        [HttpDelete("types/{docTypeId}")]
        [ActionPermissionFilter(Permission = "hr:documents:types:delete")]
        public async Task<IActionResult> DeleteDocumentType(long docTypeId)
        {
            var result = await _documentService.DeleteDocumentTypeAsync(docTypeId);
            
            return result ? SUCCESS("Document type deleted successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to delete document type");
        }

        #endregion

        #region Employee Documents

        /// <summary>
        /// Get employee documents
        /// الحصول على مستندات الموظفين
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "hr:documents:list")]
        public async Task<IActionResult> GetDocuments([FromQuery] DocumentQueryDto query)
        {
            var documents = await _documentService.GetEmployeeDocumentsAsync(query);
            return SUCCESS(documents);
        }

        /// <summary>
        /// Get documents by employee
        /// الحصول على مستندات موظف معين
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [ActionPermissionFilter(Permission = "hr:documents:list")]
        public async Task<IActionResult> GetEmployeeDocuments(long employeeId, [FromQuery] string category = null)
        {
            var documents = await _documentService.GetDocumentsByEmployeeAsync(employeeId, category);
            return SUCCESS(documents);
        }

        /// <summary>
        /// Get document by ID
        /// </summary>
        [HttpGet("{documentId}")]
        [ActionPermissionFilter(Permission = "hr:documents:view")]
        public async Task<IActionResult> GetDocument(long documentId)
        {
            var document = await _documentService.GetDocumentByIdAsync(documentId);
            if (document == null)
                return ToResponse(ResultCode.CUSTOM_ERROR, "Document not found");
            
            return SUCCESS(document);
        }

        /// <summary>
        /// Upload document
        /// رفع مستند
        /// </summary>
        [HttpPost("upload")]
        [ActionPermissionFilter(Permission = "hr:documents:upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadDto upload)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");

            try
            {
                var document = await _documentService.UploadDocumentAsync(upload);
                return SUCCESS(document, "Document uploaded successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Upload multiple documents
        /// رفع مستندات متعددة
        /// </summary>
        [HttpPost("upload-multiple")]
        [ActionPermissionFilter(Permission = "hr:documents:upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMultipleDocuments([FromForm] MultiDocumentUploadDto uploads)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");

            try
            {
                var documents = await _documentService.UploadMultipleDocumentsAsync(uploads);
                return SUCCESS(documents, $"{documents.Count} documents uploaded successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update document
        /// </summary>
        [HttpPut("{documentId}")]
        [ActionPermissionFilter(Permission = "hr:documents:edit")]
        public async Task<IActionResult> UpdateDocument(long documentId, [FromBody] EmployeeDocumentDto document)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");

            document.DocumentId = documentId;
            var result = await _documentService.UpdateDocumentAsync(document);
            
            return result ? SUCCESS("Document updated successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to update document");
        }

        /// <summary>
        /// Delete document
        /// </summary>
        [HttpDelete("{documentId}")]
        [ActionPermissionFilter(Permission = "hr:documents:delete")]
        public async Task<IActionResult> DeleteDocument(long documentId)
        {
            var result = await _documentService.DeleteDocumentAsync(documentId);
            
            return result ? SUCCESS("Document deleted successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to delete document");
        }

        /// <summary>
        /// Verify document
        /// التحقق من المستند
        /// </summary>
        [HttpPost("{documentId}/verify")]
        [ActionPermissionFilter(Permission = "hr:documents:verify")]
        public async Task<IActionResult> VerifyDocument(long documentId, 
            [FromBody] DocumentVerificationDto verification)
        {
            var result = await _documentService.VerifyDocumentAsync(
                documentId, 
                verification.VerifiedBy ?? HttpContext.GetName(), 
                verification.IsValid, 
                verification.Comments);
            
            return result ? SUCCESS("Document verified successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to verify document");
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Download document
        /// تحميل المستند
        /// </summary>
        [HttpGet("{documentId}/download")]
        [ActionPermissionFilter(Permission = "hr:documents:download")]
        public async Task<IActionResult> DownloadDocument(long documentId)
        {
            try
            {
                var fileBytes = await _documentService.DownloadDocumentAsync(documentId);
                var document = await _documentService.GetDocumentByIdAsync(documentId);
                
                return File(fileBytes, "application/octet-stream", document.FileName);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Preview document
        /// معاينة المستند
        /// </summary>
        [HttpGet("{documentId}/preview")]
        [ActionPermissionFilter(Permission = "hr:documents:view")]
        public async Task<IActionResult> PreviewDocument(long documentId)
        {
            try
            {
                var fileBytes = await _documentService.DownloadDocumentAsync(documentId);
                var document = await _documentService.GetDocumentByIdAsync(documentId);
                
                var contentType = document.FileType switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };
                
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Move document to another employee
        /// </summary>
        [HttpPost("{documentId}/move")]
        [ActionPermissionFilter(Permission = "hr:documents:edit")]
        public async Task<IActionResult> MoveDocument(long documentId, [FromBody] MoveDocumentDto move)
        {
            var result = await _documentService.MoveDocumentAsync(documentId, move.NewEmployeeId);
            
            return result ? SUCCESS("Document moved successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to move document");
        }

        /// <summary>
        /// Archive document
        /// </summary>
        [HttpPost("{documentId}/archive")]
        [ActionPermissionFilter(Permission = "hr:documents:edit")]
        public async Task<IActionResult> ArchiveDocument(long documentId)
        {
            var result = await _documentService.ArchiveDocumentAsync(documentId);
            
            return result ? SUCCESS("Document archived successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to archive document");
        }

        /// <summary>
        /// Restore archived document
        /// </summary>
        [HttpPost("{documentId}/restore")]
        [ActionPermissionFilter(Permission = "hr:documents:edit")]
        public async Task<IActionResult> RestoreDocument(long documentId)
        {
            var result = await _documentService.RestoreDocumentAsync(documentId);
            
            return result ? SUCCESS("Document restored successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to restore document");
        }

        #endregion

        #region Categories

        /// <summary>
        /// Get document categories
        /// الحصول على فئات المستندات
        /// </summary>
        [HttpGet("categories")]
        [ActionPermissionFilter(Permission = "hr:documents:list")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _documentService.GetDocumentCategoriesAsync();
            return SUCCESS(categories);
        }

        /// <summary>
        /// Create category
        /// </summary>
        [HttpPost("categories")]
        [ActionPermissionFilter(Permission = "hr:documents:categories:add")]
        public async Task<IActionResult> CreateCategory([FromBody] DocumentCategoryDto category)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");

            var result = await _documentService.CreateCategoryAsync(category);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update category
        /// </summary>
        [HttpPut("categories/{categoryId}")]
        [ActionPermissionFilter(Permission = "hr:documents:categories:edit")]
        public async Task<IActionResult> UpdateCategory(long categoryId, [FromBody] DocumentCategoryDto category)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");

            category.CategoryId = categoryId;
            var result = await _documentService.UpdateCategoryAsync(category);
            
            return result ? SUCCESS("Category updated successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to update category");
        }

        #endregion

        #region Compliance & Validation

        /// <summary>
        /// Check employee compliance
        /// التحقق من امتثال الموظف
        /// </summary>
        [HttpGet("compliance/{employeeId}")]
        [ActionPermissionFilter(Permission = "hr:documents:compliance")]
        public async Task<IActionResult> CheckCompliance(long employeeId)
        {
            var compliance = await _documentService.CheckEmployeeComplianceAsync(employeeId);
            return SUCCESS(compliance);
        }

        /// <summary>
        /// Get expiring documents
        /// الحصول على المستندات المنتهية الصلاحية
        /// </summary>
        [HttpGet("expiring")]
        [ActionPermissionFilter(Permission = "hr:documents:list")]
        public async Task<IActionResult> GetExpiringDocuments([FromQuery] int days = 30)
        {
            var documents = await _documentService.GetExpiringDocumentsAsync(days);
            return SUCCESS(documents);
        }

        /// <summary>
        /// Get missing required documents
        /// </summary>
        [HttpGet("missing")]
        [ActionPermissionFilter(Permission = "hr:documents:compliance")]
        public async Task<IActionResult> GetMissingDocuments([FromQuery] long? employeeId = null)
        {
            var documents = await _documentService.GetMissingRequiredDocumentsAsync(employeeId);
            return SUCCESS(documents);
        }

        /// <summary>
        /// Send expiry reminder
        /// </summary>
        [HttpPost("{documentId}/remind")]
        [ActionPermissionFilter(Permission = "hr:documents:remind")]
        public async Task<IActionResult> SendExpiryReminder(long documentId)
        {
            var result = await _documentService.SendExpiryReminderAsync(documentId);
            
            return result ? SUCCESS("Reminder sent successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to send reminder");
        }

        /// <summary>
        /// Send bulk expiry reminders
        /// </summary>
        [HttpPost("remind-bulk")]
        [ActionPermissionFilter(Permission = "hr:documents:remind")]
        public async Task<IActionResult> SendBulkReminders()
        {
            var result = await _documentService.SendBulkExpiryRemindersAsync();
            
            return result ? SUCCESS("Reminders sent successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to send reminders");
        }

        #endregion

        #region Templates

        /// <summary>
        /// Get document templates
        /// </summary>
        [HttpGet("templates")]
        [ActionPermissionFilter(Permission = "hr:documents:templates")]
        public async Task<IActionResult> GetTemplates([FromQuery] string category = null)
        {
            var templates = await _documentService.GetDocumentTemplatesAsync(category);
            return SUCCESS(templates);
        }

        /// <summary>
        /// Create template
        /// </summary>
        [HttpPost("templates")]
        [ActionPermissionFilter(Permission = "hr:documents:templates:add")]
        public async Task<IActionResult> CreateTemplate([FromBody] DocumentTemplateDto template)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");

            var result = await _documentService.CreateTemplateAsync(template);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate document from template
        /// </summary>
        [HttpPost("templates/{templateId}/generate")]
        [ActionPermissionFilter(Permission = "hr:documents:templates:use")]
        public async Task<IActionResult> GenerateFromTemplate(long templateId, 
            [FromBody] Dictionary<string, string> data)
        {
            var fileBytes = await _documentService.GenerateDocumentFromTemplateAsync(templateId, data);
            return File(fileBytes, "application/octet-stream", "generated_document.pdf");
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Bulk upload documents
        /// رفع مستندات بالجملة
        /// </summary>
        [HttpPost("bulk-upload")]
        [ActionPermissionFilter(Permission = "hr:documents:bulk")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BulkUpload([FromForm] BulkDocumentUploadDto bulkUpload)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");

            var result = await _documentService.BulkUploadDocumentsAsync(bulkUpload);
            return SUCCESS(result);
        }

        /// <summary>
        /// Bulk verify documents
        /// </summary>
        [HttpPost("bulk-verify")]
        [ActionPermissionFilter(Permission = "hr:documents:verify")]
        public async Task<IActionResult> BulkVerify([FromBody] BulkVerifyDto bulkVerify)
        {
            var result = await _documentService.BulkVerifyDocumentsAsync(
                bulkVerify.DocumentIds, 
                bulkVerify.VerifiedBy ?? HttpContext.GetName());
            
            return result ? SUCCESS("Documents verified successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to verify documents");
        }

        /// <summary>
        /// Export document report
        /// </summary>
        [HttpPost("export")]
        [ActionPermissionFilter(Permission = "hr:documents:export")]
        public async Task<IActionResult> ExportReport([FromBody] DocumentReportQueryDto query)
        {
            var reportBytes = await _documentService.ExportDocumentReportAsync(query);
            var fileName = $"document_report_{DateTime.Now:yyyyMMdd}.{query.Format ?? "pdf"}";
            
            return File(reportBytes, "application/octet-stream", fileName);
        }

        /// <summary>
        /// Bulk delete documents
        /// </summary>
        [HttpPost("bulk-delete")]
        [ActionPermissionFilter(Permission = "hr:documents:delete")]
        public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteDto bulkDelete)
        {
            var result = await _documentService.BulkDeleteDocumentsAsync(bulkDelete.DocumentIds);
            
            return result ? SUCCESS("Documents deleted successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to delete documents");
        }

        #endregion

        #region Search & Analytics

        /// <summary>
        /// Search documents
        /// البحث في المستندات
        /// </summary>
        [HttpGet("search")]
        [ActionPermissionFilter(Permission = "hr:documents:list")]
        public async Task<IActionResult> SearchDocuments([FromQuery] string searchTerm, 
            [FromQuery] string category = null)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return ToResponse(ResultCode.PARAM_ERROR, "Search term is required");

            var documents = await _documentService.SearchDocumentsAsync(searchTerm, category);
            return SUCCESS(documents);
        }

        /// <summary>
        /// Get document statistics
        /// إحصائيات المستندات
        /// </summary>
        [HttpGet("statistics")]
        [ActionPermissionFilter(Permission = "hr:documents:stats")]
        public async Task<IActionResult> GetStatistics([FromQuery] long? employeeId = null, 
            [FromQuery] long? departmentId = null)
        {
            var stats = await _documentService.GetDocumentStatisticsAsync(employeeId, departmentId);
            return SUCCESS(stats);
        }

        /// <summary>
        /// Get document audit trail
        /// </summary>
        [HttpGet("{documentId}/audit")]
        [ActionPermissionFilter(Permission = "hr:documents:audit")]
        public async Task<IActionResult> GetAuditTrail(long documentId)
        {
            var audit = await _documentService.GetDocumentAuditTrailAsync(documentId);
            return SUCCESS(audit);
        }

        #endregion

        #region OCR & Processing

        /// <summary>
        /// Process document with OCR
        /// </summary>
        [HttpPost("{documentId}/ocr")]
        [ActionPermissionFilter(Permission = "hr:documents:ocr")]
        public async Task<IActionResult> ProcessOCR(long documentId)
        {
            try
            {
                var result = await _documentService.ProcessDocumentOCRAsync(documentId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Extract and save metadata
        /// </summary>
        [HttpPost("{documentId}/extract-metadata")]
        [ActionPermissionFilter(Permission = "hr:documents:edit")]
        public async Task<IActionResult> ExtractMetadata(long documentId)
        {
            var result = await _documentService.ExtractAndSaveMetadataAsync(documentId);
            
            return result ? SUCCESS("Metadata extracted successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to extract metadata");
        }

        #endregion

        #region Security & Access

        /// <summary>
        /// Check document access
        /// </summary>
        [HttpGet("{documentId}/access")]
        [ActionPermissionFilter(Permission = "hr:documents:access")]
        public async Task<IActionResult> CheckAccess(long documentId)
        {
            var hasAccess = await _documentService.CheckDocumentAccessAsync(documentId, HttpContext.GetUId().ToString());
            return SUCCESS(new { HasAccess = hasAccess });
        }

        /// <summary>
        /// Share document
        /// </summary>
        [HttpPost("{documentId}/share")]
        [ActionPermissionFilter(Permission = "hr:documents:share")]
        public async Task<IActionResult> ShareDocument(long documentId, [FromBody] ShareDocumentDto share)
        {
            var result = await _documentService.ShareDocumentAsync(documentId, share.UserIds, share.ExpiryDate);
            
            return result ? SUCCESS("Document shared successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to share document");
        }

        /// <summary>
        /// Revoke document access
        /// </summary>
        [HttpDelete("{documentId}/access/{userId}")]
        [ActionPermissionFilter(Permission = "hr:documents:share")]
        public async Task<IActionResult> RevokeAccess(long documentId, string userId)
        {
            var result = await _documentService.RevokeDocumentAccessAsync(documentId, userId);
            
            return result ? SUCCESS("Access revoked successfully") 
                         : ToResponse(ResultCode.FAIL, "Failed to revoke access");
        }

        #endregion
    }

    #region DTOs for Controller

    public class DocumentVerificationDto
    {
        public bool IsValid { get; set; }
        public string Comments { get; set; }
        public string VerifiedBy { get; set; }
    }

    public class MoveDocumentDto
    {
        //[JsonConverter(typeof(ValueToStringConverter))]
        public long NewEmployeeId { get; set; }
    }

    public class BulkVerifyDto
    {
        public List<long> DocumentIds { get; set; }
        public string VerifiedBy { get; set; }
    }

    public class BulkDeleteDto
    {
        public List<long> DocumentIds { get; set; }
    }

    public class ShareDocumentDto
    {
        public List<string> UserIds { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    #endregion
}

