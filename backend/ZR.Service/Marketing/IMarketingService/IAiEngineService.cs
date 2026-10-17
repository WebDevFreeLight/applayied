using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Marketing.AiEngine;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// AI Engine Service Interface - خدمة محرك الذكاء الاصطناعي
    /// </summary>
    public interface IAiEngineService : IBaseService<MktAudienceSegment>
    {
        /// <summary>
        /// Get audience segments with filtering
        /// </summary>
        PagedInfo<AudienceSegmentDto> GetSegments(AudienceSegmentQueryDto query);

        /// <summary>
        /// Add or update an audience segment
        /// </summary>
        MktAudienceSegment ManageSegments(AudienceSegmentDto dto);

        /// <summary>
        /// Get behavior predictions with filtering
        /// </summary>
        PagedInfo<BehaviorPredictionDto> GetPredictions(BehaviorPredictionQueryDto query);

        /// <summary>
        /// Generate a new behavior prediction
        /// </summary>
        MktBehaviorPrediction GeneratePrediction(GeneratePredictionRequest request);

        /// <summary>
        /// Get AI agents with filtering
        /// </summary>
        PagedInfo<AiAgentDto> GetAiAgents(AiAgentQueryDto query);

        /// <summary>
        /// Execute an AI agent task
        /// </summary>
        MktAiAgentTask ExecuteAgentTask(ExecuteAiTaskRequest request);

        /// <summary>
        /// Get competitor analysis entries with filtering
        /// </summary>
        PagedInfo<CompetitorAnalysisDto> GetCompetitorAnalysis(CompetitorAnalysisQueryDto query);

        /// <summary>
        /// Get budget optimization recommendations
        /// </summary>
        PagedInfo<BudgetOptimizationDto> GetBudgetOptimizations(BudgetOptimizationQueryDto query);

        /// <summary>
        /// Approve or reject a budget optimization
        /// </summary>
        int ApproveBudgetOptimization(ApproveBudgetOptimizationDto dto, long userId);
    }
}
