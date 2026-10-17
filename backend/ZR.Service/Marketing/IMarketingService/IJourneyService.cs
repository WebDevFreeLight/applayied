using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Marketing.Journeys;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Journey Service Interface - خدمة رحلة العميل
    /// </summary>
    public interface IJourneyService : IBaseService<MktCustomerJourney>
    {
        #region Journey CRUD

        /// <summary>
        /// Get journeys with filtering
        /// </summary>
        PagedInfo<JourneyOutputDto> GetJourneys(JourneyQueryDto query);

        /// <summary>
        /// Get journey detail with stages
        /// </summary>
        JourneyOutputDto GetJourneyDetail(long journeyId);

        /// <summary>
        /// Add a new journey
        /// </summary>
        MktCustomerJourney AddJourney(JourneyDto dto);

        /// <summary>
        /// Update an existing journey
        /// </summary>
        int UpdateJourney(JourneyDto dto);

        /// <summary>
        /// Delete journeys by IDs
        /// </summary>
        int DeleteJourneys(long[] ids);

        /// <summary>
        /// Update journey status
        /// </summary>
        int UpdateJourneyStatus(long journeyId, string status);

        #endregion

        #region Stages

        /// <summary>
        /// Get stages for a journey
        /// </summary>
        List<JourneyStageDto> GetStages(long journeyId);

        /// <summary>
        /// Add a stage to a journey
        /// </summary>
        MktJourneyStage AddStage(JourneyStageDto dto);

        /// <summary>
        /// Update a journey stage
        /// </summary>
        int UpdateStage(JourneyStageDto dto);

        /// <summary>
        /// Delete a journey stage
        /// </summary>
        int DeleteStage(long stageId);

        #endregion

        #region Triggers

        /// <summary>
        /// Get triggers for a journey
        /// </summary>
        List<JourneyTriggerDto> GetTriggers(long journeyId);

        /// <summary>
        /// Add a trigger to a journey
        /// </summary>
        MktJourneyTrigger AddTrigger(JourneyTriggerDto dto);

        /// <summary>
        /// Update a journey trigger
        /// </summary>
        int UpdateTrigger(JourneyTriggerDto dto);

        #endregion

        #region Execution

        /// <summary>
        /// Enroll a member into a journey
        /// </summary>
        MktJourneyExecution EnrollMember(EnrollJourneyRequest request);

        /// <summary>
        /// Get journey executions with filtering
        /// </summary>
        PagedInfo<JourneyExecutionDto> GetExecutions(JourneyExecutionQueryDto query);

        /// <summary>
        /// Advance an execution to the next stage
        /// </summary>
        int AdvanceExecution(long executionId);

        /// <summary>
        /// Pause an execution
        /// </summary>
        int PauseExecution(long executionId);

        #endregion

        #region Analytics

        /// <summary>
        /// Get journey analytics summary
        /// </summary>
        JourneyAnalyticsDto GetJourneyAnalytics();

        #endregion

        #region Dynamic Content

        /// <summary>
        /// Get dynamic content with filtering
        /// </summary>
        PagedInfo<DynamicContentDto> GetDynamicContent(DynamicContentQueryDto query);

        /// <summary>
        /// Manage dynamic content (add or update)
        /// </summary>
        MktDynamicContent ManageDynamicContent(DynamicContentDto dto);

        #endregion
    }
}
