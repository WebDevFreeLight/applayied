using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Incident - الحوادث والبلاغات
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventIncidentService), ServiceLifetime = LifeTime.Transient)]
    public class EventIncidentService : BaseService<EventIncident>, IEventIncidentService
    {
        public PagedInfo<EventIncidentDto> GetList(EventIncidentQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .ToPage<EventIncident, EventIncidentDto>(parm);
        }

        public EventIncident GetInfo(long incidentId)
        {
            return Queryable().Where(x => x.IncidentId == incidentId).First();
        }

        public EventIncident AddEventIncident(EventIncident model)
        {
            // Generate incident number if not provided
            if (string.IsNullOrEmpty(model.IncidentNumber))
            {
                model.IncidentNumber = $"INC-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            }

            model.IncidentTime = DateTime.Now;
            model.Status = "reported";

            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateEventIncident(EventIncident model)
        {
            return Update(model, true, "Update Event Incident");
        }

        public List<EventIncident> GetIncidentsByEventId(long eventId)
        {
            return Queryable()
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.IncidentTime, OrderByType.Desc)
                .ToList();
        }

        public List<EventIncident> GetIncidentsByAssignee(long assignedToId)
        {
            return Queryable()
                .Where(x => x.AssignedToId == assignedToId && x.Status != "closed" && x.Status != "resolved")
                .OrderBy(x => x.Priority)
                .OrderBy(x => x.IncidentTime)
                .ToList();
        }

        public int AssignIncident(long incidentId, long assignedToId)
        {
            // Get existing incident
            var existing = GetInfo(incidentId);
            if (existing == null) return 0;

            // Update only assignment-related fields
            existing.AssignedToId = assignedToId;
            existing.Status = "assigned";
            existing.AssignedTime = DateTime.Now;
            existing.AssignedDate = DateTime.Now;

            // Use full model update to preserve all other fields
            return Context.Updateable(existing).ExecuteCommand();
        }

        public int ResolveIncident(long incidentId, string resolution)
        {
            var incident = GetInfo(incidentId);
            if (incident == null) return 0;

            var responseTime = incident.AssignedTime.HasValue
                ? (int)(DateTime.Now - incident.AssignedTime.Value).TotalMinutes
                : (int)(DateTime.Now - incident.IncidentTime).TotalMinutes;

            // Update only resolution-related fields
            incident.Status = "resolved";
            incident.ResolvedTime = DateTime.Now;
            incident.Resolution = resolution;
            incident.ResponseTimeMinutes = responseTime;

            // Use full model update to preserve all other fields
            return Context.Updateable(incident).ExecuteCommand();
        }

        public int CloseIncident(long incidentId, int satisfactionRating)
        {
            // Get existing incident
            var incident = GetInfo(incidentId);
            if (incident == null) return 0;

            // Update only close-related fields
            incident.Status = "closed";
            incident.ClosedTime = DateTime.Now;
            incident.SatisfactionRating = satisfactionRating;

            // Use full model update to preserve all other fields
            return Context.Updateable(incident).ExecuteCommand();
        }

        private static Expressionable<EventIncident> QueryExp(EventIncidentQueryDto parm)
        {
            var predicate = Expressionable.Create<EventIncident>();

            predicate = predicate.AndIF(parm.EventId != null, it => it.EventId == parm.EventId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.IncidentNumber), it => it.IncidentNumber.Contains(parm.IncidentNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.IncidentType), it => it.IncidentType == parm.IncidentType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Priority), it => it.Priority == parm.Priority);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Severity), it => it.Severity == parm.Severity);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.AndIF(parm.BeginIncidentTime != null, it => it.IncidentTime >= parm.BeginIncidentTime);
            predicate = predicate.AndIF(parm.EndIncidentTime != null, it => it.IncidentTime <= parm.EndIncidentTime);
            predicate = predicate.AndIF(parm.AssignedToId != null, it => it.AssignedToId == parm.AssignedToId);

            return predicate;
        }
    }
}

