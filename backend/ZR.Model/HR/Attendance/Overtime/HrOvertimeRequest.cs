using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Converter;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.Overtime
{
    /// <summary>
    /// HR Overtime Request Model
    /// </summary>
    [SugarTable("hr_overtime_request")]
    public class HrOvertimeRequest : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long OvertimeRequestId { get; set; }

        public long EmployeeId { get; set; }

        public DateTime WorkDate { get; set; }

        public TimeSpan PlannedStartTime { get; set; }
        public TimeSpan PlannedEndTime { get; set; }

        public int PlannedMinutes { get; set; }

        [SugarColumn(Length = 300)]
        public string Reason { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long StatusDictCode { get; set; } // Pending / Approved / Rejected

        public bool IsFinalized { get; set; }
    }

}
