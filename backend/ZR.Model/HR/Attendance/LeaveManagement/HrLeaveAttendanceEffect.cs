using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.LeaveManagement
{
    /// <summary>
    /// HR Leave Attendance Effect Model
    /// </summary>
    [SugarTable("hr_leave_attendance_effect")]
    public class HrLeaveAttendanceEffect : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long EffectId { get; set; }

        public long LeaveRequestId { get; set; }

        public long EmployeeId { get; set; }

        public DateTime AffectedDate { get; set; }

        public decimal DeductedMinutes { get; set; }

        public bool MarkedAsAbsent { get; set; }

        public bool AffectsOvertimeCalculation { get; set; }

        public string Remark { get; set; }
    }
}
