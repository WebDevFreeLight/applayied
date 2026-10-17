using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.Finance.Tax.Dto
{
    #region Query DTOs

    /// <summary>
    /// Tax Group Query DTO
    /// </summary>
    public class TaxGroupQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public bool? IsActive { get; set; }
    }

    #endregion

    #region Create/Update DTOs

    /// <summary>
    /// Create Tax Group DTO
    /// </summary>
    public class CreateTaxGroupDto
    {
        public long? CompanyId { get; set; }

        [Required(ErrorMessage = "Group code is required - رمز المجموعة مطلوب")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "English name is required - الاسم بالإنجليزية مطلوب")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string NameAr { get; set; }

        [StringLength(1000)]
        public string DescriptionEn { get; set; }

        [StringLength(1000)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Is compound (tax on tax)?
        /// </summary>
        public bool IsCompound { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Tax codes in this group with their sequence
        /// </summary>
        public List<TaxGroupLineDto> Lines { get; set; } = new List<TaxGroupLineDto>();
    }

    /// <summary>
    /// Update Tax Group DTO
    /// </summary>
    public class UpdateTaxGroupDto
    {
        [Required]
        public Guid Id { get; set; }

        public long? CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string NameAr { get; set; }

        [StringLength(1000)]
        public string DescriptionEn { get; set; }

        [StringLength(1000)]
        public string DescriptionAr { get; set; }

        public bool IsCompound { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }

        /// <summary>
        /// Tax codes in this group with their sequence
        /// </summary>
        public List<TaxGroupLineDto> Lines { get; set; } = new List<TaxGroupLineDto>();
    }

    /// <summary>
    /// Tax Group Line DTO (for create/update)
    /// </summary>
    public class TaxGroupLineDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Tax code is required")]
        public Guid TaxCodeId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Sequence must be between 1 and 100")]
        public int Sequence { get; set; }

        /// <summary>
        /// Override rate (null = use tax code default)
        /// </summary>
        public decimal? OverrideRate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    #endregion

    #region Response DTOs

    /// <summary>
    /// Tax Group Response DTO
    /// </summary>
    public class TaxGroupResponseDto
    {
        public Guid Id { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public bool IsCompound { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public decimal CombinedRate { get; set; }
        public string CombinedRateDisplay { get; set; }
        public List<TaxGroupLineResponseDto> Lines { get; set; } = new List<TaxGroupLineResponseDto>();
        public DateTime? CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateBy { get; set; }
    }

    /// <summary>
    /// Tax Group Line Response DTO
    /// </summary>
    public class TaxGroupLineResponseDto
    {
        public Guid Id { get; set; }
        public Guid TaxGroupId { get; set; }
        public Guid TaxCodeId { get; set; }
        public string TaxCode { get; set; }
        public string TaxCodeNameEn { get; set; }
        public string TaxCodeNameAr { get; set; }
        public int Sequence { get; set; }
        public decimal? OverrideRate { get; set; }
        public decimal EffectiveRate { get; set; }
        public string EffectiveRateDisplay { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Tax Group List Item DTO (for dropdowns)
    /// </summary>
    public class TaxGroupListItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public decimal CombinedRate { get; set; }
        public bool IsCompound { get; set; }
        public bool IsActive { get; set; }
        public int TaxCodeCount { get; set; }

        public string DisplayLabel => $"{Code} ({CombinedRate:0.##}%)";
    }

    #endregion
}
