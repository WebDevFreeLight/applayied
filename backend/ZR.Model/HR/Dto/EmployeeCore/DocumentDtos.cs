using System;
using Microsoft.AspNetCore.Http;

namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// DTO for uploading a new document
    /// </summary>
    public class UploadDocumentDto
    {
        public long? DocumentTypeDictCode { get; set; }
        public string DocumentName { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating document metadata
    /// </summary>
    public class UpdateDocumentDto
    {
        public long? DocumentTypeDictCode { get; set; }
        public string DocumentName { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string IssuingAuthority { get; set; }
        public string Notes { get; set; }
        public bool? IsVerified { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// DTO for document query/filter
    /// </summary>
    public class DocumentQueryDto
    {
        public long EmployeeId { get; set; }
        public long? DocumentTypeDictCode { get; set; }
        public string SearchTerm { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsExpiringSoon { get; set; } // Within 30 days
        public bool? IsVerified { get; set; }
        public string Status { get; set; }
    }
}
