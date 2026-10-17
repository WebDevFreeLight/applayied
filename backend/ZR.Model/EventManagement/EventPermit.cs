using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Permit - تصاريح الدخول
    /// </summary>
    [SugarTable("event_permits")]
    [SqlDict(
        dictType: "permit",
        dictName: "Event Permit - التصاريح",
        valueColumn: "permitnumber",
        labelColumns: "permitnumber, ' - ', applicantname, ' - ', applicantnamear",
        RemarkColumn = "notes",
        OrderBy = "permitnumber ASC",
        Description = "Auto-generated from EventPermit table"
    )]
    public class EventPermit : SysBase
    {
        /// <summary>
        /// Permit ID - معرف التصريح
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PermitId { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        /// <summary>
        /// Employee ID - معرف الموظف/العامل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public string? EmployeeId { get; set; }

        /// <summary>
        /// Visitor ID - معرف الزائر
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public string? VisitorId { get; set; }

        /// <summary>
        /// Permit Number - رقم التصريح
        /// </summary>
        public string PermitNumber { get; set; }

        /// <summary>
        /// Permit Type - نوع التصريح (staff, vendor, vip, media, contractor, vehicle)
        /// </summary>
        public string PermitType { get; set; }

        /// <summary>
        /// Permit Category - فئة التصريح
        /// </summary>
        public string PermitCategory { get; set; }

        /// <summary>
        /// Applicant Name - اسم مقدم الطلب
        /// </summary>
        public string ApplicantName { get; set; }

        /// <summary>
        /// Applicant Name (Arabic) - اسم مقدم الطلب بالعربية
        /// </summary>
        public string ApplicantNameAr { get; set; }

        /// <summary>
        /// Company Name - اسم الشركة
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// National ID - رقم الهوية
        /// </summary>
        public string NationalId { get; set; }

        /// <summary>
        /// Contact Phone - هاتف التواصل
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Valid From - صالح من
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Valid Until - صالح حتى
        /// </summary>
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// Issue Date - تاريخ الإصدار
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Issued By - صادر من
        /// </summary>
        public string IssuedBy { get; set; }

        /// <summary>
        /// QR Code - رمز الاستجابة السريع
        /// </summary>
        public string QRCode { get; set; }

        /// <summary>
        /// Zone Access - الوصول للمناطق (JSON array)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public string ZoneAccess { get; set; }


        /// <summary>
        /// Gate Access - الوصول للبوابات (JSON array)
        /// </summary>
  
        [JsonConverter(typeof(ValueToStringConverter))]
        public string GateAccess { get; set; }

        /// <summary>
        /// Access Level - مستوى الوصول (basic, advanced, full)
        /// </summary>
        public string AccessLevel { get; set; }

        /// <summary>
        /// Purpose - الغرض
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// Vehicle Plate Number - رقم لوحة المركبة
        /// </summary>
        public string VehiclePlateNumber { get; set; }

        /// <summary>
        /// Vehicle Type - نوع المركبة
        /// </summary>
        public string VehicleType { get; set; }

        /// <summary>
        /// Photo URL - رابط الصورة
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Attachment URL - رابط المرفق
        /// </summary>
        public string AttachmentUrl { get; set; }

        /// <summary>
        /// Status - الحالة (pending, approved, rejected, active, expired, cancelled)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Approved By - تمت الموافقة من قبل
        /// </summary>
        public long ApprovedBy { get; set; }

        /// <summary>
        /// Approval Date - تاريخ الموافقة
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Rejection Reason - سبب الرفض
        /// </summary>
        public string RejectionReason { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        ///// <summary>
        ///// Is Active - نشط
        ///// </summary>
        //public bool IsActive { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string HolderName { get; set; }
    }
}

