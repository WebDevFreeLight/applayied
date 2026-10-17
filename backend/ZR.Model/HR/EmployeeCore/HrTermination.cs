using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.EmployeeCore
{
    /// <summary>
    /// Employee Termination / Exit Management - hr_termination
    /// Tracks the full exit lifecycle: Resignation/Termination → Approval → ESB → Final Settlement → Deactivation
    /// </summary>
    [SugarTable("hr_termination")]
    public class HrTermination : SysBase
    {
        [SugarColumn(ColumnName = "termination_id", IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TerminationId { get; set; }

        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        /// <summary>
        /// resignation, termination, end_of_contract, retirement, death, mutual_agreement
        /// </summary>
        [SugarColumn(ColumnName = "termination_type", Length = 50)]
        public string TerminationType { get; set; }

        /// <summary>
        /// draft, pending_approval, approved, notice_period, in_progress, pending_settlement, completed, cancelled
        /// </summary>
        [SugarColumn(ColumnName = "status", Length = 30)]
        public string Status { get; set; } = "draft";

        [SugarColumn(ColumnName = "request_date")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [SugarColumn(ColumnName = "last_working_date", IsNullable = true)]
        public DateTime? LastWorkingDate { get; set; }

        [SugarColumn(ColumnName = "effective_date", IsNullable = true)]
        public DateTime? EffectiveDate { get; set; }

        [SugarColumn(ColumnName = "notice_period_days")]
        public int NoticePeriodDays { get; set; } = 30;

        [SugarColumn(ColumnName = "notice_served")]
        public bool NoticeServed { get; set; } = false;

        [SugarColumn(ColumnName = "reason", ColumnDataType = "text", IsNullable = true)]
        public string Reason { get; set; }

        [SugarColumn(ColumnName = "reason_category", Length = 100, IsNullable = true)]
        public string ReasonCategory { get; set; }

        // Approval
        [SugarColumn(ColumnName = "approved_by", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApprovedBy { get; set; }

        [SugarColumn(ColumnName = "approved_date", IsNullable = true)]
        public DateTime? ApprovedDate { get; set; }

        [SugarColumn(ColumnName = "approval_comments", Length = 500, IsNullable = true)]
        public string ApprovalComments { get; set; }

        // ESB / Final Settlement
        [SugarColumn(ColumnName = "esb_amount", DecimalDigits = 2, IsNullable = true)]
        public decimal? ESBAmount { get; set; }

        [SugarColumn(ColumnName = "leave_encashment_amount", DecimalDigits = 2, IsNullable = true)]
        public decimal? LeaveEncashmentAmount { get; set; }

        [SugarColumn(ColumnName = "pending_loan_amount", DecimalDigits = 2, IsNullable = true)]
        public decimal? PendingLoanAmount { get; set; }

        [SugarColumn(ColumnName = "other_deductions", DecimalDigits = 2, IsNullable = true)]
        public decimal? OtherDeductions { get; set; }

        [SugarColumn(ColumnName = "final_settlement_amount", DecimalDigits = 2, IsNullable = true)]
        public decimal? FinalSettlementAmount { get; set; }

        [SugarColumn(ColumnName = "settlement_paid")]
        public bool SettlementPaid { get; set; } = false;

        [SugarColumn(ColumnName = "settlement_paid_date", IsNullable = true)]
        public DateTime? SettlementPaidDate { get; set; }

        // Exit Checklist
        [SugarColumn(ColumnName = "asset_return_completed")]
        public bool AssetReturnCompleted { get; set; } = false;

        [SugarColumn(ColumnName = "it_access_revoked")]
        public bool ITAccessRevoked { get; set; } = false;

        [SugarColumn(ColumnName = "exit_interview_completed")]
        public bool ExitInterviewCompleted { get; set; } = false;

        [SugarColumn(ColumnName = "clearance_completed")]
        public bool ClearanceCompleted { get; set; } = false;

        [SugarColumn(ColumnName = "experience_letter_issued")]
        public bool ExperienceLetterIssued { get; set; } = false;

        [SugarColumn(ColumnName = "notes", ColumnDataType = "text", IsNullable = true)]
        public string Notes { get; set; }

        // Workflow link
        [SugarColumn(ColumnName = "workflow_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? WorkflowId { get; set; }
    }
}
