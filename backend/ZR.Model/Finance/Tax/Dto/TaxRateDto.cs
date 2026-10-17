using System;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.Finance.Tax.Dto
{
    #region Query DTOs

    /// <summary>
    /// Tax Rate Query DTO
    /// </summary>
    public class TaxRateQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public Guid? TaxCodeId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IncludeExpired { get; set; }
    }

    #endregion

    #region Create/Update DTOs

    /// <summary>
    /// Create Tax Rate DTO
    /// </summary>
    public class CreateTaxRateDto
    {
        [Required(ErrorMessage = "Tax code is required - رمز الضريبة مطلوب")]
        public Guid TaxCodeId { get; set; }

        [Required(ErrorMessage = "Rate is required - المعدل مطلوب")]
        [Range(0, 100, ErrorMessage = "Rate must be between 0 and 100")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Effective from date is required - تاريخ البداية مطلوب")]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [StringLength(500)]
        public string ChangeReason { get; set; }

        [StringLength(200)]
        public string LegalReference { get; set; }

        public bool IsApproved { get; set; } = true;
    }

    /// <summary>
    /// Update Tax Rate DTO
    /// </summary>
    public class UpdateTaxRateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid TaxCodeId { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Rate { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [StringLength(500)]
        public string ChangeReason { get; set; }

        [StringLength(200)]
        public string LegalReference { get; set; }

        public bool IsApproved { get; set; }
    }

    /// <summary>
    /// End Tax Rate DTO (set effective_to date)
    /// </summary>
    public class EndTaxRateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "End date is required - تاريخ النهاية مطلوب")]
        public DateTime EffectiveTo { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }
    }

    #endregion

    #region Response DTOs

    /// <summary>
    /// Tax Rate Response DTO
    /// </summary>
    public class TaxRateResponseDto
    {
        public Guid Id { get; set; }
        public Guid TaxCodeId { get; set; }
        public string TaxCode { get; set; }
        public string TaxCodeNameEn { get; set; }
        public string TaxCodeNameAr { get; set; }
        public decimal Rate { get; set; }
        public string RateDisplay { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string ChangeReason { get; set; }
        public string LegalReference { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsCurrentlyEffective { get; set; }
        public bool IsExpired { get; set; }
        public bool IsFuture { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateBy { get; set; }
    }

    /// <summary>
    /// Get Rate For Date Request
    /// </summary>
    public class GetRateForDateRequest
    {
        [Required]
        public Guid TaxCodeId { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Get Rate For Date Response
    /// </summary>
    public class GetRateForDateResponse
    {
        public Guid TaxCodeId { get; set; }
        public string TaxCode { get; set; }
        public DateTime Date { get; set; }
        public decimal? Rate { get; set; }
        public Guid? RateId { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool RateFound { get; set; }
        public string ErrorMessage { get; set; }
    }

    #endregion
}
