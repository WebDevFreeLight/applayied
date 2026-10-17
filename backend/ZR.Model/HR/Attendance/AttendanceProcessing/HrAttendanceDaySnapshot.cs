using SqlSugar;
using System;
using System.Collections.Generic;
using ZR.Model.System;

namespace ZR.Model.HR.Attendance.AttendanceProcessing
{
    /// <summary>
    /// Attendance Day Snapshot - IMMUTABLE versioned snapshot of daily attendance
    /// Purpose: Audit-safe, payroll-safe, recalculation-safe attendance records
    /// Replaces: Direct modification of HrAttendanceDay for finalized periods
    /// </summary>
    /// <remarks>
    /// CRITICAL BUSINESS RULES:
    /// 1. Snapshots are IMMUTABLE once finalized (is_finalized = true)
    /// 2. Recalculations create NEW snapshot versions (v1, v2, v3...)
    /// 3. Payroll MUST reference specific snapshot version
    /// 4. Policy state captured at calculation time (for reproducibility)
    /// 5. Complete audit trail: who, when, why, what changed
    /// </remarks>
    [SugarTable("hr_attendance_day_snapshot")]
    public class HrAttendanceDaySnapshot
    {
        /// <summary>
        /// Snapshot Primary Key (surrogate)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long SnapshotId { get; set; }

        #region Composite Natural Key

        /// <summary>
        /// Employee ID (part of natural key)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long EmployeeId { get; set; }

        /// <summary>
        /// Attendance Date (part of natural key)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime AttendanceDate { get; set; }

        /// <summary>
        /// Snapshot Version (1, 2, 3...)
        /// Increments each time attendance is recalculated for this employee/date
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int SnapshotVersion { get; set; }

        #endregion

        #region Snapshot Metadata

        /// <summary>
        /// When snapshot was created (UTC)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Who created the snapshot (user, not service account)
        /// </summary>
        [SugarColumn(Length = 100)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Why snapshot was created
        /// Examples: "InitialProcessing", "AdjustmentApplied", "LeaveApproved", "ManualRecalculation"
        /// </summary>
        [SugarColumn(Length = 200)]
        public string SnapshotReason { get; set; }

        /// <summary>
        /// Links to newer snapshot version (if this snapshot was superseded)
        /// </summary>
        public long? SupersededBySnapshotId { get; set; }

        #endregion

        #region Policy Snapshot (for reproducibility)

        /// <summary>
        /// Attendance Policy ID used for calculation
        /// </summary>
        public long? PolicyId { get; set; }

        /// <summary>
        /// Complete policy configuration as JSON (at calculation time)
        /// CRITICAL: Allows reproducing calculation even if policy changes later
        /// </summary>
        [SugarColumn(ColumnDataType = "jsonb")]
        public string PolicySnapshotJson { get; set; }

        /// <summary>
        /// Policy version used for calculation
        /// </summary>
        public int? PolicyVersion { get; set; }

        #endregion

        #region Shift Snapshot

        /// <summary>
        /// Shift ID used for calculation
        /// </summary>
        public long? ShiftId { get; set; }

        /// <summary>
        /// Complete shift configuration as JSON (at calculation time)
        /// </summary>
        [SugarColumn(ColumnDataType = "jsonb")]
        public string ShiftSnapshotJson { get; set; }

        #endregion

        #region Calculation Engine Metadata

        /// <summary>
        /// Software version used for calculation (e.g., "v2.1.0")
        /// CRITICAL: Different versions may produce different results
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 20)]
        public string CalculationEngineVersion { get; set; } = "v1.0.0";

        /// <summary>
        /// Exact timestamp when calculation was performed (UTC)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CalculationTimestamp { get; set; } = DateTime.UtcNow;

        #endregion

        #region Calculated Values (IMMUTABLE after finalization)

        /// <summary>
        /// Expected work minutes for this day (from shift/calendar)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int ExpectedMinutes { get; set; }

        /// <summary>
        /// Actual worked minutes (from attendance logs)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int WorkedMinutes { get; set; }

        /// <summary>
        /// Late arrival minutes
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int LateMinutes { get; set; }

        /// <summary>
        /// Early departure minutes
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int EarlyLeaveMinutes { get; set; }

        /// <summary>
        /// Overtime minutes (worked beyond expected)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int OvertimeMinutes { get; set; }

        /// <summary>
        /// Leave minutes (from approved leave requests)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int LeaveMinutes { get; set; }

        /// <summary>
        /// Break minutes (paid/unpaid breaks)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int BreakMinutes { get; set; }

        #endregion

        #region Status Flags

        /// <summary>
        /// TRUE if employee was absent (no attendance logs and working day)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsAbsent { get; set; }

        /// <summary>
        /// TRUE if this date is a weekend
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsWeekend { get; set; }

        /// <summary>
        /// TRUE if this date is a holiday
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsHoliday { get; set; }

        /// <summary>
        /// TRUE if employee took partial day leave (worked some hours)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsPartialDayLeave { get; set; }

        #endregion

        #region Punch Times

        /// <summary>
        /// First check-in time (UTC)
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// Last check-out time (UTC)
        /// </summary>
        public DateTime? CheckOutTime { get; set; }

        /// <summary>
        /// Device ID where employee checked in
        /// </summary>
        public long? CheckInDeviceId { get; set; }

        /// <summary>
        /// Device ID where employee checked out
        /// </summary>
        public long? CheckOutDeviceId { get; set; }

