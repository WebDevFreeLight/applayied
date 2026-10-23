using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonException = System.Text.Json.JsonException;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>One AI provider entry (OpenAI-compatible chat/completions endpoint).</summary>
    public class AIProviderConfig
    {
        public string Name { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public string Model { get; set; } = "";
    }

    /// <summary>
    /// AI service with automatic multi-provider fallback chain. Tries each configured provider
    /// in order until one succeeds. Supported free/cheap providers (all OpenAI-compatible):
    /// Groq, OpenRouter, Cerebras, GoogleGemini, SambaNova, Together, HuggingFace, Qwen,
    /// DeepSeek, Mistral, NVIDIA, Grok (x.ai). Configure in appsettings.json under "AI:Providers".
    /// Only providers with real API keys (not YOUR_*) are used. A key can also be supplied by
    /// provider name via "AI:Keys:&lt;Name&gt;" (env AI__Keys__Groq=...), which is the easy way on a
    /// cloud host. Adapted from the reference project's GrokAIService for the PMO portal domain.
    /// </summary>
    public class PmoAiService : IPmoAiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PmoAiService> _logger;
        private readonly List<AIProviderConfig> _providers = new();
        private readonly List<(string Name, string Reason)> _skipped = new();
        private readonly TimeSpan _providerTimeout;
        private readonly TimeSpan _totalTimeout;
        private readonly int _maxTokens;

        // Providers that failed with a non-transient error (bad/expired key, no credit, retired model)
        // are parked for a while so each request doesn't wait on them again. Static because the typed
        // HttpClient registration makes this service transient.
        private static readonly ConcurrentDictionary<string, (DateTime Until, string Reason)> Cooldown = new();
        private static readonly TimeSpan HardFailureCooldown = TimeSpan.FromMinutes(10);

        private readonly record struct Attempt(string Text, string Error, bool Permanent);

        public PmoAiService(HttpClient httpClient, IConfiguration config, ILogger<PmoAiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Defaults keep the whole chain under the frontend's 60s request timeout.
            _providerTimeout = TimeSpan.FromSeconds(config.GetValue("AI:ProviderTimeoutSeconds", 20));
            _totalTimeout = TimeSpan.FromSeconds(config.GetValue("AI:TotalTimeoutSeconds", 50));
            // Without max_tokens some providers (OpenRouter) reserve the model's full output window
            // and reject the call with 402 when the account can't cover it.
            _maxTokens = config.GetValue("AI:MaxTokens", 1500);

            var all = config.GetSection("AI:Providers").Get<List<AIProviderConfig>>() ?? new List<AIProviderConfig>();
            foreach (var p in all)
            {
                var keyByName = string.IsNullOrWhiteSpace(p.Name) ? null : config[$"AI:Keys:{p.Name}"];
                if (!string.IsNullOrWhiteSpace(keyByName)) p.ApiKey = keyByName.Trim();

                var reason = SkipReason(p);
                if (reason == null) _providers.Add(p);
                else _skipped.Add((p.Name, reason));
            }

            _logger.LogInformation("[PMO-AI] Initialized with {Count} provider(s): {Names}",
                _providers.Count, string.Join(", ", _providers.Select(p => $"{p.Name}({p.Model})")));
            if (_providers.Count == 0)
                _logger.LogWarning("[PMO-AI] No usable AI provider. Set a key such as AI__Keys__Groq. Skipped: {Skipped}",
                    string.Join(", ", _skipped.Select(s => $"{s.Name} ({s.Reason})")));
        }

        public bool IsConfigured => _providers.Count > 0;

        public string LastError { get; private set; }

        public List<object> GetProviderStatus() =>
            _providers.Select((p, i) =>
            {
                var parked = Cooldown.TryGetValue(CooldownKey(p), out var cd) && cd.Until > DateTime.UtcNow;
                return (object)new
                {
                    p.Name, p.Model, p.BaseUrl, Order = i + 1, IsDefault = i == 0,
                    CoolingDownUntil = parked ? cd.Until : (DateTime?)null,
                    LastFailure = parked ? cd.Reason : null
                };
            }).ToList();

        public List<object> GetSkippedProviders() =>
            _skipped.Select(s => (object)new { s.Name, s.Reason }).ToList();

        private static string SkipReason(AIProviderConfig p)
        {
            if (string.IsNullOrWhiteSpace(p.ApiKey)) return "no API key";
            if (p.ApiKey.StartsWith("YOUR_", StringComparison.Ordinal)) return "placeholder API key";
            if (string.IsNullOrWhiteSpace(p.BaseUrl)) return "no BaseUrl";
            if (string.IsNullOrWhiteSpace(p.Model)) return "no Model";
            return null;
        }

        // ==================== PMO feature prompts ====================

        public Task<string> ChatAsync(string message, string context = null, string lang = "en")
        {
            var sys = Sys(lang,
                "You are the AI assistant inside a PMO (Project Management Office) portal for project portfolio and financial control. " +
                "Answer concisely and practically about projects, tasks, milestones, risks, change requests, budgets and cost control. " +
                "Prefer short paragraphs and bullet points.",
                "أنت المساعد الذكي داخل بوابة مكتب إدارة المشاريع (PMO) لإدارة محفظة المشاريع والتحكم المالي. " +
                "أجب باختصار وبشكل عملي حول المشاريع والمهام والمعالم والمخاطر وطلبات التغيير والميزانيات وضبط التكاليف.");
            var user = string.IsNullOrWhiteSpace(context) ? message : $"Context:\n{context}\n\nQuestion:\n{message}";
            return CallAIWithFallback(sys, user);
        }

        public Task<string> ProjectSummaryAsync(string factsJson, string lang = "en")
        {
            var sys = Sys(lang,
                "You are a senior PMO analyst. Given a project's facts as JSON, write a crisp executive summary: " +
                "overall health (one line), schedule status, budget/cost status, top risks, and 2-3 recommended actions. Use headings.",
                "أنت محلل أول في مكتب إدارة المشاريع. من بيانات المشروع بصيغة JSON اكتب ملخصاً تنفيذياً موجزاً: " +
                "الصحة العامة، حالة الجدول الزمني، حالة الميزانية/التكلفة، أبرز المخاطر، و2-3 إجراءات موصى بها. استخدم عناوين.");
            return CallAIWithFallback(sys, factsJson);
        }

        public Task<string> RiskAnalysisAsync(string risksJson, string lang = "en")
        {
            var sys = Sys(lang,
                "You are a risk management expert. Given a project's risk register as JSON, identify recurring themes, " +
                "rank the top exposures (probability × impact), and give concrete mitigation recommendations. Be specific and brief.",
                "أنت خبير في إدارة المخاطر. من سجل المخاطر بصيغة JSON حدد الأنماط المتكررة، ورتّب أعلى التعرضات (الاحتمال × الأثر)، " +
                "وقدم توصيات تخفيف محددة. كن دقيقاً وموجزاً.");
            return CallAIWithFallback(sys, risksJson);
        }

        public Task<string> DashboardInsightsAsync(string kpiJson, string lang = "en")
        {
            var sys = Sys(lang,
                "You are a portfolio director. Given portfolio KPIs as JSON, give a 4-6 bullet executive briefing: " +
                "what is on track, what needs attention, budget/overrun outlook, and one priority for this week.",
                "أنت مدير محفظة مشاريع. من مؤشرات الأداء بصيغة JSON قدم إحاطة تنفيذية من 4-6 نقاط: " +
                "ما هو ضمن المسار، وما يحتاج إلى انتباه، وتوقعات الميزانية/التجاوز، وأولوية واحدة لهذا الأسبوع.");
            return CallAIWithFallback(sys, kpiJson);
        }

        public Task<string> DraftNotificationAsync(string ruleKey, string factsJson, string lang = "en")
        {
            var sys = Sys(lang,
                $"You write short, professional in-app notifications for a PMO portal. Rule: {ruleKey}. " +
                "Given the facts as JSON, write a 1-2 sentence notification body telling the project owner what happened and the single action to take. No greeting, no signature.",
                $"تكتب إشعارات داخلية قصيرة ومهنية لبوابة مكتب إدارة المشاريع. القاعدة: {ruleKey}. " +
                "من البيانات بصيغة JSON اكتب نص إشعار من جملة أو جملتين يخبر مالك المشروع بما حدث والإجراء الواجب اتخاذه. بدون تحية أو توقيع.");
            return CallAIWithFallback(sys, factsJson);
        }

        public async Task<string> AutofillAsync(string kind, string fields, string prompt, string lang = "en")
        {
            var sys = Sys(lang,
                $"You fill in a PMO \"{kind}\" form. Given the user's short description, return ONLY a compact JSON object " +
                $"whose keys are exactly these fields: {fields}. Use realistic, professional, concise values. For a field " +
                "written as name(A|B|C), the value MUST be one of the listed options. Output JSON only — no markdown, no code fences, no commentary.",
                $"تملأ نموذج \"{kind}\" في نظام إدارة المشاريع. من وصف المستخدم المختصر، أعِد فقط كائن JSON مضغوط " +
                $"مفاتيحه هي هذه الحقول بالضبط: {fields}. استخدم قيمًا واقعية ومهنية وموجزة. الحقل المكتوب name(A|B|C) " +
                "يجب أن تكون قيمته إحدى الخيارات المذكورة. أعِد JSON فقط — بدون أي تنسيق أو تعليقات.");
            var raw = await CallAIWithFallback(sys, prompt);
            return CleanJson(raw);
        }

        /// <summary>Strip markdown fences / surrounding prose so only the JSON object remains.</summary>
        private static string CleanJson(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            var start = s.IndexOf('{');
            var end = s.LastIndexOf('}');
            return (start >= 0 && end > start) ? s.Substring(start, end - start + 1) : s;
        }

        public Task<string> CompleteAsync(string systemPrompt, string userPrompt)
            => CallAIWithFallback(systemPrompt, userPrompt);

        private static string Sys(string lang, string en, string ar)
            => string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase) ? ar : en;

        // ==================== Fallback engine ====================

        /// <summary>
        /// Call AI with automatic multi-provider fallback chain.
        /// Tries each configured provider in order until one succeeds. On total failure returns null
        /// and sets <see cref="LastError"/> to a per-provider account of what went wrong.
        /// </summary>
        private async Task<string> CallAIWithFallback(string systemPrompt, string userPrompt)
        {
            LastError = null;
            if (_providers.Count == 0)
            {
                LastError = "no AI provider is configured on the server (set an API key, e.g. AI__Keys__Groq)";
                _logger.LogWarning("[PMO-AI] {Error}", LastError);
                return null;
            }

            var failures = new List<string>();
            var notTried = new List<string>();
            var budget = Stopwatch.StartNew();
            foreach (var p in _providers)
            {
                var key = CooldownKey(p);
                if (Cooldown.TryGetValue(key, out var cd) && cd.Until > DateTime.UtcNow)
                {
                    failures.Add($"{p.Name}: {cd.Reason} (parked until {cd.Until:HH:mm} UTC)");
                    continue;
                }

                var remaining = _totalTimeout - budget.Elapsed;
                if (remaining < TimeSpan.FromSeconds(1))
                {
                    notTried.Add(p.Name);
                    continue;
                }

                var attempt = await CallAIProvider(p, systemPrompt, userPrompt, remaining < _providerTimeout ? remaining : _providerTimeout);
                if (attempt.Text != null)
                {
                    Cooldown.TryRemove(key, out _);
                    return attempt.Text;
                }

                failures.Add($"{p.Name}: {attempt.Error}");
                if (attempt.Permanent) Cooldown[key] = (DateTime.UtcNow + HardFailureCooldown, attempt.Error);
            }

            if (notTried.Count > 0)
                failures.Add($"not tried, {_totalTimeout.TotalSeconds:0}s time budget used up: {string.Join(", ", notTried)}");
            LastError = $"all {_providers.Count} AI provider(s) failed — {string.Join("; ", failures)}";
            _logger.LogError("[PMO-AI] {Error}", LastError);
            return null;
        }

        private async Task<Attempt> CallAIProvider(AIProviderConfig p, string systemPrompt, string userPrompt, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var cts = new CancellationTokenSource(timeout);

                var requestBody = new
                {
                    model = p.Model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.5,
                    max_tokens = _maxTokens
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{p.BaseUrl.TrimEnd('/')}/chat/completions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", p.ApiKey);
                request.Content = content;

                // OpenRouter requires attribution headers.
                if (p.Name.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.Add("HTTP-Referer", "https://pmo.portal");
                    request.Headers.Add("X-Title", "PMO Portal");
                }

                using var response = await _httpClient.SendAsync(request, cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var code = (int)response.StatusCode;
                    var error = $"HTTP {code} {ProviderMessage(responseBody)}";
                    _logger.LogWarning("[PMO-AI] {Provider} ({Model}) failed in {Elapsed}ms: {Error}",
                        p.Name, p.Model, sw.ElapsedMilliseconds, error);
                    // Bad/expired key, no credit, unknown or retired model: retrying soon won't help.
                    return new Attempt(null, error, code is 401 or 402 or 403 or 404 or 410);
                }

                var text = ReadContent(responseBody);
                if (string.IsNullOrWhiteSpace(text))
                {
                    const string error = "response had no message content";
                    _logger.LogWarning("[PMO-AI] {Provider} ({Model}) {Error}: {Body}",
                        p.Name, p.Model, error, Clip(responseBody, 300));
                    return new Attempt(null, error, false);
                }

                _logger.LogInformation("[PMO-AI] {Provider} OK in {Elapsed}ms (model: {Model})", p.Name, sw.ElapsedMilliseconds, p.Model);
                return new Attempt(text, null, false);
            }
            catch (OperationCanceledException)
            {
                var error = $"timed out after {timeout.TotalSeconds:0}s";
                _logger.LogWarning("[PMO-AI] {Provider} ({Model}) {Error}", p.Name, p.Model, error);
                return new Attempt(null, error, false);
            }
            catch (Exception ex)
            {
                var error = $"{ex.GetType().Name}: {Clip(ex.Message, 160)}";
                _logger.LogWarning(ex, "[PMO-AI] {Provider} ({Model}) call failed", p.Name, p.Model);
                return new Attempt(null, error, false);
            }
        }

        /// <summary>choices[0].message.content, or null when the body isn't a chat completion.</summary>
        private static string ReadContent(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                return doc.RootElement.TryGetProperty("choices", out var choices)
                       && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0
                       && choices[0].TryGetProperty("message", out var message)
                       && message.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String
                    ? c.GetString()
                    : null;
            }
            catch (JsonException) { return null; }
        }

        /// <summary>The human-readable error from a provider's error body (shapes differ per vendor).</summary>
        private static string ProviderMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return "(empty response)";
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                // Gemini wraps its error object in an array.
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) root = root[0];
                var msg = Str(root, "error", "message") ?? Str(root, "message") ?? Str(root, "detail")
                          ?? Str(root, "error") ?? Str(root, "title");
                if (msg != null) return Clip(msg, 160);
            }
            catch (JsonException) { }
            return Clip(body, 160);
        }

        private static string Str(JsonElement e, params string[] path)
        {
            foreach (var k in path)
                if (e.ValueKind != JsonValueKind.Object || !e.TryGetProperty(k, out e)) return null;
            return e.ValueKind == JsonValueKind.String ? e.GetString() : null;
        }

        private static string Clip(string s, int max)
        {
            s = Regex.Replace(s ?? "", @"\s+", " ").Trim();
            return s.Length > max ? s[..max] + "…" : s;
        }

        private static string CooldownKey(AIProviderConfig p) => $"{p.Name}|{p.BaseUrl}|{p.Model}|{p.ApiKey}";
    }
}
