 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Attendance Query DTO
    /// </summary>
    public class AttendanceQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShiftId { get; set; }
        public bool? IsAbnormal { get; set; }
    }

    /// <summary>
    /// Attendance Input DTO
    /// </summary>
    public class AttendanceDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AttendanceId { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Attendance date is required")]
        public DateTime AttendanceDate { get; set; }
        
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShiftId { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? WorkHours { get; set; }
        public decimal? OvertimeHours { get; set; }
        public int? LateMinutes { get; set; }
        public int? EarlyLeaveMinutes { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        public bool IsAbnormal { get; set; }
        public string AbnormalReason { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CheckInDeviceId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CheckOutDeviceId { get; set; }
        public string CheckInLocation { get; set; }
        public string CheckOutLocation { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Attendance Log Query DTO
    /// </summary>
    public class AttendanceLogQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DeviceId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string LogType { get; set; }
        public bool? IsProcessed { get; set; }
    }

    /// <summary>
    /// Biometric Import DTO
    /// </summary>
    public class BiometricImportDto
    {
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; }
        
        [Required(ErrorMessage = "Device ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeviceId { get; set; }
        
        public DateTime? ImportDate { get; set; }
    }

    /// <summary>
    /// Attendance Shift DTO
    /// </summary>
    public class AttendanceShiftDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShiftId { get; set; }
        
        [Required(ErrorMessage = "Shift code is required")]
        public string ShiftCode { get; set; }
        
        [Required(ErrorMessage = "Shift name is required")]
        public string ShiftName { get; set; }
        
        public string ShiftNameAr { get; set; }
        
        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }
        
        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }
        
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
        public int? AllowLateMinutes { get; set; }
        public int? AllowEarlyMinutes { get; set; }
        public bool IsOvernight { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Assign Shift DTO
    /// </summary>
    public class AssignShiftDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Shift is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShiftId { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Manual Check In/Out DTO
    /// </summary>
    public class ManualCheckDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Time is required")]
        public DateTime CheckTime { get; set; }
        
        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } // check_in, check_out
        
        public string Reason { get; set; }
        public string Location { get; set; }
    }

    /// <summary>
    /// Attendance Device DTO
    /// </summary>
    public class AttendanceDeviceDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeviceId { get; set; }
        
        [Required(ErrorMessage = "Device code is required")]
        public string DeviceCode { get; set; }
        
        [Required(ErrorMessage = "Device name is required")]
        public string DeviceName { get; set; }
        
        public string DeviceType { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string IpAddress { get; set; }
        public int? Port { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Attendance Statistics DTO
    /// </summary>
    public class AttendanceStatisticsDto
    {
        public int TotalEmployees { get; set; }
        public int TotalDays { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLate { get; set; }
        public int TotalEarlyLeave { get; set; }
        public decimal TotalWorkHours { get; set; }
        public decimal TotalOvertimeHours { get; set; }
        public decimal AverageWorkHours { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    /// <summary>
    /// Employee Shift Assignment DTO
    /// </summary>
    public class EmployeeShiftAssignmentDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Shift is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShiftId { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        public string AssignmentReason { get; set; }
        public bool IsTemporary { get; set; }
        public string RotationPattern { get; set; }
    }

    /// <summary>
    /// Check In/Out DTO
    /// </summary>
    public class CheckInOutDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Time is required")]
        public DateTime Time { get; set; }
        
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DeviceId { get; set; }
        public string Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string VerifyMode { get; set; }
    }

    /// <summary>
    /// Bulk Attendance DTO
    /// </summary>
    public class BulkAttendanceDto
    {
        public DateTime Date { get; set; }
        public List<BulkAttendanceItem> Attendances { get; set; }

    }

    /// <summary>
    /// Bulk Attendance Item
    /// </summary>
    public class BulkAttendanceItem
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string Status { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string Remarks { get; set; }
    }

    /// <summary>
    /// Attendance Export DTO
    /// </summary>
    public class AttendanceExportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long? EmployeeId { get; set; }
        public long? DepartmentId { get; set; }
        public string Format { get; set; } = "excel";
        public List<string> Columns { get; set; }





          
    }

    /// <summary>
    /// Bulk Shift Assignment DTO
    /// </summary>
    public class BulkShiftAssignmentDto
    {
        public List<long> EmployeeIds { get; set; }
        public long ShiftId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }



    }
}

