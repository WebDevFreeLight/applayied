using ZR.Model.HR.Business.Dto;
using ZR.Model.HR.Business;

namespace ZR.Service.HR.Business.IBusinessService
{
    /// <summary>
    /// Insurance Plans Tableservice接口
    /// </summary>
    public interface IHrInsurancePlanService : IBaseService<HrInsurancePlan>
    {
        PagedInfo<HrInsurancePlanDto> GetList(HrInsurancePlanQueryDto parm);

        HrInsurancePlan GetInfo(long Planid);


        HrInsurancePlan AddHrInsurancePlan(HrInsurancePlan parm);
        int UpdateHrInsurancePlan(HrInsurancePlan parm);
        
        bool TruncateHrInsurancePlan();

        (string, object, object) ImportHrInsurancePlan(List<HrInsurancePlan> list);

        PagedInfo<HrInsurancePlanDto> ExportList(HrInsurancePlanQueryDto parm);
    }
}

