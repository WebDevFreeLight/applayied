namespace ZR.Service.Notification
{
    public interface INotificationDispatcher
    {
        Task SendAsync(NotificationRequest request);
        Task SendToUsersAsync(NotificationPayload payload, List<long> userIds);
        Task SendToRoleAsync(NotificationPayload payload, string roleKey);
        Task SendToRolesAsync(NotificationPayload payload, params string[] roleKeys);
    }
}
