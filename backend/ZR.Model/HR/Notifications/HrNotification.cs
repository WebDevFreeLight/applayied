 
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR
{
    /// <summary>
    /// Notification Table
    /// </summary>
    [SugarTable("hr_notification")]
    
    [SqlDict(
        dictType: "notification",
        dictName: "Hr Notification",
        valueColumn: "NotificationId",
        labelColumns: "notificationtype",
        RemarkColumn = "remark",
        OrderBy = "NotificationId ASC",
        Description = "Auto-generated from HrNotification table"
    )]
    public class HrNotification : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long NotificationId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long RecipientId { get; set; }

        [SugarColumn(Length = 20)]
        public string NotificationType { get; set; } // email, sms, push, in_app

        [SugarColumn(Length = 200)]
        public string Subject { get; set; }

        
        public string Body { get; set; }

        [SugarColumn(Length = 20)]
        public string Priority { get; set; } // high, medium, low

        [SugarColumn(Length = 50)]
        public string Category { get; set; } // leave, payroll, performance, training, document

        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }

        public bool IsSent { get; set; }

        public DateTime? SentDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Attachments { get; set; }

        [SugarColumn(Length = 100)]
        public string ReferenceType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReferenceId { get; set; }
 

        // Additional properties for compatibility
        [SugarColumn(IsIgnore = true)]
        public string Type { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string Title { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string Message { get; set; }

        [SugarColumn(Length = 50)]
        public string RecipientRole { get; set; }

        public bool ActionRequired { get; set; }

        [SugarColumn(Length = 500)]
        public string ActionUrl { get; set; }
        public string RecipientType { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
    }

    /// <summary>
    /// Notification Template Table
    /// </summary>
    [SugarTable("hr_notification_template")]
    public class HrNotificationTemplate : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }

        [SugarColumn(Length = 100)]
        public string TemplateCode { get; set; }

        [SugarColumn(Length = 200)]
        public string TemplateName { get; set; }

        [SugarColumn(Length = 200)]
        public string TemplateNameAr { get; set; }

        [SugarColumn(Length = 20)]
        public string NotificationType { get; set; }

        [SugarColumn(Length = 50)]
        public string Category { get; set; }

        [SugarColumn(Length = 200)]
        public string Subject { get; set; }

        
        public string Body { get; set; }

        
        public string BodyAr { get; set; }

        [SugarColumn(Length = 20)]
        public string Priority { get; set; }

        
        public string Parameters { get; set; } // JSON string of available parameters

        public bool IsActive { get; set; }

      
    }

    /// <summary>
    /// Notification Log Table
    /// </summary>
    [SugarTable("hr_notification_log")]
    public class HrNotificationLog : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LogId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long NotificationId { get; set; }

        [SugarColumn(Length = 20)]
        public string Channel { get; set; }

        public bool IsSuccess { get; set; }

        [SugarColumn(Length = 500)]
        public string ErrorMessage { get; set; }


    }

    /// <summary>
    /// Notification Subscription Table
    /// </summary>
    [SugarTable("hr_notification_subscription")]
    public class HrNotificationSubscription : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SubscriptionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 50)]
        public string Category { get; set; }

        [SugarColumn(Length = 20)]
        public string Channel { get; set; }

        public bool IsEnabled { get; set; }

        [SugarColumn(Length = 20)]
        public string Frequency { get; set; } // immediate, daily, weekly

        [SugarColumn(Length = 10)]
        public string PreferredTime { get; set; }


    }

    /// <summary>
    /// Email Queue Table
    /// </summary>
    [SugarTable("hr_email_queue")]
    public class HrEmailQueue : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long QueueId { get; set; }

        [SugarColumn(Length = 200)]
        public string ToEmail { get; set; }

        [SugarColumn(Length = 200)]
        public string CcEmail { get; set; }

        [SugarColumn(Length = 200)]
        public string BccEmail { get; set; }

        [SugarColumn(Length = 200)]
        public string Subject { get; set; }

        
        public string Body { get; set; }

        [SugarColumn(Length = 500)]
        public string Attachments { get; set; }

        [SugarColumn(Length = 20)]
        public string Priority { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // pending, sending, sent, failed

        public int RetryCount { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public DateTime? SentTime { get; set; }

        [SugarColumn(Length = 500)]
        public string ErrorMessage { get; set; }
         

    }

    /// <summary>
    /// SMS Queue Table
    /// </summary>
    [SugarTable("hr_sms_queue")]
    public class HrSmsQueue : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long QueueId { get; set; }

        [SugarColumn(Length = 20)]
        public string MobileNumber { get; set; }

        [SugarColumn(Length = 500)]
        public string Message { get; set; }

        [SugarColumn(Length = 20)]
        public string Priority { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; }

        public int RetryCount { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public DateTime? SentTime { get; set; }

        [SugarColumn(Length = 500)]
        public string ErrorMessage { get; set; }

        [SugarColumn(Length = 100)]
        public string MessageId { get; set; }
 
    }

    /// <summary>
    /// Push Notification Token Table
    /// </summary>
    [SugarTable("hr_push_token")]
    public class HrPushToken : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TokenId { get; set; }
            
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 500)]
        public string DeviceToken { get; set; }

        [SugarColumn(Length = 20)]
        public string Platform { get; set; } // ios, android

        [SugarColumn(Length = 100)]
        public string DeviceId { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastUsedDate { get; set; }
         
    }
}

