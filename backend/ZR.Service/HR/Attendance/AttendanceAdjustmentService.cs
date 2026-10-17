using Infrastructure.Attribute;
using Mapster;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.AttendanceProcessing;
using ZR.Model.HR.Dto.Attendance;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.Attendance
{
    [AppService(ServiceType = typeof(IAttendanceAdjustmentService), ServiceLifetime = LifeTime.Transient)]
    public class AttendanceAdjustmentService : IAttendanceAdjustmentService
    {
        private readonly HrAttendanceAdjustmentRepository _adjustmentRepo;
        private readonly IAttendanceProcessingService _attendanceService;

        public AttendanceAdjustmentService(
            HrAttendanceAdjustmentRepository adjustmentRepo,
            IAttendanceProcessingService attendanceService)
        {
            _adjustmentRepo = adjustmentRepo;
            _attendanceService = attendanceService;
        }

        public async Task<HrAttendanceAdjustmentDto> CreateAdjustment(CreateHrAttendanceAdjustmentDto dto)
        {
            var adjustment = dto.Adapt<HrAttendanceAdjustment>();
            adjustment.IsApproved = false;

            await _adjustmentRepo.InsertAsync(adjustment);
            return adjustment.Adapt<HrAttendanceAdjustmentDto>();
        }

        public async Task<bool> ApproveAdjustment(ApproveAdjustmentDto dto)
        {
            var adjustment = await _adjustmentRepo.GetByIdAsync(dto.AdjustmentId);
            adjustment.IsApproved = true;

            await _adjustmentRepo.UpdateAsync(adjustment);

            // Apply the adjustment
            await ApplyAdjustment(dto.AdjustmentId);

            return true;
        }

        public async Task<bool> RejectAdjustment(long adjustmentId, long rejectedBy, string reason)
        {
            var adjustment = await _adjustmentRepo.GetByIdAsync(adjustmentId);
            adjustment.IsApproved = false;

            await _adjustmentRepo.UpdateAsync(adjustment);
            return true;
        }

        public async Task<HrAttendanceDayDto> ApplyAdjustment(long adjustmentId)
        {
            var adjustment = await _adjustmentRepo.GetByIdAsync(adjustmentId);
            if (!adjustment.IsApproved) return null;

            // Recalculate attendance for the date (now returns DTO)
            return await _attendanceService.RecalculateAttendance(adjustment.EmployeeId, adjustment.AdjustmentDate);
        }

        public async Task<List<HrAttendanceAdjustmentDto>> GetAdjustments(long employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var adjustments = await _adjustmentRepo.GetListAsync(adj => adj.EmployeeId == employeeId);
            return adjustments.Adapt<List<HrAttendanceAdjustmentDto>>();
        }
    }
}
