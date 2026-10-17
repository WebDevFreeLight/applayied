# ATTENDANCE MODULE - PHASED EXECUTION PLAN
## Enterprise-Grade Implementation with Rollback Strategy

**Date**: 2025-12-22
**Classification**: IMPLEMENTATION GUIDE
**Estimated Timeline**: 6-8 weeks (3 phases)
**Risk Level**: MEDIUM (mitigated by phased approach)

---

## EXECUTIVE SUMMARY

This document provides a **step-by-step execution plan** for implementing enterprise-grade enhancements to the Attendance Module while **minimizing risk** and **maintaining production stability**.

**Key Principles**:
- ✅ **Incremental** - Changes deployed in small, testable phases
- ✅ **Reversible** - Each phase has rollback script
- ✅ **Zero-Downtime** - Production system remains operational
- ✅ **Data-Safe** - Existing data preserved throughout migration
- ✅ **Audit-Ready** - Complete change log maintained

---

## PHASE 0: PREPARATION (Week 0)

### Objectives
- Set up development/staging environment
- Back up production database
- Establish testing framework
- Train team on new architecture

### Tasks

#### 0.1 Environment Setup
```bash
# Create dedicated migration branch
git checkout -b feature/attendance-enterprise-refactoring

# Verify database backup strategy
pg_dump mahamdb > backup_pre_attendance_migration_$(date +%Y%m%d).sql

# Set up staging environment (copy of production)
# - Same database version
# - Same data volume (or representative subset)
# - Same service configuration
```

#### 0.2 Code Review
```bash
# Review gap analysis document
cat docs/ATTENDANCE_ENTERPRISE_GAP_ANALYSIS.md

# Review migration scripts
cat ZR.Database/Scripts/Attendance_Phase1_Critical_Migrations.sql
cat ZR.Database/Scripts/Attendance_Phase2_HighPriority_Migrations.sql

# Review new models
ls ZR.Model/HR/Attendance/AttendanceProcessing/*.cs
```

#### 0.3 Team Training
- **Architecture Review**: 2-hour session on new snapshot/event architecture
- **Migration Walkthrough**: 1-hour hands-on with staging database
- **Rollback Drills**: Practice rollback procedures
- **Testing Strategy**: Define acceptance criteria for each phase

#### 0.4 Success Criteria
- [ ] Staging environment matches production
- [ ] Full database backup completed and verified (can restore)
- [ ] Team trained on new architecture
- [ ] Testing framework ready (unit + integration tests)

---

## PHASE 1: FOUNDATION (Week 1-2)

### Objectives
- Deploy database schema changes (non-breaking)
- Deploy new domain models
- Verify backward compatibility
- **NO** business logic changes yet

### Tasks

#### 1.1 Deploy Phase 1 Database Migrations (Critical)

**File**: `ZR.Database/Scripts/Attendance_Phase1_Critical_Migrations.sql`

**Contains**:
- New tables: `hr_attendance_day_snapshot`, `hr_attendance_event`
- Policy versioning columns
- Finalization protection triggers
- Audit log enhancements

**Deployment**:
```bash
# On STAGING first
psql -h staging-db -U app_user -d mahamdb \
  -f ZR.Database/Scripts/Attendance_Phase1_Critical_Migrations.sql \
  > phase1_staging_deploy.log 2>&1

# Verify
psql -h staging-db -U app_user -d mahamdb <<EOF
SELECT COUNT(*) FROM hr_attendance_day_snapshot;  -- Should be 0 (empty)
SELECT COUNT(*) FROM hr_attendance_event;         -- Should be 0 (empty)
SELECT column_name FROM information_schema.columns
WHERE table_name = 'hr_attendance_policy'
AND column_name IN ('effective_from', 'effective_to', 'policy_version');
-- Should return 3 rows
EOF
```

**Rollback** (if issues found):
```bash
# Use rollback script at end of Phase1 migration file
psql -h staging-db -U app_user -d mahamdb <<'EOF'
-- (See rollback section in migration file)
DROP TRIGGER IF EXISTS trg_prevent_finalized_snapshot_modification ...
EOF
```

