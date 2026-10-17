using Infrastructure.Attribute;
using Quartz;
using SqlSugar;
using SqlSugar.IOC;
using System;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;
using ZR.Service.HR.IHrService;

namespace ZR.Tasks.TaskScheduler
{
    /// <summary>
    /// Contract & Probation Expiry Alert Job
    /// Runs daily to check for expiring contracts and probation periods.
    /// Sends notifications at 90, 60, and 30 days before expiry.
    /// Cron: 0 0 7 * * ?  (daily at 7 AM)
    /// </summary>
    [AppService(ServiceType = typeof(Job_ContractExpiryAlert), ServiceLifetime = LifeTime.Scoped)]
    public class Job_ContractExpiryAlert : JobBase, IJob
    {
        private readonly INotificationService _notificationService;

        public Job_ContractExpiryAlert(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteJob(context, Run);
        }

        public async Task<string> Run()
        {
            var db = DbScoped.SugarScope;
            var today = DateTime.Today;
            var results = new System.Text.StringBuilder();
            int contractAlerts = 0;
            int probationAlerts = 0;

            // --- Contract Expiry Alerts (30, 60, 90 days) ---
            var thresholdDays = new[] { 30, 60, 90 };

            foreach (var days in thresholdDays)
            {
                var targetDate = today.AddDays(days);

                // Find contracts expiring on exactly this threshold date
                var expiringContracts = await db.Queryable<HrEmployeeContract>()
                    .Where(c => c.IsCurrent &&
                                c.ContractEndDate != null &&
                                c.ContractEndDate.Value.Date == targetDate.Date)
                    .ToListAsync();

                foreach (var contract in expiringContracts)
                {
                    try
                    {
                        var notification = new ZR.Model.HR.HrNotification
                        {
                            EmployeeId = contract.EmployeeId,
                            RecipientId = contract.EmployeeId,
                            Subject = $"Contract Expiry Alert - {days} Days",
                            Body = $"Your employment contract expires on {contract.ContractEndDate:yyyy-MM-dd}. Please contact HR for renewal.",
                            NotificationType = "in_app",
                            Category = "document",
                            Priority = days <= 30 ? "high" : "medium",
                            ReferenceType = "contract_expiry",
                            IsRead = false,
                            Create_time = DateTime.Now,
                            Create_by = "System"
                        };
                        await _notificationService.SendNotificationAsync(notification);
                        contractAlerts++;
                    }
                    catch { /* Log but continue processing others */ }
                }
            }

            // --- Probation End Alerts (14 days before) ---
            var probationTargetDate = today.AddDays(14);
            var endingProbations = await db.Queryable<HrEmployeeContract>()
                .Where(c => c.IsCurrent &&
                            c.ProbationEndDate != null &&
                            c.ProbationEndDate.Value.Date == probationTargetDate.Date)
                .ToListAsync();

            foreach (var contract in endingProbations)
            {
                try
                {
                    var notification = new ZR.Model.HR.HrNotification
                    {
                        EmployeeId = contract.EmployeeId,
                        RecipientId = contract.EmployeeId,
                        Subject = "Probation Period Ending - 14 Days",
                        Body = $"Probation period ends on {contract.ProbationEndDate:yyyy-MM-dd}. Manager review required.",
                        NotificationType = "in_app",
                        Category = "document",
                        ReferenceType = "probation_end",
                        Priority = "high",
                        IsRead = false,
                        Create_time = DateTime.Now,
                        Create_by = "System"
                    };
                    await _notificationService.SendNotificationAsync(notification);
                    probationAlerts++;
                }
                catch { /* Log but continue */ }
            }

            results.AppendLine($"Contract expiry alerts sent: {contractAlerts}");
            results.AppendLine($"Probation end alerts sent: {probationAlerts}");

            return results.ToString().Trim();
        }
    }
}
