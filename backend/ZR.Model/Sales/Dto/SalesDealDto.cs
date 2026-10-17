using System;
using System.Collections.Generic;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ZR.Model.Sales.Dto
{
    /// <summary>
    /// Deal query parameters
    /// </summary>
    public class DealQueryDto : PagerInfo
    {
        /// <summary>
        /// Filter by Lead ID
        /// </summary>
        public long? LeadId { get; set; }

        /// <summary>
        /// Filter by Deal Status
        /// </summary>
        public string DealStatus { get; set; }

        /// <summary>
        /// Filter by Assigned Employee
        /// </summary>
        public long? AssignedTo { get; set; }

        /// <summary>
        /// Filter by Priority
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Filter by Deal Title (search)
        /// </summary>
        public string DealTitle { get; set; }

        /// <summary>
        /// Filter by Expected Close Date start
        /// </summary>
        public DateTime? ExpectedCloseDateStart { get; set; }

        /// <summary>
        /// Filter by Expected Close Date end
        /// </summary>
        public DateTime? ExpectedCloseDateEnd { get; set; }

        #region Role-Based Filtering Properties (Set by Controller)

        /// <summary>
        /// Current user ID (set by controller for role-based filtering)
        /// </summary>
        [JsonIgnore]
        public long? CurrentUserId { get; set; }

        /// <summary>
        /// Whether current user is admin (set by controller)
        /// </summary>
        [JsonIgnore]
        public bool IsCurrentUserAdmin { get; set; }

        #endregion
    }

    /// <summary>
    /// Create deal DTO
    /// </summary>
    public class CreateDealDto
    {
        public string DealTitle { get; set; }
        public long LeadId { get; set; }
        public decimal DealValue { get; set; }
        public string Currency { get; set; } = "SAR";
        public long AssignedTo { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Notes { get; set; }
        public string DealType { get; set; } = "Rent";
        public decimal? SharePercentage { get; set; }
    }

    /// <summary>
    /// Update deal DTO
    /// </summary>
    public class UpdateDealDto
    {
        public long DealId { get; set; }
        public string DealTitle { get; set; }
        public long LeadId { get; set; }
        public decimal DealValue { get; set; }
        public string Currency { get; set; }
        public string DealStatus { get; set; }
        public string DealStage { get; set; }
        public long AssignedTo { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public string WonReason { get; set; }
        public string LostReason { get; set; }
        public string Notes { get; set; }
        public string Priority { get; set; }
        public string DealType { get; set; }
        public decimal? SharePercentage { get; set; }
    }

    /// <summary>
    /// Update deal status DTO
    /// </summary>
    public class UpdateDealStatusDto
    {
        /// <summary>
        /// Deal ID
        /// </summary>
        public long DealId { get; set; }

        /// <summary>
        /// New status: Under Negotiation, Won, Lost
        /// </summary>
        public string NewStatus { get; set; }

        /// <summary>
        /// Notes/Reason for status change (mandatory for Won/Lost)
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// Deal response DTO with joined fields
    /// </summary>
    public class DealResponseDto
    {
        public long DealId { get; set; }
        public long LeadId { get; set; }
        public string LeadName { get; set; }
        public string DealTitle { get; set; }
        public decimal DealValue { get; set; }
        public string Currency { get; set; }
        public string DealStatus { get; set; }
        public string DealStage { get; set; }
        public long AssignedTo { get; set; }
        public string AssignedToName { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public string WonReason { get; set; }
        public string LostReason { get; set; }
        public string Notes { get; set; }
        public string Priority { get; set; }
        public long? SeasonId { get; set; }
        public bool IsConverted { get; set; }
        public string DealType { get; set; }
        public decimal? SharePercentage { get; set; }
        public string Create_by { get; set; }
        public DateTime Create_time { get; set; }
        public string Update_by { get; set; }
        public DateTime? Update_time { get; set; }
    }

    /// <summary>
    /// Dashboard statistics DTO
    /// </summary>
    public class DealDashboardDto
    {
        public int OpenDeals { get; set; }
        public int WonDeals { get; set; }
        public int LostDeals { get; set; }
        public int TotalDeals { get; set; }
        public decimal TotalPipelineValue { get; set; }
        public decimal WonValue { get; set; }
        public decimal LostValue { get; set; }
    }
}
