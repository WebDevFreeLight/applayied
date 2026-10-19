using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using JsonSerializer = System.Text.Json.JsonSerializer;
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
    /// Only providers with real API keys (not YOUR_*) are used. Adapted from the reference
    /// project's GrokAIService for the PMO portal domain.
    /// </summary>
    public class PmoAiService : IPmoAiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PmoAiService> _logger;
        private readonly List<AIProviderConfig> _providers;
        private static readonly TimeSpan ProviderTimeout = TimeSpan.FromSeconds(30);

        public PmoAiService(HttpClient httpClient, IConfiguration config, ILogger<PmoAiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var all = config.GetSection("AI:Providers").Get<List<AIProviderConfig>>() ?? new List<AIProviderConfig>();
            // Keep only providers with a real key (skip blanks and YOUR_* placeholders).
            _providers = all
                .Where(p => !string.IsNullOrWhiteSpace(p.ApiKey) && !p.ApiKey.StartsWith("YOUR_"))
                .ToList();

            _logger.LogInformation("[PMO-AI] Initialized with {Count} provider(s): {Names}",
                _providers.Count, string.Join(", ", _providers.Select(p => $"{p.Name}({p.Model})")));
        }

        public bool IsConfigured => _providers.Count > 0;

        public List<object> GetProviderStatus() =>
            _providers.Select((p, i) => (object)new { p.Name, p.Model, p.BaseUrl, Order = i + 1, IsDefault = i == 0 }).ToList();

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
        /// Tries each configured provider in order until one succeeds.
        /// </summary>
        private async Task<string> CallAIWithFallback(string systemPrompt, string userPrompt)
        {
            if (_providers.Count == 0)
            {
                _logger.LogWarning("[PMO-AI] No providers configured. Add API keys in appsettings.json AI:Providers");
                return null;
            }

            for (int i = 0; i < _providers.Count; i++)
            {
                var p = _providers[i];
                var result = await CallAIProvider(p.BaseUrl, p.ApiKey, p.Model, systemPrompt, userPrompt, p.Name);
                if (result != null) return result;

                if (i < _providers.Count - 1)
                    _logger.LogWarning("[PMO-AI] {Provider} failed, trying {Next}...", p.Name, _providers[i + 1].Name);
            }

            _logger.LogError("[PMO-AI] All {Count} providers failed", _providers.Count);
            return null;
        }

        private async Task<string> CallAIProvider(string baseUrl, string apiKey, string model, string systemPrompt, string userPrompt, string providerName)
        {
            try
            {
                using var cts = new CancellationTokenSource(ProviderTimeout);

                var requestBody = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.5
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/chat/completions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = content;

                // OpenRouter requires attribution headers.
                if (providerName.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.Add("HTTP-Referer", "https://pmo.portal");
                    request.Headers.Add("X-Title", "PMO Portal");
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.SendAsync(request, cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
                sw.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[PMO-AI] {Provider} error ({Elapsed}ms): {Status} - {Body}",
                        providerName, sw.ElapsedMilliseconds, response.StatusCode,
                        responseBody.Length > 400 ? responseBody[..400] : responseBody);
                    return null;
                }

                using var doc = JsonDocument.Parse(responseBody);
                var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                _logger.LogInformation("[PMO-AI] {Provider} OK in {Elapsed}ms (model: {Model})", providerName, sw.ElapsedMilliseconds, model);
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[PMO-AI] {Provider} timed out after {Timeout}s", providerName, ProviderTimeout.TotalSeconds);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PMO-AI] {Provider} call failed", providerName);
                return null;
            }
        }
    }
}
