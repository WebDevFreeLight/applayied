using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Converter;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ZR.Model.EventManagement.Dto
{
    // ==================== Event Incident ====================
    public class EventIncidentQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }
        public string IncidentNumber { get; set; }
        public string IncidentType { get; set; }
        public string Priority { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public DateTime? BeginIncidentTime { get; set; }
        public DateTime? EndIncidentTime { get; set; }
        public DateTime? BeginReportedDate { get; set; }
        public DateTime? EndReportedDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssignedToId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GateId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? IncidentTypeId { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string ReportedBy { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Event Incident Create DTO - Only fields allowed during creation
    /// </summary>
    public class EventIncidentCreateDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Event ID is required")]
        public long EventId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? IncidentTypeId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string TitleAr { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public string DescriptionAr { get; set; }

        public string Priority { get; set; } // Low, Medium, High, Critical

        public string Severity { get; set; } // Minor, Major, Critical

        [Required(ErrorMessage = "Incident Time is required")]
        public DateTime IncidentTime { get; set; }

        [Required(ErrorMessage = "Reported Date is required")]
        public DateTime ReportedDate { get; set; }

        [Required(ErrorMessage = "Reported By is required")]
        public string ReportedBy { get; set; }

        public string ReporterType { get; set; } // Staff, Visitor, Security, Vendor, Other

        public string ReporterContact { get; set; }

        public string Location { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GateId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? BoothId { get; set; }

        public string Attachments { get; set; } // JSON array of URLs or file paths

        public string WitnessInformation { get; set; }
    }

    /// <summary>
    /// Event Incident Update DTO - Workflow-based field groups
    /// </summary>
    public class EventIncidentUpdateDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Incident ID is required")]
        public long IncidentId { get; set; }

        // (1) General editable fields
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string Priority { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GateId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? BoothId { get; set; }
        public string Attachments { get; set; }
        public string WitnessInformation { get; set; }
        public string Notes { get; set; }
        public bool? IsActive { get; set; }

        // (2) Assignment fields (only during assignment workflow)
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssignedToId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? AssignedTime { get; set; }
        public string AssignedTeam { get; set; }

        // (3) Resolution workflow
        public int? ResponseTimeMinutes { get; set; }
        public int? ResolutionTimeMinutes { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public DateTime? ResolvedTime { get; set; }
        public string Resolution { get; set; }
        public string ResolutionNotes { get; set; }

        // (4) Escalation workflow
        public bool? Escalated { get; set; }
        public string EscalatedTo { get; set; }
        public DateTime? EscalationDate { get; set; }
        public string EscalationReason { get; set; }

        // (5) Follow-up workflow
        public bool? FollowUpRequired { get; set; }
        public DateTime? FollowUpDate { get; set; }

        // (6) Closing workflow
        public DateTime? ClosedDate { get; set; }
        public DateTime? ClosedTime { get; set; }
        public string ClosedBy { get; set; }
        public int? SatisfactionRating { get; set; }
    }

    /// <summary>
    /// Event Incident Response DTO - For reading/querying incidents
    /// </summary>
    public class EventIncidentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long IncidentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }
        
        public string IncidentNumber { get; set; }
        public string IncidentType { get; set; }
        public string IncidentCategory { get; set; }

        public string Title { get; set; }
        public string TitleAr { get; set; }

        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        
        public string Priority { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        
        public string Location { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GateId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? BoothId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? IncidentTypeId { get; set; }
        
        public DateTime ReportedDate { get; set; }
        public DateTime IncidentTime { get; set; }
        public string ReportedBy { get; set; }
        public string ReporterType { get; set; }
        public string ReporterContact { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssignedToId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? AssignedTime { get; set; }
        public string AssignedTeam { get; set; }
        
        public int? ResponseTimeMinutes { get; set; }
        public int? ResolutionTimeMinutes { get; set; }
        
        public DateTime? ResolutionDate { get; set; }
        public DateTime? ResolvedTime { get; set; }
        public string Resolution { get; set; }
        public string ResolutionNotes { get; set; }
        
        public bool Escalated { get; set; }
        public string EscalatedTo { get; set; }
        public DateTime? EscalationDate { get; set; }
        public string EscalationReason { get; set; }
        
        public string Attachments { get; set; }
        public string WitnessInformation { get; set; }
        
        public bool FollowUpRequired { get; set; }
        public DateTime? FollowUpDate { get; set; }
        
        public DateTime? ClosedDate { get; set; }
        public DateTime? ClosedTime { get; set; }
        public string ClosedBy { get; set; }
        public int? SatisfactionRating { get; set; }
        
        public string Notes { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Assign Incident Request DTO
    /// </summary>
    public class AssignIncidentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Incident ID is required")]
        public long IncidentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Assigned To ID is required")]
        public long AssignedToId { get; set; }
    }

    /// <summary>
    /// Resolve Incident Request DTO
    /// </summary>
    public class ResolveIncidentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Incident ID is required")]
        public long IncidentId { get; set; }

        [Required(ErrorMessage = "Resolution is required")]
        public string Resolution { get; set; }
    }

    /// <summary>
    /// Close Incident Request DTO
    /// </summary>
    public class CloseIncidentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Incident ID is required")]
        public long IncidentId { get; set; }

        [Required(ErrorMessage = "Satisfaction rating is required")]
        public int SatisfactionRating { get; set; }
    }

}

