using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Model.Customers.Leads;
using ZR.Model.Sales;
using ZR.Model.HR.EmployeeCore;
using ZR.Repository;

namespace ZR.Service.Operations
{
    [AppService(ServiceType = typeof(IOpsTicketService), ServiceLifetime = LifeTime.Transient)]
    public class OpsTicketService : BaseService<OpsTicket>, IOpsTicketService
    {
        public async Task<long?> GetEmployeeIdByUserIdAsync(long userId)
        {
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.UserId.HasValue && e.UserId.Value == userId)
                .FirstAsync();
            return employee?.EmployeeId;
        }

        public async Task<PagedInfo<OpsTicket>> GetTicketsListAsync(OpsTicketQueryDto query)
        {
            long? filterEmployeeId = null;

            if (!query.IsCurrentUserAdmin && query.CurrentUserId.HasValue)
            {
                filterEmployeeId = await GetEmployeeIdByUserIdAsync(query.CurrentUserId.Value);
                if (!filterEmployeeId.HasValue)
                {
                    return new PagedInfo<OpsTicket>
                    {
                        Result = new List<OpsTicket>(),
                        TotalNum = 0,
                        PageIndex = query.PageNum,
                        PageSize = query.PageSize
                    };
                }
            }

            var now = DateTime.Now;
            var pageQuery = Context
                .Queryable<OpsTicket, BpLead, SalesDeal, HrEmployeeCore, HrPersonName>(
                    (t, lead, deal, emp, name) => new JoinQueryInfos(
                        JoinType.Left, t.LeadId == lead.LeadId,
                        JoinType.Left, t.DealId == deal.DealId,
                        JoinType.Left, t.AssignedTo == emp.EmployeeId,
                        JoinType.Left, emp.PersonId == name.PersonId &&
                                       name.IsPrimary == true &&
                                       name.EffectiveFrom <= now &&
                                       (name.EffectiveTo == null || name.EffectiveTo > now)))
                .WhereIF(filterEmployeeId.HasValue, (t, lead, deal, emp, name) => t.AssignedTo == filterEmployeeId.Value)
                .WhereIF(query.LeadId.HasValue, (t, lead, deal, emp, name) => t.LeadId == query.LeadId.Value)
                .WhereIF(query.DealId.HasValue, (t, lead, deal, emp, name) => t.DealId == query.DealId.Value)
                .WhereIF(!string.IsNullOrEmpty(query.TicketStatus), (t, lead, deal, emp, name) => t.TicketStatus == query.TicketStatus)
                .WhereIF(!string.IsNullOrEmpty(query.Category), (t, lead, deal, emp, name) => t.Category == query.Category)
                .WhereIF(!string.IsNullOrEmpty(query.Priority), (t, lead, deal, emp, name) => t.Priority == query.Priority)
                .WhereIF(query.AssignedTo.HasValue && !filterEmployeeId.HasValue, (t, lead, deal, emp, name) => t.AssignedTo == query.AssignedTo.Value)
                .WhereIF(!string.IsNullOrEmpty(query.Title), (t, lead, deal, emp, name) => t.Title.Contains(query.Title))
                .Select((t, lead, deal, emp, name) => new OpsTicket
                {
                    TicketId = t.TicketId,
                    Title = t.Title,
                    Description = t.Description,
                    LeadId = t.LeadId,
                    LeadName = lead.BrandName,
                    DealId = t.DealId,
                    DealTitle = deal.DealTitle,
                    Category = t.Category,
                    Priority = t.Priority,
                    TicketStatus = t.TicketStatus,
                    AssignedTo = t.AssignedTo,
                    AssignedToName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    ReportedBy = t.ReportedBy,
                    ResolutionNotes = t.ResolutionNotes,
                    ResolvedDate = t.ResolvedDate,
                    EscalatedToDeal = t.EscalatedToDeal,
                    Create_by = t.Create_by,
                    Create_time = t.Create_time,
                    Update_by = t.Update_by,
                    Update_time = t.Update_time
                });

            return await pageQuery.ToPageAsync(query);
        }

