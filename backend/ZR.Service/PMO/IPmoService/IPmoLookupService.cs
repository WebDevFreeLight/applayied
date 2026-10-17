using System.Collections.Generic;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoLookupService
    {
        List<PmoLookupItem> Departments();
        List<PmoLookupItem> Users();
        List<PmoLookupItem> Managers();
        List<PmoLookupItem> Projects();
    }
}
