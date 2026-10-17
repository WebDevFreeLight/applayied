using System;
using SqlSugar;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Zatca
{
    /// <summary>
    /// ZATCA Configuration - إعدادات زاتكا
    /// Company-level ZATCA settings and API configuration
    /// </summary>
    [SugarTable("zatca_configuration")]
    [SugarIndex("UX_zatca_config_company", nameof(CompanyID), OrderByType.Asc, true)]
    public class ZatcaConfiguration : SysBase
    {
        /// <summary>
        /// Configuration ID - معرف الإعداد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        #region Seller Information - معلومات البائع

        /// <summary>
        /// Seller VAT Registration Number - الرقم الضريبي للبائع
        /// 15 digits
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 15, ColumnName = "seller_vat_number")]
        public string SellerVatNumber { get; set; }

        /// <summary>
        /// Seller Name (Arabic) - اسم البائع بالعربية
        /// Required for ZATCA
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 200, ColumnName = "seller_name_ar")]
        public string SellerNameAr { get; set; }

        /// <summary>
        /// Seller Name (English) - اسم البائع بالإنجليزية
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 200, ColumnName = "seller_name_en")]
        public string SellerNameEn { get; set; }

        /// <summary>
        /// Seller Street Name - اسم الشارع
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 200, ColumnName = "seller_street")]
        public string SellerStreet { get; set; }

        /// <summary>
        /// Seller Building Number - رقم المبنى
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 20, ColumnName = "seller_building_number")]
        public string SellerBuildingNumber { get; set; }

        /// <summary>
        /// Seller Plot Identification - رقم القطعة
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 20, ColumnName = "seller_plot_identification")]
        public string SellerPlotIdentification { get; set; }

        /// <summary>
        /// Seller City Subdivision (District) - الحي
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 100, ColumnName = "seller_city_subdivision")]
        public string SellerCitySubdivision { get; set; }

        /// <summary>
        /// Seller City - المدينة
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 100, ColumnName = "seller_city")]
        public string SellerCity { get; set; }

        /// <summary>
        /// Seller Postal Code - الرمز البريدي
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 10, ColumnName = "seller_postal_code")]
        public string SellerPostalCode { get; set; }

        /// <summary>
        /// Seller Country Code - رمز الدولة (SA)
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 2, ColumnName = "seller_country_code")]
        public string SellerCountryCode { get; set; } = "SA";

        /// <summary>
        /// Seller Country Subentity (Region) - المنطقة
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 100, ColumnName = "seller_country_subentity")]
        public string SellerCountrySubentity { get; set; }

        /// <summary>
        /// Seller Additional Street Name - اسم شارع إضافي
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 200, ColumnName = "seller_additional_street")]
        public string SellerAdditionalStreet { get; set; }

        #endregion

        #region API Configuration - إعدادات API

        /// <summary>
        /// Current Environment - البيئة الحالية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "environment")]
        public ZatcaEnvironment Environment { get; set; } = ZatcaEnvironment.Sandbox;

        /// <summary>
        /// Sandbox API URL - رابط API للاختبار
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 500, ColumnName = "sandbox_api_url")]
        public string SandboxApiUrl { get; set; } = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal";

        /// <summary>
        /// Simulation API URL - رابط API للمحاكاة
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 500, ColumnName = "simulation_api_url")]
        public string SimulationApiUrl { get; set; } = "https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation";

        /// <summary>
        /// Production API URL - رابط API للإنتاج
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 500, ColumnName = "production_api_url")]
        public string ProductionApiUrl { get; set; } = "https://gw-fatoora.zatca.gov.sa/e-invoicing/core";

        /// <summary>
        /// API Timeout (seconds) - مهلة API بالثواني
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "api_timeout")]
        public int ApiTimeout { get; set; } = 30;

        /// <summary>
        /// Max Retry Attempts - أقصى عدد محاولات إعادة المحاولة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "max_retry_attempts")]
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Retry Delay (milliseconds) - تأخير إعادة المحاولة بالمللي ثانية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "retry_delay_ms")]
        public int RetryDelayMs { get; set; } = 1000;

        #endregion

        #region Invoice Settings - إعدادات الفاتورة

        /// <summary>
        /// Default Payment Means Code - كود وسيلة الدفع الافتراضية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "default_payment_means_code")]
        public ZatcaPaymentMeansCode DefaultPaymentMeansCode { get; set; } = ZatcaPaymentMeansCode.CreditTransfer;

        /// <summary>
        /// Auto Submit Standard Invoices - إرسال الفواتير القياسية تلقائياً
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "auto_submit_standard")]
        public bool AutoSubmitStandard { get; set; } = false;

        /// <summary>
        /// Auto Submit Simplified Invoices - إرسال الفواتير المبسطة تلقائياً
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "auto_submit_simplified")]
        public bool AutoSubmitSimplified { get; set; } = false;

        /// <summary>
        /// Generate QR Code for Standard Invoices - إنشاء QR للفواتير القياسية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "qr_for_standard")]
        public bool QrForStandard { get; set; } = true;

        /// <summary>
        /// Generate QR Code for Simplified Invoices - إنشاء QR للفواتير المبسطة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "qr_for_simplified")]
        public bool QrForSimplified { get; set; } = true;

        /// <summary>
        /// Default Currency - العملة الافتراضية
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 3, ColumnName = "default_currency")]
        public string DefaultCurrency { get; set; } = "SAR";

        #endregion

        #region Active Device

        /// <summary>
        /// Active Device ID - معرف الجهاز النشط
        /// FK to ZatcaDevice
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "active_device_id")]
        public Guid? ActiveDeviceId { get; set; }

        /// <summary>
        /// Active Device (Navigation) - الجهاز النشط
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ZatcaDevice ActiveDevice { get; set; }

        #endregion

        #region Status

        /// <summary>
        /// Is Enabled - مفعّل
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Is Configured - تم الإعداد
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_configured")]
        public bool IsConfigured { get; set; } = false;

        /// <summary>
        /// Last Configuration Check - آخر فحص للإعدادات
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_config_check")]
        public DateTime? LastConfigCheck { get; set; }

        #endregion

        /// <summary>
        /// Get API URL based on current environment
        /// </summary>
        public string GetApiUrl()
        {
            return Environment switch
            {
                ZatcaEnvironment.Sandbox => SandboxApiUrl,
                ZatcaEnvironment.Simulation => SimulationApiUrl,
                ZatcaEnvironment.Production => ProductionApiUrl,
                _ => SandboxApiUrl
            };
        }
    }
}