#### 1.2 Deploy New Domain Models

**Files Created**:
- `ZR.Model/HR/Attendance/AttendanceProcessing/HrAttendanceDaySnapshot.cs`
- `ZR.Model/HR/Attendance/AttendanceProcessing/HrAttendanceEvent.cs`
- `ZR.Model/HR/Attendance/Overtime/HrOvertimeSegmentVerification.cs`

**Deployment**:
```bash
# Build solution to verify models compile
dotnet build ZRAdminMahamBackend.sln

# Expected: Build successful (new tables exist but unused yet)
```

**Verification**:
- [ ] Solution builds without errors
- [ ] New models visible in IDE
- [ ] **NO** changes to existing services yet (backward compatible)

#### 1.3 Smoke Test Existing Functionality

**Critical Test Cases**:
```csharp
// Test 1: Existing attendance processing still works
var result = await AttendanceProcessingService.ProcessAttendanceForDate(
    employeeId: 123,
    date: DateTime.Today
);
Assert.IsNotNull(result); // HrAttendanceDay created

// Test 2: Existing payroll summary generation still works
var summary = await PayrollSummaryService.GenerateSummaryForEmployee(
    employeeId: 123,
    payrollPeriod: "2025-01"
);
Assert.IsNotNull(summary);
Assert.True(summary.TotalWorkedMinutes > 0);

// Test 3: Finalization protection trigger works
var existingSummary = await GetSummary(123, "2025-01");
existingSummary.IsFinalized = true;
await UpdateSummary(existingSummary);

// Try to modify finalized summary
existingSummary.TotalWorkedMinutes = 999;
var ex = await Assert.ThrowsAsync<Exception>(() => UpdateSummary(existingSummary));
Assert.Contains("Cannot modify finalized", ex.Message);
```

#### 1.4 Production Deployment (Phase 1)

**Pre-Deployment Checklist**:
- [ ] All staging tests passed
- [ ] Database backup completed (production)
- [ ] Rollback script tested on staging
- [ ] Deployment window scheduled (low-traffic period)
- [ ] Team on standby for rollback

**Deployment Window**: Saturday 2:00 AM - 4:00 AM (low traffic)

```bash
# 1. Backup production database
pg_dump -h prod-db -U app_user mahamdb > \
  backup_pre_phase1_$(date +%Y%m%d_%H%M%S).sql

# 2. Run migration
psql -h prod-db -U app_user -d mahamdb \
  -f ZR.Database/Scripts/Attendance_Phase1_Critical_Migrations.sql

# 3. Verify
psql -h prod-db -U app_user -d mahamdb -c \
  "SELECT trigger_name FROM information_schema.triggers
   WHERE trigger_name LIKE '%finalized%';"
# Expected: 2 rows (snapshot + summary triggers)

# 4. Deploy code (new models)
dotnet publish ZR.Admin.WebApi -c Release
# Deploy to production servers
# (Use your existing deployment pipeline)

# 5. Restart services
systemctl restart zradmin-api

# 6. Monitor logs
tail -f /var/log/zradmin/api.log
# Expected: NO errors, system starts normally
```

**Post-Deployment Monitoring** (First 48 hours):
- Monitor error logs for exceptions
- Monitor database performance (new triggers)
- Monitor API response times
- Test critical user flows (attendance processing, payroll)

#### 1.5 Success Criteria (Phase 1)
- [ ] All migrations deployed successfully
- [ ] All triggers functional
- [ ] **ZERO** errors in production logs
- [ ] All existing features work as before
- [ ] New tables created (empty)
- [ ] Team confirms backward compatibility

**Rollback Trigger**: Any production errors or data corruption

---

## PHASE 2: SNAPSHOT INTEGRATION (Week 3-4)

### Objectives
- Implement snapshot creation logic
- Implement event emission logic
- **Parallel Run**: Create snapshots alongside existing data
- Verify snapshot accuracy vs live data

### Tasks

#### 2.1 Deploy Phase 2 Database Migrations

**File**: `ZR.Database/Scripts/Attendance_Phase2_HighPriority_Migrations.sql`

