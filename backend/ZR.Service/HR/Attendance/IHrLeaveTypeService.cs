using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// HR Leave Type Service Interface
    /// </summary>
    public interface IHrLeaveTypeService : IBaseService<HrLeaveType>
    {
        /// <summary>
        /// Get paginated list of leave types
        /// </summary>
        PagedInfo<HrLeaveTypeDto> GetList(HrLeaveTypeQueryDto parm);

        /// <summary>
        /// Get all active leave types
        /// </summary>
        Task<List<HrLeaveTypeDto>> GetActiveTypesAsync();

        /// <summary>
        /// Get leave type details by ID
        /// </summary>
        HrLeaveTypeDto GetInfo(long leaveTypeId);

        /// <summary>
        /// Add new leave type
        /// </summary>
        HrLeaveTypeDto AddHrLeaveType(CreateHrLeaveTypeDto dto);

        /// <summary>
        /// Update existing leave type
        /// </summary>
        int UpdateHrLeaveType(UpdateHrLeaveTypeDto dto);

        /// <summary>
        /// Delete leave type (soft delete if in use, hard delete otherwise)
        /// </summary>
        Task<int> DeleteAsync(long leaveTypeId);

        /// <summary>
        /// Truncate leave type table (with backup)
        /// </summary>
        bool TruncateHrLeaveType();

        /// <summary>
        /// Import leave types from list
        /// </summary>
        (string, object, object) ImportHrLeaveType(List<CreateHrLeaveTypeDto> list);

        /// <summary>
        /// Export leave types to list
        /// </summary>
        PagedInfo<HrLeaveTypeDto> ExportList(HrLeaveTypeQueryDto parm);
    }
}
