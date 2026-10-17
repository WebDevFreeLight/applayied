using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.EventManagement
{

    /// <summary>
    /// Event Visitor - زائر الفعالية
    /// </summary>
    [SugarTable("event_visitors")]
    
    [SqlDict(
        dictType: "visitor",
        dictName: "Event Visitor - الزوار",
        valueColumn: "visitorcode",
        labelColumns: "visitorcode, ' - ', firstname, ' - ', lastname",
        RemarkColumn = "notes",
        OrderBy = "visitorcode ASC",
        Description = "Auto-generated from EventVisitor table"
    )]
    public class EventVisitor : SysBase
    {
        /// <summary>
        /// Visitor ID - معرف الزائر
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long VisitorId { get; set; }

        /// <summary>
        /// Visitor Code - رمز الزائر
        /// </summary>
        public string VisitorCode { get; set; }

        /// <summary>
        /// First Name - الاسم الأول
        /// </summary>
        [SugarColumn(ColumnName = "first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last Name - الاسم الأخير
        /// </summary>
        [SugarColumn(ColumnName = "last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// First Name (Arabic) - الاسم الأول بالعربية
        /// </summary>
        [SugarColumn(ColumnName = "first_name_ar")]
        public string FirstNameAr { get; set; }

        /// <summary>
        /// Last Name (Arabic) - الاسم الأخير بالعربية
        /// </summary>
        [SugarColumn(ColumnName = "last_name_ar")]
        public string LastNameAr { get; set; }

        /// <summary>
        /// Identification Type - نوع الهوية (national_id, passport, iqama, etc.)
        /// </summary>
        [SugarColumn(ColumnName = "identification_type")]
        public string IdentificationType { get; set; }

        /// <summary>
        /// National ID - رقم الهوية الوطنية
        /// </summary>
        [SugarColumn(ColumnName = "national_id")]
        public string NationalId { get; set; }

        /// <summary>
        /// Passport Number - رقم الجواز
        /// </summary>
        [SugarColumn(ColumnName = "passport_number")]
        public string PassportNumber { get; set; }

        /// <summary>
        /// Identification Number - رقم الهوية (generic identifier)
        /// </summary>
        [SugarColumn(ColumnName = "identification_number")]
        public string IdentificationNumber { get; set; }

        /// <summary>
        /// Nationality - الجنسية
        /// </summary>
        public string Nationality { get; set; }

        /// <summary>
        /// Nationality Code - رمز الجنسية (ISO code)
        /// </summary>
        [SugarColumn(ColumnName = "nationality_code")]
        public string NationalityCode { get; set; }

        /// <summary>
        /// Date of Birth - تاريخ الميلاد
        /// </summary>
        [SugarColumn(ColumnName = "date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Gender - الجنس
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Phone Number - رقم الهاتف
        /// </summary>
        [SugarColumn(ColumnName = "phone_number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Verification Status - حالة التحقق (pending, verified, rejected)
        /// </summary>
        [SugarColumn(ColumnName = "verification_status")]
        public string VerificationStatus { get; set; }

        /// <summary>
        /// Photo URL - رابط الصورة الشخصية
        /// </summary>
        [SugarColumn(ColumnName = "photo_url")]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Visitor Type - نوع الزائر (regular, vip, media, exhibitor, speaker)
        /// </summary>
        public string VisitorType { get; set; }
    }
}

