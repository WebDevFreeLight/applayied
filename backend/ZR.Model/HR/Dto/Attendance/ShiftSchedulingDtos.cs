using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;

namespace ZR.Model.HR.Dto.Attendance
{
    // ==================== HrShift DTOs ====================

    /// <summary>
    /// Shift Query DTO
    /// </summary>
    public class HrShiftQueryDto : PagerInfo
    {
        /// <summary>
        /// Shift Name filter
        /// </summary>
        public string ShiftName { get; set; }

        /// <summary>
        /// Is Split Shift filter
        /// </summary>
        public bool? IsSplitShift { get; set; }

        /// <summary>
        /// Is Night Shift filter
        /// </summary>
        public bool? IsNightShift { get; set; }

        /// <summary>
        /// Is Active filter
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Crosses Midnight filter
        /// </summary>
        public bool? CrossesMidnight { get; set; }
    }

    /// <summary>
    /// Shift DTO - for read/display operations (Enhanced from existing)
    /// </summary>
    public class ShiftDto
    {
        [ExcelColumn(Name = "Shift ID")]
        [ExcelColumnName("Shift ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShiftId { get; set; }

        [ExcelColumn(Name = "Shift Name")]
        [ExcelColumnName("Shift Name")]
        public string ShiftName { get; set; }

        [ExcelColumn(Name = "Is Split Shift")]
        [ExcelColumnName("Is Split Shift")]
        public bool IsSplitShift { get; set; }

        [ExcelColumn(Name = "Is Night Shift")]
        [ExcelColumnName("Is Night Shift")]
        public bool IsNightShift { get; set; }

        [ExcelColumn(Name = "Crosses Midnight")]
        [ExcelColumnName("Crosses Midnight")]
        public bool CrossesMidnight { get; set; }

        [ExcelColumn(Name = "Total Expected Minutes")]
        [ExcelColumnName("Total Expected Minutes")]
        public int TotalExpectedMinutes { get; set; }

        [ExcelColumn(Name = "Is Active")]
        [ExcelColumnName("Is Active")]
        public bool IsActive { get; set; }

        [ExcelColumn(Name = "Start Time")]
        [ExcelColumnName("Start Time")]
        public TimeSpan StartTime { get; set; }

        [ExcelColumn(Name = "End Time")]
        [ExcelColumnName("End Time")]
        public TimeSpan EndTime { get; set; }

        [ExcelColumn(Name = "Grace Period Minutes")]
        [ExcelColumnName("Grace Period Minutes")]
        public int GracePeriodMinutes { get; set; }

        [ExcelColumn(Name = "Work Hours")]
        [ExcelColumnName("Work Hours")]
        public decimal WorkHours => TotalExpectedMinutes / 60m;

        [ExcelColumn(Name = "Creator")]
        [ExcelColumnName("Creator")]
        public string CreateBy { get; set; }

        [ExcelColumn(Name = "Create Time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Create Time")]
        public DateTime? CreateTime { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        // Nested segments
        [ExcelIgnore]
        public List<ShiftSegmentDto> Segments { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsActiveLabel => IsActive ? "Active" : "Inactive";

        [ExcelIgnore]
        public string IsSplitShiftLabel => IsSplitShift ? "Split" : "Continuous";

        [ExcelIgnore]
        public string IsNightShiftLabel => IsNightShift ? "Night" : "Day";

        [ExcelIgnore]
        public string CrossesMidnightLabel => CrossesMidnight ? "Yes" : "No";
    }

    /// <summary>
    /// Create Shift DTO (includes nested segments)
    /// </summary>
    public class CreateShiftDto
    {
        [Required(ErrorMessage = "Shift name is required")]
        public string ShiftName { get; set; }

        public bool IsSplitShift { get; set; } = false;

        public bool IsNightShift { get; set; } = false;

        public bool CrossesMidnight { get; set; } = false;

        [Required(ErrorMessage = "Total expected minutes is required")]
        public int TotalExpectedMinutes { get; set; }

        public bool IsActive { get; set; } = true;

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public int GracePeriodMinutes { get; set; } = 15;

        public string Remark { get; set; }

        // Nested segments for shift creation
        [Required(ErrorMessage = "At least one shift segment is required")]
        public List<CreateShiftSegmentDto> Segments { get; set; }
    }

    /// <summary>
    /// Update Shift DTO
    /// </summary>
    public class UpdateShiftDto
    {
        [Required(ErrorMessage = "Shift ID is required")]
        public long ShiftId { get; set; }

        [Required(ErrorMessage = "Shift name is required")]
        public string ShiftName { get; set; }

        public bool IsSplitShift { get; set; }

        public bool IsNightShift { get; set; }

        public bool CrossesMidnight { get; set; }

        [Required(ErrorMessage = "Total expected minutes is required")]
        public int TotalExpectedMinutes { get; set; }

        public bool IsActive { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public int GracePeriodMinutes { get; set; }

        public string Remark { get; set; }

        // Optional: Update segments (if null, don't update segments)
        public List<UpdateShiftSegmentDto> Segments { get; set; }
    }

    // ==================== HrShiftSegment DTOs ====================

    /// <summary>
    /// Shift Segment DTO (existing)
    /// </summary>
    public class ShiftSegmentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SegmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShiftId { get; set; }

        public int SegmentOrder { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsBreak { get; set; }

        public bool IsPaid { get; set; }

        public int Duration { get; set; } // In minutes

        public string SegmentType { get; set; } // e.g., "Work", "Break", "Lunch"

        // Labels for display
        [ExcelIgnore]
        public string IsBreakLabel => IsBreak ? "Break" : "Work";

        [ExcelIgnore]
        public string IsPaidLabel => IsPaid ? "Paid" : "Unpaid";

        [ExcelIgnore]
        public decimal DurationHours => Duration / 60m;
    }

    /// <summary>
    /// Create Shift Segment DTO
    /// </summary>
    public class CreateShiftSegmentDto
    {
        [Required(ErrorMessage = "Segment order is required")]
        public int SegmentOrder { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }

        public bool IsBreak { get; set; } = false;

        public bool IsPaid { get; set; } = true;

        public string SegmentType { get; set; }
    }

    /// <summary>
    /// Update Shift Segment DTO
    /// </summary>
    public class UpdateShiftSegmentDto
    {
        public long? SegmentId { get; set; } // Null for new segments

        [Required(ErrorMessage = "Segment order is required")]
        public int SegmentOrder { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }

        public bool IsBreak { get; set; }

        public bool IsPaid { get; set; }

        public string SegmentType { get; set; }
    }

    // ==================== HrEmployeeShiftAssignment DTOs ====================

    /// <summary>
    /// Shift Assignment Query DTO
    /// </summary>
    public class ShiftAssignmentQueryDto : PagerInfo
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
        /// Policy ID filter
        /// </summary>
        public long? PolicyId { get; set; }

        /// <summary>
        /// Effective From filter
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        /// <summary>
        /// Effective To filter
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Is Active filter
        /// </summary>
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Shift Assignment DTO - for read/display operations
    /// </summary>
    public class ShiftAssignmentDto
    {
        [ExcelColumn(Name = "Assignment ID")]
        [ExcelColumnName("Assignment ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignmentId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Shift ID")]
        [ExcelColumnName("Shift ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShiftId { get; set; }

        [ExcelColumn(Name = "Policy ID")]
        [ExcelColumnName("Policy ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PolicyId { get; set; }

        [ExcelColumn(Name = "Effective From", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Effective From")]
        public DateTime EffectiveFrom { get; set; }

        [ExcelColumn(Name = "Effective To", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Effective To")]
        public DateTime? EffectiveTo { get; set; }

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
        public bool IsActive => (!EffectiveTo.HasValue || EffectiveTo.Value >= DateTime.Now) && EffectiveFrom <= DateTime.Now;

        [ExcelIgnore]
        public string IsActiveLabel => IsActive ? "Active" : "Inactive";
    }

    /// <summary>
    /// Shift Assignment Request DTO (existing)
    /// </summary>
    public class ShiftAssignmentRequestDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Shift ID is required")]
        public long ShiftId { get; set; }

        [Required(ErrorMessage = "Effective from date is required")]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public long? PolicyId { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Bulk Shift Assignment DTO (for assigning multiple employees to a shift)
    /// </summary>
    public class BulkShiftAssignmentDto
    {
        [Required(ErrorMessage = "Employee IDs are required")]
        public List<long> EmployeeIds { get; set; }

        [Required(ErrorMessage = "Shift ID is required")]
        public long ShiftId { get; set; }

        [Required(ErrorMessage = "Effective from date is required")]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public long? PolicyId { get; set; }

        public string Remark { get; set; }
    }

    // ==================== Controller Compatibility Aliases ====================

    /// <summary>
    /// Create Shift DTO - Alias for controller compatibility
    /// </summary>
    public class CreateHrShiftDto : CreateShiftDto
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Update Shift DTO - Alias for controller compatibility
    /// </summary>
    public class UpdateHrShiftDto : UpdateShiftDto
    {
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    /// <summary>
    /// Create Employee Shift Assignment DTO - Alias for controller compatibility
    /// </summary>
    public class CreateHrEmployeeShiftAssignmentDto : ShiftAssignmentRequestDto
    {
        public string AssignedBy { get; set; }
        public DateTime AssignedAt { get; set; }
    }

    // ==================== HrShiftCalendarOverride DTOs ====================

    /// <summary>
    /// Create Shift Calendar Override DTO
    /// </summary>
    public class CreateHrShiftCalendarOverrideDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Override date is required")]
        public DateTime OverrideDate { get; set; }

        public long? OriginalShiftId { get; set; }

        [Required(ErrorMessage = "Override shift ID is required")]
        public long OverrideShiftId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        public bool IsActive { get; set; } = true;

        public string Remark { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Update Shift Calendar Override DTO
    /// </summary>
    public class UpdateHrShiftCalendarOverrideDto
    {
        [Required(ErrorMessage = "Override shift ID is required")]
        public long OverrideShiftId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        public bool IsActive { get; set; }

        public string Remark { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
