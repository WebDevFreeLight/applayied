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
    /// Document Expiry Alert Job
    /// Runs daily to check for expiring employee documents (Iqama, passport, license, etc.).
    /// Sends notifications at 90, 60, and 30 days before expiry.
    /// Cron: 0 0 7 * * ?  (daily at 7 AM)
    /// </summary>
    [AppService(ServiceType = typeof(Job_DocumentExpiryAlert), ServiceLifetime = LifeTime.Scoped)]
    public class Job_DocumentExpiryAlert : JobBase, IJob
    {
        private readonly INotificationService _notificationService;

        public Job_DocumentExpiryAlert(INotificationService notificationService)
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
            int alertsSent = 0;

            var thresholdDays = new[] { 30, 60, 90 };

            foreach (var days in thresholdDays)
            {
                var targetDate = today.AddDays(days);

                // Find documents expiring on exactly this threshold date
                var expiringDocs = await db.Queryable<HrEmployeeDocument>()
                    .Where(d => d.Status == "Active" &&
                                d.ExpiryDate != null &&
                                d.ExpiryDate.Value.Date == targetDate.Date)
                    .ToListAsync();

                foreach (var doc in expiringDocs)
                {
                    try
                    {
                        await _notificationService.SendDocumentExpiryNotificationAsync(
                            doc.EmployeeId,
                            doc.DocumentName ?? "Document",
                            doc.ExpiryDate.Value);
                        alertsSent++;
                    }
                    catch { /* Log but continue processing others */ }
                }
            }

            // Also find already expired documents (not yet flagged)
            var expiredDocs = await db.Queryable<HrEmployeeDocument>()
                .Where(d => d.Status == "Active" &&
                            d.ExpiryDate != null &&
                            d.ExpiryDate.Value.Date < today &&
                            d.ExpiryDate.Value.Date >= today.AddDays(-1)) // Only today's newly expired
                .ToListAsync();

            foreach (var doc in expiredDocs)
            {
                try
                {
                    var notification = new ZR.Model.HR.HrNotification
                    {
                        EmployeeId = doc.EmployeeId,
                        RecipientId = doc.EmployeeId,
                        Subject = "Document Expired",
                        Body = $"Document '{doc.DocumentName}' (#{doc.DocumentNumber}) has expired on {doc.ExpiryDate:yyyy-MM-dd}. Immediate renewal required.",
                        NotificationType = "in_app",
                        Category = "document",
                        ReferenceType = "document_expired",
                        Priority = "high",
                        IsRead = false,
                        Create_time = DateTime.Now,
                        Create_by = "System"
                    };
                    await _notificationService.SendNotificationAsync(notification);
                    alertsSent++;
                }
                catch { /* Log but continue */ }
            }

            results.AppendLine($"Document expiry alerts sent: {alertsSent}");
            results.AppendLine($"Expired documents flagged: {expiredDocs.Count}");

            return results.ToString().Trim();
        }
    }
}
