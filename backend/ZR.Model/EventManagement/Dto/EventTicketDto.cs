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

    // ==================== Event Ticket ====================
    public class EventTicketQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VisitorId { get; set; }
        public string TicketNumber { get; set; }
        public string TicketType { get; set; }
        public string Status { get; set; }
        public DateTime? BeginIssueDate { get; set; }
        public DateTime? EndIssueDate { get; set; }
    }

    public class EventTicketDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TicketId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Event ID is required")]
        public long EventId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Visitor ID is required")]
        public long VisitorId { get; set; }
        public string TicketNumber { get; set; }
        public string TicketType { get; set; }
        public decimal Price { get; set; }
        public string QRCode { get; set; }
        public DateTime IssueDate { get; set; }
        public string Status { get; set; }
        public bool CanBeTransferred { get; set; }





        /// <summary>
        /// Ticket Category - ??? ???????
        /// </summary>

        public string TicketCategory { get; set; }



        /// <summary>
        /// Currency - ??????
        /// </summary>

        public string Currency { get; set; }

        /// <summary>
        /// Barcode - ????????
        /// </summary>

        public string Barcode { get; set; }



        /// <summary>
        /// Is Paid - ?? ?????
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Payment Method - ????? ?????
        /// </summary>

        public string PaymentMethod { get; set; }

        /// <summary>
        /// Payment Reference - ???? ?????
        /// </summary>

        public string PaymentReference { get; set; }

        /// <summary>
        /// Seat Number - ??? ??????
        /// </summary>

        public string SeatNumber { get; set; }

        /// <summary>
        /// Zone Access - ?????? ??????? (JSON array of zone IDs)
        /// </summary>
        public string ZoneAccess { get; set; }


        /// <summary>
        /// Transferred To Visitor ID - ?? ??????? ???
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TransferredToVisitorId { get; set; }

        /// <summary>
        /// Transfer Date - ????? ???????
        /// </summary>
        public DateTime? TransferDate { get; set; }

        /// <summary>
        /// Notes - ???????
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - ???
        /// </summary>
        public bool IsActive { get; set; }
        
     
        
        /// <summary>
        /// Payment Status - ???? ?????
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Is Checked In - ?? ?? ????? ?????? (Quick reference - automatically updated from TicketCheckin table)
        /// </summary>
        public bool IsCheckedIn { get; set; }

        /// <summary>
        /// Last Check-in At - ??? ??? ???? (Quick reference - automatically updated from TicketCheckin table)
        /// </summary>
        public DateTime? LastCheckinAt { get; set; }

    }

    /// <summary>
    /// Transfer Ticket Request DTO
    /// </summary>
    public class TransferTicketDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Ticket ID is required")]
        public long TicketId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "New Visitor ID is required")]
        public long NewVisitorId { get; set; }
    }

}

