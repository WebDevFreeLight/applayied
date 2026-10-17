using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;

namespace ZR.Model.Finance.ARAP.Dto
{
    /// <summary>
    /// Vendor Query DTO - استعلام المورد
    /// </summary>
    public class VendorQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string VendorCode { get; set; }
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
    /// Create Vendor DTO - إنشاء مورد جديد
    /// </summary>
    public class CreateVendorDto
    {
        [Required(ErrorMessage = "رمز المورد مطلوب")]
        [StringLength(50, ErrorMessage = "رمز المورد يجب ألا يتجاوز 50 حرف")]
        public string VendorCode { get; set; }

        [Required(ErrorMessage = "اسم المورد بالعربي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم المورد بالعربي يجب ألا يتجاوز 200 حرف")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "اسم المورد بالإنجليزي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم المورد بالإنجليزي يجب ألا يتجاوز 200 حرف")]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

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

        #endregion
    }

    /// <summary>
    /// Update Vendor DTO - تحديث مورد
    /// </summary>
    public class UpdateVendorDto
    {
        [Required(ErrorMessage = "معرف المورد مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رمز المورد مطلوب")]
        [StringLength(50, ErrorMessage = "رمز المورد يجب ألا يتجاوز 50 حرف")]
        public string VendorCode { get; set; }

        [Required(ErrorMessage = "اسم المورد بالعربي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم المورد بالعربي يجب ألا يتجاوز 200 حرف")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "اسم المورد بالإنجليزي مطلوب")]
        [StringLength(200, ErrorMessage = "اسم المورد بالإنجليزي يجب ألا يتجاوز 200 حرف")]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

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

        #endregion
    }

    /// <summary>
    /// Vendor List DTO - قائمة الموردين
    /// </summary>
    public class VendorListDto
    {
        [ExcelColumn(Name = "Vendor ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Vendor Code")]
        public string VendorCode { get; set; }

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
    }

    /// <summary>
    /// Vendor Detail DTO - تفاصيل المورد
    /// </summary>
    public class VendorDetailDto : VendorListDto
    {
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// Vendor Select DTO - المورد للقائمة المنسدلة
    /// </summary>
    public class VendorSelectDto
    {
        public Guid Id { get; set; }
        public string VendorCode { get; set; }
        public string Name { get; set; }
        public string DefaultCurrencyCode { get; set; }
        public Guid? DefaultTaxGroupId { get; set; }
        public int? PaymentTerms { get; set; }
    }
}


