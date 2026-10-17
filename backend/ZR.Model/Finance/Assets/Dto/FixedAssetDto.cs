using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Assets.Enums;

namespace ZR.Model.Finance.Assets.Dto
{
    /// <summary>
    /// Fixed Asset Query DTO - استعلام الأصول الثابتة
    /// </summary>
    public class FixedAssetQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string AssetCode { get; set; }
        /// <summary>
        /// Search in both NameAr and NameEn - البحث في الاسم العربي والإنجليزي
        /// </summary>
        public string AssetName { get; set; }
        public Guid? CategoryId { get; set; }
        public AssetStatus? Status { get; set; }
        public DateTime? PurchaseDateFrom { get; set; }
        public DateTime? PurchaseDateTo { get; set; }
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Fixed Asset DTO - إنشاء أصل ثابت
    /// </summary>
    public class CreateFixedAssetDto
    {
        /// <summary>
        /// Asset code - auto-generated if not provided
        /// رمز الأصل - يتم توليده تلقائياً إذا لم يتم توفيره
        /// </summary>
        [StringLength(50)]
        public string AssetCode { get; set; }

        [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "تاريخ الشراء مطلوب")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "تكلفة الشراء مطلوبة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "تكلفة الشراء يجب أن تكون أكبر من صفر")]
        public decimal PurchaseCost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "القيمة التخريدية يجب أن تكون صفر أو أكثر")]
        public decimal SalvageValue { get; set; }

        [Required(ErrorMessage = "فئة الأصل مطلوبة")]
        public Guid CategoryId { get; set; }

        public Guid? GLAssetAccountId { get; set; }
        public Guid? GLAccumulatedDepreciationAccountId { get; set; }
        public Guid? GLDepreciationExpenseAccountId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Update Fixed Asset DTO - تحديث أصل ثابت
    /// </summary>
    public class UpdateFixedAssetDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رمز الأصل مطلوب")]
        [StringLength(50)]
        public string AssetCode { get; set; }

        [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "تاريخ الشراء مطلوب")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "تكلفة الشراء مطلوبة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "تكلفة الشراء يجب أن تكون أكبر من صفر")]
        public decimal PurchaseCost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "القيمة التخريدية يجب أن تكون صفر أو أكثر")]
        public decimal SalvageValue { get; set; }

        [Required(ErrorMessage = "فئة الأصل مطلوبة")]
        public Guid CategoryId { get; set; }

        public Guid? GLAssetAccountId { get; set; }
        public Guid? GLAccumulatedDepreciationAccountId { get; set; }
        public Guid? GLDepreciationExpenseAccountId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Fixed Asset List DTO - قائمة الأصول الثابتة
    /// </summary>
    public class FixedAssetListDto
    {
        [ExcelColumn(Name = "Asset ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Asset Code")]
        public string AssetCode { get; set; }

        [ExcelColumn(Name = "Name (Arabic)")]
        public string NameAr { get; set; }

        [ExcelColumn(Name = "Name (English)")]
        public string NameEn { get; set; }

        [ExcelColumn(Name = "Purchase Date", Format = "yyyy-MM-dd")]
        public DateTime PurchaseDate { get; set; }

        [ExcelColumn(Name = "Purchase Cost")]
        public decimal PurchaseCost { get; set; }

        [ExcelColumn(Name = "Salvage Value")]
        public decimal SalvageValue { get; set; }

        [ExcelColumn(Name = "Current Book Value")]
        public decimal CurrentBookValue { get; set; }

        [ExcelColumn(Name = "Accumulated Depreciation")]
        public decimal AccumulatedDepreciation { get; set; }

        [ExcelColumn(Name = "Status")]
        public AssetStatus Status { get; set; }

        public string StatusName => Status switch
        {
            AssetStatus.Active => "Active - نشط",
            AssetStatus.Sold => "Sold - مباع",
            AssetStatus.Disposed => "Disposed - مستبعد",
            AssetStatus.FullyDepreciated => "Fully Depreciated - مهلك بالكامل",
            _ => Status.ToString()
        };

        public Guid CategoryId { get; set; }
        public string CategoryNameAr { get; set; }
        public string CategoryNameEn { get; set; }

        public DateTime? LastDepreciationDate { get; set; }
    }

    /// <summary>
    /// Fixed Asset Detail DTO - تفاصيل الأصل الثابت
    /// </summary>
    public class FixedAssetDetailDto
    {
        public Guid Id { get; set; }
        public string AssetCode { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal SalvageValue { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryNameAr { get; set; }
        public string CategoryNameEn { get; set; }
        public string CategoryCode { get; set; }
        public Guid? GLAssetAccountId { get; set; }
        public Guid? GLAccumulatedDepreciationAccountId { get; set; }
        public Guid? GLDepreciationExpenseAccountId { get; set; }
        public decimal CurrentBookValue { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public DateTime? LastDepreciationDate { get; set; }
        public AssetStatus Status { get; set; }
        public string StatusName => Status switch
        {
            AssetStatus.Active => "Active - نشط",
            AssetStatus.Sold => "Sold - مباع",
            AssetStatus.Disposed => "Disposed - مستبعد",
            AssetStatus.FullyDepreciated => "Fully Depreciated - مهلك بالكامل",
            _ => Status.ToString()
        };
        public string Notes { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Calculated fields
        public decimal DepreciableAmount => PurchaseCost - SalvageValue;
        public decimal RemainingDepreciableAmount => CurrentBookValue - SalvageValue;
        public bool CanDepreciate => Status == AssetStatus.Active && RemainingDepreciableAmount > 0;
    }

    /// <summary>
    /// Asset Sale DTO - بيع الأصل
    /// </summary>
    public class AssetSaleDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid AssetId { get; set; }

        [Required(ErrorMessage = "تاريخ البيع مطلوب")]
        public DateTime SaleDate { get; set; }

        [Required(ErrorMessage = "سعر البيع مطلوب")]
        [Range(0, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون صفر أو أكثر")]
        public decimal SalePrice { get; set; }

        /// <summary>
        /// Cash/Bank Account ID to receive sale proceeds - حساب النقد/البنك لاستلام عائدات البيع
        /// </summary>
        [Required(ErrorMessage = "حساب النقد/البنك مطلوب")]
        public Guid CashAccountId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }
    }

    /// <summary>
    /// Asset Disposal DTO - استبعاد الأصل
    /// </summary>
    public class AssetDisposalDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid AssetId { get; set; }

        [Required(ErrorMessage = "تاريخ الاستبعاد مطلوب")]
        public DateTime DisposalDate { get; set; }

        [StringLength(1000)]
        public string DisposalReason { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }
    }

    /// <summary>
    /// Asset Revaluation DTO - إعادة تقييم الأصل
    /// </summary>
    public class AssetRevaluationDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid AssetId { get; set; }

        [Required(ErrorMessage = "تاريخ إعادة التقييم مطلوب")]
        public DateTime RevaluationDate { get; set; }

        [Required(ErrorMessage = "القيمة الجديدة مطلوبة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "القيمة الجديدة يجب أن تكون أكبر من صفر")]
        public decimal NewValue { get; set; }

        [StringLength(1000)]
        public string Reason { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }
    }
}

