using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Operations Permit - تصريح العمليات
    /// </summary>
    [SugarTable("ops_permit")]
    public class OpsPermit : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "permit_id", IsPrimaryKey = true, IsIdentity = false)]
        public long PermitId { get; set; }

        /// <summary>
        /// Auto-generated (PRM-YYYY-NNNNN)
        /// </summary>
        [SugarColumn(ColumnName = "permit_number", Length = 20)]
        public string PermitNumber { get; set; }

        /// <summary>
        /// FK to blv_boulevard_seasons
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "season_id")]
        public long SeasonId { get; set; }

        /// <summary>
        /// FK to blv_boulevard_season_zones
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "zone_id", IsNullable = true)]
        public long? ZoneId { get; set; }

        /// <summary>
        /// FK to arap_customer (Guid as string)
        /// </summary>
        [SugarColumn(ColumnName = "customer_id", Length = 50)]
        public string CustomerId { get; set; }

        /// <summary>
        /// FK to ops_customer_worker (null = customer themselves)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "worker_id", IsNullable = true)]
        public long? WorkerId { get; set; }

        /// <summary>
        /// FK to ops_permit_type
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "permit_type_id")]
        public long PermitTypeId { get; set; }

        /// <summary>
        /// Person entering (EN)
        /// </summary>
        [SugarColumn(ColumnName = "applicant_name", Length = 200)]
        public string ApplicantName { get; set; }

        /// <summary>
        /// Person entering (AR)
        /// </summary>
        [SugarColumn(ColumnName = "applicant_name_ar", Length = 200, IsNullable = true)]
        public string ApplicantNameAr { get; set; }

        /// <summary>
        /// ID number of the person
        /// </summary>
        [SugarColumn(ColumnName = "national_id", Length = 20, IsNullable = true)]
        public string NationalId { get; set; }

        /// <summary>
        /// Contact phone
        /// </summary>
        [SugarColumn(ColumnName = "contact_phone", Length = 20, IsNullable = true)]
        public string ContactPhone { get; set; }

        /// <summary>
        /// Contact email
        /// </summary>
        [SugarColumn(ColumnName = "email", Length = 100, IsNullable = true)]
        public string Email { get; set; }

        /// <summary>
        /// Customer company name
        /// </summary>
        [SugarColumn(ColumnName = "company_name", Length = 200, IsNullable = true)]
        public string CompanyName { get; set; }

        /// <summary>
        /// Permit start date
        /// </summary>
        [SugarColumn(ColumnName = "valid_from")]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Permit end date
        /// </summary>
        [SugarColumn(ColumnName = "valid_until")]
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// When permit was created
        /// </summary>
        [SugarColumn(ColumnName = "issue_date")]
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Staff who issued
        /// </summary>
        [SugarColumn(ColumnName = "issued_by", Length = 100, IsNullable = true)]
        public string IssuedBy { get; set; }

        /// <summary>
        /// Reason for access
        /// </summary>
        [SugarColumn(ColumnName = "purpose", Length = 500, IsNullable = true)]
        public string Purpose { get; set; }

        /// <summary>
        /// Access level: basic, advanced, full
        /// </summary>
        [SugarColumn(ColumnName = "access_level", Length = 20, IsNullable = true)]
        public string AccessLevel { get; set; }

        /// <summary>
        /// For vehicle permits
        /// </summary>
        [SugarColumn(ColumnName = "vehicle_plate_number", Length = 20, IsNullable = true)]
        public string VehiclePlateNumber { get; set; }

        /// <summary>
        /// Vehicle type
        /// </summary>
        [SugarColumn(ColumnName = "vehicle_type", Length = 50, IsNullable = true)]
        public string VehicleType { get; set; }

        /// <summary>
        /// Permit holder photo
        /// </summary>
        [SugarColumn(ColumnName = "photo_url", Length = 500, IsNullable = true)]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Supporting documents
        /// </summary>
        [SugarColumn(ColumnName = "attachment_url", Length = 500, IsNullable = true)]
        public string AttachmentUrl { get; set; }

        /// <summary>
        /// QR code data
        /// </summary>
        [SugarColumn(ColumnName = "qr_code", Length = 500, IsNullable = true)]
        public string QRCode { get; set; }

        /// <summary>
        /// Status: pending, approved, rejected, active, expired, cancelled
        /// </summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "pending";

        /// <summary>
        /// User ID who approved
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "approved_by", IsNullable = true)]
        public long? ApprovedBy { get; set; }

        /// <summary>
        /// When approved
        /// </summary>
        [SugarColumn(ColumnName = "approval_date", IsNullable = true)]
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Why rejected
        /// </summary>
        [SugarColumn(ColumnName = "rejection_reason", Length = 500, IsNullable = true)]
        public string RejectionReason { get; set; }

        /// <summary>
        /// General notes
        /// </summary>
        [SugarColumn(ColumnName = "notes", ColumnDataType = "TEXT", IsNullable = true)]
        public string Notes { get; set; }

        // ===== Sela Work Permit Fields =====

        /// <summary>
        /// FK to blv_boulevard_shops (Unit Name)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "shop_id", IsNullable = true)]
        public long? ShopId { get; set; }

        /// <summary>
        /// Gate number
        /// </summary>
        [SugarColumn(ColumnName = "gate_number", Length = 20, IsNullable = true)]
        public string GateNumber { get; set; }

        /// <summary>
        /// Direction: access / exit
        /// </summary>
        [SugarColumn(ColumnName = "direction", Length = 10, IsNullable = true)]
        public string Direction { get; set; }

        /// <summary>
        /// Work permit opening date+time
        /// </summary>
        [SugarColumn(ColumnName = "opening_date", IsNullable = true)]
        public DateTime? OpeningDate { get; set; }

        /// <summary>
        /// Work permit closing date+time
        /// </summary>
        [SugarColumn(ColumnName = "closing_date", IsNullable = true)]
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Comma-separated work type IDs
        /// </summary>
        [SugarColumn(ColumnName = "work_type_ids", Length = 200, IsNullable = true)]
        public string WorkTypeIds { get; set; }

        /// <summary>
        /// Comma-separated work category IDs
        /// </summary>
        [SugarColumn(ColumnName = "work_category_ids", Length = 200, IsNullable = true)]
        public string WorkCategoryIds { get; set; }

        /// <summary>
        /// Work condition: normal / emergency
        /// </summary>
        [SugarColumn(ColumnName = "work_condition", Length = 20, IsNullable = true)]
        public string WorkCondition { get; set; }

        /// <summary>
        /// Detailed work description
        /// </summary>
        [SugarColumn(ColumnName = "work_description", ColumnDataType = "TEXT", IsNullable = true)]
        public string WorkDescription { get; set; }

        // Navigation
        [SugarColumn(IsIgnore = true)]
        public string SeasonName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string ZoneName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string CustomerName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string WorkerName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string PermitTypeName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public string ShopName { get; set; }
        [SugarColumn(IsIgnore = true)]
        public List<string> WorkTypeNames { get; set; }
        [SugarColumn(IsIgnore = true)]
        public List<string> WorkCategoryNames { get; set; }
        [SugarColumn(IsIgnore = true)]
        public List<OpsPermitSafetyCheck> SafetyChecks { get; set; }
        [SugarColumn(IsIgnore = true)]
        public List<OpsPermitWorker> Workers { get; set; }
    }
}
