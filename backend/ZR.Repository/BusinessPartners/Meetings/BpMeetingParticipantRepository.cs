using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model.BusinessPartners.Meetings;

namespace ZR.Repository.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Participant Repository - Many-to-Many relationship
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpMeetingParticipantRepository : BaseRepository<BpMeetingParticipant>
    {
        /// <summary>
        /// Get all participants for a meeting
        /// </summary>
        public List<BpMeetingParticipant> GetMeetingParticipants(long meetingId)
        {
            return Context.Queryable<BpMeetingParticipant>()
                .Where(p => p.MeetingId == meetingId)
                .OrderBy(p => p.Role == ParticipantRole.ORGANIZER ? 0 : 1, OrderByType.Asc)
                .OrderBy(p => p.CreatedAt, OrderByType.Asc)
                .ToList();
        }

        /// <summary>
        /// Get organizer of a meeting
        /// </summary>
        public BpMeetingParticipant GetOrganizer(long meetingId)
        {
            return Context.Queryable<BpMeetingParticipant>()
                .Where(p => p.MeetingId == meetingId && p.Role == ParticipantRole.ORGANIZER)
                .First();
        }

        /// <summary>
        /// Check if employee is already a participant
        /// </summary>
        public bool IsParticipant(long meetingId, long employeeId)
        {
            return Context.Queryable<BpMeetingParticipant>()
                .Where(p => p.MeetingId == meetingId && p.EmployeeId == employeeId)
                .Any();
        }

        /// <summary>
        /// Get all meetings for an employee with role and status
        /// </summary>
        public List<BpMeetingParticipant> GetEmployeeParticipations(long employeeId, string status = null)
        {
            var query = Context.Queryable<BpMeetingParticipant>()
                .Where(p => p.EmployeeId == employeeId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.AttendanceStatus == status);

            return query.OrderBy(p => p.CreatedAt, OrderByType.Desc).ToList();
        }

        /// <summary>
        /// Get pending attendance responses for employee
        /// </summary>
        public List<BpMeetingParticipant> GetPendingResponses(long employeeId)
        {
            return Context.Queryable<BpMeetingParticipant>()
                .Where(p => p.EmployeeId == employeeId && p.AttendanceStatus == AttendanceStatus.PENDING)
                .OrderBy(p => p.CreatedAt, OrderByType.Asc)
                .ToList();
        }

        /// <summary>
        /// Update attendance status
        /// </summary>
        public bool UpdateAttendanceStatus(long participantId, string status, string notes = null)
        {
            var participant = Context.Queryable<BpMeetingParticipant>()
                .Where(p => p.ParticipantId == participantId)
                .First();

            if (participant == null)
                return false;

            participant.AttendanceStatus = status;
            participant.ResponseDatetime = DateTime.Now;
            participant.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(notes))
                participant.Notes = notes;

            return Context.Updateable(participant).ExecuteCommand() > 0;
        }

        /// <summary>
        /// Delete all participants for a meeting (when cancelling)
        /// </summary>
        public bool DeleteMeetingParticipants(long meetingId)
        {
            return Context.Deleteable<BpMeetingParticipant>()
                .Where(p => p.MeetingId == meetingId)
                .ExecuteCommand() > 0;
        }
    }
}
