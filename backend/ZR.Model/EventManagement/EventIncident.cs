using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Incident - الحوادث والبلاغات
    /// </summary>
    [SugarTable("event_incidents")]
    
    [SqlDict(
        dictType: "incident",
        dictName: "Event Incident - الحوادث",
        valueColumn: "incidentnumber",
        labelColumns: "incidentnumber, ' - ', title",
        RemarkColumn = "description",
        OrderBy = "incidentnumber ASC",
        Description = "Auto-generated from EventIncident table"
    )]
    public class EventIncident : SysBase
    {

        /// <summary>
        /// Incident ID - معرف الحادث
        /// </summary>
                [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long IncidentId { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        /// <summary>
        /// Incident Number - رقم البلاغ
        /// </summary>
        public string IncidentNumber { get; set; }

        /// <summary>
        /// Incident Type - نوع الحادث (security, medical, technical, maintenance, complaint, emergency)
        /// </summary>
        public string IncidentType { get; set; }

        /// <summary>
        /// Incident Category - فئة الحادث
        /// </summary>
        public string IncidentCategory { get; set; }

        /// <summary>
        /// Priority - الأولوية (low, medium, high, critical)
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Severity - الخطورة (minor, moderate, major, critical)
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Title - العنوان
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Location - الموقع
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Zone ID - معرف المنطقة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }

        /// <summary>
        /// Booth ID - معرف الكشك
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? BoothId { get; set; }

        /// <summary>
        /// Reported Date - تاريخ البلاغ
        /// </summary>
        public DateTime ReportedDate { get; set; }

        /// <summary>
        /// Reported By - تم البلاغ بواسطة (User ID or Name)
        /// </summary>
        public string ReportedBy { get; set; }

        /// <summary>
        /// Reporter Type - نوع المبلغ (visitor, staff, tenant, security)
        /// </summary>
        public string ReporterType { get; set; }

        /// <summary>
        /// Reporter Contact - جهة اتصال المبلغ
        /// </summary>
        public string ReporterContact { get; set; }

        /// <summary>
        /// Status - الحالة (reported, assigned, in-progress, resolved, closed, escalated)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Assigned To - مكلف به (Employee ID)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssignedToId { get; set; }

        /// <summary>
        /// Assigned Date - تاريخ التكليف
        /// </summary>
        public DateTime? AssignedDate { get; set; }

        /// <summary>
        /// Assigned Team - الفريق المكلف
        /// </summary>
        public string AssignedTeam { get; set; }

        /// <summary>
        /// Response Time (Minutes) - وقت الاستجابة بالدقائق
        /// </summary>
        public int? ResponseTimeMinutes { get; set; }

        /// <summary>
        /// Resolution Time (Minutes) - وقت الحل بالدقائق
        /// </summary>
        public int? ResolutionTimeMinutes { get; set; }

        /// <summary>
        /// Resolution Date - تاريخ الحل
        /// </summary>
        public DateTime? ResolutionDate { get; set; }

        /// <summary>
        /// Resolution Notes - ملاحظات الحل
        /// </summary>
        public string ResolutionNotes { get; set; }

        /// <summary>
        /// Escalated - تم التصعيد
        /// </summary>
        public bool Escalated { get; set; }

        /// <summary>
        /// Escalated To - تم التصعيد إلى
        /// </summary>
        public string EscalatedTo { get; set; }

        /// <summary>
        /// Escalation Date - تاريخ التصعيد
        /// </summary>
        public DateTime? EscalationDate { get; set; }

        /// <summary>
        /// Escalation Reason - سبب التصعيد
        /// </summary>
        public string EscalationReason { get; set; }

        /// <summary>
        /// Attachments (JSON array of URLs) - المرفقات
        /// </summary>
        public string Attachments { get; set; }

        /// <summary>
        /// Witness Information - معلومات الشهود
        /// </summary>
        public string WitnessInformation { get; set; }

        /// <summary>
        /// Follow Up Required - يتطلب متابعة
        /// </summary>
        public bool FollowUpRequired { get; set; }

        /// <summary>
        /// Follow Up Date - تاريخ المتابعة
        /// </summary>
        public DateTime? FollowUpDate { get; set; }

        /// <summary>
        /// Closed Date - تاريخ الإغلاق
        /// </summary>
        public DateTime? ClosedDate { get; set; }

        /// <summary>
        /// Closed By - تم الإغلاق بواسطة
        /// </summary>
        public string ClosedBy { get; set; }

        /// <summary>
        /// Customer Satisfaction Rating - تقييم رضا العميل
        /// </summary>
        public int? SatisfactionRating { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; }
        public DateTime IncidentTime { get; set; }
        public DateTime? AssignedTime { get; set; }
        public DateTime ResolvedTime { get; set; }
        public string Resolution { get; set; }
        public DateTime ClosedTime { get; set; }
    }
}