**Contains**:
- Attendance log hardening (device payload, timezone, immutability)
- Overtime verification table
- Leave/attendance decoupling enhancements
- Multi-timezone support
- Weekend configuration table

**Deployment** (same process as Phase 1):
```bash
# Staging first
psql -h staging-db -U app_user -d mahamdb \
  -f ZR.Database/Scripts/Attendance_Phase2_HighPriority_Migrations.sql

# Verify overtime verification table
psql -h staging-db -U app_user -d mahamdb -c \
  "SELECT COUNT(*) FROM hr_overtime_segment_verification;"
# Expected: 0 (empty)
```

#### 2.2 Implement Snapshot Creation Service

**New Service**: `SnapshotService.cs` (in `ZR.Service/HR/Attendance/`)

```csharp
/// <summary>
/// Snapshot Service - Creates immutable attendance snapshots
/// </summary>
public class SnapshotService : ISnapshotService
{
    public async Task<HrAttendanceDaySnapshot> CreateSnapshotFromAttendanceDay(
        HrAttendanceDay attendanceDay,
        string reason,
        string createdBy)
    {
        // Get latest snapshot version for this employee/date
        var latestVersion = await GetLatestSnapshotVersion(
            attendanceDay.EmployeeId,
            attendanceDay.AttendanceDate
        );

        var snapshot = new HrAttendanceDaySnapshot
        {
            EmployeeId = attendanceDay.EmployeeId,
            AttendanceDate = attendanceDay.AttendanceDate,
            SnapshotVersion = latestVersion + 1,
            SnapshotReason = reason,
            CreatedBy = createdBy,

            // Copy all calculated values
            ExpectedMinutes = attendanceDay.ExpectedMinutes,
            WorkedMinutes = attendanceDay.WorkedMinutes,
            LateMinutes = attendanceDay.LateMinutes,
            EarlyLeaveMinutes = attendanceDay.EarlyLeaveMinutes,
            OvertimeMinutes = attendanceDay.OvertimeMinutes,
            LeaveMinutes = attendanceDay.LeaveMinutes ?? 0,

            // Copy flags
            IsAbsent = attendanceDay.IsAbsent,
            IsWeekend = attendanceDay.IsWeekend,
            IsHoliday = attendanceDay.IsHoliday,

            // Capture policy snapshot (for reproducibility)
            PolicyId = attendanceDay.PolicyId,
            PolicySnapshotJson = await CapturePolicy SnapshotJson(attendanceDay.PolicyId),

            // Capture shift snapshot
            ShiftId = attendanceDay.ShiftId,
            ShiftSnapshotJson = await CaptureShiftSnapshotJson(attendanceDay.ShiftId),

            // Metadata
            CalculationEngineVersion = "v1.0.0", // From config/version file
            CalculationTimestamp = DateTime.UtcNow,

            // NOT finalized yet (finalization happens during payroll)
            IsFinalized = false
        };

        await _snapshotRepo.InsertAsync(snapshot);
        return snapshot;
    }
}
```

#### 2.3 Update Attendance Processing Service (Snapshot Creation)

**File**: `ZR.Service/HR/Attendance/AttendanceProcessingService.cs`

**Changes**:
```csharp
public async Task<HrAttendanceDay> ProcessAttendanceForDate(
    long employeeId, DateTime date)
{
    // ... existing processing logic ...
    var attendanceDay = await ProcessEmployeeAttendanceForDate(employeeId, date);

    // ✅ NEW: Create snapshot after processing
    var snapshot = await _snapshotService.CreateSnapshotFromAttendanceDay(
        attendanceDay,
        reason: "InitialProcessing",
        createdBy: "AttendanceProcessingService"
    );

    _logger.LogInformation(
        "Created attendance snapshot {SnapshotId} v{Version} for employee {EmployeeId} date {Date}",
        snapshot.SnapshotId, snapshot.SnapshotVersion, employeeId, date
    );

    return attendanceDay;
}
```

