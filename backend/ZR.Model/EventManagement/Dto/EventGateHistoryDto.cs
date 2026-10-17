using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Event Gate History DTO
    /// </summary>
    public class EventGateHistoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long GateId { get; set; }

        public string GateCode { get; set; }

        public string GateName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }

        public string EventName { get; set; }

        public string ActionType { get; set; }

        public string PreviousStatus { get; set; }

        public string NewStatus { get; set; }

        public string PreviousGateType { get; set; }

        public string NewGateType { get; set; }

        public int? PreviousCapacity { get; set; }

        public int? NewCapacity { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PreviousManagerId { get; set; }

        public string PreviousManagerName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NewManagerId { get; set; }

        public string NewManagerName { get; set; }

        public DateTime ActionDate { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PerformedByUserId { get; set; }

        public string PerformedByUsername { get; set; }

        public string Notes { get; set; }

        public string IpAddress { get; set; }

        // Visitor/Ticket Check-in Fields
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VisitorId { get; set; }

        public string VisitorName { get; set; }

        public string VisitorCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TicketId { get; set; }

        public string TicketNumber { get; set; }

        public string CheckinAction { get; set; }

        public string DeviceInfo { get; set; }
















         



    }

    /// <summary>
    /// Query DTO for filtering gate history
    /// </summary>
    public class EventGateHistoryQueryDto : PagerInfo
    {

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GateId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }

        public string ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
         
    }
}

