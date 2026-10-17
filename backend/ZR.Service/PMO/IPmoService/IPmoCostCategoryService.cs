using System.Collections.Generic;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoCostCategoryService
    {
        List<PmoCostCategory> GetAll();
        PmoCostCategory Add(PmoCostCategoryInputDto dto, PmoActor actor);
        int Update(PmoCostCategoryInputDto dto, PmoActor actor);
    }
}
