using System;
using System.ComponentModel.DataAnnotations;
using ZR.Model.CustomerAssets.Enums;

namespace ZR.Model.CustomerAssets.Dto
{
    /// <summary>
    /// Customer Asset Query DTO - استعلام أصل العميل
    /// </summary>
    public class CustomerAssetQueryDto : PagerInfo
    {
        public string AssetCode { get; set; }
        public string SerialNumber { get; set; }
        public string NameEn { get; set; }
        public Guid? AssetTypeId { get; set; }
        public InventoryAssetStatus? CurrentStatus { get; set; }
        public AssetCondition? CurrentCondition { get; set; }
    }

    /// <summary>
    /// Create Customer Asset DTO - إنشاء أصل عميل
    /// </summary>
    public class CreateCustomerAssetDto
    {
        [Required(ErrorMessage = "نوع الأصل مطلوب")]
        public Guid AssetTypeId { get; set; }

        [StringLength(100)]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "اسم الأصل بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "اسم الأصل بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        public AssetCondition CurrentCondition { get; set; } = AssetCondition.New;

        public DateTime? PurchaseDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? PurchaseCost { get; set; }

        public DateTime? WarrantyExpiry { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Update Customer Asset DTO - تحديث أصل عميل
    /// </summary>
    public class UpdateCustomerAssetDto
    {
        [Required(ErrorMessage = "معرف الأصل مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "نوع الأصل مطلوب")]
        public Guid AssetTypeId { get; set; }

        [StringLength(100)]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "اسم الأصل بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "اسم الأصل بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        public AssetCondition CurrentCondition { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? PurchaseCost { get; set; }

        public DateTime? WarrantyExpiry { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Customer Asset List DTO - قائمة أصول العملاء
    /// </summary>
    public class CustomerAssetListDto
    {
        public Guid Id { get; set; }
        public string AssetCode { get; set; }
        public string SerialNumber { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public AssetCondition CurrentCondition { get; set; }
        public InventoryAssetStatus CurrentStatus { get; set; }
        public Guid AssetTypeId { get; set; }
        public string AssetTypeNameEn { get; set; }
        public string AssetTypeNameAr { get; set; }
        public decimal? PurchaseCost { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// Customer Asset Detail DTO - تفاصيل أصل العميل
    /// </summary>
    public class CustomerAssetDetailDto : CustomerAssetListDto
    {
        public DateTime? PurchaseDate { get; set; }
        public string Notes { get; set; }
        public string Create_by { get; set; }
        public string Update_by { get; set; }
        public DateTime? Update_time { get; set; }
    }

    /// <summary>
    /// Available Asset Select DTO - الأصول المتاحة للقائمة المنسدلة
    /// </summary>
    public class AvailableAssetSelectDto
    {
        public Guid Id { get; set; }
        public string AssetCode { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string SerialNumber { get; set; }
        public string AssetTypeNameEn { get; set; }
        public string AssetTypeNameAr { get; set; }
    }
}
