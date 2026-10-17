using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Booth History - سجل الأكشاك
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventBoothHistoryService), ServiceLifetime = LifeTime.Transient)]
    public class EventBoothHistoryService : BaseService<EventBoothHistory>, IEventBoothHistoryService
    {
        public PagedInfo<EventBoothHistoryDto> GetList(EventBoothHistoryQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.ActionDate)
                .ToPage<EventBoothHistory, EventBoothHistoryDto>(parm);
        }

        public List<EventBoothHistoryDto> GetHistoryByBoothId(long boothId)
        {
            return Queryable()
                .Where(x => x.BoothId == boothId)
                .OrderByDescending(x => x.ActionDate)
                .Select(x => new EventBoothHistoryDto
                {
                    HistoryId = x.HistoryId,
                    BoothId = x.BoothId,
                    BoothCode = x.BoothCode,
                    BoothName = x.BoothName,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    TenantId = x.TenantId,
                    TenantName = x.TenantName,
                    ActionType = x.ActionType,
                    PreviousStatus = x.PreviousStatus,
                    NewStatus = x.NewStatus,
                    PreviousTenantId = x.PreviousTenantId,
                    PreviousTenantName = x.PreviousTenantName,
                    RentalPrice = x.RentalPrice,
                    ActionDate = x.ActionDate,
                    PerformedByUserId = x.PerformedByUserId,
                    PerformedByUsername = x.PerformedByUsername,
                    Notes = x.Notes,
                    IpAddress = x.IpAddress
                })
                .ToList();
        }

        public List<EventBoothHistoryDto> GetHistoryByTenantId(long tenantId)
        {
            return Queryable()
                .Where(x => x.TenantId == tenantId || x.PreviousTenantId == tenantId)
                .OrderByDescending(x => x.ActionDate)
                .Select(x => new EventBoothHistoryDto
                {
                    HistoryId = x.HistoryId,
                    BoothId = x.BoothId,
                    BoothCode = x.BoothCode,
                    BoothName = x.BoothName,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    TenantId = x.TenantId,
                    TenantName = x.TenantName,
                    ActionType = x.ActionType,
                    PreviousStatus = x.PreviousStatus,
                    NewStatus = x.NewStatus,
                    PreviousTenantId = x.PreviousTenantId,
                    PreviousTenantName = x.PreviousTenantName,
                    RentalPrice = x.RentalPrice,
                    ActionDate = x.ActionDate,
                    PerformedByUserId = x.PerformedByUserId,
                    PerformedByUsername = x.PerformedByUsername,
                    Notes = x.Notes,
                    IpAddress = x.IpAddress
                })
                .ToList();
        }

        public List<EventBoothHistoryDto> GetHistoryByEventId(long eventId)
        {
            return Queryable()
                .Where(x => x.EventId == eventId)
                .OrderByDescending(x => x.ActionDate)
                .Select(x => new EventBoothHistoryDto
                {
                    HistoryId = x.HistoryId,
                    BoothId = x.BoothId,
                    BoothCode = x.BoothCode,
                    BoothName = x.BoothName,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    TenantId = x.TenantId,
                    TenantName = x.TenantName,
                    ActionType = x.ActionType,
                    PreviousStatus = x.PreviousStatus,
                    NewStatus = x.NewStatus,
                    PreviousTenantId = x.PreviousTenantId,
                    PreviousTenantName = x.PreviousTenantName,
                    RentalPrice = x.RentalPrice,
                    ActionDate = x.ActionDate,
                    PerformedByUserId = x.PerformedByUserId,
                    PerformedByUsername = x.PerformedByUsername,
                    Notes = x.Notes,
                    IpAddress = x.IpAddress
                })
                .ToList();
        }

        public EventBoothHistory LogHistory(EventBoothHistory history)
        {
            history.HistoryId = SnowFlakeSingle.Instance.NextId();
            history.ActionDate = DateTime.Now;
            return Insertable(history).ExecuteReturnEntity();
        }

        private static Expressionable<EventBoothHistory> QueryExp(EventBoothHistoryQueryDto parm)
        {
            var predicate = Expressionable.Create<EventBoothHistory>();

            predicate = predicate.AndIF(parm.BoothId != null, it => it.BoothId == parm.BoothId);
            predicate = predicate.AndIF(parm.EventId != null, it => it.EventId == parm.EventId);
            predicate = predicate.AndIF(parm.TenantId != null, it => it.TenantId == parm.TenantId || it.PreviousTenantId == parm.TenantId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ActionType), it => it.ActionType == parm.ActionType);
            predicate = predicate.AndIF(parm.StartDate != null, it => it.ActionDate >= parm.StartDate);
            predicate = predicate.AndIF(parm.EndDate != null, it => it.ActionDate <= parm.EndDate);

            return predicate;
        }
    }
}

