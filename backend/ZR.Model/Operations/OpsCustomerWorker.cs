using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Workers under a Customer - عمال العميل
    /// </summary>
    [SugarTable("ops_customer_worker")]
    public class OpsCustomerWorker : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "worker_id", IsPrimaryKey = true, IsIdentity = false)]
        public long WorkerId { get; set; }

        /// <summary>
        /// Customer ID (FK -> arap_customer)
        /// </summary>
        [SugarColumn(ColumnName = "customer_id", Length = 50)]
        public string CustomerId { get; set; }

        /// <summary>
        /// Auto-generated code (WRK-YYYY-NNNNN)
        /// </summary>
        [SugarColumn(ColumnName = "worker_code", Length = 20)]
        public string WorkerCode { get; set; }

        /// <summary>
        /// Worker full name (EN)
        /// </summary>
        [SugarColumn(ColumnName = "full_name", Length = 200)]
        public string FullName { get; set; }

        /// <summary>
        /// Worker full name (AR)
        /// </summary>
        [SugarColumn(ColumnName = "full_name_ar", Length = 200, IsNullable = true)]
        public string FullNameAr { get; set; }

        /// <summary>
        /// National ID / Iqama number
        /// </summary>
        [SugarColumn(ColumnName = "national_id", Length = 20, IsNullable = true)]
        public string NationalId { get; set; }

        /// <summary>
        /// Passport number
        /// </summary>
        [SugarColumn(ColumnName = "passport_number", Length = 20, IsNullable = true)]
        public string PassportNumber { get; set; }

        /// <summary>
        /// Document type: NationalID, Iqama, Passport
        /// </summary>
        [SugarColumn(ColumnName = "document_type", Length = 20)]
        public string DocumentType { get; set; } = "NationalID";

        /// <summary>
        /// Contact phone
        /// </summary>
        [SugarColumn(ColumnName = "phone", Length = 20, IsNullable = true)]
        public string Phone { get; set; }

        /// <summary>
        /// Contact email
        /// </summary>
        [SugarColumn(ColumnName = "email", Length = 100, IsNullable = true)]
        public string Email { get; set; }

        /// <summary>
        /// Worker nationality
        /// </summary>
        [SugarColumn(ColumnName = "nationality", Length = 50, IsNullable = true)]
        public string Nationality { get; set; }

        /// <summary>
        /// Role/position
        /// </summary>
        [SugarColumn(ColumnName = "job_title", Length = 100, IsNullable = true)]
        public string JobTitle { get; set; }

        /// <summary>
        /// Worker photo URL
        /// </summary>
        [SugarColumn(ColumnName = "photo_url", Length = 500, IsNullable = true)]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Iqama image URL
        /// </summary>
        [SugarColumn(ColumnName = "iqama_image_url", Length = 500, IsNullable = true)]
        public string IqamaImageUrl { get; set; }

        /// <summary>
        /// Active/inactive flag
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Additional notes
        /// </summary>
        [SugarColumn(ColumnName = "notes", ColumnDataType = "TEXT", IsNullable = true)]
        public string Notes { get; set; }

        // Navigation
        [SugarColumn(IsIgnore = true)]
        public string CustomerName { get; set; }
    }
}
