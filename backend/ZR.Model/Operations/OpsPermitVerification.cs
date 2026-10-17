using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Document Verification - التحقق من الوثائق
    /// </summary>
    [SugarTable("ops_permit_verification")]
    public class OpsPermitVerification : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "verification_id", IsPrimaryKey = true, IsIdentity = false)]
        public long VerificationId { get; set; }

        /// <summary>
        /// FK to ops_permit (optional)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "permit_id", IsNullable = true)]
        public long? PermitId { get; set; }

        /// <summary>
        /// FK to arap_customer
        /// </summary>
        [SugarColumn(ColumnName = "customer_id", Length = 50, IsNullable = true)]
        public string CustomerId { get; set; }

        /// <summary>
        /// FK to ops_customer_worker
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "worker_id", IsNullable = true)]
        public long? WorkerId { get; set; }

        /// <summary>
        /// Name of person being verified
        /// </summary>
        [SugarColumn(ColumnName = "applicant_name", Length = 200)]
        public string ApplicantName { get; set; }

        /// <summary>
        /// ID document number
        /// </summary>
        [SugarColumn(ColumnName = "id_number", Length = 30)]
        public string IdNumber { get; set; }

        /// <summary>
        /// Document type: NationalID, Iqama, Passport, DrivingLicense
        /// </summary>
        [SugarColumn(ColumnName = "document_type", Length = 30)]
        public string DocumentType { get; set; } = "NationalID";

        /// <summary>
        /// Uploaded document file path
        /// </summary>
        [SugarColumn(ColumnName = "document_url", Length = 500, IsNullable = true)]
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Status: pending, verified, rejected
        /// </summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Staff who verified
        /// </summary>
        [SugarColumn(ColumnName = "verified_by", Length = 100, IsNullable = true)]
        public string VerifiedBy { get; set; }

        /// <summary>
        /// When verified
        /// </summary>
        [SugarColumn(ColumnName = "verified_date", IsNullable = true)]
        public DateTime? VerifiedDate { get; set; }

        /// <summary>
        /// Why rejected
        /// </summary>
        [SugarColumn(ColumnName = "rejection_reason", Length = 500, IsNullable = true)]
        public string RejectionReason { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [SugarColumn(ColumnName = "notes", ColumnDataType = "TEXT", IsNullable = true)]
        public string Notes { get; set; }

        // Navigation
        [SugarColumn(IsIgnore = true)]
        public string PermitNumber { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string CustomerName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string WorkerName { get; set; }
    }
}
