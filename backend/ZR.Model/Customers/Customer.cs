using System;
using SqlSugar;
using ZR.Model.Finance.ARAP;
using ZR.Model.System;

namespace ZR.Model.Customers
{
    /// <summary>
    /// Customer - العميل
    /// </summary>
    [SugarTable("arap_customer")]
    public class Customer : SysBase, ISoftDelete
    {
        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Customer Code - رمز العميل
        /// </summary>
        [SugarColumn(Length = 50)]
        public string CustomerCode { get; set; }

        /// <summary>
        /// Name (Arabic) - الاسم بالعربي
        /// </summary>
        [SugarColumn(Length = 200)]
        public string NameAr { get; set; }

        /// <summary>
        /// Name (English) - الاسم بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200)]
        public string NameEn { get; set; }

        /// <summary>
        /// Contact Person - الشخص المسؤول
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ContactPerson { get; set; }

        /// <summary>
        /// Email - البريد الإلكتروني
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Email { get; set; }

        /// <summary>
        /// Phone - الهاتف
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Phone { get; set; }

        /// <summary>
        /// Credit Limit - الحد الائتماني
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal CreditLimit { get; set; }

        /// <summary>
        /// GL Account ID (AR Control Account) - معرف الحساب المحاسبي (حساب العملاء)
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public Guid? GLAccountId { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        #region New Fields - ARAP Critical Fixes

        /// <summary>
        /// Address (Arabic) - العنوان بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "address_ar")]
        public string AddressAr { get; set; }

        /// <summary>
        /// Address (English) - العنوان بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "address_en")]
        public string AddressEn { get; set; }

        /// <summary>
        /// Tax Number - الرقم الضريبي
        /// VAT registration number for ZATCA compliance
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "tax_number")]
        public string TaxNumber { get; set; }

        /// <summary>
        /// Default Currency ID - معرف العملة الافتراضية
        /// FK to currencies table
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_currency_id")]
        public Guid? DefaultCurrencyId { get; set; }

        /// <summary>
        /// Default Tax Group ID - معرف مجموعة الضريبة الافتراضية
        /// FK to tax_group table
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "default_tax_group_id")]
        public Guid? DefaultTaxGroupId { get; set; }

        /// <summary>
        /// Payment Terms ID - معرف شروط الدفع
        /// FK to arap_payment_terms
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "payment_terms_id")]
        public int? PaymentTermsId { get; set; }

        /// <summary>
        /// Credit Days - أيام الائتمان
        /// Number of days for credit payment (overrides payment terms if set)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "credit_days")]
        public int? CreditDays { get; set; } = 30;

        /// <summary>
        /// Lead ID - linking to the lead that was converted to this customer
        /// معرف العميل المحتمل الذي تم تحويله لهذا العميل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "lead_id")]
        public long? LeadId { get; set; }

        /// <summary>
        /// Brand Name - الاسم التجاري
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "brand_name")]
        public string BrandName { get; set; }

        /// <summary>
        /// Brand Image / Logo URL - صورة/شعار العلامة التجارية
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "brand_image")]
        public string BrandImage { get; set; }

        /// <summary>
        /// Identity Document Image URL - صورة الهوية
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "identity_image")]
        public string IdentityImage { get; set; }

        /// <summary>
        /// Tax Certificate Document URL - شهادة الضريبة
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "tax_certificate")]
        public string TaxCertificate { get; set; }

        /// <summary>
        /// Commercial Register Document URL - السجل التجاري
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "commercial_register")]
        public string CommercialRegister { get; set; }

        /// <summary>
        /// Customer Profile Document URL - ملف العميل التعريفي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "customer_profile")]
        public string CustomerProfile { get; set; }

        /// <summary>
        /// Website URL - رابط الموقع الإلكتروني
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "website_url")]
        public string WebsiteUrl { get; set; }

        [SugarColumn(IsNullable = false, ColumnName = "is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [SugarColumn(IsNullable = true, ColumnName = "deleted_at")]
        public DateTime? DeletedAt { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Payment Terms (Navigation Property) - شروط الدفع
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public PaymentTerms PaymentTerms { get; set; }

        #endregion
    }
}
