using System;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    /// <summary>Filter/paging object for the milestone list.</summary>
    public class PmoMilestoneQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ProjectId { get; set; }

        public string Status { get; set; }

        /// <summary>When set, only milestones due between cutoff (today) and cutoff + N days.</summary>
        public int? DueWithinDays { get; set; }

        /// <summary>Reference date for the due-within window; defaults to today.</summary>
        public DateTime? Cutoff { get; set; }
    }

    /// <summary>Create/update input for a milestone.</summary>
    public class PmoMilestoneInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MilestoneId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [Required(ErrorMessage = "Milestone name is required")]
        public string Name { get; set; }

        public DateTime? DueDate { get; set; }

        /// <summary>Pending | Achieved | Missed (defaults to Pending).</summary>
        public string Status { get; set; }
    }
}
