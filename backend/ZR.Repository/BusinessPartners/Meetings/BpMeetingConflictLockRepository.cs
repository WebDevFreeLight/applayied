using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model.BusinessPartners.Meetings;

namespace ZR.Repository.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Conflict Lock Repository - Explicit conflict prevention
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpMeetingConflictLockRepository : BaseRepository<BpMeetingConflictLock>
    {
        /// <summary>
        /// Check if time slot has conflicts
        /// Uses the database function check_meeting_conflict for PostgreSQL
        /// Or manual check for other databases
        /// </summary>
        public bool HasConflict(string scope, long? scopeId, DateTime startDatetime, DateTime endDatetime, long? excludeMeetingId = null)
        {
            // PostgreSQL function call
            if (Context.CurrentConnectionConfig.DbType == DbType.PostgreSQL)
            {
                // Format parameters for PostgreSQL properly
                var scopeIdParam = scopeId.HasValue ? scopeId.Value.ToString() : "NULL";
                var excludeMeetingIdParam = excludeMeetingId.HasValue ? excludeMeetingId.Value.ToString() : "NULL";

                // Use positional parameters ($1, $2, etc.) for PostgreSQL
                var sql = $@"SELECT check_meeting_conflict(
                    '{scope}',
                    {scopeIdParam},
                    '{startDatetime:yyyy-MM-dd HH:mm:ss}',
                    '{endDatetime:yyyy-MM-dd HH:mm:ss}',
                    {excludeMeetingIdParam}
                )";

                var result = Context.Ado.GetScalar(sql);

                return result != null && Convert.ToBoolean(result);
            }

            // Fallback for other databases
            return CheckConflictManual(scope, scopeId, startDatetime, endDatetime, excludeMeetingId);
        }

        /// <summary>
        /// Manual conflict check (fallback for non-PostgreSQL)
        /// </summary>
        private bool CheckConflictManual(string scope, long? scopeId, DateTime startDatetime, DateTime endDatetime, long? excludeMeetingId = null)
        {
            var query = Context.Queryable<BpMeetingConflictLock>()
                .Where(l => l.Scope == scope)
                .Where(l => l.StartDatetime < endDatetime && l.EndDatetime > startDatetime);

            // Scope matching logic
            if (scope == LockScope.COMPANY)
            {
                // Company-wide locks conflict with everything
                query = query.Where(l => l.ScopeId == null || scopeId == null);
            }
            else
            {
                // Employee or Entity specific
                query = query.Where(l => l.ScopeId == scopeId || l.ScopeId == null);
            }

            if (excludeMeetingId.HasValue)
                query = query.Where(l => l.MeetingId != excludeMeetingId.Value);

            return query.Any();
        }

        /// <summary>
        /// Get all conflicts for a time range
        /// </summary>
        public List<BpMeetingConflictLock> GetConflicts(string scope, long? scopeId, DateTime startDatetime, DateTime endDatetime, long? excludeMeetingId = null)
        {
            var query = Context.Queryable<BpMeetingConflictLock>()
                .Where(l => l.Scope == scope)
                .Where(l => l.StartDatetime < endDatetime && l.EndDatetime > startDatetime);

            if (scopeId.HasValue)
                query = query.Where(l => l.ScopeId == scopeId || l.ScopeId == null);

            if (excludeMeetingId.HasValue)
                query = query.Where(l => l.MeetingId != excludeMeetingId.Value);

            return query.ToList();
        }

        /// <summary>
        /// Create lock for entity (lead/customer)
        /// </summary>
        public bool CreateEntityLock(long meetingId, long entityId, DateTime startDatetime, DateTime endDatetime)
        {
            var lockId = Context.Queryable<BpMeetingConflictLock>().Max(l => l.LockId) + 1;

            var entityLock = new BpMeetingConflictLock
            {
                LockId = lockId,
                MeetingId = meetingId,
                Scope = LockScope.ENTITY,
                ScopeId = entityId,
                StartDatetime = startDatetime,
                EndDatetime = endDatetime,
                CreatedAt = DateTime.Now
            };

            return Context.Insertable(entityLock).ExecuteCommand() > 0;
        }

        /// <summary>
        /// Create lock for employee
        /// </summary>
        public bool CreateEmployeeLock(long meetingId, long employeeId, DateTime startDatetime, DateTime endDatetime)
        {
            var lockId = Context.Queryable<BpMeetingConflictLock>().Max(l => l.LockId) + 1;

            var employeeLock = new BpMeetingConflictLock
            {
                LockId = lockId,
                MeetingId = meetingId,
                Scope = LockScope.EMPLOYEE,
                ScopeId = employeeId,
                StartDatetime = startDatetime,
                EndDatetime = endDatetime,
                CreatedAt = DateTime.Now
            };

            return Context.Insertable(employeeLock).ExecuteCommand() > 0;
        }

        /// <summary>
        /// Create company-wide lock
        /// </summary>
        public bool CreateCompanyLock(long meetingId, DateTime startDatetime, DateTime endDatetime)
        {
            var lockId = Context.Queryable<BpMeetingConflictLock>().Max(l => l.LockId) + 1;

            var companyLock = new BpMeetingConflictLock
            {
                LockId = lockId,
                MeetingId = meetingId,
                Scope = LockScope.COMPANY,
                ScopeId = null, // NULL for company-wide
                StartDatetime = startDatetime,
                EndDatetime = endDatetime,
                CreatedAt = DateTime.Now
            };

            return Context.Insertable(companyLock).ExecuteCommand() > 0;
        }

        /// <summary>
        /// Delete all locks for a meeting (when cancelling or updating)
        /// </summary>
        public bool DeleteMeetingLocks(long meetingId)
        {
            return Context.Deleteable<BpMeetingConflictLock>()
                .Where(l => l.MeetingId == meetingId)
                .ExecuteCommand() >= 0; // >= 0 because there might be no locks
        }

        /// <summary>
        /// Get active locks for an employee
        /// </summary>
        public List<BpMeetingConflictLock> GetEmployeeLocks(long employeeId, DateTime? fromDate = null)
        {
            var from = fromDate ?? DateTime.Now;

            return Context.Queryable<BpMeetingConflictLock>()
                .Where(l => l.Scope == LockScope.EMPLOYEE && l.ScopeId == employeeId)
                .Where(l => l.EndDatetime > from)
                .OrderBy(l => l.StartDatetime, OrderByType.Asc)
                .ToList();
        }

        /// <summary>
        /// Get active locks for an entity (lead/customer)
        /// </summary>
        public List<BpMeetingConflictLock> GetEntityLocks(long entityId, DateTime? fromDate = null)
        {
            var from = fromDate ?? DateTime.Now;

            return Context.Queryable<BpMeetingConflictLock>()
                .Where(l => l.Scope == LockScope.ENTITY && l.ScopeId == entityId)
                .Where(l => l.EndDatetime > from)
                .OrderBy(l => l.StartDatetime, OrderByType.Asc)
                .ToList();
        }
    }
}
