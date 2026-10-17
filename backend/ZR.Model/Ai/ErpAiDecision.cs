using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.AI
{
    /// <summary>
    /// AI Decision - قرار الوكيل الذكي
    /// </summary>
    [SugarTable("erp_ai_decisions")]
    public class ErpAiDecision : SysBase
    {
        /// <summary>
        /// Decision ID - معرف القرار
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DecisionId { get; set; }

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
        /// Decision Type - نوع القرار
        /// </summary>
        public string DecisionType { get; set; }

        /// <summary>
        /// Decision Context - سياق القرار (JSON)
        /// </summary>
        public string DecisionContext { get; set; }

        /// <summary>
        /// Options Considered - الخيارات المدروسة (JSON)
        /// </summary>
        public string OptionsConsidered { get; set; }

        /// <summary>
        /// Selected Option - الخيار المختار
        /// </summary>
        public string SelectedOption { get; set; }

        /// <summary>
        /// Reasoning - التبرير
        /// </summary>
        public string Reasoning { get; set; }

        /// <summary>
        /// Confidence Score - درجة الثقة (0-100)
        /// </summary>
        public decimal ConfidenceScore { get; set; }

        /// <summary>
        /// Impact Assessment - تقييم التأثير (JSON)
        /// </summary>
        public string ImpactAssessment { get; set; }

        /// <summary>
        /// Auto Executed - نُفذ تلقائياً
        /// </summary>
        public bool AutoExecuted { get; set; }

        /// <summary>
        /// User Approved - موافقة المستخدم
        /// </summary>
        public bool? UserApproved { get; set; }

        /// <summary>
        /// Approved By - وُوفق بواسطة
        /// </summary>
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Approval Time - وقت الموافقة
        /// </summary>
        public DateTime? ApprovalTime { get; set; }

        /// <summary>
        /// Executed At - وقت التنفيذ
        /// </summary>
        public DateTime? ExecutedAt { get; set; }

        /// <summary>
        /// Result - النتيجة (JSON)
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Status - الحالة (pending/approved/rejected/executed/failed)
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}
