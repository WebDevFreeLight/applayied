using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Finance.ZATCA
{
    /// <summary>
    /// ZATCA Settings - إعدادات هيئة الزكاة
    /// </summary>
    [SugarTable("erp_zatca_settings")]
    public class ErpZatcaSettings : SysBase
    {
        /// <summary>
        /// Settings ID - معرف الإعدادات
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SettingsId { get; set; }

     

        /// <summary>
        /// Seller Name - اسم البائع
        /// </summary>
        public string SellerName { get; set; }

        /// <summary>
        /// Seller Name Arabic - اسم البائع بالعربي
        /// </summary>
        public string SellerNameAr { get; set; }

        /// <summary>
        /// VAT Registration Number - رقم التسجيل الضريبي
        /// </summary>
        public string VatRegistrationNumber { get; set; }

        /// <summary>
        /// Commercial Registration - السجل التجاري
        /// </summary>
        public string CommercialRegistration { get; set; }

        /// <summary>
        /// Street Name - اسم الشارع
        /// </summary>
        public string StreetName { get; set; }

        /// <summary>
        /// Building Number - رقم المبنى
        /// </summary>
        public string BuildingNumber { get; set; }

        /// <summary>
        /// Plot Identification - رقم القطعة
        /// </summary>
        public string PlotIdentification { get; set; }

        /// <summary>
        /// City - المدينة
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// City Subdivision - الحي
        /// </summary>
        public string CitySubdivision { get; set; }

        /// <summary>
        /// Postal Zone - الرمز البريدي
        /// </summary>
        public string PostalZone { get; set; }

        /// <summary>
        /// Country Code - رمز الدولة
        /// </summary>
        public string CountryCode { get; set; } = "SA";

        /// <summary>
        /// Environment - البيئة (sandbox/production)
        /// </summary>
        public string Environment { get; set; } = "sandbox";

        /// <summary>
        /// API URL - رابط API
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Certificate - الشهادة
        /// </summary>
        public string Certificate { get; set; }

        /// <summary>
        /// Private Key - المفتاح الخاص
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// CSR - طلب توقيع الشهادة
        /// </summary>
        public string CSR { get; set; }

        /// <summary>
        /// CSID - معرف الامتثال
        /// </summary>
        public string CSID { get; set; }

        /// <summary>
        /// Production CSID - معرف الإنتاج
        /// </summary>
        public string ProductionCSID { get; set; }

        /// <summary>
        /// Secret - السر
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Is Phase 2 Enabled - المرحلة الثانية مفعلة
        /// </summary>
        public bool IsPhase2Enabled { get; set; }

        /// <summary>
        /// Auto Submit - إرسال تلقائي
        /// </summary>
        public bool AutoSubmit { get; set; }

        /// <summary>
        /// Invoice Counter - عداد الفواتير
        /// </summary>
        public int InvoiceCounter { get; set; }

        /// <summary>
        /// Last Invoice Hash - آخر تجزئة فاتورة
        /// </summary>
        public string LastInvoiceHash { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

