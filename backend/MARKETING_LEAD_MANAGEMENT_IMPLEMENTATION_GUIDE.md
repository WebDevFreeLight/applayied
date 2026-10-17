# Marketing Lead Management System - Implementation Guide

## 📋 Overview
This document guides the implementation of a comprehensive Marketing Lead Management System with:
- Lead Activity Tracking
- Lead Assignment History
- Calendar-based Meeting Scheduling
- Marketing Employee Performance Analytics
- Frontend Dashboard with Tabs

---

## ✅ Phase 1: Database & Models (COMPLETED)

### Database Schema Created
File: `ZR.Database/Scripts/Marketing_LeadManagement_DDL.sql`

**Tables Created:**
1. ✅ `bp_lead` - Enhanced with new fields (source, budget, priority, expected_revenue, lost_reason)
2. ✅ `bp_lead_activity` - Activity tracking (calls, meetings, emails, etc.)
3. ✅ `bp_lead_assignment_log` - Assignment history
4. ✅ `bp_lead_meeting` - Calendar-based meeting scheduling

**Views Created:**
- ✅ `v_marketing_employee_performance` - Pre-aggregated performance metrics

### Backend Models Created
**Entities:**
- ✅ `BpLead.cs` - Updated with new properties
- ✅ `BpLeadActivity.cs` - Activity tracking model
- ✅ `BpLeadAssignmentLog.cs` - Assignment history model
- ✅ `BpLeadMeeting.cs` - Meeting scheduling model

**DTOs:**
- ✅ `LeadManagementDto.cs` - Comprehensive DTO collection including:
  - Activity DTOs (Create, Query, Response)
  - Assignment DTOs (Assign, Bulk Assign, Query)
  - Meeting DTOs (Create, Update, Cancel, Complete)
  - Performance DTOs (Metrics, Reports, Distributions)
  - Calendar DTOs (Query, Availability)

### Repositories Created
- ✅ `BpLeadRepository.cs` - Existing, no changes needed
- ✅ `BpLeadActivityRepository.cs` - New
- ✅ `BpLeadAssignmentLogRepository.cs` - New
- ✅ `BpLeadMeetingRepository.cs` - New

---

## 🚧 Phase 2: Services (TO BE IMPLEMENTED)

### 2.1 Lead Activity Service

**File:** `ZR.Service/Marketing/Leads/BpLeadActivityService.cs`

