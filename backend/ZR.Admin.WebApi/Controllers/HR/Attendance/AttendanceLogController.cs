using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto.Attendance;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Attendance
{
    /// <summary>
    /// Attendance Log Management Controller - Raw attendance logs from devices
    /// </summary>
    [Verify]
    [Route("api/hr/attendance/logs")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class AttendanceLogController : BaseController
    {
        private readonly IAttendanceLogService _logService;

        public AttendanceLogController(IAttendanceLogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Get attendance logs with pagination
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetAttendanceLogs([FromQuery] HrAttendanceLogQueryDto dto)
        {
            var response = await _logService.GetPagedLogsAsync(dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get log by ID
        /// </summary>
        [HttpGet("{logId}")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetLog(long logId)
        {
            var log = await _logService.GetByIdAsync(logId);
            if (log == null)
                return ToResponse(ResultCode.FAIL, "Log not found");

            return SUCCESS(log);
        }

        /// <summary>
        /// Get employee logs for date
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetEmployeeLogs(long employeeId, [FromQuery] DateTime date)
        {
            var logs = await _logService.GetEmployeeLogsAsync(employeeId, date);
            return SUCCESS(logs);
        }

        /// <summary>
        /// Get employee logs for date range
        /// </summary>
        [HttpGet("employee/{employeeId}/range")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetEmployeeLogsRange(long employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await _logService.GetEmployeeLogsRangeAsync(employeeId, startDate, endDate);
            return SUCCESS(logs);
        }

        /// <summary>
        /// Get unprocessed logs
        /// </summary>
        [HttpGet("unprocessed")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetUnprocessedLogs([FromQuery] DateTime date)
        {
            var logs = await _logService.GetUnprocessedLogsAsync(date);
            return SUCCESS(logs);
        }

        /// <summary>
        /// Mark logs as processed
        /// </summary>
        [HttpPost("mark-processed")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Mark Logs Processed", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> MarkLogsAsProcessed([FromBody] List<long> logIds)
        {
            var result = await _logService.MarkLogsAsProcessedAsync(logIds);
            return SUCCESS(new { ProcessedCount = result });
        }

        /// <summary>
        /// Add manual attendance log
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("hr:attendance:add")]
        [Log(Title = "Add Attendance Log", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddLog([FromBody] CreateHrAttendanceLogDto dto)
        {
            var result = await _logService.CreateLogAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete log
        /// </summary>
        [HttpDelete("{logId}")]
        [ActionPermissionFilter("hr:attendance:delete")]
        [Log(Title = "Delete Attendance Log", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteLog(long logId)
        {
            var result = await _logService.DeleteAsync(logId);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get device logs for date
        /// </summary>
        [HttpGet("device/{deviceId}")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetDeviceLogs(long deviceId, [FromQuery] DateTime date)
        {
            var logs = await _logService.GetDeviceLogsAsync(deviceId, date);
            return SUCCESS(logs);
        }

        /// <summary>
        /// Get logs summary for date
        /// </summary>
        [HttpGet("summary")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetLogsSummary([FromQuery] DateTime date)
        {
            var summary = await _logService.GetLogsSummaryAsync(date);
            return SUCCESS(summary);
        }

        /// <summary>
        /// Get duplicate logs
        /// </summary>
        [HttpGet("duplicates")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetDuplicateLogs([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var duplicates = await _logService.GetDuplicateLogsAsync(
                startDate ?? DateTime.Today.AddDays(-7),
                endDate ?? DateTime.Today);
            return SUCCESS(duplicates);
        }

        /// <summary>
        /// Merge duplicate logs
        /// </summary>
        [HttpPost("merge-duplicates")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Merge Duplicate Logs", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> MergeDuplicateLogs([FromBody] MergeDuplicatesDto dto)
        {
            var result = await _logService.MergeDuplicateLogsAsync(dto.LogIds);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Import biometric data
        /// </summary>
        [HttpPost("import")]
        [ActionPermissionFilter("hr:attendance:import")]
        [Log(Title = "Import Attendance Logs", BusinessType = BusinessType.IMPORT)]
        public async Task<IActionResult> ImportBiometric([FromForm] BiometricImportDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "File is required");
            }

            var result = await _logService.ImportBiometricDataAsync(dto.File, dto.DeviceId);
            return SUCCESS(new { ImportedCount = result });
        }

        /// <summary>
        /// Export logs
        /// </summary>
        [HttpPost("export")]
        [ActionPermissionFilter("hr:attendance:export")]
        public async Task<IActionResult> ExportLogs([FromBody] ExportLogsDto dto)
        {
            try
            {
                var fileData = await _logService.ExportLogsAsync(dto);
                return File(fileData.Data, fileData.ContentType, fileData.FileName);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }
    }

    public class MergeDuplicatesDto
    {
        public List<long> LogIds { get; set; }
    }

    public class BiometricImportDto
    {
        public Microsoft.AspNetCore.Http.IFormFile File { get; set; }
        public long DeviceId { get; set; }
    }

    public class ExportLogsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long? EmployeeId { get; set; }
        public long? DeviceId { get; set; }
        public string Format { get; set; } = "xlsx"; // xlsx, csv, pdf
    }
}
