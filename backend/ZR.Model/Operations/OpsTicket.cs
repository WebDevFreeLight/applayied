using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Operations Ticket - تذكرة العمليات
    /// </summary>
    [SugarTable("ops_ticket")]
    public class OpsTicket : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "ticket_id", IsPrimaryKey = true, IsIdentity = false)]
        public long TicketId { get; set; }

        /// <summary>
        /// Ticket Title
        /// </summary>
        [SugarColumn(ColumnName = "title", Length = 200)]
        public string Title { get; set; }

        /// <summary>
        /// Ticket Description
        /// </summary>
        [SugarColumn(ColumnName = "description", ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Lead ID (FK -> bp_lead, optional)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "lead_id", IsNullable = true)]
        public long? LeadId { get; set; }

        /// <summary>
        /// Deal ID (FK -> sales_deal, optional)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "deal_id", IsNullable = true)]
        public long? DealId { get; set; }

        /// <summary>
        /// Category: Technical, Operational, Compliance, Other
        /// </summary>
        [SugarColumn(ColumnName = "category", Length = 50)]
        public string Category { get; set; } = "Operational";

        /// <summary>
        /// Priority: Low, Medium, High, Urgent
        /// </summary>
        [SugarColumn(ColumnName = "priority", Length = 20)]
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Ticket Status: Open, InProgress, Resolved, Closed
        /// </summary>
        [SugarColumn(ColumnName = "ticket_status", Length = 50)]
        public string TicketStatus { get; set; } = "Open";

        /// <summary>
        /// Assigned Operations Staff (FK -> hr_employee_core)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "assigned_to", IsNullable = true)]
        public long? AssignedTo { get; set; }

        /// <summary>
        /// Reported By Employee (FK -> hr_employee_core)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "reported_by", IsNullable = true)]
        public long? ReportedBy { get; set; }

        /// <summary>
        /// Resolution Notes (filled when Resolved/Closed)
        /// </summary>
        [SugarColumn(ColumnName = "resolution_notes", ColumnDataType = "TEXT", IsNullable = true)]
        public string ResolutionNotes { get; set; }

        /// <summary>
        /// Resolved Date
        /// </summary>
        [SugarColumn(ColumnName = "resolved_date", IsNullable = true)]
        public DateTime? ResolvedDate { get; set; }

        /// <summary>
        /// Whether this ticket was escalated to the linked deal
        /// </summary>
        [SugarColumn(ColumnName = "escalated_to_deal", IsNullable = false)]
        public bool EscalatedToDeal { get; set; } = false;

        // Navigation properties (not mapped to database)

        [SugarColumn(IsIgnore = true)]
        public string LeadName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string DealTitle { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string AssignedToName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ReportedByName { get; set; }
    }
}
