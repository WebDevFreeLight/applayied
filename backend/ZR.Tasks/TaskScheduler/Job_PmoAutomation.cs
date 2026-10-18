using System.Text;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Quartz;
using ZR.Service.PMO;
using ZR.Service.PMO.IPmoService;
using ZR.ServiceCore.Signalr;

namespace ZR.Tasks.TaskScheduler
{
    /// <summary>
    /// PMO business-automation scheduled job. Runs the idempotent automation rules
    /// (overdue-task escalation + budget-overrun alerts), writing to pmo_automation_run +
    /// pmo_outbox_notification, delivering each alert into the owner's Messages inbox and
    /// pushing it live over SignalR. The AI fallback chain drafts the notification bodies.
    ///
    /// Registered in sys_tasks as AssemblyName="ZR.Tasks", ClassName="TaskScheduler.Job_PmoAutomation"
    /// (seeded by PmoDataSeeder). Managed from the Scheduled Tasks page like any other job.
    /// </summary>
    [AppService(ServiceType = typeof(Job_PmoAutomation), ServiceLifetime = LifeTime.Scoped)]
    public class Job_PmoAutomation : JobBase, IJob
    {
        private readonly IPmoAiService _ai;
        private readonly IPmoNotifier _notifier;

        public Job_PmoAutomation(IPmoAiService ai, IPmoNotifier notifier)
        {
            _ai = ai;
            _notifier = notifier;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteJob(context, Run);
        }

        public Task<string> Run()
        {
            var runner = new PmoAutomationRunner(db: null, ai: _ai, useAi: true, notifier: _notifier);
            var results = runner.RunAll("scheduler");

            var sb = new StringBuilder();
            foreach (var r in results)
                sb.AppendLine($"{r.RuleKey}: {r.Status} (candidates={r.Candidates}, created={r.Created}, skipped={r.Skipped})");
            return Task.FromResult(sb.ToString().Trim());
        }
    }
}
