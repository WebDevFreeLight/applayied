using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;

namespace ZR.Model.HR.Dto.Attendance
{
    // ==================== HrAttendanceDay DTOs ====================

    /// <summary>
    /// Attendance Day Query DTO
    /// </summary>
    public class HrAttendanceDayQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Shift ID filter
        /// </summary>
        public long? ShiftId { get; set; }

        /// <summary>
        /// Begin Attendance Date for range filter
        /// </summary>
        public DateTime? BeginAttendanceDate { get; set; }

        /// <summary>
        /// End Attendance Date for range filter
        /// </summary>
        public DateTime? EndAttendanceDate { get; set; }

        /// <summary>
        /// Is Absent filter
        /// </summary>
        public bool? IsAbsent { get; set; }

        /// <summary>
        /// Is Weekend filter
        /// </summary>
        public bool? IsWeekend { get; set; }

        /// <summary>
        /// Is Holiday filter
        /// </summary>
        public bool? IsHoliday { get; set; }

        /// <summary>
        /// Status filter
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Attendance Status Dict Code filter
        /// </summary>
        public long? AttendanceStatusDictCode { get; set; }

        /// <summary>
        /// Is Late filter
        /// </summary>
        public bool? IsLate { get; set; }

        /// <summary>
        /// Is Early Out filter
        /// </summary>
        public bool? IsEarlyOut { get; set; }

