using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee documents - hr_employee_document
    /// </summary>
    [SugarTable("hr_employee_document")]
    public class HrEmployeeDocument : SysBase
    {
        [SugarColumn(ColumnName = "document_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DocumentId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "document_type_dict_code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DocumentTypeDictCode { get; set; }

        [SugarColumn(ColumnName = "document_name", Length = 250)]
        public string DocumentName { get; set; }

        [SugarColumn(ColumnName = "document_number", Length = 100, IsNullable = true)]
        public string DocumentNumber { get; set; }

        [SugarColumn(ColumnName = "file_name", Length = 250)]
        public string FileName { get; set; }

        [SugarColumn(ColumnName = "file_path", Length = 500)]
        public string FilePath { get; set; }

        [SugarColumn(ColumnName = "file_size")]
        public long FileSize { get; set; }

        [SugarColumn(ColumnName = "mime_type", Length = 100, IsNullable = true)]
        public string MimeType { get; set; }

        [SugarColumn(ColumnName = "upload_date")]
        public DateTime UploadDate { get; set; } = DateTime.Now;

        [SugarColumn(ColumnName = "issue_date", IsNullable = true)]
        public DateTime? IssueDate { get; set; }

        [SugarColumn(ColumnName = "expiry_date", IsNullable = true)]
        public DateTime? ExpiryDate { get; set; }

        [SugarColumn(ColumnName = "issuing_authority", Length = 200, IsNullable = true)]
        public string IssuingAuthority { get; set; }

        [SugarColumn(ColumnName = "is_verified")]
        public bool IsVerified { get; set; } = false;

        [SugarColumn(ColumnName = "verified_by", Length = 100, IsNullable = true)]
        public string VerifiedBy { get; set; }

        [SugarColumn(ColumnName = "verified_date", IsNullable = true)]
        public DateTime? VerifiedDate { get; set; }

        [SugarColumn(ColumnName = "notes", ColumnDataType = "text", IsNullable = true)]
        public string Notes { get; set; }

        [SugarColumn(ColumnName = "status", Length = 50, IsNullable = true)]
        public string Status { get; set; } = "Active";
    }
}
