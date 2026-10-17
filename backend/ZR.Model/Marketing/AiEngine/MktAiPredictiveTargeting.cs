using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.AiEngine
{
    /// <summary>
    /// AI Audience Segment - شرائح الجمهور الذكية
    /// AI-powered audience segmentation based on behavior, demographics, and predictive models
    /// </summary>
    [SugarTable("mkt_audience_segment")]
    public class MktAudienceSegment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SegmentId { get; set; }

        [SugarColumn(Length = 100)]
        public string SegmentCode { get; set; }

        [SugarColumn(Length = 200)]
        public string SegmentName { get; set; }

        [SugarColumn(Length = 200)]
        public string SegmentNameAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Type: demographic, behavioral, psychographic, geographic, predictive, lookalike, custom
        /// </summary>
        [SugarColumn(Length = 50)]
        public string SegmentType { get; set; }

        /// <summary>
        /// JSON: segment rules/criteria for dynamic membership
        /// Example: {"age_range": [25,45], "interests": ["tech","events"], "location": "Riyadh", "past_events": 3}
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string CriteriaJson { get; set; }

        /// <summary>
        /// Source: manual, ai_generated, imported, crowd_behavior
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Source { get; set; } = "manual";

        public int MemberCount { get; set; } = 0;

        /// <summary>
        /// AI confidence score for segment quality (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AiQualityScore { get; set; }

        /// <summary>
        /// Predicted conversion rate for this segment (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? PredictedConversionRate { get; set; }

        /// <summary>
        /// Average customer lifetime value for segment
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AvgLifetimeValue { get; set; }

        /// <summary>
        /// JSON: behavioral tags from AI analysis
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string BehavioralTagsJson { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDynamic { get; set; } = true;

        [SugarColumn(IsNullable = true)]
        public DateTime? LastRefreshedAt { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// AI Segment Member - أعضاء الشريحة
    /// </summary>
    [SugarTable("mkt_segment_member")]
    public class MktSegmentMember
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long SegmentId { get; set; }

        /// <summary>
        /// Member type: attendee, exhibitor, sponsor, investor, merchant
        /// </summary>
        [SugarColumn(Length = 30)]
        public string MemberType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// AI-calculated engagement score (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? EngagementScore { get; set; }

        /// <summary>
        /// AI-predicted purchase probability (0-1)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? PurchaseProbability { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;

        [SugarColumn(Length = 30)]
        public string AddedBy { get; set; } = "system";
    }

    /// <summary>
    /// AI Predictive Model - نماذج الذكاء الاصطناعي التنبؤية
    /// Configuration and tracking of AI/ML models used for predictions
    /// </summary>
    [SugarTable("mkt_predictive_model")]
    public class MktPredictiveModel : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ModelId { get; set; }

        [SugarColumn(Length = 100)]
        public string ModelCode { get; set; }

        [SugarColumn(Length = 200)]
        public string ModelName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string ModelNameAr { get; set; }

        /// <summary>
        /// Type: demand_forecast, churn_prediction, conversion_prediction, ltv_prediction,
        ///       crowd_density, sentiment_analysis, price_optimization, anomaly_detection
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ModelType { get; set; }

        /// <summary>
        /// Algorithm: linear_regression, random_forest, neural_network, xgboost, lstm, transformer, ensemble
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Algorithm { get; set; }

        /// <summary>
        /// JSON: model hyperparameters and configuration
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ConfigJson { get; set; }

        /// <summary>
        /// JSON: feature importance scores
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string FeatureImportanceJson { get; set; }

        /// <summary>
        /// Model accuracy (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Accuracy { get; set; }

        /// <summary>
        /// Model precision (0-1)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Precision { get; set; }

        /// <summary>
        /// Model recall (0-1)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Recall { get; set; }

        /// <summary>
        /// F1 Score (0-1)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? F1Score { get; set; }

        /// <summary>
        /// Status: training, active, deprecated, failed
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "training";

        [SugarColumn(Length = 50, IsNullable = true)]
        public string Version { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LastTrainedAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? NextTrainingAt { get; set; }

        public int TrainingDataSize { get; set; } = 0;

        /// <summary>
        /// Training frequency: daily, weekly, monthly, on_demand
        /// </summary>
        [SugarColumn(Length = 20)]
        public string TrainingFrequency { get; set; } = "weekly";

        public bool IsActive { get; set; } = true;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// AI Behavior Prediction - التنبؤات السلوكية
    /// Predictions generated by AI models for specific entities/events
    /// </summary>
    [SugarTable("mkt_behavior_prediction")]
    public class MktBehaviorPrediction
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PredictionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ModelId { get; set; }

        /// <summary>
        /// Target: event, campaign, product, audience_segment
        /// </summary>
        [SugarColumn(Length = 50)]
        public string TargetType { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long TargetId { get; set; }

        /// <summary>
        /// Prediction: demand_level, conversion_rate, attendance_count, revenue,
        ///             churn_probability, engagement_level, optimal_price, crowd_density
        /// </summary>
        [SugarColumn(Length = 50)]
        public string PredictionType { get; set; }

        [SugarColumn(DecimalDigits = 4)]
        public decimal PredictedValue { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ActualValue { get; set; }

        /// <summary>
        /// Confidence level (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal ConfidenceLevel { get; set; }

        /// <summary>
        /// JSON: prediction details and contributing factors
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DetailsJson { get; set; }

        /// <summary>
        /// JSON: recommended actions based on prediction
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string RecommendationsJson { get; set; }

        public DateTime PredictionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// The date this prediction is for
        /// </summary>
        public DateTime TargetDate { get; set; }

        /// <summary>
        /// Status: pending, validated, expired, superseded
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Error margin percentage
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ErrorMargin { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// AI Marketing Agent - وكلاء الذكاء الاصطناعي التسويقيين
    /// Autonomous AI agents for specific marketing tasks
    /// </summary>
    [SugarTable("mkt_ai_agent")]
    public class MktAiAgent : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AgentId { get; set; }

        [SugarColumn(Length = 100)]
        public string AgentCode { get; set; }

        [SugarColumn(Length = 200)]
        public string AgentName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string AgentNameAr { get; set; }

        /// <summary>
        /// Agent role: competitor_analyst, content_creator, budget_optimizer,
        ///             market_researcher, sentiment_analyzer, campaign_optimizer,
        ///             crowd_analyst, sponsor_matcher
        /// </summary>
        [SugarColumn(Length = 50)]
        public string AgentRole { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// JSON: agent capabilities and skills
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string CapabilitiesJson { get; set; }

        /// <summary>
        /// JSON: agent configuration (API keys, model settings, behavior rules)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ConfigJson { get; set; }

        /// <summary>
        /// AI model provider: openai, anthropic, google, azure, local
        /// </summary>
        [SugarColumn(Length = 30)]
        public string ModelProvider { get; set; } = "openai";

        [SugarColumn(Length = 100, IsNullable = true)]
        public string ModelName { get; set; }

        /// <summary>
        /// Status: active, paused, training, error, retired
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "active";

        public int TasksCompleted { get; set; } = 0;
        public int TasksFailed { get; set; } = 0;

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? SuccessRate { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AvgResponseTimeMs { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LastActiveAt { get; set; }

        /// <summary>
        /// Execution schedule: realtime, hourly, daily, weekly, on_demand
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Schedule { get; set; } = "on_demand";

        public bool IsActive { get; set; } = true;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// AI Agent Task Execution Log - سجل مهام وكيل الذكاء الاصطناعي
    /// </summary>
    [SugarTable("mkt_ai_agent_task")]
    public class MktAiAgentTask
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AgentId { get; set; }

        /// <summary>
        /// Task type: analyze_competitor, generate_content, optimize_budget,
        ///            analyze_sentiment, predict_demand, generate_report, match_sponsors
        /// </summary>
        [SugarColumn(Length = 50)]
        public string TaskType { get; set; }

        /// <summary>
        /// JSON: task input parameters
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string InputJson { get; set; }

        /// <summary>
        /// JSON: task output/results
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string OutputJson { get; set; }

        /// <summary>
        /// Status: queued, running, completed, failed, cancelled
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "queued";

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = true)]
        public DateTime? StartedAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Processing time in milliseconds
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long? ProcessingTimeMs { get; set; }

        /// <summary>
        /// Tokens used for AI processing
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? TokensUsed { get; set; }

        /// <summary>
        /// Cost of AI processing in SAR
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ProcessingCost { get; set; }

        /// <summary>
        /// User who triggered the task
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? TriggeredBy { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// AI Competitor Analysis - تحليل المنافسين بالذكاء الاصطناعي
    /// Results from competitor analysis AI agent
    /// </summary>
    [SugarTable("mkt_competitor_analysis")]
    public class MktCompetitorAnalysis : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AnalysisId { get; set; }

        [SugarColumn(Length = 200)]
        public string CompetitorName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string CompetitorNameAr { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string CompetitorUrl { get; set; }

        /// <summary>
        /// Sector: events, exhibitions, conferences, entertainment, hospitality
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Sector { get; set; }

        /// <summary>
        /// JSON: competitor strengths analysis
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string StrengthsJson { get; set; }

        /// <summary>
        /// JSON: competitor weaknesses analysis
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string WeaknessesJson { get; set; }

        /// <summary>
        /// JSON: market positioning data
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string MarketPositionJson { get; set; }

        /// <summary>
        /// JSON: pricing intelligence
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string PricingIntelJson { get; set; }

        /// <summary>
        /// JSON: social media analysis (followers, engagement, sentiment)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string SocialAnalysisJson { get; set; }

        /// <summary>
        /// Threat level: low, medium, high, critical
        /// </summary>
        [SugarColumn(Length = 20)]
        public string ThreatLevel { get; set; } = "medium";

        /// <summary>
        /// Overall competitor score (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? OverallScore { get; set; }

        /// <summary>
        /// JSON: AI-generated strategic recommendations
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string RecommendationsJson { get; set; }

        /// <summary>
        /// JSON: Saudi market specific insights
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string SaudiMarketInsightsJson { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LastAnalyzedAt { get; set; }

        [SugarColumn(Length = 30)]
        public string AnalysisFrequency { get; set; } = "weekly";

        public bool IsActive { get; set; } = true;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// AI Budget Optimization - تحسين الميزانية بالذكاء الاصطناعي
    /// AI-driven marketing budget allocation and optimization
    /// </summary>
    [SugarTable("mkt_budget_optimization")]
    public class MktBudgetOptimization : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OptimizationId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CampaignId { get; set; }

        /// <summary>
        /// Scope: campaign, channel, monthly, quarterly, annual
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Scope { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal CurrentBudget { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal RecommendedBudget { get; set; }

        /// <summary>
        /// JSON: recommended budget allocation across channels
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string AllocationJson { get; set; }

        /// <summary>
        /// JSON: current vs recommended comparison
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ComparisonJson { get; set; }

        /// <summary>
        /// Expected ROI improvement percentage
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? ExpectedRoiImprovement { get; set; }

        /// <summary>
        /// Expected cost savings
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? ExpectedSavings { get; set; }

        /// <summary>
        /// AI confidence (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal Confidence { get; set; }

        /// <summary>
        /// JSON: reasoning and factors
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ReasoningJson { get; set; }

        /// <summary>
        /// Status: pending, approved, applied, rejected
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "pending";

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ApprovedBy { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ApprovedAt { get; set; }

        public DateTime OptimizationDate { get; set; } = DateTime.Now;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    public static class AiAgentRoles
    {
        public const string COMPETITOR_ANALYST = "competitor_analyst";
        public const string CONTENT_CREATOR = "content_creator";
        public const string BUDGET_OPTIMIZER = "budget_optimizer";
        public const string MARKET_RESEARCHER = "market_researcher";
        public const string SENTIMENT_ANALYZER = "sentiment_analyzer";
        public const string CAMPAIGN_OPTIMIZER = "campaign_optimizer";
        public const string CROWD_ANALYST = "crowd_analyst";
        public const string SPONSOR_MATCHER = "sponsor_matcher";
    }
}
