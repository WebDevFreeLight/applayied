using System;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;

namespace ZR.Model.HR.Dto.Attendance
{
    // ==================== HrLeaveRequest DTOs ====================

    /// <summary>
    /// Leave Request Query DTO
    /// </summary>
    public class HrLeaveRequestQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Leave Type ID filter
        /// </summary>
        public long? LeaveTypeId { get; set; }

        /// <summary>
        /// Status Dict Code filter
        /// </summary>
        public long? StatusDictCode { get; set; }

        /// <summary>
        /// Status filter (Pending, Approved, Rejected)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Begin From Date for range filter
        /// </summary>
        public DateTime? BeginFromDate { get; set; }

        /// <summary>
        /// End From Date for range filter
        /// </summary>
        public DateTime? EndFromDate { get; set; }

        /// <summary>
        /// Begin Request Date for range filter
        /// </summary>
        public DateTime? BeginRequestDate { get; set; }

        /// <summary>
        /// End Request Date for range filter
        /// </summary>
        public DateTime? EndRequestDate { get; set; }

        /// <summary>
        /// Approved By filter
        /// </summary>
        public long? ApprovedBy { get; set; }

        /// <summary>
        /// Is Emergency filter
        /// </summary>
        public bool? IsEmergency { get; set; }
    }

    /// <summary>
    /// Leave Request DTO - for read/display operations
    /// </summary>
    public class HrLeaveRequestDto
    {
        [ExcelColumn(Name = "Request ID")]
        [ExcelColumnName("Request ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequestId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Leave Type ID")]
        [ExcelColumnName("Leave Type ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveTypeId { get; set; }

        [ExcelColumn(Name = "From Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("From Date")]
        public DateTime FromDate { get; set; }

        [ExcelColumn(Name = "To Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("To Date")]
        public DateTime ToDate { get; set; }

        [ExcelColumn(Name = "Approved Minutes")]
        [ExcelColumnName("Approved Minutes")]
        public decimal? ApprovedMinutes { get; set; }

        [ExcelColumn(Name = "Days")]
        [ExcelColumnName("Days")]
        public decimal? Days { get; set; }

        [ExcelColumn(Name = "Status Dict Code")]
        [ExcelColumnName("Status Dict Code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? StatusDictCode { get; set; }

        [ExcelColumn(Name = "Status")]
        [ExcelColumnName("Status")]
        public string Status { get; set; }

        [ExcelColumn(Name = "Request Date", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Request Date")]
        public DateTime? RequestDate { get; set; }

        [ExcelColumn(Name = "Reason")]
        [ExcelColumnName("Reason")]
        public string Reason { get; set; }

        [ExcelColumn(Name = "Approved By")]
        [ExcelColumnName("Approved By")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApprovedBy { get; set; }

        [ExcelColumn(Name = "Approval Date", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Approval Date")]
        public DateTime? ApprovalDate { get; set; }

        [ExcelColumn(Name = "Approval Comments")]
        [ExcelColumnName("Approval Comments")]
        public string ApprovalComments { get; set; }

        [ExcelColumn(Name = "Rejection Reason")]
        [ExcelColumnName("Rejection Reason")]
        public string RejectionReason { get; set; }

        [ExcelColumn(Name = "Attachment URL")]
        [ExcelColumnName("Attachment URL")]
        public string AttachmentUrl { get; set; }

        [ExcelColumn(Name = "Is Emergency")]
        [ExcelColumnName("Is Emergency")]
        public bool IsEmergency { get; set; }

        [ExcelColumn(Name = "Creator")]
        [ExcelColumnName("Creator")]
        public string CreateBy { get; set; }

        [ExcelColumn(Name = "Create Time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Create Time")]
        public DateTime? CreateTime { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        // Computed properties
        [ExcelIgnore]
        public DateTime StartDate => FromDate;

        [ExcelIgnore]
        public DateTime EndDate => ToDate;

        // Labels for display
        [ExcelIgnore]
        public string IsEmergencyLabel => IsEmergency ? "Emergency" : "Normal";

        [ExcelIgnore]
        public bool IsApproved => !string.IsNullOrEmpty(Status) && Status.ToLower() == "approved";

        [ExcelIgnore]
        public bool IsRejected => !string.IsNullOrEmpty(Status) && Status.ToLower() == "rejected";

        [ExcelIgnore]
        public string StatusLabel => Status ?? "Pending";
    }

    /// <summary>
    /// Leave Request Create DTO (existing)
    /// </summary>
    public class LeaveRequestCreateDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Leave type ID is required")]
        public long LeaveTypeId { get; set; }

        [Required(ErrorMessage = "From date is required")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required")]
        public DateTime ToDate { get; set; }

        public decimal? RequestedMinutes { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        public string AttachmentUrl { get; set; }

        public bool IsEmergency { get; set; } = false;

        public string Remark { get; set; }
    }

    /// <summary>
    /// Update Leave Request DTO
    /// </summary>
    public class UpdateLeaveRequestDto
    {
        [Required(ErrorMessage = "Request ID is required")]
        public long RequestId { get; set; }

        [Required(ErrorMessage = "Leave type ID is required")]
        public long LeaveTypeId { get; set; }

        [Required(ErrorMessage = "From date is required")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required")]
        public DateTime ToDate { get; set; }

        public decimal? RequestedMinutes { get; set; }

        public string Reason { get; set; }

        public string AttachmentUrl { get; set; }

        public bool IsEmergency { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Leave Approval DTO (existing)
    /// </summary>
    public class LeaveApprovalDto
    {
        [Required(ErrorMessage = "Request ID is required")]
        public long RequestId { get; set; }

        [Required(ErrorMessage = "Approved by is required")]
        public long ApprovedBy { get; set; }

        public decimal? ApprovedMinutes { get; set; }

        public string ApprovalComments { get; set; }
    }

    /// <summary>
    /// Reject Leave Request DTO
    /// </summary>
    public class RejectLeaveRequestDto
    {
        [Required(ErrorMessage = "Request ID is required")]
        public long RequestId { get; set; }

        [Required(ErrorMessage = "Rejected by is required")]
        public long RejectedBy { get; set; }

        [Required(ErrorMessage = "Rejection reason is required")]
        public string RejectionReason { get; set; }
    }

    // ==================== HrLeaveBalance DTOs ====================

    /// <summary>
    /// Leave Balance Query DTO
    /// </summary>
    public class LeaveBalanceQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Leave Type ID filter
        /// </summary>
        public long? LeaveTypeId { get; set; }

        /// <summary>
        /// Year filter
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Has Balance filter (RemainingMinutes > 0)
        /// </summary>
        public bool? HasBalance { get; set; }
    }

    /// <summary>
    /// Leave Balance DTO - for read/display operations (Enhanced from existing)
    /// </summary>
    public class LeaveBalanceDto
    {
        [ExcelColumn(Name = "Balance ID")]
        [ExcelColumnName("Balance ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BalanceId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Leave Type ID")]
        [ExcelColumnName("Leave Type ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveTypeId { get; set; }

        [ExcelColumn(Name = "Leave Type Name")]
        [ExcelColumnName("Leave Type Name")]
        public string LeaveTypeName { get; set; }

        [ExcelColumn(Name = "Year")]
        [ExcelColumnName("Year")]
        public int Year { get; set; }

        [ExcelColumn(Name = "Total Minutes")]
        [ExcelColumnName("Total Minutes")]
        public decimal TotalMinutes { get; set; }

        [ExcelColumn(Name = "Used Minutes")]
        [ExcelColumnName("Used Minutes")]
        public decimal UsedMinutes { get; set; }

        [ExcelColumn(Name = "Remaining Minutes")]
        [ExcelColumnName("Remaining Minutes")]
        public decimal RemainingMinutes { get; set; }

        [ExcelColumn(Name = "Carried Forward Minutes")]
        [ExcelColumnName("Carried Forward Minutes")]
        public decimal? CarriedForwardMinutes { get; set; }

        [ExcelColumn(Name = "Total Days")]
        [ExcelColumnName("Total Days")]
        public decimal TotalDays => TotalMinutes / (decimal)480; // 8 hours = 480 minutes

        [ExcelColumn(Name = "Used Days")]
        [ExcelColumnName("Used Days")]
        public decimal UsedDays => UsedMinutes / (decimal)480;

        [ExcelColumn(Name = "Remaining Days")]
        [ExcelColumnName("Remaining Days")]
        public decimal RemainingDays => RemainingMinutes / (decimal)480;

        [ExcelColumn(Name = "Creator")]
        [ExcelColumnName("Creator")]
        public string CreateBy { get; set; }

        [ExcelColumn(Name = "Create Time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Create Time")]
        public DateTime? CreateTime { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        // Labels for display
        [ExcelIgnore]
        public bool HasBalance => RemainingMinutes > 0;

        [ExcelIgnore]
        public string HasBalanceLabel => HasBalance ? "Available" : "Exhausted";
    }

    /// <summary>
    /// Adjust Leave Balance DTO
    /// </summary>
    public class AdjustLeaveBalanceDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Leave type ID is required")]
        public long LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Adjustment minutes is required")]
        public decimal AdjustmentMinutes { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        public string Remark { get; set; }
    }
}
