using System.Threading.Tasks;

namespace ZR.Service.PMO.IPmoService
{
    /// <summary>Result of a Power BI "embed for your organization" token request.</summary>
    public class PowerBiEmbedResult
    {
        /// <summary>True when the service principal + workspace/report are configured (no YOUR_* placeholders).</summary>
        public bool Configured { get; set; }
        /// <summary>The report's embedUrl from the Power BI REST API.</summary>
        public string EmbedUrl { get; set; } = "";
        /// <summary>The target report id.</summary>
        public string ReportId { get; set; } = "";
        /// <summary>Short-lived embed token the browser SDK uses to render the report.</summary>
        public string Token { get; set; } = "";
        /// <summary>Token expiry (UTC, ISO-8601).</summary>
        public string Expiry { get; set; } = "";
        /// <summary>Human-readable reason when Configured is false or the token request failed.</summary>
        public string Error { get; set; } = "";
    }

    /// <summary>
    /// Mints Power BI embed tokens via an Azure AD service principal (client-credentials),
    /// so the React app can show the PMO report with "embed for your organization".
    /// Configure under appsettings "PowerBI"; real secrets belong in appsettings.Development.json.
    /// </summary>
    public interface IPmoPowerBiService
    {
        /// <summary>Whether the PowerBI config section holds real values (not YOUR_* placeholders).</summary>
        bool IsConfigured { get; }

        /// <summary>Acquire an AAD token, resolve the report's embedUrl and generate an embed token.</summary>
        Task<PowerBiEmbedResult> GetEmbedAsync();
    }
}
