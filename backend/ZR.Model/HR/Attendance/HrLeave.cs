
using Infrastructure.Converter;
using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance
{
    /// <summary>
    /// Leave Main Table
    /// </summary>
    [SugarTable("hr_leave")]
    
    [SqlDict(
        dictType: "leave",
        dictName: "Hr Leave",
        valueColumn: "LeaveId",
        labelColumns: "reason",
        RemarkColumn = "approvecomments",
        OrderBy = "LeaveId ASC",
        Description = "Auto-generated from HrLeave table"
    )]
    public class HrLeave : SysBase
    {

        /// <summary>
        /// Leave ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveId { get; set; }

        /// <summary>
        /// Employee ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        /// <summary>
        /// Leave Type ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveTypeId { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Start time within the day (nullable for full-day leave)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// End time within the day (nullable for full-day leave)
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Total requested minutes (authoritative for attendance impact)
        /// </summary>
        public int RequestedMinutes { get; set; }



        // =========================
        // Request Context
        // =========================

        [SugarColumn(Length = 500)]
        public string Reason { get; set; }

        public bool IsEmergency { get; set; }

        [SugarColumn(Length = 500)]
        public string AttachmentUrl { get; set; }



        // =========================
        // Workflow & State
        // =========================

        /// <summary>
        /// Leave status dictionary (Pending, Approved, Rejected, Cancelled)
        /// </summary>
        public long StatusDictCode { get; set; }

        /// <summary>
        /// Current workflow step (for multi-step approvals)
        /// </summary>
        public int WorkflowStep { get; set; }

        /// <summary>
        /// Indicates whether this leave is finalized and immutable
        /// </summary>
        public bool IsFinalized { get; set; }


        // =========================
        // Approval (Single Source of Truth)
        // =========================

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [SugarColumn(Length = 500)]
        public string ApprovalComment { get; set; }

        // =========================
        // Cancellation / Rejection
        // =========================

        [SugarColumn(Length = 500)]
        public string RejectionReason { get; set; }

        [SugarColumn(Length = 500)]
        public string CancellationReason { get; set; }

        // =========================
        // Technical Integrity
        // =========================

        /// <summary>
        /// Version for optimistic concurrency & recalculation safety
        /// </summary>
        public int RowVersion { get; set; }

        /// <summary>
        /// Indicates whether attendance impact events were generated
        /// </summary>
        public bool AttendanceEventGenerated { get; set; }
    }


}

