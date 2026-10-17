using System;
using Infrastructure.Converter;

namespace ZR.Model.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Participant - Many-to-Many relationship between Meetings and Employees
    /// مشاركو الاجتماع
    /// </summary>
    [SugarTable("bp_meeting_participant")]
    public class BpMeetingParticipant
    {
        /// <summary>
        /// Participant ID (Primary Key)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long ParticipantId { get; set; }

        /// <summary>
        /// Meeting ID (Foreign Key)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = false)]
        public long MeetingId { get; set; }

        /// <summary>
        /// Employee ID (Foreign Key)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = false)]
        public long EmployeeId { get; set; }

        /// <summary>
        /// Role: ORGANIZER, ATTENDEE, OBSERVER
        /// الدور: منظم، حاضر، مراقب
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string Role { get; set; } = "ATTENDEE";

        /// <summary>
        /// Attendance Status: PENDING, ACCEPTED, DECLINED, ATTENDED, ABSENT
        /// حالة الحضور
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string AttendanceStatus { get; set; } = "PENDING";

        /// <summary>
        /// Response DateTime (when employee accepted/declined)
        /// تاريخ الرد
        /// </summary>
        public DateTime? ResponseDatetime { get; set; }

        /// <summary>
        /// Participant Notes
        /// ملاحظات المشارك
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string Notes { get; set; }

        /// <summary>
        /// Created At
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Updated At
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties (not mapped)
        [SugarColumn(IsIgnore = true)]
        public BpMeeting Meeting { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string EmployeeName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string EmployeeEmail { get; set; }
    }

    /// <summary>
    /// Participant Role Constants
    /// </summary>
    public static class ParticipantRole
    {
        public const string ORGANIZER = "ORGANIZER";
        public const string ATTENDEE = "ATTENDEE";
        public const string OBSERVER = "OBSERVER";
    }

    /// <summary>
    /// Attendance Status Constants
    /// </summary>
    public static class AttendanceStatus
    {
        public const string PENDING = "PENDING";
        public const string ACCEPTED = "ACCEPTED";
        public const string DECLINED = "DECLINED";
        public const string ATTENDED = "ATTENDED";
        public const string ABSENT = "ABSENT";
    }
}
