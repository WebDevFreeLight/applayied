using SqlSugar;
using System;
using System.Collections.Generic;

namespace ZR.Model.HR.Attendance.AttendanceProcessing
{
    /// <summary>
    /// Attendance Event Log - Event sourcing for attendance changes
    /// Purpose: Complete audit trail of WHY data changed, WHO triggered it, WHEN it happened
    /// Replaces: Direct service-to-service calls with no audit trail
    /// </summary>
    /// <remarks>
    /// CRITICAL BUSINESS RULES:
    /// 1. ALL attendance changes MUST emit events (no silent modifications)
    /// 2. Events are IMMUTABLE after creation (append-only log)
    /// 3. Event processing is IDEMPOTENT (safe to replay)
    /// 4. Events capture complete causality chain (source record → event → snapshot)
    /// 5. Failed events are retried with exponential backoff
    /// </remarks>
    [SugarTable("hr_attendance_event")]
    public class HrAttendanceEvent
    {
        /// <summary>
        /// Event Primary Key
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long EventId { get; set; }

        #region Event Metadata

        /// <summary>
        /// Type of event that occurred
        /// </summary>
        /// <remarks>
        /// Valid values:
        /// - LeaveApproved: Leave request approved
        /// - LeaveCancelled: Leave request cancelled
        /// - LeaveModified: Leave request modified (dates/duration changed)
        /// - AttendanceAdjustmentApproved: Manual adjustment approved
        /// - AttendanceAdjustmentRejected: Manual adjustment rejected
        /// - PolicyChanged: Attendance policy updated
        /// - PolicyActivated: New policy version activated
        /// - PolicyDeactivated: Policy version deactivated
        /// - ShiftReassigned: Employee assigned to different shift
        /// - ShiftModified: Shift configuration changed
        /// - HolidayAdded: Holiday added to calendar
        /// - HolidayRemoved: Holiday removed from calendar
        /// - ManualRecalculation: User-triggered recalculation
        /// - BulkRecalculation: Batch recalculation job
        /// - ScheduledProcessing: Daily scheduled processing
        /// - PeriodFinalized: Payroll period finalized
        /// - PeriodUnfinalized: Payroll period reopened
        /// - OvertimeApproved: Overtime request approved
        /// - OvertimeRejected: Overtime request rejected
        /// </remarks>
        [SugarColumn(IsNullable = false, Length = 50)]
        public string EventType { get; set; }

