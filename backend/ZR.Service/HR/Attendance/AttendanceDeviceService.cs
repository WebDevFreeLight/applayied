using Infrastructure.Attribute;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.AttendanceCapture;
using ZR.Model.HR.Dto.Attendance;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.Attendance
{
    /// <summary>
    /// Attendance Device Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IAttendanceDeviceService), ServiceLifetime = LifeTime.Transient)]
    public class AttendanceDeviceService : BaseService<HrAttendanceDevice>, IAttendanceDeviceService
    {
        /// <summary>
        /// Get list of active attendance devices
        /// </summary>
        public async Task<List<HrAttendanceDevice>> GetActiveDevicesAsync()
        {
            return await Queryable()
                .Where(device => device.IsActive == true)
                .Where(device => device.IsDeleted == false || device.IsDeleted == null)
                .OrderBy(device => device.DeviceName)
                .ToListAsync();
        }

        /// <summary>
        /// Sync device data
        /// </summary>
        public async Task<int> SyncDeviceDataAsync(long deviceId)
        {
            var device = await Queryable()
                .Where(d => d.DeviceId == deviceId)
                .FirstAsync();

            if (device == null)
                throw new Exception($"Device with ID {deviceId} not found");

            if (!device.IsActive.GetValueOrDefault())
                throw new Exception($"Device {device.DeviceName} is not active");

            try
            {
                // Here you would implement the actual device synchronization logic
                // This typically involves:
                // 1. Connecting to the device using its IP and port
                // 2. Authenticating with device credentials
                // 3. Fetching attendance logs since last sync time
                // 4. Storing logs in HrAttendanceLog table
                // 5. Updating device's last sync time

                // Simulated sync process
                var syncedRecords = await SimulateDeviceSync(device);

                // Update device last sync time
                await Context.Updateable<HrAttendanceDevice>()
                    .SetColumns(d => new HrAttendanceDevice
                    {
                        LastSyncTime = DateTime.Now,
                        LastSyncStatus = "Success",
                        LastSyncRecords = syncedRecords,
                        Update_time = DateTime.Now
                    })
                    .Where(d => d.DeviceId == deviceId)
                    .ExecuteCommandAsync();

                return syncedRecords;
            }
            catch (Exception ex)
            {
                // Update device sync status with error
                await Context.Updateable<HrAttendanceDevice>()
                    .SetColumns(d => new HrAttendanceDevice
                    {
                        LastSyncTime = DateTime.Now,
                        LastSyncStatus = $"Failed: {ex.Message}",
                        LastSyncRecords = 0,
                        Update_time = DateTime.Now
                    })
                    .Where(d => d.DeviceId == deviceId)
                    .ExecuteCommandAsync();

                throw;
            }
        }

        /// <summary>
        /// Test device connection
        /// </summary>
        public async Task<bool> TestDeviceConnectionAsync(long deviceId)
        {
            var device = await Queryable()
                .Where(d => d.DeviceId == deviceId)
                .FirstAsync();

            if (device == null)
                throw new Exception($"Device with ID {deviceId} not found");

            try
            {
                // Here you would implement actual device connection testing
                // This typically involves:
                // 1. Attempting to establish a connection to the device
                // 2. Sending a ping or test command
                // 3. Waiting for response within timeout period
                // 4. Validating the response

                // Simulated connection test
                var isConnected = await SimulateConnectionTest(device);

                // Update device online status
                await UpdateDeviceStatusAsync(deviceId, isConnected);

                return isConnected;
            }
            catch (Exception)
            {
                await UpdateDeviceStatusAsync(deviceId, false);
                return false;
            }
        }

        /// <summary>
        /// Update device online status
        /// </summary>
        public async Task<int> UpdateDeviceStatusAsync(long deviceId, bool isOnline)
        {
            return await Context.Updateable<HrAttendanceDevice>()
                .SetColumns(device => new HrAttendanceDevice
                {
                    IsOnline = isOnline,
                    LastPingTime = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(device => device.DeviceId == deviceId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Simulate device synchronization (replace with actual implementation)
        /// </summary>
        private async Task<int> SimulateDeviceSync(HrAttendanceDevice device)
        {
            // This is a simulation. In production, you would:
            // 1. Connect to the actual biometric device
            // 2. Fetch attendance logs
            // 3. Process and store them

            var random = new Random();
            var recordCount = random.Next(10, 100);

            // Simulate fetching and storing logs
            var logs = new List<HrAttendanceLog>();
            var lastSyncTime = device.LastSyncTime ?? DateTime.Now.AddDays(-1);

            for (int i = 0; i < recordCount; i++)
            {
                logs.Add(new HrAttendanceLog
                {
                    EmployeeId = random.Next(1, 100),
                    DeviceId = device.DeviceId,
                    LogTime = lastSyncTime.AddMinutes(random.Next(1, 1440)),
                    LogType = random.Next(0, 2) == 0 ? "IN" : "OUT",
                    VerifyMode = "Fingerprint",
                    IsProcessed = false,
                    Create_time = DateTime.Now
                });
            }

            if (logs.Any())
            {
                await Context.Insertable(logs).ExecuteCommandAsync();
            }

            return await Task.FromResult(recordCount);
        }

        /// <summary>
        /// Simulate connection test (replace with actual implementation)
        /// </summary>
        private async Task<bool> SimulateConnectionTest(HrAttendanceDevice device)
        {
            // This is a simulation. In production, you would:
            // 1. Actually attempt to connect to the device
            // 2. Send a test command
            // 3. Wait for response

            // Simulate network delay
            await Task.Delay(100);

            // Simulate 90% success rate for testing
            var random = new Random();
            return random.Next(1, 11) <= 9;
        }

        /// <summary>
        /// Get device status for all devices
        /// </summary>
        public async Task<object> GetDeviceStatusAsync()
        {
            var devices = await Queryable()
                .Where(d => d.IsActive == true)
                .Where(d => d.IsDeleted == false || d.IsDeleted == null)
                .ToListAsync();

            var onlineCount = devices.Count(d => d.IsOnline == true);
            var offlineCount = devices.Count(d => d.IsOnline != true);
            var totalDevices = devices.Count;

            return new
            {
                TotalDevices = totalDevices,
                OnlineCount = onlineCount,
                OfflineCount = offlineCount,
                OnlinePercentage = totalDevices > 0 ? (decimal)onlineCount / totalDevices * 100 : 0,
                Devices = devices.Select(d => new
                {
                    d.DeviceId,
                    d.DeviceName,
                    d.DeviceCode,
                    d.Location,
                    d.IsOnline,
                    d.LastSyncTime,
                    d.LastSyncStatus,
                    d.LastSyncRecords,
                    LastPingTime = d.LastPingTime ?? d.Update_time
                }).OrderBy(d => d.IsOnline).ThenBy(d => d.DeviceName)
            };
        }

        /// <summary>
        /// Sync all active devices
        /// </summary>
        public async Task<object> SyncAllDevicesAsync()
        {
            var activeDevices = await GetActiveDevicesAsync();

            if (!activeDevices.Any())
            {
                return new
                {
                    Success = false,
                    Message = "No active devices found",
                    SyncedDevices = 0,
                    TotalRecords = 0
                };
            }

            var syncResults = new List<object>();
            var totalRecords = 0;
            var successCount = 0;
            var failureCount = 0;

            foreach (var device in activeDevices)
            {
                try
                {
                    var recordsCount = await SyncDeviceDataAsync(device.DeviceId);
                    totalRecords += recordsCount;
                    successCount++;

                    syncResults.Add(new
                    {
                        DeviceId = device.DeviceId,
                        DeviceName = device.DeviceName,
                        Success = true,
                        RecordsSynced = recordsCount,
                        Message = "Sync successful"
                    });
                }
                catch (Exception ex)
                {
                    failureCount++;
                    syncResults.Add(new
                    {
                        DeviceId = device.DeviceId,
                        DeviceName = device.DeviceName,
                        Success = false,
                        RecordsSynced = 0,
                        Message = ex.Message
                    });
                }
            }

            return new
            {
                Success = true,
                Message = $"Sync completed for {activeDevices.Count} devices",
                SyncedDevices = successCount,
                FailedDevices = failureCount,
                TotalRecords = totalRecords,
                Details = syncResults
            };
        }

        /// <summary>
        /// Get device by ID
        /// </summary>
        public async Task<HrAttendanceDevice> GetByIdAsync(long deviceId)
        {
            return await Queryable().Where(d => d.DeviceId == deviceId).FirstAsync();
        }

        /// <summary>
        /// Create new device
        /// </summary>
        public async Task<long> CreateDeviceAsync(CreateHrAttendanceDeviceDto dto)
        {
            var device = dto.Adapt<HrAttendanceDevice>();
            device.Create_time = DateTime.Now;
            // CreatedBy should be set from controller
            device.IsActive = true;
            device.IsDeleted = false;

            return await InsertAsync(device);
        }

        /// <summary>
        /// Update existing device
        /// </summary>
        public async Task<bool> UpdateDeviceAsync(long deviceId, UpdateHrAttendanceDeviceDto dto)
        {
            var device = await GetByIdAsync(deviceId);
            if (device == null)
                return false;

            dto.Adapt(device);
            device.Update_time = DateTime.Now;
            // ModifiedBy should be set from controller

            var result = await UpdateAsync(device);
            return result;
        }

        /// <summary>
        /// Delete device
        /// </summary>
        public async Task<bool> DeleteDeviceAsync(long deviceId)
        {
            var device = await GetByIdAsync(deviceId);
            if (device == null)
                return false;

            var result = await DeleteAsync(device);
            return result;
        }

        /// <summary>
        /// Get device logs
        /// </summary>
        public async Task<List<object>> GetDeviceLogsAsync(long deviceId, DateTime startDate, DateTime endDate)
        {
            var logs = await Context.Queryable<HrAttendanceLog>()
                .Where(l => l.DeviceId == deviceId)
                .Where(l => l.LogTime >= startDate && l.LogTime <= endDate)
                .OrderBy(l => l.LogTime)
                .Select(l => new
                {
                    l.LogId,
                    l.EmployeeId,
                    l.LogTime,
                    l.LogType,
                    l.VerifyMode,
                    l.IsProcessed
                })
                .ToListAsync();

            return logs.Cast<object>().ToList();
        }

        /// <summary>
        /// Enable device
        /// </summary>
        public async Task<bool> EnableDeviceAsync(long deviceId)
        {
            var result = await Context.Updateable<HrAttendanceDevice>()
                .SetColumns(d => new HrAttendanceDevice { IsActive = true, Update_time = DateTime.Now })
                .Where(d => d.DeviceId == deviceId)
                .ExecuteCommandAsync();

            return result > 0;
        }

        /// <summary>
        /// Disable device
        /// </summary>
        public async Task<bool> DisableDeviceAsync(long deviceId)
        {
            var result = await Context.Updateable<HrAttendanceDevice>()
                .SetColumns(d => new HrAttendanceDevice { IsActive = false, Update_time = DateTime.Now })
                .Where(d => d.DeviceId == deviceId)
                .ExecuteCommandAsync();

            return result > 0;
        }
    }
}
