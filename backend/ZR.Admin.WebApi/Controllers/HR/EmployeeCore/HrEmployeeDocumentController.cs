using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;
using ZR.Admin.WebApi.Extensions;

namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Employee Core - Document Controller
    /// Handles ONLY: EmployeeDocument (file upload, download, management)
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/document")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrEmployeeDocumentController : BaseController
    {
        private readonly IHrEmployeeDocumentService _documentService;
        private readonly string _uploadBasePath;

        public HrEmployeeDocumentController(
            IHrEmployeeDocumentService documentService,
            IConfiguration configuration)
        {
            _documentService = documentService;
            // Get upload path from configuration or use default
            _uploadBasePath = configuration["FileUpload:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads", "employee-documents");

            // Ensure directory exists
            if (!Directory.Exists(_uploadBasePath))
            {
                Directory.CreateDirectory(_uploadBasePath);
            }
        }

        /// <summary>
        /// Get all documents for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>List of all documents for this employee</returns>
        [HttpGet("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:document:query")]
        public async Task<IActionResult> GetDocuments(long employeeId)
        {
            var documents = await _documentService.GetListAsync(x => x.EmployeeId == employeeId);
            var documentsList = documents.OrderByDescending(d => d.UploadDate).ToList();

            return SUCCESS(documentsList);
        }

        /// <summary>
        /// Get documents with filtering
        /// </summary>
        /// <param name="query">Filter criteria</param>
        /// <returns>Filtered list of documents</returns>
        [HttpPost("{employeeId:long}/filter")]
        [ActionPermissionFilter("hr:employeecore:document:query")]
        public async Task<IActionResult> GetDocumentsFiltered(long employeeId, [FromBody] DocumentQueryDto query)
        {
            var documents = await _documentService.GetListAsync(x => x.EmployeeId == employeeId);
            var filtered = documents.AsEnumerable();

            // Apply filters
            if (query.DocumentTypeDictCode.HasValue)
            {
                filtered = filtered.Where(d => d.DocumentTypeDictCode == query.DocumentTypeDictCode.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                filtered = filtered.Where(d =>
                    d.DocumentName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (d.DocumentNumber != null && d.DocumentNumber.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (query.IsVerified.HasValue)
            {
                filtered = filtered.Where(d => d.IsVerified == query.IsVerified.Value);
            }

            if (!string.IsNullOrEmpty(query.Status))
            {
                filtered = filtered.Where(d => d.Status == query.Status);
            }

            // Check expiry
            if (query.IsExpired.HasValue && query.IsExpired.Value)
            {
                filtered = filtered.Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < DateTime.Now);
            }

            if (query.IsExpiringSoon.HasValue && query.IsExpiringSoon.Value)
            {
                var thirtyDaysFromNow = DateTime.Now.AddDays(30);
                filtered = filtered.Where(d => d.ExpiryDate.HasValue &&
                    d.ExpiryDate.Value >= DateTime.Now &&
                    d.ExpiryDate.Value <= thirtyDaysFromNow);
            }

            var result = filtered.OrderByDescending(d => d.UploadDate).ToList();
            return SUCCESS(result);
        }

        /// <summary>
        /// Upload a new document
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="file">File to upload</param>
        /// <param name="dto">Document metadata</param>
        /// <returns>Created document ID</returns>
        [HttpPost("{employeeId:long}/upload")]
        [ActionPermissionFilter("hr:employeecore:document:add")]
        [Log(Title = "Upload Employee Document", BusinessType = BusinessType.INSERT)]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<IActionResult> UploadDocument(
            long employeeId,
            [FromForm] IFormFile file,
            [FromForm] string documentName,
            [FromForm] long? documentTypeDictCode,
            [FromForm] string documentNumber,
            [FromForm] DateTime? issueDate,
            [FromForm] DateTime? expiryDate,
            [FromForm] string issuingAuthority,
            [FromForm] string notes)
        {
            if (file == null || file.Length == 0)
            {
                return ToResponse(ResultCode.FAIL, "No file uploaded");
            }

            // Validate file size (10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return ToResponse(ResultCode.FAIL, "File size must be less than 10MB");
            }

            // Validate file extension
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return ToResponse(ResultCode.FAIL, $"File type {fileExtension} is not allowed");
            }

            try
            {
                // Create employee-specific directory
                var employeeDir = Path.Combine(_uploadBasePath, employeeId.ToString());
                if (!Directory.Exists(employeeDir))
                {
                    Directory.CreateDirectory(employeeDir);
                }

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(employeeDir, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create document record
                var document = new Model.HR.EmployeeCore.HrEmployeeDocument
                {
                    EmployeeId = employeeId,
                    DocumentTypeDictCode = documentTypeDictCode,
                    DocumentName = string.IsNullOrEmpty(documentName) ? file.FileName : documentName,
                    DocumentNumber = documentNumber,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    MimeType = file.ContentType,
                    UploadDate = DateTime.Now,
                    IssueDate = issueDate,
                    ExpiryDate = expiryDate,
                    IssuingAuthority = issuingAuthority,
                    Notes = notes,
                    IsVerified = false,
                    Status = "Active",
                    Create_by = HttpContext.GetName(),
                    Create_time = DateTime.Now
                };

                var documentId = await _documentService.AddAsync(document);

                return SUCCESS(new { DocumentId = documentId, Message = "Document uploaded successfully" });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to upload document: {ex.Message}");
            }
        }

        /// <summary>
        /// Download a document
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>File download</returns>
        [HttpGet("{employeeId:long}/{documentId:long}/download")]
        [ActionPermissionFilter("hr:employeecore:document:query")]
        public async Task<IActionResult> DownloadDocument(long employeeId, long documentId)
        {
            var document = await _documentService.GetByIdAsync(documentId);

            if (document == null || document.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.FAIL, "Document not found");
            }

            if (!global::System.IO.File.Exists(document.FilePath))
            {
                return ToResponse(ResultCode.FAIL, "File not found on server");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(document.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, document.MimeType ?? "application/octet-stream", document.FileName);
        }

        /// <summary>
        /// Update document metadata
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="dto">Updated metadata</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/{documentId:long}")]
        [ActionPermissionFilter("hr:employeecore:document:edit")]
        [Log(Title = "Update Document Metadata", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateDocument(long employeeId, long documentId, [FromBody] UpdateDocumentDto dto)
        {
            var document = await _documentService.GetByIdAsync(documentId);

            if (document == null || document.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.FAIL, "Document not found");
            }

            if (dto.DocumentTypeDictCode.HasValue)
                document.DocumentTypeDictCode = dto.DocumentTypeDictCode;

            if (dto.DocumentName != null)
                document.DocumentName = dto.DocumentName;

            if (dto.DocumentNumber != null)
                document.DocumentNumber = dto.DocumentNumber;

            if (dto.IssueDate.HasValue)
                document.IssueDate = dto.IssueDate;

            if (dto.ExpiryDate.HasValue)
                document.ExpiryDate = dto.ExpiryDate;

            if (dto.IssuingAuthority != null)
                document.IssuingAuthority = dto.IssuingAuthority;

            if (dto.Notes != null)
                document.Notes = dto.Notes;

            if (dto.IsVerified.HasValue)
            {
                document.IsVerified = dto.IsVerified.Value;
                if (dto.IsVerified.Value)
                {
                    document.VerifiedBy = HttpContext.GetName();
                    document.VerifiedDate = DateTime.Now;
                }
            }

            if (dto.Status != null)
                document.Status = dto.Status;

            document.Update_by = HttpContext.GetName();
            document.Update_time = DateTime.Now;

            await _documentService.UpdateAsync(document);

            return SUCCESS(document);
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("{employeeId:long}/{documentId:long}")]
        [ActionPermissionFilter("hr:employeecore:document:delete")]
        [Log(Title = "Delete Employee Document", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteDocument(long employeeId, long documentId)
        {
            var document = await _documentService.GetByIdAsync(documentId);

            if (document == null || document.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.FAIL, "Document not found");
            }

            try
            {
                // Delete physical file
                if (global::System.IO.File.Exists(document.FilePath))
                {
                    global::System.IO.File.Delete(document.FilePath);
                }

                // Delete database record
                await _documentService.DeleteAsync(new long[] { documentId });

                return SUCCESS(new { Message = "Document deleted successfully" });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to delete document: {ex.Message}");
            }
        }

        /// <summary>
        /// Get document statistics for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Document statistics</returns>
        [HttpGet("{employeeId:long}/statistics")]
        [ActionPermissionFilter("hr:employeecore:document:query")]
        public async Task<IActionResult> GetDocumentStatistics(long employeeId)
        {
            var documents = await _documentService.GetListAsync(x => x.EmployeeId == employeeId);
            var documentsList = documents.ToList();

            var now = DateTime.Now;
            var thirtyDaysFromNow = now.AddDays(30);

            var stats = new
            {
                TotalDocuments = documentsList.Count,
                VerifiedDocuments = documentsList.Count(d => d.IsVerified),
                ExpiredDocuments = documentsList.Count(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < now),
                ExpiringSoon = documentsList.Count(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value >= now && d.ExpiryDate.Value <= thirtyDaysFromNow),
                TotalSize = documentsList.Sum(d => d.FileSize),
                DocumentsByType = documentsList.GroupBy(d => d.DocumentTypeDictCode)
                    .Select(g => new { TypeCode = g.Key, Count = g.Count() })
                    .ToList()
            };

            return SUCCESS(stats);
        }
    }
}
