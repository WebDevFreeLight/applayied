using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Configuration.Enums;

namespace ZR.Model.Finance.Configuration.Dto
{
    /// <summary>
    /// Tax Rule Query DTO - استعلام قاعدة الضريبة
    /// </summary>
    public class TaxRuleQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string TaxName { get; set; }
        public TaxType? TaxType { get; set; }
        public bool? Status { get; set; }
        public long? TenantId { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Tax Rule DTO - إنشاء قاعدة ضريبة جديدة
    /// </summary>
    public class CreateTaxRuleDto
    {
        [Required(ErrorMessage = "اسم الضريبة مطلوب")]
        [StringLength(200, ErrorMessage = "اسم الضريبة يجب ألا يتجاوز 200 حرف")]
        public string TaxName { get; set; }

        [Required(ErrorMessage = "نوع الضريبة مطلوب")]
        public TaxType TaxType { get; set; }

        [Required(ErrorMessage = "نسبة الضريبة مطلوبة")]
        [Range(0, 100, ErrorMessage = "نسبة الضريبة يجب أن تكون بين 0 و 100")]
        public decimal Rate { get; set; }

        public Guid? ApplicableAccountId { get; set; }

        public bool Status { get; set; } = true;

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update Tax Rule DTO - تحديث قاعدة ضريبة
    /// </summary>
    public class UpdateTaxRuleDto
    {
        [Required(ErrorMessage = "معرف قاعدة الضريبة مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "اسم الضريبة مطلوب")]
        [StringLength(200, ErrorMessage = "اسم الضريبة يجب ألا يتجاوز 200 حرف")]
        public string TaxName { get; set; }

        [Required(ErrorMessage = "نوع الضريبة مطلوب")]
        public TaxType TaxType { get; set; }

        [Required(ErrorMessage = "نسبة الضريبة مطلوبة")]
        [Range(0, 100, ErrorMessage = "نسبة الضريبة يجب أن تكون بين 0 و 100")]
        public decimal Rate { get; set; }

        public Guid? ApplicableAccountId { get; set; }

        public bool Status { get; set; } = true;

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Tax Rule List DTO - قائمة قواعد الضريبة
    /// </summary>
    public class TaxRuleListDto
    {
        [ExcelColumn(Name = "Tax Rule ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Tax Name")]
        public string TaxName { get; set; }

        [ExcelColumn(Name = "Tax Type")]
        public TaxType TaxType { get; set; }

        [ExcelColumn(Name = "Rate")]
        public decimal Rate { get; set; }

        [ExcelColumn(Name = "Status")]
        public bool Status { get; set; }

        public Guid? ApplicableAccountId { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Tax Rule Detail DTO - تفاصيل قاعدة الضريبة
    /// </summary>
    public class TaxRuleDetailDto : TaxRuleListDto
    {
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// Calculate Tax DTO - حساب الضريبة
    /// </summary>
    public class CalculateTaxDto
    {
        [Required(ErrorMessage = "معرف قاعدة الضريبة مطلوب")]
        public Guid TaxRuleId { get; set; }

        [Required(ErrorMessage = "المبلغ الأساسي مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal BaseAmount { get; set; }
    }

    /// <summary>
    /// Tax Calculation Result DTO - نتيجة حساب الضريبة
    /// </summary>
    public class TaxCalculationResultDto
    {
        public Guid TaxRuleId { get; set; }
        public string TaxName { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}


