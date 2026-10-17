using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto.Attendance;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.Attendance
{
    [AppService(ServiceType = typeof(ILeaveManagementService), ServiceLifetime = LifeTime.Transient)]
    public class LeaveManagementService : BaseService<HrLeaveRequest>, ILeaveManagementService
    {
        private readonly HrLeaveRequestRepository _requestRepo;
        private readonly HrLeaveBalanceRepository _balanceRepo;
        private readonly HrLeaveAttendanceEffectRepository _effectRepo;
        private readonly HrLeavePolicyRepository _policyRepo;

        public LeaveManagementService(
            HrLeaveRequestRepository requestRepo,
            HrLeaveBalanceRepository balanceRepo,
            HrLeaveAttendanceEffectRepository effectRepo,
            HrLeavePolicyRepository policyRepo)
        {
            _requestRepo = requestRepo;
            _balanceRepo = balanceRepo;
            _effectRepo = effectRepo;
            _policyRepo = policyRepo;
        }

        public async Task<HrLeaveRequestDto> SubmitLeaveRequest(LeaveRequestCreateDto dto)
        {
            var request = dto.Adapt<HrLeaveRequest>();
            request.StatusDictCode = 1; // Pending

            await _requestRepo.InsertAsync(request);
            return request.Adapt<HrLeaveRequestDto>();
        }

        public async Task ApproveLeaveRequest(LeaveApprovalDto dto)
        {
            var request = await _requestRepo.GetByIdAsync(dto.RequestId);
            if (request == null)
                throw new Exception("Leave request not found");

            request.StatusDictCode = 2; // Approved
            if (dto.ApprovedMinutes.HasValue)
            {
                request.ApprovedMinutes = dto.ApprovedMinutes;
            }
            else
            {
                // Calculate approved minutes based on date range
                var days = (request.ToDate.Date - request.FromDate.Date).Days + 1;
                request.ApprovedMinutes = days * 480; // 8 hours = 480 minutes
            }

            await _requestRepo.UpdateAsync(request);

            // Generate leave effects
            await CalculateLeaveEffects(request.RequestId);

            // Update leave balance is handled in CalculateLeaveEffects
        }

        public async Task<bool> RejectLeaveRequest(RejectLeaveRequestDto dto)
        {
            var request = await _requestRepo.GetByIdAsync(dto.RequestId);
            request.StatusDictCode = 3; // Rejected

            await _requestRepo.UpdateAsync(request);
            return true;
        }

        public async Task<List<LeaveBalanceDto>> GetLeaveBalances(long employeeId)
        {
            var balances = await _balanceRepo.GetBalancesByEmployee(employeeId);
            return balances.Adapt<List<LeaveBalanceDto>>();
        }

        public async Task<List<HrLeaveRequestDto>> GetLeaveHistory(long employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var requests = await _requestRepo.GetLeaveRequestsByEmployee(employeeId);
            return requests.Adapt<List<HrLeaveRequestDto>>();
        }

        public async Task CalculateLeaveEffects(long requestId)
        {
            // Fetch the leave request by ID
            var leaveRequest = await _requestRepo.GetByIdAsync(requestId);
            if (leaveRequest == null)
                return;

            // Get leave type to determine policy settings
            var leaveType = await Context.Queryable<HrLeaveType>()
                .Where(t => t.LeaveTypeId == leaveRequest.LeaveTypeId)
                .FirstAsync();

            var isPaid = leaveType?.IsPaid ?? true;

            for (var date = leaveRequest.FromDate.Date; date <= leaveRequest.ToDate.Date; date = date.AddDays(1))
            {
                var effect = new HrLeaveAttendanceEffect
                {
                    LeaveRequestId = leaveRequest.RequestId,
                    EmployeeId = leaveRequest.EmployeeId,
                    AffectedDate = date,
                    DeductedMinutes = 480, // 8 hours per day - can be enhanced to use shift hours
                    MarkedAsAbsent = !isPaid, // Mark as absent only for unpaid leave
                    AffectsOvertimeCalculation = true, // Leave days don't count for overtime
                    Remark = $"Auto-generated from approved leave request {leaveRequest.RequestId}"
                };

                await _effectRepo.InsertAsync(effect);
            }

            // Update leave balance
            var totalMinutes = leaveRequest.ApprovedMinutes ?? ((leaveRequest.ToDate.Date - leaveRequest.FromDate.Date).Days + 1) * 480;
            await UpdateLeaveBalance(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId, totalMinutes);
        }

        public async Task<bool> UpdateLeaveBalance(long employeeId, long leaveTypeId, decimal deductMinutes)
        {
            var balance = await _balanceRepo.GetBalanceByEmployeeAndType(employeeId, leaveTypeId);
            if (balance == null) return false;

            // Deduct from balance
            balance.UsedMinutes += deductMinutes;
            balance.BalanceMinutes = balance.EntitledMinutes - balance.UsedMinutes;

            // Ensure balance doesn't go negative
            if (balance.BalanceMinutes < 0)
            {
                throw new Exception($"Insufficient leave balance. Available: {balance.BalanceMinutes / 480} days, Requested: {deductMinutes / 480} days");
            }

            await _balanceRepo.UpdateAsync(balance);
            return true;
        }
    }
}
