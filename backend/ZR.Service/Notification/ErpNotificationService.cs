using Infrastructure.Attribute;
using ZR.Model.Notification;
using ZR.Service.Notification.IService;

namespace ZR.Service.Notification
{
    [AppService(ServiceType = typeof(IErpNotificationService), ServiceLifetime = LifeTime.Transient)]
    public class ErpNotificationService : BaseService<ErpNotification>, IErpNotificationService
    {
        public ErpNotification CreateNotification(ErpNotification model)
        {
            model.NotificationId = SnowFlakeSingle.Instance.NextId();
            model.IsDeleted = false;
            Insert(model);
            return model;
        }

        public async Task<int> BulkInsertRecipientsAsync(List<ErpNotificationRecipient> recipients)
        {
            return await Context.Insertable(recipients).ExecuteCommandAsync();
        }

        public List<UserNotificationDto> GetUserNotifications(long userId, bool unreadOnly = false)
        {
            return Context.Queryable<ErpNotificationRecipient>()
                .LeftJoin<ErpNotification>((r, n) => r.NotificationId == n.NotificationId)
                .Where((r, n) => r.UserId == userId && r.IsDeleted == false)
                .WhereIF(unreadOnly, (r, n) => r.IsRead == false)
                .OrderByDescending((r, n) => r.CreateTime)
                .Select((r, n) => new UserNotificationDto
                {
                    RecipientId = r.RecipientId,
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    TitleAr = n.TitleAr,
                    Message = n.Message,
                    MessageAr = n.MessageAr,
                    NotificationType = n.NotificationType,
                    Severity = n.Severity,
                    Module = n.Module,
                    ReferenceType = n.ReferenceType,
                    ReferenceId = n.ReferenceId,
                    ActionUrl = n.ActionUrl,
                    SenderName = n.SenderName,
                    IsRead = r.IsRead,
                    ReadAt = r.ReadAt,
                    CreateTime = r.CreateTime,
                })
                .ToList();
        }

        public int GetUnreadCount(long userId)
        {
            return Context.Queryable<ErpNotificationRecipient>()
                .Count(r => r.UserId == userId && !r.IsRead && !r.IsDeleted);
        }

        public int MarkAsRead(long recipientId, long userId)
        {
            return Context.Updateable<ErpNotificationRecipient>()
                .SetColumns(it => new ErpNotificationRecipient { IsRead = true, ReadAt = DateTime.Now })
                .Where(it => it.RecipientId == recipientId && it.UserId == userId)
                .ExecuteCommand();
        }

        public int MarkAllAsRead(long userId)
        {
            return Context.Updateable<ErpNotificationRecipient>()
                .SetColumns(it => new ErpNotificationRecipient { IsRead = true, ReadAt = DateTime.Now })
                .Where(it => it.UserId == userId && it.IsRead == false && it.IsDeleted == false)
                .ExecuteCommand();
        }

        public int SoftDeleteRecipient(long recipientId, long userId)
        {
            return Context.Updateable<ErpNotificationRecipient>()
                .SetColumns(it => new ErpNotificationRecipient { IsDeleted = true, DeletedAt = DateTime.Now })
                .Where(it => it.RecipientId == recipientId && it.UserId == userId)
                .ExecuteCommand();
        }

        public ErpNotification GetInfo(long notificationId)
        {
            return GetFirst(x => x.NotificationId == notificationId);
        }
    }
}
