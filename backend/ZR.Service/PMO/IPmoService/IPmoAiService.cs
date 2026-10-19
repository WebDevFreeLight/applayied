using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZR.Service.PMO.IPmoService
{
    /// <summary>
    /// Multi-provider AI gateway for the PMO portal. Mirrors the reference project's
    /// GrokAIService: an ordered fallback chain of OpenAI-compatible providers (Groq,
    /// OpenRouter, Cerebras, Gemini, SambaNova, Together, HuggingFace, Qwen, DeepSeek,
    /// Mistral, NVIDIA, Grok). The first provider that answers wins; the rest are tried
    /// only on failure. Configured in appsettings.json under "AI:Providers".
    /// </summary>
    public interface IPmoAiService
    {
        /// <summary>Names/models/order of the providers that hold a real API key.</summary>
        List<object> GetProviderStatus();

        /// <summary>True when at least one provider is configured with a real key.</summary>
        bool IsConfigured { get; }

        /// <summary>Low-level: run one system+user prompt through the fallback chain.</summary>
        Task<string> CompleteAsync(string systemPrompt, string userPrompt);

        /// <summary>Free-form PMO assistant chat (optionally grounded with context text).</summary>
        Task<string> ChatAsync(string message, string context = null, string lang = "en");

        /// <summary>Executive narrative for a single project given its computed metrics.</summary>
        Task<string> ProjectSummaryAsync(string factsJson, string lang = "en");

        /// <summary>Risk register analysis: themes, top exposures, mitigation advice.</summary>
        Task<string> RiskAnalysisAsync(string risksJson, string lang = "en");

        /// <summary>Portfolio-level insight for the executive dashboard KPIs.</summary>
        Task<string> DashboardInsightsAsync(string kpiJson, string lang = "en");

        /// <summary>Draft the body of an automation notification (used by the scheduler).</summary>
        Task<string> DraftNotificationAsync(string ruleKey, string factsJson, string lang = "en");

        /// <summary>Autofill a form: return ONLY a JSON object with the requested fields from a prompt.</summary>
        Task<string> AutofillAsync(string kind, string fields, string prompt, string lang = "en");
    }
}
