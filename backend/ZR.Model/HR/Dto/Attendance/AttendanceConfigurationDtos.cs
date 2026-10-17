using System;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;

namespace ZR.Model.HR.Dto.Attendance
{
    // ==================== HrWorkCalendar DTOs ====================

    /// <summary>
    /// Work Calendar Query DTO
    /// </summary>
    public class HrWorkCalendarQueryDto : PagerInfo
    {
        /// <summary>
        /// Calendar Name filter
        /// </summary>
        public string CalendarName { get; set; }

        /// <summary>
        /// Is Active filter
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Is Default filter
        /// </summary>
        public bool? IsDefault { get; set; }
    }

    /// <summary>
    /// Work Calendar DTO - for read/display operations (Enhanced from existing)
    /// </summary>
    public class WorkCalendarDto
    {
        [ExcelColumn(Name = "Calendar ID")]
        [ExcelColumnName("Calendar ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CalendarId { get; set; }

        [ExcelColumn(Name = "Calendar Name")]
        [ExcelColumnName("Calendar Name")]
        public string CalendarName { get; set; }

        [ExcelColumn(Name = "Working Days Pattern")]
        [ExcelColumnName("Working Days Pattern")]
        public string WorkingDaysPattern { get; set; }

        [ExcelColumn(Name = "Sunday Is Working")]
        [ExcelColumnName("Sunday Is Working")]
        public bool SundayIsWorking { get; set; }

        [ExcelColumn(Name = "Monday Is Working")]
        [ExcelColumnName("Monday Is Working")]
        public bool MondayIsWorking { get; set; }

        [ExcelColumn(Name = "Tuesday Is Working")]
        [ExcelColumnName("Tuesday Is Working")]
        public bool TuesdayIsWorking { get; set; }

        [ExcelColumn(Name = "Wednesday Is Working")]
        [ExcelColumnName("Wednesday Is Working")]
        public bool WednesdayIsWorking { get; set; }

        [ExcelColumn(Name = "Thursday Is Working")]
        [ExcelColumnName("Thursday Is Working")]
        public bool ThursdayIsWorking { get; set; }

        [ExcelColumn(Name = "Friday Is Working")]
        [ExcelColumnName("Friday Is Working")]
        public bool FridayIsWorking { get; set; }

        [ExcelColumn(Name = "Saturday Is Working")]
        [ExcelColumnName("Saturday Is Working")]
        public bool SaturdayIsWorking { get; set; }

        [ExcelColumn(Name = "Default Start Time")]
        [ExcelColumnName("Default Start Time")]
        public TimeSpan? DefaultStartTime { get; set; }

        [ExcelColumn(Name = "Default End Time")]
        [ExcelColumnName("Default End Time")]
        public TimeSpan? DefaultEndTime { get; set; }

        [ExcelColumn(Name = "Is Default")]
        [ExcelColumnName("Is Default")]
        public bool IsDefault { get; set; }

        [ExcelColumn(Name = "Is Active")]
        [ExcelColumnName("Is Active")]
        public bool IsActive { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsActiveLabel => IsActive ? "Active" : "Inactive";

        [ExcelIgnore]
        public string IsDefaultLabel => IsDefault ? "Default" : "Custom";
    }

    /// <summary>
    /// Create Work Calendar DTO
    /// </summary>
    public class CreateWorkCalendarDto
    {
        [Required(ErrorMessage = "Calendar name is required")]
        public string CalendarName { get; set; }

        public string WorkingDaysPattern { get; set; }

        public bool SundayIsWorking { get; set; } = false;

        public bool MondayIsWorking { get; set; } = true;

        public bool TuesdayIsWorking { get; set; } = true;

        public bool WednesdayIsWorking { get; set; } = true;

        public bool ThursdayIsWorking { get; set; } = true;

        public bool FridayIsWorking { get; set; } = false;

        public bool SaturdayIsWorking { get; set; } = false;

        public TimeSpan? DefaultStartTime { get; set; }

        public TimeSpan? DefaultEndTime { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update Work Calendar DTO
    /// </summary>
    public class UpdateWorkCalendarDto
    {
        [Required(ErrorMessage = "Calendar ID is required")]
        public long CalendarId { get; set; }

        [Required(ErrorMessage = "Calendar name is required")]
        public string CalendarName { get; set; }

        public string WorkingDaysPattern { get; set; }

        public bool SundayIsWorking { get; set; }

        public bool MondayIsWorking { get; set; }

        public bool TuesdayIsWorking { get; set; }

        public bool WednesdayIsWorking { get; set; }

        public bool ThursdayIsWorking { get; set; }

        public bool FridayIsWorking { get; set; }

        public bool SaturdayIsWorking { get; set; }

        public TimeSpan? DefaultStartTime { get; set; }

        public TimeSpan? DefaultEndTime { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }
    }

    // ==================== HrEmployeeCalendarAssignment DTOs ====================

    /// <summary>
    /// Calendar Assignment Query DTO
    /// </summary>
    public class CalendarAssignmentQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Calendar ID filter
        /// </summary>
        public long? CalendarId { get; set; }

        /// <summary>
        /// Is Primary filter
        /// </summary>
        public bool? IsPrimary { get; set; }

        /// <summary>
        /// Effective From filter
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        /// <summary>
        /// Effective To filter
        /// </summary>
        public DateTime? EffectiveTo { get; set; }
    }

    /// <summary>
    /// Calendar Assignment DTO - for read/display operations
    /// </summary>
    public class CalendarAssignmentDto
    {
        [ExcelColumn(Name = "Assignment ID")]
        [ExcelColumnName("Assignment ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignmentId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Calendar ID")]
        [ExcelColumnName("Calendar ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CalendarId { get; set; }

        [ExcelColumn(Name = "Effective From", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Effective From")]
        public DateTime EffectiveFrom { get; set; }

        [ExcelColumn(Name = "Effective To", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Effective To")]
        public DateTime? EffectiveTo { get; set; }

        [ExcelColumn(Name = "Is Primary")]
        [ExcelColumnName("Is Primary")]
        public bool IsPrimary { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsPrimaryLabel => IsPrimary ? "Primary" : "Secondary";

        [ExcelIgnore]
        public bool IsActive => (!EffectiveTo.HasValue || EffectiveTo.Value >= DateTime.Now) && EffectiveFrom <= DateTime.Now;

        [ExcelIgnore]
        public string IsActiveLabel => IsActive ? "Active" : "Inactive";
    }

    /// <summary>
    /// Calendar Assignment Request DTO (existing)
    /// </summary>
    public class CalendarAssignmentRequestDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Calendar ID is required")]
        public long CalendarId { get; set; }

        [Required(ErrorMessage = "Effective from date is required")]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsPrimary { get; set; } = true;
    }

    // ==================== HrHoliday DTOs ====================

    /// <summary>
    /// Holiday Query DTO
    /// </summary>
    public class HrHolidayQueryDto : PagerInfo
    {
        /// <summary>
        /// Holiday Name filter
        /// </summary>
        public string HolidayName { get; set; }

        /// <summary>
        /// Begin Holiday Date for range filter
        /// </summary>
        public DateTime? BeginHolidayDate { get; set; }

        /// <summary>
        /// End Holiday Date for range filter
        /// </summary>
        public DateTime? EndHolidayDate { get; set; }

        /// <summary>
        /// Holiday Type filter
        /// </summary>
        public string HolidayType { get; set; }

        /// <summary>
        /// Is Active filter
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Is Paid filter
        /// </summary>
        public bool? IsPaid { get; set; }

        /// <summary>
        /// Is Recurring filter
        /// </summary>
        public bool? IsRecurring { get; set; }
    }

    /// <summary>
    /// Holiday DTO - for read/display operations
    /// </summary>
    public class HrHolidayDto
    {
        [ExcelColumn(Name = "Holiday ID")]
        [ExcelColumnName("Holiday ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HolidayId { get; set; }

        [ExcelColumn(Name = "Holiday Name")]
        [ExcelColumnName("Holiday Name")]
        public string HolidayName { get; set; }

        [ExcelColumn(Name = "Holiday Name (AR)")]
        [ExcelColumnName("Holiday Name (AR)")]
        public string HolidayNameAr { get; set; }

        [ExcelColumn(Name = "Holiday Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Holiday Date")]
        public DateTime HolidayDate { get; set; }

        [ExcelColumn(Name = "Holiday Type")]
        [ExcelColumnName("Holiday Type")]
        public string HolidayType { get; set; }

        [ExcelColumn(Name = "Is Paid")]
        [ExcelColumnName("Is Paid")]
        public bool IsPaid { get; set; }

        [ExcelColumn(Name = "Is Recurring")]
        [ExcelColumnName("Is Recurring")]
        public bool IsRecurring { get; set; }

        [ExcelColumn(Name = "Is Active")]
        [ExcelColumnName("Is Active")]
        public bool IsActive { get; set; }

        [ExcelColumn(Name = "Description")]
        [ExcelColumnName("Description")]
        public string Description { get; set; }

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
        public string IsPaidLabel => IsPaid ? "Paid" : "Unpaid";

        [ExcelIgnore]
        public string IsRecurringLabel => IsRecurring ? "Recurring" : "One-time";

        [ExcelIgnore]
        public string IsActiveLabel => IsActive ? "Active" : "Inactive";
    }

    /// <summary>
    /// Create Holiday DTO
    /// </summary>
    public class CreateHrHolidayDto
    {
        [Required(ErrorMessage = "Holiday name is required")]
        public string HolidayName { get; set; }

        public string HolidayNameAr { get; set; }

        [Required(ErrorMessage = "Holiday date is required")]
        public DateTime HolidayDate { get; set; }

        public string HolidayType { get; set; }

        public bool IsPaid { get; set; } = true;

        public bool IsRecurring { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public string Description { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Update Holiday DTO
    /// </summary>
    public class UpdateHrHolidayDto
    {
        [Required(ErrorMessage = "Holiday ID is required")]
        public long HolidayId { get; set; }

        [Required(ErrorMessage = "Holiday name is required")]
        public string HolidayName { get; set; }

        public string HolidayNameAr { get; set; }

        [Required(ErrorMessage = "Holiday date is required")]
        public DateTime HolidayDate { get; set; }

        public string HolidayType { get; set; }

        public bool IsPaid { get; set; }

        public bool IsRecurring { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public string Remark { get; set; }
    }

    // ==================== HrAttendancePolicy DTOs ====================

    /// <summary>
    /// Attendance Policy DTO - Simple CRUD
    /// </summary>
    public class HrAttendancePolicyDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PolicyId { get; set; }

        public string PolicyName { get; set; }

        public string PolicyCode { get; set; }

        public int LateGraceMinutes { get; set; }

        public int EarlyLeaveGraceMinutes { get; set; }

        public int MinimumWorkMinutes { get; set; }

        public bool AutoCalculateOvertime { get; set; }

        public int OvertimeThresholdMinutes { get; set; }

        public bool RequireCheckInOut { get; set; }

        public bool AllowRemoteCheckIn { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public string Remark { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsActiveLabel => IsActive ? "Active" : "Inactive";

        [ExcelIgnore]
        public string AutoCalculateOvertimeLabel => AutoCalculateOvertime ? "Yes" : "No";

        [ExcelIgnore]
        public string RequireCheckInOutLabel => RequireCheckInOut ? "Required" : "Optional";

        [ExcelIgnore]
        public string AllowRemoteCheckInLabel => AllowRemoteCheckIn ? "Allowed" : "Not Allowed";
    }
}
