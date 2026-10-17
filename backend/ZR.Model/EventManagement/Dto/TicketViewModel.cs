using ZR.Model.EventManagement;

namespace ZR.Model.EventManagement.Dto
{
    /// <summary>
    /// View model for Event Ticket PDF
    /// </summary>
    public class TicketViewModel
    {
        /// <summary>
        /// Ticket information
        /// </summary>
        public EventTicket Ticket { get; set; }

        /// <summary>
        /// Visitor name (First Name + Last Name) - REQUIRED for ticket printing
        /// </summary>
        public string VisitorName { get; set; } = string.Empty;
    }
}


