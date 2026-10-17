using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Member - المشترك/العضو
    /// </summary>
    [SugarTable("erp_members")]
    [SqlDict(
        dictType: "erp_member",
        dictName: "Member - المشترك",
        valueColumn: "memberid",
        labelColumns: "fullname",
        RemarkColumn = "fullnamear",
        OrderBy = "membercode ASC",
        Description = "Auto-generated from ErpMember table"
    )]
    public class ErpMember : SysBase
    {
        /// <summary>
        /// Member ID - معرف العضو
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Member Code - كود العضو
        /// </summary>
        public string MemberCode { get; set; }

        /// <summary>
        /// Full Name - الاسم الكامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Full Name Arabic - الاسم بالعربي
        /// </summary>
        public string FullNameAr { get; set; }

        /// <summary>
        /// National ID - رقم الهوية
        /// </summary>
        public string NationalId { get; set; }

        /// <summary>
        /// Phone - الهاتف
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gender - الجنس (male/female)
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Birth Date - تاريخ الميلاد
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Photo URL - رابط الصورة
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Address - العنوان
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// City - المدينة
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Emergency Contact - جهة اتصال الطوارئ
        /// </summary>
        public string EmergencyContact { get; set; }

        /// <summary>
        /// Emergency Phone - هاتف الطوارئ
        /// </summary>
        public string EmergencyPhone { get; set; }

        /// <summary>
        /// Health Notes - ملاحظات صحية
        /// </summary>
        public string HealthNotes { get; set; }

        /// <summary>
        /// Medical Conditions - الحالات الطبية
        /// </summary>
        public string MedicalConditions { get; set; }

        /// <summary>
        /// Membership Start Date - تاريخ بدء العضوية
        /// </summary>
        public DateTime? MembershipStartDate { get; set; }

        /// <summary>
        /// Member Group ID - معرف مجموعة العملاء
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? MemberGroupId { get; set; }

       
        /// <summary>
        /// Referred By Member ID - عضو المحيل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReferredByMemberId { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب المرتبط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AccountId { get; set; }

        /// <summary>
        /// Current Balance - الرصيد الحالي
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Points Balance - رصيد النقاط
        /// </summary>
        public int PointsBalance { get; set; }

        /// <summary>
        /// Total Visits - إجمالي الزيارات
        /// </summary>
        public int TotalVisits { get; set; }

        /// <summary>
        /// Last Visit Date - تاريخ آخر زيارة
        /// </summary>
        public DateTime? LastVisitDate { get; set; }

        /// <summary>
        /// Has Fingerprint - لديه بصمة
        /// </summary>
        public bool HasFingerprint { get; set; }

        /// <summary>
        /// Fingerprint Data - بيانات البصمة
        /// </summary>
        public string FingerprintData { get; set; }

        /// <summary>
        /// QR Code - رمز QR
        /// </summary>
        public string QrCode { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Is Blacklisted - محظور
        /// </summary>
        public bool IsBlacklisted { get; set; }
    }
}

