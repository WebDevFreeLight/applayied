using System;

namespace ZR.Model.Operations.Readiness.Dto
{
    // ─── Requirement DTOs ───

    /// <summary>
    /// Query requirements
    /// </summary>
    public class HandoverRequirementQueryDto : PagerInfo
    {
        public bool? IsActive { get; set; }
        public string Code { get; set; }
    }

    /// <summary>
    /// Create/update requirement
    /// </summary>
    public class HandoverRequirementDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RequirementId { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public decimal Percentage { get; set; }
        public bool PhotoRequired { get; set; }
        public bool IsBlocking { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Percentage validation result
    /// </summary>
    public class RequirementValidationDto
    {
        public decimal TotalPercentage { get; set; }
        public bool IsValid { get; set; }
        public int ActiveCount { get; set; }
    }

    // ─── Store Handover DTOs ───

    /// <summary>
    /// Query handovers (paginated)
    /// </summary>
    public class StoreHandoverQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ShopId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SeasonId { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Initialize handover for a shop
    /// </summary>
    public class InitializeHandoverDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ShopId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SeasonId { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Complete/uncomplete a handover item
    /// </summary>
    public class CompleteItemDto
    {
        public string PhotoUrls { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Request handover approval
    /// </summary>
    public class RequestHandoverDto
    {
        public string Notes { get; set; }
    }

    /// <summary>
    /// Approve handover
    /// </summary>
    public class ApproveHandoverDto
    {
        public string Notes { get; set; }
    }

    /// <summary>
    /// Reject handover
    /// </summary>
    public class RejectHandoverDto
    {
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// Progress summary
    /// </summary>
    public class HandoverProgressDto
    {
        public decimal Progress { get; set; }
        public bool CanRequestHandover { get; set; }
        public int TotalItems { get; set; }
        public int CompletedItems { get; set; }
        public string[] BlockingIncomplete { get; set; }
    }
}
