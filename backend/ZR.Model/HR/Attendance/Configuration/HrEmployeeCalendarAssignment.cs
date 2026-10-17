using Infrastructure.Converter;
using Newtonsoft.Json;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.Configuration
{
    /// <summary>
    /// HR Employee Calendar Assignment Model
    /// Links employees to work calendars for determining working days
    /// </summary>
    [SugarTable("hr_employee_calendar_assignment")]
    public class HrEmployeeCalendarAssignment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignmentId { get; set; }

        /// <summary>
        /// Employee ID (FK to hr_employee_core)
        /// </summary>
        [SugarColumn(ColumnName = "employee_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        /// <summary>
        /// Calendar ID (FK to hr_work_calendar)
        /// </summary>
        [SugarColumn(ColumnName = "calendar_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CalendarId { get; set; }

        /// <summary>
        /// Effective start date for this calendar assignment
        /// </summary>
        [SugarColumn(ColumnName = "effective_from")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Effective end date (null = currently active)
        /// </summary>
        [SugarColumn(ColumnName = "effective_to", IsNullable = true)]
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Is this the primary calendar for the employee
        /// </summary>
        [SugarColumn(ColumnName = "is_primary")]
        public bool IsPrimary { get; set; } = true;

        /// <summary>
        /// Remarks or notes about this assignment
        /// </summary>
        [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
        public string Remark { get; set; }
    }
}
