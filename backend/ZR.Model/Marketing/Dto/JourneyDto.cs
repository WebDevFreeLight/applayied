using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Customer Journey DTOs

    public class JourneyQueryDto : PagerInfo
    {
        public string JourneyName { get; set; }
        public string JourneyType { get; set; }
        public string Status { get; set; }
        public string EntryTrigger { get; set; }
        public bool? AiPersonalizationEnabled { get; set; }
    }

    public class JourneyDto
    {
        public long JourneyId { get; set; }
        public string JourneyCode { get; set; }
        public string JourneyName { get; set; }
        public string JourneyNameAr { get; set; }
        public string Description { get; set; }
        public string JourneyType { get; set; }
        public string EntryTrigger { get; set; }
        public string EntryConditionsJson { get; set; }
        public string ExitConditionsJson { get; set; }
        public long? TargetSegmentId { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AiPersonalizationEnabled { get; set; }
        public bool DynamicContentEnabled { get; set; }
        public string FlowDefinitionJson { get; set; }
        public string Priority { get; set; }
        public List<JourneyStageDto> Stages { get; set; }
    }

    public class JourneyOutputDto : JourneyDto
    {
        public int TotalEnrolled { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalDropped { get; set; }
        public int CurrentlyActive { get; set; }
        public decimal? ConversionRate { get; set; }
        public decimal? AvgDurationHours { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal DropRate { get; set; }
        public string SegmentName { get; set; }
    }

    #endregion

    #region Journey Stage DTOs

    public class JourneyStageDto
    {
        public long StageId { get; set; }
        public long JourneyId { get; set; }
        public string StageName { get; set; }
        public string StageNameAr { get; set; }
        public string StageType { get; set; }
        public int SortOrder { get; set; }
        public string ConfigJson { get; set; }
        public int? WaitMinutes { get; set; }
        public string WaitUntilTime { get; set; }
        public string ConditionJson { get; set; }
        public long? NextStageIfTrue { get; set; }
        public long? NextStageIfFalse { get; set; }
        public long? NextStageId { get; set; }
        public string DynamicContentJson { get; set; }
        public string AbTestJson { get; set; }
        public int TotalProcessed { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalFailed { get; set; }
        public decimal? EngagementRate { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region Journey Trigger DTOs

    public class JourneyTriggerDto
    {
        public long TriggerId { get; set; }
        public long JourneyId { get; set; }
        public string TriggerName { get; set; }
        public string EventType { get; set; }
        public string ConditionsJson { get; set; }
        public string ActionType { get; set; }
        public string ActionParamsJson { get; set; }
        public long? TargetStageId { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public int TimesTriggered { get; set; }
    }

    #endregion

    #region Journey Execution DTOs

    public class JourneyExecutionQueryDto : PagerInfo
    {
        public long? JourneyId { get; set; }
        public string MemberType { get; set; }
        public long? MemberId { get; set; }
        public string Status { get; set; }
        public bool? IsConverted { get; set; }
        public DateTime? EnrolledFrom { get; set; }
        public DateTime? EnrolledTo { get; set; }
    }

    public class JourneyExecutionDto
    {
        public long ExecutionId { get; set; }
        public long JourneyId { get; set; }
        public string JourneyName { get; set; }
        public string MemberType { get; set; }
        public long MemberId { get; set; }
        public string MemberName { get; set; }
        public long? CurrentStageId { get; set; }
        public string CurrentStageName { get; set; }
        public string Status { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public int StagesCompleted { get; set; }
        public decimal? EngagementScore { get; set; }
        public string PersonalizationProfileJson { get; set; }
        public bool IsConverted { get; set; }
        public DateTime? ConvertedAt { get; set; }
        public decimal? AttributedRevenue { get; set; }
    }

    public class EnrollJourneyRequest
    {
        public long JourneyId { get; set; }
        public string MemberType { get; set; }
        public long MemberId { get; set; }
    }

    #endregion

    #region Dynamic Content DTOs

    public class DynamicContentQueryDto : PagerInfo
    {
        public string ContentName { get; set; }
        public string ContentType { get; set; }
        public bool? IsActive { get; set; }
        public bool? AiOptimized { get; set; }
    }

    public class DynamicContentDto
    {
        public long ContentId { get; set; }
        public string ContentName { get; set; }
        public string ContentNameAr { get; set; }
        public string ContentType { get; set; }
        public string DefaultContent { get; set; }
        public string DefaultContentAr { get; set; }
        public string VariantsJson { get; set; }
        public string TemplateVarsJson { get; set; }
        public string LanguageMode { get; set; }
        public bool AiOptimized { get; set; }
        public int TimesServed { get; set; }
        public decimal? EngagementRate { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region Journey Analytics DTOs

    public class JourneyAnalyticsDto
    {
        public int TotalJourneys { get; set; }
        public int ActiveJourneys { get; set; }
        public int TotalEnrolled { get; set; }
        public int TotalConverted { get; set; }
        public decimal OverallConversionRate { get; set; }
        public decimal OverallCompletionRate { get; set; }
        public decimal TotalAttributedRevenue { get; set; }
        public decimal AvgEngagementScore { get; set; }
        public List<JourneyPerformanceSummary> JourneyPerformance { get; set; }
        public List<StageFunnelDto> FunnelAnalysis { get; set; }
    }

    public class JourneyPerformanceSummary
    {
        public long JourneyId { get; set; }
        public string JourneyName { get; set; }
        public string JourneyType { get; set; }
        public int Enrolled { get; set; }
        public int Completed { get; set; }
        public int Converted { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal? Revenue { get; set; }
    }

    public class StageFunnelDto
    {
        public string StageName { get; set; }
        public int StageOrder { get; set; }
        public int EnteredCount { get; set; }
        public int CompletedCount { get; set; }
        public int DroppedCount { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal? AvgTimeMinutes { get; set; }
    }

    #endregion
}
