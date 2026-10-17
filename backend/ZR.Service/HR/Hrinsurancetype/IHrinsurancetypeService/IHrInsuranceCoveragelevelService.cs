using ZR.Model.HR.Hrinsurancetype.Dto;
using ZR.Model.HR.Hrinsurancetype;

namespace ZR.Service.HR.Hrinsurancetype.IHrinsurancetypeService
{
    /// <summary>
    /// insurancetypeservice接口
    /// </summary>
    public interface IHrInsuranceCoveragelevelService : IBaseService<HrInsuranceCoveragelevel>
    {
        PagedInfo<HrInsuranceCoveragelevelDto> GetList(HrInsuranceCoveragelevelQueryDto parm);

        HrInsuranceCoveragelevel GetInfo(long Id);


        HrInsuranceCoveragelevel AddHrInsuranceCoveragelevel(HrInsuranceCoveragelevel parm);
        int UpdateHrInsuranceCoveragelevel(HrInsuranceCoveragelevel parm);
        
        bool TruncateHrInsuranceCoveragelevel();

        (string, object, object) ImportHrInsuranceCoveragelevel(List<HrInsuranceCoveragelevel> list);

        PagedInfo<HrInsuranceCoveragelevelDto> ExportList(HrInsuranceCoveragelevelQueryDto parm);
    }
}

