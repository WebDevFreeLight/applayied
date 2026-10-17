using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Service.EventManagement.IEventManagementService;
using ZR.Model.System;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Ticket Check-in/Checkout Service - ./E) *3,JD /.HD H.1H, 'D*0'C1
    /// </summary>
    [AppService(ServiceType = typeof(ITicketCheckinService), ServiceLifetime = LifeTime.Transient)]
    public class TicketCheckinService : BaseService<TicketCheckin>, ITicketCheckinService
    {
        private readonly IEventGateHistoryService _gateHistoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TicketCheckinService(
            IEventGateHistoryService gateHistoryService,
            IHttpContextAccessor httpContextAccessor)
        {
            _gateHistoryService = gateHistoryService;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Get paginated check-in history
        /// </summary>
        public PagedInfo<TicketCheckinHistoryDto> GetHistory(TicketCheckinQueryDto parm)
        {
            var predicate = Expressionable.Create<TicketCheckin>();

            predicate = predicate.AndIF(parm.TicketId != null, it => it.TicketId == parm.TicketId);
            predicate = predicate.AndIF(parm.EventId != null, it => it.EventId == parm.EventId);
            predicate = predicate.AndIF(parm.VisitorId != null, it => it.VisitorId == parm.VisitorId);
            predicate = predicate.AndIF(parm.GateId != null, it => it.GateId == parm.GateId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Action), it => it.Action == parm.Action);
            predicate = predicate.AndIF(parm.BeginActionAt != null, it => it.ActionAt >= parm.BeginActionAt);
            predicate = predicate.AndIF(parm.EndActionAt != null, it => it.ActionAt <= parm.EndActionAt);
            predicate = predicate.AndIF(parm.OperatorId != null, it => it.OperatorId == parm.OperatorId);

            var totalCount = 0;
            var list = Context.Queryable<TicketCheckin>()
                .LeftJoin<EventTicket>((c, t) => c.TicketId == t.TicketId)
                .LeftJoin<EventVisitor>((c, t, v) => t.VisitorId == v.VisitorId)
                .LeftJoin<Event>((c, t, v, e) => c.EventId == e.Eventid)
                .LeftJoin<EventGate>((c, t, v, e, g) => c.GateId == g.Gateid)
                .LeftJoin<SysUser>((c, t, v, e, g, u) => c.OperatorId == u.UserId)
                .Where(predicate.ToExpression())
                .OrderBy((c, t, v, e, g, u) => c.ActionAt, OrderByType.Desc)
                .Select((c, t, v, e, g, u) => new TicketCheckinHistoryDto
                {
                    CheckinId = c.CheckinId,
                    TicketId = c.TicketId,
                    TicketNumber = t.TicketNumber ?? string.Empty,
                    VisitorId = c.VisitorId,
                    VisitorName = SqlFunc.MergeString(v.FirstName, " ", v.LastName),
                    VisitorEmail = v.Email ?? string.Empty,
                    EventId = c.EventId,
                    EventName = e.Eventname ?? string.Empty,
                    GateId = c.GateId,
                    GateName = g.Gatename ?? string.Empty,
                    Action = c.Action,
                    ActionAt = c.ActionAt,
                    OperatorId = c.OperatorId,
                    OperatorName = u.NickName ?? string.Empty,
                    DeviceInfo = c.DeviceInfo ?? string.Empty,
                    Notes = c.Notes ?? string.Empty,
                    IpAddress = c.IpAddress ?? string.Empty,
                    Location = c.Location ?? string.Empty,
                    CreateTime = c.Create_time
                })
                .ToPageList(parm.PageNum, parm.PageSize, ref totalCount);

            return new PagedInfo<TicketCheckinHistoryDto>
            {
                Result = list,
                TotalNum = totalCount,
                PageIndex = parm.PageNum,
                PageSize = parm.PageSize
            };
        }

        /// <summary>
        /// Check-in ticket
        /// </summary>
        public TicketCheckin CheckInTicket(CreateTicketCheckinDto dto, long operatorId, string ipAddress)
        {
            Context.Ado.BeginTran();
            try
            {
                // 1. Get ticket
                var ticket = Context.Queryable<EventTicket>()
                    .First(x => x.TicketId == dto.TicketId)
                    ?? throw new CustomException("Ticket not found");

                // 2. Validate ticket is not already checked in
                if (ticket.IsCheckedIn)
                {
                    throw new CustomException("Ticket is already checked in. Please check out first.");
                }

                // 3. Validate ticket is active
                if (!ticket.IsActive)
                {
                    throw new CustomException("Ticket is not active");
                }

                // 4. Validate ticket status
                // Only block cancelled and expired tickets
                // "used" status is set when ticket is transferred, but transferred tickets can still be used by new owner
                if (ticket.Status == "cancelled" || ticket.Status == "expired")
                {
                    throw new CustomException($"Ticket cannot be used. Status: {ticket.Status}");
                }

                // 5. Validate event is active
                var eventInfo = Context.Queryable<Event>()
                    .First(e => e.Eventid == ticket.EventId)
                    ?? throw new CustomException("Event not found");

                //if (eventInfo.Status != "active" && eventInfo.Status != "ongoing")
                //{
                //    throw new CustomException("Event is not active");
                //}

                // 6. Validate gate
                var gate = Context.Queryable<EventGate>()
                    .First(g => g.Gateid == dto.GateId)
                    ?? throw new CustomException("Gate not found");

                if (gate.Eventid != ticket.EventId)
                {
                    throw new CustomException("Gate does not belong to this event");
                }

                // 7. Create check-in record
                var checkin = new TicketCheckin
                {
                    CheckinId = SnowFlakeSingle.Instance.NextId(),
                    TicketId = dto.TicketId,
                    EventId = ticket.EventId,
                    VisitorId = ticket.VisitorId,
                    GateId = dto.GateId,
                    Action = "checkin",
                    ActionAt = DateTime.Now,
                    OperatorId = operatorId,
                    DeviceInfo = dto.DeviceInfo,
                    Notes = dto.Notes,
                    IpAddress = ipAddress,
                    Location = dto.Location
                };

                // 8. Update ticket quick reference fields (automatically synced from TicketCheckin table)
                ticket.IsCheckedIn = true;
                ticket.LastCheckinAt = DateTime.Now;
                // Don't change status on check-in, keep it as is (pending, etc.)

                // 9. Save both records
                Context.Insertable(checkin).ExecuteCommand();
                Context.Updateable(ticket).ExecuteCommand();

                // 10. Update event occupancy
                Context.Updateable<Event>()
                    .SetColumns(it => it.Currentoccupancy == it.Currentoccupancy + 1)
                    .Where(it => it.Eventid == ticket.EventId)
                    .ExecuteCommand();

                // 11. Log to gate history
                var visitor = ticket.VisitorId > 0 ? Context.Queryable<EventVisitor>().First(v => v.VisitorId == ticket.VisitorId) : null;
                var history = new EventGateHistory
                {
                    HistoryId = SnowFlakeSingle.Instance.NextId(),
                    GateId = gate.Gateid,
                    GateCode = gate.Gatecode,
                    GateName = gate.Gatename,
                    EventId = ticket.EventId,
                    ActionType = "ticket_checkin",
                    ActionDate = DateTime.Now,
                    TicketId = ticket.TicketId,
                    TicketNumber = ticket.TicketNumber,
                    VisitorId = visitor?.VisitorId,
                    VisitorName = visitor != null ? $"{visitor.FirstName} {visitor.LastName}" : null,
                    VisitorCode = visitor?.VisitorCode,
                    CheckinAction = "checkin",
                    DeviceInfo = dto.DeviceInfo,
                    PerformedByUserId = operatorId,
                    PerformedByUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    IpAddress = ipAddress,
                    Notes = $"Ticket {ticket.TicketNumber} checked in at gate {gate.Gatecode}"
                };
                _gateHistoryService.LogHistory(history);

                Context.Ado.CommitTran();
                return checkin;
            }
            catch
            {
                Context.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// Check-out ticket
        /// </summary>
        public TicketCheckin CheckOutTicket(CreateTicketCheckoutDto dto, long operatorId, string ipAddress)
        {
            Context.Ado.BeginTran();
            try
            {
                // 1. Get ticket
                var ticket = Context.Queryable<EventTicket>()
                    .First(x => x.TicketId == dto.TicketId)
                    ?? throw new CustomException("Ticket not found");

                // 2. Validate ticket is checked in
                if (!ticket.IsCheckedIn)
                {
                    throw new CustomException("Ticket is not checked in. Cannot check out.");
                }

                // 3. Validate gate
                var gate = Context.Queryable<EventGate>()
                    .First(g => g.Gateid == dto.GateId)
                    ?? throw new CustomException("Gate not found");

                if (gate.Eventid != ticket.EventId)
                {
                    throw new CustomException("Gate does not belong to this event");
                }

                // 4. Create checkout record
                var checkout = new TicketCheckin
                {
                    CheckinId = SnowFlakeSingle.Instance.NextId(),
                    TicketId = dto.TicketId,
                    EventId = ticket.EventId,
                    VisitorId = ticket.VisitorId,
                    GateId = dto.GateId,
                    Action = "checkout",
                    ActionAt = DateTime.Now,
                    OperatorId = operatorId,
                    DeviceInfo = dto.DeviceInfo,
                    Notes = dto.Notes,
                    IpAddress = ipAddress,
                    Location = dto.Location
                };

                // 5. Update ticket quick reference fields (automatically synced from TicketCheckin table)
                // Only update IsCheckedIn flag - don't change status to "used" on checkout
                // Status should only become "used" when ticket is transferred
                ticket.IsCheckedIn = false;

                // 6. Save both records
                Context.Insertable(checkout).ExecuteCommand();
                Context.Updateable(ticket).ExecuteCommand();

                // 7. Update event occupancy
                Context.Updateable<Event>()
                    .SetColumns(it => it.Currentoccupancy == SqlFunc.IIF(it.Currentoccupancy > 0, it.Currentoccupancy - 1, 0))
                    .Where(it => it.Eventid == ticket.EventId)
                    .ExecuteCommand();

                // 8. Log to gate history
                var visitor = ticket.VisitorId > 0 ? Context.Queryable<EventVisitor>().First(v => v.VisitorId == ticket.VisitorId) : null;
                var history = new EventGateHistory
                {
                    HistoryId = SnowFlakeSingle.Instance.NextId(),
                    GateId = gate.Gateid,
                    GateCode = gate.Gatecode,
                    GateName = gate.Gatename,
                    EventId = ticket.EventId,
                    ActionType = "ticket_checkout",
                    ActionDate = DateTime.Now,
                    TicketId = ticket.TicketId,
                    TicketNumber = ticket.TicketNumber,
                    VisitorId = visitor?.VisitorId,
                    VisitorName = visitor != null ? $"{visitor.FirstName} {visitor.LastName}" : null,
                    VisitorCode = visitor?.VisitorCode,
                    CheckinAction = "checkout",
                    DeviceInfo = dto.DeviceInfo,
                    PerformedByUserId = operatorId,
                    PerformedByUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
                    IpAddress = ipAddress,
                    Notes = $"Ticket {ticket.TicketNumber} checked out at gate {gate.Gatecode}"
                };
                _gateHistoryService.LogHistory(history);

                Context.Ado.CommitTran();
                return checkout;
            }
            catch
            {
                Context.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// Get ticket current status
        /// </summary>
        public TicketStatusDto GetTicketStatus(long ticketId)
        {
            // Get the latest check-in/checkout records from TicketCheckin table
            var lastCheckin = Context.Queryable<TicketCheckin>()
                .Where(c => c.TicketId == ticketId && c.Action == "checkin")
                .OrderBy(c => c.ActionAt, OrderByType.Desc)
                .First();

            var lastCheckout = Context.Queryable<TicketCheckin>()
                .Where(c => c.TicketId == ticketId && c.Action == "checkout")
                .OrderBy(c => c.ActionAt, OrderByType.Desc)
                .First();

            var totalCheckins = Context.Queryable<TicketCheckin>()
                .Where(c => c.TicketId == ticketId && c.Action == "checkin")
                .Count();

            var isCurrentlyCheckedIn = false;
            long? lastGateId = null;
            string lastGateName = null;

            if (lastCheckin != null)
            {
                if (lastCheckout == null || lastCheckin.ActionAt > lastCheckout.ActionAt)
                {
                    isCurrentlyCheckedIn = true;
                    lastGateId = lastCheckin.GateId;
                }
                else
                {
                    lastGateId = lastCheckout.GateId;
                }
            }

            if (lastGateId.HasValue)
            {
                var gate = Context.Queryable<EventGate>()
                    .First(g => g.Gateid == lastGateId);
                lastGateName = gate?.Gatename;
            }

            // First get ticket data from SQL query (only database fields)
            var ticketData = Context.Queryable<EventTicket>()
                .LeftJoin<EventVisitor>((t, v) => t.VisitorId == v.VisitorId)
                .Where(t => t.TicketId == ticketId)
                .Select((t, v) => new TicketStatusDto
                {
                    TicketId = t.TicketId,
                    TicketNumber = t.TicketNumber,
                    VisitorId = t.VisitorId,
                    VisitorName = v.FirstName + " " + v.LastName,
                    VisitorEmail = v.Email,
                    Status = t.Status,
                    TicketType = t.TicketType
                })
                .First();

            // Then assign local C# variables outside of the SQL expression
            if (ticketData != null)
            {
                ticketData.IsCheckedIn = isCurrentlyCheckedIn;
                ticketData.TotalCheckins = totalCheckins;
                ticketData.LastCheckinAt = lastCheckin != null ? lastCheckin.ActionAt : (DateTime?)null;
                ticketData.LastCheckoutAt = lastCheckout != null ? lastCheckout.ActionAt : (DateTime?)null;
                ticketData.LastGateId = lastGateId;
                ticketData.LastGateName = lastGateName;
            }

            return ticketData;
        }

        /// <summary>
        /// Get ticket history
        /// </summary>
        public List<TicketCheckinHistoryDto> GetTicketHistory(long ticketId, int pageNum = 1, int pageSize = 100)
        {
            return Context.Queryable<TicketCheckin>()
                .LeftJoin<EventTicket>((c, t) => c.TicketId == t.TicketId)
                .LeftJoin<EventVisitor>((c, t, v) => t.VisitorId == v.VisitorId)
                .LeftJoin<Event>((c, t, v, e) => c.EventId == e.Eventid)
                .LeftJoin<EventGate>((c, t, v, e, g) => c.GateId == g.Gateid)
                .LeftJoin<SysUser>((c, t, v, e, g, u) => c.OperatorId == u.UserId)
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.ActionAt, OrderByType.Desc)
                .Select((c, t, v, e, g, u) => new TicketCheckinHistoryDto
                {
                    CheckinId = c.CheckinId,
                    TicketId = c.TicketId,
                    TicketNumber = t.TicketNumber ?? string.Empty,
                    VisitorId = c.VisitorId,
                    VisitorName = SqlFunc.MergeString(v.FirstName, " ", v.LastName),
                    VisitorEmail = v.Email ?? string.Empty,
                    EventId = c.EventId,
                    EventName = e.Eventname ?? string.Empty,
                    GateId = c.GateId,
                    GateName = g.Gatename ?? string.Empty,
                    Action = c.Action,
                    ActionAt = c.ActionAt,
                    OperatorId = c.OperatorId,
                    OperatorName = u.NickName ?? string.Empty,
                    DeviceInfo = c.DeviceInfo ?? string.Empty,
                    Notes = c.Notes ?? string.Empty,
                    IpAddress = c.IpAddress ?? string.Empty,
                    Location = c.Location ?? string.Empty,
                    CreateTime = c.Create_time
                })
                .ToPageList(pageNum, pageSize);
        }

        /// <summary>
        /// Get all tickets for a visitor
        /// </summary>
        public List<TicketStatusDto> GetVisitorTickets(long visitorId)
        {
            var tickets = Context.Queryable<EventTicket>()
                .LeftJoin<EventVisitor>((t, v) => t.VisitorId == v.VisitorId)
                .Where(t => t.VisitorId == visitorId && t.IsActive)
                .OrderBy(t => t.Create_time, OrderByType.Desc)
                .Select((t, v) => new
                {
                    Ticket = t,
                    Visitor = v
                })
                .ToList();

            var result = new List<TicketStatusDto>();

            foreach (var item in tickets)
            {
                var ticket = item.Ticket;
                var visitor = item.Visitor;

                // Get check-in/checkout status from TicketCheckin table
                var lastCheckin = Context.Queryable<TicketCheckin>()
                    .Where(c => c.TicketId == ticket.TicketId && c.Action == "checkin")
                    .OrderBy(c => c.ActionAt, OrderByType.Desc)
                    .First();

                var lastCheckout = Context.Queryable<TicketCheckin>()
                    .Where(c => c.TicketId == ticket.TicketId && c.Action == "checkout")
                    .OrderBy(c => c.ActionAt, OrderByType.Desc)
                    .First();

                var totalCheckins = Context.Queryable<TicketCheckin>()
                    .Where(c => c.TicketId == ticket.TicketId && c.Action == "checkin")
                    .Count();

                var isCurrentlyCheckedIn = false;
                long? lastGateId = null;
                string lastGateName = null;

                if (lastCheckin != null)
                {
                    if (lastCheckout == null || lastCheckin.ActionAt > lastCheckout.ActionAt)
                    {
                        isCurrentlyCheckedIn = true;
                        lastGateId = lastCheckin.GateId;
                    }
                    else
                    {
                        lastGateId = lastCheckout.GateId;
                    }
                }

                if (lastGateId.HasValue)
                {
                    var gate = Context.Queryable<EventGate>()
                        .First(g => g.Gateid == lastGateId);
                    lastGateName = gate?.Gatename;
                }

                result.Add(new TicketStatusDto
                {
                    TicketId = ticket.TicketId,
                    TicketNumber = ticket.TicketNumber,
                    VisitorId = ticket.VisitorId,
                    VisitorName = visitor != null ? $"{visitor.FirstName} {visitor.LastName}" : null,
                    VisitorEmail = visitor?.Email,
                    IsCheckedIn = isCurrentlyCheckedIn,
                    TotalCheckins = totalCheckins,
                    LastCheckinAt = lastCheckin != null ? lastCheckin.ActionAt : (DateTime?)null,
                    LastCheckoutAt = lastCheckout != null ? lastCheckout.ActionAt : (DateTime?)null,
                    LastGateId = lastGateId,
                    LastGateName = lastGateName,
                    Status = ticket.Status,
                    TicketType = ticket.TicketType
                });
            }

            return result;
        }

        /// <summary>
        /// Get check-in record by ID
        /// </summary>
        public TicketCheckin GetInfo(long checkinId)
        {
            return Context.Queryable<TicketCheckin>()
                .First(x => x.CheckinId == checkinId);
        }
    }
}

