using System;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    /// <summary>Filter/paging object for the task list.</summary>
    public class PmoTaskQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ProjectId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssigneeUserId { get; set; }

        public string Status { get; set; }
        public string Priority { get; set; }

        /// <summary>Matches task title (case-insensitive contains).</summary>
        public string Keyword { get; set; }

        /// <summary>When true, only tasks past their due date and not Done.</summary>
        public bool? OverdueOnly { get; set; }

        /// <summary>Reference date for overdue/enrichment; defaults to today.</summary>
        public DateTime? Cutoff { get; set; }
    }

    /// <summary>Create/update input for a task.</summary>
    public class PmoTaskInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        public string Title { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssigneeUserId { get; set; }

        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        /// <summary>Relative weight used in weighted project progress (defaults to 1 if &lt;= 0).</summary>
        public decimal Weight { get; set; }

        /// <summary>0–100.</summary>
        public int ProgressPct { get; set; }

        /// <summary>Optimistic-concurrency token.</summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Version { get; set; }
    }

    /// <summary>Contributor-facing progress update for a single task.</summary>
    public class PmoTaskProgressDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public int ProgressPct { get; set; }

        public string Status { get; set; }

        /// <summary>Optimistic-concurrency token.</summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? Version { get; set; }
    }
}
