using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsTicketService
    {
        Task<PagedInfo<OpsTicket>> GetTicketsListAsync(OpsTicketQueryDto query);
        Task<OpsTicket> GetTicketByIdAsync(long ticketId);
        Task<OpsTicket> CreateTicketAsync(OpsTicket ticket);
        Task<int> UpdateTicketAsync(OpsTicket ticket);
        Task<int> UpdateTicketStatusAsync(long ticketId, string newStatus, string notes, bool escalateToDeal);
        Task<int> DeleteTicketsAsync(long[] ticketIds);
        Task<TicketDashboardDto> GetDashboardAsync(long? assignedTo);
        Task<long?> GetEmployeeIdByUserIdAsync(long userId);
    }
}
