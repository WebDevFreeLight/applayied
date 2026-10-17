using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// Ticket Check-in Query Parameters
    /// </summary>
    public class TicketCheckinQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TicketId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VisitorId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GateId { get; set; }

        public string Action { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OperatorId { get; set; }

        public DateTime? BeginActionAt { get; set; }
        public DateTime? EndActionAt { get; set; }
    }

    /// <summary>
    /// Create Ticket Check-in DTO
    /// </summary>
    public class CreateTicketCheckinDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TicketId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long GateId { get; set; }

        public string DeviceInfo { get; set; }
        public string Notes { get; set; }
        public string Location { get; set; }
    }

    /// <summary>
    /// Create Ticket Checkout DTO
    /// </summary>
    public class CreateTicketCheckoutDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TicketId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long GateId { get; set; }

        public string DeviceInfo { get; set; }
        public string Notes { get; set; }
        public string Location { get; set; }
    }

    /// <summary>
    /// Ticket Check-in History DTO
    /// </summary>
    public class TicketCheckinHistoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CheckinId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long TicketId { get; set; }

        public string TicketNumber { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VisitorId { get; set; }

        public string VisitorName { get; set; }
        public string VisitorEmail { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        public string EventName { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GateId { get; set; }

        public string GateName { get; set; }
        public string Action { get; set; }
        public DateTime ActionAt { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OperatorId { get; set; }

        public string OperatorName { get; set; }
        public string DeviceInfo { get; set; }
        public string Notes { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public DateTime? CreateTime { get; set; }
    }

    /// <summary>
    /// Ticket Status DTO
    /// </summary>
    public class TicketStatusDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TicketId { get; set; }

        public string TicketNumber { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VisitorId { get; set; }

        public string VisitorName { get; set; }
        public string VisitorEmail { get; set; }
        public bool IsCheckedIn { get; set; }
        public int TotalCheckins { get; set; }
        public DateTime? LastCheckinAt { get; set; }
        public DateTime? LastCheckoutAt { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? LastGateId { get; set; }

        public string LastGateName { get; set; }
        public string Status { get; set; }
        public string TicketType { get; set; }
    }
}

