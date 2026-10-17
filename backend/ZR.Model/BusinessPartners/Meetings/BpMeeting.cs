using System;
using System.Collections.Generic;
using Infrastructure.Converter;

namespace ZR.Model.BusinessPartners.Meetings
{
    /// <summary>
    /// Generic Meeting - For Leads and Customers
    /// الاجتماعات العامة للعملاء المحتملين والعملاء الفعليين
    /// </summary>
    [SugarTable("bp_meeting")]
    public class BpMeeting
    {
        /// <summary>
        /// Meeting ID (Primary Key)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long MeetingId { get; set; }

        /// <summary>
        /// Entity Type: LEAD or CUSTOMER
        /// نوع الكيان: عميل محتمل أو عميل فعلي
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string EntityType { get; set; }

        /// <summary>
        /// Entity ID (Lead ID or Customer ID)
        /// معرف الكيان (معرف العميل المحتمل أو العميل الفعلي)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = false)]
        public long EntityId { get; set; }

        /// <summary>
        /// Meeting Title
        /// عنوان الاجتماع
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false)]
        public string Title { get; set; }

        /// <summary>
        /// Meeting Description
        /// وصف الاجتماع
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string Description { get; set; }

        /// <summary>
        /// Start Date and Time
        /// تاريخ ووقت البداية
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime StartDatetime { get; set; }

        /// <summary>
        /// End Date and Time
        /// تاريخ ووقت النهاية
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime EndDatetime { get; set; }

        /// <summary>
        /// Timezone (default: Asia/Riyadh)
        /// المنطقة الزمنية
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Timezone { get; set; } = "Asia/Riyadh";

        /// <summary>
        /// Meeting Type: ONSITE, ONLINE, CALL
        /// نوع الاجتماع: ميداني، أونلاين، مكالمة
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string MeetingType { get; set; }

        /// <summary>
        /// Meeting Channel: ZOOM, GOOGLE_MEET, TEAMS, PHONE, OFFICE, OTHER
        /// قناة الاجتماع
        /// </summary>
        [SugarColumn(Length = 30)]
        public string MeetingChannel { get; set; }

        /// <summary>
        /// Physical Location (for ONSITE meetings)
        /// الموقع الفعلي للاجتماعات الميدانية
        /// </summary>
        [SugarColumn(Length = 500)]
        public string Location { get; set; }

        /// <summary>
        /// Meeting URL (for ONLINE meetings)
        /// رابط الاجتماع الأونلاين
        /// </summary>
        [SugarColumn(Length = 500)]
        public string MeetingUrl { get; set; }

        /// <summary>
        /// Status: SCHEDULED, COMPLETED, CANCELLED, NO_SHOW
        /// الحالة: مجدول، مكتمل، ملغي، لم يحضر
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string Status { get; set; } = "SCHEDULED";

        /// <summary>
        /// Cancellation Reason
        /// سبب الإلغاء
        /// </summary>
        [SugarColumn(Length = 500)]
        public string CancelReason { get; set; }

        /// <summary>
        /// Created By (Employee ID)
        /// أنشئ بواسطة (معرف الموظف)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = false)]
        public long CreatedBy { get; set; }

        /// <summary>
        /// Created At
        /// تاريخ الإنشاء
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Updated By (Employee ID)
        /// تم التحديث بواسطة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UpdatedBy { get; set; }

        /// <summary>
        /// Updated At
        /// تاريخ التحديث
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Company ID
        /// معرف الشركة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }

        /// <summary>
        /// Branch ID
        /// معرف الفرع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? BranchId { get; set; }

        // Navigation properties (not mapped)
        [SugarColumn(IsIgnore = true)]
        public string EntityName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string OrganizerName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<BpMeetingParticipant> Participants { get; set; }

        [SugarColumn(IsIgnore = true)]
        public BpMeetingOutcome Outcome { get; set; }

        // Computed properties
        [SugarColumn(IsIgnore = true)]
        public int DurationMinutes => (int)(EndDatetime - StartDatetime).TotalMinutes;

        [SugarColumn(IsIgnore = true)]
        public bool IsUpcoming => StartDatetime > DateTime.Now && Status == "SCHEDULED";

        [SugarColumn(IsIgnore = true)]
        public bool IsActive => Status == "SCHEDULED";

        [SugarColumn(IsIgnore = true)]
        public bool IsPast => EndDatetime < DateTime.Now;
    }

    /// <summary>
    /// Entity Type Constants
    /// </summary>
    public static class EntityType
    {
        public const string LEAD = "LEAD";
        public const string CUSTOMER = "CUSTOMER";
    }

    /// <summary>
    /// Meeting Type Constants
    /// </summary>
    public static class MeetingType
    {
        public const string ONSITE = "ONSITE";
        public const string ONLINE = "ONLINE";
        public const string CALL = "CALL";
    }

    /// <summary>
    /// Meeting Channel Constants
    /// </summary>
    public static class MeetingChannel
    {
        public const string ZOOM = "ZOOM";
        public const string GOOGLE_MEET = "GOOGLE_MEET";
        public const string TEAMS = "TEAMS";
        public const string PHONE = "PHONE";
        public const string OFFICE = "OFFICE";
        public const string OTHER = "OTHER";
    }

    /// <summary>
    /// Meeting Status Constants
    /// </summary>
    public static class MeetingStatus
    {
        public const string SCHEDULED = "SCHEDULED";
        public const string COMPLETED = "COMPLETED";
        public const string CANCELLED = "CANCELLED";
        public const string NO_SHOW = "NO_SHOW";
    }
}
