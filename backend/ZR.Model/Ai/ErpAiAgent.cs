using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.AI
{
    /// <summary>
    /// AI Agent - وكيل الذكاء الاصطناعي
    /// </summary>
    [SugarTable("erp_ai_agents")]
    [SqlDict(
        dictType: "erp_ai_agent",
        dictName: "AI Agent - وكيل ذكي",
        valueColumn: "agentid",
        labelColumns: "agentname",
        RemarkColumn = "description",
        OrderBy = "agentname ASC",
        Description = "Auto-generated from ErpAiAgent table"
    )]
    public class ErpAiAgent : SysBase
    {
        /// <summary>
        /// Agent ID - معرف الوكيل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AgentId { get; set; }

        /// <summary>
        /// Agent Name - اسم الوكيل
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// Agent Name Arabic - اسم الوكيل بالعربي
        /// </summary>
        public string AgentNameAr { get; set; }

        /// <summary>
        /// Agent Type - نوع الوكيل (thinking/planning/decision/learning/execution)
        /// </summary>
        public string AgentType { get; set; }

        /// <summary>
        /// Agent Mode ID - معرف نمط الوكيل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AgentModeId { get; set; }

        /// <summary>
        /// Model Provider ID - معرف مزود النموذج
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ModelProviderId { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Capabilities - القدرات (JSON)
        /// </summary>
        public string Capabilities { get; set; }

        /// <summary>
        /// Permissions - الصلاحيات (JSON)
        /// </summary>
        public string Permissions { get; set; }

        /// <summary>
        /// Configuration - الإعدادات (JSON)
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// System Prompt - النص التوجيهي
        /// </summary>
        public string SystemPrompt { get; set; }

        /// <summary>
        /// Temperature - درجة الحرارة
        /// </summary>
        public decimal Temperature { get; set; } = 0.7m;

        /// <summary>
        /// Max Tokens - الحد الأقصى للرموز
        /// </summary>
        public int MaxTokens { get; set; } = 4096;

        /// <summary>
        /// Is Auto Execute - تنفيذ تلقائي
        /// </summary>
        public bool IsAutoExecute { get; set; }

        /// <summary>
        /// Requires Approval - يتطلب موافقة
        /// </summary>
        public bool RequiresApproval { get; set; } = true;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
