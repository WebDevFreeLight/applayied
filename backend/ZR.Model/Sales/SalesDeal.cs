using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Sales Deal - tracks revenue opportunities from qualified leads
    /// </summary>
    [SugarTable("sales_deal")]
    public class SalesDeal : SysBase, ISoftDelete
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "deal_id", IsPrimaryKey = true, IsIdentity = false)]
        public long DealId { get; set; }

        /// <summary>
        /// Lead ID (FK -> bp_lead)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "lead_id")]
        public long LeadId { get; set; }

        /// <summary>
        /// Deal Title
        /// </summary>
        [SugarColumn(ColumnName = "deal_title", Length = 200)]
        public string DealTitle { get; set; }

        /// <summary>
        /// Deal Value (monetary amount)
        /// </summary>
        [SugarColumn(ColumnName = "deal_value", DecimalDigits = 2)]
        public decimal DealValue { get; set; }

        /// <summary>
        /// Currency (default SAR)
        /// </summary>
        [SugarColumn(ColumnName = "currency", Length = 10)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Deal Status: Under Negotiation, Won, Lost
        /// </summary>
        [SugarColumn(ColumnName = "deal_status", Length = 50)]
        public string DealStatus { get; set; } = "Under Negotiation";

        /// <summary>
        /// Deal Stage (optional finer stage within a status)
        /// </summary>
        [SugarColumn(ColumnName = "deal_stage", Length = 100, IsNullable = true)]
        public string DealStage { get; set; }

        /// <summary>
        /// Assigned To Employee (FK -> hr_employee_core)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "assigned_to")]
        public long AssignedTo { get; set; }

        /// <summary>
        /// Expected Close Date
        /// </summary>
        [SugarColumn(ColumnName = "expected_close_date", IsNullable = true)]
        public DateTime? ExpectedCloseDate { get; set; }

        /// <summary>
        /// Actual Close Date (set when Won or Lost)
        /// </summary>
        [SugarColumn(ColumnName = "actual_close_date", IsNullable = true)]
        public DateTime? ActualCloseDate { get; set; }

        /// <summary>
        /// Reason for winning the deal
        /// </summary>
        [SugarColumn(ColumnName = "won_reason", Length = 500, IsNullable = true)]
        public string WonReason { get; set; }

        /// <summary>
        /// Reason for losing the deal
        /// </summary>
        [SugarColumn(ColumnName = "lost_reason", Length = 500, IsNullable = true)]
        public string LostReason { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        [SugarColumn(ColumnName = "notes", ColumnDataType = "text", IsNullable = true)]
        public string Notes { get; set; }

        /// <summary>
        /// Priority: Low, Medium, High, Urgent
        /// </summary>
        [SugarColumn(ColumnName = "priority", Length = 20)]
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Deal Type: Rent, Shared
        /// </summary>
        [SugarColumn(ColumnName = "deal_type", Length = 20, IsNullable = true)]
        public string DealType { get; set; } = "Rent";

        /// <summary>
        /// Share Percentage (0-100, used when DealType = Shared)
        /// </summary>
        [SugarColumn(ColumnName = "share_percentage", DecimalDigits = 2, IsNullable = true)]
        public decimal? SharePercentage { get; set; }

        /// <summary>
        /// Season ID - معرف الموسم
        /// FK to blv_boulevard_seasons. Context for filtering shops.
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "season_id", IsNullable = true)]
        public long? SeasonId { get; set; }

        /// <summary>
        /// Is Converted to Contract - هل تم التحويل لعقد
        /// Set to true when deal is Won and contract is generated.
        /// </summary>
        [SugarColumn(ColumnName = "is_converted", IsNullable = false)]
        public bool IsConverted { get; set; } = false;

        [SugarColumn(ColumnName = "is_deleted", IsNullable = false)]
        public bool IsDeleted { get; set; } = false;

        [SugarColumn(ColumnName = "deleted_at", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties (not mapped to database)

        [SugarColumn(IsIgnore = true)]
        public string LeadName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string AssignedToName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string LeadStatus { get; set; }
    }
}
