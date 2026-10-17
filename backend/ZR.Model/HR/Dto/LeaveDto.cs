 
using Infrastructure.Converter;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ZR.Model.System;


namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Leave Query DTO
    /// </summary>
    public class LeaveQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? LeaveTypeId { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public bool? IsEmergency { get; set; }
    }

    /// <summary>
    /// Leave Request DTO
    /// </summary>
    public class LeaveRequestDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequestId { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Leave type is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveTypeId { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
        
        [Required(ErrorMessage = "Number of days is required")]
        [Range(0.5, 365, ErrorMessage = "Days must be between 0.5 and 365")]
        public decimal Days { get; set; }
        
        [Required(ErrorMessage = "Reason is required")]
        [MaxLength(500)]
        public string Reason { get; set; }
        
        public string AttachmentUrl { get; set; }
        public bool IsEmergency { get; set; }
        public IFormFile AttachmentFile { get; set; }
    }






  

    /// <summary>
    /// Leave Type DTO
    /// </summary>
    public class LeaveTypeDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveTypeId { get; set; }
        
        [Required(ErrorMessage = "Type code is required")]
        public string TypeCode { get; set; }
        
        [Required(ErrorMessage = "Type name is required")]
        public string TypeName { get; set; }
        
        public string TypeNameAr { get; set; }
        public decimal? DaysPerYear { get; set; }
        public bool IsPaid { get; set; }
        public bool RequiresApproval { get; set; }
        public bool RequiresDocument { get; set; }
        public int? MinDays { get; set; }
        public int? MaxDays { get; set; }
        public int? AdvanceNoticeDays { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Leave Balance DTO
    /// </summary>
    public class LeaveBalanceDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BalanceId { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Leave type is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveTypeId { get; set; }
        
        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }
        
        public decimal EntitledDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal Balance { get; set; }
        public decimal CarriedOver { get; set; }
        public decimal ExpiredDays { get; set; }
    }

    /// <summary>
    /// Holiday DTO
    /// </summary>
    public class HolidayDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HolidayId { get; set; }
        
        [Required(ErrorMessage = "Holiday date is required")]
        public DateTime HolidayDate { get; set; }
        
        [Required(ErrorMessage = "Holiday name is required")]
        public string HolidayName { get; set; }
        
        public string HolidayNameAr { get; set; }
        public string HolidayType { get; set; }
        public bool IsPaid { get; set; }
    }

    /// <summary>
    /// Leave Approval DTO
    /// </summary>
    public class ApprovalDto
    {
        [Required(ErrorMessage = "Approval decision is required")]
        public bool Approved { get; set; }
        
        [MaxLength(500)]
        public string Comments { get; set; }
    }

    /// <summary>
    /// Leave Statistics DTO
    /// </summary>
    public class LeaveStatisticsDto
    {
        public int TotalRequests { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
        public decimal TotalDays { get; set; }
        public int EmergencyLeaves { get; set; }
        public Dictionary<string, int> ByLeaveType { get; set; }
        public Dictionary<string, int> ByMonth { get; set; }
        public decimal TotalMinutesRequested { get; set; }
        public decimal TotalMinutesApproved { get; set; }
        public decimal TotalDaysRequested => TotalMinutesRequested / 480; // 8 ÓÇÚÇÊ Úãá ÇÝÊÑÇÖí
        public decimal TotalDaysApproved => TotalMinutesApproved / 480;
    }

    /// <summary>
    /// Leave Balance Summary DTO
    /// </summary>
    public class LeaveBalanceSummaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int Year { get; set; }
        public List<LeaveTypeBalance> Balances { get; set; }
        public decimal TotalEntitled { get; set; }
        public decimal TotalUsed { get; set; }
        public decimal TotalRemaining { get; set; }
    }

    public class LeaveTypeBalance
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public decimal Entitled { get; set; }
        public decimal Used { get; set; }
        public decimal Balance { get; set; }
        public decimal CarriedOver { get; set; }
    }

    /// <summary>
    /// Leave Calendar Entry DTO
    /// </summary>
    public class LeaveCalendarDto
    {
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsHoliday { get; set; }
        public string HolidayName { get; set; }
        public int EmployeesOnLeave { get; set; }
        public List<EmployeeLeaveInfo> Leaves { get; set; }
    }

    public class EmployeeLeaveInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Calculate Days DTO
    /// </summary>
    public class CalculateDaysDto
    {
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
        
        public bool ExcludeHolidays { get; set; } = true;
    }
}

