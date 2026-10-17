
using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using MiniExcelLibs.Attributes;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.AttendanceCapture
{
    /// <summary>
    /// HR Attendance Log Model
    /// </summary>
    [SugarTable("hr_attendance_log")]
    public class HrAttendanceLog : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long LogId { get; set; }

        public long EmployeeId { get; set; }

        public DateTime LogTime { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? LogTypeDictCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SourceDictCode { get; set; }

        public long? DeviceId { get; set; }

        public bool IsProcessed { get; set; }

        // Additional properties for compatibility
        public string PunchType { get; set; } // "IN", "OUT", "CheckIn", "CheckOut"
        public string Location { get; set; }
        public string VerifyMode { get; set; }
        public DateTime? ProcessedTime { get; set; }
        public bool IsSynchronized { get; set; }

        // Compatibility properties
        public DateTime PunchTime { get => LogTime; set => LogTime = value; }
        public string LogType { get; set; }
    }

}

