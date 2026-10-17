 
using Infrastructure.Converter;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using ZR.Model.System;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Model.HR
{
    /// <summary>
    /// Manager Dashboard Model
    /// </summary>
    public class HrManagerDashboard
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ManagerId { get; set; }
        public DateTime GeneratedDate { get; set; }
        public int TotalTeamMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public int OnLeaveToday { get; set; }
        public int PendingLeaveRequests { get; set; }
        public int PendingPerformanceReviews { get; set; }
        public int PendingExpenseRequests { get; set; }
        public decimal TeamAverageRating { get; set; }
        public int GoalsOnTrack { get; set; }
        public int GoalsAtRisk { get; set; }
        public List<string> UpcomingBirthdays { get; set; }
        public List<string> UpcomingAnniversaries { get; set; }
        public List<string> UpcomingTrainings { get; set; }
        public List<HrManagerAlert> Alerts { get; set; }
    }

    /// <summary>
    /// Manager Alert Model
    /// </summary>
    public class HrManagerAlert
    {
        public string AlertType { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// Team Statistics Model
    /// </summary>
    public class HrTeamStatistics
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ManagerId { get; set; }
        public DateTime CalculatedDate { get; set; }
        public int TotalMembers { get; set; }
        public Dictionary<string, int> DepartmentDistribution { get; set; }
        public Dictionary<string, int> PositionDistribution { get; set; }
        public Dictionary<string, int> GenderDistribution { get; set; }
        public decimal AverageTenure { get; set; }
        public Dictionary<string, int> TopSkills { get; set; }
    }

    /// <summary>
    /// Employee Detail Model
    /// </summary>
    public class HrEmployeeDetail
    {
        public object Employee { get; set; } // Can be EmployeeApiDto or EmployeeFullDto
        public string CurrentAttendance { get; set; }
        public decimal LeaveBalance { get; set; }
        public decimal? PerformanceRating { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public decimal TrainingProgress { get; set; }
    }

    /// <summary>
    /// Attendance Report Model
    /// </summary>
    public class HrAttendanceReport
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int LateDays { get; set; }
        public decimal TotalWorkHours { get; set; }
        public decimal TotalOvertimeHours { get; set; }
        public decimal AttendancePercentage { get; set; }
    }

    /// <summary>
    /// Leave Analytics Model
    /// </summary>
    public class HrLeaveAnalytics
    {
        public int Year { get; set; }
        public int TeamSize { get; set; }
        public int TotalLeaveRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int PendingRequests { get; set; }
        public Dictionary<int, int> MonthlyDistribution { get; set; }
        public Dictionary<string, decimal> LeaveTypeDistribution { get; set; }
        public decimal AverageLeaveDaysPerEmployee { get; set; }
    }

    /// <summary>
    /// Team Performance Metrics Model
    /// </summary>
    public class HrTeamPerformanceMetrics
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ManagerId { get; set; }
        public DateTime CalculatedDate { get; set; }
        public decimal AverageRating { get; set; }
        public int HighPerformers { get; set; }
        public int LowPerformers { get; set; }
        public int TotalGoals { get; set; }
        public int GoalsOnTrack { get; set; }
        public int GoalsAtRisk { get; set; }
        public int GoalsCompleted { get; set; }
        public Dictionary<string, int> SkillsGapAnalysis { get; set; }
    }

    /// <summary>
    /// Manager Report Model
    /// </summary>
    [SugarTable("hr_manager_report")]
    public class HrManagerReport :   SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReportId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ManagerId { get; set; }

        [SugarColumn(Length = 50)]
        public string ReportType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        
        public string ReportData { get; set; }

        public DateTime GeneratedDate { get; set; }

        [SugarColumn(Length = 500)]
        public string FilePath { get; set; }
    }

    /// <summary>
    /// Expense Request Model
    /// </summary>
    [SugarTable("hr_expense_request")]
    public class HrExpenseRequest : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ExpenseId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 100)]
        public string ExpenseCategory { get; set; }

        public decimal Amount { get; set; }

        [SugarColumn(Length = 3)]
        public string Currency { get; set; }

        public DateTime ExpenseDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Description { get; set; }

        [SugarColumn(Length = 500)]
        public string Attachments { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; }

        [SugarColumn(Length = 50)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [SugarColumn(Length = 500)]
        public string ApprovalComments { get; set; }

        [SugarColumn(Length = 100)]
        public string PaymentReference { get; set; }

        public DateTime? PaymentDate { get; set; }
    }

    /// <summary>
    /// Employee Dashboard Widgets
    /// </summary>
    [SugarTable("hr_dashboard_widget")]
    public class HrDashboardWidget : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WidgetId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long UserId { get; set; }

        [SugarColumn(Length = 50)]
        public string WidgetType { get; set; }

        [SugarColumn(Length = 100)]
        public string WidgetTitle { get; set; }

        public int Position { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        
        public string Configuration { get; set; }

        public bool IsVisible { get; set; }

        public bool IsCollapsed { get; set; }
    }

    /// <summary>
    /// Quick Actions Model
    /// </summary>
    public class HrQuickAction
    {
        public string ActionType { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
        public string Permission { get; set; }
        public string Color { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Dashboard Notification
    /// </summary>
    public class HrDashboardNotification
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Priority { get; set; }
        public string ActionUrl { get; set; }
        public bool IsRead { get; set; }
    }
}

