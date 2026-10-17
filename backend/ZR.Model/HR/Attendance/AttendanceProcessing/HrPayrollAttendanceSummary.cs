using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.AttendanceProcessing
{
    /// <summary>
    /// HR Payroll Attendance Summary Model
    /// Aggregates all attendance data for a payroll period per employee
    /// Critical for payroll calculation accuracy
    /// </summary>
    [SugarTable("hr_payroll_attendance_summary")]
    public class HrPayrollAttendanceSummary : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SummaryId { get; set; }

        /// <summary>
        /// Employee ID (FK to hr_employee_core)
        /// </summary>
        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        /// <summary>
        /// Payroll period (format: YYYY-MM, e.g., "2025-01")
        /// </summary>
        [SugarColumn(ColumnName = "payroll_period", Length = 20)]
        public string PayrollPeriod { get; set; }

        /// <summary>
        /// Total expected working minutes for the period
        /// </summary>
        [SugarColumn(ColumnName = "total_expected_minutes")]
        public int TotalExpectedMinutes { get; set; }

        /// <summary>
        /// Total actual worked minutes
        /// </summary>
        [SugarColumn(ColumnName = "total_worked_minutes")]
        public int TotalWorkedMinutes { get; set; }

        /// <summary>
        /// Total late arrival minutes
        /// </summary>
        [SugarColumn(ColumnName = "total_late_minutes")]
        public int TotalLateMinutes { get; set; }

        /// <summary>
        /// Total early departure minutes
        /// </summary>
        [SugarColumn(ColumnName = "total_early_leave_minutes")]
        public int TotalEarlyLeaveMinutes { get; set; }

        /// <summary>
        /// Total absent minutes (expected but not worked)
        /// </summary>
        [SugarColumn(ColumnName = "total_absent_minutes")]
        public int TotalAbsentMinutes { get; set; }

        /// <summary>
        /// Total leave minutes (from approved leaves)
        /// </summary>
        [SugarColumn(ColumnName = "total_leave_minutes")]
        public int TotalLeaveMinutes { get; set; }

        /// <summary>
        /// Total approved overtime minutes (regular daily overtime)
        /// </summary>
        [SugarColumn(ColumnName = "total_approved_overtime_minutes")]
        public int TotalApprovedOvertimeMinutes { get; set; }

        /// <summary>
        /// Total weekend overtime minutes
        /// </summary>
        [SugarColumn(ColumnName = "total_weekend_overtime_minutes")]
        public int TotalWeekendOvertimeMinutes { get; set; }

        /// <summary>
        /// Total holiday overtime minutes
        /// </summary>
        [SugarColumn(ColumnName = "total_holiday_overtime_minutes")]
        public int TotalHolidayOvertimeMinutes { get; set; }

        /// <summary>
        /// Number of days present (days with worked minutes > 0)
        /// </summary>
        [SugarColumn(ColumnName = "days_present")]
        public int DaysPresent { get; set; }

        /// <summary>
        /// Number of days absent
        /// </summary>
        [SugarColumn(ColumnName = "days_absent")]
        public int DaysAbsent { get; set; }

        /// <summary>
        /// Number of days on leave
        /// </summary>
        [SugarColumn(ColumnName = "days_on_leave")]
        public int DaysOnLeave { get; set; }

        /// <summary>
        /// Calculated salary factor (days-based)
        /// Formula: days_present / expected_working_days
        /// Gulf region standard: simpler than minutes-based calculation
        /// </summary>
        [SugarColumn(ColumnName = "calculated_salary_factor", DecimalDigits = 4)]
        public decimal CalculatedSalaryFactor { get; set; }

        /// <summary>
        /// Is this summary finalized and locked for payroll
        /// </summary>
        [SugarColumn(ColumnName = "is_finalized")]
        public bool IsFinalized { get; set; } = false;

        /// <summary>
        /// Date when summary was finalized
        /// </summary>
        [SugarColumn(ColumnName = "finalized_date", IsNullable = true)]
        public DateTime? FinalizedDate { get; set; }

        /// <summary>
        /// User who finalized this summary
        /// </summary>
        [SugarColumn(ColumnName = "finalized_by", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? FinalizedBy { get; set; }

        /// <summary>
        /// Remarks or notes about this summary
        /// </summary>
        [SugarColumn(ColumnName = "remark", Length = 1000, IsNullable = true)]
        public string Remark { get; set; }
    }
}
