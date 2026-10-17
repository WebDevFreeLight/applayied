using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using ZR.Common;
using ZR.Model;
using ZR.Model.Marketing.Dto;
using ZR.Model.Marketing.PublicRelations;
using ZR.Repository;
using ZR.Repository.Marketing.PublicRelations;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    [AppService(ServiceType = typeof(IPublicRelationsService), ServiceLifetime = LifeTime.Transient)]
    public class PublicRelationsService : BaseService<MktPressRelease>, IPublicRelationsService
    {
        private readonly MktMediaContactRepository _contactRepo;
        private readonly MktMediaMentionRepository _mentionRepo;
        private readonly MktPrSentimentRepository _sentimentRepo;

        public PublicRelationsService(
            MktMediaContactRepository contactRepo,
            MktMediaMentionRepository mentionRepo,
            MktPrSentimentRepository sentimentRepo)
        {
            _contactRepo = contactRepo;
            _mentionRepo = mentionRepo;
            _sentimentRepo = sentimentRepo;
        }

        #region Press Releases

        public PagedInfo<PressReleaseOutputDto> GetPressReleases(PressReleaseQueryDto query)
        {
            var predicate = Expressionable.Create<MktPressRelease>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Title),
                it => it.Title.Contains(query.Title) || it.TitleAr.Contains(query.Title));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Category), it => it.Category == query.Category);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.EventId.HasValue, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(query.IsAiGenerated.HasValue, it => it.IsAiGenerated == query.IsAiGenerated);
            predicate = predicate.AndIF(query.DateFrom.HasValue, it => it.PublishDate >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo.HasValue, it => it.PublishDate <= query.DateTo);

            return Context.Queryable<MktPressRelease>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktPressRelease, PressReleaseOutputDto>(query);
        }

        public PressReleaseOutputDto GetPressReleaseDetail(long releaseId)
        {
            var entity = GetById(releaseId);
            return entity?.Adapt<PressReleaseOutputDto>();
        }

        public long AddPressRelease(PressReleaseDto dto)
        {
            var entity = dto.Adapt<MktPressRelease>();
            entity.Status = "draft";
            return Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdatePressRelease(PressReleaseDto dto)
        {
            var entity = dto.Adapt<MktPressRelease>();
            return Update(entity);
        }

        public int DeletePressReleases(long[] ids)
        {
            return Delete(ids);
        }

        public int DistributePressRelease(long releaseId)
        {
            return Context.Updateable<MktPressRelease>()
                .SetColumns(x => x.Status == "distributed")
                .SetColumns(x => x.DistributedAt == DateTime.Now)
                .Where(x => x.ReleaseId == releaseId)
                .ExecuteCommand();
        }

        public int ApprovePressRelease(long releaseId)
        {
            return Context.Updateable<MktPressRelease>()
                .SetColumns(x => x.Status == "approved")
                .SetColumns(x => x.ApprovedAt == DateTime.Now)
                .Where(x => x.ReleaseId == releaseId)
                .ExecuteCommand();
        }

        #endregion

        #region Media Contacts

        public PagedInfo<MediaContactDto> GetMediaContacts(MediaContactQueryDto query)
        {
            var predicate = Expressionable.Create<MktMediaContact>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.FullName),
                it => it.FullName.Contains(query.FullName) || it.FullNameAr.Contains(query.FullName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.MediaOutlet),
                it => it.MediaOutlet.Contains(query.MediaOutlet) || it.MediaOutletAr.Contains(query.MediaOutlet));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ContactType), it => it.ContactType == query.ContactType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Specialty), it => it.Specialty == query.Specialty);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Reach), it => it.Reach == query.Reach);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.RelationshipLevel), it => it.RelationshipLevel == query.RelationshipLevel);
            predicate = predicate.AndIF(query.IsActive.HasValue, it => it.IsActive == query.IsActive);

            return _contactRepo.Context.Queryable<MktMediaContact>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktMediaContact, MediaContactDto>(query);
        }

        public MediaContactDto GetMediaContactDetail(long contactId)
        {
            var entity = _contactRepo.GetById(contactId);
            return entity?.Adapt<MediaContactDto>();
        }

        public long AddMediaContact(MediaContactDto dto)
        {
            var entity = dto.Adapt<MktMediaContact>();
            return _contactRepo.Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateMediaContact(MediaContactDto dto)
        {
            var entity = dto.Adapt<MktMediaContact>();
            return _contactRepo.Update(entity);
        }

        public int DeleteMediaContacts(long[] ids)
        {
            return _contactRepo.Delete(ids);
        }

        #endregion

        #region Media Mentions

        public PagedInfo<MediaMentionDto> GetMentions(MediaMentionQueryDto query)
        {
            var predicate = Expressionable.Create<MktMediaMention>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Title),
                it => it.Title.Contains(query.Title) || it.TitleAr.Contains(query.Title));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.MediaOutlet), it => it.MediaOutlet.Contains(query.MediaOutlet));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.MentionType), it => it.MentionType == query.MentionType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Sentiment), it => it.Sentiment == query.Sentiment);
            predicate = predicate.AndIF(query.EventId.HasValue, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(query.PressReleaseId.HasValue, it => it.PressReleaseId == query.PressReleaseId);
            predicate = predicate.AndIF(query.IsFeatured.HasValue, it => it.IsFeatured == query.IsFeatured);
            predicate = predicate.AndIF(query.DateFrom.HasValue, it => it.PublishedAt >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo.HasValue, it => it.PublishedAt <= query.DateTo);

            return _mentionRepo.Context.Queryable<MktMediaMention>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.PublishedAt)
                .ToPage<MktMediaMention, MediaMentionDto>(query);
        }

        public MediaMentionDto GetMentionDetail(long mentionId)
        {
            var entity = _mentionRepo.GetById(mentionId);
            return entity?.Adapt<MediaMentionDto>();
        }

        public long AddMention(MediaMentionDto dto)
        {
            var entity = dto.Adapt<MktMediaMention>();
            return _mentionRepo.Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateMention(MediaMentionDto dto)
        {
            var entity = dto.Adapt<MktMediaMention>();
            return _mentionRepo.Update(entity);
        }

        public int DeleteMentions(long[] ids)
        {
            return _mentionRepo.Delete(ids);
        }

        #endregion

        #region Sentiment & Analytics

        public List<PrSentimentDto> GetSentiment(PrSentimentQueryDto query)
        {
            var predicate = Expressionable.Create<MktPrSentiment>();
            predicate = predicate.AndIF(query.EventId.HasValue, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Period), it => it.Period == query.Period);
            predicate = predicate.AndIF(query.DateFrom.HasValue, it => it.PeriodStart >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo.HasValue, it => it.PeriodEnd <= query.DateTo);

            return _sentimentRepo.Context.Queryable<MktPrSentiment>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.PeriodStart)
                .ToList()
                .Select(x => x.Adapt<PrSentimentDto>())
                .ToList();
        }

        public PrAnalyticsDto GetPrAnalytics(DateTime? dateFrom, DateTime? dateTo)
        {
            var from = dateFrom ?? DateTime.Now.AddMonths(-6);
            var to = dateTo ?? DateTime.Now;

            var totalReleases = Count(x => true);
            var allMentions = _mentionRepo.Context.Queryable<MktMediaMention>()
                .Where(x => x.PublishedAt >= from && x.PublishedAt <= to)
                .ToList();

            var totalContacts = _contactRepo.Count(x => x.IsActive);
            var totalReach = allMentions.Sum(x => x.EstimatedReach);
            var totalPrValue = allMentions.Sum(x => x.EstimatedPrValue ?? 0);

            var positive = allMentions.Count(x => x.Sentiment == "positive");
            var neutral = allMentions.Count(x => x.Sentiment == "neutral");
            var negative = allMentions.Count(x => x.Sentiment == "negative");
            var total = allMentions.Count;

            return new PrAnalyticsDto
            {
                TotalPressReleases = totalReleases,
                TotalMediaMentions = total,
                TotalMediaReach = totalReach,
                TotalPrValue = totalPrValue,
                TotalMediaContacts = totalContacts,
                PositiveSentimentRate = total > 0 ? (decimal)positive / total * 100 : 0,
                NeutralSentimentRate = total > 0 ? (decimal)neutral / total * 100 : 0,
                NegativeSentimentRate = total > 0 ? (decimal)negative / total * 100 : 0,
                NetSentimentScore = total > 0 ? (decimal)(positive - negative) / total * 100 : 0,
                MonthlyTrend = allMentions
                    .GroupBy(x => new DateTime(x.PublishedAt.Year, x.PublishedAt.Month, 1))
                    .Select(g => new PrTrendDto
                    {
                        Date = g.Key,
                        Mentions = g.Count(),
                        Reach = g.Sum(x => x.EstimatedReach),
                        PrValue = g.Sum(x => x.EstimatedPrValue ?? 0)
                    })
                    .OrderBy(x => x.Date)
                    .ToList(),
                TopMediaOutlets = allMentions
                    .GroupBy(x => x.MediaOutlet)
                    .Select(g => new TopMediaOutletDto
                    {
                        MediaOutlet = g.Key,
                        Mentions = g.Count(),
                        TotalReach = g.Sum(x => x.EstimatedReach),
                        DominantSentiment = g.GroupBy(x => x.Sentiment)
                            .OrderByDescending(sg => sg.Count())
                            .First().Key
                    })
                    .OrderByDescending(x => x.Mentions)
                    .Take(10)
                    .ToList(),
                SentimentTrend = allMentions
                    .GroupBy(x => new DateTime(x.PublishedAt.Year, x.PublishedAt.Month, 1))
                    .Select(g => new SentimentDistributionDto
                    {
                        Date = g.Key,
                        Positive = g.Count(x => x.Sentiment == "positive"),
                        Neutral = g.Count(x => x.Sentiment == "neutral"),
                        Negative = g.Count(x => x.Sentiment == "negative"),
                        NetScore = g.Count() > 0
                            ? (decimal)(g.Count(x => x.Sentiment == "positive") - g.Count(x => x.Sentiment == "negative")) / g.Count() * 100
                            : 0
                    })
                    .OrderBy(x => x.Date)
                    .ToList()
            };
        }

        #endregion
    }
}
