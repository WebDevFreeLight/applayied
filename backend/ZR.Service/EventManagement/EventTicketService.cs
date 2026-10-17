using System;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Ticket - التذاكر
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventTicketService), ServiceLifetime = LifeTime.Transient)]
    public class EventTicketService : BaseService<EventTicket>, IEventTicketService
    {
        public PagedInfo<EventTicketDto> GetList(EventTicketQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .ToPage<EventTicket, EventTicketDto>(parm);
        }

        public EventTicket GetInfo(long ticketId)
        {
            return Queryable().Where(x => x.TicketId == ticketId).ToList().FirstOrDefault();
        }

        public EventTicket AddEventTicket(EventTicket model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // Attempt to reuse existing visitor if we can identify them by phone number and name
            // Note: eventId parameter is kept for backward compatibility but not used since visitors are no longer tied to events
           
            

            // Generate ticket number if not provided
            if (string.IsNullOrEmpty(model.TicketNumber))
            {
                model.TicketNumber = $"TKT-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            }

            // Generate unique QR code if not provided
            if (string.IsNullOrEmpty(model.QRCode))
            {
                model.QRCode = GenerateUniqueCode("QR", model.EventId);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Generated QRCode: {model.QRCode} for EventId: {model.EventId}");
            }

            // Generate unique Barcode if not provided
            if (string.IsNullOrEmpty(model.Barcode))
            {
                model.Barcode = GenerateUniqueCode("BAR", model.EventId);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Generated Barcode: {model.Barcode} for EventId: {model.EventId}");
            }

            model.IssueDate = DateTime.Now;
            model.Status = "active";

            Insertable(model).ExecuteReturnSnowflakeId();
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Successfully added EventTicket with TicketNumber: {model.TicketNumber}");
            return model;
        }

   



        /// <summary>
        /// Generate a unique code for QRCode or Barcode
        /// </summary>
        /// <param name="prefix">Prefix for the code (e.g., "QR" or "BAR")</param>
        /// <param name="eventId">Event ID to include in the code</param>
        /// <returns>Unique code string</returns>
        private static string GenerateUniqueCode(string prefix, long eventId)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
            return $"{prefix}-{eventId}-{timestamp}-{uniqueId}";
        }

        public int UpdateEventTicket(EventTicket model)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Updating EventTicket with TicketId: {model.TicketId}");

            // Generate unique QR code if not provided
            if (string.IsNullOrEmpty(model.QRCode))
            {
                model.QRCode = GenerateUniqueCode("QR", model.EventId);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Generated QRCode: {model.QRCode} for EventId: {model.EventId}");
            }
            

            // Generate unique Barcode if not provided
            if (string.IsNullOrEmpty(model.Barcode))
            {
                model.Barcode = GenerateUniqueCode("BAR", model.EventId);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Generated Barcode: {model.Barcode} for EventId: {model.EventId}");
            }

            var result = Update(model, true, "Update Event Ticket");
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Successfully updated EventTicket with TicketId: {model.TicketId}");
            return result;
        }

        public List<EventTicket> GetTicketsByEventId(long eventId)
        {
            return Queryable()
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .ToList();
        }

        public List<EventTicket> GetTicketsByVisitorId(long visitorId)
        {
            return Queryable()
                .Where(x => x.VisitorId == visitorId)
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .ToList();
        }

        public int UseTicket(long ticketId)
        {
            return Update(x => x.TicketId == ticketId, it => new EventTicket
            {
                Status = "used"
            });
        }

        public int TransferTicket(long ticketId, long newVisitorId)
        {
            // 1. Get and validate ticket
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TransferTicket - Looking for ticket ID: {ticketId}, New Visitor ID: {newVisitorId}");
            
            var ticket = GetInfo(ticketId);
            if (ticket == null)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TransferTicket - Ticket not found with ID: {ticketId}");
                throw new CustomException($"Ticket not found with ID: {ticketId}");
            }
            
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TransferTicket - Ticket found: {ticket.TicketNumber}, CanBeTransferred: {ticket.CanBeTransferred}, IsCheckedIn: {ticket.IsCheckedIn}, IsActive: {ticket.IsActive}");

            if (!ticket.CanBeTransferred)
            {
                throw new CustomException("This ticket cannot be transferred");
            }

            // 2. Validate ticket is not checked in (optional - you might want to allow transferring checked-in tickets)
            if (ticket.IsCheckedIn)
            {
                throw new CustomException("Cannot transfer a ticket that is currently checked in. Please check out first.");
            }

             // 3. Validate new visitor exists
             var newVisitor = Context.Queryable<EventVisitor>()
                 .Where(v => v.VisitorId == newVisitorId)
                 .ToList()
                 .FirstOrDefault();
             if (newVisitor == null)
             {
                 throw new CustomException($"Target visitor not found with ID: {newVisitorId}");
             }

            // 4. Validate ticket is active
            if (!ticket.IsActive)
            {
                throw new CustomException("Cannot transfer an inactive ticket");
            }

            // 5. Update ticket with transfer information
            var result = Update(x => x.TicketId == ticketId, it => new EventTicket
            {
                VisitorId = newVisitorId,
                TransferredToVisitorId = newVisitorId,
                TransferDate = DateTime.Now
            });

            if (result <= 0)
            {
                throw new CustomException("Failed to transfer ticket. Please try again.");
            }

            return result;
        }

        public int CancelTicket(long ticketId)
        {
            return Update(x => x.TicketId == ticketId, it => new EventTicket
            {
                Status = "cancelled",
                Update_time = DateTime.Now
            });
        }

        private static Expressionable<EventTicket> QueryExp(EventTicketQueryDto parm)
        {
            var predicate = Expressionable.Create<EventTicket>();

            predicate = predicate.AndIF(parm.EventId != null, it => it.EventId == parm.EventId);
            predicate = predicate.AndIF(parm.VisitorId != null, it => it.VisitorId == parm.VisitorId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TicketNumber), it => it.TicketNumber.Contains(parm.TicketNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TicketType), it => it.TicketType == parm.TicketType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.AndIF(parm.BeginIssueDate != null, it => it.IssueDate >= parm.BeginIssueDate);
            predicate = predicate.AndIF(parm.EndIssueDate != null, it => it.IssueDate <= parm.EndIssueDate);

            return predicate;
        }
    }
}

