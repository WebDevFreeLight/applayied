using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;

namespace ZR.Model.Customers.Dto
{
    /// <summary>
    /// Customer Query DTO - استعلام العميل
    /// </summary>
    public class CustomerQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string CustomerCode { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool? IsActive { get; set; }
        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
    }

    /// <summary>
    /// Create Customer DTO - إنشاء عميل جديد
    /// </summary>
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "رمز العميل مطلوب")]
        [StringLength(50, ErrorMessage = "رمز العميل يجب ألا يتجاوز 50 حرف")]
        public string CustomerCode { get; set; }

        [Required(ErrorMessage = "اسم العميل بالعربي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم العميل بالعربي يجب ألا يتجاوز 200 حرف")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "اسم العميل بالإنجليزي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم العميل بالإنجليزي يجب ألا يتجاوز 200 حرف")]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "الحد الائتماني يجب أن يكون موجباً")]
        public decimal CreditLimit { get; set; } = 0;

        public Guid? GLAccountId { get; set; }

        public bool IsActive { get; set; } = true;

        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        #region New Fields - ARAP Critical Fixes

        /// <summary>
        /// Address (Arabic) - العنوان بالعربي
        /// </summary>
        [StringLength(500)]
        public string AddressAr { get; set; }

        /// <summary>
        /// Address (English) - العنوان بالإنجليزي
        /// </summary>
        [StringLength(500)]
        public string AddressEn { get; set; }

        /// <summary>
        /// Tax Number - الرقم الضريبي (VAT)
        /// </summary>
        [StringLength(50)]
        public string TaxNumber { get; set; }

        /// <summary>
        /// Default Currency ID - معرف العملة الافتراضية
        /// </summary>
        public Guid? DefaultCurrencyId { get; set; }

        /// <summary>
        /// Default Tax Group ID - معرف مجموعة الضريبة الافتراضية
        /// </summary>
        public Guid? DefaultTaxGroupId { get; set; }

        /// <summary>
        /// Payment Terms ID - معرف شروط الدفع
        /// </summary>
        public int? PaymentTermsId { get; set; }

        /// <summary>
        /// Credit Days - أيام الائتمان
        /// </summary>
        [Range(0, 365)]
        public int? CreditDays { get; set; } = 30;

        /// <summary>
        /// Lead ID - معرف العميل المحتمل المحول
        /// </summary>
        public long? LeadId { get; set; }

        /// <summary>
        /// Brand Name - الاسم التجاري
        /// </summary>
        [StringLength(200)]
        public string BrandName { get; set; }

        /// <summary>
        /// Brand Image / Logo URL - صورة/شعار العلامة التجارية
        /// </summary>
        [StringLength(500)]
        public string BrandImage { get; set; }

        [StringLength(500)]
        public string IdentityImage { get; set; }

        [StringLength(500)]
        public string TaxCertificate { get; set; }

        [StringLength(500)]
        public string CommercialRegister { get; set; }

        [StringLength(500)]
        public string CustomerProfile { get; set; }

        /// <summary>
        /// Website URL - رابط الموقع الإلكتروني
        /// </summary>
        [StringLength(500)]
        public string WebsiteUrl { get; set; }

        #endregion
    }

    /// <summary>
    /// Update Customer DTO - تحديث عميل
    /// </summary>
    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "معرف العميل مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رمز العميل مطلوب")]
        [StringLength(50, ErrorMessage = "رمز العميل يجب ألا يتجاوز 50 حرف")]
        public string CustomerCode { get; set; }

        [Required(ErrorMessage = "اسم العميل بالعربي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم العميل بالعربي يجب ألا يتجاوز 200 حرف")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "اسم العميل بالإنجليزي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم العميل بالإنجليزي يجب ألا يتجاوز 200 حرف")]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "الحد الائتماني يجب أن يكون موجباً")]
        public decimal CreditLimit { get; set; } = 0;

        public Guid? GLAccountId { get; set; }

        public bool IsActive { get; set; } = true;

        public long? TenantId { get; set; }

        #region New Fields - ARAP Critical Fixes

        /// <summary>
        /// Address (Arabic) - العنوان بالعربي
        /// </summary>
        [StringLength(500)]
        public string AddressAr { get; set; }

        /// <summary>
        /// Address (English) - العنوان بالإنجليزي
        /// </summary>
        [StringLength(500)]
        public string AddressEn { get; set; }

        /// <summary>
        /// Tax Number - الرقم الضريبي (VAT)
        /// </summary>
        [StringLength(50)]
        public string TaxNumber { get; set; }

        /// <summary>
        /// Default Currency ID - معرف العملة الافتراضية
        /// </summary>
        public Guid? DefaultCurrencyId { get; set; }

        /// <summary>
        /// Default Tax Group ID - معرف مجموعة الضريبة الافتراضية
        /// </summary>
        public Guid? DefaultTaxGroupId { get; set; }

        /// <summary>
        /// Payment Terms ID - معرف شروط الدفع
        /// </summary>
        public int? PaymentTermsId { get; set; }

        /// <summary>
        /// Credit Days - أيام الائتمان
        /// </summary>
        [Range(0, 365)]
        public int? CreditDays { get; set; } = 30;

        /// <summary>
        /// Brand Name - الاسم التجاري
        /// </summary>
        [StringLength(200)]
        public string BrandName { get; set; }

        /// <summary>
        /// Brand Image / Logo URL - صورة/شعار العلامة التجارية
        /// </summary>
        [StringLength(500)]
        public string BrandImage { get; set; }

        [StringLength(500)]
        public string IdentityImage { get; set; }

        [StringLength(500)]
        public string TaxCertificate { get; set; }

        [StringLength(500)]
        public string CommercialRegister { get; set; }

        [StringLength(500)]
        public string CustomerProfile { get; set; }

        /// <summary>
        /// Website URL - رابط الموقع الإلكتروني
        /// </summary>
        [StringLength(500)]
        public string WebsiteUrl { get; set; }

        #endregion
    }

    /// <summary>
    /// Customer List DTO - قائمة العملاء
    /// </summary>
    public class CustomerListDto
    {
        [ExcelColumn(Name = "Customer ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Customer Code")]
        public string CustomerCode { get; set; }

        [ExcelColumn(Name = "Name (Arabic)")]
        public string NameAr { get; set; }

        [ExcelColumn(Name = "Name (English)")]
        public string NameEn { get; set; }

        [ExcelColumn(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [ExcelColumn(Name = "Email")]
        public string Email { get; set; }

        [ExcelColumn(Name = "Phone")]
        public string Phone { get; set; }

        [ExcelColumn(Name = "Credit Limit")]
        public decimal CreditLimit { get; set; }

        [ExcelColumn(Name = "Is Active")]
        public bool IsActive { get; set; }

        public Guid? GLAccountId { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // New Fields - ARAP Critical Fixes
        public string AddressAr { get; set; }
        public string AddressEn { get; set; }
        public string TaxNumber { get; set; }
        public Guid? DefaultCurrencyId { get; set; }
        public string DefaultCurrencyCode { get; set; }
        public Guid? DefaultTaxGroupId { get; set; }
        public string DefaultTaxGroupName { get; set; }
        public int? PaymentTermsId { get; set; }
        public string PaymentTermsName { get; set; }
        public int? CreditDays { get; set; }
        public string BrandName { get; set; }
        public string BrandImage { get; set; }
        public string IdentityImage { get; set; }
        public string TaxCertificate { get; set; }
        public string CommercialRegister { get; set; }
        public string CustomerProfile { get; set; }
        public string WebsiteUrl { get; set; }
        public long? LeadId { get; set; }
    }

    /// <summary>
    /// Customer Detail DTO - تفاصيل العميل
    /// </summary>
    public class CustomerDetailDto : CustomerListDto
    {
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// Customer Select DTO - العميل للقائمة المنسدلة
    /// </summary>
    public class CustomerSelectDto
    {
        public Guid Id { get; set; }
        public string CustomerCode { get; set; }
        public string Name { get; set; }
        public string DefaultCurrencyCode { get; set; }
        public Guid? DefaultTaxGroupId { get; set; }
        public int? PaymentTerms { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
    }
}
