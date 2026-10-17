using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.Notification
{
    /// <summary>
    /// Flattened notification DTO for user-facing queries - عرض الإشعار للمستخدم
    /// </summary>
    public class UserNotificationDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RecipientId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long NotificationId { get; set; }

        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public string NotificationType { get; set; }
        public string Severity { get; set; }
        public string Module { get; set; }
        public string ReferenceType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReferenceId { get; set; }

        public string ActionUrl { get; set; }
        public string SenderName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
