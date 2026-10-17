using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.AI
{
    /// <summary>
    /// AI Thinking Log - سجل التفكير الذكي
    /// </summary>
    [SugarTable("erp_ai_thinking_logs")]
    public class ErpAiThinkingLog : SysBase
    {
        /// <summary>
        /// Log ID - معرف السجل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LogId { get; set; }

        /// <summary>
        /// Agent ID - معرف الوكيل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AgentId { get; set; }

        /// <summary>
        /// Task ID - معرف المهمة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TaskId { get; set; }

        /// <summary>
        /// Thinking Step - خطوة التفكير
        /// </summary>
        public int ThinkingStep { get; set; }

        /// <summary>
        /// Thought Type - نوع الفكرة (analysis/reasoning/conclusion/observation)
        /// </summary>
        public string ThoughtType { get; set; }

        /// <summary>
        /// Thought Content - محتوى الفكرة
        /// </summary>
        public string ThoughtContent { get; set; }

        /// <summary>
        /// Confidence Score - درجة الثقة (0-100)
        /// </summary>
        public decimal ConfidenceScore { get; set; }

        /// <summary>
        /// Data Sources - مصادر البيانات (JSON)
        /// </summary>
        public string DataSources { get; set; }

        /// <summary>
        /// Entities Referenced - الكيانات المرجعية (JSON)
        /// </summary>
        public string EntitiesReferenced { get; set; }

        /// <summary>
        /// Duration MS - المدة بالملي ثانية
        /// </summary>
        public int? DurationMs { get; set; }

        /// <summary>
        /// Is Key Insight - رؤية رئيسية
        /// </summary>
        public bool IsKeyInsight { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}
