using System;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    /// <summary>Filter/paging object for the risk/issue list.</summary>
    public class PmoRiskQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ProjectId { get; set; }

        public string Status { get; set; }
        public string Kind { get; set; }
        public int? MinScore { get; set; }

        /// <summary>When true, restrict to high-exposure open items (Score &gt;= 12 AND Status == "Open").</summary>
        public bool? HighExposureOpenOnly { get; set; }

        /// <summary>Matches title (case-insensitive contains).</summary>
        public string Keyword { get; set; }
    }

    /// <summary>Create/update input for a risk/issue.</summary>
    public class PmoRiskInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RiskId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        /// <summary>Risk | Issue</summary>
        public string Kind { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OwnerUserId { get; set; }

        /// <summary>1–5</summary>
        public int Probability { get; set; }

        /// <summary>1–5</summary>
        public int Impact { get; set; }

        public string Mitigation { get; set; }
        public DateTime? DueDate { get; set; }

        /// <summary>Open | Mitigating | Closed</summary>
        public string Status { get; set; }
    }
}
