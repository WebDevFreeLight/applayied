using System;
using Infrastructure.Converter;

namespace ZR.Model.Customers.Leads
{
    /// <summary>
    /// Lead Activity Tracking - Records all interactions with leads
    /// </summary>
    [SugarTable("bp_lead_activity")]
    public class BpLeadActivity : SysBase, ISoftDelete
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "activity_id", IsPrimaryKey = true, IsIdentity = false)]
        public long ActivityId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "lead_id", IsNullable = false)]
        public long LeadId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "employee_id", IsNullable = false)]
        public long EmployeeId { get; set; }

        [SugarColumn(ColumnName = "activity_type", Length = 50, IsNullable = false)]
        public string ActivityType { get; set; }

        [SugarColumn(ColumnName = "activity_date", IsNullable = false)]
        public DateTime ActivityDate { get; set; }

        [SugarColumn(ColumnName = "duration_minutes")]
        public int? DurationMinutes { get; set; }

        [SugarColumn(ColumnName = "outcome", Length = 50)]
        public string Outcome { get; set; }

        [SugarColumn(ColumnName = "notes", ColumnDataType = "TEXT")]
        public string Notes { get; set; }

        [SugarColumn(ColumnName = "next_follow_up_date")]
        public DateTime? NextFollowUpDate { get; set; }

        [SugarColumn(ColumnName = "is_automated", IsNullable = false)]
        public bool IsAutomated { get; set; } = false;

        [SugarColumn(ColumnName = "is_deleted", IsNullable = false)]
        public bool IsDeleted { get; set; } = false;

        [SugarColumn(ColumnName = "deleted_at", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties (not mapped to database)
        [SugarColumn(IsIgnore = true)]
        public BpLead Lead { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string EmployeeName { get; set; }
    }
}