        #endregion

        #region Segment Proofs (for split shifts)

        /// <summary>
        /// JSON array of shift segments with work proof
        /// Example: [{"segment_id": 1, "in": "08:00", "out": "12:00", "worked": 240}]
        /// CRITICAL: Provides audit trail of exact work periods
        /// </summary>
        [SugarColumn(ColumnDataType = "jsonb")]
        public string SegmentsJson { get; set; }

        #endregion

        #region Leave Effect Linkage

        /// <summary>
        /// Array of hr_leave_attendance_effect IDs that affected this day
        /// Provides traceability: attendance → leave effects → leave requests
        /// </summary>
        [SugarColumn(ColumnDataType = "bigint[]")]
        public long[] LeaveEffectIds { get; set; }

        #endregion

        #region Attendance Log Linkage (proof of source data)

        /// <summary>
        /// Array of hr_attendance_log IDs used for this calculation
        /// CRITICAL: Links snapshot to raw source data (immutable proof)
        /// </summary>
        [SugarColumn(ColumnDataType = "bigint[]")]
        public long[] SourceLogIds { get; set; }

        #endregion

        #region Finalization (CRITICAL: Makes snapshot immutable)

        /// <summary>
        /// TRUE when snapshot is locked for payroll
        /// CRITICAL: Finalized snapshots CANNOT be modified (enforced by trigger)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsFinalized { get; set; }

        /// <summary>
        /// When snapshot was finalized (UTC)
        /// </summary>
        public DateTime? FinalizedAt { get; set; }

        /// <summary>
        /// Who finalized the snapshot (for accountability)
        /// </summary>
        [SugarColumn(Length = 100)]
        public string FinalizedBy { get; set; }

        /// <summary>
        /// Payroll period this snapshot was finalized for (YYYY-MM)
        /// Example: "2025-02"
        /// Links snapshot to payroll run
        /// </summary>
        [SugarColumn(Length = 7)]
        public string FinalizedForPayrollPeriod { get; set; }

        #endregion

        #region Event Linkage

        /// <summary>
        /// Event that triggered this snapshot creation
        /// Links to hr_attendance_event for full causality chain
        /// </summary>
        public long? TriggeredByEventId { get; set; }

        #endregion

        #region Audit Fields

        /// <summary>
        /// Optional remarks (e.g., "Recalculated after manual adjustment #12345")
        /// </summary>
        [SugarColumn(ColumnDataType = "text")]
        public string Remark { get; set; }

        #endregion

        #region Navigation Properties (not mapped)

        /// <summary>
        /// Navigation: Event that triggered this snapshot (if any)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public HrAttendanceEvent TriggeredByEvent { get; set; }

        /// <summary>
        /// Navigation: Newer snapshot version (if superseded)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public HrAttendanceDaySnapshot SupersededBySnapshot { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get snapshot display name for logs/UI
        /// Example: "Employee 123, 2025-02-15, v2"
        /// </summary>
        public string GetDisplayName()
        {
            return $"Employee {EmployeeId}, {AttendanceDate:yyyy-MM-dd}, v{SnapshotVersion}";
        }

        /// <summary>
        /// Check if snapshot can be modified
        /// </summary>
        public bool CanModify()
        {
            return !IsFinalized;
        }

        /// <summary>
        /// Validates snapshot before save
        /// </summary>
        public (bool IsValid, List<string> Errors) Validate()
        {
            var errors = new List<string>();

            if (SnapshotVersion < 1)
                errors.Add("Snapshot version must be >= 1");

            if (ExpectedMinutes < 0)
                errors.Add("Expected minutes cannot be negative");

            if (WorkedMinutes < 0)
                errors.Add("Worked minutes cannot be negative");

            if (IsFinalized && string.IsNullOrEmpty(FinalizedBy))
                errors.Add("Finalized snapshots must have FinalizedBy");

            if (IsFinalized && !FinalizedAt.HasValue)
                errors.Add("Finalized snapshots must have FinalizedAt");

            return (errors.Count == 0, errors);
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Shift segment work proof (stored in SegmentsJson)
    /// </summary>
    public class SnapshotSegmentProof
    {
        public long SegmentId { get; set; }
        public string SegmentName { get; set; }
        public TimeSpan? ExpectedStart { get; set; }
        public TimeSpan? ExpectedEnd { get; set; }
        public DateTime? ActualIn { get; set; }
        public DateTime? ActualOut { get; set; }
        public int WorkedMinutes { get; set; }
        public int ExpectedMinutes { get; set; }
        public bool IsMissed { get; set; }
        public long? InLogId { get; set; } // Link to attendance log
        public long? OutLogId { get; set; }
    }

    /// <summary>
    /// Policy snapshot (stored in PolicySnapshotJson)
    /// Captures policy state at calculation time
    /// </summary>
    public class PolicySnapshot
    {
        public long PolicyId { get; set; }
        public int PolicyVersion { get; set; }
        public string PolicyName { get; set; }
        public int StandardDailyMinutes { get; set; }
        public int MaxLateMinutes { get; set; }
        public int MaxEarlyLeaveMinutes { get; set; }
        public int MinMinutesForPresent { get; set; }
        public int OvertimeAfterMinutes { get; set; }
        public string RoundingRuleDictCode { get; set; }
        public DateTime CapturedAt { get; set; }
    }

    #endregion
}
