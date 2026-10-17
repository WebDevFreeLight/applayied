using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.EmailMarketing
{
    /// <summary>
    /// Email Template - قوالب البريد الإلكتروني
    /// Reusable email templates with multi-language and dynamic content support
    /// </summary>
    [SugarTable("mkt_email_template")]
    public class MktEmailTemplate : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }

        [SugarColumn(Length = 100)]
        public string TemplateCode { get; set; }

        [SugarColumn(Length = 200)]
        public string TemplateName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string TemplateNameAr { get; set; }

        /// <summary>
        /// Category: promotional, newsletter, transactional, event_invitation,
        ///           event_reminder, feedback, welcome, re_engagement, sponsor_report, custom
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Category { get; set; }

        [SugarColumn(Length = 300)]
        public string SubjectLine { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string SubjectLineAr { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string PreviewText { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string PreviewTextAr { get; set; }

        /// <summary>
        /// Full HTML body (English)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string HtmlBody { get; set; }

        /// <summary>
        /// Full HTML body (Arabic)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string HtmlBodyAr { get; set; }

        /// <summary>
        /// Plain text version
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string PlainTextBody { get; set; }

        /// <summary>
        /// JSON: available merge tags/variables
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string MergeTagsJson { get; set; }

        /// <summary>
        /// JSON: dynamic content blocks
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DynamicBlocksJson { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string FromName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string FromEmail { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Thumbnail preview URL
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// AI-generated template
        /// </summary>
        public bool IsAiGenerated { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int TimesUsed { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? AvgOpenRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? AvgClickRate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Email Campaign - حملات البريد الإلكتروني
    /// </summary>
    [SugarTable("mkt_email_campaign")]
    public class MktEmailCampaign : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmailCampaignId { get; set; }

        [SugarColumn(Length = 100)]
        public string CampaignCode { get; set; }

        [SugarColumn(Length = 300)]
        public string CampaignName { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string CampaignNameAr { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ParentCampaignId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? TemplateId { get; set; }

        /// <summary>
        /// Type: blast, automated, drip, trigger_based, ab_test
        /// </summary>
        [SugarColumn(Length = 30)]
        public string CampaignType { get; set; }

        [SugarColumn(Length = 300)]
        public string SubjectLine { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string SubjectLineAr { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string PreviewText { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string FromName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string FromEmail { get; set; }

        [SugarColumn(ColumnDataType = "TEXT")]
        public string HtmlContent { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string HtmlContentAr { get; set; }

        /// <summary>
        /// JSON: target subscriber list IDs or segment IDs
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string RecipientListJson { get; set; }

        /// <summary>
        /// Status: draft, scheduled, sending, sent, paused, cancelled, failed
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";

        [SugarColumn(IsNullable = true)]
        public DateTime? ScheduledAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? SentAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? CompletedAt { get; set; }

        // Sending metrics
        public int TotalRecipients { get; set; } = 0;
        public int TotalSent { get; set; } = 0;
        public int TotalDelivered { get; set; } = 0;
        public int TotalOpened { get; set; } = 0;
        public int TotalClicked { get; set; } = 0;
        public int TotalBounced { get; set; } = 0;
        public int TotalUnsubscribed { get; set; } = 0;
        public int TotalComplained { get; set; } = 0;

        // Calculated rates
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? OpenRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ClickRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? BounceRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? UnsubscribeRate { get; set; }

        /// <summary>
        /// Revenue attributed to this email campaign
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? Revenue { get; set; }

        /// <summary>
        /// AI send time optimization enabled
        /// </summary>
        public bool AiSendTimeOptimization { get; set; } = false;

        /// <summary>
        /// AI subject line optimization enabled
        /// </summary>
        public bool AiSubjectOptimization { get; set; } = false;

        /// <summary>
        /// SDAIA compliance checked
        /// </summary>
        public bool ComplianceChecked { get; set; } = false;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }

        // Computed
        [SugarColumn(IsIgnore = true)]
        public decimal DeliveryRate => TotalSent > 0 ? ((decimal)TotalDelivered / TotalSent) * 100 : 0;
    }

    /// <summary>
    /// Email Subscriber - المشتركين في القائمة البريدية
    /// </summary>
    [SugarTable("mkt_subscriber")]
    public class MktSubscriber : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SubscriberId { get; set; }

        [SugarColumn(Length = 200)]
        public string Email { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string FirstName { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string LastName { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string FirstNameAr { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string LastNameAr { get; set; }

        [SugarColumn(Length = 50, IsNullable = true)]
        public string Phone { get; set; }

        [SugarColumn(Length = 10, IsNullable = true)]
        public string Language { get; set; } = "ar";

        /// <summary>
        /// Type: attendee, exhibitor, sponsor, investor, merchant, general
        /// </summary>
        [SugarColumn(Length = 30)]
        public string SubscriberType { get; set; } = "general";

        /// <summary>
        /// Linked member ID from system
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? MemberId { get; set; }

        /// <summary>
        /// Status: active, unsubscribed, bounced, complained, cleaned
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "active";

        /// <summary>
        /// Source: registration, import, website, event_checkin, api, manual
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Source { get; set; } = "manual";

        /// <summary>
        /// JSON: subscriber tags for segmentation
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TagsJson { get; set; }

        /// <summary>
        /// JSON: custom fields (industry, company, preferences)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string CustomFieldsJson { get; set; }

        /// <summary>
        /// JSON: subscription preferences (which types of emails to receive)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string PreferencesJson { get; set; }

        /// <summary>
        /// Engagement score (0-100) based on open/click history
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? EngagementScore { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LastOpenedAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LastClickedAt { get; set; }

        public int TotalEmailsReceived { get; set; } = 0;
        public int TotalOpened { get; set; } = 0;
        public int TotalClicked { get; set; } = 0;

        /// <summary>
        /// Double opt-in confirmed
        /// </summary>
        public bool IsConfirmed { get; set; } = false;

        [SugarColumn(IsNullable = true)]
        public DateTime? ConfirmedAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? UnsubscribedAt { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string UnsubscribeReason { get; set; }

        /// <summary>
        /// SDAIA consent obtained
        /// </summary>
        public bool DataConsentObtained { get; set; } = false;

        [SugarColumn(IsNullable = true)]
        public DateTime? ConsentObtainedAt { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string IpAddress { get; set; }

        [SugarColumn(Length = 50, IsNullable = true)]
        public string Country { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string City { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Subscriber List - قوائم المشتركين
    /// </summary>
    [SugarTable("mkt_subscriber_list")]
    public class MktSubscriberList : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ListId { get; set; }

        [SugarColumn(Length = 200)]
        public string ListName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string ListNameAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Type: static, dynamic (auto-updated based on criteria)
        /// </summary>
        [SugarColumn(Length = 20)]
        public string ListType { get; set; } = "static";

        /// <summary>
        /// JSON: criteria for dynamic lists
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string CriteriaJson { get; set; }

        public int MemberCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [SugarColumn(IsNullable = true)]
        public DateTime? LastRefreshedAt { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Subscriber List Member - أعضاء القائمة
    /// </summary>
    [SugarTable("mkt_subscriber_list_member")]
    public class MktSubscriberListMember
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ListId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long SubscriberId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;

        [SugarColumn(Length = 30)]
        public string AddedBy { get; set; } = "system";
    }
}
