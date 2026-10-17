using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsWorkCategoryService : IBaseService<OpsWorkCategory>
    {
        PagedInfo<OpsWorkCategoryDto> GetList(OpsWorkCategoryQueryDto parm);
        OpsWorkCategory GetInfo(long workCategoryId);
        OpsWorkCategory AddWorkCategory(OpsWorkCategory model);
        int UpdateWorkCategory(OpsWorkCategory model);
        PagedInfo<OpsWorkCategoryDto> ExportList(OpsWorkCategoryQueryDto parm);
        List<OpsWorkCategoryDto> GetAllActive();
    }
}
