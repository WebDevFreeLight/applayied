using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model;
using ZR.Model.Marketing.Journeys;
using ZR.Model.Marketing.Dto;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Journey Service Implementation - خدمة رحلة العميل
    /// Journey lifecycle management, stage flow, trigger processing, execution tracking, analytics
    /// </summary>
    [AppService(ServiceType = typeof(IJourneyService), ServiceLifetime = LifeTime.Transient)]
    public class JourneyService : BaseService<MktCustomerJourney>, IJourneyService
    {
        #region Journey CRUD

        /// <summary>
        /// Get journeys with filtering
        /// </summary>
        public PagedInfo<JourneyOutputDto> GetJourneys(JourneyQueryDto query)
        {
            var predicate = Expressionable.Create<MktCustomerJourney>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.JourneyName), it => it.JourneyName.Contains(query.JourneyName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.JourneyType), it => it.JourneyType == query.JourneyType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.EntryTrigger), it => it.EntryTrigger == query.EntryTrigger);
            predicate = predicate.AndIF(query.AiPersonalizationEnabled != null, it => it.AiPersonalizationEnabled == query.AiPersonalizationEnabled);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(j => j.Create_time)
                .Select(j => new JourneyOutputDto
                {
                    JourneyId = j.JourneyId,
                    JourneyCode = j.JourneyCode,
                    JourneyName = j.JourneyName,
                    JourneyNameAr = j.JourneyNameAr,
                    Description = j.Description,
                    JourneyType = j.JourneyType,
                    EntryTrigger = j.EntryTrigger,
                    EntryConditionsJson = j.EntryConditionsJson,
                    ExitConditionsJson = j.ExitConditionsJson,
                    TargetSegmentId = j.TargetSegmentId,
                    Status = j.Status,
                    StartDate = j.StartDate,
                    EndDate = j.EndDate,
                    AiPersonalizationEnabled = j.AiPersonalizationEnabled,
                    DynamicContentEnabled = j.DynamicContentEnabled,
                    FlowDefinitionJson = j.FlowDefinitionJson,
                    Priority = j.Priority,
                    TotalEnrolled = j.TotalEnrolled,
                    TotalCompleted = j.TotalCompleted,
                    TotalDropped = j.TotalDropped,
                    CurrentlyActive = j.CurrentlyActive,
                    ConversionRate = j.ConversionRate,
                    AvgDurationHours = j.AvgDurationHours,
                    CompletionRate = j.CompletionRate,
                    DropRate = j.DropRate
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Get journey detail with stages
        /// </summary>
        public JourneyOutputDto GetJourneyDetail(long journeyId)
        {
            var journey = Queryable()
                .Where(j => j.JourneyId == journeyId)
                .First();

            if (journey == null) return null;

            var dto = new JourneyOutputDto
            {
                JourneyId = journey.JourneyId,
                JourneyCode = journey.JourneyCode,
                JourneyName = journey.JourneyName,
                JourneyNameAr = journey.JourneyNameAr,
                Description = journey.Description,
                JourneyType = journey.JourneyType,
                EntryTrigger = journey.EntryTrigger,
                EntryConditionsJson = journey.EntryConditionsJson,
                ExitConditionsJson = journey.ExitConditionsJson,
                TargetSegmentId = journey.TargetSegmentId,
                Status = journey.Status,
                StartDate = journey.StartDate,
                EndDate = journey.EndDate,
                AiPersonalizationEnabled = journey.AiPersonalizationEnabled,
                DynamicContentEnabled = journey.DynamicContentEnabled,
                FlowDefinitionJson = journey.FlowDefinitionJson,
                Priority = journey.Priority,
                TotalEnrolled = journey.TotalEnrolled,
                TotalCompleted = journey.TotalCompleted,
                TotalDropped = journey.TotalDropped,
                CurrentlyActive = journey.CurrentlyActive,
                ConversionRate = journey.ConversionRate,
                AvgDurationHours = journey.AvgDurationHours,
                CompletionRate = journey.CompletionRate,
                DropRate = journey.DropRate
            };

            // Load stages
            dto.Stages = GetStages(journeyId);

            return dto;
        }

        /// <summary>
        /// Add a new journey
        /// </summary>
        public MktCustomerJourney AddJourney(JourneyDto dto)
        {
            var journey = new MktCustomerJourney
            {
                JourneyCode = dto.JourneyCode,
                JourneyName = dto.JourneyName,
                JourneyNameAr = dto.JourneyNameAr,
                Description = dto.Description,
                JourneyType = dto.JourneyType,
                EntryTrigger = dto.EntryTrigger,
                EntryConditionsJson = dto.EntryConditionsJson,
                ExitConditionsJson = dto.ExitConditionsJson,
                TargetSegmentId = dto.TargetSegmentId,
                Status = dto.Status ?? "draft",
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                AiPersonalizationEnabled = dto.AiPersonalizationEnabled,
                DynamicContentEnabled = dto.DynamicContentEnabled,
                FlowDefinitionJson = dto.FlowDefinitionJson,
                Priority = dto.Priority ?? "medium"
            };

            Insertable(journey).ExecuteReturnSnowflakeId();

            // Add stages if provided
            if (dto.Stages != null && dto.Stages.Count > 0)
            {
                foreach (var stageDto in dto.Stages)
                {
                    stageDto.JourneyId = journey.JourneyId;
                    AddStage(stageDto);
                }
            }

            return journey;
        }

        /// <summary>
        /// Update an existing journey
        /// </summary>
        public int UpdateJourney(JourneyDto dto)
        {
            var journey = Queryable()
                .Where(j => j.JourneyId == dto.JourneyId)
                .First();

            if (journey == null) return 0;

            journey.JourneyCode = dto.JourneyCode;
            journey.JourneyName = dto.JourneyName;
            journey.JourneyNameAr = dto.JourneyNameAr;
            journey.Description = dto.Description;
            journey.JourneyType = dto.JourneyType;
            journey.EntryTrigger = dto.EntryTrigger;
            journey.EntryConditionsJson = dto.EntryConditionsJson;
            journey.ExitConditionsJson = dto.ExitConditionsJson;
            journey.TargetSegmentId = dto.TargetSegmentId;
            journey.Status = dto.Status;
            journey.StartDate = dto.StartDate;
            journey.EndDate = dto.EndDate;
            journey.AiPersonalizationEnabled = dto.AiPersonalizationEnabled;
            journey.DynamicContentEnabled = dto.DynamicContentEnabled;
            journey.FlowDefinitionJson = dto.FlowDefinitionJson;
            journey.Priority = dto.Priority;

            return Update(journey, true, "Update journey");
        }

        /// <summary>
        /// Delete journeys by IDs
        /// </summary>
        public int DeleteJourneys(long[] ids)
        {
            return Delete(ids);
        }

        /// <summary>
        /// Update journey status
        /// </summary>
        public int UpdateJourneyStatus(long journeyId, string status)
        {
            return Context.Updateable<MktCustomerJourney>()
                .SetColumns(j => j.Status == status)
                .Where(j => j.JourneyId == journeyId)
                .ExecuteCommand();
        }

        #endregion

        #region Stages

        /// <summary>
        /// Get stages for a journey
        /// </summary>
        public List<JourneyStageDto> GetStages(long journeyId)
        {
            return Context.Queryable<MktJourneyStage>()
                .Where(s => s.JourneyId == journeyId)
                .OrderBy(s => s.SortOrder)
                .Select(s => new JourneyStageDto
                {
                    StageId = s.StageId,
                    JourneyId = s.JourneyId,
                    StageName = s.StageName,
                    StageNameAr = s.StageNameAr,
                    StageType = s.StageType,
                    SortOrder = s.SortOrder,
                    ConfigJson = s.ConfigJson,
                    WaitMinutes = s.WaitMinutes,
                    WaitUntilTime = s.WaitUntilTime,
                    ConditionJson = s.ConditionJson,
                    NextStageIfTrue = s.NextStageIfTrue,
                    NextStageIfFalse = s.NextStageIfFalse,
                    NextStageId = s.NextStageId,
                    DynamicContentJson = s.DynamicContentJson,
                    AbTestJson = s.AbTestJson,
                    TotalProcessed = s.TotalProcessed,
                    TotalSuccess = s.TotalSuccess,
                    TotalFailed = s.TotalFailed,
                    EngagementRate = s.EngagementRate,
                    IsActive = s.IsActive
                })
                .ToList();
        }

        /// <summary>
        /// Add a stage to a journey
        /// </summary>
        public MktJourneyStage AddStage(JourneyStageDto dto)
        {
            var stage = new MktJourneyStage
            {
                JourneyId = dto.JourneyId,
                StageName = dto.StageName,
                StageNameAr = dto.StageNameAr,
                StageType = dto.StageType,
                SortOrder = dto.SortOrder,
                ConfigJson = dto.ConfigJson,
                WaitMinutes = dto.WaitMinutes,
                WaitUntilTime = dto.WaitUntilTime,
                ConditionJson = dto.ConditionJson,
                NextStageIfTrue = dto.NextStageIfTrue,
                NextStageIfFalse = dto.NextStageIfFalse,
                NextStageId = dto.NextStageId,
                DynamicContentJson = dto.DynamicContentJson,
                AbTestJson = dto.AbTestJson,
                IsActive = dto.IsActive
            };

            Context.Insertable(stage).ExecuteReturnSnowflakeId();
            return stage;
        }

        /// <summary>
        /// Update a journey stage
        /// </summary>
        public int UpdateStage(JourneyStageDto dto)
        {
            var stage = Context.Queryable<MktJourneyStage>()
                .Where(s => s.StageId == dto.StageId)
                .First();

            if (stage == null) return 0;

            stage.StageName = dto.StageName;
            stage.StageNameAr = dto.StageNameAr;
            stage.StageType = dto.StageType;
            stage.SortOrder = dto.SortOrder;
            stage.ConfigJson = dto.ConfigJson;
            stage.WaitMinutes = dto.WaitMinutes;
            stage.WaitUntilTime = dto.WaitUntilTime;
            stage.ConditionJson = dto.ConditionJson;
            stage.NextStageIfTrue = dto.NextStageIfTrue;
            stage.NextStageIfFalse = dto.NextStageIfFalse;
            stage.NextStageId = dto.NextStageId;
            stage.DynamicContentJson = dto.DynamicContentJson;
            stage.AbTestJson = dto.AbTestJson;
            stage.IsActive = dto.IsActive;

            return Context.Updateable(stage).ExecuteCommand();
        }

        /// <summary>
        /// Delete a journey stage
        /// </summary>
        public int DeleteStage(long stageId)
        {
            return Context.Deleteable<MktJourneyStage>()
                .In(stageId)
                .ExecuteCommand();
        }

        #endregion

        #region Triggers

        /// <summary>
        /// Get triggers for a journey
        /// </summary>
        public List<JourneyTriggerDto> GetTriggers(long journeyId)
        {
            return Context.Queryable<MktJourneyTrigger>()
                .Where(t => t.JourneyId == journeyId)
                .OrderBy(t => t.Priority)
                .Select(t => new JourneyTriggerDto
                {
                    TriggerId = t.TriggerId,
                    JourneyId = t.JourneyId,
                    TriggerName = t.TriggerName,
                    EventType = t.EventType,
                    ConditionsJson = t.ConditionsJson,
                    ActionType = t.ActionType,
                    ActionParamsJson = t.ActionParamsJson,
                    TargetStageId = t.TargetStageId,
                    Priority = t.Priority,
                    IsActive = t.IsActive,
                    TimesTriggered = t.TimesTriggered
                })
                .ToList();
        }

        /// <summary>
        /// Add a trigger to a journey
        /// </summary>
        public MktJourneyTrigger AddTrigger(JourneyTriggerDto dto)
        {
            var trigger = new MktJourneyTrigger
            {
                JourneyId = dto.JourneyId,
                TriggerName = dto.TriggerName,
                EventType = dto.EventType,
                ConditionsJson = dto.ConditionsJson,
                ActionType = dto.ActionType,
                ActionParamsJson = dto.ActionParamsJson,
                TargetStageId = dto.TargetStageId,
                Priority = dto.Priority,
                IsActive = dto.IsActive
            };

            Context.Insertable(trigger).ExecuteReturnSnowflakeId();
            return trigger;
        }

        /// <summary>
        /// Update a journey trigger
        /// </summary>
        public int UpdateTrigger(JourneyTriggerDto dto)
        {
            var trigger = Context.Queryable<MktJourneyTrigger>()
                .Where(t => t.TriggerId == dto.TriggerId)
                .First();

            if (trigger == null) return 0;

            trigger.TriggerName = dto.TriggerName;
            trigger.EventType = dto.EventType;
            trigger.ConditionsJson = dto.ConditionsJson;
            trigger.ActionType = dto.ActionType;
            trigger.ActionParamsJson = dto.ActionParamsJson;
            trigger.TargetStageId = dto.TargetStageId;
            trigger.Priority = dto.Priority;
            trigger.IsActive = dto.IsActive;

            return Context.Updateable(trigger).ExecuteCommand();
        }

        #endregion

        #region Execution

        /// <summary>
        /// Enroll a member into a journey
        /// </summary>
        public MktJourneyExecution EnrollMember(EnrollJourneyRequest request)
        {
            // Check if member is already enrolled and active
            var existing = Context.Queryable<MktJourneyExecution>()
                .Where(e => e.JourneyId == request.JourneyId
                    && e.MemberId == request.MemberId
                    && e.Status == "active")
                .First();

            if (existing != null) return existing;

            // Get the first stage of the journey
            var firstStage = Context.Queryable<MktJourneyStage>()
                .Where(s => s.JourneyId == request.JourneyId && s.IsActive)
                .OrderBy(s => s.SortOrder)
                .First();

            var execution = new MktJourneyExecution
            {
                JourneyId = request.JourneyId,
                MemberType = request.MemberType,
                MemberId = request.MemberId,
                CurrentStageId = firstStage?.StageId,
                Status = "active",
                EnrolledAt = DateTime.Now,
                LastActivityAt = DateTime.Now
            };

            Context.Insertable(execution).ExecuteReturnSnowflakeId();

            // Update journey enrollment count
            Context.Updateable<MktCustomerJourney>()
                .SetColumns(j => j.TotalEnrolled == j.TotalEnrolled + 1)
                .SetColumns(j => j.CurrentlyActive == j.CurrentlyActive + 1)
                .Where(j => j.JourneyId == request.JourneyId)
                .ExecuteCommand();

            return execution;
        }

        /// <summary>
        /// Get journey executions with filtering
        /// </summary>
        public PagedInfo<JourneyExecutionDto> GetExecutions(JourneyExecutionQueryDto query)
        {
            var predicate = Expressionable.Create<MktJourneyExecution>();

            predicate = predicate.AndIF(query.JourneyId != null, it => it.JourneyId == query.JourneyId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.MemberType), it => it.MemberType == query.MemberType);
            predicate = predicate.AndIF(query.MemberId != null, it => it.MemberId == query.MemberId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.IsConverted != null, it => it.IsConverted == query.IsConverted);
            predicate = predicate.AndIF(query.EnrolledFrom != null, it => it.EnrolledAt >= query.EnrolledFrom);
            predicate = predicate.AndIF(query.EnrolledTo != null, it => it.EnrolledAt <= query.EnrolledTo);

            var response = Context.Queryable<MktJourneyExecution>()
                .Where(predicate.ToExpression())
                .OrderByDescending(e => e.EnrolledAt)
                .Select(e => new JourneyExecutionDto
                {
                    ExecutionId = e.ExecutionId,
                    JourneyId = e.JourneyId,
                    MemberType = e.MemberType,
                    MemberId = e.MemberId,
                    CurrentStageId = e.CurrentStageId,
                    Status = e.Status,
                    EnrolledAt = e.EnrolledAt,
                    CompletedAt = e.CompletedAt,
                    LastActivityAt = e.LastActivityAt,
                    StagesCompleted = e.StagesCompleted,
                    EngagementScore = e.EngagementScore,
                    PersonalizationProfileJson = e.PersonalizationProfileJson,
                    IsConverted = e.IsConverted,
                    ConvertedAt = e.ConvertedAt,
                    AttributedRevenue = e.AttributedRevenue
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Advance an execution to the next stage
        /// </summary>
        public int AdvanceExecution(long executionId)
        {
            var execution = Context.Queryable<MktJourneyExecution>()
                .Where(e => e.ExecutionId == executionId)
                .First();

            if (execution == null || execution.Status != "active") return 0;

            // Get current stage and find next stage
            if (execution.CurrentStageId.HasValue)
            {
                var currentStage = Context.Queryable<MktJourneyStage>()
                    .Where(s => s.StageId == execution.CurrentStageId)
                    .First();

                long? nextStageId = currentStage?.NextStageId;

                if (nextStageId == null && currentStage != null)
                {
                    // Get next stage by sort order
                    var nextStage = Context.Queryable<MktJourneyStage>()
                        .Where(s => s.JourneyId == execution.JourneyId
                            && s.SortOrder > currentStage.SortOrder
                            && s.IsActive)
                        .OrderBy(s => s.SortOrder)
                        .First();

                    nextStageId = nextStage?.StageId;
                }

                if (nextStageId.HasValue)
                {
                    return Context.Updateable<MktJourneyExecution>()
                        .SetColumns(e => e.CurrentStageId == nextStageId)
                        .SetColumns(e => e.StagesCompleted == e.StagesCompleted + 1)
                        .SetColumns(e => e.LastActivityAt == DateTime.Now)
                        .Where(e => e.ExecutionId == executionId)
                        .ExecuteCommand();
                }
                else
                {
                    // No next stage, mark as completed
                    return Context.Updateable<MktJourneyExecution>()
                        .SetColumns(e => e.Status == "completed")
                        .SetColumns(e => e.CompletedAt == DateTime.Now)
                        .SetColumns(e => e.StagesCompleted == e.StagesCompleted + 1)
                        .SetColumns(e => e.LastActivityAt == DateTime.Now)
                        .Where(e => e.ExecutionId == executionId)
                        .ExecuteCommand();
                }
            }

            return 0;
        }

        /// <summary>
        /// Pause an execution
        /// </summary>
        public int PauseExecution(long executionId)
        {
            return Context.Updateable<MktJourneyExecution>()
                .SetColumns(e => e.Status == "paused")
                .SetColumns(e => e.LastActivityAt == DateTime.Now)
                .Where(e => e.ExecutionId == executionId && e.Status == "active")
                .ExecuteCommand();
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get journey analytics summary
        /// </summary>
        public JourneyAnalyticsDto GetJourneyAnalytics()
        {
            var journeys = Queryable().ToList();
            var executions = Context.Queryable<MktJourneyExecution>().ToList();

            var totalConverted = executions.Count(e => e.IsConverted);
            var totalEnrolled = executions.Count;

            var analytics = new JourneyAnalyticsDto
            {
                TotalJourneys = journeys.Count,
                ActiveJourneys = journeys.Count(j => j.Status == "active"),
                TotalEnrolled = totalEnrolled,
                TotalConverted = totalConverted,
                OverallConversionRate = totalEnrolled > 0 ? (decimal)totalConverted / totalEnrolled * 100 : 0,
                OverallCompletionRate = totalEnrolled > 0
                    ? (decimal)executions.Count(e => e.Status == "completed") / totalEnrolled * 100 : 0,
                TotalAttributedRevenue = executions
                    .Where(e => e.AttributedRevenue.HasValue)
                    .Sum(e => e.AttributedRevenue.Value),
                AvgEngagementScore = executions
                    .Where(e => e.EngagementScore.HasValue)
                    .Select(e => e.EngagementScore.Value)
                    .DefaultIfEmpty(0)
                    .Average(),

                JourneyPerformance = journeys.Select(j =>
                {
                    var jExecs = executions.Where(e => e.JourneyId == j.JourneyId).ToList();
                    var jConverted = jExecs.Count(e => e.IsConverted);
                    return new JourneyPerformanceSummary
                    {
                        JourneyId = j.JourneyId,
                        JourneyName = j.JourneyName,
                        JourneyType = j.JourneyType,
                        Enrolled = jExecs.Count,
                        Completed = jExecs.Count(e => e.Status == "completed"),
                        Converted = jConverted,
                        ConversionRate = jExecs.Count > 0 ? (decimal)jConverted / jExecs.Count * 100 : 0,
                        Revenue = jExecs.Where(e => e.AttributedRevenue.HasValue).Sum(e => e.AttributedRevenue.Value)
                    };
                }).OrderByDescending(j => j.ConversionRate).ToList(),

                FunnelAnalysis = new List<StageFunnelDto>()
            };

            return analytics;
        }

        #endregion

        #region Dynamic Content

        /// <summary>
        /// Get dynamic content with filtering
        /// </summary>
        public PagedInfo<DynamicContentDto> GetDynamicContent(DynamicContentQueryDto query)
        {
            var predicate = Expressionable.Create<MktDynamicContent>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ContentName),
                it => it.ContentName.Contains(query.ContentName) || it.ContentNameAr.Contains(query.ContentName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ContentType), it => it.ContentType == query.ContentType);
            predicate = predicate.AndIF(query.IsActive != null, it => it.IsActive == query.IsActive);
            predicate = predicate.AndIF(query.AiOptimized != null, it => it.AiOptimized == query.AiOptimized);

            return Context.Queryable<MktDynamicContent>()
                .Where(predicate.ToExpression())
                .OrderByDescending(c => c.Create_time)
                .Select(c => new DynamicContentDto
                {
                    ContentId = c.ContentId,
                    ContentName = c.ContentName,
                    ContentNameAr = c.ContentNameAr,
                    ContentType = c.ContentType,
                    DefaultContent = c.DefaultContent,
                    DefaultContentAr = c.DefaultContentAr,
                    VariantsJson = c.VariantsJson,
                    TemplateVarsJson = c.TemplateVarsJson,
                    LanguageMode = c.LanguageMode,
                    AiOptimized = c.AiOptimized,
                    IsActive = c.IsActive
                })
                .ToPage(query);
        }

        /// <summary>
        /// Manage dynamic content (add or update)
        /// </summary>
        public MktDynamicContent ManageDynamicContent(DynamicContentDto dto)
        {
            if (dto.ContentId > 0)
            {
                var existing = Context.Queryable<MktDynamicContent>()
                    .Where(c => c.ContentId == dto.ContentId)
                    .First();

                if (existing != null)
                {
                    existing.ContentName = dto.ContentName;
                    existing.ContentNameAr = dto.ContentNameAr;
                    existing.ContentType = dto.ContentType;
                    existing.DefaultContent = dto.DefaultContent;
                    existing.DefaultContentAr = dto.DefaultContentAr;
                    existing.VariantsJson = dto.VariantsJson;
                    existing.TemplateVarsJson = dto.TemplateVarsJson;
                    existing.LanguageMode = dto.LanguageMode;
                    existing.AiOptimized = dto.AiOptimized;
                    existing.IsActive = dto.IsActive;

                    Context.Updateable(existing).ExecuteCommand();
                    return existing;
                }
            }

            var content = new MktDynamicContent
            {
                ContentName = dto.ContentName,
                ContentNameAr = dto.ContentNameAr,
                ContentType = dto.ContentType,
                DefaultContent = dto.DefaultContent,
                DefaultContentAr = dto.DefaultContentAr,
                VariantsJson = dto.VariantsJson,
                TemplateVarsJson = dto.TemplateVarsJson,
                LanguageMode = dto.LanguageMode ?? "bilingual",
                AiOptimized = dto.AiOptimized,
                IsActive = dto.IsActive
            };

            Context.Insertable(content).ExecuteReturnSnowflakeId();
            return content;
        }

        #endregion
    }
}
