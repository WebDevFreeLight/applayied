using ZR.Model.HR.EmployeeCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Repository;
using ZR.Service.HR.IService;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// Leave Balance Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(ILeaveBalanceService), ServiceLifetime = LifeTime.Transient)]
    public class LeaveBalanceService : BaseService<HrLeaveBalance>, ILeaveBalanceService
    {
        private readonly IEmployeeDataService _employeeDataService;

        public LeaveBalanceService(IEmployeeDataService employeeDataService)
        {
            _employeeDataService = employeeDataService;
        }
        public async Task<HrLeaveBalance> GetBalanceAsync(long employeeId, long leaveTypeId)
        {
            var currentYear = DateTime.Now.Year;

            var balance = await GetFirstAsync(b =>
                b.EmployeeId == employeeId &&
                b.LeaveTypeId == leaveTypeId &&
                b.Year == currentYear);

            if (balance == null)
            {
                await InitializeEmployeeBalancesAsync(employeeId);
                // Re-fetch after initialization
                balance = await GetFirstAsync(b =>
                    b.EmployeeId == employeeId &&
                    b.LeaveTypeId == leaveTypeId &&
                    b.Year == currentYear);
            }

            return balance;
        }

        public async Task<List<HrLeaveBalance>> GetEmployeeBalancesAsync(long employeeId)
        {
            var currentYear = DateTime.Now.Year;

            // Check if balances exist for this employee, if not initialize them
            var count = await Queryable()
                .Where(b => b.EmployeeId == employeeId && b.Year == currentYear)
                .CountAsync();

            if (count == 0)
            {
                await InitializeEmployeeBalancesAsync(employeeId);
            }

            return await Queryable()
                .InnerJoin<HrLeaveType>((b, t) => b.LeaveTypeId == t.LeaveTypeId)
                .Where((b, t) => b.EmployeeId == employeeId && b.Year == currentYear && t.IsActive)
                .Select((b, t) => b)
                .ToListAsync();
        }

        public async Task<int> SetBalanceAsync(HrLeaveBalance balance)
        {
            var existing = await GetBalanceAsync(balance.EmployeeId, balance.LeaveTypeId);
            
            if (existing != null)
            {
                balance.BalanceId = existing.BalanceId;
                balance.Update_time = DateTime.Now;
                return await Context.Updateable(balance)
                    .IgnoreColumns(b => new { b.Create_by, b.Create_time })
                    .ExecuteCommandAsync();
            }
            else
            {
                balance.Create_time = DateTime.Now;
                balance.Year = DateTime.Now.Year;
                long insertResult = await InsertAsync(balance);

                int response = insertResult.ParseToInt();

                    return response;
            }
        }

        public async Task<bool> DeductBalanceAsync(long employeeId, long leaveTypeId, decimal days)
        {
            var balance = await GetBalanceAsync(employeeId, leaveTypeId);

            if (balance == null || balance.AvailableDays < days)
            {
                return false;
            }

            // Convert days to minutes (8 hours = 480 minutes)
            var minutes = days * 480;
            balance.UsedMinutes += minutes;
            balance.BalanceMinutes = balance.EntitledMinutes - balance.UsedMinutes;
            balance.Update_time = DateTime.Now;

            var result = await Context.Updateable(balance)
                .UpdateColumns(b => new { b.UsedMinutes, b.BalanceMinutes, b.Update_time })
                .ExecuteCommandAsync();

            return result > 0;
        }

        public async Task<bool> AddBalanceAsync(long employeeId, long leaveTypeId, decimal days)
        {
            var balance = await GetBalanceAsync(employeeId, leaveTypeId);
            
            if (balance == null)
            {
                return false;
            }

            // Convert days to minutes (8 hours = 480 minutes)
            var minutes = days * 480;
            balance.UsedMinutes -= minutes;
            balance.BalanceMinutes = balance.EntitledMinutes - balance.UsedMinutes;
            balance.Update_time = DateTime.Now;

            var result = await Context.Updateable(balance)
                .UpdateColumns(b => new { b.UsedMinutes, b.BalanceMinutes, b.Update_time })
                .ExecuteCommandAsync();

            return result > 0;
        }

        public async Task<int> InitializeEmployeeBalancesAsync(long employeeId)
        {
            var currentYear = DateTime.Now.Year;
            var leaveTypes = await Context.Queryable<HrLeaveType>()
                .Where(t => t.IsActive)
                .ToListAsync();

            var balances = new List<HrLeaveBalance>();

            foreach (var leaveType in leaveTypes)
            {
                var existing = await GetFirstAsync(b =>
                    b.EmployeeId == employeeId &&
                    b.LeaveTypeId == leaveType.LeaveTypeId &&
                    b.Year == currentYear);

                if (existing == null)
                {
                    balances.Add(new HrLeaveBalance
                    {
                        EmployeeId = employeeId,
                        LeaveTypeId = leaveType.LeaveTypeId,
                        Year = currentYear,
                        EntitledMinutes = leaveType.DaysPerYear * 480,
                        UsedMinutes = 0,
                        BalanceMinutes = leaveType.DaysPerYear * 480,
                        CarriedOverMinutes = 0,
                        ExpiredMinutes = 0,
                        Create_time = DateTime.Now,
                        Create_by = "System"
                    });
                }
            }

            if (balances.Any())
            {
                return await Context.Insertable(balances).ExecuteCommandAsync();
            }

            return 0;
        }

        public async Task<int> ProcessYearlyEntitlementsAsync(int year)
        {
            // Use EmployeeDataService to get active employees
            var employees = await _employeeDataService.GetAllActiveEmployeesAsync();

            var leaveTypes = await Context.Queryable<HrLeaveType>()
                .Where(t => t.IsActive)
                .ToListAsync();

            var balances = new List<HrLeaveBalance>();

            foreach (var employee in employees)
            {
                foreach (var leaveType in leaveTypes)
                {
                    var existing = await GetFirstAsync(b =>
                        b.EmployeeId == employee.EmployeeId &&
                        b.LeaveTypeId == leaveType.LeaveTypeId &&
                        b.Year == year);

                    if (existing == null)
                    {
                        // Calculate pro-rated entitlement if employee joined during the year
                        var entitledDays = leaveType.DaysPerYear;

                        if (employee.HireDate?.Year == year)
                        {
                            var monthsWorked = 12 -( employee.HireDate?.Month??0) + 1;
                            entitledDays = Math.Round((entitledDays / 12) * monthsWorked, 1);
                        }

                        balances.Add(new HrLeaveBalance
                        {
                            EmployeeId = employee.EmployeeId,
                            LeaveTypeId = leaveType.LeaveTypeId,
                            Year = year,
                            EntitledMinutes = entitledDays * 480,
                            UsedMinutes = 0,
                            BalanceMinutes = entitledDays * 480,
                            CarriedOverMinutes = 0,
                            ExpiredMinutes = 0,
                            Create_time = DateTime.Now,
                            Create_by = "System"
                        });
                    }
                }
            }

            if (balances.Any())
            {
                return await Context.Insertable(balances).ExecuteCommandAsync();
            }

            return 0;
        }

        public async Task<int> ProcessCarryOverAsync(int fromYear, int toYear)
        {
            var fromBalances = await Queryable()
                .LeftJoin<HrLeaveType>((b, t) => b.LeaveTypeId == t.LeaveTypeId)
                .Where((b, t) => b.Year == fromYear && b.BalanceMinutes > 0)
                .Select((b, t) => new { Balance = b, MaxCarryOver = t.MaxCarryForward })
                .ToListAsync();

            var processedCount = 0;

            foreach (var item in fromBalances)
            {
                var balance = item.Balance;
                var maxCarryOverDays = item.MaxCarryOver;
                var availableDays = balance.AvailableDays;
                var carryOverDays = Math.Min(availableDays, maxCarryOverDays);
                var carryOverMinutes = carryOverDays * 480;

                if (carryOverMinutes > 0)
                {
                    // Update to-year balance
                    var toBalance = await GetFirstAsync(b =>
                        b.EmployeeId == balance.EmployeeId &&
                        b.LeaveTypeId == balance.LeaveTypeId &&
                        b.Year == toYear);

                    if (toBalance != null)
                    {
                        toBalance.CarriedOverMinutes = carryOverMinutes;
                        toBalance.BalanceMinutes += carryOverMinutes;
                        toBalance.Update_time = DateTime.Now;

                        await Context.Updateable(toBalance)
                            .UpdateColumns(b => new { b.CarriedOverMinutes, b.BalanceMinutes, b.Update_time })
                            .ExecuteCommandAsync();
                    }
                }

                // Mark expired minutes in from-year balance
                var expiredMinutes = (availableDays - carryOverDays) * 480;
                balance.ExpiredMinutes = expiredMinutes;
                balance.BalanceMinutes = 0;
                balance.Update_time = DateTime.Now;
                
                await Context.Updateable(balance)
                    .UpdateColumns(b => new { b.ExpiredMinutes, b.BalanceMinutes, b.Update_time })
                    .ExecuteCommandAsync();

                processedCount++;
            }

            return processedCount;
        }

        public async Task<object> GetBalanceSummaryAsync(long employeeId)
        {
            var currentYear = DateTime.Now.Year;
            var balances = await Queryable()
                .LeftJoin<HrLeaveType>((b, t) => b.LeaveTypeId == t.LeaveTypeId)
                .Where((b, t) => b.EmployeeId == employeeId && b.Year == currentYear)
                .Select((b, t) => new
                {
                    b.BalanceId,
                    b.LeaveTypeId,
                    LeaveTypeName = t.TypeName,
                    EntitledDays = b.EntitledDays,
                    UsedDays = b.UsedDays,
                    AvailableDays = b.AvailableDays,
                    CarriedOverDays = b.CarriedOverMinutes / 480,
                    ExpiredDays = b.ExpiredMinutes / 480,
                    IsPaid = t.IsPaid
                })
                .ToListAsync();

            return new
            {
                EmployeeId = employeeId,
                Year = currentYear,
                TotalEntitled = balances.Sum(b => b.EntitledDays),
                TotalUsed = balances.Sum(b => b.UsedDays),
                TotalBalance = balances.Sum(b => b.AvailableDays),
                TotalCarriedOver = balances.Sum(b => b.CarriedOverDays),
                Details = balances
            };
        }

        public async Task<List<object>> GetDepartmentBalancesAsync(long departmentId)
        {
            var currentYear = DateTime.Now.Year;

            // Get employees by department using EmployeeDataService
            var employees = await _employeeDataService.GetEmployeesByDepartmentAsync(departmentId);
            var employeeIds = employees.Select(e => e.EmployeeId).ToList();

            // Create employee lookup dictionary
            var employeeLookup = employees.ToDictionary(e => e.EmployeeId);

            // Get balances for these employees
            var balances = await Context.Queryable<HrLeaveBalance>()
                .Where(b => employeeIds.Contains(b.EmployeeId) && b.Year == currentYear)
                .ToListAsync();

            // Group and aggregate balances
            var result = balances
                .GroupBy(b => b.EmployeeId)
                .Select(g =>
                {
                    var employee = employeeLookup[g.Key];
                    return new
                    {
                        EmployeeId = g.Key,
                        EmployeeName = employee.NameAr ?? employee.FullName,
                        EmployeeCode = employee.EmployeeCode,
                        TotalEntitled = g.Sum(b => b.EntitledMinutes) / 480,
                        TotalUsed = g.Sum(b => b.UsedMinutes) / 480,
                        TotalBalance = g.Sum(b => b.BalanceMinutes) / 480
                    } as object;
                })
                .ToList();

            return result;
        }

        public async Task<int> AdjustBalanceAsync(long employeeId, long leaveTypeId, decimal adjustment, string reason)
        {
            var balance = await GetBalanceAsync(employeeId, leaveTypeId);
            
            if (balance == null)
            {
                return 0;
            }

            // Convert days to minutes
            var adjustmentMinutes = adjustment * 480;

            if (adjustment > 0)
            {
                // Positive adjustment - add to entitled
                balance.EntitledMinutes += adjustmentMinutes;
            }
            else
            {
                // Negative adjustment - add to used
                balance.UsedMinutes += Math.Abs(adjustmentMinutes);
            }

            // Recalculate balance
            balance.BalanceMinutes = balance.EntitledMinutes - balance.UsedMinutes;
            balance.Update_time = DateTime.Now;
            balance.Update_by = reason;

            return await Context.Updateable(balance)
                .UpdateColumns(b => new { b.BalanceMinutes, b.EntitledMinutes, b.UsedMinutes, b.Update_time, b.Update_by })
                .ExecuteCommandAsync();
        }
    }
}
