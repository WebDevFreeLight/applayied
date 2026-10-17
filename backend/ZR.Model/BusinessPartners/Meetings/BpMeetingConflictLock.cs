using System;
using Infrastructure.Converter;

namespace ZR.Model.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Conflict Lock - Prevents double booking
    /// قفل تعارض الاجتماعات - يمنع الحجز المزدوج
    /// </summary>
    [SugarTable("bp_meeting_conflict_lock")]
    public class BpMeetingConflictLock
    {
        /// <summary>
        /// Lock ID (Primary Key)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long LockId { get; set; }

        /// <summary>
        /// Meeting ID (Foreign Key)
        /// معرف الاجتماع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = false)]
        public long MeetingId { get; set; }

        /// <summary>
        /// Start DateTime of locked period
        /// بداية الفترة المقفلة
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime StartDatetime { get; set; }

        /// <summary>
        /// End DateTime of locked period
        /// نهاية الفترة المقفلة
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime EndDatetime { get; set; }

        /// <summary>
        /// Scope: COMPANY (company-wide), EMPLOYEE (specific employee), ENTITY (specific lead/customer)
        /// النطاق: الشركة، موظف محدد، كيان محدد
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string Scope { get; set; }

        /// <summary>
        /// Scope ID
        /// - NULL for COMPANY scope (all employees busy)
        /// - employee_id for EMPLOYEE scope
        /// - entity_id for ENTITY scope (lead_id or customer_id)
        /// معرف النطاق
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ScopeId { get; set; }

        /// <summary>
        /// Created At
        /// تاريخ الإنشاء
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties (not mapped)
        [SugarColumn(IsIgnore = true)]
        public BpMeeting Meeting { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string LockDescription { get; set; }

        // Computed properties
        [SugarColumn(IsIgnore = true)]
        public int DurationMinutes => (int)(EndDatetime - StartDatetime).TotalMinutes;

        [SugarColumn(IsIgnore = true)]
        public bool IsActive => EndDatetime > DateTime.Now;
    }

    /// <summary>
    /// Conflict Lock Scope Constants
    /// </summary>
    public static class LockScope
    {
        /// <summary>
        /// Company-wide block - All employees unavailable
        /// حظر على مستوى الشركة - جميع الموظفين غير متاحين
        /// </summary>
        public const string COMPANY = "COMPANY";

        /// <summary>
        /// Employee-specific lock - Specific employee unavailable
        /// قفل خاص بموظف - موظف محدد غير متاح
        /// </summary>
        public const string EMPLOYEE = "EMPLOYEE";

        /// <summary>
        /// Entity-specific lock - Specific lead/customer unavailable
        /// قفل خاص بكيان - عميل محتمل/فعلي محدد غير متاح
        /// </summary>
        public const string ENTITY = "ENTITY";
    }
}
