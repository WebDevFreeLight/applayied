 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Document Query DTO
    /// </summary>
    public class DocumentQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DocTypeId { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string? IsVerified { get; set; }
        public DateTime? ExpiryDateFrom { get; set; }
        public DateTime? ExpiryDateTo { get; set; }
        public bool? ShowExpired { get; set; }
        public bool? ShowArchived { get; set; }
    }

    /// <summary>
    /// Document Type DTO
    /// </summary>
    public class DocumentTypeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocTypeId { get; set; }
        
        [Required(ErrorMessage = "Type code is required")]
        public string TypeCode { get; set; }
        
        [Required(ErrorMessage = "Type name is required")]
        public string TypeName { get; set; }
        
        public string TypeNameAr { get; set; }
        public string Category { get; set; }
        public bool IsRequired { get; set; }
        public bool HasExpiry { get; set; }
        public int? ReminderDays { get; set; }
        public string AllowedFormats { get; set; } // pdf,jpg,png
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MaxFileSize { get; set; } // in bytes
        public bool RequiresVerification { get; set; }
        public string VerificationFields { get; set; } // JSON of fields to verify
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Employee Document DTO
    /// </summary>
    public class EmployeeDocumentDto:SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DocumentId { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Document type is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocTypeId { get; set; }
        
        [Required(ErrorMessage = "Document name is required")]
        public string DocumentName { get; set; }
        
        public string DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? FileSize { get; set; }
        public string Status { get; set; } // valid, expired, pending, archived
        public string IsVerified { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string VerificationComments { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string Metadata { get; set; } // JSON metadata
        public bool IsArchived { get; set; }
        public DateTime? ArchivedDate { get; set; }
        public string Notes { get; set; }
        
        // Display properties
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DocTypeName { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public string PreviewUrl { get; set; }
        public string DownloadUrl { get; set; }
        
        // Additional properties
        public string DocumentType { get; set; }
        public bool IsActive { get; set; }
        public bool IsMandatory { get; set; }
        public int? ReminderDays { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
        public int? DaysToExpiry => ExpiryDate.HasValue ? (int?)(ExpiryDate.Value - DateTime.Now).TotalDays : null;
        public DateTime Create_time { get; set; }
        public string Create_by { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerificationNotes { get; set; }
    }

    /// <summary>
    /// Document Upload DTO
    /// </summary>
    public class DocumentUploadDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Document type is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocTypeId { get; set; }
        
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; }
        
        public string DocumentName { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string Notes { get; set; }
        public bool AutoExtractMetadata { get; set; }
    }

    /// <summary>
    /// Multi Document Upload DTO
    /// </summary>
    public class MultiDocumentUploadDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }
        
        [Required(ErrorMessage = "Files are required")]
        public List<IFormFile> Files { get; set; }
        
        public Dictionary<string, DocumentMetadata> FileMetadata { get; set; }
    }

    public class DocumentMetadata
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocTypeId { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; }
        public string Tags { get; set; }
    }

    /// <summary>
    /// Document Category DTO
    /// </summary>
    public class DocumentCategoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CategoryId { get; set; }
        
        [Required(ErrorMessage = "Category code is required")]
        public string CategoryCode { get; set; }
        
        [Required(ErrorMessage = "Category name is required")]
        public string CategoryName { get; set; }
        
        public string CategoryNameAr { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int DisplayOrder { get; set; }
        public string AllowedFormats { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Document Compliance DTO
    /// </summary>
    public class DocumentComplianceDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public int TotalRequired { get; set; }
        public int TotalUploaded { get; set; }
        public int TotalVerified { get; set; }
        public int TotalExpired { get; set; }
        public int TotalExpiringSoon { get; set; }
        public decimal CompliancePercentage { get; set; }
        public List<DocumentComplianceItem> Items { get; set; }
        public string ComplianceStatus { get; set; } // compliant, partial, non_compliant
    }

    public class DocumentComplianceItem
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public bool IsRequired { get; set; }
        public bool IsUploaded { get; set; }
        public bool IsVerified { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Expiring Document DTO
    /// </summary>
    public class ExpiringDocumentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocumentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string Department { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string Status { get; set; }
        public string EmployeeEmail { get; set; }
        public string ManagerEmail { get; set; }
        public bool ReminderSent { get; set; }
        public DateTime? LastReminderDate { get; set; }
    }

    /// <summary>
    /// Missing Document DTO
    /// </summary>
    public class MissingDocumentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string Department { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public string Category { get; set; }
        public bool IsRequired { get; set; }
        public DateTime? DueDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    /// <summary>
    /// Document Template DTO
    /// </summary>
    public class DocumentTemplateDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }
        
        [Required(ErrorMessage = "Template name is required")]
        public string TemplateName { get; set; }
        
        public string TemplateNameAr { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string TemplateContent { get; set; } // HTML or Word template
        public string Variables { get; set; } // JSON array of variable names
        public string FileFormat { get; set; } // pdf, docx
        public bool IsActive { get; set; }
        public string SampleData { get; set; } // JSON sample data
    }

    /// <summary>
    /// Bulk Upload Result DTO
    /// </summary>
    public class BulkUploadResultDto
    {
        public int TotalFiles { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<UploadResult> Results { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    public class UploadResult
    {
        public string FileName { get; set; }
        public bool Success { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DocumentId { get; set; }
        public string Error { get; set; }
        public string EmployeeName { get; set; }
    }

    /// <summary>
    /// Bulk Document Upload DTO
    /// </summary>
    public class BulkDocumentUploadDto
    {
        [Required(ErrorMessage = "Files are required")]
        public List<IFormFile> Files { get; set; }
        
        public string Category { get; set; }
        public bool AutoAssignToEmployees { get; set; } // Based on file naming convention
        public string NamingPattern { get; set; } // e.g., {EmployeeCode}_{DocType}_{Date}
        public bool AutoExtractMetadata { get; set; }
        public bool SkipDuplicates { get; set; }
    }

    /// <summary>
    /// Document Report Query DTO
    /// </summary>
    public class DocumentReportQueryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public string Category { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportType { get; set; } // compliance, expiry, audit
        public string Format { get; set; } // pdf, excel
    }

    /// <summary>
    /// Document Statistics DTO
    /// </summary>
    public class DocumentStatisticsDto
    {
        public int TotalDocuments { get; set; }
        public int TotalEmployees { get; set; }
        public int VerifiedDocuments { get; set; }
        public int PendingVerification { get; set; }
        public int ExpiredDocuments { get; set; }
        public int ExpiringSoon { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TotalStorageSize { get; set; }
        public Dictionary<string, int> DocumentsByCategory { get; set; }
        public Dictionary<string, int> DocumentsByType { get; set; }
        public Dictionary<string, int> DocumentsByStatus { get; set; }
        public List<MonthlyUploadTrend> UploadTrends { get; set; }
        public decimal ComplianceRate { get; set; }
        public List<TopDocumentType> TopDocumentTypes { get; set; }
    }

    public class MonthlyUploadTrend
    {
        public string Month { get; set; }
        public int Count { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TotalSize { get; set; }
    }

    public class TopDocumentType
    {
        public string TypeName { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Document Audit DTO
    /// </summary>
    public class DocumentAuditDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AuditId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocumentId { get; set; }
        public string Action { get; set; } // upload, update, verify, delete, download, view
        public string PerformedBy { get; set; }
        public DateTime ActionDate { get; set; }
        public string Details { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public string OldValues { get; set; } // JSON
        public string NewValues { get; set; } // JSON
    }

    /// <summary>
    /// Document OCR Result DTO
    /// </summary>
    public class DocumentOCRResultDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocumentId { get; set; }
        public bool Success { get; set; }
        public string ExtractedText { get; set; }
        public Dictionary<string, string> ExtractedFields { get; set; }
        public decimal Confidence { get; set; }
        public string Language { get; set; }
        public List<string> DetectedKeywords { get; set; }
        public string ProcessingTime { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Document Share DTO
    /// </summary>
    public class DocumentShareDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShareId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocumentId { get; set; }
        public string SharedWith { get; set; }
        public string ShareType { get; set; } // view, download, edit
        public DateTime SharedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ShareLink { get; set; }
        public int AccessCount { get; set; }
        public DateTime? LastAccessDate { get; set; }
    }
}

