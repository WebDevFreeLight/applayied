using System;
using System.ComponentModel.DataAnnotations;
using ZR.Model.Finance.Enums;

namespace ZR.Model.Finance.Tax.Dto
{
    #region Query DTOs

    /// <summary>
    /// Tax Code Query DTO
    /// </summary>
    public class TaxCodeQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public TaxType? TaxType { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
        public string CountryCode { get; set; }
    }

    #endregion

    #region Create/Update DTOs

    /// <summary>
    /// Create Tax Code DTO
    /// </summary>
    public class CreateTaxCodeDto
    {
        public long? CompanyId { get; set; }

        [Required(ErrorMessage = "Tax code is required - رمز الضريبة مطلوب")]
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

        public TaxType TaxType { get; set; } = TaxType.Standard;

        public bool IsPercent { get; set; } = true;

        [Required(ErrorMessage = "Tax rate is required - معدل الضريبة مطلوب")]
        [Range(0, 100, ErrorMessage = "Rate must be between 0 and 100")]
        public decimal Rate { get; set; }

        public Guid? DefaultPayableAccountId { get; set; }

        public Guid? DefaultRecoverableAccountId { get; set; }

        public Guid? ReverseChargeAccountId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [StringLength(10)]
        public string CountryCode { get; set; }
    }

    /// <summary>
    /// Update Tax Code DTO
    /// </summary>
    public class UpdateTaxCodeDto
    {
        [Required]
        public Guid Id { get; set; }

        public long? CompanyId { get; set; }

        [Required(ErrorMessage = "Tax code is required")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "English name is required")]
        [StringLength(200)]
        public string NameEn { get; set; }

        [StringLength(200)]
        public string NameAr { get; set; }

        [StringLength(1000)]
        public string DescriptionEn { get; set; }

        [StringLength(1000)]
        public string DescriptionAr { get; set; }

        public TaxType TaxType { get; set; }

        public bool IsPercent { get; set; }

        [Range(0, 100)]
        public decimal Rate { get; set; }

        public Guid? DefaultPayableAccountId { get; set; }

        public Guid? DefaultRecoverableAccountId { get; set; }

        public Guid? ReverseChargeAccountId { get; set; }

        public bool IsActive { get; set; }

        public bool IsDefault { get; set; }

        public int SortOrder { get; set; }

        [StringLength(10)]
        public string CountryCode { get; set; }
    }

    #endregion

    #region Response DTOs

    /// <summary>
    /// Tax Code Response DTO
    /// </summary>
    public class TaxCodeResponseDto
    {
        public Guid Id { get; set; }
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public TaxType TaxType { get; set; }
        public string TaxTypeName { get; set; }
        public bool IsPercent { get; set; }
        public decimal Rate { get; set; }
        public string RateDisplay { get; set; }
        public Guid? DefaultPayableAccountId { get; set; }
        public string DefaultPayableAccountNumber { get; set; }
        public string DefaultPayableAccountName { get; set; }
        public Guid? DefaultRecoverableAccountId { get; set; }
        public string DefaultRecoverableAccountNumber { get; set; }
        public string DefaultRecoverableAccountName { get; set; }
        public Guid? ReverseChargeAccountId { get; set; }
        public string ReverseChargeAccountNumber { get; set; }
        public string ReverseChargeAccountName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public string CountryCode { get; set; }
        public decimal EffectiveRate { get; set; }
        public string EffectiveRateDisplay { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateBy { get; set; }
    }

    /// <summary>
    /// Tax Code List Item DTO (for dropdowns/selectors)
    /// </summary>
    public class TaxCodeListItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public decimal Rate { get; set; }
        public bool IsPercent { get; set; }
        public TaxType TaxType { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Display label: "VAT5 (5%)" or "EXEMPT (0%)"
        /// </summary>
        public string DisplayLabel => IsPercent
            ? $"{Code} ({Rate:0.##}%)"
            : $"{Code} ({Rate:0.00})";
    }

    #endregion
}
