using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using ZR.Common;
using ZR.Model;
using ZR.Model.Marketing.Dto;
using ZR.Model.Marketing.EmailMarketing;
using ZR.Repository;
using ZR.Repository.Marketing.EmailMarketing;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    [AppService(ServiceType = typeof(IEmailMarketingService), ServiceLifetime = LifeTime.Transient)]
    public class EmailMarketingService : BaseService<MktEmailTemplate>, IEmailMarketingService
    {
        private readonly MktEmailCampaignRepository _campaignRepo;
        private readonly MktSubscriberRepository _subscriberRepo;
        private readonly MktSubscriberListRepository _listRepo;
        private readonly MktSubscriberListMemberRepository _listMemberRepo;

        public EmailMarketingService(
            MktEmailCampaignRepository campaignRepo,
            MktSubscriberRepository subscriberRepo,
            MktSubscriberListRepository listRepo,
            MktSubscriberListMemberRepository listMemberRepo)
        {
            _campaignRepo = campaignRepo;
            _subscriberRepo = subscriberRepo;
            _listRepo = listRepo;
            _listMemberRepo = listMemberRepo;
        }

        #region Templates

        public PagedInfo<EmailTemplateDto> GetTemplates(EmailTemplateQueryDto query)
        {
            var predicate = Expressionable.Create<MktEmailTemplate>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.TemplateName),
                it => it.TemplateName.Contains(query.TemplateName) || it.TemplateNameAr.Contains(query.TemplateName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Category), it => it.Category == query.Category);
            predicate = predicate.AndIF(query.IsActive.HasValue, it => it.IsActive == query.IsActive);
            predicate = predicate.AndIF(query.IsAiGenerated.HasValue, it => it.IsAiGenerated == query.IsAiGenerated);

            var page = Context.Queryable<MktEmailTemplate>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktEmailTemplate, EmailTemplateDto>(query);
            return page;
        }

        public EmailTemplateDto GetTemplateDetail(long templateId)
        {
            var entity = GetById(templateId);
            return entity?.Adapt<EmailTemplateDto>();
        }

        public long AddTemplate(EmailTemplateDto dto)
        {
            var entity = dto.Adapt<MktEmailTemplate>();
            return Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateTemplate(EmailTemplateDto dto)
        {
            var entity = dto.Adapt<MktEmailTemplate>();
            return Update(entity);
        }

        public int DeleteTemplates(long[] ids)
        {
            return Delete(ids);
        }

        #endregion

        #region Campaigns

        public PagedInfo<EmailCampaignOutputDto> GetCampaigns(EmailCampaignQueryDto query)
        {
            var predicate = Expressionable.Create<MktEmailCampaign>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CampaignName),
                it => it.CampaignName.Contains(query.CampaignName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CampaignType), it => it.CampaignType == query.CampaignType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.TemplateId.HasValue, it => it.TemplateId == query.TemplateId);

            var page = _campaignRepo.Context.Queryable<MktEmailCampaign>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktEmailCampaign, EmailCampaignOutputDto>(query);
            return page;
        }

        public EmailCampaignOutputDto GetCampaignDetail(long campaignId)
        {
            var entity = _campaignRepo.GetById(campaignId);
            return entity?.Adapt<EmailCampaignOutputDto>();
        }

        public long CreateCampaign(EmailCampaignDto dto)
        {
            var entity = dto.Adapt<MktEmailCampaign>();
            entity.Status = "draft";
            return _campaignRepo.Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateCampaign(EmailCampaignDto dto)
        {
            var entity = dto.Adapt<MktEmailCampaign>();
            return _campaignRepo.Update(entity);
        }

        public int SendCampaign(long campaignId)
        {
            return _campaignRepo.Context.Updateable<MktEmailCampaign>()
                .SetColumns(x => x.Status == "sending")
                .SetColumns(x => x.SentAt == DateTime.Now)
                .Where(x => x.EmailCampaignId == campaignId)
                .ExecuteCommand();
        }

        public int ScheduleCampaign(long campaignId, DateTime scheduledAt)
        {
            return _campaignRepo.Context.Updateable<MktEmailCampaign>()
                .SetColumns(x => x.Status == "scheduled")
                .SetColumns(x => x.ScheduledAt == scheduledAt)
                .Where(x => x.EmailCampaignId == campaignId)
                .ExecuteCommand();
        }

        public int PauseCampaign(long campaignId)
        {
            return _campaignRepo.Context.Updateable<MktEmailCampaign>()
                .SetColumns(x => x.Status == "paused")
                .Where(x => x.EmailCampaignId == campaignId)
                .ExecuteCommand();
        }

        public int DeleteCampaigns(long[] ids)
        {
            return _campaignRepo.Delete(ids);
        }

        #endregion

        #region Subscribers

        public PagedInfo<SubscriberOutputDto> GetSubscribers(SubscriberQueryDto query)
        {
            var predicate = Expressionable.Create<MktSubscriber>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Email), it => it.Email.Contains(query.Email));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Name),
                it => it.FirstName.Contains(query.Name) || it.LastName.Contains(query.Name) ||
                      it.FirstNameAr.Contains(query.Name) || it.LastNameAr.Contains(query.Name));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.SubscriberType), it => it.SubscriberType == query.SubscriberType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Source), it => it.Source == query.Source);
            predicate = predicate.AndIF(query.DataConsentObtained.HasValue, it => it.DataConsentObtained == query.DataConsentObtained);

            var page = _subscriberRepo.Context.Queryable<MktSubscriber>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktSubscriber, SubscriberOutputDto>(query);
            return page;
        }

        public SubscriberOutputDto GetSubscriberDetail(long subscriberId)
        {
            var entity = _subscriberRepo.GetById(subscriberId);
            return entity?.Adapt<SubscriberOutputDto>();
        }

        public long AddSubscriber(SubscriberDto dto)
        {
            var entity = dto.Adapt<MktSubscriber>();
            entity.Status = "active";
            return _subscriberRepo.Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateSubscriber(SubscriberDto dto)
        {
            var entity = dto.Adapt<MktSubscriber>();
            return _subscriberRepo.Update(entity);
        }

        public int ImportSubscribers(ImportSubscribersDto dto)
        {
            int count = 0;
            foreach (var item in dto.Subscribers)
            {
                var existing = _subscriberRepo.Context.Queryable<MktSubscriber>()
                    .First(x => x.Email == item.Email);

                if (existing == null)
                {
                    var entity = item.Adapt<MktSubscriber>();
                    entity.Status = "active";
                    entity.Source = dto.Source ?? "import";
                    _subscriberRepo.Insertable(entity).ExecuteReturnSnowflakeId();

                    if (dto.ListId.HasValue)
                    {
                        var listMember = new MktSubscriberListMember
                        {
                            ListId = dto.ListId.Value,
                            SubscriberId = entity.SubscriberId,
                            AddedBy = "import"
                        };
                        _listMemberRepo.Insert(listMember);
                    }
                    count++;
                }
            }
            return count;
        }

        public int Unsubscribe(long subscriberId, string reason)
        {
            return _subscriberRepo.Context.Updateable<MktSubscriber>()
                .SetColumns(x => x.Status == "unsubscribed")
                .SetColumns(x => x.UnsubscribedAt == DateTime.Now)
                .SetColumns(x => x.UnsubscribeReason == reason)
                .Where(x => x.SubscriberId == subscriberId)
                .ExecuteCommand();
        }

        #endregion

        #region Subscriber Lists

        public PagedInfo<SubscriberListDto> GetLists(SubscriberListQueryDto query)
        {
            var predicate = Expressionable.Create<MktSubscriberList>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ListName),
                it => it.ListName.Contains(query.ListName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ListType), it => it.ListType == query.ListType);
            predicate = predicate.AndIF(query.IsActive.HasValue, it => it.IsActive == query.IsActive);

            return _listRepo.Context.Queryable<MktSubscriberList>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktSubscriberList, SubscriberListDto>(query);
        }

        public long AddList(SubscriberListDto dto)
        {
            var entity = dto.Adapt<MktSubscriberList>();
            return _listRepo.Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateList(SubscriberListDto dto)
        {
            var entity = dto.Adapt<MktSubscriberList>();
            return _listRepo.Update(entity);
        }

        public int DeleteLists(long[] ids)
        {
            return _listRepo.Delete(ids);
        }

        #endregion

        #region Analytics

        public EmailAnalyticsDto GetEmailAnalytics(DateTime? dateFrom, DateTime? dateTo)
        {
            var from = dateFrom ?? DateTime.Now.AddMonths(-6);
            var to = dateTo ?? DateTime.Now;

            var totalSubscribers = _subscriberRepo.Count(x => true);
            var activeSubscribers = _subscriberRepo.Count(x => x.Status == "active");

            var campaigns = _campaignRepo.Context.Queryable<MktEmailCampaign>()
                .Where(x => x.Status == "sent" || x.Status == "completed")
                .Where(x => x.SentAt >= from && x.SentAt <= to)
                .ToList();

            var totalSent = campaigns.Sum(x => x.TotalSent);
            var totalOpened = campaigns.Sum(x => x.TotalOpened);
            var totalClicked = campaigns.Sum(x => x.TotalClicked);

            return new EmailAnalyticsDto
            {
                TotalSubscribers = totalSubscribers,
                ActiveSubscribers = activeSubscribers,
                AvgOpenRate = totalSent > 0 ? (decimal)totalOpened / totalSent * 100 : 0,
                AvgClickRate = totalSent > 0 ? (decimal)totalClicked / totalSent * 100 : 0,
                TotalCampaignsSent = campaigns.Count,
                TotalEmailsSent = totalSent,
                TotalRevenue = campaigns.Sum(x => x.Revenue ?? 0)
            };
        }

        #endregion
    }
}
