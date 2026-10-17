using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    public class PmoChangeRequestQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long? ProjectId { get; set; }
        public string Status { get; set; }
        public string Keyword { get; set; }
    }

    public class PmoChangeRequestInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long ChangeRequestId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? EstimatedCostImpact { get; set; }
    }

    /// <summary>A workflow decision: StartReview | Approve | Reject | Return.</summary>
    public class PmoChangeRequestDecisionDto
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long ChangeRequestId { get; set; }

        [Required(ErrorMessage = "An action is required")]
        public string Action { get; set; }
        public string Reason { get; set; }
        public long? Version { get; set; }
    }
}
