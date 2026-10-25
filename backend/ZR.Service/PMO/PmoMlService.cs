using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>
    /// HTTP client for the PMO ML service. Configuration ("ML" section, env ML__BaseUrl ...):
    ///   BaseUrl              http://localhost:8001
    ///   ApiKey               optional shared secret sent as X-ML-Key (must match api.api_key in ml/config)
    ///   TimeoutSeconds       per prediction call (default 30)
    ///   TrainTimeoutSeconds  for POST /train (default 300)
    /// </summary>
    public class PmoMlService : IPmoMlService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PmoMlService> _logger;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly TimeSpan _timeout;
        private readonly TimeSpan _trainTimeout;

        public PmoMlService(HttpClient http, IConfiguration config, ILogger<PmoMlService> logger)
        {
            _http = http;
            _http.Timeout = Timeout.InfiniteTimeSpan;          // per-call timeouts below
            _logger = logger;
            _baseUrl = (config["ML:BaseUrl"] ?? "http://localhost:8001").TrimEnd('/');
            _apiKey = config["ML:ApiKey"];
            _timeout = TimeSpan.FromSeconds(config.GetValue("ML:TimeoutSeconds", 30));
            _trainTimeout = TimeSpan.FromSeconds(config.GetValue("ML:TrainTimeoutSeconds", 300));
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_baseUrl);

        public Task<JToken> GetAsync(string path, IDictionary<string, string> query = null, bool longRunning = false)
        {
            var qs = query == null ? "" : string.Join("&", query
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{_baseUrl}/{path.TrimStart('/')}" + (qs.Length > 0 ? "?" + qs : "");
            return SendAsync(new HttpRequestMessage(HttpMethod.Get, url), longRunning);
        }

        public Task<JToken> PostAsync(string path, object body, bool longRunning = false)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/{path.TrimStart('/')}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(body ?? new { }), Encoding.UTF8, "application/json")
            };
            return SendAsync(req, longRunning);
        }

        public async Task<byte[]> GetChartAsync(string reportPath)
        {
            // only files under /reports, no traversal
            if (string.IsNullOrWhiteSpace(reportPath) || reportPath.Contains("..") || !reportPath.EndsWith(".png"))
                return null;
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/reports/{reportPath.TrimStart('/')}");
            AddKey(req);
            try
            {
                using var cts = new CancellationTokenSource(_timeout);
                using var res = await _http.SendAsync(req, cts.Token);
                return res.IsSuccessStatusCode ? await res.Content.ReadAsByteArrayAsync(cts.Token) : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PMO-ML] chart {Path} failed", reportPath);
                return null;
            }
        }

        private void AddKey(HttpRequestMessage req)
        {
            if (!string.IsNullOrWhiteSpace(_apiKey)) req.Headers.Add("X-ML-Key", _apiKey);
        }

        private async Task<JToken> SendAsync(HttpRequestMessage req, bool longRunning)
        {
            AddKey(req);
            var timeout = longRunning ? _trainTimeout : _timeout;
            try
            {
                using var cts = new CancellationTokenSource(timeout);
                using var res = await _http.SendAsync(req, cts.Token);
                var body = await res.Content.ReadAsStringAsync(cts.Token);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[PMO-ML] {Method} {Url} -> {Status}: {Body}", req.Method, req.RequestUri,
                        (int)res.StatusCode, body.Length > 300 ? body[..300] : body);
                    return Unavailable($"ML service returned HTTP {(int)res.StatusCode}", body);
                }
                return JToken.Parse(body);
            }
            catch (OperationCanceledException)
            {
                return Unavailable($"ML service did not answer within {timeout.TotalSeconds:0}s");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PMO-ML] {Method} {Url} failed", req.Method, req.RequestUri);
                return Unavailable($"ML service unreachable at {_baseUrl} ({ex.GetType().Name})");
            }
            finally
            {
                req.Dispose();
            }
        }

        private static JToken Unavailable(string message, string detail = null) => JObject.FromObject(new
        {
            available = false,
            message,
            detail = string.IsNullOrWhiteSpace(detail) ? null : (detail.Length > 300 ? detail[..300] : detail)
        });
    }
}
