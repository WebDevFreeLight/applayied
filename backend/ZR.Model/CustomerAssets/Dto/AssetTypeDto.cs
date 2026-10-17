using System;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.CustomerAssets.Dto
{
    /// <summary>
    /// Asset Type Query DTO - استعلام نوع الأصل
    /// </summary>
    public class AssetTypeQueryDto : PagerInfo
    {
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Create Asset Type DTO - إنشاء نوع أصل
    /// </summary>
    public class CreateAssetTypeDto
    {
        [Required(ErrorMessage = "رمز نوع الأصل مطلوب")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "اسم نوع الأصل بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "اسم نوع الأصل بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update Asset Type DTO - تحديث نوع أصل
    /// </summary>
    public class UpdateAssetTypeDto
    {
        [Required(ErrorMessage = "معرف نوع الأصل مطلوب")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "رمز نوع الأصل مطلوب")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "اسم نوع الأصل بالإنجليزي مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [Required(ErrorMessage = "اسم نوع الأصل بالعربي مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Asset Type List DTO - قائمة أنواع الأصول
    /// </summary>
    public class AssetTypeListDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// Asset Type Select DTO - نوع الأصل للقائمة المنسدلة
    /// </summary>
    public class AssetTypeSelectDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
    }
}
