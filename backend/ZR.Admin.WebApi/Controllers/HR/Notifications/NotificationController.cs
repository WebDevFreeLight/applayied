using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using SqlSugar;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IHrService;

namespace ZR.Admin.WebApi.Controllers.HR.Notifications
{
    /// <summary>
    /// Notification Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/notifications")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        #region Notification Management

        /// <summary>
        /// Send notification
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto dto)
        {
            var notification = dto.Adapt<HrNotification>();
            notification.Create_by = HttpContext.GetName();
            notification.Create_time = DateTime.Now;
            
            var result = await _notificationService.SendNotificationAsync(notification);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send bulk notifications
        /// </summary>
        [HttpPost("bulk")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Bulk Notifications", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendBulkNotifications([FromBody] List<NotificationDto> dtos)
        {
            var notifications = dtos.Adapt<List<HrNotification>>();
            foreach (var notification in notifications)
            {
                notification.Create_by = HttpContext.GetName();
                notification.Create_time = DateTime.Now;
            }
            
            var result = await _notificationService.SendBulkNotificationAsync(notifications);
            return SUCCESS(new { SentCount = result });
        }

        /// <summary>
        /// Get user notifications
        /// </summary>
        [HttpGet("user/{employeeId}")]
        [ActionPermissionFilter("hr:notifications:list")]
        public async Task<IActionResult> GetUserNotifications(long employeeId)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(employeeId);
            return SUCCESS(notifications);
        }

        /// <summary>
        /// Get unread notifications
        /// </summary>
        [HttpGet("user/{employeeId}/unread")]
        [ActionPermissionFilter("hr:notifications:list")]
        public async Task<IActionResult> GetUnreadNotifications(long employeeId)
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync(employeeId);
            return SUCCESS(notifications);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost("{notificationId}/read")]
        [ActionPermissionFilter("hr:notifications:read")]
        [Log(Title = "Mark Notification Read", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> MarkAsRead(long notificationId)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPost("user/{employeeId}/read-all")]
        [ActionPermissionFilter("hr:notifications:read")]
        [Log(Title = "Mark All Notifications Read", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> MarkAllAsRead(long employeeId)
        {
            var result = await _notificationService.MarkAllAsReadAsync(employeeId);
            return SUCCESS(new { MarkedCount = result });
        }

        #endregion

        #region Notification Templates

        /// <summary>
        /// Get notification template
        /// </summary>
        [HttpGet("templates/{templateCode}")]
        [ActionPermissionFilter("hr:notifications:templates")]
        public async Task<IActionResult> GetTemplate(string templateCode)
        {
            var template = await _notificationService.GetTemplateAsync(templateCode);
            return SUCCESS(template);
        }

        /// <summary>
        /// Get all templates
        /// </summary>
        [HttpGet("templates")]
        [ActionPermissionFilter("hr:notifications:templates")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _notificationService.GetAllTemplatesAsync();
            return SUCCESS(templates);
        }

        /// <summary>
        /// Create notification template
        /// </summary>
        [HttpPost("templates")]
        [ActionPermissionFilter("hr:notifications:templates:create")]
        [Log(Title = "Create Notification Template", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateNotificationTemplate([FromBody] NotificationTemplateDto dto)
        {
            var template = dto.Adapt<HrNotificationTemplate>();
            template.Create_by = HttpContext.GetName();
            template.Create_time = DateTime.Now;
            template.IsActive = true;
            
            var result = await _notificationService.CreateNotificationTemplateAsync(template);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update notification template
        /// </summary>
        [HttpPut("templates/{templateId}")]
        [ActionPermissionFilter("hr:notifications:templates:edit")]
        [Log(Title = "Update Notification Template", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateTemplate(long templateId, [FromBody] NotificationTemplateDto dto)
        {
            var template = dto.Adapt<HrNotificationTemplate>();
            template.TemplateId = templateId;
            template.Update_by = HttpContext.GetName();
            template.Update_time = DateTime.Now;
            
            var result = await _notificationService.UpdateTemplateAsync(template);
            return SUCCESS(result);
        }

        /// <summary>
        /// Create notification from template
        /// </summary>
        [HttpPost("templates/{templateCode}/send")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Template Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateNotificationFromTemplate(string templateCode, [FromBody] TemplateNotificationDto dto)
        {
            var result = await _notificationService.CreateNotificationFromTemplateAsync(
                templateCode, dto.RecipientId, dto.Parameters);
            return SUCCESS(result);
        }

        #endregion

        #region Specific Notification Types

        /// <summary>
        /// Send leave approval notification
        /// </summary>
        [HttpPost("leave-approval")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Leave Approval Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendLeaveApprovalNotification([FromBody] LeaveApprovalNotificationDto dto)
        {
            var result = await _notificationService.SendLeaveApprovalNotificationAsync(
                dto.EmployeeId, dto.LeaveId, dto.Status);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send payslip notification
        /// </summary>
        [HttpPost("payslip")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Payslip Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendPayslipNotification([FromBody] PayslipNotificationDto dto)
        {
            var result = await _notificationService.SendPayslipNotificationAsync(
                dto.EmployeeId, dto.PayrollId, dto.PayPeriod);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send performance review notification
        /// </summary>
        [HttpPost("performance-review")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Performance Review Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendPerformanceReviewNotification([FromBody] PerformanceReviewNotificationDto dto)
        {
            var result = await _notificationService.SendPerformanceReviewNotificationAsync(
                dto.EmployeeId, dto.ReviewId, dto.ReviewType);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send training notification
        /// </summary>
        [HttpPost("training")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Training Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendTrainingNotification([FromBody] TrainingNotificationDto dto)
        {
            var result = await _notificationService.SendTrainingNotificationAsync(
                dto.EmployeeId, dto.SessionId, dto.CourseName);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send document expiry notification
        /// </summary>
        [HttpPost("document-expiry")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Document Expiry Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendDocumentExpiryNotification([FromBody] DocumentExpiryNotificationDto dto)
        {
            var result = await _notificationService.SendDocumentExpiryNotificationAsync(
                dto.EmployeeId, dto.DocumentType, dto.ExpiryDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send birthday notification
        /// </summary>
        [HttpPost("birthday/{employeeId}")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Birthday Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendBirthdayNotification(long employeeId)
        {
            var result = await _notificationService.SendBirthdayNotificationAsync(employeeId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send anniversary notification
        /// </summary>
        [HttpPost("anniversary")]
        [ActionPermissionFilter("hr:notifications:send")]
        [Log(Title = "Send Anniversary Notification", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SendAnniversaryNotification([FromBody] AnniversaryNotificationDto dto)
        {
            var result = await _notificationService.SendAnniversaryNotificationAsync(dto.EmployeeId, dto.Years);
            return SUCCESS(result);
        }

        #endregion

        #region Notification Logs

        /// <summary>
        /// Get notification logs
        /// </summary>
        [HttpGet("logs")]
        [ActionPermissionFilter("hr:notifications:logs")]
        public async Task<IActionResult> GetNotificationLogs([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;
            
            var logs = await _notificationService.GetNotificationLogsAsync(start, end);
            return SUCCESS(logs);
        }

        #endregion

        #region Notification Statistics

        /// <summary>
        /// Get notification count by user
        /// </summary>
        [HttpGet("stats/user/{employeeId}")]
        [ActionPermissionFilter("hr:notifications:stats")]
        public async Task<IActionResult> GetNotificationStats(long employeeId)
        {
            var allNotifications = await _notificationService.GetUserNotificationsAsync(employeeId);
            var unreadNotifications = await _notificationService.GetUnreadNotificationsAsync(employeeId);
            
            var stats = new
            {
                TotalNotifications = allNotifications.Count,
                UnreadNotifications = unreadNotifications.Count,
                ReadNotifications = allNotifications.Count - unreadNotifications.Count,
                ReadPercentage = allNotifications.Count > 0 ? 
                    Math.Round((double)(allNotifications.Count - unreadNotifications.Count) / allNotifications.Count * 100, 2) : 0
            };
            
            return SUCCESS(stats);
        }

        /// <summary>
        /// Get notification summary
        /// </summary>
        [HttpGet("stats/summary")]
        [ActionPermissionFilter("hr:notifications:stats")]
        public async Task<IActionResult> GetNotificationSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;
            
            var logs = await _notificationService.GetNotificationLogsAsync(start, end);
            
            var summary = new
            {
                TotalSent = logs.Count,
                SuccessfulSent = logs.Count(l => l.IsSuccess),
                FailedSent = logs.Count(l => !l.IsSuccess),
                SuccessRate = logs.Count > 0 ? 
                    Math.Round((double)logs.Count(l => l.IsSuccess) / logs.Count * 100, 2) : 0,
                ChannelBreakdown = logs.GroupBy(l => l.Channel)
                    .Select(g => new { Channel = g.Key, Count = g.Count() })
                    .ToList()
            };
            
            return SUCCESS(summary);
        }

        #endregion

        #region Notification Management

        /// <summary>
        /// Delete old notifications
        /// </summary>
        [HttpDelete("cleanup")]
        [ActionPermissionFilter("hr:notifications:cleanup")]
        [Log(Title = "Cleanup Old Notifications", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> CleanupOldNotifications([FromQuery] int daysOld = 90)
        {
            // This would require implementing a cleanup method in the service
            // For now, return a placeholder response
            var cutoffDate = DateTime.Now.AddDays(-daysOld);
            
            return SUCCESS(new { 
                Message = $"Cleanup would remove notifications older than {daysOld} days (before {cutoffDate:yyyy-MM-dd})",
                CutoffDate = cutoffDate
            });
        }

        /// <summary>
        /// Get notification delivery status
        /// </summary>
        [HttpGet("{notificationId}/status")]
        [ActionPermissionFilter("hr:notifications:list")]
        public async Task<IActionResult> GetNotificationStatus(long notificationId)
        {
            // This would require querying the notification and its logs
            // For now, return a placeholder response based on the notification service structure
            return SUCCESS(new { 
                NotificationId = notificationId,
                Status = "SENT",
                DeliveryStatus = "DELIVERED",
                SentDate = DateTime.Now.AddMinutes(-30),
                ReadDate = (DateTime?)null
            });
        }

        /// <summary>
        /// Retry failed notification
        /// </summary>
        [HttpPost("{notificationId}/retry")]
        [ActionPermissionFilter("hr:notifications:retry")]
        [Log(Title = "Retry Failed Notification", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RetryFailedNotification(long notificationId)
        {
            // This would require implementing a retry mechanism in the service
            // For now, return a placeholder response
            return SUCCESS(new { 
                NotificationId = notificationId,
                RetryStatus = "QUEUED_FOR_RETRY",
                RetryTime = DateTime.Now
            });
        }

        #endregion

        #region Notification Preferences (Placeholder)

        /// <summary>
        /// Get user notification preferences
        /// </summary>
        [HttpGet("preferences/{employeeId}")]
        [ActionPermissionFilter("hr:notifications:preferences")]
        public async Task<IActionResult> GetNotificationPreferences(long employeeId)
        {
            // Placeholder for notification preferences
            var preferences = new
            {
                EmployeeId = employeeId,
                EmailNotifications = true,
                SmsNotifications = false,
                PushNotifications = true,
                InAppNotifications = true,
                Categories = new[]
                {
                    new { Category = "LEAVE", Enabled = true },
                    new { Category = "PAYROLL", Enabled = true },
                    new { Category = "TRAINING", Enabled = false },
                    new { Category = "PERFORMANCE", Enabled = true }
                }
            };
            
            return SUCCESS(preferences);
        }

        /// <summary>
        /// Update user notification preferences
        /// </summary>
        [HttpPut("preferences/{employeeId}")]
        [ActionPermissionFilter("hr:notifications:preferences")]
        [Log(Title = "Update Notification Preferences", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateNotificationPreferences(long employeeId, [FromBody] object preferences)
        {
            // Placeholder for updating notification preferences
            return SUCCESS(new { 
                EmployeeId = employeeId,
                Updated = true,
                UpdatedAt = DateTime.Now
            });
        }

        #endregion
    }
}
