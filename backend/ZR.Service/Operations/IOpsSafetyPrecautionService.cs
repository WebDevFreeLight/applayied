using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsSafetyPrecautionService : IBaseService<OpsSafetyPrecaution>
    {
        PagedInfo<OpsSafetyPrecautionDto> GetList(OpsSafetyPrecautionQueryDto parm);
        OpsSafetyPrecaution GetInfo(long precautionId);
        OpsSafetyPrecaution AddPrecaution(OpsSafetyPrecaution model);
        int UpdatePrecaution(OpsSafetyPrecaution model);
        PagedInfo<OpsSafetyPrecautionDto> ExportList(OpsSafetyPrecautionQueryDto parm);
        List<OpsSafetyPrecautionDto> GetAllActive();
    }
}
