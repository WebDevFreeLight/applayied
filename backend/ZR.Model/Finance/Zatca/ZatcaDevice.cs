using System;
using SqlSugar;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Zatca
{
    /// <summary>
    /// ZATCA Device - جهاز زاتكا (EGS - E-Invoice Generation Solution)
    /// Represents a registered device/solution for ZATCA e-invoicing
    /// Each device has its own certificate and can generate invoices
    /// </summary>
    [SugarTable("zatca_device")]
    [SugarIndex("UX_zatca_device_company_serial", nameof(CompanyID), OrderByType.Asc, nameof(DeviceSerialNumber), OrderByType.Asc, true)]
    public class ZatcaDevice : SysBase
    {
        /// <summary>
        /// Device ID - معرف الجهاز
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Device Serial Number - الرقم التسلسلي للجهاز
        /// Used in CSR generation
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 100, ColumnName = "device_serial_number")]
        public string DeviceSerialNumber { get; set; }

        /// <summary>
        /// Device Name - اسم الجهاز
        /// Friendly name for identification
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 200, ColumnName = "device_name")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Organization Identifier (VAT Number) - معرف المنشأة (الرقم الضريبي)
        /// 15 digits VAT registration number
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 15, ColumnName = "organization_identifier")]
        public string OrganizationIdentifier { get; set; }

        /// <summary>
        /// Organization Unit Name - اسم الوحدة التنظيمية
        /// Branch or location name (10 digits)
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 100, ColumnName = "organization_unit_name")]
        public string OrganizationUnitName { get; set; }

        /// <summary>
        /// Organization Name (English) - اسم المنشأة بالإنجليزية
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 200, ColumnName = "organization_name_en")]
        public string OrganizationNameEn { get; set; }

        /// <summary>
        /// Organization Name (Arabic) - اسم المنشأة بالعربية
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 200, ColumnName = "organization_name_ar")]
        public string OrganizationNameAr { get; set; }

        /// <summary>
        /// Country Code - رمز الدولة (SA)
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 2, ColumnName = "country_code")]
        public string CountryCode { get; set; } = "SA";

        /// <summary>
        /// Invoice Type (Standard, Simplified, Both) - نوع الفاتورة
        /// Binary flags: 1000 = Standard, 0100 = Simplified, 1100 = Both
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 4, ColumnName = "invoice_type")]
        public string InvoiceType { get; set; } = "1100";

        /// <summary>
        /// Location Address - عنوان الموقع
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnName = "location_address")]
        public string LocationAddress { get; set; }

        /// <summary>
        /// Industry/Business Category - قطاع الأعمال
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 200, ColumnName = "industry_business_category")]
        public string IndustryBusinessCategory { get; set; }

        #region Certificate Fields

        /// <summary>
        /// CSR (Certificate Signing Request) - طلب توقيع الشهادة
        /// Generated during onboarding
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "csr")]
        public string Csr { get; set; }

        /// <summary>
        /// Private Key (Encrypted) - المفتاح الخاص (مشفر)
        /// Used for signing invoices
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "private_key")]
        public string PrivateKey { get; set; }

        /// <summary>
        /// Compliance CSID - معرف الامتثال
        /// Obtained from ZATCA compliance API
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnName = "compliance_csid")]
        public string ComplianceCsid { get; set; }

        /// <summary>
        /// Compliance CSID Secret - سر معرف الامتثال
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnName = "compliance_csid_secret")]
        public string ComplianceCsidSecret { get; set; }

        /// <summary>
        /// Compliance Certificate - شهادة الامتثال
        /// X.509 certificate from ZATCA
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "compliance_certificate")]
        public string ComplianceCertificate { get; set; }

        /// <summary>
        /// Production CSID - معرف الإنتاج
        /// Obtained after compliance checks pass
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnName = "production_csid")]
        public string ProductionCsid { get; set; }

        /// <summary>
        /// Production CSID Secret - سر معرف الإنتاج
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnName = "production_csid_secret")]
        public string ProductionCsidSecret { get; set; }

        /// <summary>
        /// Production Certificate - شهادة الإنتاج
        /// X.509 certificate for production
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "production_certificate")]
        public string ProductionCertificate { get; set; }

        /// <summary>
        /// Certificate Expiry Date - تاريخ انتهاء الشهادة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "certificate_expiry_date")]
        public DateTime? CertificateExpiryDate { get; set; }

        #endregion

        #region Status Fields

        /// <summary>
        /// Environment - البيئة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "environment")]
        public ZatcaEnvironment Environment { get; set; } = ZatcaEnvironment.Sandbox;

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Is Compliance Completed - تم الامتثال
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_compliance_completed")]
        public bool IsComplianceCompleted { get; set; } = false;

        /// <summary>
        /// Is Production Ready - جاهز للإنتاج
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_production_ready")]
        public bool IsProductionReady { get; set; } = false;

        /// <summary>
        /// Compliance Completed At - تاريخ إتمام الامتثال
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "compliance_completed_at")]
        public DateTime? ComplianceCompletedAt { get; set; }

        /// <summary>
        /// Production Activated At - تاريخ تفعيل الإنتاج
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "production_activated_at")]
        public DateTime? ProductionActivatedAt { get; set; }

        /// <summary>
        /// Last Used At - آخر استخدام
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_used_at")]
        public DateTime? LastUsedAt { get; set; }

        #endregion
    }
}
