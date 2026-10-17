using ZR.Model.HR.Hrinsurancetype.Dto;
using ZR.Model.HR.Hrinsurancetype;

namespace ZR.Service.HR.Hrinsurancetype.IHrinsurancetypeService
{
    /// <summary>
    /// employeeependentrelationshipservice接口
    /// </summary>
    public interface IHrEmployeeDependentRelationshipService : IBaseService<HrEmployeeDependentRelationship>
    {
        PagedInfo<HrEmployeeDependentRelationshipDto> GetList(HrEmployeeDependentRelationshipQueryDto parm);

        HrEmployeeDependentRelationship GetInfo(long Dependentid);


        HrEmployeeDependentRelationship AddHrEmployeeDependentRelationship(HrEmployeeDependentRelationship parm);
        int UpdateHrEmployeeDependentRelationship(HrEmployeeDependentRelationship parm);
        
        bool TruncateHrEmployeeDependentRelationship();

        (string, object, object) ImportHrEmployeeDependentRelationship(List<HrEmployeeDependentRelationship> list);

        PagedInfo<HrEmployeeDependentRelationshipDto> ExportList(HrEmployeeDependentRelationshipQueryDto parm);
    }
}