**Verification Test**:
```csharp
[Fact]
public async Task ProcessAttendance_CreatesSnapshot()
{
    // Arrange
    var employeeId = 123;
    var date = DateTime.Today;

    // Act
    var result = await _attendanceProcessingService.ProcessAttendanceForDate(
        employeeId, date);

    // Assert: Attendance day created
    Assert.NotNull(result);

    // Assert: Snapshot created
    var snapshot = await _snapshotRepo.GetLatestSnapshot(employeeId, date);
    Assert.NotNull(snapshot);
    Assert.Equal(1, snapshot.SnapshotVersion); // First version
    Assert.Equal(result.WorkedMinutes, snapshot.WorkedMinutes);
    Assert.False(snapshot.IsFinalized); // Not finalized yet
}
```

#### 2.4 Implement Event Emission

**New Service**: `EventService.cs` (in `ZR.Service/HR/Attendance/`)

```csharp
public class EventService : IEventService
{
    public async Task<HrAttendanceEvent> EmitEvent(
        string eventType,
        string eventSource,
        string triggeredBy,
        object eventPayload,
        long? employeeId = null,
        DateTime? affectedDateFrom = null,
        DateTime? affectedDateTo = null)
    {
        var eventPayloadJson = JsonSerializer.Serialize(eventPayload);
        var eventPayloadHash = ComputeSha256Hash(eventPayloadJson);

        var attendanceEvent = new HrAttendanceEvent
        {
            EventType = eventType,
            EventSource = eventSource,
            TriggeredBy = triggeredBy,
            EmployeeId = employeeId,
            AffectedDateFrom = affectedDateFrom,
            AffectedDateTo = affectedDateTo,
            EventPayloadJson = eventPayloadJson,
            EventPayloadHash = eventPayloadHash,
            ProcessingStatus = "Pending",
            EventPriority = DetermineEventPriority(eventType),
            IdempotencyKey = GenerateIdempotencyKey(eventType, eventPayload)
        };

        await _eventRepo.InsertAsync(attendanceEvent);

        _logger.LogInformation(
            "Emitted event {EventId} type {EventType} for employee {EmployeeId}",
            attendanceEvent.EventId, eventType, employeeId
        );

        return attendanceEvent;
    }
}
```

#### 2.5 Update Leave Service (Event Emission)

**File**: `ZR.Service/HR/Leave/LeaveManagementService.cs`

```csharp
public async Task<bool> ApproveLeaveRequest(
    long leaveRequestId, long approvedBy, string comments)
{
    var leaveRequest = await _leaveRequestRepo.GetByIdAsync(leaveRequestId);
    if (leaveRequest == null) return false;

    // ... existing approval logic ...
    leaveRequest.Status = "Approved";
    await _leaveRequestRepo.UpdateAsync(leaveRequest);

    // ✅ NEW: Emit event
    var eventPayload = new LeaveApprovedEventPayload
    {
        LeaveRequestId = leaveRequestId,
        EmployeeId = leaveRequest.EmployeeId,
        LeaveTypeId = leaveRequest.LeaveTypeId,
        FromDate = leaveRequest.FromDate,
        ToDate = leaveRequest.ToDate,
        TotalDays = leaveRequest.Days,
        ApprovedBy = approvedBy.ToString(),
        ApprovalTimestamp = DateTime.UtcNow
    };

    await _eventService.EmitEvent(
        eventType: "LeaveApproved",
        eventSource: "LeaveManagementService",
        triggeredBy: approvedBy.ToString(),
        eventPayload: eventPayload,
        employeeId: leaveRequest.EmployeeId,
        affectedDateFrom: leaveRequest.FromDate,
        affectedDateTo: leaveRequest.ToDate
    );

    return true;
}
```

#### 2.6 Parallel Run & Validation

**Goal**: Verify snapshot data matches live data

