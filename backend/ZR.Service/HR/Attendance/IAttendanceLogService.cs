using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ZR.Model;
using ZR.Model.HR.Attendance.AttendanceCapture;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Attendance Log Service Interface
    /// </summary>
    public interface IAttendanceLogService
    {
        /// <summary>
        /// Get paged list of attendance logs
        /// </summary>
        Task<PagedInfo<HrAttendanceLogDto>> GetPageListAsync(Expression<Func<HrAttendanceLog, bool>> predicate, HrAttendanceLogQueryDto pager);

        /// <summary>
        /// Get paged logs with query DTO
        /// </summary>
        Task<PagedInfo<HrAttendanceLogDto>> GetPagedLogsAsync(HrAttendanceLogQueryDto query);

        /// <summary>
        /// Get log by ID
        /// </summary>
        Task<HrAttendanceLog> GetByIdAsync(long logId);

        /// <summary>
        /// Mark logs as processed
        /// </summary>
        Task<int> MarkLogsAsProcessedAsync(List<long> logIds);

        /// <summary>
        /// Get unprocessed logs for a date
        /// </summary>
        Task<List<HrAttendanceLogDto>> GetUnprocessedLogsAsync(DateTime date);

        /// <summary>
        /// Get employee logs for a specific date
        /// </summary>
        Task<List<HrAttendanceLogDto>> GetEmployeeLogsAsync(long employeeId, DateTime date);

        /// <summary>
        /// Get employee logs for date range
        /// </summary>
        Task<List<HrAttendanceLogDto>> GetEmployeeLogsRangeAsync(long employeeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Process logs to create attendance records
        /// </summary>
        Task<int> ProcessLogsToAttendanceAsync(DateTime date);

        /// <summary>
        /// Import logs from device
        /// </summary>
        Task<int> ImportLogsFromDeviceAsync(ImportAttendanceLogsDto dto);

        /// <summary>
        /// Get logs by device
        /// </summary>
        Task<List<HrAttendanceLogDto>> GetLogsByDeviceAsync(long deviceId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get device logs for a specific date
        /// </summary>
        Task<List<HrAttendanceLogDto>> GetDeviceLogsAsync(long deviceId, DateTime date);

        /// <summary>
        /// Clean duplicate logs
        /// </summary>
        Task<int> CleanDuplicateLogsAsync(DateTime date);

        /// <summary>
        /// Get logs summary for date range
        /// </summary>
        Task<object> GetLogsSummaryAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get logs summary for single date
        /// </summary>
        Task<object> GetLogsSummaryAsync(DateTime date);

        /// <summary>
        /// Create manual attendance log
        /// </summary>
        Task<int> CreateLogAsync(CreateHrAttendanceLogDto dto);

        /// <summary>
        /// Delete attendance log
        /// </summary>
        Task<bool> DeleteAsync(long logId);

        /// <summary>
        /// Get duplicate logs
        /// </summary>
        Task<List<object>> GetDuplicateLogsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Merge duplicate logs
        /// </summary>
        Task<bool> MergeDuplicateLogsAsync(List<long> logIds);

        /// <summary>
        /// Import biometric data from file
        /// </summary>
        Task<int> ImportBiometricDataAsync(IFormFile file, long deviceId);

        /// <summary>
        /// Export logs to file
        /// </summary>
        Task<ExportFileResult> ExportLogsAsync(object exportDto);
    }

    /// <summary>
    /// Export file result
    /// </summary>
    public class ExportFileResult
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
