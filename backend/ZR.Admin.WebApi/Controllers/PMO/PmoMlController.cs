using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZR.Service.PMO.IPmoService;
using ZR.ServiceCore.Middleware;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Machine-learning insights, served by the Python PMO ML service (ml/). This controller
    /// forwards requests, applies RBAC (finance-derived models need pmo:finance:list, exactly like
    /// the Finance tab) and passes the UI language through (Accept-Language -> ?lang=en|ar).
    /// </summary>
    [Route("pmo/ml")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoMlController : PmoBaseController
    {
        private readonly IPmoMlService _ml;
        public PmoMlController(IPmoMlService ml) => _ml = ml;

        private string Lang => Request.Headers.AcceptLanguage.ToString().StartsWith("ar") ? "ar" : "en";

        private Dictionary<string, string> Q(params (string key, string value)[] extra)
        {
            var q = new Dictionary<string, string> { ["lang"] = Lang };
            foreach (var (k, v) in extra) q[k] = v;
            return q;
        }

        /// <summary>ML service health: database target (no password), model versions, scheduler.</summary>
        [HttpGet("status")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> Status() => SUCCESS(await _ml.GetAsync("health"));

        /// <summary>Every model: definition, best algorithm, metrics vs baseline, chart paths.</summary>
        [HttpGet("models")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> Models() => SUCCESS(await _ml.GetAsync("models", Q()));

        /// <summary>Full metadata of one model (candidates, feature importance, confusion matrix ...).</summary>
        [HttpGet("models/{problem}")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> Model(string problem) => SUCCESS(await _ml.GetAsync($"models/{problem}", Q()));

        /// <summary>Delay probability for open tasks (optionally one project).</summary>
        [HttpGet("task-delay")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> TaskDelay([FromQuery] string projectId)
            => SUCCESS(await _ml.GetAsync("predict/task-delay", Q(("project_id", projectId))));

        /// <summary>Suggested Low / Medium / High severity for a risk being logged.</summary>
        [HttpPost("risk-severity")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> RiskSeverity([FromBody] PmoMlRiskDto dto)
            => SUCCESS(await _ml.PostAsync("predict/risk-severity", new
            {
                text = dto?.Text ?? "",
                kind = string.IsNullOrWhiteSpace(dto?.Kind) ? "Risk" : dto.Kind,
                project_id = long.TryParse(dto?.ProjectId, out var pid) ? pid : (long?)null,
                lang = Lang
            }));

        /// <summary>Next-month spend forecast per project (finance data -> finance permission).</summary>
        [HttpGet("spend-forecast")]
        [ActionPermissionFilter(Permission = "pmo:finance:list")]
        public async Task<IActionResult> SpendForecast([FromQuery] string projectId)
            => SUCCESS(await _ml.GetAsync("forecast/spend", Q(("project_id", projectId))));

        /// <summary>Most unusual posted cost transactions (finance permission).</summary>
        [HttpGet("cost-anomalies")]
        [ActionPermissionFilter(Permission = "pmo:finance:list")]
        public async Task<IActionResult> CostAnomalies([FromQuery] string projectId, [FromQuery] int top = 10)
            => SUCCESS(await _ml.GetAsync("anomalies/costs", Q(("project_id", projectId), ("top", top.ToString()))));

        /// <summary>Portfolio segments (KMeans) with a readable name per project.</summary>
        [HttpGet("segments")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> Segments() => SUCCESS(await _ml.GetAsync("segments", Q()));

        /// <summary>Drift (PSI), new data and live accuracy; whether retraining is recommended.</summary>
        [HttpGet("monitoring")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> Monitoring() => SUCCESS(await _ml.GetAsync("monitoring", Q()));

        /// <summary>Recent training runs (manual, API, scheduled).</summary>
        [HttpGet("history")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> History() => SUCCESS(await _ml.GetAsync("training/history"));

        /// <summary>Retrain now (all models, or the listed ones). Admin only; can take a minute.</summary>
        [HttpPost("train")]
        [ActionPermissionFilter(Permission = "pmo:ml:train")]
        public async Task<IActionResult> Train([FromBody] PmoMlTrainDto dto)
            => SUCCESS(await _ml.PostAsync("train", new { problems = dto?.Problems, eda = dto?.Eda ?? true }, longRunning: true));

        /// <summary>An EDA / evaluation chart (PNG) from the ML service, e.g. models/task_delay/&lt;v&gt;/roc_curve.png.</summary>
        [HttpGet("chart")]
        [ActionPermissionFilter(Permission = "pmo:ml:view")]
        public async Task<IActionResult> Chart([FromQuery] string path)
        {
            var bytes = await _ml.GetChartAsync(path);
            return bytes == null ? NotFound() : File(bytes, "image/png");
        }
    }

    public class PmoMlRiskDto
    {
        public string Text { get; set; }
        public string Kind { get; set; }
        public string ProjectId { get; set; }
    }

    public class PmoMlTrainDto
    {
        public List<string> Problems { get; set; }
        public bool? Eda { get; set; }
    }
}