**Validation Script**:
```sql
-- Compare attendance day vs snapshot
SELECT
    ad.employee_id,
    ad.attendance_date,
    ad.worked_minutes AS live_worked,
    snap.worked_minutes AS snapshot_worked,
    CASE
        WHEN ad.worked_minutes = snap.worked_minutes THEN 'MATCH'
        ELSE 'MISMATCH'
    END AS status
FROM hr_attendance_day ad
LEFT JOIN hr_attendance_day_snapshot snap
    ON ad.employee_id = snap.employee_id
    AND ad.attendance_date = snap.attendance_date
    AND snap.snapshot_version = (
        SELECT MAX(snapshot_version)
        FROM hr_attendance_day_snapshot s2
        WHERE s2.employee_id = ad.employee_id
        AND s2.attendance_date = ad.attendance_date
    )
WHERE ad.attendance_date >= CURRENT_DATE - INTERVAL '30 days'
ORDER BY ad.employee_id, ad.attendance_date;

-- Expected: All rows show 'MATCH'
```

#### 2.7 Production Deployment (Phase 2)

**Same deployment process as Phase 1**:
- Deploy migrations
- Deploy code changes
- Monitor for 48 hours
- Run validation scripts daily

#### 2.8 Success Criteria (Phase 2)
- [ ] Snapshots created for all new attendance processing
- [ ] Events emitted for all leave approvals/cancellations
- [ ] Snapshot data matches live data (100% accuracy)
- [ ] **ZERO** errors in production
- [ ] Performance acceptable (snapshot creation < 100ms)

**Rollback Trigger**: Data mismatches or performance degradation

---

## PHASE 3: CUTOVER TO SNAPSHOTS (Week 5-6)

### Objectives
- Switch payroll to read from snapshots (not live data)
- Implement finalization workflow
- Implement event processing service
- **CRITICAL**: Verify payroll calculations unchanged

### Tasks

#### 3.1 Update Payroll Summary Service (Read from Snapshots)

**File**: `ZR.Service/HR/Attendance/PayrollAttendanceSummaryService.cs`

**BEFORE** (reads from live tables):
```csharp
public async Task<HrPayrollAttendanceSummary> GenerateSummaryForEmployee(
    long employeeId, string payrollPeriod)
{
    // OLD: Read from hr_attendance_day
    var attendanceDays = await _attendanceDayRepo.GetByPayrollPeriod(
        employeeId, payrollPeriod);

    var summary = new HrPayrollAttendanceSummary
    {
        TotalWorkedMinutes = attendanceDays.Sum(d => d.WorkedMinutes),
        // ...
    };
}
```

**AFTER** (reads from snapshots):
```csharp
public async Task<HrPayrollAttendanceSummary> GenerateSummaryForEmployee(
    long employeeId, string payrollPeriod)
{
    // ✅ NEW: Read from snapshots
    var snapshots = await _snapshotRepo.GetLatestSnapshotsForPeriod(
        employeeId, payrollPeriod);

    var summary = new HrPayrollAttendanceSummary
    {
        TotalWorkedMinutes = snapshots.Sum(s => s.WorkedMinutes),
        // ✅ NEW: Track which snapshot versions used
        SnapshotVersionUsed = $"v{snapshots.Min(s => s.SnapshotVersion)}-v{snapshots.Max(s => s.SnapshotVersion)}",
        CalculationEngineVersion = "v1.0.0",
        // ...
    };

    return summary;
}
```

**Verification Test** (CRITICAL):
```csharp
[Fact]
public async Task PayrollSummary_SnapshotVsLive_MatchesExactly()
{
    // Arrange
    var employeeId = 123;
    var payrollPeriod = "2025-01";

    // Generate summary from LIVE data (old way)
    var summaryFromLive = await GenerateSummaryFromLiveData(employeeId, payrollPeriod);

    // Generate summary from SNAPSHOTS (new way)
    var summaryFromSnapshots = await GenerateSummaryFromSnapshots(employeeId, payrollPeriod);

    // Assert: MUST match exactly
    Assert.Equal(summaryFromLive.TotalWorkedMinutes, summaryFromSnapshots.TotalWorkedMinutes);
    Assert.Equal(summaryFromLive.TotalLateMinutes, summaryFromSnapshots.TotalLateMinutes);
    Assert.Equal(summaryFromLive.TotalOvertimeMinutes, summaryFromSnapshots.TotalOvertimeMinutes);
    Assert.Equal(summaryFromLive.CalculatedSalaryFactor, summaryFromSnapshots.CalculatedSalaryFactor);

    // If ANY values differ → STOP DEPLOYMENT
}
```

