using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.Shifts_Scheduling
{
    /// <summary>
    /// HR Shift Model
    /// </summary>
    [SugarTable("hr_shift")]
    public class HrShift : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long ShiftId { get; set; }

        public string ShiftName { get; set; }

        public bool IsSplitShift { get; set; }

        public bool IsNightShift { get; set; }

        public bool CrossesMidnight { get; set; }

        public int TotalExpectedMinutes { get; set; }

        public bool IsActive { get; set; }

        // Legacy compatibility properties
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int GracePeriodMinutes { get; set; }
        public decimal WorkHours => TotalExpectedMinutes / 60m;
    }
}
