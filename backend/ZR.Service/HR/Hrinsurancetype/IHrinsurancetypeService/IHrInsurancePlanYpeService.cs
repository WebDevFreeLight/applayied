using ZR.Model.HR.Hrinsurancetype.Dto;
using ZR.Model.HR.Hrinsurancetype;

namespace ZR.Service.HR.Hrinsurancetype.IHrinsurancetypeService
{
    /// <summary>
    /// hrinsuranceplanypeservice接口
    /// </summary>
    public interface IHrInsurancePlanYpeService : IBaseService<HrInsurancePlanYpe>
    {
        PagedInfo<HrInsurancePlanYpeDto> GetList(HrInsurancePlanYpeQueryDto parm);

        HrInsurancePlanYpe GetInfo(long Id);


        HrInsurancePlanYpe AddHrInsurancePlanYpe(HrInsurancePlanYpe parm);
        int UpdateHrInsurancePlanYpe(HrInsurancePlanYpe parm);
        
        bool TruncateHrInsurancePlanYpe();

        (string, object, object) ImportHrInsurancePlanYpe(List<HrInsurancePlanYpe> list);

        PagedInfo<HrInsurancePlanYpeDto> ExportList(HrInsurancePlanYpeQueryDto parm);
    }
}

