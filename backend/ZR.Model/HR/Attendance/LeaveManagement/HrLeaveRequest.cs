
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.LeaveManagement
{
    /// <summary>
    /// HR Leave Request Model
    /// </summary>
    [SugarTable("hr_leave_request")]
    public class HrLeaveRequest : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long RequestId { get; set; }

        public long EmployeeId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public long LeaveTypeId { get; set; }

        public decimal? ApprovedMinutes { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? StatusDictCode { get; set; }

        // Additional properties for compatibility with existing services
        public DateTime? RequestDate { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApprovedBy { get; set; }

        public string? Status { get; set; }

        public string? Reason { get; set; }

        public string? RejectionReason { get; set; }

        // Additional fields for extended leave management
        public string? ApproveComments { get; set; }
        public string? ApprovalComments { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsEmergency { get; set; }

        // Alias properties for compatibility
        public DateTime StartDate { get => FromDate; set => FromDate = value; }
        public DateTime EndDate { get => ToDate; set => ToDate = value; }
        public decimal? Days { get; set; }

        // Additional aliases
        public long? ApproverId { get => ApprovedBy; set => ApprovedBy = value; }
        public DateTime? ApproveDate { get => ApprovalDate; set => ApprovalDate = value; }
    }


}

