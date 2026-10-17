using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model;
using ZR.Model.Marketing.AiEngine;
using ZR.Model.Marketing.Dto;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// AI Engine Service Implementation - خدمة محرك الذكاء الاصطناعي
    /// AI agent management, prediction handling, segment management, competitor analysis, budget optimization
    /// </summary>
    [AppService(ServiceType = typeof(IAiEngineService), ServiceLifetime = LifeTime.Transient)]
    public class AiEngineService : BaseService<MktAudienceSegment>, IAiEngineService
    {
        #region Audience Segments

        /// <summary>
        /// Get audience segments with filtering
        /// </summary>
        public PagedInfo<AudienceSegmentDto> GetSegments(AudienceSegmentQueryDto query)
        {
            var predicate = Expressionable.Create<MktAudienceSegment>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.SegmentName), it => it.SegmentName.Contains(query.SegmentName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.SegmentType), it => it.SegmentType == query.SegmentType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Source), it => it.Source == query.Source);
            predicate = predicate.AndIF(query.IsActive != null, it => it.IsActive == query.IsActive);
            predicate = predicate.AndIF(query.IsDynamic != null, it => it.IsDynamic == query.IsDynamic);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(s => s.Create_time)
                .Select(s => new AudienceSegmentDto
                {
                    SegmentId = s.SegmentId,
                    SegmentCode = s.SegmentCode,
                    SegmentName = s.SegmentName,
                    SegmentNameAr = s.SegmentNameAr,
                    Description = s.Description,
                    SegmentType = s.SegmentType,
                    CriteriaJson = s.CriteriaJson,
                    Source = s.Source,
                    MemberCount = s.MemberCount,
                    AiQualityScore = s.AiQualityScore,
                    PredictedConversionRate = s.PredictedConversionRate,
                    AvgLifetimeValue = s.AvgLifetimeValue,
                    BehavioralTagsJson = s.BehavioralTagsJson,
                    IsActive = s.IsActive,
                    IsDynamic = s.IsDynamic,
                    LastRefreshedAt = s.LastRefreshedAt
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Add or update an audience segment
        /// </summary>
        public MktAudienceSegment ManageSegments(AudienceSegmentDto dto)
        {
            if (dto.SegmentId > 0)
            {
                var existing = Queryable().Where(s => s.SegmentId == dto.SegmentId).First();
                if (existing != null)
                {
                    existing.SegmentCode = dto.SegmentCode;
                    existing.SegmentName = dto.SegmentName;
                    existing.SegmentNameAr = dto.SegmentNameAr;
                    existing.Description = dto.Description;
                    existing.SegmentType = dto.SegmentType;
                    existing.CriteriaJson = dto.CriteriaJson;
                    existing.Source = dto.Source;
                    existing.IsActive = dto.IsActive;
                    existing.IsDynamic = dto.IsDynamic;
                    existing.BehavioralTagsJson = dto.BehavioralTagsJson;

                    Update(existing, true, "Update audience segment");
                    return existing;
                }
            }

            var segment = new MktAudienceSegment
            {
                SegmentCode = dto.SegmentCode,
                SegmentName = dto.SegmentName,
                SegmentNameAr = dto.SegmentNameAr,
                Description = dto.Description,
                SegmentType = dto.SegmentType,
                CriteriaJson = dto.CriteriaJson,
                Source = dto.Source ?? "manual",
                IsActive = dto.IsActive,
                IsDynamic = dto.IsDynamic,
                BehavioralTagsJson = dto.BehavioralTagsJson
            };

            Insertable(segment).ExecuteReturnSnowflakeId();
            return segment;
        }

        #endregion

        #region Predictions

        /// <summary>
        /// Get behavior predictions with filtering
        /// </summary>
        public PagedInfo<BehaviorPredictionDto> GetPredictions(BehaviorPredictionQueryDto query)
        {
            var predicate = Expressionable.Create<MktBehaviorPrediction>();

            predicate = predicate.AndIF(query.ModelId != null, it => it.ModelId == query.ModelId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.TargetType), it => it.TargetType == query.TargetType);
            predicate = predicate.AndIF(query.TargetId != null, it => it.TargetId == query.TargetId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.PredictionType), it => it.PredictionType == query.PredictionType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.DateFrom != null, it => it.PredictionDate >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo != null, it => it.PredictionDate <= query.DateTo);

            var response = Context.Queryable<MktBehaviorPrediction>()
                .Where(predicate.ToExpression())
                .OrderByDescending(p => p.PredictionDate)
                .Select(p => new BehaviorPredictionDto
                {
                    PredictionId = p.PredictionId,
                    ModelId = p.ModelId,
                    TargetType = p.TargetType,
                    TargetId = p.TargetId,
                    PredictionType = p.PredictionType,
                    PredictedValue = p.PredictedValue,
                    ActualValue = p.ActualValue,
                    ConfidenceLevel = p.ConfidenceLevel,
                    DetailsJson = p.DetailsJson,
                    RecommendationsJson = p.RecommendationsJson,
                    PredictionDate = p.PredictionDate,
                    TargetDate = p.TargetDate,
                    Status = p.Status,
                    ErrorMargin = p.ErrorMargin,
                    AccuracyRate = p.ActualValue.HasValue && p.PredictedValue != 0
                        ? (1 - Math.Abs((p.ActualValue.Value - p.PredictedValue) / p.PredictedValue)) * 100
                        : null
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Generate a new behavior prediction
        /// </summary>
        public MktBehaviorPrediction GeneratePrediction(GeneratePredictionRequest request)
        {
            var prediction = new MktBehaviorPrediction
            {
                ModelId = request.ModelId,
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                PredictionType = request.PredictionType,
                TargetDate = request.TargetDate,
                PredictionDate = DateTime.Now,
                Status = "pending",
                // Simulated prediction values - in real implementation, call AI model
                PredictedValue = 0,
                ConfidenceLevel = 0,
                DetailsJson = request.ContextJson
            };

            Context.Insertable(prediction).ExecuteReturnSnowflakeId();
            return prediction;
        }

        #endregion

        #region AI Agents

        /// <summary>
        /// Get AI agents with filtering
        /// </summary>
        public PagedInfo<AiAgentDto> GetAiAgents(AiAgentQueryDto query)
        {
            var predicate = Expressionable.Create<MktAiAgent>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.AgentRole), it => it.AgentRole == query.AgentRole);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.IsActive != null, it => it.IsActive == query.IsActive);

            var response = Context.Queryable<MktAiAgent>()
                .Where(predicate.ToExpression())
                .OrderByDescending(a => a.Create_time)
                .Select(a => new AiAgentDto
                {
                    AgentId = a.AgentId,
                    AgentCode = a.AgentCode,
                    AgentName = a.AgentName,
                    AgentNameAr = a.AgentNameAr,
                    AgentRole = a.AgentRole,
                    Description = a.Description,
                    CapabilitiesJson = a.CapabilitiesJson,
                    ConfigJson = a.ConfigJson,
                    ModelProvider = a.ModelProvider,
                    ModelName = a.ModelName,
                    Status = a.Status,
                    TasksCompleted = a.TasksCompleted,
                    TasksFailed = a.TasksFailed,
                    SuccessRate = a.SuccessRate,
                    AvgResponseTimeMs = a.AvgResponseTimeMs,
                    LastActiveAt = a.LastActiveAt,
                    Schedule = a.Schedule,
                    IsActive = a.IsActive
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Execute an AI agent task
        /// </summary>
        public MktAiAgentTask ExecuteAgentTask(ExecuteAiTaskRequest request)
        {
            var task = new MktAiAgentTask
            {
                AgentId = request.AgentId,
                TaskType = request.TaskType,
                InputJson = request.InputJson,
                Status = "queued",
                CreatedAt = DateTime.Now
            };

            Context.Insertable(task).ExecuteReturnSnowflakeId();

            // Update agent last active timestamp
            Context.Updateable<MktAiAgent>()
                .SetColumns(a => a.LastActiveAt == DateTime.Now)
                .Where(a => a.AgentId == request.AgentId)
                .ExecuteCommand();

            return task;
        }

        #endregion

        #region Competitor Analysis

        /// <summary>
        /// Get competitor analysis entries with filtering
        /// </summary>
        public PagedInfo<CompetitorAnalysisDto> GetCompetitorAnalysis(CompetitorAnalysisQueryDto query)
        {
            var predicate = Expressionable.Create<MktCompetitorAnalysis>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CompetitorName), it => it.CompetitorName.Contains(query.CompetitorName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Sector), it => it.Sector == query.Sector);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ThreatLevel), it => it.ThreatLevel == query.ThreatLevel);
            predicate = predicate.AndIF(query.IsActive != null, it => it.IsActive == query.IsActive);

            var response = Context.Queryable<MktCompetitorAnalysis>()
                .Where(predicate.ToExpression())
                .OrderByDescending(c => c.Create_time)
                .Select(c => new CompetitorAnalysisDto
                {
                    AnalysisId = c.AnalysisId,
                    CompetitorName = c.CompetitorName,
                    CompetitorNameAr = c.CompetitorNameAr,
                    CompetitorUrl = c.CompetitorUrl,
                    Sector = c.Sector,
                    StrengthsJson = c.StrengthsJson,
                    WeaknessesJson = c.WeaknessesJson,
                    MarketPositionJson = c.MarketPositionJson,
                    PricingIntelJson = c.PricingIntelJson,
                    SocialAnalysisJson = c.SocialAnalysisJson,
                    ThreatLevel = c.ThreatLevel,
                    OverallScore = c.OverallScore,
                    RecommendationsJson = c.RecommendationsJson,
                    SaudiMarketInsightsJson = c.SaudiMarketInsightsJson,
                    LastAnalyzedAt = c.LastAnalyzedAt,
                    AnalysisFrequency = c.AnalysisFrequency,
                    IsActive = c.IsActive
                })
                .ToPage(query);

            return response;
        }

        #endregion

        #region Budget Optimization

        /// <summary>
        /// Get budget optimization recommendations
        /// </summary>
        public PagedInfo<BudgetOptimizationDto> GetBudgetOptimizations(BudgetOptimizationQueryDto query)
        {
            var predicate = Expressionable.Create<MktBudgetOptimization>();

            predicate = predicate.AndIF(query.CampaignId != null, it => it.CampaignId == query.CampaignId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Scope), it => it.Scope == query.Scope);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.DateFrom != null, it => it.OptimizationDate >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo != null, it => it.OptimizationDate <= query.DateTo);

            var response = Context.Queryable<MktBudgetOptimization>()
                .Where(predicate.ToExpression())
                .OrderByDescending(b => b.OptimizationDate)
                .Select(b => new BudgetOptimizationDto
                {
                    OptimizationId = b.OptimizationId,
                    CampaignId = b.CampaignId,
                    Scope = b.Scope,
                    CurrentBudget = b.CurrentBudget,
                    RecommendedBudget = b.RecommendedBudget,
                    AllocationJson = b.AllocationJson,
                    ComparisonJson = b.ComparisonJson,
                    ExpectedRoiImprovement = b.ExpectedRoiImprovement,
                    ExpectedSavings = b.ExpectedSavings,
                    Confidence = b.Confidence,
                    ReasoningJson = b.ReasoningJson,
                    Status = b.Status,
                    ApprovedBy = b.ApprovedBy,
                    ApprovedAt = b.ApprovedAt,
                    OptimizationDate = b.OptimizationDate
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Approve or reject a budget optimization
        /// </summary>
        public int ApproveBudgetOptimization(ApproveBudgetOptimizationDto dto, long userId)
        {
            var status = dto.Approve ? "approved" : "rejected";

            return Context.Updateable<MktBudgetOptimization>()
                .SetColumns(b => b.Status == status)
                .SetColumns(b => b.ApprovedBy == userId)
                .SetColumns(b => b.ApprovedAt == DateTime.Now)
                .Where(b => b.OptimizationId == dto.OptimizationId)
                .ExecuteCommand();
        }

        #endregion
    }
}
