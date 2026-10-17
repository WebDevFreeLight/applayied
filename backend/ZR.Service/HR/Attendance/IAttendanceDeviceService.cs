using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.AttendanceCapture;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Attendance Device Service Interface
    /// </summary>
    public interface IAttendanceDeviceService
    {
        /// <summary>
        /// Get list of active attendance devices
        /// </summary>
        Task<List<HrAttendanceDevice>> GetActiveDevicesAsync();

        /// <summary>
        /// Get device by ID
        /// </summary>
        Task<HrAttendanceDevice> GetByIdAsync(long deviceId);

        /// <summary>
        /// Create new device
        /// </summary>
        Task<long> CreateDeviceAsync(CreateHrAttendanceDeviceDto dto);

        /// <summary>
        /// Update existing device
        /// </summary>
        Task<bool> UpdateDeviceAsync(long deviceId, UpdateHrAttendanceDeviceDto dto);

        /// <summary>
        /// Delete device
        /// </summary>
        Task<bool> DeleteDeviceAsync(long deviceId);

        /// <summary>
        /// Sync device data
        /// </summary>
        Task<int> SyncDeviceDataAsync(long deviceId);

        /// <summary>
        /// Test device connection
        /// </summary>
        Task<bool> TestDeviceConnectionAsync(long deviceId);

        /// <summary>
        /// Get device status
        /// </summary>
        Task<object> GetDeviceStatusAsync();

        /// <summary>
        /// Sync all devices
        /// </summary>
        Task<object> SyncAllDevicesAsync();

        /// <summary>
        /// Get device logs
        /// </summary>
        Task<List<object>> GetDeviceLogsAsync(long deviceId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Enable device
        /// </summary>
        Task<bool> EnableDeviceAsync(long deviceId);

        /// <summary>
        /// Disable device
        /// </summary>
        Task<bool> DisableDeviceAsync(long deviceId);
    }
}
