using Infrastructure.Converter;
using Infrastructure.Model;

namespace ZR.Model.Marketing.Dto
{
    /// <summary>
    /// Marketing Employee Query DTO
    /// Used for filtering marketing team members
    /// </summary>
    public class MarketingEmployeeQueryDto : PagerInfo
    {
        /// <summary>
        /// Department ID filter
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        /// <summary>
        /// Search query (searches in name and email)
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// Status filter (active/inactive)
        /// </summary>
        public string StatusFilter { get; set; }
    }
}
