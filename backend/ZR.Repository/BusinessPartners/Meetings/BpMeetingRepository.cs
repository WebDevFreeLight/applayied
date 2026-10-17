using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.BusinessPartners.Meetings;

namespace ZR.Repository.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Repository - Generic meeting system for Leads and Customers
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpMeetingRepository : BaseRepository<BpMeeting>
    {
        /// <summary>
        /// Get meetings with participants and outcome
        /// </summary>
        public List<BpMeeting> GetMeetingsWithDetails(long? entityId = null, string entityType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = Context.Queryable<BpMeeting>();

            if (entityId.HasValue)
                query = query.Where(m => m.EntityId == entityId.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(m => m.EntityType == entityType);

            if (startDate.HasValue)
                query = query.Where(m => m.StartDatetime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(m => m.EndDatetime <= endDate.Value);

            return query.OrderBy(m => m.StartDatetime, OrderByType.Desc).ToList();
        }

        /// <summary>
        /// Get upcoming meetings for an employee
        /// </summary>
        public List<BpMeeting> GetEmployeeUpcomingMeetings(long employeeId, DateTime? fromDate = null)
        {
            var from = fromDate ?? DateTime.Now;

            return Context.Queryable<BpMeeting, BpMeetingParticipant>((m, p) => new JoinQueryInfos(
                    JoinType.Inner, m.MeetingId == p.MeetingId
                ))
                .Where((m, p) => p.EmployeeId == employeeId
                    && m.Status == MeetingStatus.SCHEDULED
                    && m.StartDatetime >= from)
                .Select((m, p) => m)
                .OrderBy(m => m.StartDatetime, OrderByType.Asc)
                .ToList();
        }

        /// <summary>
        /// Get calendar meetings for date range
        /// </summary>
        public List<BpMeeting> GetCalendarMeetings(DateTime startDate, DateTime endDate, long? employeeId = null)
        {
            var query = Context.Queryable<BpMeeting>();

            if (employeeId.HasValue)
            {
                query = query
                    .InnerJoin<BpMeetingParticipant>((m, p) => m.MeetingId == p.MeetingId)
                    .Where((m, p) => p.EmployeeId == employeeId.Value)
                    .Select((m, p) => m);
            }

            return query
                .Where(m => m.StartDatetime < endDate && m.EndDatetime > startDate)
                .Where(m => m.Status != MeetingStatus.CANCELLED)
                .OrderBy(m => m.StartDatetime, OrderByType.Asc)
                .ToList();
        }

        /// <summary>
        /// Check if meeting time conflicts with existing meetings
        /// </summary>
        public bool HasTimeConflict(DateTime startDatetime, DateTime endDatetime, long? excludeMeetingId = null)
        {
            var query = Context.Queryable<BpMeeting>()
                .Where(m => m.Status == MeetingStatus.SCHEDULED)
                .Where(m => m.StartDatetime < endDatetime && m.EndDatetime > startDatetime);

            if (excludeMeetingId.HasValue)
                query = query.Where(m => m.MeetingId != excludeMeetingId.Value);

            return query.Any();
        }
    }
}