#### 3.2 Implement Finalization Workflow

**New Method**: `PayrollAttendanceSummaryService.FinalizeSummaryWithSnapshots()`

```csharp
public async Task<bool> FinalizeSummaryWithSnapshots(
    long employeeId, string payrollPeriod, string finalizedBy)
{
    // Get payroll summary
    var summary = await GetSummaryByEmployeeAndPeriod(employeeId, payrollPeriod);
    if (summary == null || summary.IsFinalized) return false;

    // Get all snapshots used for this summary
    var snapshots = await _snapshotRepo.GetLatestSnapshotsForPeriod(
        employeeId, payrollPeriod);

    // ✅ CRITICAL: Finalize all snapshots FIRST
    foreach (var snapshot in snapshots)
    {
        snapshot.IsFinalized = true;
        snapshot.FinalizedAt = DateTime.UtcNow;
        snapshot.FinalizedBy = finalizedBy;
        snapshot.FinalizedForPayrollPeriod = payrollPeriod;
        await _snapshotRepo.UpdateAsync(snapshot);
    }

    // Then finalize summary
    summary.IsFinalized = true;
    summary.FinalizedDate = DateTime.UtcNow;
    summary.FinalizedBy = finalizedBy;
    summary.FinalizationCount++;
    summary.SnapshotVersionUsed = GetSnapshotVersionRange(snapshots);
    await _summaryRepo.UpdateAsync(summary);

    _logger.LogWarning(
        "Finalized payroll summary {SummaryId} and {Count} snapshots for employee {EmployeeId} period {Period}",
        summary.SummaryId, snapshots.Count, employeeId, payrollPeriod
    );

    return true;
}
```

#### 3.3 Implement Event Processing Service (Background Worker)

**New Service**: `EventProcessingService.cs`

```csharp
/// <summary>
/// Background service that processes pending attendance events
/// </summary>
public class EventProcessingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Poll for pending events (ordered by priority)
                var pendingEvents = await _eventRepo.GetPendingEvents(limit: 100);

                foreach (var attendanceEvent in pendingEvents)
                {
                    await ProcessEvent(attendanceEvent);
                }

                // Sleep before next poll (e.g., 30 seconds)
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing attendance events");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task ProcessEvent(HrAttendanceEvent attendanceEvent)
    {
        try
        {
            attendanceEvent.MarkAsProcessing();
            await _eventRepo.UpdateAsync(attendanceEvent);

            // Route to appropriate handler based on event type
            switch (attendanceEvent.EventType)
            {
                case "LeaveApproved":
                    await HandleLeaveApproved(attendanceEvent);
                    break;
                case "LeaveCancelled":
                    await HandleLeaveCancelled(attendanceEvent);
                    break;
                case "AttendanceAdjustmentApproved":
                    await HandleAttendanceAdjustment(attendanceEvent);
                    break;
                // ... other event types
            }

            attendanceEvent.MarkAsProcessed();
            await _eventRepo.UpdateAsync(attendanceEvent);
        }
        catch (Exception ex)
        {
            attendanceEvent.MarkAsFailed(ex.Message);
            await _eventRepo.UpdateAsync(attendanceEvent);

            // Schedule retry with exponential backoff
            if (attendanceEvent.ShouldRetry())
            {
                attendanceEvent.ScheduledFor = DateTime.UtcNow.Add(attendanceEvent.GetRetryDelay());
                attendanceEvent.ProcessingStatus = "Pending";
                await _eventRepo.UpdateAsync(attendanceEvent);
            }
        }
    }
}
```

#### 3.4 Validation & Testing (CRITICAL)

**Test Scenarios**:

1. **Scenario: Full Month Payroll Processing**
   - Process attendance for entire month (30 days, 100 employees)
   - Generate payroll summaries from snapshots
   - Finalize summaries
   - Verify finalized snapshots are immutable
   - **Expected**: All calculations correct, no errors

2. **Scenario: Recalculation After Finalization**
   - Finalize payroll summary
   - Attempt to recalculate
   - **Expected**: Exception thrown (cannot modify finalized)