        /// <summary>
        /// Source of the event
        /// Examples: "API", "ScheduledJob", "ManualRecalculation", "BackgroundWorker", "MobileApp"
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 100)]
        public string EventSource { get; set; }

        /// <summary>
        /// When event occurred (UTC)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// REAL USER who triggered the event (not service account)
        /// CRITICAL: For accountability and audit compliance
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 100)]
        public string TriggeredBy { get; set; }

        #endregion

        #region Event Scope (which data is affected)

        /// <summary>
        /// Employee affected by this event (null for bulk operations)
        /// </summary>
        public long? EmployeeId { get; set; }

        /// <summary>
        /// First date affected by this event
        /// </summary>
        public DateTime? AffectedDateFrom { get; set; }

        /// <summary>
        /// Last date affected by this event
        /// </summary>
        public DateTime? AffectedDateTo { get; set; }

        /// <summary>
        /// Department affected (for bulk operations)
        /// </summary>
        public long? DepartmentId { get; set; }

        #endregion

        #region Event Source (what caused this event)

        /// <summary>
        /// Source table name (e.g., "hr_leave_request", "hr_attendance_adjustment")
        /// Enables tracing back to originating record
        /// </summary>
        [SugarColumn(Length = 100)]
        public string SourceTable { get; set; }

        /// <summary>
        /// Source record ID (primary key of source table)
        /// </summary>
        public long? SourceRecordId { get; set; }

        /// <summary>
        /// Snapshot of source record (complete data at event time)
        /// CRITICAL: Allows reproducing event even if source record deleted/modified
        /// </summary>
        [SugarColumn(ColumnDataType = "jsonb")]
        public string SourceRecordSnapshot { get; set; }

        #endregion

        #region Event Payload

        /// <summary>
        /// Complete event data as JSON
        /// CRITICAL: Contains all information needed to process/replay event
        /// </summary>
        /// <remarks>
        /// Example for LeaveApproved:
        /// {
        ///   "leave_request_id": 12345,
        ///   "employee_id": 678,
        ///   "leave_type_id": 2,
        ///   "from_date": "2025-02-15",
        ///   "to_date": "2025-02-17",
        ///   "total_days": 3,
        ///   "approved_by": "manager_user_123",
        ///   "approval_timestamp": "2025-02-10T14:30:00Z"
        /// }
        /// </remarks>
        [SugarColumn(IsNullable = false, ColumnDataType = "jsonb")]
        public string EventPayloadJson { get; set; }

        /// <summary>
        /// SHA256 hash of event payload (for integrity verification)
        /// Detects if event payload was tampered with
        /// </summary>
        [SugarColumn(Length = 64)]
        public string EventPayloadHash { get; set; }

        #endregion

        #region Processing Status

        /// <summary>
        /// Current processing status
        /// Values: Pending, Processing, Processed, Failed, Cancelled, Duplicate
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 20)]
        public string ProcessingStatus { get; set; } = "Pending";

        /// <summary>
        /// When event processing completed (UTC)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// When event processing started (UTC)
        /// Used to detect stuck/zombie processors
        /// </summary>
        public DateTime? ProcessingStartedAt { get; set; }

        /// <summary>
        /// Error message if processing failed
        /// </summary>
        [SugarColumn(ColumnDataType = "text")]
        public string ProcessingError { get; set; }

        /// <summary>
        /// Number of times processing was retried
        /// Used for exponential backoff
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int ProcessingRetryCount { get; set; }

        #endregion

        #region Processing Results

        /// <summary>
        /// TRUE if event triggered attendance recalculation
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool RecalculationTriggered { get; set; }

        /// <summary>
        /// Array of snapshot IDs created as result of this event
        /// Links event → snapshots for complete audit trail
        /// </summary>
        [SugarColumn(ColumnDataType = "bigint[]")]
        public long[] AffectedSnapshotIds { get; set; }

        /// <summary>
        /// Array of employee IDs affected (for bulk operations)
        /// </summary>
        [SugarColumn(ColumnDataType = "bigint[]")]
        public long[] AffectedEmployeeIds { get; set; }

        #endregion

        #region Priority & Scheduling

        /// <summary>
        /// Event priority (1=urgent, 5=normal, 10=low)
        /// Determines processing order
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int EventPriority { get; set; } = 5;

        /// <summary>
        /// When event should be processed (for delayed processing)
        /// null = process immediately
        /// </summary>
        public DateTime? ScheduledFor { get; set; }

        #endregion

        #region Idempotency (prevent duplicate processing)

        /// <summary>
        /// Unique key to prevent duplicate event processing
        /// Example: "leave_approved_12345", "adjustment_applied_67890"
        /// CRITICAL: Enables safe event replay without duplication
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true)]
        public string IdempotencyKey { get; set; }

        /// <summary>
        /// If this event is a duplicate, links to original event
        /// </summary>
        public long? DuplicateOfEventId { get; set; }

        #endregion

        #region Audit

        /// <summary>
        /// When event was created in database (UTC)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Navigation Properties (not mapped)

        /// <summary>
        /// Navigation: Snapshots created by this event
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<HrAttendanceDaySnapshot> CreatedSnapshots { get; set; }

        /// <summary>
        /// Navigation: Original event (if this is a duplicate)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public HrAttendanceEvent DuplicateOfEvent { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get event display name for logs/UI
        /// </summary>
        public string GetDisplayName()
        {
            return $"{EventType} - {EventSource} - {EventTimestamp:yyyy-MM-dd HH:mm:ss}";
        }

        /// <summary>
        /// Check if event is ready for processing
        /// </summary>
        public bool IsReadyForProcessing()
        {
            return ProcessingStatus == "Pending" &&
                   (!ScheduledFor.HasValue || ScheduledFor.Value <= DateTime.UtcNow);
        }

        /// <summary>
        /// Check if event processing is stuck (started but not completed)
        /// </summary>
        public bool IsStuck(TimeSpan maxProcessingTime)
        {
            return ProcessingStatus == "Processing" &&
                   ProcessingStartedAt.HasValue &&
                   (DateTime.UtcNow - ProcessingStartedAt.Value) > maxProcessingTime;
        }

        /// <summary>
        /// Mark event as processing
        /// </summary>
        public void MarkAsProcessing()
        {
            ProcessingStatus = "Processing";
            ProcessingStartedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark event as processed successfully
        /// </summary>
        public void MarkAsProcessed()
        {
            ProcessingStatus = "Processed";
            ProcessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark event as failed
        /// </summary>
        public void MarkAsFailed(string errorMessage)
        {
            ProcessingStatus = "Failed";
            ProcessingError = errorMessage;
            ProcessingRetryCount++;
        }

        /// <summary>
        /// Calculate retry delay using exponential backoff
        /// </summary>
        public TimeSpan GetRetryDelay()
        {
            // Exponential backoff: 1min, 2min, 4min, 8min, 16min, max 1 hour
            var delayMinutes = Math.Min(Math.Pow(2, ProcessingRetryCount), 60);
            return TimeSpan.FromMinutes(delayMinutes);
        }

        /// <summary>
        /// Check if event should be retried
        /// </summary>
        public bool ShouldRetry(int maxRetries = 5)
        {
            return ProcessingStatus == "Failed" &&
                   ProcessingRetryCount < maxRetries;
        }

        /// <summary>
        /// Validates event before save
        /// </summary>
        public (bool IsValid, List<string> Errors) Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(EventType))
                errors.Add("EventType is required");

            if (string.IsNullOrWhiteSpace(EventSource))
                errors.Add("EventSource is required");

            if (string.IsNullOrWhiteSpace(TriggeredBy))
                errors.Add("TriggeredBy is required (must be real user)");

            if (string.IsNullOrWhiteSpace(EventPayloadJson))
                errors.Add("EventPayloadJson is required");

            if (EventPriority < 1 || EventPriority > 10)
                errors.Add("EventPriority must be between 1 and 10");

            if (ProcessingRetryCount < 0)
                errors.Add("ProcessingRetryCount cannot be negative");

            return (errors.Count == 0, errors);
        }

        #endregion
    }

    #region Event Payload Classes

    /// <summary>
    /// Base class for event payloads (serialized to EventPayloadJson)
    /// </summary>
    public abstract class AttendanceEventPayload
    {
        public string EventType { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string TriggeredBy { get; set; }
    }

    /// <summary>
    /// Leave Approved Event Payload
    /// </summary>
    public class LeaveApprovedEventPayload : AttendanceEventPayload
    {
        public long LeaveRequestId { get; set; }
        public long EmployeeId { get; set; }
        public long LeaveTypeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalDays { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovalTimestamp { get; set; }
    }

    /// <summary>
    /// Leave Cancelled Event Payload
    /// </summary>
    public class LeaveCancelledEventPayload : AttendanceEventPayload
    {
        public long LeaveRequestId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string CancellationReason { get; set; }
    }

    /// <summary>
    /// Attendance Adjustment Approved Event Payload
    /// </summary>
    public class AttendanceAdjustmentApprovedEventPayload : AttendanceEventPayload
    {
        public long AdjustmentId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string AdjustmentType { get; set; }
        public int AdjustedMinutes { get; set; }
        public string AdjustmentField { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovalTimestamp { get; set; }
    }

    /// <summary>
    /// Manual Recalculation Event Payload
    /// </summary>
    public class ManualRecalculationEventPayload : AttendanceEventPayload
    {
        public long? EmployeeId { get; set; } // null for bulk
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long? DepartmentId { get; set; } // for bulk by department
        public string RecalculationReason { get; set; }
        public List<long> EmployeeIds { get; set; } // for bulk by employee list
    }

    /// <summary>
    /// Policy Changed Event Payload
    /// </summary>
    public class PolicyChangedEventPayload : AttendanceEventPayload
    {
        public long PolicyId { get; set; }
        public int OldPolicyVersion { get; set; }
        public int NewPolicyVersion { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public string ChangeDescription { get; set; }
        public bool RequiresRecalculation { get; set; }
    }

    #endregion
}
