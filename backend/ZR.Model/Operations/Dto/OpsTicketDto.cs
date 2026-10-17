using System;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ZR.Model.Operations.Dto
{
    /// <summary>
    /// Ticket Query DTO
    /// </summary>
    public class OpsTicketQueryDto : PagerInfo
    {
        public long? LeadId { get; set; }
        public long? DealId { get; set; }
        public string TicketStatus { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public long? AssignedTo { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public long? CurrentUserId { get; set; }

        [JsonIgnore]
        public bool IsCurrentUserAdmin { get; set; }
    }

    /// <summary>
    /// Create Ticket DTO
    /// </summary>
    public class CreateOpsTicketDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public long? LeadId { get; set; }
        public long? DealId { get; set; }
        public string Category { get; set; } = "Operational";
        public string Priority { get; set; } = "Medium";
        public long? AssignedTo { get; set; }
    }

    /// <summary>
    /// Update Ticket DTO
    /// </summary>
    public class UpdateOpsTicketDto
    {
        public long TicketId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long? LeadId { get; set; }
        public long? DealId { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public long? AssignedTo { get; set; }
    }

    /// <summary>
    /// Update Ticket Status DTO
    /// </summary>
    public class UpdateTicketStatusDto
    {
        public long TicketId { get; set; }
        public string NewStatus { get; set; }
        public string Notes { get; set; }
        /// <summary>
        /// If true and ticket has a DealId, escalate findings to the deal
        /// </summary>
        public bool EscalateToDeal { get; set; } = false;
    }

    /// <summary>
    /// Ticket Dashboard DTO
    /// </summary>
    public class TicketDashboardDto
    {
        public int OpenCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
        public int TotalTickets { get; set; }
        public int EscalatedCount { get; set; }
    }
}