```csharp
using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Marketing.Leads;
using ZR.Model.Marketing.Dto;
using ZR.Repository.Marketing;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing.Leads
{
    [AppService(ServiceType = typeof(IBpLeadActivityService), ServiceLifetime = LifeTime.Transient)]
    public class BpLeadActivityService : BaseService<BpLeadActivity>, IBpLeadActivityService
    {
        private readonly BpLeadActivityRepository _activityRepository;
        private readonly BpLeadRepository _leadRepository;

        public BpLeadActivityService(
            BpLeadActivityRepository activityRepository,
            BpLeadRepository leadRepository)
        {
            _activityRepository = activityRepository;
            _leadRepository = leadRepository;
        }

        public async Task<BpLeadActivity> LogActivityAsync(CreateLeadActivityDto dto, string createdBy)
        {
            var activity = new BpLeadActivity
            {
                ActivityId = SnowFlakeSingle.Instance.NextId(),
                LeadId = dto.LeadId,
                EmployeeId = dto.EmployeeId,
                ActivityType = dto.ActivityType,
                ActivityDate = dto.ActivityDate,
                DurationMinutes = dto.DurationMinutes,
                Outcome = dto.Outcome,
                Notes = dto.Notes,
                NextFollowUpDate = dto.NextFollowUpDate,
                Create_by = createdBy,
                Create_time = DateTime.Now
            };

            await InsertAsync(activity);

            // Update lead's last contact date
            var lead = await _leadRepository.GetByIdAsync(dto.LeadId);
            if (lead != null)
            {
                lead.LastContactDate = dto.ActivityDate;
                if (!lead.FirstContactDate.HasValue)
                {
                    lead.FirstContactDate = dto.ActivityDate;
                }
                await _leadRepository.UpdateAsync(lead);
            }

            return activity;
        }

        public async Task<PagedInfo<LeadActivityDto>> GetActivitiesAsync(LeadActivityQueryDto query)
        {
            var exp = Expressionable.Create<BpLeadActivity>();

            if (query.LeadId.HasValue)
                exp = exp.And(it => it.LeadId == query.LeadId.Value);

            if (query.EmployeeId.HasValue)
                exp = exp.And(it => it.EmployeeId == query.EmployeeId.Value);

            if (!string.IsNullOrEmpty(query.ActivityType))
                exp = exp.And(it => it.ActivityType == query.ActivityType);

            if (query.StartDate.HasValue)
                exp = exp.And(it => it.ActivityDate >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                exp = exp.And(it => it.ActivityDate <= query.EndDate.Value);

            if (!string.IsNullOrEmpty(query.Outcome))
                exp = exp.And(it => it.Outcome == query.Outcome);

            var activities = await Context.Queryable<BpLeadActivity, BpLead, HrEmployeeCore, HrPersonName>(
                (a, l, e, pn) => new JoinQueryInfos(
                    JoinType.Left, a.LeadId == l.LeadId,
                    JoinType.Left, a.EmployeeId == e.EmployeeId,
                    JoinType.Left, e.PersonId == pn.PersonId && pn.IsPrimary == true
                ))
                .Where(exp.ToExpression())
                .OrderBy((a, l, e, pn) => a.ActivityDate, OrderByType.Desc)
                .Select((a, l, e, pn) => new LeadActivityDto
                {
                    ActivityId = a.ActivityId,
                    LeadId = a.LeadId,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = pn.GivenName + " " + pn.FamilyName,
                    LeadName = l.BrandName,
                    ActivityType = a.ActivityType,
                    ActivityDate = a.ActivityDate,
                    DurationMinutes = a.DurationMinutes,
                    Outcome = a.Outcome,
                    Notes = a.Notes,
                    NextFollowUpDate = a.NextFollowUpDate,
                    IsAutomated = a.IsAutomated,
                    CreateTime = a.Create_time
                })
                .ToPageAsync(query);

            return activities;
        }

        public async Task<List<LeadActivityDto>> GetRecentActivitiesAsync(long employeeId, int count = 10)
        {
            var activities = await Context.Queryable<BpLeadActivity, BpLead>(
                (a, l) => new JoinQueryInfos(
                    JoinType.Left, a.LeadId == l.LeadId
                ))
                .Where((a, l) => a.EmployeeId == employeeId)
                .OrderBy((a, l) => a.ActivityDate, OrderByType.Desc)
                .Take(count)
                .Select((a, l) => new LeadActivityDto
                {
                    ActivityId = a.ActivityId,
                    LeadId = a.LeadId,
                    LeadName = l.BrandName,
                    ActivityType = a.ActivityType,
                    ActivityDate = a.ActivityDate,
                    DurationMinutes = a.DurationMinutes,
                    Outcome = a.Outcome,
                    Notes = a.Notes
                })
                .ToListAsync();

            return activities;
        }
    }
}
```

**Interface:** `ZR.Service/Marketing/IMarketingService/IBpLeadActivityService.cs`
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Marketing.Dto;
using ZR.Model.Marketing.Leads;

