using System.Text.Json;

namespace ZR.Model.PMO.Dto
{
    /// <summary>
    /// Request payload for the AI endpoints. The frontend forwards the data it already loaded
    /// for display (project facts, risk register, dashboard KPIs) as <see cref="Facts"/>, so the
    /// server does not re-query. <see cref="Lang"/> selects the response language (en/ar).
    /// </summary>
    public class PmoAiRequestDto
    {
        /// <summary>Free-form user message (chat endpoint).</summary>
        public string Message { get; set; }

        /// <summary>Optional grounding text for chat.</summary>
        public string Context { get; set; }

        /// <summary>Arbitrary facts object (project/risks/KPIs) serialized to JSON for the prompt.</summary>
        public JsonElement Facts { get; set; }

        /// <summary>Response language: "en" (default) or "ar".</summary>
        public string Lang { get; set; } = "en";

        /// <summary>Autofill: the kind of form (e.g. "project", "risk", "change request").</summary>
        public string Kind { get; set; }

        /// <summary>Autofill: comma-separated field spec, e.g. "name, description, priority(Low|Medium|High)".</summary>
        public string Fields { get; set; }

        /// <summary>Serialize <see cref="Facts"/> to a compact JSON string for the prompt.</summary>
        public string FactsJson() =>
            Facts.ValueKind == JsonValueKind.Undefined || Facts.ValueKind == JsonValueKind.Null
                ? "{}"
                : Facts.GetRawText();
    }
}
