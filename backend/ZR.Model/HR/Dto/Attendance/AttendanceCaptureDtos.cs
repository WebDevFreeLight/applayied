using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using MiniExcelLibs.Attributes;
using Newtonsoft.Json;

namespace ZR.Model.HR.Dto.Attendance
{
    // ==================== HrAttendanceDevice DTOs ====================

    /// <summary>
    /// Attendance Device Query DTO
    /// </summary>
    public class HrAttendanceDeviceQueryDto : PagerInfo
    {
        /// <summary>
        /// Device Name filter
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Device Code filter
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// Device Type filter (biometric, card_reader, face_recognition, mobile)
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Location filter
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Department ID filter
        /// </summary>
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Is Active filter
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Is Online filter
        /// </summary>
        public bool? IsOnline { get; set; }

        /// <summary>
        /// IP Address filter
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Begin Create Time for range filter
        /// </summary>
        public DateTime? BeginCreateTime { get; set; }

        /// <summary>
        /// End Create Time for range filter
        /// </summary>
        public DateTime? EndCreateTime { get; set; }
    }

    /// <summary>
    /// Attendance Device DTO - for read/display operations
    /// </summary>
    public class HrAttendanceDeviceDto
    {
        [ExcelColumn(Name = "Device ID")]
        [ExcelColumnName("Device ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeviceId { get; set; }

        [ExcelColumn(Name = "Device Name")]
        [ExcelColumnName("Device Name")]
        public string DeviceName { get; set; }

        [ExcelColumn(Name = "Device Code")]
        [ExcelColumnName("Device Code")]
        public string DeviceCode { get; set; }

        [ExcelColumn(Name = "Device Type")]
        [ExcelColumnName("Device Type")]
        public string DeviceType { get; set; }

        [ExcelColumn(Name = "Device Model")]
        [ExcelColumnName("Device Model")]
        public string DeviceModel { get; set; }

        [ExcelColumn(Name = "Brand")]
        [ExcelColumnName("Brand")]
        public string Brand { get; set; }

        [ExcelColumn(Name = "Model")]
        [ExcelColumnName("Model")]
        public string Model { get; set; }

        [ExcelColumn(Name = "Manufacturer")]
        [ExcelColumnName("Manufacturer")]
        public string Manufacturer { get; set; }

        [ExcelColumn(Name = "Serial Number")]
        [ExcelColumnName("Serial Number")]
        public string SerialNumber { get; set; }

        [ExcelColumn(Name = "IP Address")]
        [ExcelColumnName("IP Address")]
        public string IpAddress { get; set; }

        [ExcelColumn(Name = "Port")]
        [ExcelColumnName("Port")]
        public int? Port { get; set; }

        [ExcelColumn(Name = "MAC Address")]
        [ExcelColumnName("MAC Address")]
        public string MacAddress { get; set; }

        [ExcelColumn(Name = "Location")]
        [ExcelColumnName("Location")]
        public string Location { get; set; }

        [ExcelColumn(Name = "Department ID")]
        [ExcelColumnName("Department ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [ExcelColumn(Name = "Is Active")]
        [ExcelColumnName("Is Active")]
        public bool? IsActive { get; set; }

        [ExcelColumn(Name = "Is Online")]
        [ExcelColumnName("Is Online")]
        public bool? IsOnline { get; set; }

        [ExcelColumn(Name = "Connection Type")]
        [ExcelColumnName("Connection Type")]
        public string ConnectionType { get; set; }

        [ExcelColumn(Name = "SDK Version")]
        [ExcelColumnName("SDK Version")]
        public string SdkVersion { get; set; }

        [ExcelColumn(Name = "Firmware Version")]
        [ExcelColumnName("Firmware Version")]
        public string FirmwareVersion { get; set; }

        [ExcelColumn(Name = "Last Sync Time", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Last Sync Time")]
        public DateTime? LastSyncTime { get; set; }

        [ExcelColumn(Name = "Last Sync Status")]
        [ExcelColumnName("Last Sync Status")]
        public string LastSyncStatus { get; set; }

        [ExcelColumn(Name = "Last Sync Records")]
        [ExcelColumnName("Last Sync Records")]
        public int? LastSyncRecords { get; set; }

        [ExcelColumn(Name = "Last Ping Time", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Last Ping Time")]
        public DateTime? LastPingTime { get; set; }

        [ExcelColumn(Name = "Auto Sync Enabled")]
        [ExcelColumnName("Auto Sync Enabled")]
        public bool AutoSyncEnabled { get; set; }

        [ExcelColumn(Name = "Sync Interval (Minutes)")]
        [ExcelColumnName("Sync Interval (Minutes)")]
        public int? SyncInterval { get; set; }

        [ExcelColumn(Name = "Max Users")]
        [ExcelColumnName("Max Users")]
        public int? MaxUsers { get; set; }

        [ExcelColumn(Name = "Current Users")]
        [ExcelColumnName("Current Users")]
        public int? CurrentUsers { get; set; }

        [ExcelColumn(Name = "Max Logs")]
        [ExcelColumnName("Max Logs")]
        public int? MaxLogs { get; set; }

        [ExcelColumn(Name = "Current Logs")]
        [ExcelColumnName("Current Logs")]
        public int? CurrentLogs { get; set; }

        [ExcelColumn(Name = "Installation Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Installation Date")]
        public DateTime? InstallationDate { get; set; }

        [ExcelColumn(Name = "Last Maintenance Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Last Maintenance Date")]
        public DateTime? LastMaintenanceDate { get; set; }

        [ExcelColumn(Name = "Next Maintenance Date", Format = "yyyy-MM-dd")]
        [ExcelColumnName("Next Maintenance Date")]
        public DateTime? NextMaintenanceDate { get; set; }

        [ExcelColumn(Name = "Creator")]
        [ExcelColumnName("Creator")]
        public string CreateBy { get; set; }

        [ExcelColumn(Name = "Create Time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Create Time")]
        public DateTime? CreateTime { get; set; }

        [ExcelColumn(Name = "Updater")]
        [ExcelColumnName("Updater")]
        public string UpdateBy { get; set; }

        [ExcelColumn(Name = "Update Time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Update Time")]
        public DateTime? UpdateTime { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsActiveLabel => IsActive == true ? "Active" : "Inactive";

        [ExcelIgnore]
        public string IsOnlineLabel => IsOnline == true ? "Online" : "Offline";

        [ExcelIgnore]
        public string AutoSyncEnabledLabel => AutoSyncEnabled ? "Enabled" : "Disabled";
    }

    /// <summary>
    /// Create Attendance Device DTO
    /// </summary>
    public class CreateHrAttendanceDeviceDto
    {
        [Required(ErrorMessage = "Device name is required")]
        public string DeviceName { get; set; }

        [Required(ErrorMessage = "Device code is required")]
        public string DeviceCode { get; set; }

        [Required(ErrorMessage = "Device type is required")]
        public string DeviceType { get; set; }

        public string DeviceModel { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public string Manufacturer { get; set; }

        public string SerialNumber { get; set; }

        public string IpAddress { get; set; }

        public int? Port { get; set; }

        public string MacAddress { get; set; }

        public string Location { get; set; }

        public long? DepartmentId { get; set; }

        public bool? IsActive { get; set; } = true;

        public string ConnectionType { get; set; }

        public string ConnectionString { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SdkVersion { get; set; }

        public string FirmwareVersion { get; set; }

        public bool AutoSyncEnabled { get; set; } = false;

        public int? SyncInterval { get; set; }

        public int? MaxUsers { get; set; }

        public int? MaxLogs { get; set; }

        public DateTime? InstallationDate { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// Update Attendance Device DTO
    /// </summary>
    public class UpdateHrAttendanceDeviceDto
    {
        [Required(ErrorMessage = "Device ID is required")]
        public long DeviceId { get; set; }

        [Required(ErrorMessage = "Device name is required")]
        public string DeviceName { get; set; }

        [Required(ErrorMessage = "Device code is required")]
        public string DeviceCode { get; set; }

        public string DeviceType { get; set; }

        public string DeviceModel { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public string Manufacturer { get; set; }

        public string SerialNumber { get; set; }

        public string IpAddress { get; set; }

        public int? Port { get; set; }

        public string MacAddress { get; set; }

        public string Location { get; set; }

        public long? DepartmentId { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsOnline { get; set; }

        public string ConnectionType { get; set; }

        public string ConnectionString { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SdkVersion { get; set; }

        public string FirmwareVersion { get; set; }

        public bool AutoSyncEnabled { get; set; }

        public int? SyncInterval { get; set; }

        public int? MaxUsers { get; set; }

        public int? CurrentUsers { get; set; }

        public int? MaxLogs { get; set; }

        public int? CurrentLogs { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        public string Remark { get; set; }
    }

    // ==================== HrAttendanceLog DTOs ====================

    /// <summary>
    /// Attendance Log Query DTO
    /// </summary>
    public class HrAttendanceLogQueryDto : PagerInfo
    {
        /// <summary>
        /// Employee ID filter
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// Device ID filter
        /// </summary>
        public long? DeviceId { get; set; }

        /// <summary>
        /// Punch Type filter (IN, OUT, CheckIn, CheckOut)
        /// </summary>
        public string PunchType { get; set; }

        /// <summary>
        /// Log Type filter
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// Is Processed filter
        /// </summary>
        public bool? IsProcessed { get; set; }

        /// <summary>
        /// Is Synchronized filter
        /// </summary>
        public bool? IsSynchronized { get; set; }

        /// <summary>
        /// Begin Log Time for range filter
        /// </summary>
        public DateTime? BeginLogTime { get; set; }

        /// <summary>
        /// End Log Time for range filter
        /// </summary>
        public DateTime? EndLogTime { get; set; }

        /// <summary>
        /// Location filter
        /// </summary>
        public string Location { get; set; }
    }

    /// <summary>
    /// Attendance Log DTO - for read/display operations
    /// </summary>
    public class HrAttendanceLogDto
    {
        [ExcelColumn(Name = "Log ID")]
        [ExcelColumnName("Log ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LogId { get; set; }

        [ExcelColumn(Name = "Employee ID")]
        [ExcelColumnName("Employee ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [ExcelColumn(Name = "Log Time", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Log Time")]
        public DateTime LogTime { get; set; }

        [ExcelColumn(Name = "Punch Time", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Punch Time")]
        public DateTime PunchTime { get; set; }

        [ExcelColumn(Name = "Punch Type")]
        [ExcelColumnName("Punch Type")]
        public string PunchType { get; set; }

        [ExcelColumn(Name = "Log Type")]
        [ExcelColumnName("Log Type")]
        public string LogType { get; set; }

        [ExcelColumn(Name = "Log Type Dict Code")]
        [ExcelColumnName("Log Type Dict Code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? LogTypeDictCode { get; set; }

        [ExcelColumn(Name = "Source Dict Code")]
        [ExcelColumnName("Source Dict Code")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SourceDictCode { get; set; }

        [ExcelColumn(Name = "Device ID")]
        [ExcelColumnName("Device ID")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DeviceId { get; set; }

        [ExcelColumn(Name = "Location")]
        [ExcelColumnName("Location")]
        public string Location { get; set; }

        [ExcelColumn(Name = "Verify Mode")]
        [ExcelColumnName("Verify Mode")]
        public string VerifyMode { get; set; }

        [ExcelColumn(Name = "Is Processed")]
        [ExcelColumnName("Is Processed")]
        public bool IsProcessed { get; set; }

        [ExcelColumn(Name = "Processed Time", Format = "yyyy-MM-dd HH:mm:ss")]
        [ExcelColumnName("Processed Time")]
        public DateTime? ProcessedTime { get; set; }

        [ExcelColumn(Name = "Is Synchronized")]
        [ExcelColumnName("Is Synchronized")]
        public bool IsSynchronized { get; set; }

        [ExcelColumn(Name = "Creator")]
        [ExcelColumnName("Creator")]
        public string CreateBy { get; set; }

        [ExcelColumn(Name = "Create Time", Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        [ExcelColumnName("Create Time")]
        public DateTime? CreateTime { get; set; }

        [ExcelColumn(Name = "Remark")]
        [ExcelColumnName("Remark")]
        public string Remark { get; set; }

        // Labels for display
        [ExcelIgnore]
        public string IsProcessedLabel => IsProcessed ? "Processed" : "Pending";

        [ExcelIgnore]
        public string IsSynchronizedLabel => IsSynchronized ? "Synced" : "Not Synced";
    }

    /// <summary>
    /// Import Attendance Logs DTO - for bulk import from devices
    /// </summary>
    public class ImportAttendanceLogsDto
    {
        [Required(ErrorMessage = "Device ID is required")]
        public long DeviceId { get; set; }

        [Required(ErrorMessage = "Logs are required")]
        public List<CreateHrAttendanceLogDto> Logs { get; set; }
    }

    /// <summary>
    /// Create Attendance Log DTO
    /// </summary>
    public class CreateHrAttendanceLogDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public long EmployeeId { get; set; }

        [Required(ErrorMessage = "Log time is required")]
        public DateTime LogTime { get; set; }

        public string PunchType { get; set; }

        public string LogType { get; set; }

        public long? LogTypeDictCode { get; set; }

        public long? SourceDictCode { get; set; }

        public long? DeviceId { get; set; }

        public string Location { get; set; }

        public string VerifyMode { get; set; }

        public bool IsProcessed { get; set; } = false;

        public bool IsSynchronized { get; set; } = false;

        public string Remark { get; set; }
    }
}
