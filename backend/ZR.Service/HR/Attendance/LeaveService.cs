using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using SqlSugar;
using ZR.Model.HR.Attendance;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    [AppService(ServiceType = typeof(ILeaveService), ServiceLifetime = LifeTime.Transient)]
    public class LeaveService : BaseService<HrLeave>, ILeaveService
    {
        private const int DailyWorkingMinutes = 480;

        #region Queries

        public async Task<PagedInfo<HrLeave>> GetPageListAsync(
            Expression<Func<HrLeave, bool>> predicate,
            PagerInfo pager)
        {
            return await Queryable()
                .Where(predicate)
                .OrderBy(l => l.StartDate, OrderByType.Desc)
                .ToPageAsync(pager);
        }

        public async Task<HrLeave> GetByIdAsync(long leaveId)
        {
            return await Queryable()
                .Where(l => l.LeaveId == leaveId)
                .FirstAsync();
        }

        #endregion

        #region Submit Leave

        public async Task<long> SubmitLeaveRequestAsync(HrLeave leave, long requestedById)
        {
            if (leave == null)
                throw new ArgumentNullException(nameof(leave));

            CalculateRequestedMinutes(leave);

            await ValidateOverlapAsync(leave);
            await ValidateBalanceAsync(leave.EmployeeId, leave.LeaveTypeId, leave.RequestedMinutes);

            leave.StatusDictCode = 1; // Pending
            leave.WorkflowStep = 1;
            leave.IsFinalized = false;
            leave.AttendanceEventGenerated = false;
            leave.Create_time = DateTime.Now;
            leave.Create_by = requestedById.ToString();

            return await InsertReturnIdentityAsync(leave);
        }

        private void CalculateRequestedMinutes(HrLeave leave)
        {
            if (leave.RequestedMinutes > 0)
                return;

            if (leave.StartTime.HasValue && leave.EndTime.HasValue &&
                leave.StartDate.Date == leave.EndDate.Date)
            {
                leave.RequestedMinutes =
                    (int)(leave.EndTime.Value - leave.StartTime.Value).TotalMinutes;
            }
            else
            {
                var days = (leave.EndDate.Date - leave.StartDate.Date).Days + 1;
                leave.RequestedMinutes = days * DailyWorkingMinutes;
            }
        }

        private async Task ValidateOverlapAsync(HrLeave leave)
        {
            var exists = await Queryable()
                .Where(l =>
                    l.EmployeeId == leave.EmployeeId &&
                    l.LeaveId != leave.LeaveId &&
                    l.StatusDictCode != 3 &&
                    l.StatusDictCode != 4 &&
                    !(l.EndDate < leave.StartDate || l.StartDate > leave.EndDate))
                .AnyAsync();

            if (exists)
                throw new AppException("«·≈Ã«“…   ⁄«—÷ „⁄ ÿ·» ¬Œ—.");
        }

        private async Task ValidateBalanceAsync(long employeeId, long leaveTypeId, int requestedMinutes)
        {
            var balance = await GetCurrentBalanceAsync(employeeId, leaveTypeId);

            if (balance == null || balance.BalanceMinutes < requestedMinutes)
                throw new AppException("—’Ìœ «·≈Ã«“… €Ì— ﬂ«›Ú.");
        }

        #endregion

        #region Approval Flow

        public async Task<bool> ApproveLeaveAsync(long leaveId, long approverId, string comment = null)
        {
            var leave = await GetByIdAsync(leaveId);

            if (leave == null || leave.IsFinalized || leave.StatusDictCode != 1)
                throw new AppException("·« Ì„ﬂ‰ «⁄ „«œ Â–« «·ÿ·».");

            await Context.Ado.BeginTranAsync();

            try
            {
                await Context.Updateable<HrLeaveBalance>()
                    .SetColumns(b => new HrLeaveBalance
                    {
                        UsedMinutes = b.UsedMinutes + leave.RequestedMinutes,
                        BalanceMinutes = b.BalanceMinutes - leave.RequestedMinutes,
                        Update_time = DateTime.Now
                    })
                    .Where(b =>
                        b.EmployeeId == leave.EmployeeId &&
                        b.LeaveTypeId == leave.LeaveTypeId &&
                        b.Year == leave.StartDate.Year)
                    .ExecuteCommandAsync();

                await Context.Updateable<HrLeave>()
                    .SetColumns(l => new HrLeave
                    {
                        StatusDictCode = 2,
                        ApprovedBy = approverId,
                        ApprovedAt = DateTime.Now,
                        ApprovalComment = comment,
                        IsFinalized = true,
                        AttendanceEventGenerated = true,
                        Update_time = DateTime.Now,
                        Update_by = approverId.ToString()
                    })
                    .Where(l => l.LeaveId == leaveId)
                    .ExecuteCommandAsync();

                await GenerateAttendanceEffectAsync(leave);

                await Context.Ado.CommitTranAsync();
                return true;
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
        }

        public async Task<bool> RejectLeaveAsync(long leaveId, long rejectedById, string reason)
        {
            return await Context.Updateable<HrLeave>()
                .SetColumns(l => new HrLeave
                {
                    StatusDictCode = 3,
                    RejectionReason = reason,
                    IsFinalized = true,
                    Update_time = DateTime.Now,
                    Update_by = rejectedById.ToString()
                })
                .Where(l => l.LeaveId == leaveId)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> CancelLeaveAsync(long leaveId, long cancelledById, string reason = null)
        {
            var leave = await GetByIdAsync(leaveId);

            if (leave == null || leave.StatusDictCode == 4)
                throw new AppException("«·ÿ·» €Ì— ’«·Õ ··≈·€«¡.");

            await Context.Ado.BeginTranAsync();

            try
            {
                if (leave.StatusDictCode == 2)
                {
                    await Context.Updateable<HrLeaveBalance>()
                        .SetColumns(b => new HrLeaveBalance
                        {
                            UsedMinutes = b.UsedMinutes - leave.RequestedMinutes,
                            BalanceMinutes = b.BalanceMinutes + leave.RequestedMinutes,
                            Update_time = DateTime.Now
                        })
                        .Where(b =>
                            b.EmployeeId == leave.EmployeeId &&
                            b.LeaveTypeId == leave.LeaveTypeId)
                        .ExecuteCommandAsync();
                }

                await Context.Updateable<HrLeave>()
                    .SetColumns(l => new HrLeave
                    {
                        StatusDictCode = 4,
                        CancellationReason = reason,
                        IsFinalized = true,
                        Update_time = DateTime.Now,
                        Update_by = cancelledById.ToString()
                    })
                    .Where(l => l.LeaveId == leaveId)
                    .ExecuteCommandAsync();

                await Context.Ado.CommitTranAsync();
                return true;
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
        }

        #endregion

        #region Balance

        public Task<HrLeaveBalance> GetCurrentBalanceAsync(long employeeId, long leaveTypeId)
        {
            return Context.Queryable<HrLeaveBalance>()
                .Where(b =>
                    b.EmployeeId == employeeId &&
                    b.LeaveTypeId == leaveTypeId &&
                    b.Year == DateTime.Now.Year)
                .FirstAsync();
        }

        public async Task<bool> HasSufficientBalanceAsync(
            long employeeId,
            long leaveTypeId,
            decimal requestedMinutes)
        {
            var balance = await GetCurrentBalanceAsync(employeeId, leaveTypeId);
            return balance != null && balance.BalanceMinutes >= requestedMinutes;
        }

        #endregion

        #region Attendance Effect

        private async Task GenerateAttendanceEffectAsync(HrLeave leave)
        {
            var totalDays =
                (leave.EndDate.Date - leave.StartDate.Date).Days + 1;

            var minutesPerDay =
                leave.RequestedMinutes / totalDays;

            for (var date = leave.StartDate.Date; date <= leave.EndDate.Date; date = date.AddDays(1))
            {
                await Context.Insertable(new HrLeaveAttendanceEffect
                {
                    LeaveRequestId = leave.LeaveId,
                    EmployeeId = leave.EmployeeId,
                    AffectedDate = date,
                    DeductedMinutes = minutesPerDay,
                    MarkedAsAbsent = true,
                    AffectsOvertimeCalculation = true,
                    Remark = "Auto generated from approved leave"
                }).ExecuteCommandAsync();
            }
        }

        public Task<List<HrLeave>> GetEmployeeLeavesAsync(long employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ForwardToNextApproverAsync(long leaveId, long currentApproverId, string comment = null)
        {
            throw new NotImplementedException();
        }

        public Task<LeaveStatisticsDto> GetLeaveStatisticsAsync(long? employeeId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> ApplyLeaveAsync(HrLeave leave)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
