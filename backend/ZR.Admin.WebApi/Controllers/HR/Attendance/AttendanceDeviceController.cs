using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto.Attendance;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Attendance
{
    /// <summary>
    /// Attendance Device Management Controller - Device registration, sync, status
    /// </summary>
    [Verify]
    [Route("api/hr/attendance/devices")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class AttendanceDeviceController : BaseController
    {
        private readonly IAttendanceDeviceService _deviceService;

        public AttendanceDeviceController(IAttendanceDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        /// <summary>
        /// Get all devices
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> GetDevices()
        {
            var devices = await _deviceService.GetActiveDevicesAsync();
            return SUCCESS(devices);
        }

        /// <summary>
        /// Get device by ID
        /// </summary>
        [HttpGet("{deviceId}")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetDevice(long deviceId)
        {
            var device = await _deviceService.GetByIdAsync(deviceId);
            if (device == null)
                return ToResponse(ResultCode.FAIL, "Device not found");

            return SUCCESS(device);
        }

        /// <summary>
        /// Add device
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("hr:attendance:add")]
        [Log(Title = "Device Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddDevice([FromBody] CreateHrAttendanceDeviceDto dto)
        {
            var result = await _deviceService.CreateDeviceAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update device
        /// </summary>
        [HttpPut("{deviceId}")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Device Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateDevice(long deviceId, [FromBody] UpdateHrAttendanceDeviceDto dto)
        {
            var result = await _deviceService.UpdateDeviceAsync(deviceId, dto);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL, "Update failed");
        }

        /// <summary>
        /// Delete device
        /// </summary>
        [HttpDelete("{deviceId}")]
        [ActionPermissionFilter("hr:attendance:delete")]
        [Log(Title = "Device Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteDevice(long deviceId)
        {
            var result = await _deviceService.DeleteDeviceAsync(deviceId);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Sync device data
        /// </summary>
        [HttpPost("{deviceId}/sync")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Device Sync", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> SyncDevice(long deviceId)
        {
            try
            {
                var result = await _deviceService.SyncDeviceDataAsync(deviceId);
                return SUCCESS(new { SyncedRecords = result });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Test device connection
        /// </summary>
        [HttpPost("{deviceId}/test")]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> TestDeviceConnection(long deviceId)
        {
            var result = await _deviceService.TestDeviceConnectionAsync(deviceId);
            return SUCCESS(new { IsConnected = result });
        }

        /// <summary>
        /// Get device status
        /// </summary>
        [HttpGet("status")]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> GetDeviceStatus()
        {
            try
            {
                var status = await _deviceService.GetDeviceStatusAsync();
                return SUCCESS(status);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Sync all devices
        /// </summary>
        [HttpPost("sync-all")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Sync All Devices", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> SyncAllDevices()
        {
            try
            {
                var result = await _deviceService.SyncAllDevicesAsync();
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get device logs
        /// </summary>
        [HttpGet("{deviceId}/logs")]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> GetDeviceLogs(long deviceId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var logs = await _deviceService.GetDeviceLogsAsync(
                deviceId,
                startDate ?? DateTime.Today.AddDays(-7),
                endDate ?? DateTime.Today);
            return SUCCESS(logs);
        }

        /// <summary>
        /// Enable device
        /// </summary>
        [HttpPost("{deviceId}/enable")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Enable Device", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> EnableDevice(long deviceId)
        {
            var result = await _deviceService.EnableDeviceAsync(deviceId);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Disable device
        /// </summary>
        [HttpPost("{deviceId}/disable")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Disable Device", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> DisableDevice(long deviceId)
        {
            var result = await _deviceService.DisableDeviceAsync(deviceId);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }
    }
}
