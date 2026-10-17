using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.HR.Attendance.Configuration
{
    /// <summary>
    /// HR Attendance Policy Model
    /// </summary>
    [SugarTable("hr_attendance_policy")]
    public class HrAttendancePolicy : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long PolicyId { get; set; }

        public string PolicyName { get; set; }

        public int StandardDailyMinutes { get; set; }

        public int MaxLateMinutes { get; set; }

        public int MaxEarlyLeaveMinutes { get; set; }

        public int MinMinutesForPresent { get; set; }

        public int OvertimeAfterMinutes { get; set; }

        public bool AllowSplitShift { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RoundingRuleDictCode { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }
    }
}
