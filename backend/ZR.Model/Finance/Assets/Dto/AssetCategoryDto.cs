using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Assets.Enums;

namespace ZR.Model.Finance.Assets.Dto
{
    /// <summary>
    /// Asset Category Query DTO - استعلام فئات الأصول
    /// </summary>
    public class AssetCategoryQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string CategoryCode { get; set; }
        /// <summary>
        /// Search in both NameAr and NameEn - البحث في الاسم العربي والإنجليزي
        /// </summary>
        public string CategoryName { get; set; }
        public DepreciationMethod? DefaultDepreciationMethod { get; set; }
        public bool? IsActive { get; set; }
        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Create Asset Category DTO - إنشاء فئة أصول
    /// </summary>
    public class CreateAssetCategoryDto
    {
        /// <summary>
        /// Category code - auto-generated if not provided
        /// رمز الفئة - يتم توليده تلقائياً إذا لم يتم توفيره
        /// </summary>
        [StringLength(50)]
        public string CategoryCode { get; set; }

        [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "طريقة الإهلاك مطلوبة")]
        public DepreciationMethod DepreciationMethod { get; set; }

        [Range(0, 1200, ErrorMessage = "العمر الإنتاجي يجب أن يكون بين 0 و 1200 شهر")]
        public int UsefulLifeMonths { get; set; }

        public Guid? DefaultAssetAccountId { get; set; }
        public Guid? DefaultDepreciationExpenseAccountId { get; set; }
        public Guid? DefaultAccumulatedDepreciationAccountId { get; set; }
        public Guid? DefaultGainLossAccountId { get; set; }

        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Update Asset Category DTO - تحديث فئة أصول
    /// </summary>
    public class UpdateAssetCategoryDto
    {
        [Required(ErrorMessage = "معرف الفئة مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رمز الفئة مطلوب")]
        [StringLength(50)]
        public string CategoryCode { get; set; }

        [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "طريقة الإهلاك مطلوبة")]
        public DepreciationMethod DepreciationMethod { get; set; }

        [Range(0, 1200, ErrorMessage = "العمر الإنتاجي يجب أن يكون بين 0 و 1200 شهر")]
        public int UsefulLifeMonths { get; set; }

        public Guid? DefaultAssetAccountId { get; set; }
        public Guid? DefaultDepreciationExpenseAccountId { get; set; }
        public Guid? DefaultAccumulatedDepreciationAccountId { get; set; }
        public Guid? DefaultGainLossAccountId { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Company ID for filtering
        /// معرف الشركة
        /// </summary>
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Asset Category List DTO - قائمة فئات الأصول
    /// </summary>
    public class AssetCategoryListDto
    {
        [ExcelColumn(Name = "Category ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Category Code")]
        public string CategoryCode { get; set; }

        [ExcelColumn(Name = "Name (Arabic)")]
        public string NameAr { get; set; }

        [ExcelColumn(Name = "Name (English)")]
        public string NameEn { get; set; }

        [ExcelColumn(Name = "Depreciation Method")]
        public DepreciationMethod DepreciationMethod { get; set; }

        public string DepreciationMethodName => DepreciationMethod switch
        {
            Enums.DepreciationMethod.StraightLine => "Straight Line - القسط الثابت",
            Enums.DepreciationMethod.DecliningBalance => "Declining Balance - القسط المتناقص",
            Enums.DepreciationMethod.None => "None - بدون إهلاك",
            _ => DepreciationMethod.ToString()
        };

        [ExcelColumn(Name = "Useful Life (Months)")]
        public int UsefulLifeMonths { get; set; }

        [ExcelColumn(Name = "Is Active")]
        public bool IsActive { get; set; }

        public int AssetCount { get; set; }
    }

    /// <summary>
    /// Asset Category Detail DTO - تفاصيل فئة الأصول
    /// </summary>
    public class AssetCategoryDetailDto
    {
        public Guid Id { get; set; }
        public string CategoryCode { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public string DepreciationMethodName => DepreciationMethod switch
        {
            Enums.DepreciationMethod.StraightLine => "Straight Line - القسط الثابت",
            Enums.DepreciationMethod.DecliningBalance => "Declining Balance - القسط المتناقص",
            Enums.DepreciationMethod.None => "None - بدون إهلاك",
            _ => DepreciationMethod.ToString()
        };
        public int UsefulLifeMonths { get; set; }
        public Guid? DefaultAssetAccountId { get; set; }
        public Guid? DefaultDepreciationExpenseAccountId { get; set; }
        public Guid? DefaultAccumulatedDepreciationAccountId { get; set; }
        public Guid? DefaultGainLossAccountId { get; set; }
        public bool IsActive { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}

