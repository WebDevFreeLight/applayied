using System;
using Infrastructure.Converter;

namespace ZR.Model.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Outcome - Post-meeting results and follow-up actions
    /// نتائج الاجتماع وإجراءات المتابعة
    /// </summary>
    [SugarTable("bp_meeting_outcome")]
    public class BpMeetingOutcome
    {
        /// <summary>
        /// Outcome ID (Primary Key)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long OutcomeId { get; set; }

        /// <summary>
        /// Meeting ID (Foreign Key)
        /// معرف الاجتماع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = false)]
        public long MeetingId { get; set; }

        /// <summary>
        /// Outcome Type: POSITIVE, NEUTRAL, NEGATIVE
        /// نوع النتيجة: إيجابي، محايد، سلبي
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string OutcomeType { get; set; }

        /// <summary>
        /// Outcome Notes/Details
        /// ملاحظات النتيجة
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string Notes { get; set; }

        /// <summary>
        /// Next Action: FOLLOW_UP, PROPOSAL, CLOSE, SCHEDULE_MEETING, SEND_INFO, NONE
        /// الإجراء التالي
        /// </summary>
        [SugarColumn(Length = 30)]
        public string NextAction { get; set; }

        /// <summary>
        /// Next Action Date (scheduled date for follow-up)
        /// تاريخ الإجراء التالي
        /// </summary>
        public DateTime? NextActionDate { get; set; }

        /// <summary>
        /// Employee assigned to next action
        /// الموظف المكلف بالإجراء التالي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NextActionAssignee { get; set; }

        /// <summary>
        /// Recorded By (Employee ID)
        /// تم التسجيل بواسطة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RecordedBy { get; set; }

        /// <summary>
        /// Recorded At
        /// تاريخ التسجيل
        /// </summary>
        public DateTime RecordedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Updated At
        /// تاريخ التحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties (not mapped)
        [SugarColumn(IsIgnore = true)]
        public BpMeeting Meeting { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string RecordedByName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string NextActionAssigneeName { get; set; }

        // Computed properties
        [SugarColumn(IsIgnore = true)]
        public bool HasNextAction => !string.IsNullOrEmpty(NextAction) && NextAction != "NONE";

        [SugarColumn(IsIgnore = true)]
        public bool IsNextActionOverdue => NextActionDate.HasValue && NextActionDate.Value < DateTime.Now;
    }

    /// <summary>
    /// Outcome Type Constants
    /// </summary>
    public static class OutcomeType
    {
        /// <summary>
        /// Positive outcome - Good progress, interested, moving forward
        /// نتيجة إيجابية
        /// </summary>
        public const string POSITIVE = "POSITIVE";

        /// <summary>
        /// Neutral outcome - No clear direction, needs more discussion
        /// نتيجة محايدة
        /// </summary>
        public const string NEUTRAL = "NEUTRAL";

        /// <summary>
        /// Negative outcome - Not interested, rejected, dead end
        /// نتيجة سلبية
        /// </summary>
        public const string NEGATIVE = "NEGATIVE";
    }

    /// <summary>
    /// Next Action Constants
    /// </summary>
    public static class NextAction
    {
        /// <summary>
        /// Schedule a follow-up call/email
        /// جدولة متابعة
        /// </summary>
        public const string FOLLOW_UP = "FOLLOW_UP";

        /// <summary>
        /// Send a proposal/quotation
        /// إرسال عرض/اقتراح
        /// </summary>
        public const string PROPOSAL = "PROPOSAL";

        /// <summary>
        /// Close the deal (convert to customer)
        /// إغلاق الصفقة
        /// </summary>
        public const string CLOSE = "CLOSE";

        /// <summary>
        /// Schedule another meeting
        /// جدولة اجتماع آخر
        /// </summary>
        public const string SCHEDULE_MEETING = "SCHEDULE_MEETING";

        /// <summary>
        /// Send information/materials
        /// إرسال معلومات
        /// </summary>
        public const string SEND_INFO = "SEND_INFO";

        /// <summary>
        /// No further action required
        /// لا يوجد إجراء مطلوب
        /// </summary>
        public const string NONE = "NONE";
    }
}
