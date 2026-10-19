using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;
using ZR.ServiceCore.Middleware;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// PMO AI assistant. Backed by a multi-provider fallback chain (Groq → OpenRouter → Cerebras →
    /// Gemini → …) configured in appsettings.json "AI:Providers". The frontend forwards the data it
    /// already displays (project facts, risk register, dashboard KPIs); the server never re-queries.
    /// </summary>
    [Route("pmo/ai")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoAiController : PmoBaseController
    {
        private readonly IPmoAiService _ai;
        public PmoAiController(IPmoAiService ai) => _ai = ai;

        /// <summary>Which providers are configured (names/models/order) and whether AI is available.</summary>
        [HttpGet("status")]
        [ActionPermissionFilter(Permission = "pmo:ai:use")]
        public IActionResult Status() => SUCCESS(new { configured = _ai.IsConfigured, providers = _ai.GetProviderStatus() });

        /// <summary>Free-form assistant chat about the portfolio.</summary>
        [HttpPost("chat")]
        [ActionPermissionFilter(Permission = "pmo:ai:use")]
        public async Task<IActionResult> Chat([FromBody] PmoAiRequestDto dto)
            => Reply(await _ai.ChatAsync(dto.Message, dto.Context, dto.Lang));

        /// <summary>Executive summary for one project from its displayed facts.</summary>
        [HttpPost("project-summary")]
        [ActionPermissionFilter(Permission = "pmo:ai:use")]
        public async Task<IActionResult> ProjectSummary([FromBody] PmoAiRequestDto dto)
            => Reply(await _ai.ProjectSummaryAsync(dto.FactsJson(), dto.Lang));

        /// <summary>Risk-register analysis for a project.</summary>
        [HttpPost("risk-analysis")]
        [ActionPermissionFilter(Permission = "pmo:ai:use")]
        public async Task<IActionResult> RiskAnalysis([FromBody] PmoAiRequestDto dto)
            => Reply(await _ai.RiskAnalysisAsync(dto.FactsJson(), dto.Lang));

        /// <summary>Portfolio insight for the executive dashboard KPIs.</summary>
        [HttpPost("dashboard-insights")]
        [ActionPermissionFilter(Permission = "pmo:ai:use")]
        public async Task<IActionResult> DashboardInsights([FromBody] PmoAiRequestDto dto)
            => Reply(await _ai.DashboardInsightsAsync(dto.FactsJson(), dto.Lang));

        /// <summary>Autofill a form from a short description — returns a JSON object of field values.</summary>
        [HttpPost("autofill")]
        [ActionPermissionFilter(Permission = "pmo:ai:use")]
        public async Task<IActionResult> Autofill([FromBody] PmoAiRequestDto dto)
            => Reply(await _ai.AutofillAsync(dto.Kind, dto.Fields, dto.Message, dto.Lang));

        // All providers failed / none configured -> 200 with a clear message so the UI can show it.
        private IActionResult Reply(string text) => SUCCESS(new
        {
            ok = !string.IsNullOrWhiteSpace(text),
            text = string.IsNullOrWhiteSpace(text)
                ? "AI is unavailable right now (no provider responded). Please try again later."
                : text
        });
    }
}
