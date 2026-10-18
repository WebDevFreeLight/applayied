using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZR.Service.PMO;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Framework
{
    /// <summary>
    /// Scheduled background worker that runs the PMO business-automation rules on an interval
    /// (configured under "PmoAutomation" in appsettings.json). This is the automatic trigger for
    /// the automation; the same engine can also be fired manually via POST /pmo/admin/run-automation.
    ///
    /// It is deliberately resilient: a failure in one tick is logged and the next tick retries, and
    /// the rules themselves are idempotent (unique dedupe_key on the outbox), so retries never
    /// resend a notification that already went out.
    /// </summary>
    public class PmoAutomationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<PmoAutomationWorker> _logger;

        public PmoAutomationWorker(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<PmoAutomationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_config.GetValue("PmoAutomation:Enabled", true))
            {
                _logger.LogInformation("[PMO-Automation] Disabled via config; worker idle.");
                return;
            }

            var interval = TimeSpan.FromMinutes(Math.Max(1, _config.GetValue("PmoAutomation:IntervalMinutes", 30)));
            var startupDelay = TimeSpan.FromSeconds(Math.Max(0, _config.GetValue("PmoAutomation:RunOnStartupDelaySeconds", 20)));

            try { await Task.Delay(startupDelay, stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                RunOnce("scheduler");
                try { await Task.Delay(interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        /// <summary>Execute all rules once in a fresh DI scope. Swallows errors (logged) so the loop survives.</summary>
        private void RunOnce(string triggeredBy)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var ai = scope.ServiceProvider.GetService<IPmoAiService>();
                var useAi = _config.GetValue("PmoAutomation:UseAi", true);

                var runner = new PmoAutomationRunner(db: null, ai: ai, useAi: useAi);
                var results = runner.RunAll(triggeredBy, ParseAsOf());

                foreach (var r in results)
                    _logger.LogInformation("[PMO-Automation] {Rule}: {Status} (candidates={Cand}, created={Created}, skipped={Skipped})",
                        r.RuleKey, r.Status, r.Candidates, r.Created, r.Skipped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PMO-Automation] Tick failed; will retry next interval.");
            }
        }

        /// <summary>Optional fixed "today" for demos ("PmoAutomation:AsOfDate" = yyyy-MM-dd); null = now.</summary>
        private DateTime? ParseAsOf()
        {
            var raw = _config["PmoAutomation:AsOfDate"];
            if (!string.IsNullOrWhiteSpace(raw) &&
                DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            return null;
        }
    }
}
