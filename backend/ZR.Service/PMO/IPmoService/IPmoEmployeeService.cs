using System.Collections.Generic;
using ZR.Model;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    /// <summary>
    /// PMO employee directory over hr_employee_core: list/create/update/delete, plus a user lookup.
    /// Keeps the portal's user→employee link (used by the login employee lookup) manageable without
    /// pulling in the full HR person/dictionary machinery.
    /// </summary>
    public interface IPmoEmployeeService
    {
        PagedInfo<PmoEmployeeVo> GetList(PmoEmployeeQueryDto parm);
        PmoEmployeeVo GetInfo(long employeeId);
        long AddEmployee(PmoEmployeeInputDto dto, string actor);
        int UpdateEmployee(PmoEmployeeInputDto dto, string actor);
        int RemoveEmployee(long employeeId);
        /// <summary>Users not yet linked to an employee — for the "link user" dropdown.</summary>
        List<object> UnlinkedUsers(string search);
    }
}