namespace ZR.Service.Marketing.IMarketingService
{
    public interface IBpLeadActivityService
    {
        Task<BpLeadActivity> LogActivityAsync(CreateLeadActivityDto dto, string createdBy);
        Task<PagedInfo<LeadActivityDto>> GetActivitiesAsync(LeadActivityQueryDto query);
        Task<List<LeadActivityDto>> GetRecentActivitiesAsync(long employeeId, int count = 10);
    }
}
```

### 2.2 Lead Assignment Service

**File:** `ZR.Service/Marketing/Leads/BpLeadAssignmentService.cs`

```csharp
using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Marketing.Leads;
using ZR.Model.Marketing.Dto;
using ZR.Repository.Marketing;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing.Leads
{
    [AppService(ServiceType = typeof(IBpLeadAssignmentService), ServiceLifetime = LifeTime.Transient)]
    public class BpLeadAssignmentService : BaseService<BpLeadAssignmentLog>, IBpLeadAssignmentService
    {
        private readonly BpLeadRepository _leadRepository;

        public BpLeadAssignmentService(BpLeadRepository leadRepository)
        {
            _leadRepository = leadRepository;
        }

        public async Task<BpLeadAssignmentLog> AssignLeadAsync(AssignLeadDto dto, string assignedBy)
        {
            // Get current lead
            var lead = await _leadRepository.GetByIdAsync(dto.LeadId);
            if (lead == null)
            {
                throw new Exception("Lead not found");
            }

            // Create assignment log
            var log = new BpLeadAssignmentLog
            {
                AssignmentLogId = SnowFlakeSingle.Instance.NextId(),
                LeadId = dto.LeadId,
                FromEmployeeId = lead.AssignedTo,
                ToEmployeeId = dto.ToEmployeeId,
                AssignmentDate = DateTime.Now,
                Reason = dto.Reason,
                ReassignmentType = dto.ReassignmentType,
                Create_by = assignedBy,
                Create_time = DateTime.Now
            };

            await InsertAsync(log);

            // Update lead assignment
            lead.AssignedTo = dto.ToEmployeeId;
            lead.Update_by = assignedBy;
            lead.Update_time = DateTime.Now;
            await _leadRepository.UpdateAsync(lead);

            return log;
        }

        public async Task<List<BpLeadAssignmentLog>> BulkAssignLeadsAsync(BulkAssignLeadsDto dto, string assignedBy)
        {
            var logs = new List<BpLeadAssignmentLog>();

            foreach (var leadId in dto.LeadIds)
            {
                try
                {
                    var assignDto = new AssignLeadDto
                    {
                        LeadId = leadId,
                        ToEmployeeId = dto.ToEmployeeId,
                        Reason = dto.Reason,
                        ReassignmentType = "Bulk"
                    };

                    var log = await AssignLeadAsync(assignDto, assignedBy);
                    logs.Add(log);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other leads
                    Console.WriteLine($"Error assigning lead {leadId}: {ex.Message}");
                }
            }

            return logs;
        }

        public async Task<PagedInfo<LeadAssignmentLogDto>> GetAssignmentHistoryAsync(LeadAssignmentQueryDto query)
        {
            var exp = Expressionable.Create<BpLeadAssignmentLog>();

            if (query.LeadId.HasValue)
                exp = exp.And(it => it.LeadId == query.LeadId.Value);

            if (query.FromEmployeeId.HasValue)
                exp = exp.And(it => it.FromEmployeeId == query.FromEmployeeId.Value);

            if (query.ToEmployeeId.HasValue)
                exp = exp.And(it => it.ToEmployeeId == query.ToEmployeeId.Value);

            if (query.StartDate.HasValue)
                exp = exp.And(it => it.AssignmentDate >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                exp = exp.And(it => it.AssignmentDate <= query.EndDate.Value);

            if (!string.IsNullOrEmpty(query.ReassignmentType))
                exp = exp.And(it => it.ReassignmentType == query.ReassignmentType);

            var history = await Context.Queryable<BpLeadAssignmentLog, BpLead, HrEmployeeCore, HrPersonName, HrEmployeeCore, HrPersonName>(
                (log, lead, fromEmp, fromName, toEmp, toName) => new JoinQueryInfos(
                    JoinType.Left, log.LeadId == lead.LeadId,
                    JoinType.Left, log.FromEmployeeId == fromEmp.EmployeeId,
                    JoinType.Left, fromEmp.PersonId == fromName.PersonId && fromName.IsPrimary == true,
                    JoinType.Left, log.ToEmployeeId == toEmp.EmployeeId,
                    JoinType.Left, toEmp.PersonId == toName.PersonId && toName.IsPrimary == true
                ))
                .Where(exp.ToExpression())
                .OrderBy((log, lead, fromEmp, fromName, toEmp, toName) => log.AssignmentDate, OrderByType.Desc)
                .Select((log, lead, fromEmp, fromName, toEmp, toName) => new LeadAssignmentLogDto
                {
                    AssignmentLogId = log.AssignmentLogId,
                    LeadId = log.LeadId,
                    FromEmployeeId = log.FromEmployeeId,
                    ToEmployeeId = log.ToEmployeeId,
                    FromEmployeeName = fromName.GivenName + " " + fromName.FamilyName,
                    ToEmployeeName = toName.GivenName + " " + toName.FamilyName,
                    LeadName = lead.BrandName,
                    AssignmentDate = log.AssignmentDate,
                    Reason = log.Reason,
                    ReassignmentType = log.ReassignmentType
                })
                .ToPageAsync(query);

            return history;
        }
    }
}
```

**Interface:** `ZR.Service/Marketing/IMarketingService/IBpLeadAssignmentService.cs`
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Marketing.Dto;
using ZR.Model.Marketing.Leads;

namespace ZR.Service.Marketing.IMarketingService
{
    public interface IBpLeadAssignmentService
    {
        Task<BpLeadAssignmentLog> AssignLeadAsync(AssignLeadDto dto, string assignedBy);
        Task<List<BpLeadAssignmentLog>> BulkAssignLeadsAsync(BulkAssignLeadsDto dto, string assignedBy);
        Task<PagedInfo<LeadAssignmentLogDto>> GetAssignmentHistoryAsync(LeadAssignmentQueryDto query);
    }
}
```

---

## 🎯 Next Steps - Implementation Checklist

### Backend (Priority Order)

1. **Create Service Interfaces**
   - [ ] `IBpLeadActivityService.cs`
   - [ ] `IBpLeadAssignmentService.cs`
   - [ ] `IBpLeadMeetingService.cs`
   - [ ] `IMarketingPerformanceService.cs`

2. **Implement Service Classes** (See code above + continue with Meeting & Performance services)
   - [ ] `BpLeadActivityService.cs`
   - [ ] `BpLeadAssignmentService.cs`
   - [ ] `BpLeadMeetingService.cs` (with overlap detection)
   - [ ] `MarketingPerformanceService.cs`

3. **Create API Controllers**
   - [ ] Extend `BpLeadController.cs` with new endpoints
   - [ ] Create `BpLeadActivityController.cs`
   - [ ] Create `BpLeadMeetingController.cs`
   - [ ] Create `MarketingPerformanceController.cs`

4. **Run Database Migration**
   - [ ] Execute `Marketing_LeadManagement_DDL.sql` in PostgreSQL
   - [ ] Verify all tables and indexes created
   - [ ] Verify view created

5. **Build and Test Backend**
   - [ ] Build solution
   - [ ] Test via Swagger/Scalar
   - [ ] Verify business rules (meeting conflicts, etc.)

### Frontend (Priority Order)

1. **Create Employee Profile Page**
   - [ ] `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/marketing/team/[employeeId]/page.tsx`
   - [ ] Tab navigation structure

2. **Implement Overview Tab**
   - [ ] KPI cards component
   - [ ] API integration for performance metrics

3. **Implement Leads Tab**
   - [ ] Lead table with filters
   - [ ] Status update actions
   - [ ] Log activity modal
   - [ ] Reassign lead modal

4. **Implement Activities Tab**
   - [ ] Activity timeline component
   - [ ] Activity statistics cards

5. **Implement Meetings Tab**
   - [ ] Calendar component (day/week/month view)
   - [ ] Create meeting modal with conflict detection
   - [ ] Edit/Cancel meeting actions

6. **Implement Performance Report Tab**
   - [ ] Lead funnel chart
   - [ ] Status distribution pie chart
   - [ ] Activity type bar chart
   - [ ] Lost reasons chart

### Testing & Validation

- [ ] Test overlapping meeting prevention
- [ ] Test company-wide meeting conflicts
- [ ] Test employee double-booking prevention
- [ ] Test performance calculation accuracy
- [ ] Test bulk assignment
- [ ] Test activity logging updates lead dates
- [ ] Test calendar views (day/week/month)

---

## 📊 Key Business Rules to Implement

### Meeting Scheduling Rules (CRITICAL)

```csharp
// In BpLeadMeetingService.cs

public async Task<bool> CheckMeetingConflictAsync(long employeeId, DateTime start, DateTime end, long? excludeMeetingId = null)
{
    var conflictQuery = Context.Queryable<BpLeadMeeting>()
        .Where(m => m.AssignedEmployeeId == employeeId)
        .Where(m => m.MeetingStatus != "Cancelled" && m.MeetingStatus != "Completed")
        .Where(m => (m.StartDatetime < end && m.EndDatetime > start));

    if (excludeMeetingId.HasValue)
    {
        conflictQuery = conflictQuery.Where(m => m.MeetingId != excludeMeetingId.Value);
    }

    var conflicts = await conflictQuery.ToListAsync();
    return conflicts.Any();
}
```

### Lead Performance Calculation

```csharp
// In MarketingPerformanceService.cs

public async Task<MarketingEmployeePerformanceDto> CalculatePerformanceAsync(long employeeId, DateTime? startDate, DateTime? endDate)
{
    // Use the pre-built view for base metrics
    var baseMetrics = await Context.Queryable<VIEW_marketing_employee_performance>()
        .Where(v => v.employee_id == employeeId)
        .FirstAsync();

    // Apply date filters if needed
    var leadQuery = Context.Queryable<BpLead>()
        .Where(l => l.AssignedTo == employeeId);

    if (startDate.HasValue)
        leadQuery = leadQuery.Where(l => l.Create_time >= startDate.Value);

    if (endDate.HasValue)
        leadQuery = leadQuery.Where(l => l.Create_time <= endDate.Value);

    // Calculate metrics...
}
```

---

## 🗂️ File Structure Summary

```
Backend:
├── ZR.Database/Scripts/
│   └── Marketing_LeadManagement_DDL.sql ✅
├── ZR.Model/Marketing/
│   ├── Leads/
│   │   ├── BpLead.cs ✅
│   │   ├── BpLeadActivity.cs ✅
│   │   ├── BpLeadAssignmentLog.cs ✅
│   │   └── BpLeadMeeting.cs ✅
│   └── Dto/
│       ├── LeadDto.cs (existing)
│       └── LeadManagementDto.cs ✅
├── ZR.Repository/Marketing/
│   ├── BpLeadRepository.cs ✅
│   ├── BpLeadActivityRepository.cs ✅
│   ├── BpLeadAssignmentLogRepository.cs ✅
│   └── BpLeadMeetingRepository.cs ✅
├── ZR.Service/Marketing/
│   ├── Leads/
│   │   ├── BpLeadService.cs (existing)
│   │   ├── BpLeadActivityService.cs ⏳
│   │   ├── BpLeadAssignmentService.cs ⏳
│   │   └── BpLeadMeetingService.cs ⏳
│   ├── IMarketingService/
│   │   ├── IBpLeadService.cs (existing)
│   │   ├── IBpLeadActivityService.cs ⏳
│   │   ├── IBpLeadAssignmentService.cs ⏳
│   │   └── IBpLeadMeetingService.cs ⏳
│   └── MarketingPerformanceService.cs ⏳
└── ZR.Admin.WebApi/Controllers/Marketing/
    ├── BpLeadController.cs (existing, extend)
    ├── BpLeadActivityController.cs ⏳
    ├── BpLeadMeetingController.cs ⏳
    └── MarketingPerformanceController.cs ⏳

Frontend:
└── apps/isomorphic-intl/src/
    └── app/[locale]/(hydrogen)/marketing/
        └── team/
            ├── page.tsx (existing team list)
            └── [employeeId]/
                ├── page.tsx ⏳ (Profile with tabs)
                ├── components/
                │   ├── OverviewTab.tsx ⏳
                │   ├── LeadsTab.tsx ⏳
                │   ├── ActivitiesTab.tsx ⏳
                │   ├── MeetingsTab.tsx ⏳
                │   └── PerformanceTab.tsx ⏳
                └── types.ts ⏳

Legend: ✅ Complete | ⏳ Pending
```

---

## 🔐 Security & Permissions

Ensure these permissions are added to your permission system:
- `marketing:leads:list`
- `marketing:leads:view`
- `marketing:leads:add`
- `marketing:leads:edit`
- `marketing:leads:delete`
- `marketing:leads:assign`
- `marketing:activities:list`
- `marketing:activities:add`
- `marketing:meetings:list`
- `marketing:meetings:add`
- `marketing:meetings:edit`
- `marketing:meetings:cancel`
- `marketing:performance:view`

---

## 📝 Notes

1. **Database Migration**: Run the SQL script before building the backend
2. **Dependencies**: Ensure all using statements reference correct namespaces
3. **Testing**: Test each service independently before integration
4. **Frontend**: Use existing component patterns from the codebase
5. **Performance**: The view `v_marketing_employee_performance` is optimized for dashboards

---

**Status**: Phase 1 Complete | Phase 2 In Progress
**Last Updated**: 2025-12-27
