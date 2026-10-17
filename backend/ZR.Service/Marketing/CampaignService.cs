using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model;
using ZR.Model.Marketing.Campaigns;
using ZR.Model.Marketing.Dto;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Campaign Service Implementation - خدمة الحملات التسويقية
    /// Full campaign lifecycle management with channel support, performance tracking, and A/B testing
    /// </summary>
    [AppService(ServiceType = typeof(ICampaignService), ServiceLifetime = LifeTime.Transient)]
    public class CampaignService : BaseService<MktCampaign>, ICampaignService
    {
        /// <summary>
        /// Get paginated campaign list with filtering
        /// </summary>
        public PagedInfo<CampaignOutputDto> GetCampaignList(CampaignQueryDto query)
        {
            var predicate = Expressionable.Create<MktCampaign>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CampaignCode), it => it.CampaignCode.Contains(query.CampaignCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CampaignName), it => it.CampaignName.Contains(query.CampaignName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CampaignType), it => it.CampaignType == query.CampaignType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Priority), it => it.Priority == query.Priority);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.TargetAudience), it => it.TargetAudience == query.TargetAudience);
            predicate = predicate.AndIF(query.StartDateFrom != null, it => it.StartDate >= query.StartDateFrom);
            predicate = predicate.AndIF(query.StartDateTo != null, it => it.StartDate <= query.StartDateTo);
            predicate = predicate.AndIF(query.EventId != null, it => it.EventId == query.EventId);
            predicate = predicate.AndIF(query.ManagerId != null, it => it.ManagerId == query.ManagerId);
            predicate = predicate.AndIF(query.AiOptimizationEnabled != null, it => it.AiOptimizationEnabled == query.AiOptimizationEnabled);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(c => c.Create_time)
                .Select(c => new CampaignOutputDto
                {
                    CampaignId = c.CampaignId,
                    CampaignCode = c.CampaignCode,
                    CampaignName = c.CampaignName,
                    CampaignNameAr = c.CampaignNameAr,
                    CampaignType = c.CampaignType,
                    Description = c.Description,
                    DescriptionAr = c.DescriptionAr,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    TotalBudget = c.TotalBudget,
                    Currency = c.Currency,
                    TargetAudience = c.TargetAudience,
                    TargetSegmentsJson = c.TargetSegmentsJson,
                    GoalsJson = c.GoalsJson,
                    Priority = c.Priority,
                    TagsJson = c.TagsJson,
                    AiOptimizationEnabled = c.AiOptimizationEnabled,
                    EventId = c.EventId,
                    ManagerId = c.ManagerId,
                    SpentAmount = c.SpentAmount,
                    AiConfidenceScore = c.AiConfidenceScore,
                    AiRecommendationsJson = c.AiRecommendationsJson
                })
                .ToPage(query);

            // Compute BudgetUtilization and RemainingDays in C# (not DB columns)
            foreach (var item in response.Result)
            {
                item.BudgetUtilization = item.TotalBudget > 0 ? (item.SpentAmount / item.TotalBudget) * 100 : 0;
                item.RemainingDays = item.EndDate > DateTime.Now ? (int)(item.EndDate - DateTime.Now).TotalDays : 0;
            }

            // Load channels and aggregated performance for each campaign
            foreach (var item in response.Result)
            {
                item.Channels = Context.Queryable<MktCampaignChannel>()
                    .Where(ch => ch.CampaignId == item.CampaignId)
                    .Select(ch => new CampaignChannelDto
                    {
                        ChannelId = ch.ChannelId,
                        CampaignId = ch.CampaignId,
                        ChannelType = ch.ChannelType,
                        ChannelBudget = ch.ChannelBudget,
                        SpentAmount = ch.SpentAmount,
                        Status = ch.Status,
                        ExternalCampaignId = ch.ExternalCampaignId,
                        ConfigJson = ch.ConfigJson,
                        Impressions = ch.Impressions,
                        Clicks = ch.Clicks,
                        Conversions = ch.Conversions,
                        CostPerClick = ch.CostPerClick,
                        CostPerConversion = ch.CostPerConversion,
                        ReturnOnAdSpend = ch.ReturnOnAdSpend
                    })
                    .ToList();

                // Aggregate performance
                var perfData = Context.Queryable<MktCampaignPerformance>()
                    .Where(p => p.CampaignId == item.CampaignId)
                    .ToList();

                item.TotalImpressions = perfData.Sum(p => p.Impressions);
                item.TotalClicks = perfData.Sum(p => p.Clicks);
                item.TotalConversions = perfData.Sum(p => p.Conversions);
                item.TotalRevenue = perfData.Sum(p => p.Revenue);
                var totalSpend = perfData.Sum(p => p.Spend);
                item.OverallRoi = totalSpend > 0 ? ((item.TotalRevenue - totalSpend) / totalSpend) * 100 : null;
            }

            return response;
        }

        /// <summary>
        /// Get campaign detail by ID with channels and performance
        /// </summary>
        public CampaignOutputDto GetCampaignDetail(long campaignId)
        {
            var campaign = Queryable()
                .Where(c => c.CampaignId == campaignId)
                .First();

            if (campaign == null) return null;

            var dto = new CampaignOutputDto
            {
                CampaignId = campaign.CampaignId,
                CampaignCode = campaign.CampaignCode,
                CampaignName = campaign.CampaignName,
                CampaignNameAr = campaign.CampaignNameAr,
                CampaignType = campaign.CampaignType,
                Description = campaign.Description,
                DescriptionAr = campaign.DescriptionAr,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Status = campaign.Status,
                TotalBudget = campaign.TotalBudget,
                Currency = campaign.Currency,
                TargetAudience = campaign.TargetAudience,
                TargetSegmentsJson = campaign.TargetSegmentsJson,
                GoalsJson = campaign.GoalsJson,
                Priority = campaign.Priority,
                TagsJson = campaign.TagsJson,
                AiOptimizationEnabled = campaign.AiOptimizationEnabled,
                EventId = campaign.EventId,
                ManagerId = campaign.ManagerId,
                SpentAmount = campaign.SpentAmount,
                BudgetUtilization = campaign.BudgetUtilization,
                RemainingDays = campaign.RemainingDays,
                AiConfidenceScore = campaign.AiConfidenceScore,
                AiRecommendationsJson = campaign.AiRecommendationsJson,
                ManagerName = campaign.ManagerName
            };

            // Load channels
            dto.Channels = Context.Queryable<MktCampaignChannel>()
                .Where(ch => ch.CampaignId == campaignId)
                .Select(ch => new CampaignChannelDto
                {
                    ChannelId = ch.ChannelId,
                    CampaignId = ch.CampaignId,
                    ChannelType = ch.ChannelType,
                    ChannelBudget = ch.ChannelBudget,
                    SpentAmount = ch.SpentAmount,
                    Status = ch.Status,
                    ExternalCampaignId = ch.ExternalCampaignId,
                    ConfigJson = ch.ConfigJson,
                    Impressions = ch.Impressions,
                    Clicks = ch.Clicks,
                    Conversions = ch.Conversions,
                    CostPerClick = ch.CostPerClick,
                    CostPerConversion = ch.CostPerConversion,
                    ReturnOnAdSpend = ch.ReturnOnAdSpend
                })
                .ToList();

            // Aggregate performance
            var perfData = Context.Queryable<MktCampaignPerformance>()
                .Where(p => p.CampaignId == campaignId)
                .ToList();

            dto.TotalImpressions = perfData.Sum(p => p.Impressions);
            dto.TotalClicks = perfData.Sum(p => p.Clicks);
            dto.TotalConversions = perfData.Sum(p => p.Conversions);
            dto.TotalRevenue = perfData.Sum(p => p.Revenue);
            var totalSpend = perfData.Sum(p => p.Spend);
            dto.OverallRoi = totalSpend > 0 ? ((dto.TotalRevenue - totalSpend) / totalSpend) * 100 : null;

            return dto;
        }

        /// <summary>
        /// Add a new campaign
        /// </summary>
        public MktCampaign AddCampaign(CampaignDto dto)
        {
            var campaign = new MktCampaign
            {
                CampaignCode = dto.CampaignCode,
                CampaignName = dto.CampaignName,
                CampaignNameAr = dto.CampaignNameAr,
                CampaignType = dto.CampaignType,
                Description = dto.Description,
                DescriptionAr = dto.DescriptionAr,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status ?? CampaignStatus.DRAFT,
                TotalBudget = dto.TotalBudget,
                Currency = dto.Currency ?? "SAR",
                TargetAudience = dto.TargetAudience ?? "all",
                TargetSegmentsJson = dto.TargetSegmentsJson,
                GoalsJson = dto.GoalsJson,
                Priority = dto.Priority ?? "medium",
                TagsJson = dto.TagsJson,
                AiOptimizationEnabled = dto.AiOptimizationEnabled,
                EventId = dto.EventId,
                ManagerId = dto.ManagerId
            };

            Insertable(campaign).ExecuteReturnSnowflakeId();

            // Add channels if provided
            if (dto.Channels != null && dto.Channels.Count > 0)
            {
                foreach (var channelDto in dto.Channels)
                {
                    var channel = new MktCampaignChannel
                    {
                        CampaignId = campaign.CampaignId,
                        ChannelType = channelDto.ChannelType,
                        ChannelBudget = channelDto.ChannelBudget,
                        Status = channelDto.Status ?? "active",
                        ExternalCampaignId = channelDto.ExternalCampaignId,
                        ConfigJson = channelDto.ConfigJson
                    };
                    Context.Insertable(channel).ExecuteReturnSnowflakeId();
                }
            }

            return campaign;
        }

        /// <summary>
        /// Update an existing campaign
        /// </summary>
        public int UpdateCampaign(CampaignDto dto)
        {
            var campaign = Queryable()
                .Where(c => c.CampaignId == dto.CampaignId)
                .First();

            if (campaign == null) return 0;

            campaign.CampaignCode = dto.CampaignCode;
            campaign.CampaignName = dto.CampaignName;
            campaign.CampaignNameAr = dto.CampaignNameAr;
            campaign.CampaignType = dto.CampaignType;
            campaign.Description = dto.Description;
            campaign.DescriptionAr = dto.DescriptionAr;
            campaign.StartDate = dto.StartDate;
            campaign.EndDate = dto.EndDate;
            campaign.Status = dto.Status;
            campaign.TotalBudget = dto.TotalBudget;
            campaign.Currency = dto.Currency;
            campaign.TargetAudience = dto.TargetAudience;
            campaign.TargetSegmentsJson = dto.TargetSegmentsJson;
            campaign.GoalsJson = dto.GoalsJson;
            campaign.Priority = dto.Priority;
            campaign.TagsJson = dto.TagsJson;
            campaign.AiOptimizationEnabled = dto.AiOptimizationEnabled;
            campaign.EventId = dto.EventId;
            campaign.ManagerId = dto.ManagerId;

            return Update(campaign, true, "Update campaign");
        }

        /// <summary>
        /// Delete campaigns by IDs
        /// </summary>
        public int DeleteCampaign(long[] campaignIds)
        {
            return Delete(campaignIds);
        }

        /// <summary>
        /// Update campaign status
        /// </summary>
        public int UpdateStatus(long campaignId, string status)
        {
            return Context.Updateable<MktCampaign>()
                .SetColumns(c => c.Status == status)
                .Where(c => c.CampaignId == campaignId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get campaign analytics summary
        /// </summary>
        public CampaignAnalyticsDto GetCampaignAnalytics(long? eventId = null)
        {
            var campaignQuery = Queryable();
            if (eventId.HasValue)
            {
                campaignQuery = campaignQuery.Where(c => c.EventId == eventId);
            }

            var campaigns = campaignQuery.ToList();
            var campaignIds = campaigns.Select(c => c.CampaignId).ToList();

            var channels = Context.Queryable<MktCampaignChannel>()
                .Where(ch => campaignIds.Contains(ch.CampaignId))
                .ToList();

            var performances = Context.Queryable<MktCampaignPerformance>()
                .Where(p => campaignIds.Contains(p.CampaignId))
                .ToList();

            var totalSpent = campaigns.Sum(c => c.SpentAmount);
            var totalRevenue = performances.Sum(p => p.Revenue);

            var analytics = new CampaignAnalyticsDto
            {
                TotalCampaigns = campaigns.Count,
                ActiveCampaigns = campaigns.Count(c => c.Status == CampaignStatus.ACTIVE),
                TotalBudget = campaigns.Sum(c => c.TotalBudget),
                TotalSpent = totalSpent,
                OverallRoi = totalSpent > 0 ? ((totalRevenue - totalSpent) / totalSpent) * 100 : 0,
                TotalImpressions = performances.Sum(p => p.Impressions),
                TotalClicks = performances.Sum(p => p.Clicks),
                TotalConversions = performances.Sum(p => p.Conversions),
                TotalRevenue = totalRevenue,
                AvgConversionRate = performances.Count > 0 && performances.Sum(p => p.Clicks) > 0
                    ? (decimal)performances.Sum(p => p.Conversions) / performances.Sum(p => p.Clicks) * 100 : 0,
                AvgCostPerClick = performances.Sum(p => p.Clicks) > 0
                    ? totalSpent / performances.Sum(p => p.Clicks) : 0,
                AvgCostPerConversion = performances.Sum(p => p.Conversions) > 0
                    ? totalSpent / performances.Sum(p => p.Conversions) : 0,

                ChannelBreakdown = channels
                    .GroupBy(ch => ch.ChannelType)
                    .Select(g => new ChannelPerformanceSummary
                    {
                        ChannelType = g.Key,
                        Budget = g.Sum(x => x.ChannelBudget),
                        Spent = g.Sum(x => x.SpentAmount),
                        Impressions = g.Sum(x => x.Impressions),
                        Clicks = g.Sum(x => x.Clicks),
                        Conversions = g.Sum(x => x.Conversions),
                        Roi = g.Sum(x => x.SpentAmount) > 0
                            ? g.Sum(x => x.ReturnOnAdSpend ?? 0) / g.Count()
                            : null
                    })
                    .ToList(),

                DailyTrend = performances
                    .GroupBy(p => p.RecordDate.Date)
                    .OrderBy(g => g.Key)
                    .TakeLast(30)
                    .Select(g => new CampaignTrendPoint
                    {
                        Date = g.Key,
                        Impressions = g.Sum(x => x.Impressions),
                        Clicks = g.Sum(x => x.Clicks),
                        Conversions = g.Sum(x => x.Conversions),
                        Spend = g.Sum(x => x.Spend),
                        Revenue = g.Sum(x => x.Revenue)
                    })
                    .ToList(),

                TopCampaigns = campaigns
                    .Select(c =>
                    {
                        var cPerf = performances.Where(p => p.CampaignId == c.CampaignId).ToList();
                        var cRevenue = cPerf.Sum(p => p.Revenue);
                        var cSpend = cPerf.Sum(p => p.Spend);
                        return new TopCampaignDto
                        {
                            CampaignId = c.CampaignId,
                            CampaignName = c.CampaignName,
                            Roi = cSpend > 0 ? ((cRevenue - cSpend) / cSpend) * 100 : 0,
                            Revenue = cRevenue,
                            Conversions = cPerf.Sum(p => p.Conversions)
                        };
                    })
                    .OrderByDescending(x => x.Roi)
                    .Take(10)
                    .ToList()
            };

            return analytics;
        }

        /// <summary>
        /// Get campaign performance data
        /// </summary>
        public List<CampaignPerformanceDto> GetCampaignPerformance(CampaignPerformanceQueryDto query)
        {
            var predicate = Expressionable.Create<MktCampaignPerformance>();

            predicate = predicate.And(p => p.CampaignId == query.CampaignId);
            predicate = predicate.AndIF(query.ChannelId != null, p => p.ChannelId == query.ChannelId);
            predicate = predicate.AndIF(query.DateFrom != null, p => p.RecordDate >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo != null, p => p.RecordDate <= query.DateTo);

            return Context.Queryable<MktCampaignPerformance>()
                .Where(predicate.ToExpression())
                .OrderBy(p => p.RecordDate)
                .Select(p => new CampaignPerformanceDto
                {
                    PerformanceId = p.PerformanceId,
                    CampaignId = p.CampaignId,
                    ChannelId = p.ChannelId,
                    RecordDate = p.RecordDate,
                    Impressions = p.Impressions,
                    Reach = p.Reach,
                    Clicks = p.Clicks,
                    Conversions = p.Conversions,
                    Engagements = p.Engagements,
                    Revenue = p.Revenue,
                    Spend = p.Spend,
                    ClickThroughRate = p.ClickThroughRate,
                    ConversionRate = p.ConversionRate,
                    CostPerClick = p.CostPerClick,
                    ReturnOnInvestment = p.ReturnOnInvestment
                })
                .ToList();
        }

        /// <summary>
        /// Add a channel to a campaign
        /// </summary>
        public MktCampaignChannel AddChannel(CampaignChannelDto dto)
        {
            var channel = new MktCampaignChannel
            {
                CampaignId = dto.CampaignId,
                ChannelType = dto.ChannelType,
                ChannelBudget = dto.ChannelBudget,
                Status = dto.Status ?? "active",
                ExternalCampaignId = dto.ExternalCampaignId,
                ConfigJson = dto.ConfigJson
            };

            Context.Insertable(channel).ExecuteReturnSnowflakeId();
            return channel;
        }

        /// <summary>
        /// Update a campaign channel
        /// </summary>
        public int UpdateChannel(CampaignChannelDto dto)
        {
            var channel = Context.Queryable<MktCampaignChannel>()
                .Where(ch => ch.ChannelId == dto.ChannelId)
                .First();

            if (channel == null) return 0;

            channel.ChannelType = dto.ChannelType;
            channel.ChannelBudget = dto.ChannelBudget;
            channel.Status = dto.Status;
            channel.ExternalCampaignId = dto.ExternalCampaignId;
            channel.ConfigJson = dto.ConfigJson;

            return Context.Updateable(channel).ExecuteCommand();
        }

        /// <summary>
        /// Create an A/B test for a campaign
        /// </summary>
        public MktAbTest CreateAbTest(AbTestDto dto)
        {
            var abTest = new MktAbTest
            {
                CampaignId = dto.CampaignId,
                TestName = dto.TestName,
                TestNameAr = dto.TestNameAr,
                TestElement = dto.TestElement,
                VariantAJson = dto.VariantAJson,
                VariantBJson = dto.VariantBJson,
                SplitPercentage = dto.SplitPercentage,
                WinnerMetric = dto.WinnerMetric,
                Status = dto.Status ?? "draft",
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            Context.Insertable(abTest).ExecuteReturnSnowflakeId();
            return abTest;
        }

        /// <summary>
        /// Get A/B tests for a campaign
        /// </summary>
        public List<AbTestDto> GetAbTests(long campaignId)
        {
            return Context.Queryable<MktAbTest>()
                .Where(t => t.CampaignId == campaignId)
                .OrderByDescending(t => t.Create_time)
                .Select(t => new AbTestDto
                {
                    TestId = t.TestId,
                    CampaignId = t.CampaignId,
                    TestName = t.TestName,
                    TestNameAr = t.TestNameAr,
                    TestElement = t.TestElement,
                    VariantAJson = t.VariantAJson,
                    VariantBJson = t.VariantBJson,
                    SplitPercentage = t.SplitPercentage,
                    WinnerMetric = t.WinnerMetric,
                    Status = t.Status,
                    Winner = t.Winner,
                    Confidence = t.Confidence,
                    ResultsJson = t.ResultsJson,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    SampleSizeA = t.SampleSizeA,
                    SampleSizeB = t.SampleSizeB
                })
                .ToList();
        }
    }
}
