using System;
using System.ComponentModel.DataAnnotations;
using MiniExcelLibs.Attributes;
using ZR.Model.Finance.Assets.Enums;

namespace ZR.Model.Finance.Assets.Dto
{
    /// <summary>
    /// Current Asset Query DTO - استعلام الأصول المتداولة
    /// </summary>
    public class CurrentAssetQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public string ItemCode { get; set; }
        /// <summary>
        /// Search in both NameAr and NameEn - البحث في الاسم العربي والإنجليزي
        /// </summary>
        public string ItemName { get; set; }
        public CurrentAssetStatus? Status { get; set; }
        public string Category { get; set; }
        public DateTime? AcquisitionDateFrom { get; set; }
        public DateTime? AcquisitionDateTo { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Create Current Asset DTO - إنشاء أصل متداول
    /// </summary>
    public class CreateCurrentAssetDto
    {
        [Required(ErrorMessage = "رمز الصنف مطلوب")]
        [StringLength(50)]
        public string ItemCode { get; set; }

        [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "تاريخ الشراء مطلوب")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "التكلفة مطلوبة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "التكلفة يجب أن تكون أكبر من صفر")]
        public decimal Cost { get; set; }

        public Guid? GLAccountId { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Update Current Asset DTO - تحديث أصل متداول
    /// </summary>
    public class UpdateCurrentAssetDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رمز الصنف مطلوب")]
        [StringLength(50)]
        public string ItemCode { get; set; }

        [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "تاريخ الشراء مطلوب")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "التكلفة مطلوبة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "التكلفة يجب أن تكون أكبر من صفر")]
        public decimal Cost { get; set; }

        public decimal CurrentValue { get; set; }

        public Guid? GLAccountId { get; set; }

        public CurrentAssetStatus Status { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Current Asset List DTO - قائمة الأصول المتداولة
    /// </summary>
    public class CurrentAssetListDto
    {
        [ExcelColumn(Name = "Asset ID")]
        public Guid Id { get; set; }

        [ExcelColumn(Name = "Item Code")]
        public string ItemCode { get; set; }

        [ExcelColumn(Name = "Name (Arabic)")]
        public string NameAr { get; set; }

        [ExcelColumn(Name = "Name (English)")]
        public string NameEn { get; set; }

        [ExcelColumn(Name = "Purchase Date", Format = "yyyy-MM-dd")]
        public DateTime PurchaseDate { get; set; }

        [ExcelColumn(Name = "Cost")]
        public decimal Cost { get; set; }

        [ExcelColumn(Name = "Current Value")]
        public decimal CurrentValue { get; set; }

        [ExcelColumn(Name = "Category")]
        public string Category { get; set; }

        [ExcelColumn(Name = "Status")]
        public CurrentAssetStatus Status { get; set; }

        public string StatusName => Status switch
        {
            CurrentAssetStatus.Active => "Active - نشط",
            CurrentAssetStatus.Consumed => "Consumed - مستهلك",
            CurrentAssetStatus.Transferred => "Transferred - محول",
            CurrentAssetStatus.WrittenOff => "Written Off - مشطوب",
            _ => Status.ToString()
        };

        public Guid? GLAccountId { get; set; }
    }

    /// <summary>
    /// Current Asset Detail DTO - تفاصيل الأصل المتداول
    /// </summary>
    public class CurrentAssetDetailDto
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Cost { get; set; }
        public decimal CurrentValue { get; set; }
        public Guid? GLAccountId { get; set; }
        public CurrentAssetStatus Status { get; set; }
        public string StatusName => Status switch
        {
            CurrentAssetStatus.Active => "Active - نشط",
            CurrentAssetStatus.Consumed => "Consumed - مستهلك",
            CurrentAssetStatus.Transferred => "Transferred - محول",
            CurrentAssetStatus.WrittenOff => "Written Off - مشطوب",
            _ => Status.ToString()
        };
        public string Category { get; set; }
        public string Notes { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}

