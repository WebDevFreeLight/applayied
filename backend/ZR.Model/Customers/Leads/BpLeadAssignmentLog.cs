using System;
using Infrastructure.Converter;

namespace ZR.Model.Customers.Leads
{
    /// <summary>
    /// Lead Assignment History - Tracks lead reassignments
    /// </summary>
    [SugarTable("bp_lead_assignment_log")]
    public class BpLeadAssignmentLog : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "assignment_log_id", IsPrimaryKey = true, IsIdentity = false)]
        public long AssignmentLogId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "lead_id", IsNullable = false)]
        public long LeadId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "from_employee_id")]
        public long? FromEmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "to_employee_id", IsNullable = false)]
        public long ToEmployeeId { get; set; }

        [SugarColumn(ColumnName = "assignment_date", IsNullable = false)]
        public DateTime AssignmentDate { get; set; }

        [SugarColumn(ColumnName = "reason", Length = 500)]
        public string Reason { get; set; }

        [SugarColumn(ColumnName = "reassignment_type", Length = 50)]
        public string ReassignmentType { get; set; }

        // Navigation properties (not mapped to database)
        [SugarColumn(IsIgnore = true)]
        public BpLead Lead { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string FromEmployeeName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ToEmployeeName { get; set; }
    }
}
