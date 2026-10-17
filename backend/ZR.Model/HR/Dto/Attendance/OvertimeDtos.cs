using System;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;

namespace ZR.Model.HR.Dto.Attendance
{
    // ==================== HrOvertimeRequest DTOs ====================

    /// <summary>
    /// Overtime Request Query DTO
    /// </summary>
    public class HrOvertimeRequestQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Begin Request Date for range filter
        /// </summary>
        public DateTime? BeginRequestDate { get; set; }

        /// <summary>
        /// End Request Date for range filter
        /// </summary>
        public DateTime? EndRequestDate { get; set; }

        /// <summary>
        /// Status Dict Code filter
        /// </summary>
        public long? StatusDictCode { get; set; }

        /// <summary>
        /// Approver ID filter
        /// </summary>
        public long? ApproverId { get; set; }

        /// <summary>
        /// Is Approved filter
        /// </summary>
        public bool? IsApproved { get; set; }
    }

    /// <summary>
    /// Overtime Request DTO - for read/display operations
    /// </summary>
    public class HrOvertimeRequestDto
    {
        [ExcelColumn(Name = "Overtime Request ID")]
        [ExcelColumnName("Overtime Request ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OvertimeRequestId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Request Date", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Request Date")]
        public DateTime RequestDate { get; set; }

        [ExcelColumn(Name = "Work Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Work Date")]
        public DateTime WorkDate { get; set; }

        [ExcelColumn(Name = "Planned Start Time")]
        [ExcelColumnName("Planned Start Time")]
        public TimeSpan PlannedStartTime { get; set; }

        [ExcelColumn(Name = "Planned End Time")]
        [ExcelColumnName("Planned End Time")]
        public TimeSpan PlannedEndTime { get; set; }

        [ExcelColumn(Name = "Planned Hours")]
        [ExcelColumnName("Planned Hours")]
        public decimal PlannedHours { get; set; }

        [ExcelColumn(Name = "Requested Minutes")]
        [ExcelColumnName("Requested Minutes")]
        public int RequestedMinutes { get; set; }

        [ExcelColumn(Name = "Reason")]
        [ExcelColumnName("Reason")]
        public string Reason { get; set; }

        [ExcelColumn(Name = "Status Dict Code")]
        [ExcelColumnName("Status Dict Code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? StatusDictCode { get; set; }

        [ExcelColumn(Name = "Approver ID")]
        [ExcelColumnName("Approver ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApproverId { get; set; }

        [ExcelColumn(Name = "Approval Date", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Approval Date")]
        public DateTime? ApprovalDate { get; set; }

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
        public bool IsApproved => ApproverId.HasValue && ApprovalDate.HasValue;

        [ExcelIgnore]
        public string IsApprovedLabel => IsApproved ? "Approved" : "Pending";
    }

    /// <summary>
    /// Overtime Request Create DTO (existing)
    /// </summary>
    public class OvertimeRequestCreateDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Work date is required")]
        public DateTime WorkDate { get; set; }

        [Required(ErrorMessage = "Requested minutes is required")]
        public decimal RequestedMinutes { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        public TimeSpan? PlannedStartTime { get; set; }

        public TimeSpan? PlannedEndTime { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Approve Overtime Request DTO
    /// </summary>
    public class ApproveOvertimeRequestDto
    {
        [Required(ErrorMessage = "Overtime request ID is required")]
        public long OvertimeRequestId { get; set; }

        [Required(ErrorMessage = "Approver ID is required")]
        public long ApproverId { get; set; }

        public decimal? ApprovedMinutes { get; set; }

        public string ApprovalComments { get; set; }
    }

    // ==================== HrOvertimeResult DTOs ====================

    /// <summary>
    /// Overtime Result Query DTO
    /// </summary>
    public class OvertimeResultQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Begin Work Date for range filter
        /// </summary>
        public DateTime? BeginWorkDate { get; set; }

        /// <summary>
        /// End Work Date for range filter
        /// </summary>
        public DateTime? EndWorkDate { get; set; }

        /// <summary>
        /// Overtime Type Dict Code filter
        /// </summary>
        public long? OvertimeTypeDictCode { get; set; }

        /// <summary>
        /// Is Weekend filter
        /// </summary>
        public bool? IsWeekend { get; set; }

        /// <summary>
        /// Is Holiday filter
        /// </summary>
        public bool? IsHoliday { get; set; }
    }

    /// <summary>
    /// Overtime Result DTO - for read/display operations
    /// </summary>
    public class OvertimeResultDto
    {
        [ExcelColumn(Name = "Overtime Result ID")]
        [ExcelColumnName("Overtime Result ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OvertimeResultId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Attendance Day ID")]
        [ExcelColumnName("Attendance Day ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AttendanceDayId { get; set; }

        [ExcelColumn(Name = "Work Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Work Date")]
        public DateTime WorkDate { get; set; }

        [ExcelColumn(Name = "Overtime Minutes")]
        [ExcelColumnName("Overtime Minutes")]
        public int OvertimeMinutes { get; set; }

        [ExcelColumn(Name = "Overtime Type Dict Code")]
        [ExcelColumnName("Overtime Type Dict Code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OvertimeTypeDictCode { get; set; }

        [ExcelColumn(Name = "Overtime Rate")]
        [ExcelColumnName("Overtime Rate")]
        public decimal OvertimeRate { get; set; }

        [ExcelColumn(Name = "Is Weekend")]
        [ExcelColumnName("Is Weekend")]
        public bool IsWeekend { get; set; }

        [ExcelColumn(Name = "Is Holiday")]
        [ExcelColumnName("Is Holiday")]
        public bool IsHoliday { get; set; }

        [ExcelColumn(Name = "Overtime Hours")]
        [ExcelColumnName("Overtime Hours")]
        public decimal OvertimeHours => OvertimeMinutes / 60m;

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
        public string IsWeekendLabel => IsWeekend ? "Weekend" : "Weekday";

        [ExcelIgnore]
        public string IsHolidayLabel => IsHoliday ? "Holiday" : "Regular Day";

        [ExcelIgnore]
        public string OvertimeTypeLabel => IsHoliday ? "Holiday OT" : IsWeekend ? "Weekend OT" : "Regular OT";
    }

    /// <summary>
    /// Overtime Summary DTO (Enhanced from existing)
    /// </summary>
    public class OvertimeSummaryDto
    {
        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "From Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("From Date")]
        public DateTime FromDate { get; set; }

        [ExcelColumn(Name = "To Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("To Date")]
        public DateTime ToDate { get; set; }

        [ExcelColumn(Name = "Total Overtime Minutes")]
        [ExcelColumnName("Total Overtime Minutes")]
        public int TotalOvertimeMinutes { get; set; }

        [ExcelColumn(Name = "Weekend Overtime Minutes")]
        [ExcelColumnName("Weekend Overtime Minutes")]
        public int WeekendOvertimeMinutes { get; set; }

        [ExcelColumn(Name = "Holiday Overtime Minutes")]
        [ExcelColumnName("Holiday Overtime Minutes")]
        public int HolidayOvertimeMinutes { get; set; }

        [ExcelColumn(Name = "Total Overtime Amount")]
        [ExcelColumnName("Total Overtime Amount")]
        public decimal TotalOvertimeAmount { get; set; }

        [ExcelColumn(Name = "Total Overtime Hours")]
        [ExcelColumnName("Total Overtime Hours")]
        public decimal TotalOvertimeHours => TotalOvertimeMinutes / 60m;

        [ExcelColumn(Name = "Weekend Overtime Hours")]
        [ExcelColumnName("Weekend Overtime Hours")]
        public decimal WeekendOvertimeHours => WeekendOvertimeMinutes / 60m;

        [ExcelColumn(Name = "Holiday Overtime Hours")]
        [ExcelColumnName("Holiday Overtime Hours")]
        public decimal HolidayOvertimeHours => HolidayOvertimeMinutes / 60m;
    }

    // ==================== HrOvertimePolicy DTOs ====================

    /// <summary>
    /// Overtime Policy DTO - Simple CRUD
    /// </summary>
    public class OvertimePolicyDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OvertimePolicyId { get; set; }

        public string PolicyName { get; set; }

        public string PolicyCode { get; set; }

        public decimal RegularOvertimeRate { get; set; } // e.g., 1.5x

        public decimal WeekendOvertimeRate { get; set; } // e.g., 2.0x

        public decimal HolidayOvertimeRate { get; set; } // e.g., 2.5x

        public int MinimumOvertimeMinutes { get; set; } // Minimum minutes to count as overtime

        public int MaxDailyOvertimeMinutes { get; set; } // Maximum overtime allowed per day

        public int MaxMonthlyOvertimeMinutes { get; set; } // Maximum overtime allowed per month

        public bool RequireApproval { get; set; }

        public bool AutoCalculate { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public string Remark { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsActiveLabel => IsActive ? "Active" : "Inactive";

        [ExcelIgnore]
        public string RequireApprovalLabel => RequireApproval ? "Required" : "Not Required";

        [ExcelIgnore]
        public string AutoCalculateLabel => AutoCalculate ? "Auto" : "Manual";
    }

    /// <summary>
    /// Create Overtime Request DTO - Alias for controller compatibility
    /// </summary>
    public class CreateHrOvertimeRequestDto : OvertimeRequestCreateDto
    {
        public string RequestedBy { get; set; }
        public DateTime RequestDate { get; set; }
    }

    /// <summary>
    /// Create Overtime Policy DTO
    /// </summary>
    public class CreateHrOvertimePolicyDto
    {
        [Required(ErrorMessage = "Policy name is required")]
        public string PolicyName { get; set; }

        [Required(ErrorMessage = "Policy code is required")]
        public string PolicyCode { get; set; }

        [Required(ErrorMessage = "Regular overtime rate is required")]
        public decimal RegularOvertimeRate { get; set; } = 1.5m;

        [Required(ErrorMessage = "Weekend overtime rate is required")]
        public decimal WeekendOvertimeRate { get; set; } = 2.0m;

        [Required(ErrorMessage = "Holiday overtime rate is required")]
        public decimal HolidayOvertimeRate { get; set; } = 2.5m;

        public int MinimumOvertimeMinutes { get; set; } = 30;
        public int MaxDailyOvertimeMinutes { get; set; } = 480; // 8 hours
        public int MaxMonthlyOvertimeMinutes { get; set; } = 4800; // 80 hours

        public bool RequireApproval { get; set; } = true;
        public bool AutoCalculate { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public string Description { get; set; }
        public string Remark { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Update Overtime Policy DTO
    /// </summary>
    public class UpdateHrOvertimePolicyDto
    {
        [Required(ErrorMessage = "Policy name is required")]
        public string PolicyName { get; set; }

        [Required(ErrorMessage = "Policy code is required")]
        public string PolicyCode { get; set; }

        [Required(ErrorMessage = "Regular overtime rate is required")]
        public decimal RegularOvertimeRate { get; set; }

        [Required(ErrorMessage = "Weekend overtime rate is required")]
        public decimal WeekendOvertimeRate { get; set; }

        [Required(ErrorMessage = "Holiday overtime rate is required")]
        public decimal HolidayOvertimeRate { get; set; }

        public int MinimumOvertimeMinutes { get; set; }
        public int MaxDailyOvertimeMinutes { get; set; }
        public int MaxMonthlyOvertimeMinutes { get; set; }

        public bool RequireApproval { get; set; }
        public bool AutoCalculate { get; set; }
        public bool IsActive { get; set; }

        public string Description { get; set; }
        public string Remark { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
