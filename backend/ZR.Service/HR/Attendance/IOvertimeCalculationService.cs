using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Interface for Overtime Calculation Service
    /// Calculates overtime based on policies and generates overtime results
    /// </summary>
    public interface IOvertimeCalculationService
    {
        /// <summary>
        /// Calculate overtime for a specific attendance day
        /// </summary>
        Task<OvertimeResultDto> CalculateOvertimeForDay(long attendanceDayId);

        /// <summary>
        /// Calculate overtime for an employee for a date range
        /// </summary>
        Task<List<OvertimeResultDto>> CalculateOvertimeForPeriod(long employeeId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Submit overtime request (pre-approval)
        /// </summary>
        Task<HrOvertimeRequestDto> SubmitOvertimeRequest(OvertimeRequestCreateDto dto);

        /// <summary>
        /// Approve overtime request
        /// </summary>
        Task<bool> ApproveOvertimeRequest(ApproveOvertimeRequestDto dto);

        /// <summary>
        /// Get overtime summary for an employee
        /// </summary>
        Task<OvertimeSummaryDto> GetOvertimeSummary(long employeeId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get applicable overtime policy for an employee
        /// </summary>
        Task<OvertimePolicyDto> GetOvertimePolicy(long employeeId);

        /// <summary>
        /// Determine overtime type (daily, weekend, holiday)
        /// </summary>
        Task<string> DetermineOvertimeType(long employeeId, DateTime date);

        // Additional methods for controller support
        Task<object> CalculateOvertimeAsync(long employeeId, DateTime startDate, DateTime endDate);
        Task<object> GetEmployeeOvertimeSummaryAsync(long employeeId, DateTime startDate, DateTime endDate);
        Task<object> GetOvertimeRequestsAsync(object dto);
        Task<object> CreateOvertimeRequestAsync(object dto);
        Task<bool> ApproveOvertimeRequestAsync(long requestId, object dto);
        Task<bool> RejectOvertimeRequestAsync(long requestId, object dto);
        Task<object> GetOvertimeReportAsync(DateTime startDate, DateTime endDate, long? departmentId);
        Task<List<object>> GetOvertimePoliciesAsync();
        Task<object> CreateOvertimePolicyAsync(object dto);
        Task<bool> UpdateOvertimePolicyAsync(long policyId, object dto);
        Task<int> VerifyOvertimeSegmentsAsync(object dto);
        Task<List<object>> GetPendingVerificationsAsync();
    }
}