        /// <summary>
        /// Is Remote filter
        /// </summary>
        public bool? IsRemote { get; set; }
    }

    /// <summary>
    /// Attendance Day DTO - for read/display operations (Enhanced from existing)
    /// </summary>
    public class HrAttendanceDayDto
    {
        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Attendance Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Attendance Date")]
        public DateTime AttendanceDate { get; set; }

        [ExcelColumn(Name = "Shift ID")]
        [ExcelColumnName("Shift ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShiftId { get; set; }

        [ExcelColumn(Name = "Policy ID")]
        [ExcelColumnName("Policy ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PolicyId { get; set; }

        [ExcelColumn(Name = "Expected Minutes")]
        [ExcelColumnName("Expected Minutes")]
        public int ExpectedMinutes { get; set; }

        [ExcelColumn(Name = "Worked Minutes")]
        [ExcelColumnName("Worked Minutes")]
        public int WorkedMinutes { get; set; }

        [ExcelColumn(Name = "Late Minutes")]
        [ExcelColumnName("Late Minutes")]
        public int LateMinutes { get; set; }

        [ExcelColumn(Name = "Early Leave Minutes")]
        [ExcelColumnName("Early Leave Minutes")]
        public int EarlyLeaveMinutes { get; set; }

        [ExcelColumn(Name = "Overtime Minutes")]
        [ExcelColumnName("Overtime Minutes")]
        public int OvertimeMinutes { get; set; }

        [ExcelColumn(Name = "Attendance Status Dict Code")]
        [ExcelColumnName("Attendance Status Dict Code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AttendanceStatusDictCode { get; set; }

        [ExcelColumn(Name = "Is Absent")]
        [ExcelColumnName("Is Absent")]
        public bool IsAbsent { get; set; }

        [ExcelColumn(Name = "Is Weekend")]
        [ExcelColumnName("Is Weekend")]
        public bool IsWeekend { get; set; }

        [ExcelColumn(Name = "Is Holiday")]
        [ExcelColumnName("Is Holiday")]
        public bool IsHoliday { get; set; }

        [ExcelColumn(Name = "Status")]
        [ExcelColumnName("Status")]
        public string Status { get; set; }

        [ExcelColumn(Name = "Is Remote")]
        [ExcelColumnName("Is Remote")]
        public bool IsRemote { get; set; }

        [ExcelColumn(Name = "Check In Time", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Check In Time")]
        public DateTime? CheckInTime { get; set; }

        [ExcelColumn(Name = "Check Out Time", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Check Out Time")]
        public DateTime? CheckOutTime { get; set; }

        [ExcelColumn(Name = "Check In Device ID")]
        [ExcelColumnName("Check In Device ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CheckInDeviceId { get; set; }

        [ExcelColumn(Name = "Check Out Device ID")]
        [ExcelColumnName("Check Out Device ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CheckOutDeviceId { get; set; }

        [ExcelColumn(Name = "Check In Location")]
        [ExcelColumnName("Check In Location")]
        public string CheckInLocation { get; set; }

        [ExcelColumn(Name = "Check Out Location")]
        [ExcelColumnName("Check Out Location")]
        public string CheckOutLocation { get; set; }

        [ExcelColumn(Name = "Work Hours")]
        [ExcelColumnName("Work Hours")]
        public decimal WorkHours => WorkedMinutes / 60m;

        [ExcelColumn(Name = "Overtime Hours")]
        [ExcelColumnName("Overtime Hours")]
        public decimal OvertimeHours => OvertimeMinutes / 60m;

        // Labels for display
        [ExcelIgnore]
        public bool IsLate => LateMinutes > 0;

        [ExcelIgnore]
        public bool IsEarlyOut => EarlyLeaveMinutes > 0;

        [ExcelIgnore]
        public string IsAbsentLabel => IsAbsent ? "Absent" : "Present";

        [ExcelIgnore]
        public string IsWeekendLabel => IsWeekend ? "Weekend" : "Weekday";

        [ExcelIgnore]
        public string IsHolidayLabel => IsHoliday ? "Holiday" : "Regular Day";

        [ExcelIgnore]
        public string IsRemoteLabel => IsRemote ? "Remote" : "On-site";
    }

    /// <summary>
    /// Create Attendance Day DTO - for check-in operations (replaces inline CheckInRequest)
    /// </summary>
    public class CreateHrAttendanceDayDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        public DateTime? AttendanceDate { get; set; } = null; // Will default to today if null

        public long? ShiftId { get; set; }

        public long? PolicyId { get; set; }

        public DateTime? CheckInTime { get; set; }

        public long? CheckInDeviceId { get; set; }

        public string CheckInLocation { get; set; }

        public bool IsRemote { get; set; } = false;

        public string Remark { get; set; }

        // Legacy compatibility
        public long? BranchID { get; set; }
    }

    /// <summary>
    /// Update Attendance Day DTO - for check-out operations (replaces inline CheckOutRequest)
    /// </summary>
    public class UpdateHrAttendanceDayDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Attendance date is required")]
        public DateTime AttendanceDate { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public long? CheckOutDeviceId { get; set; }

        public string CheckOutLocation { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Recalculate Attendance Request DTO
    /// </summary>
    public class RecalculateAttendanceRequestDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        public bool ForceRecalculate { get; set; } = false;

        public string Reason { get; set; }
    }

    /// <summary>
    /// Attendance Summary DTO
    /// </summary>
    public class AttendanceSummaryDto
    {
        public long EmployeeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalWorkedMinutes { get; set; }
        public int TotalLateMinutes { get; set; }
        public int DaysPresent { get; set; }
        public int DaysAbsent { get; set; }
        public decimal WorkHours => TotalWorkedMinutes / 60m;
    }

    // ==================== HrAttendanceAdjustment DTOs ====================

    /// <summary>
    /// Attendance Adjustment Query DTO
    /// </summary>
    public class HrAttendanceAdjustmentQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Begin Attendance Date for range filter
        /// </summary>
        public DateTime? BeginAttendanceDate { get; set; }

        /// <summary>
        /// End Attendance Date for range filter
        /// </summary>
        public DateTime? EndAttendanceDate { get; set; }

        /// <summary>
        /// Adjustment Type Dict Code filter
        /// </summary>
        public long? AdjustmentTypeDictCode { get; set; }

        /// <summary>
        /// Status Dict Code filter (Pending, Approved, Rejected)
        /// </summary>
        public long? StatusDictCode { get; set; }

        /// <summary>
        /// Requested By filter
        /// </summary>
        public long? RequestedBy { get; set; }

        /// <summary>
        /// Approved By filter
        /// </summary>
        public long? ApprovedBy { get; set; }

        /// <summary>
        /// Applied To Snapshot filter
        /// </summary>
        public bool? AppliedToSnapshot { get; set; }
    }

    /// <summary>
    /// Attendance Adjustment DTO - for read/display operations
    /// </summary>
    public class HrAttendanceAdjustmentDto
    {
        [ExcelColumn(Name = "Adjustment ID")]
        [ExcelColumnName("Adjustment ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AdjustmentId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Attendance Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Attendance Date")]
        public DateTime AttendanceDate { get; set; }

        [ExcelColumn(Name = "Shift ID")]
        [ExcelColumnName("Shift ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShiftId { get; set; }

        [ExcelColumn(Name = "Segment ID")]
        [ExcelColumnName("Segment ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SegmentId { get; set; }

        [ExcelColumn(Name = "Adjustment Type Dict Code")]
        [ExcelColumnName("Adjustment Type Dict Code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AdjustmentTypeDictCode { get; set; }

        [ExcelColumn(Name = "Adjusted Minutes")]
        [ExcelColumnName("Adjusted Minutes")]
        public int AdjustedMinutes { get; set; }

        [ExcelColumn(Name = "Adjustment Field")]
        [ExcelColumnName("Adjustment Field")]
        public string AdjustmentField { get; set; }

        [ExcelColumn(Name = "Reason")]
        [ExcelColumnName("Reason")]
        public string Reason { get; set; }

        [ExcelColumn(Name = "Requested By")]
        [ExcelColumnName("Requested By")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequestedBy { get; set; }

        [ExcelColumn(Name = "Approved By")]
        [ExcelColumnName("Approved By")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApprovedBy { get; set; }

        [ExcelColumn(Name = "Approval Date", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Approval Date")]
        public DateTime? ApprovalDate { get; set; }

        [ExcelColumn(Name = "Status Dict Code")]
        [ExcelColumnName("Status Dict Code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? StatusDictCode { get; set; }

        [ExcelColumn(Name = "Applied To Snapshot")]
        [ExcelColumnName("Applied To Snapshot")]
        public bool AppliedToSnapshot { get; set; }

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
        public bool IsApproved => ApprovedBy.HasValue && ApprovalDate.HasValue;

        [ExcelIgnore]
        public string IsApprovedLabel => IsApproved ? "Approved" : "Pending";

        [ExcelIgnore]
        public string AppliedToSnapshotLabel => AppliedToSnapshot ? "Applied" : "Not Applied";
    }

    /// <summary>
    /// Create Attendance Adjustment DTO (renamed from AttendanceAdjustmentRequestDto)
    /// </summary>
    public class CreateHrAttendanceAdjustmentDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Attendance date is required")]
        public DateTime AttendanceDate { get; set; }

        public long? ShiftId { get; set; }

        public long? SegmentId { get; set; }

        public long? AdjustmentTypeDictCode { get; set; }

        [Required(ErrorMessage = "Adjusted minutes is required")]
        public int AdjustedMinutes { get; set; }

        [Required(ErrorMessage = "Adjustment field is required")]
        public string AdjustmentField { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Requested by is required")]
        public long RequestedBy { get; set; }

        public string Remark { get; set; }

        // Legacy compatibility
        public long AdjustedBy { get => RequestedBy; set => RequestedBy = value; }
    }

    /// <summary>
    /// Approve Adjustment DTO
    /// </summary>
    public class ApproveAdjustmentDto
    {
        [Required(ErrorMessage = "Adjustment ID is required")]
        public long AdjustmentId { get; set; }

        [Required(ErrorMessage = "Approved by is required")]
        public long ApprovedBy { get; set; }

        public string Remark { get; set; }
    }

    // ==================== HrPayrollAttendanceSummary DTOs ====================

    /// <summary>
    /// Payroll Attendance Summary Query DTO
    /// </summary>
    public class PayrollAttendanceSummaryQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Payroll Period filter (YYYY-MM format)
        /// </summary>
        public string PayrollPeriod { get; set; }

        /// <summary>
        /// Is Finalized filter
        /// </summary>
        public bool? IsFinalized { get; set; }

        /// <summary>
        /// Finalized By filter
        /// </summary>
        public long? FinalizedBy { get; set; }

        /// <summary>
        /// Begin Finalized Date for range filter
        /// </summary>
        public DateTime? BeginFinalizedDate { get; set; }

        /// <summary>
        /// End Finalized Date for range filter
        /// </summary>
        public DateTime? EndFinalizedDate { get; set; }
    }

    /// <summary>
    /// Payroll Attendance Summary DTO - for read/display operations (Enhanced from existing)
    /// </summary>
    public class PayrollAttendanceSummaryDto
    {
        [ExcelColumn(Name = "Summary ID")]
        [ExcelColumnName("Summary ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SummaryId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Payroll Period")]
        [ExcelColumnName("Payroll Period")]
        public string PayrollPeriod { get; set; }

        [ExcelColumn(Name = "Total Expected Minutes")]
        [ExcelColumnName("Total Expected Minutes")]
        public int TotalExpectedMinutes { get; set; }

        [ExcelColumn(Name = "Total Worked Minutes")]
        [ExcelColumnName("Total Worked Minutes")]
        public int TotalWorkedMinutes { get; set; }

        [ExcelColumn(Name = "Total Late Minutes")]
        [ExcelColumnName("Total Late Minutes")]
        public int TotalLateMinutes { get; set; }

        [ExcelColumn(Name = "Total Early Leave Minutes")]
        [ExcelColumnName("Total Early Leave Minutes")]
        public int TotalEarlyLeaveMinutes { get; set; }

        [ExcelColumn(Name = "Total Absent Minutes")]
        [ExcelColumnName("Total Absent Minutes")]
        public int TotalAbsentMinutes { get; set; }

        [ExcelColumn(Name = "Total Leave Minutes")]
        [ExcelColumnName("Total Leave Minutes")]
        public int TotalLeaveMinutes { get; set; }

        [ExcelColumn(Name = "Total Approved Overtime Minutes")]
        [ExcelColumnName("Total Approved Overtime Minutes")]
        public int TotalApprovedOvertimeMinutes { get; set; }

        [ExcelColumn(Name = "Total Weekend Overtime Minutes")]
        [ExcelColumnName("Total Weekend Overtime Minutes")]
        public int TotalWeekendOvertimeMinutes { get; set; }

        [ExcelColumn(Name = "Total Holiday Overtime Minutes")]
        [ExcelColumnName("Total Holiday Overtime Minutes")]
        public int TotalHolidayOvertimeMinutes { get; set; }

        [ExcelColumn(Name = "Days Present")]
        [ExcelColumnName("Days Present")]
        public int DaysPresent { get; set; }

        [ExcelColumn(Name = "Days Absent")]
        [ExcelColumnName("Days Absent")]
        public int DaysAbsent { get; set; }

        [ExcelColumn(Name = "Days On Leave")]
        [ExcelColumnName("Days On Leave")]
        public int DaysOnLeave { get; set; }

        [ExcelColumn(Name = "Calculated Salary Factor")]
        [ExcelColumnName("Calculated Salary Factor")]
        public decimal CalculatedSalaryFactor { get; set; }

        [ExcelColumn(Name = "Is Finalized")]
        [ExcelColumnName("Is Finalized")]
        public bool IsFinalized { get; set; }

        [ExcelColumn(Name = "Finalized Date", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Finalized Date")]
        public DateTime? FinalizedDate { get; set; }

        [ExcelColumn(Name = "Finalized By")]
        [ExcelColumnName("Finalized By")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? FinalizedBy { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsFinalizedLabel => IsFinalized ? "Finalized" : "Draft";
    }

    /// <summary>
    /// Payroll Summary Generation Request DTO (existing)
    /// </summary>
    public class PayrollSummaryGenerationRequestDto
    {
        [Required(ErrorMessage = "Payroll period is required")]
        public string PayrollPeriod { get; set; }

        public List<long> EmployeeIds { get; set; }

        public bool ForceRegenerate { get; set; } = false;
    }

    /// <summary>
    /// Payroll Summary Finalize DTO (existing)
    /// </summary>
    public class PayrollSummaryFinalizeDto
    {
        [Required(ErrorMessage = "Payroll period is required")]
        public string PayrollPeriod { get; set; }

        [Required(ErrorMessage = "Finalized by is required")]
        public long FinalizedBy { get; set; }

        public List<long> SummaryIds { get; set; }
    }
}
