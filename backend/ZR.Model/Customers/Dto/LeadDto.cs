using System;
using System.Collections.Generic;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ZR.Model.Customers.Dto
{
    /// <summary>
    /// Lead query parameters
    /// </summary>
    public class LeadQueryDto : PagerInfo
    {
        public string BrandName { get; set; }
        public long? AssignedTo { get; set; }
        public string LeadStatus { get; set; }
        public long? LeadType { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }

        /// <summary>
        /// General search across BrandName, OwnerName, OwnerPhone, OwnerEmail
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Filter by multiple statuses (set by controller for role-based filtering)
        /// </summary>
        [JsonIgnore]
        public string[] AllowedStatuses { get; set; }

        #region Role-Based Filtering Properties (Set by Controller)

        [JsonIgnore]
        public long? CurrentUserId { get; set; }

        [JsonIgnore]
        public long? CurrentUserEmployeeId { get; set; }

        [JsonIgnore]
        public bool IsCurrentUserAdmin { get; set; }

        /// <summary>
        /// When true, role-based filter uses SalesAssignedTo instead of AssignedTo
        /// </summary>
        [JsonIgnore]
        public bool FilterBySalesAssignment { get; set; }

        /// <summary>
        /// When true, only show leads where ReadyForOperations = true
        /// </summary>
        [JsonIgnore]
        public bool? FilterByReadyForOperations { get; set; }

        #endregion
    }

    /// <summary>
    /// Lead creation DTO
    /// </summary>
    public class LeadCreateDto
    {
        public long LeadType { get; set; }
        public string BrandName { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }
        public long AssignedTo { get; set; }
        public string QualificationNotes { get; set; }
        public string InternalNotes { get; set; }
        public string BudgetRange { get; set; }
    }

    /// <summary>
    /// Lead update DTO
    /// </summary>
    public class LeadUpdateDto
    {
        public long LeadId { get; set; }
        public long LeadType { get; set; }
        public string BrandName { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }
        public long AssignedTo { get; set; }
        public string LeadStatus { get; set; }
        public string QualificationNotes { get; set; }
        public string RejectionNotes { get; set; }
        public string InternalNotes { get; set; }
        public string BudgetRange { get; set; }
    }

    /// <summary>
    /// Update lead status DTO
    /// </summary>
    public class UpdateLeadStatusDto
    {
        public long LeadId { get; set; }
        public string NewStatus { get; set; }
        public string Notes { get; set; }

        // Qualification fields (when NewStatus == "Qualified")
        public string CompanyName { get; set; }  // optional
        public string BrandName { get; set; }    // optional
        public string ActivityType { get; set; }
        public string Interesting { get; set; }
        public string LocationInterested { get; set; }  // JSON array of location IDs
        public string ContractTypeInterested { get; set; }  // "share" or "rent"
        public long? SalesAssignedTo { get; set; }
    }

    /// <summary>
    /// Update sales assignment DTO (admin only)
    /// </summary>
    public class UpdateSalesAssignmentDto
    {
        public long LeadId { get; set; }
        public long SalesAssignedTo { get; set; }
    }
}
