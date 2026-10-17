using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.AI
{
    /// <summary>
    /// AI Agent Task - مهمة الوكيل الذكي
    /// </summary>
    [SugarTable("erp_ai_agent_tasks")]
    public class ErpAiAgentTask : SysBase
    {
        /// <summary>
        /// Task ID - معرف المهمة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }

        /// <summary>
        /// Agent ID - معرف الوكيل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AgentId { get; set; }

        /// <summary>
        /// Task Type - نوع المهمة
        /// </summary>
        public string TaskType { get; set; }

        /// <summary>
        /// Task Context - سياق المهمة (JSON)
        /// </summary>
        public string TaskContext { get; set; }

        /// <summary>
        /// Input Data - البيانات المدخلة (JSON)
        /// </summary>
        public string InputData { get; set; }

        /// <summary>
        /// Output Data - البيانات المخرجة (JSON)
        /// </summary>
        public string OutputData { get; set; }

        /// <summary>
        /// Status - الحالة (pending/processing/completed/failed/cancelled)
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Priority - الأولوية (1-10)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Started At - وقت البدء
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Completed At - وقت الاكتمال
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Processing Time MS - وقت المعالجة بالملي ثانية
        /// </summary>
        public int? ProcessingTimeMs { get; set; }

        /// <summary>
        /// Tokens Used - الرموز المستخدمة
        /// </summary>
        public int? TokensUsed { get; set; }

        /// <summary>
        /// Cost - التكلفة
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Error Message - رسالة الخطأ
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Retry Count - عدد المحاولات
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Max Retries - الحد الأقصى للمحاولات
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Scheduled At - وقت الجدولة
        /// </summary>
        public DateTime? ScheduledAt { get; set; }



        /// <summary>
        /// Requested By - طُلبت بواسطة
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}
