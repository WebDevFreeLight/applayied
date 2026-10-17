# Meeting System - Complete Architecture Explanation

**Created:** 2025-12-27
**Author:** Claude
**Purpose:** Comprehensive explanation of the generic meeting system architecture

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Database Tables Explained](#database-tables-explained)
3. [Service Layer Explained](#service-layer-explained)
4. [Business Rules](#business-rules)
5. [API Response Examples](#api-response-examples)
6. [Usage Examples](#usage-examples)

---

## Overview

The meeting system is a **generic, conflict-aware scheduling system** that supports both **Leads** (potential customers) and **Customers** (existing partners). It prevents double-booking through an explicit conflict lock mechanism and tracks the complete meeting lifecycle from scheduling to outcome.

### Key Features
✅ **Generic Entity Support** - Works with both Leads and Customers
✅ **Conflict Prevention** - Explicit locking at COMPANY, EMPLOYEE, and ENTITY levels
✅ **Full Lifecycle** - SCHEDULED → COMPLETED/CANCELLED/NO_SHOW
✅ **Participant Management** - Many-to-many with roles and attendance tracking
✅ **Outcome Tracking** - Post-meeting results and follow-up actions
✅ **Calendar Integration** - Employee and company-wide scheduling

---

## Database Tables Explained

### 1. `bp_meeting` - Main Meeting Table

**Purpose:** Stores core meeting information for both Leads and Customers

**Key Fields:**
- `meeting_id` - Primary key (Snowflake ID)
- `entity_type` - ENUM: 'LEAD' or 'CUSTOMER'
- `entity_id` - Foreign key to bp_lead.lead_id OR bp_businesspartner.partner_id
- `start_datetime` / `end_datetime` - Meeting time range
- `meeting_type` - ENUM: 'ONSITE', 'ONLINE', 'CALL'
- `meeting_channel` - ENUM: 'ZOOM', 'GOOGLE_MEET', 'TEAMS', 'PHONE', 'OFFICE', 'OTHER'
- `status` - ENUM: 'SCHEDULED', 'COMPLETED', 'CANCELLED', 'NO_SHOW'

**What It Does:**
- Acts as the central meeting record
- Supports polymorphic entity references (Lead OR Customer)
- Automatically creates entity conflict lock via database trigger

**Response Example:**
```json
{
  "meetingId": "1234567890123456789",
  "entityType": "LEAD",
  "entityId": "9876543210987654321",
  "entityName": "Acme Corporation",
  "title": "Product Demo Meeting",
  "startDatetime": "2025-12-28T10:00:00",
  "endDatetime": "2025-12-28T11:00:00",
  "durationMinutes": 60,
  "meetingType": "ONLINE",
  "meetingChannel": "ZOOM",
  "status": "SCHEDULED",
  "isUpcoming": true
}
```

---

### 2. `bp_meeting_participant` - Participant Junction Table

**Purpose:** Many-to-many relationship between meetings and employees

**Key Fields:**
- `participant_id` - Primary key
- `meeting_id` - FK to bp_meeting
- `employee_id` - FK to hr_employee_core
- `role` - ENUM: 'ORGANIZER', 'ATTENDEE', 'OBSERVER'
- `attendance_status` - ENUM: 'PENDING', 'ACCEPTED', 'DECLINED', 'ATTENDED', 'ABSENT'
- `response_datetime` - When employee accepted/declined

**What It Does:**
- Links employees to meetings with specific roles
- Tracks attendance status (invitation response + actual attendance)
- Creates employee conflict locks when participant is added
- Prevents duplicate participants (unique constraint on meeting_id + employee_id)

**Response Example:**
```json
{
  "participantId": "1111111111111111111",
  "meetingId": "1234567890123456789",
  "employeeId": "2222222222222222222",
  "employeeName": "John Smith",
  "role": "ORGANIZER",
  "attendanceStatus": "ACCEPTED",
  "responseDatetime": "2025-12-27T14:30:00"
}
```

**Business Rules:**
1. **ORGANIZER cannot be removed** - Meeting must have one organizer
2. **Creator is always ORGANIZER** - Auto-assigned when meeting is created
3. **PENDING by default** - New participants start with PENDING status
4. **Auto conflict locks** - Employee lock created when participant added

---

### 3. `bp_meeting_conflict_lock` - Explicit Conflict Prevention

**Purpose:** Prevents double-booking through explicit time-based locks

**Key Fields:**
- `lock_id` - Primary key
- `meeting_id` - FK to bp_meeting
- `scope` - ENUM: 'COMPANY', 'EMPLOYEE', 'ENTITY'
- `scope_id` - NULL for COMPANY, employee_id or entity_id for others
- `start_datetime` / `end_datetime` - Locked time period

**What It Does:**
- Creates explicit "busy" markers for time slots
- Supports three levels of locking:
  - **COMPANY**: All employees unavailable (company event)
  - **EMPLOYEE**: Specific employee unavailable (personal meeting)
  - **ENTITY**: Lead/Customer unavailable (already has meeting)
- Used by conflict detection logic before creating/updating meetings
- Automatically created via database trigger OR service layer

**Lock Scope Explanation:**

| Scope    | Scope ID         | Meaning                                      |
|----------|------------------|----------------------------------------------|
| COMPANY  | NULL             | Company-wide block (all employees busy)      |
| EMPLOYEE | employee_id      | Specific employee is busy                    |
| ENTITY   | lead_id/partner_id | Lead/Customer already has a meeting         |

**Response Example:**
```json
{
  "lockId": "3333333333333333333",
  "meetingId": "1234567890123456789",
  "scope": "EMPLOYEE",
  "scopeId": "2222222222222222222",
  "startDatetime": "2025-12-28T10:00:00",
  "endDatetime": "2025-12-28T11:00:00",
  "lockDescription": "EMPLOYEE: Product Demo Meeting"
}
```

**Business Rules:**
1. **Entity lock** - Created automatically for every meeting (prevents lead/customer double-booking)
2. **Employee locks** - Created for ALL participants (organizer + attendees)
3. **Deleted on cancel** - All locks removed when meeting is cancelled/completed
4. **Regenerated on time change** - Old locks deleted, new ones created when meeting time updated

---

### 4. `bp_meeting_outcome` - Post-Meeting Results

**Purpose:** Records meeting outcomes and follow-up actions

**Key Fields:**
- `outcome_id` - Primary key
- `meeting_id` - FK to bp_meeting (one-to-one relationship)
- `outcome_type` - ENUM: 'POSITIVE', 'NEUTRAL', 'NEGATIVE'
- `next_action` - ENUM: 'FOLLOW_UP', 'PROPOSAL', 'CLOSE', 'SCHEDULE_MEETING', 'SEND_INFO', 'NONE'
- `next_action_date` - When follow-up should happen
- `next_action_assignee` - Employee responsible for follow-up

**What It Does:**
- Stores post-meeting assessment
- Tracks follow-up actions and deadlines
- Enables outcome analytics (positive/neutral/negative rates)
- Flags overdue follow-ups

**Response Example:**
```json
{
  "outcomeId": "4444444444444444444",
  "meetingId": "1234567890123456789",
  "outcomeType": "POSITIVE",
  "notes": "Client expressed strong interest in enterprise package. Ready to proceed with proposal.",
  "nextAction": "PROPOSAL",
  "nextActionDate": "2025-12-30T09:00:00",
  "nextActionAssignee": "2222222222222222222",
  "nextActionAssigneeName": "John Smith",
  "hasNextAction": true,
  "isNextActionOverdue": false,
  "recordedBy": "2222222222222222222",
  "recordedAt": "2025-12-28T11:15:00"
}
```

**Business Rules:**
1. **One per meeting** - Only one outcome record per meeting
2. **Created on complete** - Can be added when completing a meeting OR later
3. **Overdue tracking** - `isNextActionOverdue` computed based on current date
4. **Optional** - Meeting can be completed without outcome (outcome can be added later)

---

## Service Layer Explained

### BpMeetingService - Complete Lifecycle Management

The service orchestrates the entire meeting system, enforcing business rules and managing transactions.

### Core Operations Flow

#### 1. **CreateMeeting**

**Validation Steps:**
1. ✅ Entity (Lead/Customer) exists
2. ✅ End time > Start time
3. ✅ Entity is available (no conflicting meetings)
4. ✅ Organizer is available
5. ✅ All participants are available

**Transaction Steps:**
1. Create meeting record
2. Create entity conflict lock
3. Add organizer as participant with ACCEPTED status
4. Create organizer employee lock
5. Add other participants with PENDING status
6. Create employee locks for all participants

**Returns:** Complete MeetingDto with participants

---

#### 2. **UpdateMeeting**

**Validation Steps:**
1. ✅ Meeting exists and is SCHEDULED
2. ✅ If time changed: new end time > new start time
3. ✅ If time changed: entity availability (excluding current meeting)
4. ✅ If time changed: all participants available at new time

**Transaction Steps:**
1. Update meeting details
2. If time changed:
   - Delete ALL old conflict locks
   - Create new entity lock
   - Create new employee locks for all participants

**Returns:** Updated MeetingDto

---

#### 3. **CancelMeeting**

**Validation Steps:**
1. ✅ Meeting exists and is SCHEDULED

**Transaction Steps:**
1. Set status to CANCELLED
2. Add cancel reason
3. Delete ALL conflict locks (entity + all employees)

**Returns:** Success message

---

#### 4. **CompleteMeeting**

**Validation Steps:**
1. ✅ Meeting exists and is SCHEDULED

**Transaction Steps:**
1. Set status to COMPLETED
2. Delete conflict locks
3. If outcome provided: create outcome record

**Returns:** MeetingDto with outcome

---

#### 5. **AddParticipant**

**Validation Steps:**
1. ✅ Meeting exists and is SCHEDULED
2. ✅ Employee not already a participant
3. ✅ Employee is available at meeting time

**Transaction Steps:**
1. Create participant record (PENDING status)
2. Create employee conflict lock

**Returns:** MeetingParticipantDto

---

### Conflict Detection Logic

**CheckAvailability Method:**

```csharp
public AvailabilityResponseDto CheckAvailability(CheckAvailabilityDto checkDto)
{
    // Queries bp_meeting_conflict_lock table
    // Checks for overlapping locks:
    // - Same scope (COMPANY/EMPLOYEE/ENTITY)
    // - Matching scope_id (if applicable)
    // - Overlapping time range (start < end AND end > start)

    // Returns:
    // - IsAvailable: true/false
    // - Conflicts: list of conflicting meetings
}
```

**Overlap Detection Logic:**
```
Meeting A: [10:00 - 11:00]
Meeting B: [10:30 - 11:30]

Overlap Check:
A.start < B.end  AND  A.end > B.start
10:00 < 11:30    AND  11:00 > 10:30
TRUE             AND  TRUE
→ CONFLICT!
```

---

## Business Rules

### Meeting Lifecycle

```
SCHEDULED → COMPLETED ✅
         → CANCELLED ✅
         → NO_SHOW ✅

From COMPLETED/CANCELLED/NO_SHOW → Cannot transition
```

### Conflict Lock Rules

1. **Entity Lock** - ALWAYS created for every SCHEDULED meeting
2. **Employee Locks** - Created for EVERY participant
3. **Lock Deletion** - On CANCEL, COMPLETE, or NO_SHOW
4. **Lock Regeneration** - On time update

### Participant Rules

1. **Organizer Required** - Every meeting has exactly ONE organizer
2. **Cannot Remove Organizer** - Organizer removal is blocked
3. **Creator is Organizer** - Meeting creator is auto-assigned as ORGANIZER
4. **Unique Participants** - One employee can only participate once per meeting

---

## API Response Examples

### Complete Meeting with All Details

```json
{
  "meetingId": "1234567890123456789",
  "entityType": "LEAD",
  "entityId": "9876543210987654321",
  "entityName": "Acme Corporation",
  "title": "Product Demo Meeting",
  "description": "Demonstrate enterprise features to Acme's technical team",
  "startDatetime": "2025-12-28T10:00:00",
  "endDatetime": "2025-12-28T11:00:00",
  "durationMinutes": 60,
  "timezone": "Asia/Riyadh",
  "meetingType": "ONLINE",
  "meetingChannel": "ZOOM",
  "location": null,
  "meetingUrl": "https://zoom.us/j/123456789",
  "status": "SCHEDULED",
  "cancelReason": null,
  "createdBy": "2222222222222222222",
  "organizerName": "John Smith",
  "createdAt": "2025-12-27T14:30:00",
  "updatedAt": null,
  "participants": [
    {
      "participantId": "1111111111111111111",
      "employeeId": "2222222222222222222",
      "employeeName": "John Smith",
      "role": "ORGANIZER",
      "attendanceStatus": "ACCEPTED"
    },
    {
      "participantId": "5555555555555555555",
      "employeeId": "6666666666666666666",
      "employeeName": "Sarah Johnson",
      "role": "ATTENDEE",
      "attendanceStatus": "PENDING"
    }
  ],
  "outcome": null,
  "isUpcoming": true,
  "isActive": true,
  "isPast": false
}
```

### Conflict Detection Response

```json
{
  "isAvailable": false,
  "conflicts": [
    {
      "lockId": "7777777777777777777",
      "meetingId": "8888888888888888888",
      "meetingTitle": "Team Standup",
      "startDatetime": "2025-12-28T10:00:00",
      "endDatetime": "2025-12-28T10:30:00",
      "scope": "EMPLOYEE",
      "scopeId": "2222222222222222222",
      "lockDescription": "EMPLOYEE: Team Standup"
    }
  ]
}
```

### Employee Calendar Response

```json
{
  "employeeId": "2222222222222222222",
  "employeeName": "John Smith",
  "dateStart": "2025-12-28T00:00:00",
  "dateEnd": "2025-12-29T00:00:00",
  "meetings": [
    {
      "meetingId": "1234567890123456789",
      "title": "Product Demo",
      "startDatetime": "2025-12-28T10:00:00",
      "endDatetime": "2025-12-28T11:00:00",
      "entityType": "LEAD",
      "entityName": "Acme Corporation",
      "myRole": "ORGANIZER",
      "myAttendanceStatus": "ACCEPTED",
      "requiresAction": false
    },
    {
      "meetingId": "9999999999999999999",
      "title": "Follow-up Call",
      "startDatetime": "2025-12-28T14:00:00",
      "endDatetime": "2025-12-28T14:30:00",
      "entityType": "CUSTOMER",
      "entityName": "Beta Industries",
      "myRole": "ATTENDEE",
      "myAttendanceStatus": "PENDING",
      "requiresAction": true
    }
  ]
}
```

---

## Usage Examples

### Example 1: Create Meeting with Multiple Participants

```csharp
var createDto = new CreateMeetingDto
{
    EntityType = "LEAD",
    EntityId = 9876543210987654321,
    Title = "Product Demo Meeting",
    Description = "Showcase enterprise features",
    StartDatetime = new DateTime(2025, 12, 28, 10, 0, 0),
    EndDatetime = new DateTime(2025, 12, 28, 11, 0, 0),
    MeetingType = "ONLINE",
    MeetingChannel = "ZOOM",
    MeetingUrl = "https://zoom.us/j/123456789",
    CreatedBy = 2222222222222222222, // John Smith (becomes ORGANIZER)
    Participants = new List<MeetingParticipantInput>
    {
        new MeetingParticipantInput
        {
            EmployeeId = 6666666666666666666, // Sarah Johnson
            Role = "ATTENDEE"
        },
        new MeetingParticipantInput
        {
            EmployeeId = 7777777777777777777, // Mike Davis
            Role = "ATTENDEE"
        }
    }
};

var result = meetingService.CreateMeeting(createDto);
// Creates:
// - 1 meeting record
// - 4 conflict locks (1 entity + 3 employees)
// - 3 participant records (1 organizer + 2 attendees)
```

### Example 2: Check Availability Before Scheduling

```csharp
var checkDto = new CheckAvailabilityDto
{
    StartDatetime = new DateTime(2025, 12, 28, 10, 0, 0),
    EndDatetime = new DateTime(2025, 12, 28, 11, 0, 0),
    Scope = "EMPLOYEE",
    ScopeId = 2222222222222222222 // John Smith
};

var availability = meetingService.CheckAvailability(checkDto);

if (!availability.IsAvailable)
{
    // Show conflicts to user
    foreach (var conflict in availability.Conflicts)
    {
        Console.WriteLine($"Conflict: {conflict.LockDescription} from {conflict.StartDatetime} to {conflict.EndDatetime}");
    }
}
```

### Example 3: Complete Meeting with Outcome

```csharp
var completeDto = new CompleteMeetingDto
{
    MeetingId = 1234567890123456789,
    UpdatedBy = 2222222222222222222,
    OutcomeType = "POSITIVE",
    Notes = "Client very interested. Ready for proposal.",
    NextAction = "PROPOSAL",
    NextActionDate = new DateTime(2025, 12, 30, 9, 0, 0),
    NextActionAssignee = 2222222222222222222
};

var result = meetingService.CompleteMeeting(completeDto);
// Updates:
// - Meeting status → COMPLETED
// - Deletes all conflict locks
// - Creates outcome record with follow-up action
```

### Example 4: Get Employee's Pending Invitations

```csharp
var pendingInvitations = meetingService.GetPendingInvitations(employeeId: 6666666666666666666);
// Returns all meetings where Sarah Johnson has PENDING attendance status
// She can then Accept or Decline each invitation
```

### Example 5: Update Participant Attendance

```csharp
var updateDto = new UpdateParticipantAttendanceDto
{
    ParticipantId = 5555555555555555555,
    AttendanceStatus = "ACCEPTED",
    Notes = "Looking forward to the demo!"
};

var result = meetingService.UpdateParticipantAttendance(updateDto);
// Updates Sarah Johnson's attendance to ACCEPTED
// Sets response_datetime to current time
```

---

## Summary

### What Makes This System Powerful?

1. **Generic Design** - Works with both Leads and Customers without duplication
2. **Explicit Conflict Prevention** - Lock table makes conflicts visible and queryable
3. **Multi-Level Locking** - Company, Employee, and Entity levels support various scenarios
4. **Complete Lifecycle** - From scheduling to outcome tracking
5. **Robust Validation** - Multiple validation layers prevent invalid data
6. **Transaction Safety** - All multi-step operations wrapped in transactions
7. **Audit Trail** - Full tracking of who created, updated, and attended meetings

### Database Trigger Magic

The PostgreSQL trigger automatically:
- Creates entity lock when meeting is created
- Removes all locks when meeting is cancelled/completed
- Updates locks when meeting time changes

This reduces application code and ensures consistency!

---

**End of Documentation**
