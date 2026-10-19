using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZR.Service.PMO.IPmoService;
using ZR.ServiceCore.Middleware;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Power BI reporting. Serves "embed for your organization" tokens minted by a backend
    /// Azure AD service principal (see appsettings "PowerBI"), so the React Reports page can
    /// render the PMO report that reads from the same PostgreSQL (MahamDB) as the app.
    /// </summary>
    [Route("pmo/powerbi")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoPowerBiController : PmoBaseController
    {
        private readonly IPmoPowerBiService _pbi;
        public PmoPowerBiController(IPmoPowerBiService pbi) => _pbi = pbi;

        /// <summary>Whether Power BI embedding is configured (service principal + workspace/report set).</summary>
        [HttpGet("status")]
        [ActionPermissionFilter(Permission = "pmo:report:view")]
        public IActionResult Status() => SUCCESS(new { configured = _pbi.IsConfigured });

        /// <summary>Embed config for the browser SDK: embedUrl, reportId, a short-lived token and its expiry.</summary>
        [HttpGet("embed")]
        [ActionPermissionFilter(Permission = "pmo:report:view")]
        public async Task<IActionResult> Embed() => SUCCESS(await _pbi.GetEmbedAsync());
    }
}
