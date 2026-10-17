using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;


namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Tenant - مستأجر الكشك
    /// </summary>
    [SugarTable("event_tenants")]
    
    [SqlDict(
        dictType: "tenant",
        dictName: "Event Tenant - المستأجرين",
        valueColumn: "tenantcode",
        labelColumns: "tenantcode, ' - ', companyname, ' - ', companynamear",
        RemarkColumn = "notes",
        OrderBy = "tenantcode ASC",
        Description = "Auto-generated from EventTenant table"
    )]
    public class EventTenant : SysBase
    {
        /// <summary>
        /// Tenant ID - معرف المستأجر
        /// </summary>
                [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TenantId { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        /// <summary>
        /// Tenant Code - رمز المستأجر
        /// </summary>
        public string TenantCode { get; set; }

        /// <summary>
        /// Company Name - اسم الشركة
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Company Name (Arabic) - اسم الشركة بالعربية
        /// </summary>
        public string CompanyNameAr { get; set; }

        /// <summary>
        /// Commercial Registration Number - رقم السجل التجاري
        /// </summary>
        [SugarColumn(ColumnName = "commercialregistration")]
        public string CommercialRegistration { get; set; }

        /// <summary>
        /// Tax Number - الرقم الضريبي
        /// </summary>
        public string TaxNumber { get; set; }

        /// <summary>
        /// Business Type - نوع النشاط التجاري
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// Contact Person Name - اسم جهة الاتصال
        /// </summary>
        public string ContactPersonName { get; set; }

        /// <summary>
        /// Contact Position - منصب جهة الاتصال
        /// </summary>
        public string ContactPosition { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Phone - الهاتف
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Mobile - الجوال
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// Address - العنوان
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// City - المدينة
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Country - الدولة
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Website - الموقع الإلكتروني
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Logo URL - رابط الشعار
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// Bank Name - اسم البنك
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Account Number - رقم الحساب
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// IBAN - رقم الآيبان
        /// </summary>
        public string IBAN { get; set; }

        /// <summary>
        /// Registration Date - تاريخ التسجيل
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Status - الحالة (active, inactive, blacklisted, pending)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Rating - التقييم
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// Total Booths Rented - إجمالي الأكشاك المؤجرة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int TotalBoothsRented { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; }
    }
}

