using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR;

namespace ZR.Service.HR.IHrService
{
    /// <summary>
    /// Notification Service Interface
    /// </summary>
    public interface INotificationService
    {
        Task<int> SendNotificationAsync(HrNotification notification);
        Task<int> SendBulkNotificationAsync(List<HrNotification> notifications);
        Task<List<HrNotification>> GetUserNotificationsAsync(long employeeId);
        Task<List<HrNotification>> GetUnreadNotificationsAsync(long employeeId);
        Task<int> MarkAsReadAsync(long notificationId);
        Task<int> MarkAllAsReadAsync(long employeeId);
        Task<HrNotificationTemplate> GetTemplateAsync(string templateCode);
        Task<int> CreateNotificationFromTemplateAsync(string templateCode, long recipientId, Dictionary<string, string> parameters);
        Task<int> SendLeaveApprovalNotificationAsync(long employeeId, long leaveId, string status);
        Task<int> SendPayslipNotificationAsync(long employeeId, long payrollId, DateTime payPeriod);
        Task<int> SendPerformanceReviewNotificationAsync(long employeeId, long reviewId, string reviewType);
        Task<int> SendTrainingNotificationAsync(long employeeId, long sessionId, string courseName);
        Task<int> SendDocumentExpiryNotificationAsync(long employeeId, string documentType, DateTime expiryDate);
        Task<int> SendBirthdayNotificationAsync(long employeeId);
        Task<int> SendAnniversaryNotificationAsync(long employeeId, int years);
        Task<int> CreateNotificationTemplateAsync(HrNotificationTemplate template);
        Task<List<HrNotificationTemplate>> GetAllTemplatesAsync();
        Task<int> UpdateTemplateAsync(HrNotificationTemplate template);
        Task<List<HrNotificationLog>> GetNotificationLogsAsync(DateTime startDate, DateTime endDate);
    }
}
