using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Email Template DTOs

    public class EmailTemplateQueryDto : PagerInfo
    {
        public string TemplateName { get; set; }
        public string Category { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsAiGenerated { get; set; }
    }

    public class EmailTemplateDto
    {
        public long TemplateId { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string TemplateNameAr { get; set; }
        public string Category { get; set; }
        public string SubjectLine { get; set; }
        public string SubjectLineAr { get; set; }
        public string PreviewText { get; set; }
        public string PreviewTextAr { get; set; }
        public string HtmlBody { get; set; }
        public string HtmlBodyAr { get; set; }
        public string PlainTextBody { get; set; }
        public string MergeTagsJson { get; set; }
        public string DynamicBlocksJson { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string ReplyToEmail { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsAiGenerated { get; set; }
        public bool IsActive { get; set; }
        public int TimesUsed { get; set; }
        public decimal? AvgOpenRate { get; set; }
        public decimal? AvgClickRate { get; set; }
    }

    #endregion

    #region Email Campaign DTOs

    public class EmailCampaignQueryDto : PagerInfo
    {
        public string CampaignName { get; set; }
        public string CampaignType { get; set; }
        public string Status { get; set; }
        public long? TemplateId { get; set; }
        public long? ParentCampaignId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class EmailCampaignDto
    {
        public long EmailCampaignId { get; set; }
        public string CampaignCode { get; set; }
        public string CampaignName { get; set; }
        public string CampaignNameAr { get; set; }
        public long? ParentCampaignId { get; set; }
        public long? TemplateId { get; set; }
        public string CampaignType { get; set; }
        public string SubjectLine { get; set; }
        public string SubjectLineAr { get; set; }
        public string PreviewText { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string HtmlContent { get; set; }
        public string HtmlContentAr { get; set; }
        public string RecipientListJson { get; set; }
        public string Status { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public bool AiSendTimeOptimization { get; set; }
        public bool AiSubjectOptimization { get; set; }
    }

    public class EmailCampaignOutputDto : EmailCampaignDto
    {
        public string TemplateName { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalRecipients { get; set; }
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalOpened { get; set; }
        public int TotalClicked { get; set; }
        public int TotalBounced { get; set; }
        public int TotalUnsubscribed { get; set; }
        public int TotalComplained { get; set; }
        public decimal? OpenRate { get; set; }
        public decimal? ClickRate { get; set; }
        public decimal? BounceRate { get; set; }
        public decimal? UnsubscribeRate { get; set; }
        public decimal DeliveryRate { get; set; }
        public decimal? Revenue { get; set; }
        public bool ComplianceChecked { get; set; }
    }

    #endregion

    #region Subscriber DTOs

    public class SubscriberQueryDto : PagerInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string SubscriberType { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public long? ListId { get; set; }
        public bool? DataConsentObtained { get; set; }
    }

    public class SubscriberDto
    {
        public long SubscriberId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
        public string Phone { get; set; }
        public string Language { get; set; }
        public string SubscriberType { get; set; }
        public long? MemberId { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public string TagsJson { get; set; }
        public string CustomFieldsJson { get; set; }
        public string PreferencesJson { get; set; }
        public bool DataConsentObtained { get; set; }
    }

    public class SubscriberOutputDto : SubscriberDto
    {
        public decimal? EngagementScore { get; set; }
        public DateTime? LastOpenedAt { get; set; }
        public DateTime? LastClickedAt { get; set; }
        public int TotalEmailsReceived { get; set; }
        public int TotalOpened { get; set; }
        public int TotalClicked { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
        public string UnsubscribeReason { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public List<string> ListNames { get; set; }
    }

    public class ImportSubscribersDto
    {
        public long? ListId { get; set; }
        public string Source { get; set; } = "import";
        public List<SubscriberImportItem> Subscribers { get; set; }
    }

    public class SubscriberImportItem
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Language { get; set; }
        public string SubscriberType { get; set; }
        public string TagsJson { get; set; }
    }

    #endregion

    #region Subscriber List DTOs

    public class SubscriberListQueryDto : PagerInfo
    {
        public string ListName { get; set; }
        public string ListType { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SubscriberListDto
    {
        public long ListId { get; set; }
        public string ListName { get; set; }
        public string ListNameAr { get; set; }
        public string Description { get; set; }
        public string ListType { get; set; }
        public string CriteriaJson { get; set; }
        public int MemberCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastRefreshedAt { get; set; }
    }

    #endregion

    #region Email Analytics DTOs

    public class EmailAnalyticsDto
    {
        public int TotalSubscribers { get; set; }
        public int ActiveSubscribers { get; set; }
        public decimal AvgOpenRate { get; set; }
        public decimal AvgClickRate { get; set; }
        public decimal AvgBounceRate { get; set; }
        public decimal AvgUnsubscribeRate { get; set; }
        public int TotalCampaignsSent { get; set; }
        public long TotalEmailsSent { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<EmailTrendPointDto> MonthlyTrend { get; set; }
        public List<EmailCampaignSummaryDto> TopCampaigns { get; set; }
        public List<SubscriberGrowthDto> SubscriberGrowth { get; set; }
    }

    public class EmailTrendPointDto
    {
        public DateTime Date { get; set; }
        public int Sent { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public decimal OpenRate { get; set; }
        public decimal ClickRate { get; set; }
    }

    public class EmailCampaignSummaryDto
    {
        public long EmailCampaignId { get; set; }
        public string CampaignName { get; set; }
        public decimal OpenRate { get; set; }
        public decimal ClickRate { get; set; }
        public int TotalSent { get; set; }
        public decimal? Revenue { get; set; }
    }

    public class SubscriberGrowthDto
    {
        public DateTime Date { get; set; }
        public int TotalSubscribers { get; set; }
        public int NewSubscribers { get; set; }
        public int Unsubscribed { get; set; }
        public int NetGrowth { get; set; }
    }

    #endregion
}
