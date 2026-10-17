using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Mapster;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Common;
using ZR.Model;
using ZR.Model.BusinessPartners.Meetings.Dto;
using ZR.Model.Customers.Leads;
using ZR.Model.BusinessPartners.Meetings;
using ZR.Model.BusinessPartners;
using ZR.Repository;
using ZR.Repository.BusinessPartners.Meetings;

namespace ZR.Service.BusinessPartners.Meetings
{
    /// <summary>
    /// Meeting Service - Complete meeting lifecycle management with conflict prevention
    /// </summary>
    [AppService(ServiceType = typeof(IBpMeetingService), ServiceLifetime = LifeTime.Transient)]
    public class BpMeetingService : BaseService<BpMeeting>, IBpMeetingService
    {
        private readonly BpMeetingRepository _meetingRepo;
        private readonly BpMeetingParticipantRepository _participantRepo;
        private readonly BpMeetingConflictLockRepository _lockRepo;
        private readonly BpMeetingOutcomeRepository _outcomeRepo;

        public BpMeetingService(
            BpMeetingRepository meetingRepo,
            BpMeetingParticipantRepository participantRepo,
            BpMeetingConflictLockRepository lockRepo,
            BpMeetingOutcomeRepository outcomeRepo)
        {
            _meetingRepo = meetingRepo;
            _participantRepo = participantRepo;
            _lockRepo = lockRepo;
            _outcomeRepo = outcomeRepo;
        }

        // ==================== CORE MEETING OPERATIONS ====================

        public PagedInfo<MeetingDto> GetMeetingList(MeetingQueryDto queryDto)
        {
            var predicate = Expressionable.Create<BpMeeting>();

            if (!string.IsNullOrEmpty(queryDto.EntityType))
                predicate = predicate.And(m => m.EntityType == queryDto.EntityType);

            if (queryDto.EntityId.HasValue)
                predicate = predicate.And(m => m.EntityId == queryDto.EntityId.Value);

            if (!string.IsNullOrEmpty(queryDto.MeetingType))
                predicate = predicate.And(m => m.MeetingType == queryDto.MeetingType);

            if (!string.IsNullOrEmpty(queryDto.Status))
                predicate = predicate.And(m => m.Status == queryDto.Status);

            if (queryDto.StartDate.HasValue)
                predicate = predicate.And(m => m.StartDatetime >= queryDto.StartDate.Value);

            if (queryDto.EndDate.HasValue)
                predicate = predicate.And(m => m.EndDatetime <= queryDto.EndDate.Value);

            if (queryDto.EmployeeId.HasValue)
            {
                // Join with participants to filter by employee
                var employeeMeetingIds = Context.Queryable<BpMeetingParticipant>()
                    .Where(p => p.EmployeeId == queryDto.EmployeeId.Value)
                    .Select(p => p.MeetingId)
                    .ToList();

                predicate = predicate.And(m => employeeMeetingIds.Contains(m.MeetingId));
            }

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(m => m.StartDatetime, OrderByType.Desc)
                .ToPage<BpMeeting, MeetingDto>(queryDto);

            // Enrich with entity names
            foreach (var meeting in response.Result)
            {
                meeting.EntityName = GetEntityName(meeting.EntityType, meeting.EntityId);
                meeting.IsUpcoming = meeting.StartDatetime > DateTime.Now && meeting.Status == MeetingStatus.SCHEDULED;
                meeting.IsActive = meeting.Status == MeetingStatus.SCHEDULED;
                meeting.IsPast = meeting.EndDatetime < DateTime.Now;
            }

            return response;
        }

        public MeetingDto GetMeetingDetails(long meetingId)
        {
            var meeting = _meetingRepo.GetId(meetingId);
            if (meeting == null)
                return null;

            var meetingDto = meeting.Adapt<MeetingDto>();

            // Populate entity name
            meetingDto.EntityName = GetEntityName(meeting.EntityType, meeting.EntityId);

            // Get participants
            meetingDto.Participants = GetMeetingParticipants(meetingId);

            // Get organizer name
            var organizer = meetingDto.Participants?.FirstOrDefault(p => p.Role == ParticipantRole.ORGANIZER);
            meetingDto.OrganizerName = organizer?.EmployeeName ?? "Unknown";

            // Get outcome
            var outcome = _outcomeRepo.GetByMeetingId(meetingId);
            if (outcome != null)
            {
                meetingDto.Outcome = outcome.Adapt<MeetingOutcomeDto>();
                meetingDto.Outcome.HasNextAction = !string.IsNullOrEmpty(outcome.NextAction) && outcome.NextAction != NextAction.NONE;
                meetingDto.Outcome.IsNextActionOverdue = outcome.NextActionDate.HasValue && outcome.NextActionDate.Value < DateTime.Now;
            }

            // Computed properties
            meetingDto.IsUpcoming = meeting.StartDatetime > DateTime.Now && meeting.Status == MeetingStatus.SCHEDULED;
            meetingDto.IsActive = meeting.Status == MeetingStatus.SCHEDULED;
            meetingDto.IsPast = meeting.EndDatetime < DateTime.Now;

            return meetingDto;
        }