3. **Scenario: Leave Approval Triggers Recalculation**
   - Approve leave request
   - Event emitted
   - Event processor picks up event
   - Attendance recalculated
   - New snapshot version created
   - **Expected**: Snapshot v2 created with leave minutes

4. **Scenario: Policy Change Event**
   - Change attendance policy (e.g., late grace period 30→15 min)
   - Event emitted
   - Affected dates recalculated
   - **Expected**: New snapshots reference new policy version

#### 3.5 Production Deployment (Phase 3) - STAGED

**Week 5**: Deploy to Production (Shadow Mode)
- Event processing service running
- Snapshots created
- **BUT**: Payroll still reads from live data (safety net)

**Week 6**: Cutover to Snapshots
- Payroll switches to reading snapshots
- Monitor for 1 full payroll cycle
- Rollback plan ready

**Deployment Steps**:
```bash
# 1. Deploy event processing service (shadow mode)
# Update Program.cs
builder.Services.AddHostedService<EventProcessingService>();

# 2. Deploy to production
dotnet publish ZR.Admin.WebApi -c Release

# 3. Monitor event processing
# Check logs for event processing activity
tail -f /var/log/zradmin/api.log | grep "Processed event"

# 4. After 1 week of shadow mode, enable snapshot reading
# Update appsettings.json
{
  "AttendanceModule": {
    "UseSnapshotsForPayroll": true  // ✅ Enable
  }
}

# 5. Monitor payroll runs closely
# Compare payroll amounts week-over-week
# Any discrepancies → investigate immediately
```

#### 3.6 Success Criteria (Phase 3)
- [ ] Payroll summaries from snapshots match live data (100%)
- [ ] Finalization workflow functional
- [ ] Event processing service running 24/7
- [ ] All events processed within 5 minutes
- [ ] **ZERO** payroll calculation errors
- [ ] Team comfortable with new system

**Rollback Trigger**: Any payroll calculation discrepancies

---

## PHASE 4: OPTIMIZATION & CLEANUP (Week 7-8)

### Objectives
- Performance tuning
- Add monitoring/alerting
- Clean up old code
- Documentation

### Tasks

#### 4.1 Performance Optimization
- Add database indexes for common queries
- Implement caching for policy snapshots
- Optimize event processing (batch updates)

#### 4.2 Monitoring & Alerting
- Set up alerts for:
  - Event processing lag (> 10 minutes)
  - Failed events (retry count > 3)
  - Snapshot creation failures
  - Finalization errors

#### 4.3 Code Cleanup
- Remove old code paths (after 2 successful payroll cycles)
- Archive migration scripts
- Update API documentation

#### 4.4 Team Handoff
- Final training session
- Runbook for operations team
- Troubleshooting guide

---

## ROLLBACK STRATEGIES

### General Rollback Principles
1. **Always** test rollback on staging first
2. **Never** rollback during payroll processing window
3. **Backup** database before rollback
4. **Communicate** rollback to stakeholders

### Phase-Specific Rollback

#### Rollback Phase 1 (Foundation)
```bash
# Run rollback script
psql -h prod-db -U app_user -d mahamdb <<'EOF'
DROP TRIGGER IF EXISTS trg_prevent_finalized_snapshot_modification ON hr_attendance_day_snapshot;
DROP TRIGGER IF EXISTS trg_prevent_finalized_summary_modification ON hr_payroll_attendance_summary;
DROP TABLE IF EXISTS hr_attendance_day_snapshot CASCADE;
DROP TABLE IF EXISTS hr_attendance_event CASCADE;
-- (See full rollback script in migration file)
EOF

# Redeploy previous version
git checkout main
dotnet publish ZR.Admin.WebApi -c Release
# Deploy to production
```

#### Rollback Phase 2 (Snapshot Integration)
```bash
# Disable snapshot creation
# Update appsettings.json
{
  "AttendanceModule": {
    "CreateSnapshots": false  // Disable
  }
}

# Restart services
systemctl restart zradmin-api

# Monitor: Existing data still accessible
```

