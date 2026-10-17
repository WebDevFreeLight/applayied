
using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using MiniExcelLibs.Attributes;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.AttendanceCapture
{
    /// <summary>
    /// Attendance Device Table
    /// </summary>
    [SugarTable("hr_attendance_device")]
    
    [SqlDict(
        dictType: "attendance_device",
        dictName: "Hr Attendance Device",
        valueColumn: "devicecode",
        labelColumns: "devicename",
        RemarkColumn = "remark",
        OrderBy = "devicecode ASC",
        Description = "Auto-generated from HrAttendanceDevice table"
    )]
    public class HrAttendanceDevice : SysBase
    {
        /// <summary>
        /// Device ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeviceId { get; set; }

        /// <summary>
        /// Device name
        /// </summary>
        [SugarColumn(Length = 100)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Device code
        /// </summary>
        [SugarColumn(Length = 50)]
        public string DeviceCode { get; set; }

        /// <summary>
        /// Device type (biometric, card_reader, face_recognition, mobile)
        /// </summary>
        [SugarColumn(Length = 50)]
        public string DeviceType { get; set; }

        /// <summary>
        /// Device model
        /// </summary>
        [SugarColumn(Length = 100)]
        public string DeviceModel { get; set; }

        /// <summary>
        /// Brand
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Brand { get; set; }

        /// <summary>
        /// Model
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Model { get; set; }

        /// <summary>
        /// Manufacturer
        /// </summary>
        [SugarColumn(Length = 100)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Serial number
        /// </summary>
        [SugarColumn(Length = 100)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// IP address
        /// </summary>
        [SugarColumn(Length = 50)]
        public string IpAddress { get; set; }

        /// <summary>
        /// Port number
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// MAC address
        /// </summary>
        [SugarColumn(Length = 50)]
        public string MacAddress { get; set; }

        /// <summary>
        /// Location
        /// </summary>
        [SugarColumn(Length = 200)]
        public string Location { get; set; }

        /// <summary>
        /// Department ID
        /// </summary>

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Is online
        /// </summary>
        public bool? IsOnline { get; set; }

        /// <summary>
        /// Connection type (TCP, UDP, HTTP, SDK)
        /// </summary>
        [SugarColumn(Length = 20)]
        public string ConnectionType { get; set; }

        /// <summary>
        /// Connection string/parameters
        /// </summary>
        [SugarColumn(Length = 500)]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Username for device access
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Username { get; set; }

        /// <summary>
        /// Password for device access
        /// </summary>
        [SugarColumn(Length = 100)]
        public string Password { get; set; }

        /// <summary>
        /// SDK version
        /// </summary>
        [SugarColumn(Length = 50)]
        public string SdkVersion { get; set; }

        /// <summary>
        /// Firmware version
        /// </summary>
        [SugarColumn(Length = 50)]
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// Last sync time
        /// </summary>
        public DateTime? LastSyncTime { get; set; }

        /// <summary>
        /// Last sync status
        /// </summary>
        [SugarColumn(Length = 100)]
        public string LastSyncStatus { get; set; }

        /// <summary>
        /// Last sync records count
        /// </summary>
        public int? LastSyncRecords { get; set; }

        /// <summary>
        /// Last ping time
        /// </summary>
        public DateTime? LastPingTime { get; set; }

        /// <summary>
        /// Auto sync enabled
        /// </summary>
        public bool AutoSyncEnabled { get; set; }

        /// <summary>
        /// Sync interval (minutes)
        /// </summary>
        public int? SyncInterval { get; set; }

        /// <summary>
        /// Maximum users capacity
        /// </summary>
        public int? MaxUsers { get; set; }

        /// <summary>
        /// Current users count
        /// </summary>
        public int? CurrentUsers { get; set; }

        /// <summary>
        /// Maximum logs capacity
        /// </summary>
        public int? MaxLogs { get; set; }

        /// <summary>
        /// Current logs count
        /// </summary>
        public int? CurrentLogs { get; set; }

        /// <summary>
        /// Is deleted
        /// </summary>
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// Installation date
        /// </summary>
        public DateTime? InstallationDate { get; set; }

        /// <summary>
        /// Last maintenance date
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// Next maintenance date
        /// </summary>
        public DateTime? NextMaintenanceDate { get; set; }
    }
}