        public async Task<OpsTicket> GetTicketByIdAsync(long ticketId)
        {
            var now = DateTime.Now;
            return await Context
                .Queryable<OpsTicket, BpLead, SalesDeal, HrEmployeeCore, HrPersonName>(
                    (t, lead, deal, emp, name) => new JoinQueryInfos(
                        JoinType.Left, t.LeadId == lead.LeadId,
                        JoinType.Left, t.DealId == deal.DealId,
                        JoinType.Left, t.AssignedTo == emp.EmployeeId,
                        JoinType.Left, emp.PersonId == name.PersonId &&
                                       name.IsPrimary == true &&
                                       name.EffectiveFrom <= now &&
                                       (name.EffectiveTo == null || name.EffectiveTo > now)))
                .Where((t, lead, deal, emp, name) => t.TicketId == ticketId)
                .Select((t, lead, deal, emp, name) => new OpsTicket
                {
                    TicketId = t.TicketId,
                    Title = t.Title,
                    Description = t.Description,
                    LeadId = t.LeadId,
                    LeadName = lead.BrandName,
                    DealId = t.DealId,
                    DealTitle = deal.DealTitle,
                    Category = t.Category,
                    Priority = t.Priority,
                    TicketStatus = t.TicketStatus,
                    AssignedTo = t.AssignedTo,
                    AssignedToName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    ReportedBy = t.ReportedBy,
                    ResolutionNotes = t.ResolutionNotes,
                    ResolvedDate = t.ResolvedDate,
                    EscalatedToDeal = t.EscalatedToDeal,
                    Create_by = t.Create_by,
                    Create_time = t.Create_time,
                    Update_by = t.Update_by,
                    Update_time = t.Update_time
                })
                .FirstAsync();
        }

        public async Task<OpsTicket> CreateTicketAsync(OpsTicket ticket)
        {
            ticket.TicketId = SnowFlakeSingle.Instance.NextId();
            ticket.Create_time = DateTime.Now;
            ticket.TicketStatus = "Open";

            Add(ticket);
            return await GetTicketByIdAsync(ticket.TicketId);
        }

        public async Task<int> UpdateTicketAsync(OpsTicket ticket)
        {
            ticket.Update_time = DateTime.Now;
            return await Task.FromResult(Update(ticket, true));
        }

        public async Task<int> UpdateTicketStatusAsync(long ticketId, string newStatus, string notes, bool escalateToDeal)
        {
            var ticket = await GetByIdAsync(ticketId);
            if (ticket == null)
                throw new Exception("Ticket not found");

            ticket.TicketStatus = newStatus;
            ticket.Update_time = DateTime.Now;

            if (newStatus == "Resolved" || newStatus == "Closed")
            {
                ticket.ResolvedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(notes))
                    ticket.ResolutionNotes = notes;
            }

            // Escalate to deal if requested and ticket is linked to a deal
            if (escalateToDeal && ticket.DealId.HasValue && ticket.DealId.Value > 0)
            {
                var deal = await Context.Queryable<SalesDeal>()
                    .Where(d => d.DealId == ticket.DealId.Value)
                    .FirstAsync();

                if (deal != null)
                {
                    var escalationNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] [Ops Ticket #{ticket.TicketId}] Status: {newStatus} | {ticket.Title}";
                    if (!string.IsNullOrEmpty(notes))
                        escalationNote += $" | {notes}";

                    deal.Notes = string.IsNullOrEmpty(deal.Notes)
                        ? escalationNote
                        : $"{deal.Notes}\n{escalationNote}";
                    deal.Update_time = DateTime.Now;

                    await Context.Updateable(deal)
                        .UpdateColumns(d => new { d.Notes, d.Update_time })
                        .ExecuteCommandAsync();

                    ticket.EscalatedToDeal = true;
                }
            }

            return await Task.FromResult(Update(ticket, true));
        }

        public async Task<int> DeleteTicketsAsync(long[] ticketIds)
        {
            return await Context.Deleteable<OpsTicket>().In(ticketIds).ExecuteCommandAsync();
        }

        public async Task<TicketDashboardDto> GetDashboardAsync(long? assignedTo)
        {
            var query = Context.Queryable<OpsTicket>();
            if (assignedTo.HasValue)
                query = query.Where(t => t.AssignedTo == assignedTo.Value);

            var tickets = await query.ToListAsync();

            return new TicketDashboardDto
            {
                OpenCount = tickets.Count(t => t.TicketStatus == "Open"),
                InProgressCount = tickets.Count(t => t.TicketStatus == "InProgress"),
                ResolvedCount = tickets.Count(t => t.TicketStatus == "Resolved"),
                ClosedCount = tickets.Count(t => t.TicketStatus == "Closed"),
                TotalTickets = tickets.Count,
                EscalatedCount = tickets.Count(t => t.EscalatedToDeal)
            };
        }
    }
}
