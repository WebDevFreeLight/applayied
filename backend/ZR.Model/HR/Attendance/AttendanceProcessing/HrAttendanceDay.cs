using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Converter;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.AttendanceProcessing
{
    /// <summary>
    /// HR Attendance Day Model
    /// </summary>
    [SugarTable("hr_attendance_day")]
    public class HrAttendanceDay : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long EmployeeId { get; set; }

        [SugarColumn(IsPrimaryKey = true)]
        public DateTime AttendanceDate { get; set; }

        public long? ShiftId { get; set; }

        public long? PolicyId { get; set; }

        public int ExpectedMinutes { get; set; }

        public int WorkedMinutes { get; set; }

        public int LateMinutes { get; set; }

        public int EarlyLeaveMinutes { get; set; }

        public int OvertimeMinutes { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AttendanceStatusDictCode { get; set; }

        public bool IsAbsent { get; set; }

        public bool IsWeekend { get; set; }

        public bool IsHoliday { get; set; }

        // Additional compatibility properties
        public string Status { get; set; }
        public bool IsLate => LateMinutes > 0;
        public bool IsEarlyOut => EarlyLeaveMinutes > 0;
        public bool IsRemote { get; set; }

        // Punch time tracking (for compatibility with legacy code)
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public long? CheckInDeviceId { get; set; }
        public long? CheckOutDeviceId { get; set; }
        public string? CheckInLocation { get; set; }
        public string? CheckOutLocation { get; set; }

        // Calculated properties (settable for backward compatibility)
        public decimal WorkHours
        {
            get => WorkedMinutes / 60m;
            set => WorkedMinutes = (int)(value * 60);
        }
        public decimal OvertimeHours
        {
            get => OvertimeMinutes / 60m;
            set => OvertimeMinutes = (int)(value * 60);
        }

        // Legacy compatibility - AttendanceId (composite key, so using EmployeeId + Date hash)
        public long AttendanceId => EmployeeId;
    }
}
