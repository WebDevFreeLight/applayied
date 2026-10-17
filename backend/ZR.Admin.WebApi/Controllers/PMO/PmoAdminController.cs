using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Service.PMO;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>Admin-only PMO operations (data seeding / setup). Guarded server-side by role.</summary>
    [Route("pmo/admin")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoAdminController : PmoBaseController
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public PmoAdminController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        /// <summary>
        /// Import the reproducible synthetic data package into the database (idempotent, transactional).
        /// Administrator only — a Contributor/PM calling this directly is refused by the role filter.
        /// </summary>
        [HttpPost("seed")]
        [PmoAuthorize(PmoRoleKeys.Admin)]
        public IActionResult Seed()
        {
            var csvDir = _config["PmoSeedPath"];
            if (string.IsNullOrWhiteSpace(csvDir))
                csvDir = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "..", "data", "csv"));

            var log = new PmoDataSeeder().Seed(csvDir);
            return SUCCESS(new { csvDir, log });
        }
    }
}
