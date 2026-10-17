using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Converter;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.Shifts_Scheduling
{
    /// <summary>
    /// HR Shift Calendar Override Model
    /// </summary>
    [SugarTable("hr_shift_calendar_override")]
    public class HrShiftCalendarOverride : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long OverrideId { get; set; }

        public long ShiftId { get; set; }

        public DateTime CalendarDate { get; set; }

        public TimeSpan? OverrideStartTime { get; set; }

        public TimeSpan? OverrideEndTime { get; set; }

        public bool IsWorkingDay { get; set; }

        public string Remark { get; set; }

        // Additional properties for employee-specific overrides
        public long? EmployeeId { get; set; }
        public long? OverrideShiftId { get; set; }

        // Compatibility properties
        public DateTime OverrideDate { get => CalendarDate; set => CalendarDate = value; }
        public string Reason { get => Remark; set => Remark = value; }
    }
}
