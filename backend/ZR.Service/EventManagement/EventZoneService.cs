using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Zone - المناطق داخل الفعالية
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventZoneService), ServiceLifetime = LifeTime.Transient)]
    public class EventZoneService : BaseService<EventZone>, IEventZoneService
    {
        public PagedInfo<EventZoneDto> GetList(EventZoneQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .ToPage<EventZone, EventZoneDto>(parm);
        }

        public EventZone GetInfo(long zoneId)
        {
            return Queryable().Where(x => x.ZoneId == zoneId).First();
        }

        public EventZone AddEventZone(EventZone model)
        {
            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateEventZone(EventZone model)
        {
            return Update(model, true, "Update Event Zone");
        }

        public List<EventZone> GetZonesByEventId(long eventId)
        {
            return Queryable()
                .Where(x => x.EventId == eventId && x.IsActive)
                .OrderBy(x => x.ZoneName)
                .ToList();
        }

        public int UpdateOccupancy(long zoneId, int currentOccupancy)
        {
            return Update(x => x.ZoneId == zoneId, it => new EventZone
            {
                CurrentOccupancy = currentOccupancy
            });
        }

        private static Expressionable<EventZone> QueryExp(EventZoneQueryDto parm)
        {
            var predicate = Expressionable.Create<EventZone>();

            predicate = predicate.AndIF(parm.EventId != null, it => it.EventId == parm.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ZoneCode), it => it.ZoneCode.Contains(parm.ZoneCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ZoneName), it => it.ZoneName.Contains(parm.ZoneName) || it.ZoneNameAr.Contains(parm.ZoneName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ZoneType), it => it.ZoneType == parm.ZoneType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.AndIF(parm.IsActive != null, it => it.IsActive == parm.IsActive);

            return predicate;
        }
    }
}

