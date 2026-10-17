using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ZR.Model.HR;
using ZR.Repository;
using ZR.Service.HR.IHrService;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// Notification Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(INotificationService), ServiceLifetime = LifeTime.Transient)]
    public class NotificationService : BaseService<HrNotification>, INotificationService
    {
        public NotificationService()
        {
        }

        public async Task<int> SendNotificationAsync(HrNotification notification)
        {
            notification.Create_time = DateTime.Now;
            notification.IsSent = false;
            
            var result = await InsertAsync(notification);
            if (result>0)
            {
                // Process notification based on type
                switch (notification.NotificationType)
                {
                    case "email":
                        await SendEmailNotification(notification);
                        break;
                    case "sms":
                        await SendSmsNotification(notification);
                        break;
                    case "push":
                        await SendPushNotification(notification);
                        break;
                    case "in_app":
                        // In-app notifications are already saved to database
                        notification.IsSent = true;
                        notification.SentDate = DateTime.Now;
                        await UpdateAsync(notification);
                        break;
                }
                return 1;
            }
            return 0;
        }

        public async Task<int> SendBulkNotificationAsync(List<HrNotification> notifications)
        {
            var successCount = 0;
            foreach (var notification in notifications)
            {
                var result = await SendNotificationAsync(notification);
                if (result > 0) successCount++;
            }
            return successCount;
        }

        public async Task<List<HrNotification>> GetUserNotificationsAsync(long employeeId)
        {
            return await Context.Queryable<HrNotification>()
                .Where(n => n.RecipientId == employeeId)
                .OrderBy(n => n.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<List<HrNotification>> GetUnreadNotificationsAsync(long employeeId)
        {
            return await Context.Queryable<HrNotification>()
                .Where(n => n.RecipientId == employeeId && !n.IsRead)
                .OrderBy(n => n.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<int> MarkAsReadAsync(long notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                var result = await UpdateAsync(notification);
                return result ? 1 : 0;
            }
            return 0;
        }

        public async Task<int> MarkAllAsReadAsync(long employeeId)
        {
            var notifications = await GetUnreadNotificationsAsync(employeeId);
            var count = 0;
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                var result = await UpdateAsync(notification);
                if (result) count++;
            }
            return count;
        }

        public async Task<HrNotificationTemplate> GetTemplateAsync(string templateCode)
        {
            return await Context.Queryable<HrNotificationTemplate>()
                .Where(t => t.TemplateCode == templateCode && t.IsActive)
                .FirstAsync();
        }

        public async Task<int> CreateNotificationFromTemplateAsync(string templateCode, long recipientId, Dictionary<string, string> parameters)
        {
            var template = await GetTemplateAsync(templateCode);
            if (template == null) return 0;

            var notification = new HrNotification
            {
                RecipientId = recipientId,
                NotificationType = template.NotificationType,
                Subject = ReplaceTemplateParameters(template.Subject, parameters),
                Body = ReplaceTemplateParameters(template.Body, parameters),
                Priority = template.Priority,
                Category = template.Category,
                Create_by = "System",
                Create_time = DateTime.Now
            };

            return await SendNotificationAsync(notification);
        }

        public async Task<int> SendLeaveApprovalNotificationAsync(long employeeId, long leaveId, string status)
        {
            // TODO: Replace with IEmployeeApiMappingService after refactoring
            var parameters = new Dictionary<string, string>
            {
                { "EmployeeName", "Employee" }, // (await _employeeService.GetByIdAsync(employeeId))?.FirstName
                { "LeaveId", leaveId.ToString() },
                { "Status", status },
                { "Date", DateTime.Now.ToString("yyyy-MM-dd") }
            };

            return await CreateNotificationFromTemplateAsync($"LEAVE_{status.ToUpper()}", employeeId, parameters);
        }

        public async Task<int> SendPayslipNotificationAsync(long employeeId, long payrollId, DateTime payPeriod)
        {
            // TODO: Replace with IEmployeeApiMappingService after refactoring
            var parameters = new Dictionary<string, string>
            {
                { "EmployeeName", "Employee" }, // (await _employeeService.GetByIdAsync(employeeId))?.FirstName
                { "PayPeriod", payPeriod.ToString("MMMM yyyy") },
                { "PayrollId", payrollId.ToString() }
            };

            return await CreateNotificationFromTemplateAsync("PAYSLIP_READY", employeeId, parameters);
        }

        public async Task<int> SendPerformanceReviewNotificationAsync(long employeeId, long reviewId, string reviewType)
        {
            // TODO: Replace with IEmployeeApiMappingService after refactoring
            var parameters = new Dictionary<string, string>
            {
                { "EmployeeName", "Employee" }, // (await _employeeService.GetByIdAsync(employeeId))?.FirstName
                { "ReviewType", reviewType },
                { "ReviewId", reviewId.ToString() },
                { "DueDate", DateTime.Now.AddDays(7).ToString("yyyy-MM-dd") }
            };

            return await CreateNotificationFromTemplateAsync("PERFORMANCE_REVIEW", employeeId, parameters);
        }

        public async Task<int> SendTrainingNotificationAsync(long employeeId, long sessionId, string courseName)
        {
            // TODO: Replace with IEmployeeApiMappingService after refactoring
            var parameters = new Dictionary<string, string>
            {
                { "EmployeeName", "Employee" }, // (await _employeeService.GetByIdAsync(employeeId))?.FirstName
                { "CourseName", courseName },
                { "SessionId", sessionId.ToString() }
            };

            return await CreateNotificationFromTemplateAsync("TRAINING_ENROLLMENT", employeeId, parameters);
        }

        public async Task<int> SendDocumentExpiryNotificationAsync(long employeeId, string documentType, DateTime expiryDate)
        {
            // TODO: Replace with IEmployeeApiMappingService after refactoring
            var parameters = new Dictionary<string, string>
            {
                { "EmployeeName", "Employee" }, // (await _employeeService.GetByIdAsync(employeeId))?.FirstName
                { "DocumentType", documentType },
                { "ExpiryDate", expiryDate.ToString("yyyy-MM-dd") },
                { "DaysRemaining", (expiryDate - DateTime.Now).Days.ToString() }
            };

            return await CreateNotificationFromTemplateAsync("DOCUMENT_EXPIRY", employeeId, parameters);
        }

        public async Task<int> SendBirthdayNotificationAsync(long employeeId)
        {
            // TODO: Replace with IEmployeeApiMappingService after refactoring
            // var employee = await _employeeService.GetByIdAsync(employeeId);
            var parameters = new Dictionary<string, string>
            {
                { "EmployeeName", "Employee" } // employee?.FirstName
            };

            return await CreateNotificationFromTemplateAsync("BIRTHDAY_WISH", employeeId, parameters);
        }

        public async Task<int> SendAnniversaryNotificationAsync(long employeeId, int years)
        {
            // TODO: Replace with IEmployeeApiMappingService after refactoring
            // var employee = await _employeeService.GetByIdAsync(employeeId);
            var parameters = new Dictionary<string, string>
            {
                { "EmployeeName", "Employee" }, // employee?.FirstName
                { "Years", years.ToString() }
            };

            return await CreateNotificationFromTemplateAsync("WORK_ANNIVERSARY", employeeId, parameters);
        }

        public async Task<int> CreateNotificationTemplateAsync(HrNotificationTemplate template)
        {
            template.Create_time = DateTime.Now;
            var result = await Context.Insertable(template).ExecuteCommandAsync();
            return result;
        }

        public async Task<List<HrNotificationTemplate>> GetAllTemplatesAsync()
        {
            return await Context.Queryable<HrNotificationTemplate>()
                .OrderBy(t => t.TemplateCode)
                .ToListAsync();
        }

        public async Task<int> UpdateTemplateAsync(HrNotificationTemplate template)
        {
            template.Update_time = DateTime.Now;
            var result = await Context.Updateable(template).ExecuteCommandAsync();
            return result;
        }

        public async Task<List<HrNotificationLog>> GetNotificationLogsAsync(DateTime startDate, DateTime endDate)
        {
            return await Context.Queryable<HrNotificationLog>()
                .Where(l => l.Create_time >= startDate && l.Create_time <= endDate)
                .OrderBy(l => l.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        // Private helper methods
        private string ReplaceTemplateParameters(string template, Dictionary<string, string> parameters)
        {
            foreach (var param in parameters)
            {
                template = template.Replace($"{{{param.Key}}}", param.Value);
            }
            return template;
        }

        private async Task SendEmailNotification(HrNotification notification)
        {
            try
            {
                // TODO: Replace with IEmployeeApiMappingService after refactoring
                // var employee = await _employeeService.GetByIdAsync(notification.RecipientId);
                // if (string.IsNullOrEmpty(employee?.Email)) return;
                return; // Temporarily disabled until employee service refactored

                // Email configuration should be in appsettings
                // using (var client = new SmtpClient())
                // {
                //     // Configure SMTP settings from configuration
                //     var message = new MailMessage
                //     {
                //         Subject = notification.Subject,
                //         Body = notification.Body,
                //         IsBodyHtml = true
                //     };
                //     message.To.Add(employee.Email);
                //
                //     // await client.SendMailAsync(message);
                //
                //     notification.IsSent = true;
                //     notification.SentDate = DateTime.Now;
                //     await UpdateAsync(notification);
                //
                //     // Log success
                //     await LogNotification(notification.NotificationId, "email", true, null);
                // }
            }
            catch (Exception ex)
            {
                // Log failure
                await LogNotification(notification.NotificationId, "email", false, ex.Message);
            }
        }

        private async Task SendSmsNotification(HrNotification notification)
        {
            try
            {
                // TODO: Replace with IEmployeeApiMappingService after refactoring
                // var employee = await _employeeService.GetByIdAsync(notification.RecipientId);
                // if (string.IsNullOrEmpty(employee?.MobileNumber)) return;
                return; // Temporarily disabled until employee service refactored

                // Integrate with SMS gateway
                // Implementation depends on SMS provider
                //
                // notification.IsSent = true;
                // notification.SentDate = DateTime.Now;
                // await UpdateAsync(notification);
                //
                // await LogNotification(notification.NotificationId, "sms", true, null);
            }
            catch (Exception ex)
            {
                await LogNotification(notification.NotificationId, "sms", false, ex.Message);
            }
        }

        private async Task SendPushNotification(HrNotification notification)
        {
            try
            {
                // Integrate with push notification service (FCM, APNS)
                // Implementation depends on push notification provider

                notification.IsSent = true;
                notification.SentDate = DateTime.Now;
                await UpdateAsync(notification);
                
                await LogNotification(notification.NotificationId, "push", true, null);
            }
            catch (Exception ex)
            {
                await LogNotification(notification.NotificationId, "push", false, ex.Message);
            }
        }

        private async Task LogNotification(long notificationId, string channel, bool success, string error)
        {
            var log = new HrNotificationLog
            {
                NotificationId = notificationId,
                Channel = channel,
                IsSuccess = success,
                ErrorMessage = error,
                Create_time = DateTime.Now
            };
            
            await Context.Insertable(log).ExecuteCommandAsync();
        }
    }
}
