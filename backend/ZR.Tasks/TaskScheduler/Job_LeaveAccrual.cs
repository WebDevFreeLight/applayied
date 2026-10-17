using Infrastructure.Attribute;
using Quartz;
using System;
using System.Threading.Tasks;
using ZR.Service.HR.IService;

namespace ZR.Tasks.TaskScheduler
{
    /// <summary>
    /// Leave Accrual Job
    /// Runs monthly (1st of each month) to process leave entitlements.
    /// Runs Jan 1 for yearly entitlements and carry-over from previous year.
    /// Cron: 0 0 1 1 * ?  (monthly at midnight on 1st)
    /// </summary>
    [AppService(ServiceType = typeof(Job_LeaveAccrual), ServiceLifetime = LifeTime.Scoped)]
    public class Job_LeaveAccrual : JobBase, IJob
    {
        private readonly ILeaveBalanceService _leaveBalanceService;

        public Job_LeaveAccrual(ILeaveBalanceService leaveBalanceService)
        {
            _leaveBalanceService = leaveBalanceService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteJob(context, Run);
        }

        public async Task<string> Run()
        {
            var now = DateTime.Now;
            var year = now.Year;
            var month = now.Month;
            var results = new System.Text.StringBuilder();

            // On January: process carry-over from previous year, then create new year entitlements
            if (month == 1)
            {
                var carryOverCount = await _leaveBalanceService.ProcessCarryOverAsync(year - 1, year);
                results.AppendLine($"Carry-over processed: {carryOverCount} balances from {year - 1} to {year}");
            }

            // Process yearly entitlements (creates balances for employees who don't have them yet)
            var entitlementCount = await _leaveBalanceService.ProcessYearlyEntitlementsAsync(year);
            results.AppendLine($"Yearly entitlements processed: {entitlementCount} new balances created for {year}");

            return results.ToString().Trim();
        }
    }
}
