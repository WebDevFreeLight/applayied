using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Audience Segment DTOs

    public class AudienceSegmentQueryDto : PagerInfo
    {
        public string SegmentName { get; set; }
        public string SegmentType { get; set; }
        public string Source { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDynamic { get; set; }
    }

    public class AudienceSegmentDto
    {
        public long SegmentId { get; set; }
        public string SegmentCode { get; set; }
        public string SegmentName { get; set; }
        public string SegmentNameAr { get; set; }
        public string Description { get; set; }
        public string SegmentType { get; set; }
        public string CriteriaJson { get; set; }
        public string Source { get; set; }
        public int MemberCount { get; set; }
        public decimal? AiQualityScore { get; set; }
        public decimal? PredictedConversionRate { get; set; }
        public decimal? AvgLifetimeValue { get; set; }
        public string BehavioralTagsJson { get; set; }
        public bool IsActive { get; set; }
        public bool IsDynamic { get; set; }
        public DateTime? LastRefreshedAt { get; set; }
    }

    public class SegmentMemberDto
    {
        public long Id { get; set; }
        public long SegmentId { get; set; }
        public string MemberType { get; set; }
        public long MemberId { get; set; }
        public decimal? EngagementScore { get; set; }
        public decimal? PurchaseProbability { get; set; }
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }
    }

    public class AddSegmentMembersDto
    {
        public long SegmentId { get; set; }
        public List<SegmentMemberInput> Members { get; set; }
    }

    public class SegmentMemberInput
    {
        public string MemberType { get; set; }
        public long MemberId { get; set; }
    }

    #endregion

    #region Predictive Model DTOs

    public class PredictiveModelQueryDto : PagerInfo
    {
        public string ModelType { get; set; }
        public string Status { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PredictiveModelDto
    {
        public long ModelId { get; set; }
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public string ModelNameAr { get; set; }
        public string ModelType { get; set; }
        public string Algorithm { get; set; }
        public string ConfigJson { get; set; }
        public string FeatureImportanceJson { get; set; }
        public decimal? Accuracy { get; set; }
        public decimal? Precision { get; set; }
        public decimal? Recall { get; set; }
        public decimal? F1Score { get; set; }
        public string Status { get; set; }
        public string Version { get; set; }
        public DateTime? LastTrainedAt { get; set; }
        public DateTime? NextTrainingAt { get; set; }
        public int TrainingDataSize { get; set; }
        public string TrainingFrequency { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region Behavior Prediction DTOs

    public class BehaviorPredictionQueryDto : PagerInfo
    {
        public long? ModelId { get; set; }
        public string TargetType { get; set; }
        public long? TargetId { get; set; }
        public string PredictionType { get; set; }
        public string Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class BehaviorPredictionDto
    {
        public long PredictionId { get; set; }
        public long ModelId { get; set; }
        public string ModelName { get; set; }
        public string TargetType { get; set; }
        public long TargetId { get; set; }
        public string TargetName { get; set; }
        public string PredictionType { get; set; }
        public decimal PredictedValue { get; set; }
        public decimal? ActualValue { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public string DetailsJson { get; set; }
        public string RecommendationsJson { get; set; }
        public DateTime PredictionDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; }
        public decimal? ErrorMargin { get; set; }
        public decimal? AccuracyRate { get; set; }
    }

    public class GeneratePredictionRequest
    {
        public long ModelId { get; set; }
        public string TargetType { get; set; }
        public long TargetId { get; set; }
        public string PredictionType { get; set; }
        public DateTime TargetDate { get; set; }
        public string ContextJson { get; set; }
    }

    #endregion

    #region AI Agent DTOs

    public class AiAgentQueryDto : PagerInfo
    {
        public string AgentRole { get; set; }
        public string Status { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AiAgentDto
    {
        public long AgentId { get; set; }
        public string AgentCode { get; set; }
        public string AgentName { get; set; }
        public string AgentNameAr { get; set; }
        public string AgentRole { get; set; }
        public string Description { get; set; }
        public string CapabilitiesJson { get; set; }
        public string ConfigJson { get; set; }
        public string ModelProvider { get; set; }
        public string ModelName { get; set; }
        public string Status { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksFailed { get; set; }
        public decimal? SuccessRate { get; set; }
        public decimal? AvgResponseTimeMs { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public string Schedule { get; set; }
        public bool IsActive { get; set; }
    }

    public class AiAgentTaskQueryDto : PagerInfo
    {
        public long? AgentId { get; set; }
        public string TaskType { get; set; }
        public string Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class AiAgentTaskDto
    {
        public long TaskId { get; set; }
        public long AgentId { get; set; }
        public string AgentName { get; set; }
        public string TaskType { get; set; }
        public string InputJson { get; set; }
        public string OutputJson { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? ProcessingTimeMs { get; set; }
        public int? TokensUsed { get; set; }
        public decimal? ProcessingCost { get; set; }
        public long? TriggeredBy { get; set; }
        public string TriggeredByName { get; set; }
    }

    public class ExecuteAiTaskRequest
    {
        public long AgentId { get; set; }
        public string TaskType { get; set; }
        public string InputJson { get; set; }
    }

    public class AiAgentAnalyticsDto
    {
        public int TotalAgents { get; set; }
        public int ActiveAgents { get; set; }
        public int TotalTasksCompleted { get; set; }
        public int TotalTasksFailed { get; set; }
        public decimal OverallSuccessRate { get; set; }
        public decimal AvgProcessingTime { get; set; }
        public decimal TotalProcessingCost { get; set; }
        public int TotalTokensUsed { get; set; }
        public List<AgentPerformanceDto> AgentPerformance { get; set; }
        public List<TaskTypeSummaryDto> TaskTypeSummary { get; set; }
    }

    public class AgentPerformanceDto
    {
        public long AgentId { get; set; }
        public string AgentName { get; set; }
        public string AgentRole { get; set; }
        public int TasksCompleted { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AvgProcessingTime { get; set; }
    }

    public class TaskTypeSummaryDto
    {
        public string TaskType { get; set; }
        public int Count { get; set; }
        public decimal AvgProcessingTime { get; set; }
        public decimal SuccessRate { get; set; }
    }

    #endregion

    #region Competitor Analysis DTOs

    public class CompetitorAnalysisQueryDto : PagerInfo
    {
        public string CompetitorName { get; set; }
        public string Sector { get; set; }
        public string ThreatLevel { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CompetitorAnalysisDto
    {
        public long AnalysisId { get; set; }
        public string CompetitorName { get; set; }
        public string CompetitorNameAr { get; set; }
        public string CompetitorUrl { get; set; }
        public string Sector { get; set; }
        public string StrengthsJson { get; set; }
        public string WeaknessesJson { get; set; }
        public string MarketPositionJson { get; set; }
        public string PricingIntelJson { get; set; }
        public string SocialAnalysisJson { get; set; }
        public string ThreatLevel { get; set; }
        public decimal? OverallScore { get; set; }
        public string RecommendationsJson { get; set; }
        public string SaudiMarketInsightsJson { get; set; }
        public DateTime? LastAnalyzedAt { get; set; }
        public string AnalysisFrequency { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region Budget Optimization DTOs

    public class BudgetOptimizationQueryDto : PagerInfo
    {
        public long? CampaignId { get; set; }
        public string Scope { get; set; }
        public string Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class BudgetOptimizationDto
    {
        public long OptimizationId { get; set; }
        public long? CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string Scope { get; set; }
        public decimal CurrentBudget { get; set; }
        public decimal RecommendedBudget { get; set; }
        public string AllocationJson { get; set; }
        public string ComparisonJson { get; set; }
        public decimal? ExpectedRoiImprovement { get; set; }
        public decimal? ExpectedSavings { get; set; }
        public decimal Confidence { get; set; }
        public string ReasoningJson { get; set; }
        public string Status { get; set; }
        public long? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime OptimizationDate { get; set; }
    }

    public class ApproveBudgetOptimizationDto
    {
        public long OptimizationId { get; set; }
        public bool Approve { get; set; }
        public string Notes { get; set; }
    }

    #endregion
}
