using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>Strongly-typed view of the appsettings "PowerBI" section.</summary>
    public class PowerBiOptions
    {
        public string TenantId { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string WorkspaceId { get; set; } = "";
        public string ReportId { get; set; } = "";
        public string AuthorityUrl { get; set; } = "https://login.microsoftonline.com/";
        public string Scope { get; set; } = "https://analysis.windows.net/powerbi/api/.default";
        public string ApiUrl { get; set; } = "https://api.powerbi.com/";
    }

    /// <summary>
    /// Power BI "embed for your organization" using an Azure AD service principal
    /// (OAuth2 client-credentials). Three REST hops: AAD token → report metadata → embed token.
    /// No Power BI tenant is bundled; set real values in appsettings.Development.json (git-ignored)
    /// or env vars (PowerBI__ClientSecret=…). With placeholders left in place the frontend shows
    /// a configuration panel instead of a live report.
    /// </summary>
    public class PmoPowerBiService : IPmoPowerBiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PmoPowerBiService> _logger;
        private readonly PowerBiOptions _o;

        public PmoPowerBiService(HttpClient http, IConfiguration config, ILogger<PmoPowerBiService> logger)
        {
            _http = http;
            _logger = logger;
            _o = config.GetSection("PowerBI").Get<PowerBiOptions>() ?? new PowerBiOptions();
        }

        private static bool Real(string v) => !string.IsNullOrWhiteSpace(v) && !v.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);

        public bool IsConfigured =>
            Real(_o.TenantId) && Real(_o.ClientId) && Real(_o.ClientSecret) && Real(_o.WorkspaceId) && Real(_o.ReportId);

        public async Task<PowerBiEmbedResult> GetEmbedAsync()
        {
            var result = new PowerBiEmbedResult { ReportId = _o.ReportId, Configured = IsConfigured };
            if (!IsConfigured)
            {
                result.Error = "Power BI is not configured. Set PowerBI:TenantId/ClientId/ClientSecret/WorkspaceId/ReportId in appsettings.Development.json.";
                return result;
            }

            try
            {
                var aad = await AcquireAadTokenAsync();
                if (string.IsNullOrEmpty(aad))
                {
                    result.Error = "Could not acquire an Azure AD token for the service principal.";
                    return result;
                }

                var apiBase = _o.ApiUrl.TrimEnd('/');
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aad);

                // 1) report metadata -> embedUrl
                var metaResp = await _http.GetAsync($"{apiBase}/v1.0/myorg/groups/{_o.WorkspaceId}/reports/{_o.ReportId}");
                var metaJson = await metaResp.Content.ReadAsStringAsync();
                if (!metaResp.IsSuccessStatusCode)
                {
                    result.Error = $"Power BI report lookup failed ({(int)metaResp.StatusCode}). {Truncate(metaJson)}";
                    return result;
                }
                using (var meta = JsonDocument.Parse(metaJson))
                {
                    if (meta.RootElement.TryGetProperty("embedUrl", out var eu)) result.EmbedUrl = eu.GetString() ?? "";
                }

                // 2) generate an embed token (View)
                var body = new StringContent("{\"accessLevel\":\"View\"}", Encoding.UTF8);
                body.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var tokResp = await _http.PostAsync($"{apiBase}/v1.0/myorg/groups/{_o.WorkspaceId}/reports/{_o.ReportId}/GenerateToken", body);
                var tokJson = await tokResp.Content.ReadAsStringAsync();
                if (!tokResp.IsSuccessStatusCode)
                {
                    result.Error = $"Power BI GenerateToken failed ({(int)tokResp.StatusCode}). {Truncate(tokJson)}";
                    return result;
                }
                using (var tok = JsonDocument.Parse(tokJson))
                {
                    if (tok.RootElement.TryGetProperty("token", out var t)) result.Token = t.GetString() ?? "";
                    if (tok.RootElement.TryGetProperty("expiration", out var x)) result.Expiry = x.GetString() ?? "";
                }

                if (string.IsNullOrEmpty(result.Token)) result.Error = "Power BI returned no embed token.";
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PMO-PowerBI] embed token request failed");
                result.Error = "Power BI embed request failed: " + ex.Message;
                return result;
            }
        }

        private async Task<string> AcquireAadTokenAsync()
        {
            var authority = _o.AuthorityUrl.TrimEnd('/');
            var form = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", _o.ClientId),
                new("client_secret", _o.ClientSecret),
                new("scope", _o.Scope),
            });
            var resp = await _http.PostAsync($"{authority}/{_o.TenantId}/oauth2/v2.0/token", form);
            var json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("[PMO-PowerBI] AAD token failed ({Status}): {Body}", (int)resp.StatusCode, Truncate(json));
                return null;
            }
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("access_token", out var at) ? at.GetString() : null;
        }

        private static string Truncate(string s) => string.IsNullOrEmpty(s) ? "" : (s.Length > 300 ? s.Substring(0, 300) : s);
    }
}
