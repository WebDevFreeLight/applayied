using Infrastructure.Attribute;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.AttendanceProcessing;
using ZR.Model.HR.Dto.Attendance;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.Attendance
{
    /// <summary>
    /// Payroll Attendance Summary Service - MOST CRITICAL SERVICE
    /// Generates attendance summaries for payroll with days-based salary factor
    /// </summary>
    [AppService(ServiceType = typeof(IPayrollAttendanceSummaryService), ServiceLifetime = LifeTime.Transient)]
    public class PayrollAttendanceSummaryService : IPayrollAttendanceSummaryService
    {
        private readonly HrPayrollAttendanceSummaryRepository _summaryRepo;
        private readonly HrAttendanceDayRepository _attendanceRepo;
        private readonly HrLeaveAttendanceEffectRepository _leaveEffectRepo;
        private readonly HrOvertimeResultRepository _overtimeRepo;

        public PayrollAttendanceSummaryService(
            HrPayrollAttendanceSummaryRepository summaryRepo,
            HrAttendanceDayRepository attendanceRepo,
            HrLeaveAttendanceEffectRepository leaveEffectRepo,
            HrOvertimeResultRepository overtimeRepo)
        {
            _summaryRepo = summaryRepo;
            _attendanceRepo = attendanceRepo;
            _leaveEffectRepo = leaveEffectRepo;
            _overtimeRepo = overtimeRepo;
        }

        public async Task<PayrollAttendanceSummaryDto> GenerateSummaryForEmployee(long employeeId, string payrollPeriod)
        {
            // Check if summary already exists
            if (await _summaryRepo.SummaryExists(employeeId, payrollPeriod))
            {
                // Recalculate existing summary
                return await RecalculateSummary(employeeId, payrollPeriod);
            }

            // Parse payroll period (e.g., "2025-01")
            var parts = payrollPeriod.Split('-');
            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get all attendance records for the period
            var attendanceRecords = await _attendanceRepo.GetAttendanceByEmployeeAndDateRange(employeeId, startDate, endDate);

            // Get leave effects for the period
            var leaveEffects = await _leaveEffectRepo.GetEffectsByEmployeeAndDateRange(employeeId, startDate, endDate);

            // Get overtime results for the period
            var overtimeResults = await _overtimeRepo.GetOvertimeByPayrollPeriod(employeeId, payrollPeriod);

            // Calculate summary metrics
            var summary = new HrPayrollAttendanceSummary
            {
                EmployeeId = employeeId,
                PayrollPeriod = payrollPeriod,

                // Sum expected and worked minutes
                TotalExpectedMinutes = attendanceRecords.Sum(a => a.ExpectedMinutes),
                TotalWorkedMinutes = attendanceRecords.Sum(a => a.WorkedMinutes),
                TotalLateMinutes = attendanceRecords.Sum(a => a.LateMinutes),
                TotalEarlyLeaveMinutes = attendanceRecords.Sum(a => a.EarlyLeaveMinutes),
                TotalAbsentMinutes = attendanceRecords.Where(a => a.IsAbsent).Sum(a => a.ExpectedMinutes),

                // Leave minutes from leave effects
                TotalLeaveMinutes = (int)leaveEffects.Sum(l => l.DeductedMinutes),

                // Overtime by type (Note: HrOvertimeResult doesn't have Remark, this logic needs to be adjusted)
                TotalApprovedOvertimeMinutes = overtimeResults.Sum(o => o.ApprovedMinutes),
                TotalWeekendOvertimeMinutes = 0, // TODO: Get from segments or add type classification
                TotalHolidayOvertimeMinutes = 0, // TODO: Get from segments or add type classification

                // Days count
                DaysPresent = attendanceRecords.Count(a => a.WorkedMinutes > 0),
                DaysAbsent = attendanceRecords.Count(a => a.IsAbsent),
                DaysOnLeave = leaveEffects.Select(l => l.AffectedDate).Distinct().Count(),

                IsFinalized = false
            };

            // Calculate salary factor (DAYS-BASED per user preference)
            var expectedWorkingDays = attendanceRecords.Count(a => a.ExpectedMinutes > 0);
            if (expectedWorkingDays > 0)
            {
                summary.CalculatedSalaryFactor = (decimal)summary.DaysPresent / expectedWorkingDays;
            }
            else
            {
                summary.CalculatedSalaryFactor = 1.0m;
            }

            // Save summary
            await _summaryRepo.InsertAsync(summary);

            return summary.Adapt<PayrollAttendanceSummaryDto>();
        }

        public async Task<List<PayrollAttendanceSummaryDto>> GenerateSummariesForPeriod(string payrollPeriod)
        {
            // TODO: Get all active employees and generate summaries
            // This would require access to employee repository
            return new List<PayrollAttendanceSummaryDto>();
        }

        public async Task<PayrollAttendanceSummaryDto> GetSummary(long employeeId, string payrollPeriod)
        {
            var summary = await _summaryRepo.GetSummaryByEmployeeAndPeriod(employeeId, payrollPeriod);
            return summary?.Adapt<PayrollAttendanceSummaryDto>();
        }

        public async Task<List<PayrollAttendanceSummaryDto>> GetSummariesByPeriod(string payrollPeriod)
        {
            var summaries = await _summaryRepo.GetSummariesByPeriod(payrollPeriod);
            return summaries.Adapt<List<PayrollAttendanceSummaryDto>>();
        }

        public async Task<bool> FinalizeSummary(long summaryId, long finalizedBy)
        {
            var summary = await _summaryRepo.GetByIdAsync(summaryId);
            if (summary == null) return false;

            summary.IsFinalized = true;
            summary.FinalizedDate = DateTime.Now;
            summary.FinalizedBy = finalizedBy;

            await _summaryRepo.UpdateAsync(summary);
            return true;
        }

        public async Task<int> FinalizeAllSummariesForPeriod(string payrollPeriod, long finalizedBy)
        {
            var summaries = await _summaryRepo.GetSummariesByPeriod(payrollPeriod);
            int count = 0;

            foreach (var summary in summaries.Where(s => !s.IsFinalized))
            {
                summary.IsFinalized = true;
                summary.FinalizedDate = DateTime.Now;
                summary.FinalizedBy = finalizedBy;

                await _summaryRepo.UpdateAsync(summary);
                count++;
            }

            return count;
        }

        public async Task<PayrollAttendanceSummaryDto> RecalculateSummary(long employeeId, string payrollPeriod)
        {
            // Delete existing summary
            var existing = await _summaryRepo.GetSummaryByEmployeeAndPeriod(employeeId, payrollPeriod);
            if (existing != null)
            {
                await _summaryRepo.DeleteAsync(existing);
            }

            // Regenerate (GenerateSummaryForEmployee now returns DTO)
            return await GenerateSummaryForEmployee(employeeId, payrollPeriod);
        }

        public async Task<List<string>> ValidateSummaries(string payrollPeriod)
        {
            var errors = new List<string>();
            var summaries = await _summaryRepo.GetSummariesByPeriod(payrollPeriod);

            foreach (var summary in summaries)
            {
                // Validate expected vs worked minutes
                if (summary.TotalWorkedMinutes > summary.TotalExpectedMinutes + summary.TotalApprovedOvertimeMinutes)
                {
                    errors.Add($"Employee {summary.EmployeeId}: Worked minutes exceed expected + overtime");
                }

                // Validate salary factor
                if (summary.CalculatedSalaryFactor < 0 || summary.CalculatedSalaryFactor > 1.5m)
                {
                    errors.Add($"Employee {summary.EmployeeId}: Invalid salary factor {summary.CalculatedSalaryFactor}");
                }

                // Validate days
                if (summary.DaysPresent + summary.DaysAbsent > 31)
                {
                    errors.Add($"Employee {summary.EmployeeId}: Days present + absent exceeds month days");
                }
            }

            return errors;
        }
    }
}
