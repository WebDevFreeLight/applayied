using ZR.Model.HR.EmployeeCore;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance;
using ZR.Model.HR.Attendance.AttendanceProcessing;
using ZR.Model.HR.Attendance.Configuration;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto;
using ZR.Repository;
using ZR.Service.HR.IHrService;
using ZR.Service.HR.IService;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR
{

    /// <summary>
    /// Options for leave calculation
    /// </summary>
    public class LeaveCalculationOptions
    {
        public bool ExcludeHolidays { get; set; } = true;
        public bool ExcludeWeekends { get; set; } = true;
        public DayOfWeek[] CustomWeekendDays { get; set; }
        public Func<DateTime, bool> CustomExclusionRule { get; set; }
        public bool IncludePartialDays { get; set; } = false;
    }






    /// <summary>
    /// HR Leave Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IHrLeaveService), ServiceLifetime = LifeTime.Transient)]
    public class HrLeaveService : BaseService<HrLeaveRequest>, IHrLeaveService
    {

        private readonly ILeaveBalanceService _balanceService;
        private readonly IEmployeeDataService _employeeDataService;

        public HrLeaveService(ILeaveBalanceService leaveBalanceService, IEmployeeDataService employeeDataService)
        {
            _balanceService = leaveBalanceService;
            _employeeDataService = employeeDataService;
        }



        public async Task<PagedInfo<HrLeaveRequest>> GetPa3geListAsync(Expression<Func<HrLeaveRequest, bool>> predicate, LeaveQueryDto pager)
        {
            var query = Queryable()
                .LeftJoin<HrEmployeeCore>((r, e) => r.EmployeeId == e.EmployeeId)
                .LeftJoin<HrLeaveType>((r, e, t) => r.LeaveTypeId == t.LeaveTypeId)
                .LeftJoin<HrEmployeeCore>((r, e, t, a) => r.ApproverId == a.EmployeeId)
                .Where((r, e, t, a) => predicate.Compile()(r)) // ??Compile?????????
                .Select((r, e, t, a) => new HrLeaveRequest
                {
                    RequestId = r.RequestId,
                    EmployeeId = r.EmployeeId,
                    LeaveTypeId = r.LeaveTypeId,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Days = r.Days,
                    Reason = r.Reason,
                    Status = r.Status,
                    ApproverId = r.ApproverId,
                    ApproveDate = r.ApproveDate,
                    ApproveComments = r.ApproveComments,
                    AttachmentUrl = r.AttachmentUrl,
                    IsEmergency = r.IsEmergency,
                    Create_time = r.Create_time
                })
                .MergeTable() // ??:??????????
                .OrderBy(r => r.Create_time, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }



        public async Task<PagedInfo<HrLeaveRequest>> GetPageListAsync(Expression<Func<HrLeaveRequest, bool>> predicate, LeaveQueryDto pager)
        {
            var query = Queryable()
                //.LeftJoin<HrEmployeeCore>((r, e) => r.EmployeeId == e.EmployeeId)
                //.LeftJoin<HrLeaveType>((r, e, t) => r.LeaveTypeId == t.LeaveTypeId)
                //.LeftJoin<HrEmployeeCore>((r, e, t, a) => r.ApproverId == a.EmployeeId)
                .Where(predicate)
                .Select((r ) => new HrLeaveRequest
                {
                    RequestId = r.RequestId,
                    EmployeeId = r.EmployeeId,
                    LeaveTypeId = r.LeaveTypeId,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Days = r.Days,
                    Reason = r.Reason,
                    Status = r.Status,
                    ApproverId = r.ApproverId,
                    ApproveDate = r.ApproveDate,
                    ApproveComments = r.ApproveComments,
                    AttachmentUrl = r.AttachmentUrl,
                    IsEmergency = r.IsEmergency,
                    Create_time = r.Create_time
                })
                        //.MergeTable() // This converts the joined query back to a single table context

                .OrderBy(r => r.Create_time, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }

        public new async Task<List<HrLeaveRequest>> GetListAsync(Expression<Func<HrLeaveRequest, bool>> predicate)
        {
            return await Queryable()
                .Where(predicate)
                .OrderBy(r => r.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<HrLeaveRequest> GetByIdAsync(long requestId)
        {
            HrLeaveRequest hrLeaveRequest = await GetFirstAsync(r => r.RequestId == requestId);
            return hrLeaveRequest;
        }

        public async Task<long> AddAsync(HrLeaveRequest request)
        {
            // Validate leave balance
            var balance = await _balanceService.GetBalanceAsync(request.EmployeeId, request.LeaveTypeId);
            if (balance == null || balance.AvailableDays < request.Days)
            {
                throw new AppException(ExceptionCode.HR_LEAVE_INSUFFICIENT_BALANCE, "Insufficient leave balance");
            }

            // Check for conflicts
            var hasConflict = await CheckLeaveConflictAsync(request.EmployeeId, request.StartDate, request.EndDate);
            if (hasConflict)
            {
                throw new AppException(ExceptionCode.HR_LEAVE_CONFLICT, "Leave request conflicts with existing leave");
            }

            // Calculate actual leave days (excluding holidays and weekends)
            request.Days = await CalculateLeaveDaysAsync(request.StartDate, request.EndDate);

            // Set default status
            request.Status = "pending";

            // Check if emergency leave
            if (request.IsEmergency)
            {
                // Auto-approve emergency leaves if within policy
                var leaveType = await Context.Queryable<HrLeaveType>()
                    .FirstAsync(t => t.LeaveTypeId == request.LeaveTypeId);

                if (leaveType != null && !leaveType.RequiresApproval)
                {
                    request.Status = "approved";
                    request.ApproveDate = DateTime.Now;
                    request.ApproveComments = "Auto-approved (Emergency)";

                    // Deduct balance immediately
                    await _balanceService.DeductBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.Days ?? 0);
                }
            }

            long v = await InsertAsync(request);
            return v;
        }

        public new async Task<int> UpdateAsync(HrLeaveRequest request)
        {
            var original = await GetByIdAsync(request.RequestId);
            if (original == null)
            {
                throw new AppException(ExceptionCode.HR_LEAVE_NOT_FOUND, "Leave request not found");
            }

            // Only allow updates for pending requests
            if (original.Status != "pending")
            {
                throw new AppException(ExceptionCode.HR_LEAVE_INVALID_STATUS, "Cannot update approved or rejected requests");
            }

            // Recalculate days if dates changed
            if (request.StartDate != original.StartDate || request.EndDate != original.EndDate)
            {
                request.Days = await CalculateLeaveDaysAsync(request.StartDate, request.EndDate);

                // Revalidate balance
                var balance = await _balanceService.GetBalanceAsync(request.EmployeeId, request.LeaveTypeId);
                if (balance == null || balance.AvailableDays < request.Days)
                {
                    throw new AppException(ExceptionCode.HR_LEAVE_INSUFFICIENT_BALANCE, "Insufficient leave balance for updated dates");
                }

                // Check for conflicts
                var hasConflict = await CheckLeaveConflictAsync(request.EmployeeId, request.StartDate, request.EndDate, request.RequestId);
                if (hasConflict)
                {
                    throw new AppException(ExceptionCode.HR_LEAVE_CONFLICT, "Updated dates conflict with existing leave");
                }
            }

            return await Context.Updateable(request)
                .IgnoreColumns(r => new { r.Create_by, r.Create_time, r.Status, r.ApproverId, r.ApproveDate, r.ApproveComments })
                .ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] requestIds)
        {
            // Only allow deletion of pending requests
            var requests = await Context.Queryable<HrLeaveRequest>()
                .Where(r => requestIds.Contains(r.RequestId))
                .ToListAsync();

            var nonPending = requests.Where(r => r.Status != "pending").ToList();
            if (nonPending.Any())
            {
                throw new AppException(ExceptionCode.HR_LEAVE_INVALID_STATUS, "Cannot delete approved or rejected requests");
            }

            return await Context.Deleteable<HrLeaveRequest>()
                .In(requestIds)
                .ExecuteCommandAsync();
        }

        public async Task<bool> CancelRequestAsync(long requestId, string cancelledBy)
        {
            var request = await GetByIdAsync(requestId);
            if (request == null)
            {
                return false;
            }

            // Only pending or approved future leaves can be cancelled
            if (request.Status == "rejected" || request.Status == "cancelled")
            {
                return false;
            }

            if (request.Status == "approved" && request.StartDate <= DateTime.Today)
            {
                throw new AppException(ExceptionCode.HR_LEAVE_INVALID_STATUS, "Cannot cancel leave that has already started");
            }

            // If approved, return the balance
            if (request.Status == "approved")
            {
                await _balanceService.AddBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.Days ?? 0);
            }

            request.Status = "cancelled";
            request.Update_by = cancelledBy;
            request.Update_time = DateTime.Now;

            var result = await Context.Updateable(request)
                .UpdateColumns(r => new { r.Status, r.Update_by, r.Update_time })
                .ExecuteCommandAsync();

            return result > 0;
        }

        public async Task<bool> ApproveRequestAsync(long requestId, bool approved, string comments, string approvedBy)
        {
            var request = await GetByIdAsync(requestId);
            if (request == null)
            {
                return false;
            }

            if (request.Status != "Pending")
            {
                throw new AppException(ExceptionCode.HR_LEAVE_INVALID_STATUS, "Request has already been processed");
            }

            request.Status = approved ? "approved" : "rejected";
            request.ApproveDate = DateTime.Now;
            request.ApproveComments = comments;
            request.Update_by = approvedBy;
            request.Update_time = DateTime.Now;

            // Find approver ID - search by name or UserId
            var approverUserId = approvedBy.ParseToLong();
            var allEmployees = await _employeeDataService.GetAllActiveEmployeesAsync();
            var approver = allEmployees.FirstOrDefault(e =>
                (e.GivenName == approvedBy || e.FamilyName == approvedBy) ||
                (e.UserId.HasValue && e.UserId.Value == approverUserId));

            if (approver != null)
            {
                request.ApproverId = approver.EmployeeId;
            }
            else
            {
                request.ApproverId = App.UserId;
            }




            // If approved, deduct balance
            if (approved)
            {
                var deducted = await _balanceService.DeductBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.Days ?? 0);
                if (!deducted)
                {
                    throw new AppException(ExceptionCode.HR_LEAVE_BALANCE_DEDUCTION_FAILED, "Failed to deduct leave balance");
                }

                // Update attendance records for leave days
                await CreateLeaveAttendanceRecords(request);
            }

            var result = await Context.Updateable(request)
                .UpdateColumns(r => new { r.Status, r.ApproverId, r.ApproveDate, r.ApproveComments, r.Update_by, r.Update_time })
                .ExecuteCommandAsync();

            // Send notification to employee
            await SendLeaveNotification(request, approved);

            return result > 0;
        }

        public async Task<List<HrLeaveRequest>> GetPendingApprovalsAsync(long managerId)
        {
            // Get employees reporting to this manager using EmployeeDataService
            var teamMembers = await _employeeDataService.GetTeamMembersAsync(managerId);
            var subordinateIds = teamMembers.Select(e => e.EmployeeId).ToList();

            if (!subordinateIds.Any())
            {
                return new List<HrLeaveRequest>();
            }

            return await Queryable()
                .Where(r => subordinateIds.Contains(r.EmployeeId) && r.Status == "pending")
                .OrderBy(r => r.Create_time)
                .ToListAsync();
        }

        public async Task<List<HrLeaveRequest>> GetEmployeeRequestsAsync(long employeeId, int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            return await Queryable()
                .Where(r => r.EmployeeId == employeeId &&
                           r.StartDate >= startDate &&
                           r.StartDate <= endDate)
                .OrderBy(r => r.StartDate, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<object> GetStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = Queryable();

            if (startDate.HasValue)
                query = query.Where(r => r.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.EndDate <= endDate.Value);

            var requests = await query.ToListAsync();

            return new
            {
                TotalRequests = requests.Count,
                Pending = requests.Count(r => r.Status == "pending"),
                Approved = requests.Count(r => r.Status == "approved"),
                Rejected = requests.Count(r => r.Status == "rejected"),
                Cancelled = requests.Count(r => r.Status == "cancelled"),
                TotalDays = requests.Where(r => r.Status == "approved").Sum(r => r.Days),
                EmergencyLeaves = requests.Count(r => r.IsEmergency),
                ByLeaveType = requests.GroupBy(r => r.LeaveTypeId)
                    .Select(g => new { LeaveTypeId = g.Key, Count = g.Count(), Days = g.Sum(r => r.Days) })
                    .ToList(),
                ByMonth = requests.GroupBy(r => new { r.StartDate.Year, r.StartDate.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToList()
            };
        }





        /// <summary>
        /// Check if date is holiday (batch version for multiple dates)
        /// </summary>
        public async Task<DateTime> GetHolidaysAsync(DateTime startDate, DateTime endDate)
        {
            return await Context.Queryable<HrHoliday>()
                .Where(h => h.Holidaydate.Date >= startDate.Date && h.Holidaydate.Date <= endDate.Date)
                .Select(h => h.Holidaydate.Date)
                .FirstAsync();
        }

        /// <summary>
        /// Check if specific date is holiday
        /// </summary>
        public async Task<bool> IsHolidayAsync(DateTime date)
        {
            return await Context.Queryable<HrHoliday>()
                .Where(h => h.Holidaydate.Date == date.Date)
                .AnyAsync();
        }


        /// <summary>
        /// Calculate leave days for a single day
        /// </summary>
        private int CalculateSingleDay(DateTime date, LeaveCalculationOptions options)
        {
            if (options.ExcludeWeekends && IsWeekend(date.DayOfWeek, options))
                return 0;

            if (options.ExcludeHolidays && IsHolidayAsync(date).GetAwaiter().GetResult())
                return 0;

            return 1;
        }

        /// <summary>
        /// Check if a day is valid for leave
        /// </summary>
        private bool IsValidLeaveDay(DateTime date, HashSet<DayOfWeek> excludedDays, DateTime holidays, LeaveCalculationOptions options)
        {
            // Check weekends
            if (options.ExcludeWeekends && excludedDays.Contains(date.DayOfWeek))
                return false;

            // Check holidays
            if (options.ExcludeHolidays && holidays.Date==date.Date.Date)
                return false;

            // Check custom exclusion rules
            if (options.CustomExclusionRule != null && !options.CustomExclusionRule(date))
                return false;

            return true;
        }

        /// <summary>
        /// Get excluded days of week based on options
        /// </summary>
        private HashSet<DayOfWeek> GetExcludedDaysOfWeek(LeaveCalculationOptions options)
        {
            var excludedDays = new HashSet<DayOfWeek>();

            if (options.ExcludeWeekends)
            {
                // Default: Friday and Saturday for Saudi Arabia
                excludedDays.Add(DayOfWeek.Friday);
                excludedDays.Add(DayOfWeek.Saturday);

                // Add custom weekend days if specified
                if (options.CustomWeekendDays != null)
                {
                    foreach (var day in options.CustomWeekendDays)
                    {
                        excludedDays.Add(day);
                    }
                }
            }

            return excludedDays;
        }

        /// <summary>
        /// Check if day is weekend based on options
        /// </summary>
        private bool IsWeekend(DayOfWeek dayOfWeek, LeaveCalculationOptions options)
        {
            if (options.CustomWeekendDays != null && options.CustomWeekendDays.Length > 0)
                return options.CustomWeekendDays.Contains(dayOfWeek);

            // Default Saudi weekend
            return dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday;
        }



        ///// <summary>
        ///// Check if date is holiday
        ///// </summary>
        //public async Task<bool> IsHolidayAsync(DateTime date)
        //{
        //    return await Context.Queryable<HrHoliday>()
        //        .Where(h => h.HolidayDate.Date == date.Date)
        //        .AnyAsync();
        //}




        /// <summary>
        /// Calculate leave days with advanced options
        /// </summary>
        public async Task<int> CalculateLeaveDaysWithOptionsAsync(DateTime startDate, DateTime endDate, LeaveCalculationOptions options = null)
        {
            options ??= new LeaveCalculationOptions();

            if (endDate < startDate)
                throw new ArgumentException("End date must be after start date");

            if (startDate.Date == endDate.Date)
                return CalculateSingleDay(startDate, options);

            // Get all holidays in the date range in one query
            DateTime holidays = options.ExcludeHolidays
                ? await GetHolidaysAsync(startDate, endDate)
                : DateTime.Now;

            int totalDays = (int)(endDate - startDate).TotalDays + 1;
            int leaveDays = 0;

            // Pre-calculate day of week patterns
            var excludedDays = GetExcludedDaysOfWeek(options);

            for (int i = 0; i < totalDays; i++)
            {
                DateTime currentDate = startDate.AddDays(i);

                if (IsValidLeaveDay(currentDate, excludedDays, holidays, options))
                {
                    leaveDays++;
                }
            }

            return leaveDays;
        }



     



        public async Task<int> CalculateLeaveDaysAsync(DateTime startDate, DateTime endDate, bool excludeHolidays = true)
        {
            if (endDate < startDate)
            {
                throw new ArgumentException("End date must be after start date");
            }

            var days = 0;
            var current = startDate;

            while (current <= endDate)
            {
                // Skip weekends (Friday and Saturday for Saudi Arabia)
                if (current.DayOfWeek != DayOfWeek.Friday && current.DayOfWeek != DayOfWeek.Saturday)
                {
                    // Check if it's a holiday
                    if (excludeHolidays)
                    {
                        var isHoliday = await IsHolidayAsync(current);
                        if (!isHoliday)
                        {
                            days++;
                        }
                    }
                    else
                    {
                        days++;
                    }
                }
                current = current.AddDays(1);
            }

            return days;
        }

        public async Task<bool> CheckLeaveConflictAsync(long employeeId, DateTime startDate, DateTime endDate, long? excludeRequestId = null)
        {
            var query = Queryable()
                .Where(r => r.EmployeeId == employeeId &&
                           r.Status != "rejected" &&
                           r.Status != "cancelled" &&
                           ((r.StartDate <= startDate && r.EndDate >= startDate) ||
                            (r.StartDate <= endDate && r.EndDate >= endDate) ||
                            (r.StartDate >= startDate && r.EndDate <= endDate)));

            if (excludeRequestId.HasValue)
            {
                query = query.Where(r => r.RequestId != excludeRequestId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<object> GetLeaveCalendarAsync(long? departmentId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Step 1: Get all approved leaves for the date range
            var leavesQuery = Queryable()
                .Where(r => r.Status == "approved" &&
                           ((r.StartDate >= startDate && r.StartDate <= endDate) ||
                            (r.EndDate >= startDate && r.EndDate <= endDate) ||
                            (r.StartDate <= startDate && r.EndDate >= endDate)));

            var leaveRequests = await leavesQuery.ToListAsync();

            // Step 2: Batch load employee data
            var employeeIds = leaveRequests.Select(r => r.EmployeeId).Distinct().ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            // Step 3: Filter by department if needed
            if (departmentId.HasValue)
            {
                leaveRequests = leaveRequests.Where(r =>
                    employeeDict.ContainsKey(r.EmployeeId) &&
                    employeeDict[r.EmployeeId].DepartmentId == departmentId.Value
                ).ToList();
            }

            // Step 4: Create enriched leave data
            var leaves = leaveRequests.Select(r => new
            {
                r.RequestId,
                r.EmployeeId,
                EmployeeName = employeeDict.ContainsKey(r.EmployeeId) ? employeeDict[r.EmployeeId].FullName : "Unknown",
                DepartmentId = employeeDict.ContainsKey(r.EmployeeId) ? employeeDict[r.EmployeeId].DepartmentId : null,
                r.LeaveTypeId,
                r.StartDate,
                r.EndDate,
                r.Days
            }).ToList();

            // Get holidays for the month
            var holidays = await Context.Queryable<HrHoliday>()
                .Where(h => h.Holidaydate >= startDate && h.Holidaydate <= endDate)
                .ToListAsync();

            // Build calendar view
            var calendar = new List<object>();
            var current = startDate;

            while (current <= endDate)
            {
                var dayLeaves = leaves.Where(l => l.StartDate <= current && l.EndDate >= current).ToList();
                var holiday = holidays.FirstOrDefault(h => h.Holidaydate == current);

                calendar.Add(new
                {
                    Date = current,
                    DayOfWeek = current.DayOfWeek.ToString(),
                    IsWeekend = current.DayOfWeek == DayOfWeek.Friday || current.DayOfWeek == DayOfWeek.Saturday,
                    IsHoliday = holiday != null,
                    HolidayName = holiday?.Holidayname,
                    EmployeesOnLeave = dayLeaves.Count,
                    Leaves = dayLeaves
                });

                current = current.AddDays(1);
            }

            return new
            {
                Year = year,
                Month = month,
                TotalLeaveDays = leaves.Sum(l => l.Days),
                TotalEmployeesOnLeave = leaves.Select(l => l.EmployeeId).Distinct().Count(),
                Calendar = calendar
            };
        }

        public async Task<int> ProcessLeaveExpirationsAsync()
        {
            var currentYear = DateTime.Now.Year;
            var expirationDate = new DateTime(currentYear, 3, 31); // Assuming leave expires on March 31

            if (DateTime.Today > expirationDate)
            {
                // Get all balances that need to be expired
                var balances = await Context.Queryable<HrLeaveBalance>()
                    .Where(b => b.Year == currentYear - 1 && b.BalanceMinutes > 0)
                    .ToListAsync();

                foreach (var balance in balances)
                {
                    // Check carry-over policy
                    var leaveType = await Context.Queryable<HrLeaveType>()
                        .FirstAsync(t => t.LeaveTypeId == balance.LeaveTypeId);

                    if (leaveType != null)
                    {
                        var maxCarryOverDays = leaveType.MaxCarryForward;
                        var availableDays = balance.AvailableDays;
                        var carryOverDays = Math.Min(availableDays, maxCarryOverDays);

                        // Convert to minutes
                        var carryOverMinutes = carryOverDays * 480;
                        var expiredMinutes = (availableDays - carryOverDays) * 480;

                        // Update expired minutes
                        balance.ExpiredMinutes = expiredMinutes;
                        balance.BalanceMinutes = 0;
                        balance.Update_time = DateTime.Now;

                        await Context.Updateable(balance)
                            .UpdateColumns(b => new { b.ExpiredMinutes, b.BalanceMinutes, b.Update_time })
                            .ExecuteCommandAsync();

                        // Add carry-over to current year balance if applicable
                        if (carryOverMinutes > 0)
                        {
                            var currentBalance = await Context.Queryable<HrLeaveBalance>()
                                .FirstAsync(b => b.EmployeeId == balance.EmployeeId &&
                                               b.LeaveTypeId == balance.LeaveTypeId &&
                                               b.Year == currentYear);

                            if (currentBalance != null)
                            {
                                currentBalance.CarriedOverMinutes = carryOverMinutes;
                                currentBalance.BalanceMinutes += carryOverMinutes;
                                currentBalance.Update_time = DateTime.Now;

                                await Context.Updateable(currentBalance).ExecuteCommandAsync();
                            }
                        }
                    }
                }

                return balances.Count;
            }

            return 0;
        }

        // Helper methods
        private async Task CreateLeaveAttendanceRecords(HrLeaveRequest request)
        {
            var current = request.StartDate;
            var attendanceRecords = new List<HrAttendanceDay>();

            while (current <= request.EndDate)
            {
                // Skip weekends
                if (current.DayOfWeek != DayOfWeek.Friday && current.DayOfWeek != DayOfWeek.Saturday)
                {
                    // Check if it's not a holiday
                    var isHoliday = await IsHolidayAsync(current);
                    if (!isHoliday)
                    {
                        // Check if attendance record exists
                        var exists = await Context.Queryable<HrAttendanceDay>()
                            .AnyAsync(a => a.EmployeeId == request.EmployeeId && a.AttendanceDate == current);

                        if (!exists)
                        {
                            attendanceRecords.Add(new HrAttendanceDay
                            {
                                EmployeeId = request.EmployeeId,
                                AttendanceDate = current,
                                Status = "leave",
                                Remark = $"On leave - {request.LeaveTypeId}",
                                Create_by = "System",
                                Create_time = DateTime.Now
                            });
                        }
                    }
                }
                current = current.AddDays(1);
            }

            if (attendanceRecords.Any())
            {
                await Context.Insertable(attendanceRecords).ExecuteCommandAsync();
            }
        }

        private async Task SendLeaveNotification(HrLeaveRequest request, bool approved)
        {
            // Implementation for sending notifications (email, SMS, push notification)
            // This would integrate with your notification service

            var employee = await _employeeDataService.GetEmployeeFullDataAsync(request.EmployeeId);

            if (employee != null && !string.IsNullOrEmpty(employee.Email))
            {
                var subject = approved ? "Leave Request Approved" : "Leave Request Rejected";
                var body = $"Your leave request from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd} has been {(approved ? "approved" : "rejected")}.";

                if (!string.IsNullOrEmpty(request.ApproveComments))
                {
                    body += $"\n\nComments: {request.ApproveComments}";
                }

                // Send email notification
                // await _emailService.SendAsync(employee.Email, subject, body);
            }
        }

        public async Task<bool> UpdateRequestStatus(long requestId, string status, string comments)
        {
            var request = await GetByIdAsync(requestId);
            if (request == null) return false;

            request.Status = status;
            request.ApproveComments = comments;
            request.Update_time = DateTime.Now;

            return await Context.Updateable(request)
                .UpdateColumns(r => new { r.Status, r.ApproveComments, r.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> ApproveRequest(long requestId, bool approved, string comments, string approvedBy)
        {
            return await ApproveRequestAsync(requestId, approved, comments, approvedBy);
        }




        #region Leave Request Management

        /// <summary>
        /// Get employee leave requests
        /// </summary>
        public async Task<List<HrLeaveRequest>> GetEmployeeLeaveRequestsAsync(long employeeId, int year)
        {
            return await Context.Queryable<HrLeaveRequest>()
                .Where(lr => lr.EmployeeId == employeeId && lr.StartDate.Year == year)
                .OrderBy(lr => lr.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        /// <summary>
        /// Get employee leave history
        /// </summary>
        public async Task<object> GetEmployeeLeaveHistoryAsync(long employeeId, int year)
        {
            return await GetEmployeeLeaveRequestsAsync(employeeId, year);
        }

        /// <summary>
        /// Get leave request by ID
        /// </summary>
        public async Task<HrLeaveRequest> GetLeaveRequestByIdAsync(long leaveId)
        {
            return await Context.Queryable<HrLeaveRequest>()
                .Where(lr => lr.RequestId == leaveId)
                .FirstAsync();
        }

        /// <summary>
        /// Submit leave request with full validation
        /// </summary>
        public async Task<int> SubmitLeaveRequestAsync(HrLeaveRequest request)
        {
            // 1. Validate dates
            if (request.StartDate == default || request.EndDate == default)
                throw new AppException(ExceptionCode.HR_LEAVE_INVALID_DATES, "Start date and end date are required");

            if (request.EndDate < request.StartDate)
                throw new AppException(ExceptionCode.HR_LEAVE_INVALID_DATES, "End date must be after start date");

            // 2. Validate leave type exists and is active
            var leaveType = await Context.Queryable<HrLeaveType>()
                .FirstAsync(t => t.LeaveTypeId == request.LeaveTypeId);

            if (leaveType == null)
                throw new AppException(ExceptionCode.HR_LEAVE_TYPE_NOT_FOUND, "Leave type not found");

            if (!leaveType.IsActive)
                throw new AppException(ExceptionCode.HR_LEAVE_TYPE_INACTIVE, "Leave type is inactive");

            // 3. Validate min/max days
            var days = request.Days ?? 0;
            if (leaveType.MinDays.HasValue && leaveType.MinDays.Value > 0 && days < leaveType.MinDays.Value)
                throw new AppException(ExceptionCode.HR_LEAVE_MIN_DAYS, $"Minimum {leaveType.MinDays.Value} days required for this leave type");

            if (leaveType.MaxDays.HasValue && leaveType.MaxDays.Value > 0 && days > leaveType.MaxDays.Value)
                throw new AppException(ExceptionCode.HR_LEAVE_MAX_DAYS, $"Maximum {leaveType.MaxDays.Value} days allowed for this leave type");

            // 4. Validate advance notice
            if (leaveType.AdvanceNoticeDays.HasValue && leaveType.AdvanceNoticeDays.Value > 0)
            {
                var daysUntilStart = (request.StartDate - DateTime.Today).Days;
                if (daysUntilStart < leaveType.AdvanceNoticeDays.Value && !request.IsEmergency)
                    throw new AppException(ExceptionCode.HR_LEAVE_ADVANCE_NOTICE, $"This leave type requires {leaveType.AdvanceNoticeDays.Value} days advance notice");
            }

            // 5. Validate document requirement
            if (leaveType.RequiresDocument && string.IsNullOrWhiteSpace(request.AttachmentUrl))
                throw new AppException(ExceptionCode.HR_LEAVE_DOCUMENT_REQUIRED, "This leave type requires a supporting document");

            // 6. Validate leave balance
            var balance = await _balanceService.GetBalanceAsync(request.EmployeeId, request.LeaveTypeId);
            if (balance == null || balance.AvailableDays < days)
                throw new AppException(ExceptionCode.HR_LEAVE_INSUFFICIENT_BALANCE,
                    $"Insufficient leave balance. Available: {balance?.AvailableDays ?? 0} days, Requested: {days} days");

            // 7. Check for overlapping leave requests
            var hasConflict = await CheckLeaveConflictAsync(request.EmployeeId, request.StartDate, request.EndDate);
            if (hasConflict)
                throw new AppException(ExceptionCode.HR_LEAVE_CONFLICT, "Leave request conflicts with an existing leave in this period");

            // Set defaults
            request.Status = "Pending";
            request.Create_time = DateTime.Now;

            // Handle emergency leave auto-approval
            if (request.IsEmergency && !leaveType.RequiresApproval)
            {
                request.Status = "Approved";
                request.ApproveDate = DateTime.Now;
                request.ApproveComments = "Auto-approved (Emergency)";
                await _balanceService.DeductBalanceAsync(request.EmployeeId, request.LeaveTypeId, days);
            }

            return await Context.Insertable(request).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update leave request
        /// </summary>
        public async Task<int> UpdateLeaveRequestAsync(HrLeaveRequest request)
        {
            request.Update_time = DateTime.Now;
            return await Context.Updateable(request).ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete leave request
        /// </summary>
        public async Task<bool> DeleteLeaveRequestAsync(long leaveId)
        {
            return await Context.Deleteable<HrLeaveRequest>()
                .Where(lr => lr.RequestId == leaveId)
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Approve leave request
        /// </summary>
        public async Task<bool> ApproveLeaveRequestAsync(long leaveId, long approvedBy, string comments)
        {
            var request = await GetLeaveRequestByIdAsync(leaveId);
            if (request == null) return false;

            request.Status = "Approved";
            request.ApprovedBy = approvedBy;
            request.ApprovalDate = DateTime.Now;
            request.ApprovalComments = comments;
            request.Update_time = DateTime.Now;

            // Deduct leave balance
            await DeductLeaveBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.Days ?? 0);

            return await Context.Updateable(request).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Reject leave request
        /// </summary>
        public async Task<bool> RejectLeaveRequestAsync(long leaveId, long rejectedBy, string reason)
        {
            var request = await GetLeaveRequestByIdAsync(leaveId);
            if (request == null) return false;

            request.Status = "Rejected";
            request.ApprovedBy = rejectedBy;
            request.ApprovalDate = DateTime.Now;
            request.ApprovalComments = reason;
            request.Update_time = DateTime.Now;

            return await Context.Updateable(request).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Cancel leave request
        /// </summary>
        public async Task<bool> CancelLeaveRequestAsync(long leaveId, string reason)
        {
            var request = await GetLeaveRequestByIdAsync(leaveId);
            if (request == null) return false;

            request.Status = "Cancelled";
            request.RejectionReason = reason;
            request.Update_time = DateTime.Now;

            // Restore leave balance if already approved
            if (request.Status == "Approved")
            {
                await AddLeaveBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.Days ?? 0);
            }

            return await Context.Updateable(request).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Get pending leave requests for manager
        /// </summary>
        public async Task<List<HrLeaveRequest>> GetPendingLeaveRequestsAsync(long managerId)
        {
            // Get team members for this manager
            var teamMembers = await _employeeDataService.GetTeamMembersAsync(managerId);
            var subordinateIds = teamMembers.Select(e => e.EmployeeId).ToList();

            if (!subordinateIds.Any())
            {
                return new List<HrLeaveRequest>();
            }

            return await Context.Queryable<HrLeaveRequest>()
                .Where(lr => subordinateIds.Contains(lr.EmployeeId) && lr.Status == "Pending")
                .ToListAsync();
        }

        /// <summary>
        /// Get leave requests by status
        /// </summary>
        public async Task<List<HrLeaveRequest>> GetLeaveRequestsByStatusAsync(string status)
        {
            return await Context.Queryable<HrLeaveRequest>()
                .Where(lr => lr.Status == status)
                .ToListAsync();
        }

        #endregion

        #region Leave Balance Management


        /// <summary>
        /// Get employee leave balance
        /// </summary>
        public async Task<List<HrLeaveBalance>> GetEmployeeLeaveBalanceAsync(long? employeeId, int year)
        {
            var query = Context.Queryable<HrLeaveBalance>()
                .Where(lb => lb.Year == year);

            if (employeeId.HasValue)
            {
                query = query.Where(lb => lb.EmployeeId == employeeId.Value);
            }

            return await query.ToListAsync();
        }


   

        /// <summary>
        /// Update leave balance
        /// </summary>
        public async Task<bool> UpdateLeaveBalanceAsync(HrLeaveBalance balance)
        {
            balance.Update_time = DateTime.Now;
            return await Context.Updateable(balance).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Deduct leave balance
        /// </summary>
        public async Task<bool> DeductLeaveBalanceAsync(long employeeId, long leaveTypeId, decimal days)
        {
            var balance = await Context.Queryable<HrLeaveBalance>()
                .Where(lb => lb.EmployeeId == employeeId && 
                            lb.LeaveTypeId == leaveTypeId && 
                            lb.Year == DateTime.Now.Year)
                .FirstAsync();

            if (balance == null) return false;

            // Convert days to minutes
            var minutes = days * 480;
            balance.UsedMinutes += minutes;
            balance.BalanceMinutes = balance.EntitledMinutes - balance.UsedMinutes;
            balance.Update_time = DateTime.Now;

            return await Context.Updateable(balance)
                .UpdateColumns(b => new { b.UsedMinutes, b.BalanceMinutes, b.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Add leave balance
        /// </summary>
        public async Task<bool> AddLeaveBalanceAsync(long employeeId, long leaveTypeId, decimal days)
        {
            var balance = await Context.Queryable<HrLeaveBalance>()
                .Where(lb => lb.EmployeeId == employeeId && 
                            lb.LeaveTypeId == leaveTypeId && 
                            lb.Year == DateTime.Now.Year)
                .FirstAsync();

            if (balance == null) return false;

            // Convert days to minutes
            var minutes = days * 480;
            balance.EntitledMinutes += minutes;
            balance.BalanceMinutes = balance.EntitledMinutes - balance.UsedMinutes;
            balance.Update_time = DateTime.Now;

            return await Context.Updateable(balance)
                .UpdateColumns(b => new { b.EntitledMinutes, b.BalanceMinutes, b.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Process year end balances
        /// </summary>
        public async Task<bool> ProcessYearEndBalancesAsync(int year)
        {
            // Process carry forward for all employees
            var balances = await Context.Queryable<HrLeaveBalance>()
                .Where(lb => lb.Year == year)
                .ToListAsync();

            foreach (var balance in balances)
            {
                await CarryForwardLeaveAsync(balance.EmployeeId, year, year + 1);
            }

            return true;
        }

        /// <summary>
        /// Carry forward leave balance
        /// </summary>
        public async Task<bool> CarryForwardLeaveAsync(long employeeId, int fromYear, int toYear)
        {
            var oldBalances = await GetEmployeeLeaveBalanceAsync(employeeId, fromYear);
            
            foreach (var oldBalance in oldBalances)
            {
                var leaveType = await GetLeaveTypeByIdAsync(oldBalance.LeaveTypeId);
                if (leaveType == null || !leaveType.AllowCarryForward) continue;

                var carryForwardDays = Math.Min(oldBalance.AvailableDays, leaveType.MaxCarryForward);

                var totalEntitledMinutes = (leaveType.DefaultDays + carryForwardDays) * 480;
                var newBalance = new HrLeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = oldBalance.LeaveTypeId,
                    Year = toYear,
                    EntitledMinutes = totalEntitledMinutes,
                    UsedMinutes = 0,
                    BalanceMinutes = totalEntitledMinutes,
                    CarriedOverMinutes = carryForwardDays * 480,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(newBalance).ExecuteCommandAsync();
            }

            return true;
        }

        #endregion

        #region Leave Type Management

        /// <summary>
        /// Get all leave types
        /// </summary>
        public async Task<List<HrLeaveType>> GetLeaveTypesAsync()
        {
            return await Context.Queryable<HrLeaveType>()
                .Where(lt => lt.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Get leave type by ID
        /// </summary>
        public async Task<HrLeaveType> GetLeaveTypeByIdAsync(long typeId)
        {
            return await Context.Queryable<HrLeaveType>()
                .Where(lt => lt.LeaveTypeId == typeId)
                .FirstAsync();
        }

        /// <summary>
        /// Add leave type
        /// </summary>
        public async Task<int> AddLeaveTypeAsync(HrLeaveType leaveType)
        {
            leaveType.Create_time = DateTime.Now;
            leaveType.IsActive = true;
            return await Context.Insertable(leaveType).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update leave type
        /// </summary>
        public async Task<bool> UpdateLeaveTypeAsync(HrLeaveType leaveType)
        {
            leaveType.Update_time = DateTime.Now;
            return await Context.Updateable(leaveType).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Delete leave type (soft delete - mark as inactive)
        /// </summary>
        public async Task<bool> DeleteLeaveTypeAsync(long typeId)
        {
            var leaveType = await GetLeaveTypeByIdAsync(typeId);
            if (leaveType == null) return false;

            leaveType.IsActive = false;
            leaveType.Update_time = DateTime.Now;

            return await Context.Updateable(leaveType).ExecuteCommandAsync() > 0;
        }

        #endregion

        #region Holidays Management


    


        /// <summary>
        /// Get holidays for year
        /// </summary>
        public async Task<List<HrHoliday>> GetHolidaysAsync(int year)
        {
            return await Context.Queryable<HrHoliday>()
                .Where(h => h.Holidaydate.Year == year)
                .OrderBy(h => h.Holidaydate)
                .ToListAsync();
        }

        /// <summary>
        /// Add holiday
        /// </summary>
        public async Task<bool> AddHolidayAsync(HrHoliday holiday)
        {
            holiday.Create_time = DateTime.Now;





            // Check if holiday already exists
            var exists = await Context.Queryable<HrHoliday>()
                .AnyAsync(h => h.Holidaydate.Date == holiday.Holidaydate.Date);

            if (exists)
            {
                throw new AppException(ExceptionCode.CONFLICT, "Holiday already exists for this date");
            }

            holiday.Year = holiday.Holidaydate.Year;
            return await Context.Insertable(holiday).ExecuteReturnSnowflakeIdAsync() > 0;







             
        }

        /// <summary>
        /// Update holiday
        /// </summary>
        public async Task<bool> UpdateHolidayAsync(HrHoliday holiday)
        {
            holiday.Update_time = DateTime.Now;
            return await Context.Updateable(holiday).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Delete holiday
        /// </summary>
        public async Task<bool> DeleteHolidayAsync(long holidayId)
        {
            return await Context.Deleteable<HrHoliday>()
                .Where(h => h.Holidayid == holidayId)
                .ExecuteCommandAsync() > 0;
        }


        #endregion

        #region Reports

        /// <summary>
        /// Generate leave report for employee
        /// </summary>
        public async Task<byte[]> GenerateLeaveReportAsync(long employeeId, int year)
        {
            // This would typically generate an Excel or PDF report
            // For now, returning empty byte array as placeholder
            return await Task.FromResult(new byte[0]);
        }

        /// <summary>
        /// Generate department leave report
        /// </summary>
        public async Task<byte[]> GenerateDepartmentLeaveReportAsync(long departmentId, int year)
        {
            // This would typically generate an Excel or PDF report
            // For now, returning empty byte array as placeholder
            return await Task.FromResult(new byte[0]);
        }

        #endregion
    }

    // Supporting classes (these would typically be in separate model files)
 

 

  
 
}
