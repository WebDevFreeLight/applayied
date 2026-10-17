using ZR.Model;
using ZR.Model.Notification;

namespace ZR.Service.Notification.IService
{
    public interface IErpNotificationService : IBaseService<ErpNotification>
    {
        ErpNotification CreateNotification(ErpNotification model);
        Task<int> BulkInsertRecipientsAsync(List<ErpNotificationRecipient> recipients);
        List<UserNotificationDto> GetUserNotifications(long userId, bool unreadOnly = false);
        int GetUnreadCount(long userId);
        int MarkAsRead(long recipientId, long userId);
        int MarkAllAsRead(long userId);
        int SoftDeleteRecipient(long recipientId, long userId);
        ErpNotification GetInfo(long notificationId);
    }
}
