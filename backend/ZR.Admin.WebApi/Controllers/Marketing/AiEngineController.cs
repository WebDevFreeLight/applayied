using Microsoft.AspNetCore.Mvc;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Marketing.Dto;
using ZR.Model.Marketing.AiEngine;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// AI Marketing Engine Controller - محرك التسويق بالذكاء الاصطناعي
    /// Predictive targeting, AI agents, competitor analysis, budget optimization
    /// </summary>
    [Verify]
    [Route("api/marketing/ai")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class AiEngineController : BaseController
    {
        private readonly IAiEngineService _aiService;

        public AiEngineController(IAiEngineService aiService)
        {
            _aiService = aiService;
        }

        #region Audience Segments

        /// <summary>
        /// Get audience segments
        /// </summary>
        [HttpGet("segments")]
        [ActionPermissionFilter("marketing:ai:segments")]
        public IActionResult GetSegments([FromQuery] AudienceSegmentQueryDto dto)
        {
            var result = _aiService.GetSegments(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Create or update audience segment
        /// </summary>
        [HttpPost("segments")]
        [ActionPermissionFilter("marketing:ai:segments:add")]
        [Log(Title = "AI Segment", BusinessType = BusinessType.INSERT)]
        public IActionResult ManageSegment([FromBody] AudienceSegmentDto dto)
        {
            var result = _aiService.ManageSegments(dto);
            return SUCCESS(result);
        }

        #endregion

        #region Behavior Predictions

        /// <summary>
        /// Get predictions
        /// </summary>
        [HttpGet("predictions")]
        [ActionPermissionFilter("marketing:ai:predictions")]
        public IActionResult GetPredictions([FromQuery] BehaviorPredictionQueryDto dto)
        {
            var result = _aiService.GetPredictions(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate new prediction
        /// </summary>
        [HttpPost("predictions/generate")]
        [ActionPermissionFilter("marketing:ai:predictions:generate")]
        [Log(Title = "AI Prediction", BusinessType = BusinessType.INSERT)]
        public IActionResult GeneratePrediction([FromBody] GeneratePredictionRequest dto)
        {
            var result = _aiService.GeneratePrediction(dto);
            return SUCCESS(result);
        }

        #endregion

        #region AI Agents

        /// <summary>
        /// Get AI marketing agents
        /// </summary>
        [HttpGet("agents")]
        [ActionPermissionFilter("marketing:ai:agents")]
        public IActionResult GetAgents([FromQuery] AiAgentQueryDto dto)
        {
            var result = _aiService.GetAiAgents(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Execute AI agent task
        /// </summary>
        [HttpPost("agents/execute")]
        [ActionPermissionFilter("marketing:ai:agents:execute")]
        [Log(Title = "AI Task Execution", BusinessType = BusinessType.INSERT)]
        public IActionResult ExecuteAgentTask([FromBody] ExecuteAiTaskRequest dto)
        {
            var result = _aiService.ExecuteAgentTask(dto);
            return SUCCESS(result);
        }

        #endregion

        #region Competitor Analysis

        /// <summary>
        /// Get competitor analyses
        /// </summary>
        [HttpGet("competitors")]
        [ActionPermissionFilter("marketing:ai:competitors")]
        public IActionResult GetCompetitorAnalyses([FromQuery] CompetitorAnalysisQueryDto dto)
        {
            var result = _aiService.GetCompetitorAnalysis(dto);
            return SUCCESS(result);
        }

        #endregion

        #region Budget Optimization

        /// <summary>
        /// Get budget optimization recommendations
        /// </summary>
        [HttpGet("budget-optimizations")]
        [ActionPermissionFilter("marketing:ai:budget")]
        public IActionResult GetBudgetOptimizations([FromQuery] BudgetOptimizationQueryDto dto)
        {
            var result = _aiService.GetBudgetOptimizations(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve or reject budget optimization
        /// </summary>
        [HttpPost("budget-optimizations/approve")]
        [ActionPermissionFilter("marketing:ai:budget:approve")]
        [Log(Title = "Budget Optimization Approval", BusinessType = BusinessType.UPDATE)]
        public IActionResult ApproveBudgetOptimization([FromBody] ApproveBudgetOptimizationDto dto)
        {
            var userId = HttpContext.GetUId();
            var result = _aiService.ApproveBudgetOptimization(dto, userId);
            return ToResponse(result);
        }

        #endregion
    }
}