        public MeetingDto CreateMeeting(CreateMeetingDto createDto)
        {
            // Validation 1: Entity exists
            if (!ValidateEntityExists(createDto.EntityType, createDto.EntityId))
                throw new Exception($"Entity {createDto.EntityType} with ID {createDto.EntityId} does not exist");

            // Validation 2: Time range is valid
            if (createDto.EndDatetime <= createDto.StartDatetime)
                throw new Exception("End datetime must be after start datetime");

            // Validation 3: Check entity availability
            var availability = CheckAvailability(new CheckAvailabilityDto
            {
                StartDatetime = createDto.StartDatetime,
                EndDatetime = createDto.EndDatetime,
                Scope = LockScope.ENTITY,
                ScopeId = createDto.EntityId
            });

            if (!availability.IsAvailable)
                throw new Exception($"Time slot conflicts with existing meetings: {string.Join(", ", availability.Conflicts.Select(c => c.LockDescription))}");

            // Validation 4: Check creator/organizer availability
            var creatorAvailability = CheckAvailability(new CheckAvailabilityDto
            {
                StartDatetime = createDto.StartDatetime,
                EndDatetime = createDto.EndDatetime,
                Scope = LockScope.EMPLOYEE,
                ScopeId = createDto.CreatedBy
            });

            if (!creatorAvailability.IsAvailable)
                throw new Exception($"Organizer is not available at this time");

            // Validation 5: Check all participants availability
            if (createDto.Participants != null && createDto.Participants.Any())
            {
                foreach (var participant in createDto.Participants)
                {
                    var participantAvailability = CheckAvailability(new CheckAvailabilityDto
                    {
                        StartDatetime = createDto.StartDatetime,
                        EndDatetime = createDto.EndDatetime,
                        Scope = LockScope.EMPLOYEE,
                        ScopeId = participant.EmployeeId
                    });

                    if (!participantAvailability.IsAvailable)
                        throw new Exception($"Participant {participant.EmployeeId} is not available at this time");
                }
            }

            // Begin transaction
            long meetingId = 0;
            UseTran(() =>
            {
                // Step 1: Create meeting
                var meeting = createDto.Adapt<BpMeeting>();
                meeting.MeetingId = SnowFlakeSingle.Instance.NextId();
                meeting.CreatedAt = DateTime.Now;
                meeting.Status = MeetingStatus.SCHEDULED;

                var insertResult = _meetingRepo.Insert(meeting);
                if (insertResult == 0)
                    throw new Exception("Failed to create meeting");

                meetingId = meeting.MeetingId;

                // Step 2: Create entity conflict lock
                _lockRepo.CreateEntityLock(meeting.MeetingId, meeting.EntityId, meeting.StartDatetime, meeting.EndDatetime);

                // Step 3: Create organizer participant + lock
                var organizerParticipant = new BpMeetingParticipant
                {
                    ParticipantId = SnowFlakeSingle.Instance.NextId(),
                    MeetingId = meeting.MeetingId,
                    EmployeeId = createDto.CreatedBy,
                    Role = ParticipantRole.ORGANIZER,
                    AttendanceStatus = AttendanceStatus.ACCEPTED,
                    ResponseDatetime = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                _participantRepo.Insert(organizerParticipant);
                _lockRepo.CreateEmployeeLock(meeting.MeetingId, createDto.CreatedBy, meeting.StartDatetime, meeting.EndDatetime);

                // Step 4: Add other participants + locks (exclude organizer to avoid duplicate)
                if (createDto.Participants != null && createDto.Participants.Any())
                {
                    foreach (var participantInput in createDto.Participants)
                    {
                        // Skip if this participant is the organizer (already added above)
                        if (participantInput.EmployeeId == createDto.CreatedBy)
                            continue;

                        var participant = new BpMeetingParticipant
                        {
                            ParticipantId = SnowFlakeSingle.Instance.NextId(),
                            MeetingId = meeting.MeetingId,
                            EmployeeId = participantInput.EmployeeId,
                            Role = participantInput.Role ?? ParticipantRole.ATTENDEE,
                            AttendanceStatus = AttendanceStatus.PENDING,
                            Notes = participantInput.Notes,
                            CreatedAt = DateTime.Now
                        };
                        _participantRepo.Insert(participant);
                        _lockRepo.CreateEmployeeLock(meeting.MeetingId, participantInput.EmployeeId, meeting.StartDatetime, meeting.EndDatetime);
                    }
                }
            });

            // Return created meeting with details
            var createdMeeting = GetMeetingDetails(meetingId);
            return createdMeeting;
        }

        public MeetingDto UpdateMeeting(UpdateMeetingDto updateDto)
        {
            var meeting = _meetingRepo.GetId(updateDto.MeetingId);
            if (meeting == null)
                throw new Exception("Meeting not found");

            if (meeting.Status != MeetingStatus.SCHEDULED)
                throw new Exception("Only scheduled meetings can be updated");

            // Check if time is being changed
            bool timeChanged = updateDto.StartDatetime.HasValue || updateDto.EndDatetime.HasValue;
            var newStartDatetime = updateDto.StartDatetime ?? meeting.StartDatetime;
            var newEndDatetime = updateDto.EndDatetime ?? meeting.EndDatetime;

            if (timeChanged)
            {
                // Validation 1: Time range is valid
                if (newEndDatetime <= newStartDatetime)
                    throw new Exception("End datetime must be after start datetime");

                // Validation 2: Check entity availability (exclude current meeting)
                var availability = CheckAvailability(new CheckAvailabilityDto
                {
                    StartDatetime = newStartDatetime,
                    EndDatetime = newEndDatetime,
                    Scope = LockScope.ENTITY,
                    ScopeId = meeting.EntityId,
                    ExcludeMeetingId = meeting.MeetingId
                });

                if (!availability.IsAvailable)
                    throw new Exception($"Time slot conflicts with existing meetings");

                // Validation 3: Check all participants availability
                var participants = _participantRepo.GetMeetingParticipants(meeting.MeetingId);
                foreach (var participant in participants)
                {
                    var participantAvailability = CheckAvailability(new CheckAvailabilityDto
                    {
                        StartDatetime = newStartDatetime,
                        EndDatetime = newEndDatetime,
                        Scope = LockScope.EMPLOYEE,
                        ScopeId = participant.EmployeeId,
                        ExcludeMeetingId = meeting.MeetingId
                    });

                    if (!participantAvailability.IsAvailable)
                        throw new Exception($"Participant {participant.EmployeeId} is not available at new time");
                }
            }

            // Update meeting
            meeting.Title = updateDto.Title ?? meeting.Title;
            meeting.Description = updateDto.Description ?? meeting.Description;
            meeting.StartDatetime = newStartDatetime;
            meeting.EndDatetime = newEndDatetime;
            meeting.Timezone = updateDto.Timezone ?? meeting.Timezone;
            meeting.MeetingType = updateDto.MeetingType ?? meeting.MeetingType;
            meeting.MeetingChannel = updateDto.MeetingChannel ?? meeting.MeetingChannel;
            meeting.Location = updateDto.Location ?? meeting.Location;
            meeting.MeetingUrl = updateDto.MeetingUrl ?? meeting.MeetingUrl;
            meeting.UpdatedBy = updateDto.UpdatedBy;
            meeting.UpdatedAt = DateTime.Now;

            var result = _meetingRepo.Update(meeting, true);
            if (result == 0)
                throw new Exception("Failed to update meeting");

            // If time changed, regenerate all locks
            if (timeChanged)
            {
                UseTran(() =>
                {
                    // Delete old locks
                    _lockRepo.DeleteMeetingLocks(meeting.MeetingId);

                    // Create new entity lock
                    _lockRepo.CreateEntityLock(meeting.MeetingId, meeting.EntityId, newStartDatetime, newEndDatetime);

                    // Create new employee locks for all participants
                    var participants = _participantRepo.GetMeetingParticipants(meeting.MeetingId);
                    foreach (var participant in participants)
                    {
                        _lockRepo.CreateEmployeeLock(meeting.MeetingId, participant.EmployeeId, newStartDatetime, newEndDatetime);
                    }
                });
            }

            var updatedMeeting = GetMeetingDetails(meeting.MeetingId);
            return updatedMeeting;
        }

        public int CancelMeeting(CancelMeetingDto cancelDto)
        {
            var meeting = _meetingRepo.GetId(cancelDto.MeetingId);
            if (meeting == null)
                throw new Exception("Meeting not found");

            if (meeting.Status != MeetingStatus.SCHEDULED)
                throw new Exception("Only scheduled meetings can be cancelled");

            UseTran(() =>
            {
                // Update meeting status
                meeting.Status = MeetingStatus.CANCELLED;
                meeting.CancelReason = cancelDto.CancelReason;
                meeting.UpdatedBy = cancelDto.UpdatedBy;
                meeting.UpdatedAt = DateTime.Now;
                _meetingRepo.Update(meeting, true);

                // Delete all conflict locks
                _lockRepo.DeleteMeetingLocks(meeting.MeetingId);
            });

            return 1;
        }

        public MeetingDto CompleteMeeting(CompleteMeetingDto completeDto)
        {
            var meeting = _meetingRepo.GetId(completeDto.MeetingId);
            if (meeting == null)
                throw new Exception("Meeting not found");

            if (meeting.Status != MeetingStatus.SCHEDULED)
                throw new Exception("Only scheduled meetings can be completed");

            UseTran(() =>
            {
                // Update meeting status
                meeting.Status = MeetingStatus.COMPLETED;
                meeting.UpdatedBy = completeDto.UpdatedBy;
                meeting.UpdatedAt = DateTime.Now;
                _meetingRepo.Update(meeting, true);

                // Delete conflict locks
                _lockRepo.DeleteMeetingLocks(meeting.MeetingId);

                // Create outcome if provided
                if (!string.IsNullOrEmpty(completeDto.OutcomeType))
                {
                    var outcome = new BpMeetingOutcome
                    {
                        OutcomeId = SnowFlakeSingle.Instance.NextId(),
                        MeetingId = meeting.MeetingId,
                        OutcomeType = completeDto.OutcomeType,
                        Notes = completeDto.Notes,
                        NextAction = completeDto.NextAction,
                        NextActionDate = completeDto.NextActionDate,
                        NextActionAssignee = completeDto.NextActionAssignee,
                        RecordedBy = completeDto.UpdatedBy,
                        RecordedAt = DateTime.Now
                    };
                    _outcomeRepo.Insert(outcome);
                }
            });

            var completedMeeting = GetMeetingDetails(meeting.MeetingId);
            return completedMeeting;
        }

        public int MarkNoShow(long meetingId, long updatedBy)
        {
            var meeting = _meetingRepo.GetId(meetingId);
            if (meeting == null)
                throw new Exception("Meeting not found");

            if (meeting.Status != MeetingStatus.SCHEDULED)
                throw new Exception("Only scheduled meetings can be marked as no-show");

            UseTran(() =>
            {
                meeting.Status = MeetingStatus.NO_SHOW;
                meeting.UpdatedBy = updatedBy;
                meeting.UpdatedAt = DateTime.Now;
                _meetingRepo.Update(meeting, true);

                // Delete conflict locks
                _lockRepo.DeleteMeetingLocks(meeting.MeetingId);
            });

            return 1;
        }

        // ==================== PARTICIPANT MANAGEMENT ====================

        public MeetingParticipantDto AddParticipant(AddParticipantDto addDto)
        {
            var meeting = _meetingRepo.GetId(addDto.MeetingId);
            if (meeting == null)
                throw new Exception("Meeting not found");

            if (meeting.Status != MeetingStatus.SCHEDULED)
                throw new Exception("Can only add participants to scheduled meetings");

            // Check if already a participant
            if (_participantRepo.IsParticipant(addDto.MeetingId, addDto.EmployeeId))
                throw new Exception("Employee is already a participant");

            // Check employee availability
            var availability = CheckAvailability(new CheckAvailabilityDto
            {
                StartDatetime = meeting.StartDatetime,
                EndDatetime = meeting.EndDatetime,
                Scope = LockScope.EMPLOYEE,
                ScopeId = addDto.EmployeeId
            });

            if (!availability.IsAvailable)
                throw new Exception("Employee is not available at this time");

            UseTran(() =>
            {
                // Add participant
                var participant = new BpMeetingParticipant
                {
                    ParticipantId = SnowFlakeSingle.Instance.NextId(),
                    MeetingId = addDto.MeetingId,
                    EmployeeId = addDto.EmployeeId,
                    Role = addDto.Role ?? ParticipantRole.ATTENDEE,
                    AttendanceStatus = AttendanceStatus.PENDING,
                    Notes = addDto.Notes,
                    CreatedAt = DateTime.Now
                };
                _participantRepo.Insert(participant);

                // Create employee lock
                _lockRepo.CreateEmployeeLock(meeting.MeetingId, addDto.EmployeeId, meeting.StartDatetime, meeting.EndDatetime);
            });

            var participants = GetMeetingParticipants(addDto.MeetingId);
            var addedParticipant = participants.FirstOrDefault(p => p.EmployeeId == addDto.EmployeeId);

            return addedParticipant;
        }

        public int RemoveParticipant(long participantId)
        {
            var participant = _participantRepo.GetId(participantId);
            if (participant == null)
                throw new Exception("Participant not found");

            if (participant.Role == ParticipantRole.ORGANIZER)
                throw new Exception("Cannot remove organizer from meeting");

            var meeting = _meetingRepo.GetId(participant.MeetingId);
            if (meeting != null && meeting.Status != MeetingStatus.SCHEDULED)
                throw new Exception("Can only remove participants from scheduled meetings");

            UseTran(() =>
            {
                // Remove participant
                _participantRepo.Delete(participantId);

                // Remove employee lock
                var locks = _lockRepo.GetEmployeeLocks(participant.EmployeeId);
                var meetingLock = locks.FirstOrDefault(l => l.MeetingId == participant.MeetingId);
                if (meetingLock != null)
                    _lockRepo.Delete(meetingLock.LockId);
            });

            return 1;
        }

        public MeetingParticipantDto UpdateParticipantAttendance(UpdateParticipantAttendanceDto updateDto)
        {
            var success = _participantRepo.UpdateAttendanceStatus(updateDto.ParticipantId, updateDto.AttendanceStatus, updateDto.Notes);
            if (!success)
                throw new Exception("Failed to update attendance status");

            var participant = _participantRepo.GetId(updateDto.ParticipantId);
            var participantDto = participant.Adapt<MeetingParticipantDto>();

            return participantDto;
        }

        public List<MeetingParticipantDto> GetMeetingParticipants(long meetingId)
        {
            var participants = _participantRepo.GetMeetingParticipants(meetingId);
            var participantDtos = participants.Adapt<List<MeetingParticipantDto>>();

            // Enrich with employee names (would need employee service injection)
            // For now, returning without names - can be enhanced later

            return participantDtos;
        }

        public List<MeetingDto> GetPendingInvitations(long employeeId)
        {
            var pendingParticipations = _participantRepo.GetPendingResponses(employeeId);
            var meetingIds = pendingParticipations.Select(p => p.MeetingId).ToList();

            var meetings = Context.Queryable<BpMeeting>()
                .Where(m => meetingIds.Contains(m.MeetingId))
                .Where(m => m.Status == MeetingStatus.SCHEDULED)
                .ToList();

            var meetingDtos = meetings.Adapt<List<MeetingDto>>();

            foreach (var meeting in meetingDtos)
            {
                meeting.EntityName = GetEntityName(meeting.EntityType, meeting.EntityId);
            }

            return meetingDtos;
        }

        // ==================== CONFLICT DETECTION & AVAILABILITY ====================

        public AvailabilityResponseDto CheckAvailability(CheckAvailabilityDto checkDto)
        {
            var hasConflict = _lockRepo.HasConflict(
                checkDto.Scope,
                checkDto.ScopeId,
                checkDto.StartDatetime,
                checkDto.EndDatetime,
                checkDto.ExcludeMeetingId
            );

            var response = new AvailabilityResponseDto
            {
                IsAvailable = !hasConflict,
                Conflicts = new List<ConflictDto>()
            };

            if (hasConflict)
            {
                var conflicts = _lockRepo.GetConflicts(
                    checkDto.Scope,
                    checkDto.ScopeId,
                    checkDto.StartDatetime,
                    checkDto.EndDatetime,
                    checkDto.ExcludeMeetingId
                );

                response.Conflicts = conflicts.Adapt<List<ConflictDto>>();

                // Enrich with meeting titles
                foreach (var conflict in response.Conflicts)
                {
                    var meeting = _meetingRepo.GetId(conflict.MeetingId);
                    if (meeting != null)
                    {
                        conflict.MeetingTitle = meeting.Title;
                        conflict.LockDescription = $"{conflict.Scope}: {meeting.Title}";
                    }
                }
            }

            return response;
        }

        public List<TimeSlotDto> GetAvailableTimeSlots(long employeeId, DateTime date, int durationMinutes = 60)
        {
            var dayStart = date.Date.AddHours(8); // Start at 8 AM
            var dayEnd = date.Date.AddHours(18);  // End at 6 PM

            var timeSlots = new List<TimeSlotDto>();
            var currentSlot = dayStart;

            while (currentSlot.AddMinutes(durationMinutes) <= dayEnd)
            {
                var slotEnd = currentSlot.AddMinutes(durationMinutes);

                var availability = CheckAvailability(new CheckAvailabilityDto
                {
                    StartDatetime = currentSlot,
                    EndDatetime = slotEnd,
                    Scope = LockScope.EMPLOYEE,
                    ScopeId = employeeId
                });

                timeSlots.Add(new TimeSlotDto
                {
                    StartTime = currentSlot,
                    EndTime = slotEnd,
                    IsAvailable = availability.IsAvailable,
                    ConflictReason = availability.IsAvailable ? null : string.Join(", ", availability.Conflicts.Select(c => c.MeetingTitle))
                });

                currentSlot = currentSlot.AddMinutes(30); // 30-minute intervals
            }

            return timeSlots;
        }

        // ==================== CALENDAR & SCHEDULING ====================

        public EmployeeScheduleDto GetEmployeeCalendar(CalendarQueryDto queryDto)
        {
            if (!queryDto.EmployeeId.HasValue)
                return null;

            var meetings = _meetingRepo.GetCalendarMeetings(queryDto.StartDate, queryDto.EndDate, queryDto.EmployeeId.Value);
            var calendarEvents = new List<CalendarEventDto>();

            foreach (var meeting in meetings)
            {
                var participants = _participantRepo.GetMeetingParticipants(meeting.MeetingId);
                var myParticipation = participants.FirstOrDefault(p => p.EmployeeId == queryDto.EmployeeId.Value);

                var eventDto = meeting.Adapt<CalendarEventDto>();
                eventDto.EntityName = GetEntityName(meeting.EntityType, meeting.EntityId);
                eventDto.ParticipantCount = participants.Count;
                eventDto.MyRole = myParticipation?.Role;
                eventDto.MyAttendanceStatus = myParticipation?.AttendanceStatus;
                eventDto.IsUpcoming = meeting.StartDatetime > DateTime.Now && meeting.Status == MeetingStatus.SCHEDULED;
                eventDto.RequiresAction = myParticipation?.AttendanceStatus == AttendanceStatus.PENDING;

                var organizer = participants.FirstOrDefault(p => p.Role == ParticipantRole.ORGANIZER);
                eventDto.OrganizerName = organizer != null ? $"Employee {organizer.EmployeeId}" : "Unknown";

                calendarEvents.Add(eventDto);
            }

            return new EmployeeScheduleDto
            {
                EmployeeId = queryDto.EmployeeId.Value,
                DateStart = queryDto.StartDate,
                DateEnd = queryDto.EndDate,
                Meetings = calendarEvents,
                AvailableSlots = new List<TimeSlotDto>(), // Can be computed if needed
                BusySlots = new List<TimeSlotDto>()       // Can be computed if needed
            };
        }

        public List<CalendarEventDto> GetCompanyCalendar(CalendarQueryDto queryDto)
        {
            var meetings = _meetingRepo.GetCalendarMeetings(queryDto.StartDate, queryDto.EndDate);
            var calendarEvents = meetings.Adapt<List<CalendarEventDto>>();

            foreach (var eventDto in calendarEvents)
            {
                var meeting = meetings.FirstOrDefault(m => m.MeetingId == eventDto.MeetingId);
                if (meeting != null)
                {
                    eventDto.EntityName = GetEntityName(meeting.EntityType, meeting.EntityId);
                    var participants = _participantRepo.GetMeetingParticipants(meeting.MeetingId);
                    eventDto.ParticipantCount = participants.Count;
                    var organizer = participants.FirstOrDefault(p => p.Role == ParticipantRole.ORGANIZER);
                    eventDto.OrganizerName = organizer != null ? $"Employee {organizer.EmployeeId}" : "Unknown";
                }
            }

            return calendarEvents;
        }

        public List<MeetingDto> GetEntityUpcomingMeetings(string entityType, long entityId)
        {
            var meetings = Context.Queryable<BpMeeting>()
                .Where(m => m.EntityType == entityType && m.EntityId == entityId)
                .Where(m => m.Status == MeetingStatus.SCHEDULED)
                .Where(m => m.StartDatetime > DateTime.Now)
                .OrderBy(m => m.StartDatetime, OrderByType.Asc)
                .ToList();

            var meetingDtos = meetings.Adapt<List<MeetingDto>>();

            foreach (var meeting in meetingDtos)
            {
                meeting.EntityName = GetEntityName(meeting.EntityType, meeting.EntityId);
            }

            return meetingDtos;
        }

        public List<MeetingDto> GetEmployeeUpcomingMeetings(long employeeId, int daysAhead = 7)
        {
            var endDate = DateTime.Now.AddDays(daysAhead);
            var meetings = _meetingRepo.GetEmployeeUpcomingMeetings(employeeId, DateTime.Now);

            meetings = meetings.Where(m => m.StartDatetime <= endDate).ToList();

            var meetingDtos = meetings.Adapt<List<MeetingDto>>();

            foreach (var meeting in meetingDtos)
            {
                meeting.EntityName = GetEntityName(meeting.EntityType, meeting.EntityId);
            }

            return meetingDtos;
        }

        // ==================== OUTCOME MANAGEMENT ====================

        public MeetingOutcomeDto CreateOrUpdateOutcome(CreateMeetingOutcomeDto createDto)
        {
            var outcome = createDto.Adapt<BpMeetingOutcome>();
            outcome.OutcomeId = SnowFlakeSingle.Instance.NextId();

            var success = _outcomeRepo.CreateOrUpdateOutcome(outcome);
            if (!success)
                throw new Exception("Failed to save outcome");

            var outcomeDto = outcome.Adapt<MeetingOutcomeDto>();
            return outcomeDto;
        }

        public List<MeetingOutcomeDto> GetPendingFollowUps(long? employeeId = null, bool overdueOnly = false)
        {
            var outcomes = _outcomeRepo.GetPendingFollowUps(employeeId, overdueOnly);
            var outcomeDtos = outcomes.Adapt<List<MeetingOutcomeDto>>();

            foreach (var outcome in outcomeDtos)
            {
                outcome.HasNextAction = !string.IsNullOrEmpty(outcome.NextAction) && outcome.NextAction != NextAction.NONE;
                outcome.IsNextActionOverdue = outcome.NextActionDate.HasValue && outcome.NextActionDate.Value < DateTime.Now;
            }

            return outcomeDtos;
        }

        public MeetingOutcomeDto GetMeetingOutcome(long meetingId)
        {
            var outcome = _outcomeRepo.GetByMeetingId(meetingId);
            if (outcome == null)
                return null;

            var outcomeDto = outcome.Adapt<MeetingOutcomeDto>();
            outcomeDto.HasNextAction = !string.IsNullOrEmpty(outcome.NextAction) && outcome.NextAction != NextAction.NONE;
            outcomeDto.IsNextActionOverdue = outcome.NextActionDate.HasValue && outcome.NextActionDate.Value < DateTime.Now;

            return outcomeDto;
        }

        // ==================== STATISTICS & REPORTING ====================

        public MeetingStatisticsDto GetMeetingStatistics(DateTime? startDate = null, DateTime? endDate = null, long? employeeId = null)
        {
            var query = Context.Queryable<BpMeeting>();

            if (startDate.HasValue)
                query = query.Where(m => m.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(m => m.CreatedAt <= endDate.Value);

            if (employeeId.HasValue)
            {
                var employeeMeetingIds = Context.Queryable<BpMeetingParticipant>()
                    .Where(p => p.EmployeeId == employeeId.Value)
                    .Select(p => p.MeetingId)
                    .ToList();
                query = query.Where(m => employeeMeetingIds.Contains(m.MeetingId));
            }

            var meetings = query.ToList();

            var totalScheduled = meetings.Count(m => m.Status == MeetingStatus.SCHEDULED);
            var totalCompleted = meetings.Count(m => m.Status == MeetingStatus.COMPLETED);
            var totalCancelled = meetings.Count(m => m.Status == MeetingStatus.CANCELLED);
            var totalNoShow = meetings.Count(m => m.Status == MeetingStatus.NO_SHOW);

            var now = DateTime.Now;
            var upcomingToday = meetings.Count(m => m.Status == MeetingStatus.SCHEDULED && m.StartDatetime.Date == now.Date && m.StartDatetime > now);
            var upcomingThisWeek = meetings.Count(m => m.Status == MeetingStatus.SCHEDULED && m.StartDatetime >= now && m.StartDatetime < now.AddDays(7));
            var upcomingThisMonth = meetings.Count(m => m.Status == MeetingStatus.SCHEDULED && m.StartDatetime.Year == now.Year && m.StartDatetime.Month == now.Month && m.StartDatetime > now);

            var total = meetings.Count;
            var completionRate = total > 0 ? (decimal)totalCompleted / total * 100 : 0;
            var cancellationRate = total > 0 ? (decimal)totalCancelled / total * 100 : 0;
            var noShowRate = total > 0 ? (decimal)totalNoShow / total * 100 : 0;

            var totalOnsite = meetings.Count(m => m.MeetingType == MeetingType.ONSITE);
            var totalOnline = meetings.Count(m => m.MeetingType == MeetingType.ONLINE);
            var totalCall = meetings.Count(m => m.MeetingType == MeetingType.CALL);

            var avgDuration = meetings.Any() ? meetings.Average(m => (m.EndDatetime - m.StartDatetime).TotalMinutes) : 0;

            return new MeetingStatisticsDto
            {
                TotalScheduled = totalScheduled,
                TotalCompleted = totalCompleted,
                TotalCancelled = totalCancelled,
                TotalNoShow = totalNoShow,
                UpcomingToday = upcomingToday,
                UpcomingThisWeek = upcomingThisWeek,
                UpcomingThisMonth = upcomingThisMonth,
                CompletionRate = completionRate,
                CancellationRate = cancellationRate,
                NoShowRate = noShowRate,
                TotalOnsite = totalOnsite,
                TotalOnline = totalOnline,
                TotalCall = totalCall,
                AvgMeetingDurationMinutes = (decimal)avgDuration
            };
        }

        public MeetingOutcomeStatisticsDto GetOutcomeStatistics(DateTime? startDate = null, DateTime? endDate = null, long? employeeId = null)
        {
            var stats = _outcomeRepo.GetOutcomeStatistics(startDate, endDate, employeeId);

            var totalPositive = stats.ContainsKey("POSITIVE") ? stats["POSITIVE"] : 0;
            var totalNeutral = stats.ContainsKey("NEUTRAL") ? stats["NEUTRAL"] : 0;
            var totalNegative = stats.ContainsKey("NEGATIVE") ? stats["NEGATIVE"] : 0;
            var total = stats.ContainsKey("TOTAL") ? stats["TOTAL"] : 0;

            var positiveRate = total > 0 ? (decimal)totalPositive / total * 100 : 0;
            var neutralRate = total > 0 ? (decimal)totalNeutral / total * 100 : 0;
            var negativeRate = total > 0 ? (decimal)totalNegative / total * 100 : 0;

            var pendingFollowUps = GetPendingFollowUps(employeeId, false).Count;
            var overdueFollowUps = GetPendingFollowUps(employeeId, true).Count;

            return new MeetingOutcomeStatisticsDto
            {
                TotalPositive = totalPositive,
                TotalNeutral = totalNeutral,
                TotalNegative = totalNegative,
                PositiveRate = positiveRate,
                NeutralRate = neutralRate,
                NegativeRate = negativeRate,
                PendingFollowUps = pendingFollowUps,
                OverdueFollowUps = overdueFollowUps
            };
        }

        // ==================== HELPER METHODS ====================

        public bool ValidateEntityExists(string entityType, long entityId)
        {
            if (entityType == EntityType.LEAD)
            {
                return Context.Queryable<BpLead>().Where(l => l.LeadId == entityId).Any();
            }
            else if (entityType == EntityType.CUSTOMER)
            {
                return Context.Queryable<BpBusinesspartner>().Where(c => c.Partnerid == entityId).Any();
            }

            return false;
        }

        public string GetEntityName(string entityType, long entityId)
        {
            if (entityType == EntityType.LEAD)
            {
                var lead = Context.Queryable<BpLead>().Where(l => l.LeadId == entityId).First();
                return lead?.BrandName ?? "Unknown Lead";
            }
            else if (entityType == EntityType.CUSTOMER)
            {
                var customer = Context.Queryable<BpBusinesspartner>().Where(c => c.Partnerid == entityId).First();
                return customer?.Commercialname ?? "Unknown Customer";
            }

            return "Unknown Entity";
        }
    }
}
