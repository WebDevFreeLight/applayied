using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Site Visit - الزيارة الميدانية
    /// </summary>
    [SugarTable("ops_site_visit")]
    public class SiteVisit : SysBase, ISoftDelete
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "visit_id", IsPrimaryKey = true, IsIdentity = false)]
        public long VisitId { get; set; }

        /// <summary>
        /// Lead ID (FK -> bp_lead)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "lead_id", IsNullable = false)]
        public long LeadId { get; set; }

        /// <summary>
        /// Deal ID (FK -> sales_deal, optional)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "deal_id", IsNullable = true)]
        public long? DealId { get; set; }

        /// <summary>
        /// Scheduled Date
        /// </summary>
        [SugarColumn(ColumnName = "scheduled_date", IsNullable = false)]
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Scheduled Time (e.g. "10:00")
        /// </summary>
        [SugarColumn(ColumnName = "scheduled_time", Length = 10, IsNullable = true)]
        public string ScheduledTime { get; set; }

        /// <summary>
        /// Assigned Operations Staff (FK -> hr_employee_core)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "assigned_staff_id", IsNullable = true)]
        public long? AssignedStaffId { get; set; }

        /// <summary>
        /// Visit Status: Scheduled, InProgress, Completed, Cancelled
        /// </summary>
        [SugarColumn(ColumnName = "visit_status", Length = 50, IsNullable = false)]
        public string VisitStatus { get; set; } = "Scheduled";

        /// <summary>
        /// Pre-visit notes
        /// </summary>
        [SugarColumn(ColumnName = "visit_notes", ColumnDataType = "TEXT", IsNullable = true)]
        public string VisitNotes { get; set; }

        /// <summary>
        /// Post-visit completion notes/report
        /// </summary>
        [SugarColumn(ColumnName = "completion_notes", ColumnDataType = "TEXT", IsNullable = true)]
        public string CompletionNotes { get; set; }

        /// <summary>
        /// Actual completion date
        /// </summary>
        [SugarColumn(ColumnName = "completed_date", IsNullable = true)]
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Cancellation reason
        /// </summary>
        [SugarColumn(ColumnName = "cancellation_reason", Length = 500, IsNullable = true)]
        public string CancellationReason { get; set; }

        [SugarColumn(ColumnName = "is_deleted", IsNullable = false)]
        public bool IsDeleted { get; set; } = false;

        [SugarColumn(ColumnName = "deleted_at", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties (not mapped to database)
        [SugarColumn(IsIgnore = true)]
        public string LeadName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string AssignedStaffName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string DealTitle { get; set; }

        // Full lead details for visit detail view
        [SugarColumn(IsIgnore = true)]
        public string OwnerName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string OwnerPhone { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string OwnerEmail { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string LeadStatus { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string LeadSource { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string Priority { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string BudgetRange { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string QualificationNotes { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string InternalNotes { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string BrandImage { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string DealStatus { get; set; }
    }
}
