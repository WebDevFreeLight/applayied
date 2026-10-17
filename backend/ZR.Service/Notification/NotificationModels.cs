namespace ZR.Service.Notification
{
    public class NotificationPayload
    {
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Message { get; set; }
        public string MessageAr { get; set; }
        public string NotificationType { get; set; } = "system";
        public string ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public string Severity { get; set; } = "info";
        public string Module { get; set; }
        public string ActionUrl { get; set; }
        public long? SenderUserId { get; set; }
        public string SenderName { get; set; }
    }

    public class NotificationRequest
    {
        public NotificationPayload Payload { get; set; }
        public List<long> UserIds { get; set; } = new();
        public List<long> EmployeeIds { get; set; } = new();
        public List<string> RoleKeys { get; set; } = new();
    }
}