#### Rollback Phase 3 (Cutover)
```bash
# Revert payroll to read from live data
# Update appsettings.json
{
  "AttendanceModule": {
    "UseSnapshotsForPayroll": false  // Revert to live data
  }
}

# Stop event processing service
# Update Program.cs (comment out)
// builder.Services.AddHostedService<EventProcessingService>();

# Redeploy
```

---

## RISK MITIGATION

### Risk: Data Corruption
**Mitigation**:
- Daily database backups
- Point-in-time recovery enabled
- Validation scripts run after each phase
- Parallel run (snapshots + live data)

### Risk: Performance Degradation
**Mitigation**:
- Load testing on staging
- Database query plan analysis
- Index optimization
- Caching strategy

### Risk: Payroll Calculation Errors
**Mitigation**:
- Side-by-side validation (snapshot vs live)
- Test with real production data (anonymized)
- Manual review of first 2 payroll runs
- Rollback plan ready

### Risk: Team Resistance
**Mitigation**:
- Early training and involvement
- Clear documentation
- Runbooks for common scenarios
- Support channel (Slack/Teams)

---

## SUCCESS METRICS

### Technical Metrics
- **Uptime**: 99.9% during migration
- **Data Accuracy**: 100% (snapshot vs live)
- **Performance**: < 10% degradation
- **Event Processing**: < 5 min average lag

### Business Metrics
- **Payroll Accuracy**: Zero calculation errors
- **Audit Readiness**: Pass mock audit
- **Team Confidence**: > 80% comfortable with new system
- **Documentation**: Complete and reviewed

---

## POST-DEPLOYMENT

### Week 8+: Stabilization
- Monitor for 1 full month
- Collect user feedback
- Address any issues
- Document lessons learned

### Month 3: Audit Readiness Review
- Internal audit of new system
- Verify compliance with requirements
- Demonstrate snapshot reproducibility
- Test rollback procedures

### Month 6: Optimization Review
- Performance analysis
- Identify bottlenecks
- Plan for scale (100k+ employees)
- Consider advanced features (ML-based anomaly detection)

---

## COMMUNICATION PLAN

### Stakeholder Communication

**Week 0 (Planning)**:
- Email to all stakeholders: "Attendance Module Enhancement Project Kick-Off"
- Include: Timeline, benefits, risks, team contacts

**Week 1-2 (Phase 1)**:
- Daily status updates to project team
- Weekly status email to stakeholders
- Demo session: Show new snapshot architecture

**Week 3-4 (Phase 2)**:
- Progress update: "Snapshots Now Being Created"
- Training session for HR team
- FAQ document published

**Week 5-6 (Phase 3)**:
- **CRITICAL** announcement: "Payroll Switching to Snapshot System"
- Detailed communication to finance/HR
- Extra support during first payroll run

**Week 7-8 (Cleanup)**:
- Project completion announcement
- Success metrics shared
- Thank you to team

---

## CONCLUSION

This phased execution plan provides a **safe, incremental path** to enterprise-grade attendance architecture while **minimizing risk** and **maintaining production stability**.

**Key Success Factors**:
1. ✅ **Incremental** - Small, testable changes
2. ✅ **Reversible** - Rollback at any phase
3. ✅ **Validated** - Parallel run + verification
4. ✅ **Monitored** - 24/7 monitoring during migration
5. ✅ **Supported** - Team training and documentation

**Estimated Total Effort**:
- Development: 160 hours (4 weeks × 40 hours)
- Testing: 80 hours (2 weeks × 40 hours)
- Deployment & Monitoring: 40 hours (1 week × 40 hours)
- **Total**: ~280 hours (7 weeks)

**ROI**:
- **Audit Compliance**: Pass financial audits
- **Legal Protection**: Provable attendance records
- **Payroll Accuracy**: Eliminate calculation disputes
- **Regulatory Compliance**: Multi-country support
- **Scalability**: Support 100k+ employees

**Next Steps**:
1. Review this plan with stakeholders
2. Get approval for Phase 0 (Preparation)
3. Set up staging environment
4. Begin Phase 1 (Foundation)

---

**Document Version**: 1.0
**Classification**: INTERNAL - IMPLEMENTATION GUIDE
**Last Updated**: 2025-12-22
