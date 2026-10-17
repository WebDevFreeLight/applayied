using System;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    /// <summary>Filter/paging object for the project list.</summary>
    public class PmoProjectQueryDto : PagerInfo
    {
        /// <summary>Matches code or name (case-insensitive contains).</summary>
        public string Keyword { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DeptId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OwnerUserId { get; set; }

        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime? BeginStartDate { get; set; }
        public DateTime? EndStartDate { get; set; }
    }

    /// <summary>Create/update input for a project.</summary>
    public class PmoProjectInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [Required(ErrorMessage = "Project code is required")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Project name is required")]
        public string Name { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeptId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long OwnerUserId { get; set; }

        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string Description { get; set; }

        [Range(0, 9999999999, ErrorMessage = "Initial approved budget must be zero or positive")]
        public decimal InitialApprovedBudget { get; set; }

        public string Currency { get; set; } = "AED";
    }
}
