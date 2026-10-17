using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Gate History - سجل تاريخ البوابات
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventGateHistoryService), ServiceLifetime = LifeTime.Transient)]
    public class EventGateHistoryService : BaseService<EventGateHistory>, IEventGateHistoryService
    {
        public PagedInfo<EventGateHistoryDto> GetList(EventGateHistoryQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.ActionDate)
                .ToPage<EventGateHistory, EventGateHistoryDto>(parm);
        }

        public List<EventGateHistoryDto> GetHistoryByGateId(long gateId)
        {
            return Queryable()
                .Where(x => x.GateId == gateId)
                .OrderByDescending(x => x.ActionDate)
                .Select(x => new EventGateHistoryDto
                {
                    HistoryId = x.HistoryId,
                    GateId = x.GateId,
                    GateCode = x.GateCode,
                    GateName = x.GateName,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    ActionType = x.ActionType,
                    PreviousStatus = x.PreviousStatus,
                    NewStatus = x.NewStatus,
                    PreviousGateType = x.PreviousGateType,
                    NewGateType = x.NewGateType,
                    PreviousCapacity = x.PreviousCapacity,
                    NewCapacity = x.NewCapacity,
                    PreviousManagerId = x.PreviousManagerId,
                    PreviousManagerName = x.PreviousManagerName,
                    NewManagerId = x.NewManagerId,
                    NewManagerName = x.NewManagerName,
                    ActionDate = x.ActionDate,
                    PerformedByUserId = x.PerformedByUserId,
                    PerformedByUsername = x.PerformedByUsername,
                    Notes = x.Notes,
                    IpAddress = x.IpAddress,
                    VisitorId = x.VisitorId,
                    VisitorName = x.VisitorName,
                    VisitorCode = x.VisitorCode,
                    TicketId = x.TicketId,
                    TicketNumber = x.TicketNumber,
                    CheckinAction = x.CheckinAction,
                    DeviceInfo = x.DeviceInfo
                })
                .ToList();
        }

        public List<EventGateHistoryDto> GetHistoryByEventId(long eventId)
        {
            return Queryable()
                .Where(x => x.EventId == eventId)
                .OrderByDescending(x => x.ActionDate)
                .Select(x => new EventGateHistoryDto
                {
                    HistoryId = x.HistoryId,
                    GateId = x.GateId,
                    GateCode = x.GateCode,
                    GateName = x.GateName,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    ActionType = x.ActionType,
                    PreviousStatus = x.PreviousStatus,
                    NewStatus = x.NewStatus,
                    PreviousGateType = x.PreviousGateType,
                    NewGateType = x.NewGateType,
                    PreviousCapacity = x.PreviousCapacity,
                    NewCapacity = x.NewCapacity,
                    PreviousManagerId = x.PreviousManagerId,
                    PreviousManagerName = x.PreviousManagerName,
                    NewManagerId = x.NewManagerId,
                    NewManagerName = x.NewManagerName,
                    ActionDate = x.ActionDate,
                    PerformedByUserId = x.PerformedByUserId,
                    PerformedByUsername = x.PerformedByUsername,
                    Notes = x.Notes,
                    IpAddress = x.IpAddress,
                    VisitorId = x.VisitorId,
                    VisitorName = x.VisitorName,
                    VisitorCode = x.VisitorCode,
                    TicketId = x.TicketId,
                    TicketNumber = x.TicketNumber,
                    CheckinAction = x.CheckinAction,
                    DeviceInfo = x.DeviceInfo
                })
                .ToList();
        }

        public EventGateHistory LogHistory(EventGateHistory history)
        {
            history.HistoryId = SnowFlakeSingle.Instance.NextId();
            history.ActionDate = DateTime.Now;
            return Insertable(history).ExecuteReturnEntity();
        }

        private static Expressionable<EventGateHistory> QueryExp(EventGateHistoryQueryDto parm)
        {
            var predicate = Expressionable.Create<EventGateHistory>();

            predicate = predicate.AndIF(parm.GateId != null, it => it.GateId == parm.GateId);
            predicate = predicate.AndIF(parm.EventId != null, it => it.EventId == parm.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ActionType), it => it.ActionType == parm.ActionType);
            predicate = predicate.AndIF(parm.StartDate != null, it => it.ActionDate >= parm.StartDate);
            predicate = predicate.AndIF(parm.EndDate != null, it => it.ActionDate <= parm.EndDate);

            return predicate;
        }
    }
}

