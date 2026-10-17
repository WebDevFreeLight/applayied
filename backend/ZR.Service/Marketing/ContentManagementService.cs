using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using ZR.Common;
using ZR.Model;
using ZR.Model.Marketing.Dto;
using ZR.Model.Marketing.ContentMgmt;
using ZR.Repository;
using ZR.Repository.Marketing.ContentMgmt;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    [AppService(ServiceType = typeof(IContentManagementService), ServiceLifetime = LifeTime.Transient)]
    public class ContentManagementService : BaseService<MktContent>, IContentManagementService
    {
        private readonly MktAiContentGenerationRepository _aiGenRepo;

        public ContentManagementService(MktAiContentGenerationRepository aiGenRepo)
        {
            _aiGenRepo = aiGenRepo;
        }

        #region Content Library

        public PagedInfo<ContentOutputDto> GetContent(ContentQueryDto query)
        {
            var predicate = Expressionable.Create<MktContent>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Title),
                it => it.Title.Contains(query.Title) || it.TitleAr.Contains(query.Title));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ContentType), it => it.ContentType == query.ContentType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Category), it => it.Category == query.Category);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.CampaignId.HasValue, it => it.CampaignId == query.CampaignId);
            predicate = predicate.AndIF(query.EventId.HasValue, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(query.AuthorId.HasValue, it => it.AuthorId == query.AuthorId);
            predicate = predicate.AndIF(query.IsAiGenerated.HasValue, it => it.IsAiGenerated == query.IsAiGenerated);

            return Context.Queryable<MktContent>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktContent, ContentOutputDto>(query);
        }

        public ContentOutputDto GetContentDetail(long contentId)
        {
            var entity = GetById(contentId);
            if (entity == null) return null;

            // Increment view count
            Context.Updateable<MktContent>()
                .SetColumns(x => x.Views == x.Views + 1)
                .Where(x => x.ContentId == contentId)
                .ExecuteCommand();

            return entity.Adapt<ContentOutputDto>();
        }

        public long AddContent(ContentDto dto)
        {
            var entity = dto.Adapt<MktContent>();
            entity.Status = "draft";
            entity.VersionNumber = 1;
            return Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateContent(ContentDto dto)
        {
            var existing = GetById(dto.ContentId);
            if (existing == null) return 0;

            var entity = dto.Adapt<MktContent>();
            entity.VersionNumber = existing.VersionNumber + 1;
            return Update(entity);
        }

        public int DeleteContent(long[] ids)
        {
            return Delete(ids);
        }

        public int UpdateContentStatus(long contentId, string status)
        {
            return Context.Updateable<MktContent>()
                .SetColumns(x => x.Status == status)
                .Where(x => x.ContentId == contentId)
                .ExecuteCommand();
        }

        public int PublishContent(long contentId)
        {
            return Context.Updateable<MktContent>()
                .SetColumns(x => x.Status == "published")
                .SetColumns(x => x.PublishedAt == DateTime.Now)
                .Where(x => x.ContentId == contentId)
                .ExecuteCommand();
        }

        #endregion

        #region AI Content Generation

        public PagedInfo<AiContentGenerationDto> GetAiGenerations(AiContentGenerationQueryDto query)
        {
            var predicate = Expressionable.Create<MktAiContentGeneration>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.GenerationType), it => it.GenerationType == query.GenerationType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.AiProvider), it => it.AiProvider == query.AiProvider);
            predicate = predicate.AndIF(query.IsApproved.HasValue, it => it.IsApproved == query.IsApproved);

            return _aiGenRepo.Context.Queryable<MktAiContentGeneration>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.CreatedAt)
                .ToPage<MktAiContentGeneration, AiContentGenerationDto>(query);
        }

        public AiContentGenerationDto GenerateAiContent(GenerateContentRequest dto)
        {
            var entity = new MktAiContentGeneration
            {
                GenerationType = dto.GenerationType,
                Prompt = dto.Prompt,
                ContextJson = dto.ContextJson,
                AiProvider = "openai",
                AiModel = "gpt-4",
                Status = "generating",
                Output = $"[AI Generated Content for: {dto.Prompt}]",
                OutputAr = $"[محتوى مولد بالذكاء الاصطناعي: {dto.Prompt}]",
                CreatedAt = DateTime.Now
            };

            var id = _aiGenRepo.Insertable(entity).ExecuteReturnSnowflakeId();
            entity.GenerationId = id;

            // Simulate AI completion
            _aiGenRepo.Context.Updateable<MktAiContentGeneration>()
                .SetColumns(x => x.Status == "completed")
                .SetColumns(x => x.QualityScore == 92.5m)
                .SetColumns(x => x.CulturalScore == 95.0m)
                .Where(x => x.GenerationId == id)
                .ExecuteCommand();

            return entity.Adapt<AiContentGenerationDto>();
        }

        public int ApproveAiContent(ApproveContentRequest dto)
        {
            var result = _aiGenRepo.Context.Updateable<MktAiContentGeneration>()
                .SetColumns(x => x.IsApproved == dto.Approve)
                .SetColumns(x => x.Status == (dto.Approve ? "approved" : "rejected"))
                .Where(x => x.GenerationId == dto.GenerationId)
                .ExecuteCommand();

            if (dto.Approve && dto.SaveToLibrary)
            {
                var gen = _aiGenRepo.GetById(dto.GenerationId);
                if (gen != null)
                {
                    var content = new MktContent
                    {
                        Title = $"AI Generated: {gen.GenerationType}",
                        Body = gen.Output,
                        BodyAr = gen.OutputAr,
                        ContentType = gen.GenerationType,
                        Status = "draft",
                        IsAiGenerated = true,
                        AiQualityScore = gen.QualityScore,
                        VersionNumber = 1
                    };
                    var contentId = Insertable(content).ExecuteReturnSnowflakeId();

                    _aiGenRepo.Context.Updateable<MktAiContentGeneration>()
                        .SetColumns(x => x.LinkedContentId == contentId)
                        .Where(x => x.GenerationId == dto.GenerationId)
                        .ExecuteCommand();
                }
            }

            return result;
        }

        #endregion

        #region Analytics

        public ContentAnalyticsDto GetContentAnalytics(DateTime? dateFrom, DateTime? dateTo)
        {
            var totalContent = Count(x => true);
            var publishedContent = Count(x => x.Status == "published");
            var allContent = Context.Queryable<MktContent>().ToList();
            var totalViews = allContent.Sum(x => x.Views);
            var totalShares = allContent.Sum(x => x.Shares);

            return new ContentAnalyticsDto
            {
                TotalContent = totalContent,
                PublishedContent = publishedContent,
                TotalViews = totalViews,
                TotalShares = totalShares,
                AvgEngagementRate = allContent.Any() ? allContent.Average(x => x.EngagementRate ?? 0) : 0,
                TypeBreakdown = allContent.GroupBy(x => x.ContentType)
                    .Select(g => new ContentTypeBreakdownDto
                    {
                        ContentType = g.Key,
                        Count = g.Count(),
                        TotalViews = g.Sum(x => x.Views),
                        AvgEngagement = g.Average(x => x.EngagementRate ?? 0)
                    }).ToList()
            };
        }

        #endregion
    }
}
