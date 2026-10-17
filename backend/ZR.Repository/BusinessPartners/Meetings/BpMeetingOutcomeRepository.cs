using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model.BusinessPartners.Meetings;

namespace ZR.Repository.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Outcome Repository - Post-meeting results and follow-ups
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpMeetingOutcomeRepository : BaseRepository<BpMeetingOutcome>
    {
        /// <summary>
        /// Get outcome for a meeting
        /// </summary>
        public BpMeetingOutcome GetByMeetingId(long meetingId)
        {
            return Context.Queryable<BpMeetingOutcome>()
                .Where(o => o.MeetingId == meetingId)
                .First();
        }

        /// <summary>
        /// Get all outcomes with pending follow-ups
        /// </summary>
        public List<BpMeetingOutcome> GetPendingFollowUps(long? employeeId = null, bool overdueOnly = false)
        {
            var query = Context.Queryable<BpMeetingOutcome>()
                .Where(o => o.NextAction != null && o.NextAction != NextAction.NONE)
                .Where(o => o.NextActionDate.HasValue);

            if (overdueOnly)
                query = query.Where(o => o.NextActionDate.Value < DateTime.Now);

            if (employeeId.HasValue)
                query = query.Where(o => o.NextActionAssignee == employeeId.Value);

            return query.OrderBy(o => o.NextActionDate, OrderByType.Asc).ToList();
        }

        /// <summary>
        /// Get outcomes by type
        /// </summary>
        public List<BpMeetingOutcome> GetByOutcomeType(string outcomeType, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = Context.Queryable<BpMeetingOutcome>()
                .Where(o => o.OutcomeType == outcomeType);

            if (startDate.HasValue)
                query = query.Where(o => o.RecordedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.RecordedAt <= endDate.Value);

            return query.OrderBy(o => o.RecordedAt, OrderByType.Desc).ToList();
        }

        /// <summary>
        /// Get outcome statistics for a period
        /// </summary>
        public Dictionary<string, int> GetOutcomeStatistics(DateTime? startDate = null, DateTime? endDate = null, long? employeeId = null)
        {
            var query = Context.Queryable<BpMeetingOutcome>();

            if (startDate.HasValue)
                query = query.Where(o => o.RecordedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.RecordedAt <= endDate.Value);

            if (employeeId.HasValue)
                query = query.Where(o => o.RecordedBy == employeeId.Value);

            var outcomes = query.ToList();

            return new Dictionary<string, int>
            {
                { "POSITIVE", outcomes.Count(o => o.OutcomeType == OutcomeType.POSITIVE) },
                { "NEUTRAL", outcomes.Count(o => o.OutcomeType == OutcomeType.NEUTRAL) },
                { "NEGATIVE", outcomes.Count(o => o.OutcomeType == OutcomeType.NEGATIVE) },
                { "TOTAL", outcomes.Count }
            };
        }

        /// <summary>
        /// Get overdue follow-ups count
        /// </summary>
        public int GetOverdueFollowUpsCount(long? employeeId = null)
        {
            var query = Context.Queryable<BpMeetingOutcome>()
                .Where(o => o.NextAction != null && o.NextAction != NextAction.NONE)
                .Where(o => o.NextActionDate.HasValue && o.NextActionDate.Value < DateTime.Now);

            if (employeeId.HasValue)
                query = query.Where(o => o.NextActionAssignee == employeeId.Value);

            return query.Count();
        }

        /// <summary>
        /// Create or update outcome
        /// </summary>
        public bool CreateOrUpdateOutcome(BpMeetingOutcome outcome)
        {
            var existing = GetByMeetingId(outcome.MeetingId);

            if (existing != null)
            {
                // Update existing
                existing.OutcomeType = outcome.OutcomeType;
                existing.Notes = outcome.Notes;
                existing.NextAction = outcome.NextAction;
                existing.NextActionDate = outcome.NextActionDate;
                existing.NextActionAssignee = outcome.NextActionAssignee;
                existing.UpdatedAt = DateTime.Now;

                return Context.Updateable(existing).ExecuteCommand() > 0;
            }
            else
            {
                // Create new
                outcome.RecordedAt = DateTime.Now;
                return Context.Insertable(outcome).ExecuteCommand() > 0;
            }
        }
    }
}
