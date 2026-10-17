using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.AttendanceProcessing
{
    /// <summary>
    /// HR Attendance Segment Result Model
    /// </summary>
    [SugarTable("hr_attendance_segment_result")]
    public class HrAttendanceSegmentResult : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long EmployeeId { get; set; }

        [SugarColumn(IsPrimaryKey = true)]
        public DateTime AttendanceDate { get; set; }

        [SugarColumn(IsPrimaryKey = true)]
        public long SegmentId { get; set; }

        public int ExpectedMinutes { get; set; }

        public int WorkedMinutes { get; set; }

        public bool IsMissed { get; set; }

        public string Remark { get; set; }
    }
}
