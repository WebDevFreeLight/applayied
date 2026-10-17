using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Shifts_Scheduling
{
    /// <summary>
    /// HR Shift Segment Model
    /// </summary>
    [SugarTable("hr_shift_segment")]
    public class HrShiftSegment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long SegmentId { get; set; }

        public long ShiftId { get; set; }

        public int SegmentOrder { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsBreak { get; set; }

        public bool IsPaid { get; set; }

        public int? MinMinutesRequired { get; set; }
    }
}
