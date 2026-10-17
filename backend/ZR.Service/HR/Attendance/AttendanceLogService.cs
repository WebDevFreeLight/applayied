using ZR.Model.HR.EmployeeCore;
using System.Linq.Expressions;
using Infrastructure.Attribute;
using Mapster;
using ZR.Model.HR.Attendance.AttendanceCapture;
using ZR.Model.HR.Attendance.AttendanceProcessing;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Dto.Attendance;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// Attendance Log Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IAttendanceLogService), ServiceLifetime = LifeTime.Transient)]
    public class AttendanceLogService : BaseService<HrAttendanceLog>, IAttendanceLogService
    {
        /// <summary>
        /// Get paged list of attendance logs
        /// </summary>
        public async Task<PagedInfo<HrAttendanceLogDto>> GetPageListAsync(Expression<Func<HrAttendanceLog, bool>> predicate, HrAttendanceLogQueryDto pager)
        {
            var query = Queryable()
        .LeftJoin<HrEmployeeCore>((log, emp) => log.EmployeeId == emp.EmployeeId)
        .LeftJoin<HrAttendanceDevice>((log, emp, device) => log.DeviceId == device.DeviceId)
        .Where(predicate)
        .Select((log, emp, device) => new HrAttendanceLog
        {
            LogId = log.LogId,
            EmployeeId = log.EmployeeId,
            LogTime = log.LogTime,
            LogType = log.LogType,
            DeviceId = log.DeviceId,
            VerifyMode = log.VerifyMode,
            IsProcessed = log.IsProcessed,
            ProcessedTime = log.ProcessedTime,
            Create_time = log.Create_time,
            Create_by = log.Create_by,
            Update_time = log.Update_time,
            Update_by = log.Update_by,
            Remark = log.Remark
        })
        .MergeTable() // This converts the query back to a single entity context
        .OrderBy(log => log.LogTime, OrderByType.Desc);

            var pagedResult = await query.ToPageAsync(pager);

            // Map entities to DTOs
            return new PagedInfo<HrAttendanceLogDto>
            {
                PageIndex = pagedResult.PageIndex,
                PageSize = pagedResult.PageSize,
                TotalNum = pagedResult.TotalNum,
                TotalPage = pagedResult.TotalPage,
                Result = pagedResult.Result.Adapt<List<HrAttendanceLogDto>>()
            };
        }

        /// <summary>
        /// Get unprocessed logs for a specific date
        /// </summary>
        public async Task<List<HrAttendanceLogDto>> GetUnprocessedLogsAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);

            var logs = await Queryable()
                .Where(log => log.LogTime >= startDate && log.LogTime < endDate)
                .Where(log => log.IsProcessed == false || log.IsProcessed == null)
                .OrderBy(log => log.EmployeeId)
                .OrderBy(log => log.LogTime)
                .ToListAsync();

            return logs.Adapt<List<HrAttendanceLogDto>>();
        }

        /// <summary>
        /// Mark logs as processed
        /// </summary>
        public async Task<int> MarkLogsAsProcessedAsync(List<long> logIds)
        {
            if (logIds == null || logIds.Count == 0)
                return 0;

            return await Context.Updateable<HrAttendanceLog>()
                .SetColumns(log => new HrAttendanceLog
                {
                    IsProcessed = true,
                    ProcessedTime = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(log => logIds.Contains(log.LogId))
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Get employee logs for a specific date
        /// </summary>
        public async Task<List<HrAttendanceLogDto>> GetEmployeeLogsAsync(long employeeId, DateTime date)
        {
            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);

            var logs = await Queryable()
                .Where(log => log.EmployeeId == employeeId)
                .Where(log => log.LogTime >= startDate && log.LogTime < endDate)
                .OrderBy(log => log.LogTime)
                .ToListAsync();

            return logs.Adapt<List<HrAttendanceLogDto>>();
        }

        /// <summary>
        /// Process logs to create attendance records
        /// </summary>
        public async Task<int> ProcessLogsToAttendanceAsync(DateTime date)
        {
            // Query unprocessed logs directly as entities for internal processing
            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);

            var unprocessedLogs = await Queryable()
                .Where(log => log.LogTime >= startDate && log.LogTime < endDate)
                .Where(log => log.IsProcessed == false || log.IsProcessed == null)
                .OrderBy(log => log.EmployeeId)
                .OrderBy(log => log.LogTime)
                .ToListAsync();

            if (!unprocessedLogs.Any())
                return 0;

            var groupedLogs = unprocessedLogs.GroupBy(log => log.EmployeeId);
            var processedCount = 0;

            foreach (var employeeLogs in groupedLogs)
            {
                var employeeId = employeeLogs.Key;
                var logs = employeeLogs.OrderBy(l => l.LogTime).ToList();

                // Check if attendance record already exists
                var existingAttendance = await Context.Queryable<HrAttendanceDay>()
                    .Where(a => a.EmployeeId == employeeId && a.AttendanceDate == date.Date)
                    .FirstAsync();

                if (existingAttendance == null)
                {
                    existingAttendance = new ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay
                    {
                        EmployeeId = employeeId,
                        AttendanceDate = date.Date,
                        Status = "present",
                        Create_time = DateTime.Now
                    };
                }

                // Find first check-in and last check-out
                var checkInLog = logs.FirstOrDefault(l => l.LogType == "IN" || l.LogType == "check_in");
                var checkOutLog = logs.LastOrDefault(l => l.LogType == "OUT" || l.LogType == "check_out");

                if (checkInLog != null)
                {
                    existingAttendance.CheckInTime = checkInLog.LogTime;
                    existingAttendance.CheckInDeviceId = checkInLog.DeviceId;
                }

                if (checkOutLog != null && checkOutLog.LogTime > (checkInLog?.LogTime ?? DateTime.MinValue))
                {
                    existingAttendance.CheckOutTime = checkOutLog.LogTime;
                    existingAttendance.CheckOutDeviceId = checkOutLog.DeviceId;

                    // Calculate work hours
                    if (existingAttendance.CheckInTime.HasValue)
                    {
                        var workTime = existingAttendance.CheckOutTime.Value - existingAttendance.CheckInTime.Value;
                        existingAttendance.WorkHours = (decimal)workTime.TotalHours;
                    }
                }

                // Save or update attendance record
                if (existingAttendance.AttendanceId == 0)
                {
                    await Context.Insertable(existingAttendance).ExecuteCommandAsync();
                }
                else
                {
                    existingAttendance.Update_time = DateTime.Now;
                    await Context.Updateable(existingAttendance)
                        .IgnoreColumns(a => new { a.Create_by, a.Create_time })
                        .ExecuteCommandAsync();
                }

                // Mark logs as processed
                var logIds = logs.Select(l => l.LogId).ToList();
                await MarkLogsAsProcessedAsync(logIds);

                processedCount++;
            }

            return processedCount;
        }

        /// <summary>
        /// Import logs from device
        /// </summary>
        public async Task<int> ImportLogsFromDeviceAsync(ImportAttendanceLogsDto dto)
        {
            if (dto.Logs == null || !dto.Logs.Any())
                return 0;

            // Map DTOs to entities and set device ID and timestamps
            var entities = dto.Logs.Adapt<List<HrAttendanceLog>>();
            foreach (var log in entities)
            {
                log.DeviceId = dto.DeviceId;
                log.IsProcessed = false;
                log.IsSynchronized = true;
                log.Create_time = DateTime.Now;
            }

            // Bulk insert logs
            return await Context.Insertable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// Get logs by device
        /// </summary>
        public async Task<List<HrAttendanceLogDto>> GetLogsByDeviceAsync(long deviceId, DateTime startDate, DateTime endDate)
        {
            var logs = await Queryable()
                .Where(log => log.DeviceId == deviceId &&
                             log.LogTime >= startDate &&
                             log.LogTime <= endDate)
                .OrderBy(log => log.LogTime)
                .ToListAsync();

            return logs.Adapt<List<HrAttendanceLogDto>>();
        }

        /// <summary>
        /// Clean duplicate logs
        /// </summary>
        public async Task<int> CleanDuplicateLogsAsync(DateTime date)
        {
            // Find duplicate logs (same employee, device, and time within 1 minute)
            var logs = await Queryable()
                .Where(log => log.LogTime.Date == date.Date)
                .OrderBy(log => log.EmployeeId)
                .OrderBy(log => log.LogTime)
                .ToListAsync();

            var duplicateIds = new List<long>();
            HrAttendanceLog previousLog = null;

            foreach (var log in logs)
            {
                if (previousLog != null &&
                    previousLog.EmployeeId == log.EmployeeId &&
                    previousLog.DeviceId == log.DeviceId &&
                    Math.Abs((log.LogTime - previousLog.LogTime).TotalMinutes) < 1)
                {
                    // Mark the newer log as duplicate
                    duplicateIds.Add(log.LogId);
                }
                else
                {
                    previousLog = log;
                }
            }

            if (duplicateIds.Any())
            {
                return await Context.Deleteable<HrAttendanceLog>()
                    .In(duplicateIds)
                    .ExecuteCommandAsync();
            }

            return 0;
        }

        /// <summary>
        /// Get logs summary for date range
        /// </summary>
        public async Task<object> GetLogsSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await Queryable()
                .Where(log => log.LogTime >= startDate && log.LogTime <= endDate)
                .ToListAsync();

            var deviceStats = logs.GroupBy(log => log.DeviceId)
                .Select(g => new
                {
                    DeviceId = g.Key,
                    TotalLogs = g.Count(),
                    ProcessedLogs = g.Count(l => l.IsProcessed),
                    UnprocessedLogs = g.Count(l => !l.IsProcessed)
                });

            return new
            {
                TotalLogs = logs.Count,
                ProcessedLogs = logs.Count(l => l.IsProcessed),
                UnprocessedLogs = logs.Count(l => !l.IsProcessed),
                TotalEmployees = logs.Select(l => l.EmployeeId).Distinct().Count(),
                TotalDevices = logs.Select(l => l.DeviceId).Distinct().Count(),
                DateRange = new { StartDate = startDate, EndDate = endDate },
                DeviceStatistics = deviceStats,
                LogTypes = logs.GroupBy(l => l.LogType)
                    .Select(g => new { Type = g.Key, Count = g.Count() }),
                HourlyDistribution = logs.GroupBy(l => l.LogTime.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderBy(h => h.Hour)
            };
        }

        /// <summary>
        /// Get logs summary for single date
        /// </summary>
        public async Task<object> GetLogsSummaryAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);
            return await GetLogsSummaryAsync(startDate, endDate);
        }

        /// <summary>
        /// Get paged logs with query DTO
        /// </summary>
        public async Task<PagedInfo<HrAttendanceLogDto>> GetPagedLogsAsync(HrAttendanceLogQueryDto query)
        {
            var predicate = Expressionable.Create<HrAttendanceLog>();

            if (query.EmployeeId.HasValue)
                predicate = predicate.And(it => it.EmployeeId == query.EmployeeId.Value);

            if (query.DeviceId.HasValue)
                predicate = predicate.And(it => it.DeviceId == query.DeviceId.Value);

            if (query.BeginLogTime.HasValue)
                predicate = predicate.And(it => it.LogTime >= query.BeginLogTime.Value);

            if (query.EndLogTime.HasValue)
                predicate = predicate.And(it => it.LogTime <= query.EndLogTime.Value);

            if (!string.IsNullOrEmpty(query.LogType))
                predicate = predicate.And(it => it.LogType == query.LogType);

            if (query.IsProcessed.HasValue)
                predicate = predicate.And(it => it.IsProcessed == query.IsProcessed.Value);

            if (query.IsSynchronized.HasValue)
                predicate = predicate.And(it => it.IsSynchronized == query.IsSynchronized.Value);

            if (!string.IsNullOrEmpty(query.Location))
                predicate = predicate.And(it => it.Location.Contains(query.Location));

            return await GetPageListAsync(predicate.ToExpression(), query);
        }

        /// <summary>
        /// Get log by ID
        /// </summary>
        public new async Task<HrAttendanceLog> GetByIdAsync(long logId)
        {
            return await Queryable().Where(it => it.LogId == logId).FirstAsync();
        }

        /// <summary>
        /// Get employee logs for date range
        /// </summary>
        public async Task<List<HrAttendanceLogDto>> GetEmployeeLogsRangeAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            var logs = await Queryable()
                .Where(log => log.EmployeeId == employeeId)
                .Where(log => log.LogTime >= startDate && log.LogTime <= endDate)
                .OrderBy(log => log.LogTime)
                .ToListAsync();

            return logs.Adapt<List<HrAttendanceLogDto>>();
        }

        /// <summary>
        /// Get device logs for a specific date
        /// </summary>
        public async Task<List<HrAttendanceLogDto>> GetDeviceLogsAsync(long deviceId, DateTime date)
        {
            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);
            return await GetLogsByDeviceAsync(deviceId, startDate, endDate);
        }

        /// <summary>
        /// Create manual attendance log
        /// </summary>
        public async Task<int> CreateLogAsync(CreateHrAttendanceLogDto dto)
        {
            var log = dto.Adapt<HrAttendanceLog>();
            log.IsProcessed = dto.IsProcessed;
            log.IsSynchronized = dto.IsSynchronized;
            log.Create_time = DateTime.Now;

            return await Context.Insertable(log).ExecuteReturnIdentityAsync();
        }

        /// <summary>
        /// Delete attendance log
        /// </summary>
        public new async Task<bool> DeleteAsync(long logId)
        {
            var result = await Context.Deleteable<HrAttendanceLog>()
                .Where(it => it.LogId == logId)
                .ExecuteCommandAsync();
            return result > 0;
        }

        /// <summary>
        /// Get duplicate logs
        /// </summary>
        public async Task<List<object>> GetDuplicateLogsAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await Queryable()
                .Where(log => log.LogTime >= startDate && log.LogTime <= endDate)
                .OrderBy(log => log.EmployeeId)
                .OrderBy(log => log.LogTime)
                .ToListAsync();

            var duplicateGroups = new List<object>();
            var tempGroup = new List<HrAttendanceLog>();

            for (int i = 0; i < logs.Count; i++)
            {
                if (i == 0)
                {
                    tempGroup.Add(logs[i]);
                    continue;
                }

                var current = logs[i];
                var previous = logs[i - 1];

                // Check if logs are duplicates (same employee, device, within 1 minute)
                if (previous.EmployeeId == current.EmployeeId &&
                    previous.DeviceId == current.DeviceId &&
                    Math.Abs((current.LogTime - previous.LogTime).TotalMinutes) < 1)
                {
                    if (tempGroup.Count == 1)
                    {
                        tempGroup.Add(current);
                    }
                    else
                    {
                        tempGroup.Add(current);
                    }
                }
                else
                {
                    if (tempGroup.Count > 1)
                    {
                        duplicateGroups.Add(new
                        {
                            EmployeeId = tempGroup[0].EmployeeId,
                            DeviceId = tempGroup[0].DeviceId,
                            LogTime = tempGroup[0].LogTime,
                            Count = tempGroup.Count,
                            Logs = tempGroup.Adapt<List<HrAttendanceLogDto>>()
                        });
                    }
                    tempGroup = new List<HrAttendanceLog> { current };
                }
            }

            // Add last group if it's a duplicate
            if (tempGroup.Count > 1)
            {
                duplicateGroups.Add(new
                {
                    EmployeeId = tempGroup[0].EmployeeId,
                    DeviceId = tempGroup[0].DeviceId,
                    LogTime = tempGroup[0].LogTime,
                    Count = tempGroup.Count,
                    Logs = tempGroup.Adapt<List<HrAttendanceLogDto>>()
                });
            }

            return duplicateGroups;
        }

        /// <summary>
        /// Merge duplicate logs
        /// </summary>
        public async Task<bool> MergeDuplicateLogsAsync(List<long> logIds)
        {
            if (logIds == null || logIds.Count < 2)
                return false;

            // Keep the first log, delete the rest
            var logsToDelete = logIds.Skip(1).ToList();

            var result = await Context.Deleteable<HrAttendanceLog>()
                .In(logsToDelete)
                .ExecuteCommandAsync();

            return result > 0;
        }

        /// <summary>
        /// Import biometric data from file
        /// </summary>
        public async Task<int> ImportBiometricDataAsync(Microsoft.AspNetCore.Http.IFormFile file, long deviceId)
        {
            if (file == null || file.Length == 0)
                return 0;

            var logs = new List<HrAttendanceLog>();

            using (var stream = file.OpenReadStream())
            using (var reader = new System.IO.StreamReader(stream))
            {
                // Skip header if exists
                var firstLine = await reader.ReadLineAsync();
                if (firstLine == null)
                    return 0;

                // Check if first line is header
                bool hasHeader = firstLine.Contains("Employee") || firstLine.Contains("Time") || firstLine.Contains("Date");

                if (!hasHeader)
                {
                    // Process first line as data
                    var log = ParseBiometricLine(firstLine, deviceId);
                    if (log != null)
                        logs.Add(log);
                }

                // Process remaining lines
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var log = ParseBiometricLine(line, deviceId);
                    if (log != null)
                        logs.Add(log);
                }
            }

            if (logs.Any())
            {
                foreach (var log in logs)
                {
                    log.IsProcessed = false;
                    log.IsSynchronized = true;
                    log.Create_time = DateTime.Now;
                }

                return await Context.Insertable(logs).ExecuteCommandAsync();
            }

            return 0;
        }

        /// <summary>
        /// Parse biometric data line (CSV format: EmployeeId,DateTime,LogType)
        /// </summary>
        private HrAttendanceLog ParseBiometricLine(string line, long deviceId)
        {
            try
            {
                var parts = line.Split(',');
                if (parts.Length < 2)
                    return null;

                var employeeIdStr = parts[0].Trim();
                var dateTimeStr = parts[1].Trim();
                var logType = parts.Length > 2 ? parts[2].Trim() : "IN";

                if (!long.TryParse(employeeIdStr, out long employeeId))
                    return null;

                if (!DateTime.TryParse(dateTimeStr, out DateTime logTime))
                    return null;

                return new HrAttendanceLog
                {
                    EmployeeId = employeeId,
                    LogTime = logTime,
                    LogType = logType,
                    DeviceId = deviceId
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Export logs to file
        /// </summary>
        public async Task<ZR.Service.HR.IService.ExportFileResult> ExportLogsAsync(object exportDto)
        {
            // Cast to dynamic to access properties
            dynamic dto = exportDto;

            DateTime startDate = dto.StartDate;
            DateTime endDate = dto.EndDate;
            long? employeeId = dto.EmployeeId;
            long? deviceId = dto.DeviceId;
            string format = dto.Format ?? "xlsx";

            var query = Queryable()
                .Where(log => log.LogTime >= startDate && log.LogTime <= endDate);

            if (employeeId.HasValue)
                query = query.Where(log => log.EmployeeId == employeeId.Value);

            if (deviceId.HasValue)
                query = query.Where(log => log.DeviceId == deviceId.Value);

            var logs = await query
                .OrderBy(log => log.LogTime)
                .ToListAsync();

            var logDtos = logs.Adapt<List<HrAttendanceLogDto>>();

            // Generate file based on format
            byte[] fileData;
            string fileName;
            string contentType;

            if (format.ToLower() == "csv")
            {
                fileData = GenerateCsvFile(logDtos);
                fileName = $"AttendanceLogs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                contentType = "text/csv";
            }
            else
            {
                // Default to Excel format
                fileData = GenerateExcelFile(logDtos);
                fileName = $"AttendanceLogs_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return new ZR.Service.HR.IService.ExportFileResult
            {
                Data = fileData,
                FileName = fileName,
                ContentType = contentType
            };
        }

        /// <summary>
        /// Generate CSV file from logs
        /// </summary>
        private byte[] GenerateCsvFile(List<HrAttendanceLogDto> logs)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("LogId,EmployeeId,LogTime,LogType,DeviceId,IsProcessed,ProcessedTime");

            foreach (var log in logs)
            {
                sb.AppendLine($"{log.LogId},{log.EmployeeId},{log.LogTime:yyyy-MM-dd HH:mm:ss},{log.LogType},{log.DeviceId},{log.IsProcessed},{log.ProcessedTime?.ToString("yyyy-MM-dd HH:mm:ss")}");
            }

            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Generate Excel file from logs (simple CSV for now - can be enhanced with actual Excel library)
        /// </summary>
        private byte[] GenerateExcelFile(List<HrAttendanceLogDto> logs)
        {
            // For now, return CSV format
            // TODO: Implement actual Excel generation using EPPlus or similar library
            return GenerateCsvFile(logs);
        }
    }
}
