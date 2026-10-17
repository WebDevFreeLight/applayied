using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Marketing.EmailMarketing;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Email Marketing Service Interface - خدمة التسويق عبر البريد الإلكتروني
    /// </summary>
    public interface IEmailMarketingService : IBaseService<MktEmailTemplate>
    {
        PagedInfo<EmailTemplateDto> GetTemplates(EmailTemplateQueryDto query);
        EmailTemplateDto GetTemplateDetail(long templateId);
        long AddTemplate(EmailTemplateDto dto);
        int UpdateTemplate(EmailTemplateDto dto);
        int DeleteTemplates(long[] ids);
        PagedInfo<EmailCampaignOutputDto> GetCampaigns(EmailCampaignQueryDto query);
        EmailCampaignOutputDto GetCampaignDetail(long campaignId);
        long CreateCampaign(EmailCampaignDto dto);
        int UpdateCampaign(EmailCampaignDto dto);
        int SendCampaign(long campaignId);
        int ScheduleCampaign(long campaignId, DateTime scheduledAt);
        int PauseCampaign(long campaignId);
        int DeleteCampaigns(long[] ids);
        PagedInfo<SubscriberOutputDto> GetSubscribers(SubscriberQueryDto query);
        SubscriberOutputDto GetSubscriberDetail(long subscriberId);
        long AddSubscriber(SubscriberDto dto);
        int UpdateSubscriber(SubscriberDto dto);
        int ImportSubscribers(ImportSubscribersDto dto);
        int Unsubscribe(long subscriberId, string reason);
        PagedInfo<SubscriberListDto> GetLists(SubscriberListQueryDto query);
        long AddList(SubscriberListDto dto);
        int UpdateList(SubscriberListDto dto);
        int DeleteLists(long[] ids);
        EmailAnalyticsDto GetEmailAnalytics(DateTime? dateFrom, DateTime? dateTo);
    }
}
