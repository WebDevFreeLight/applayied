using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Overtime
{
    [SugarTable("hr_overtime_segment")]
    public class HrOvertimeSegment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long SegmentId { get; set; }

        public long EmployeeId { get; set; }
        public DateTime WorkDate { get; set; }

        public DateTime ActualStartTime { get; set; }
        public DateTime ActualEndTime { get; set; }

        public int ActualWorkedMinutes { get; set; }

        public DateTime OvertimeStartTime { get; set; }
        public DateTime OvertimeEndTime { get; set; }

        public int OvertimeMinutes { get; set; }

        [SugarColumn(Length = 20)]
        public string OvertimeType { get; set; }

        public long AttendanceLogStartId { get; set; }
        public long AttendanceLogEndId { get; set; }
    }

}
