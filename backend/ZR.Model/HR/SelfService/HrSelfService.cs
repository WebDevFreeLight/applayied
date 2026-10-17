 
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR
{
    /// <summary>
    /// Employee Profile for Self-Service
    /// </summary>
    public class HrEmployeeProfile
    {
        public string PersonalEmail { get; set; }
        public string MobileNumber { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string Address { get; set; }
    }

    /// <summary>
    /// Emergency Contact Table
    /// </summary>
    [SugarTable("hr_emergency_contact")]
    public class HrEmergencyContact : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ContactId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 100)]
        public string ContactName { get; set; }

        
        public string Relationship { get; set; }

        [SugarColumn(Length = 20)]
        public string PhoneNumber { get; set; }

        [SugarColumn(Length = 20)]
        public string AlternatePhone { get; set; }

        [SugarColumn(Length = 100)]
        public string Email { get; set; }

        
        public string Address { get; set; }

        public bool IsPrimary { get; set; }

 
    }

    /// <summary>
    /// Attendance Summary
    /// </summary>
    public class HrAttendanceSummary
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyDays { get; set; }
        public int LeaveDays { get; set; }
        public int HolidayDays { get; set; }
        public int WeekendDays { get; set; }
        public decimal TotalWorkHours { get; set; }
        public decimal TotalOvertimeHours { get; set; }
        public decimal AverageWorkHours { get; set; }
        public decimal TotalWorkingHours { get; set; }
    }

    /// <summary>
    /// Compensation Summary
    /// </summary>
    public class HrCompensationSummary
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public int Year { get; set; }
        public decimal CurrentBaseSalary { get; set; }
        public decimal YtdGrossEarnings { get; set; }
        public decimal YtdNetEarnings { get; set; }
        public decimal YtdTaxDeductions { get; set; }
        public decimal YtdBenefitDeductions { get; set; }
        public int EnrolledBenefits { get; set; }
        public decimal MonthlyBenefitCost { get; set; }
    }

    /// <summary>
    /// Team Attendance Summary for Managers
    /// </summary>
    public class HrTeamAttendanceSummary
    {
        public DateTime Date { get; set; }
        public int TotalTeamMembers { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int OnLeave { get; set; }
        public int Late { get; set; }
        public int WorkFromHome { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    /// <summary>
    /// Employee Activity Log Table
    /// </summary>
    [SugarTable("hr_employee_activity")]
    public class HrEmployeeActivity : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ActivityId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

         
        public string ActivityType { get; set; }

        
        public string Description { get; set; }

        public DateTime ActivityDate { get; set; }

        
        public string IpAddress { get; set; }

        [SugarColumn(Length = 200)]
        public string UserAgent { get; set; }

        [SugarColumn(Length = 100)]
        public string SessionId { get; set; }

     
    }

    /// <summary>
    /// Shift Table
    /// </summary>
    [SugarTable("hr_shift")]
    public class HrShift : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShiftId { get; set; }

        
        public string ShiftCode { get; set; }

        [SugarColumn(Length = 100)]
        public string ShiftName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal WorkHours { get; set; }

        public decimal BreakHours { get; set; }

        public int GracePeriodMinutes { get; set; }

        public bool IsNightShift { get; set; }

        public bool IsFlexible { get; set; }

        [SugarColumn(Length = 20)]
        public string WeekendDays { get; set; } // Friday,Saturday for Saudi Arabia

        public bool IsActive { get; set; }

         
    }

    /// <summary>
    /// Employee Portal Settings Table
    /// </summary>
    [SugarTable("hr_portal_settings")]
    public class HrPortalSettings : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SettingId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        
        public string Language { get; set; }

        
        public string Theme { get; set; }

        public bool EmailNotifications { get; set; }

        public bool SmsNotifications { get; set; }

        public bool PushNotifications { get; set; }

        [SugarColumn(Length = 20)]
        public string DateFormat { get; set; }

        [SugarColumn(Length = 20)]
        public string TimeFormat { get; set; }

        
        public string Timezone { get; set; }

        public bool TwoFactorEnabled { get; set; }

        
        public string DashboardWidgets { get; set; } // JSON string

        
        public string Preferences { get; set; } // JSON string
         
    }

    /// <summary>
    /// Employee Request Table (Generic)
    /// </summary>
    [SugarTable("hr_employee_request")]
    public class HrEmployeeRequest : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequestId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        
        public string RequestType { get; set; } // letter, certificate, loan, advance, etc.

        [SugarColumn(Length = 200)]
        public string Subject { get; set; }

        
        public string Description { get; set; }

        [SugarColumn(Length = 20)]
        public string Priority { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; }

        
        public string RequestData { get; set; } // JSON string

        
        public string Attachments { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        
        public string ProcessedBy { get; set; }

        
        public string Comments { get; set; }

        

    }

    /// <summary>
    /// Employee Feedback Table
    /// </summary>
    [SugarTable("hr_employee_feedback")]
    public class HrEmployeeFeedback : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long FeedbackId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        
        public string FeedbackType { get; set; } // suggestion, complaint, appreciation

        
        public string Category { get; set; }

        [SugarColumn(Length = 200)]
        public string Subject { get; set; }

        
        public string Message { get; set; }

        public bool IsAnonymous { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // pending, acknowledged, resolved

        public int? Rating { get; set; }

        
        public string Response { get; set; }

        
        public string RespondedBy { get; set; }

        public DateTime? ResponseDate { get; set; }



         
    }

    /// <summary>
    /// Employee Survey Response Table
    /// </summary>
    [SugarTable("hr_survey_response")]
    public class HrSurveyResponse : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ResponseId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long SurveyId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        
        public string Responses { get; set; } // JSON string of question-answer pairs

        public bool IsComplete { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? SubmitTime { get; set; }

        public decimal? Score { get; set; }

        
    }

    /// <summary>
    /// Employee Survey Table
    /// </summary>
    [SugarTable("hr_survey")]
    public class HrSurvey : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SurveyId { get; set; }

        [SugarColumn(Length = 200)]
        public string SurveyTitle { get; set; }

        
        public string SurveyType { get; set; } // engagement, satisfaction, feedback, exit

        
        public string Description { get; set; }

        
        public string Questions { get; set; } // JSON string

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsMandatory { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; }

        public int TotalResponses { get; set; }
 
         
    }
}

