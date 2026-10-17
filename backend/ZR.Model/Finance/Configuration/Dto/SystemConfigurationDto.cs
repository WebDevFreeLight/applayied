using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;

namespace ZR.Model.Finance.Configuration.Dto
{
    /// <summary>
    /// Configuration Query DTO - استعلام الإعدادات
    /// </summary>
    public class SystemConfigurationQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string Key { get; set; }
        public string ValueType { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }

        public DateTime? BeginCreateTime { get; set; }
        public DateTime? EndCreateTime { get; set; }
    }

    /// <summary>
    /// Create Configuration DTO - إنشاء إعداد جديد
    /// </summary>
    public class CreateConfigurationDto
    {
        [Required(ErrorMessage = "مفتاح الإعداد مطلوب")]
        [StringLength(200, ErrorMessage = "مفتاح الإعداد يجب ألا يتجاوز 200 حرف")]
        public string Key { get; set; }

        [Required(ErrorMessage = "قيمة الإعداد مطلوبة")]
        [StringLength(2000, ErrorMessage = "قيمة الإعداد يجب ألا تتجاوز 2000 حرف")]
        public string Value { get; set; }

        [Required(ErrorMessage = "نوع القيمة مطلوب")]
        [StringLength(50)]
        [RegularExpression("^(string|int|bool|json)$", ErrorMessage = "نوع القيمة يجب أن يكون: string, int, bool, json")]
        public string ValueType { get; set; } = "string";

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Update Configuration DTO - تحديث إعداد
    /// </summary>
    public class UpdateConfigurationDto
    {
        [Required(ErrorMessage = "معرف الإعداد مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "مفتاح الإعداد مطلوب")]
        [StringLength(200, ErrorMessage = "مفتاح الإعداد يجب ألا يتجاوز 200 حرف")]
        public string Key { get; set; }

        [Required(ErrorMessage = "قيمة الإعداد مطلوبة")]
        [StringLength(2000, ErrorMessage = "قيمة الإعداد يجب ألا تتجاوز 2000 حرف")]
        public string Value { get; set; }

        [Required(ErrorMessage = "نوع القيمة مطلوب")]
        [StringLength(50)]
        [RegularExpression("^(string|int|bool|json)$", ErrorMessage = "نوع القيمة يجب أن يكون: string, int, bool, json")]
        public string ValueType { get; set; }

        [StringLength(500)]
        public string DescriptionAr { get; set; }

        [StringLength(500)]
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Configuration Response DTO - استجابة الإعداد
    /// </summary>
    public class ConfigurationResponseDto
    {
        [ExcelColumn(Name = "Configuration ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Key")]
        public string Key { get; set; }

        [ExcelColumn(Name = "Value")]
        public string Value { get; set; }

        [ExcelColumn(Name = "Value Type")]
        public string ValueType { get; set; }

        [ExcelColumn(Name = "Description (Arabic)")]
        public string DescriptionAr { get; set; }

        [ExcelColumn(Name = "Description (English)")]
        public string DescriptionEn { get; set; }

        public long? TenantId { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

