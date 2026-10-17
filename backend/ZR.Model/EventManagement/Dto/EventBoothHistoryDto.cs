using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Event Booth History DTO
    /// </summary>
    public class EventBoothHistoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long BoothId { get; set; }

        public string BoothCode { get; set; }

        public string BoothName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }

        public string EventName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TenantId { get; set; }

        public string TenantName { get; set; }

        public string ActionType { get; set; }

        public string PreviousStatus { get; set; }

        public string NewStatus { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PreviousTenantId { get; set; }

        public string PreviousTenantName { get; set; }

        public decimal? RentalPrice { get; set; }

        public DateTime ActionDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PerformedByUserId { get; set; }

        public string PerformedByUsername { get; set; }

        public string Notes { get; set; }

        public string IpAddress { get; set; }
    }

    /// <summary>
    /// Query DTO for filtering booth history
    /// </summary>
    public class EventBoothHistoryQueryDto : PagerInfo
    {
        public long? BoothId { get; set; }
        public long? EventId { get; set; }
        public long? TenantId { get; set; }
        public string ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

