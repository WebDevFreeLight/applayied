using ZR.Model.Business.Dto;
using ZR.Model.Business;

namespace ZR.Service.Business.IBusinessService
{
    /// <summary>
    /// Organizations Information Tableservice接口
    /// </summary>
    public interface IOrganizationsService : IBaseService<Organizations>
    {
        PagedInfo<OrganizationsDto> GetList(OrganizationsQueryDto parm);

        Organizations GetInfo(long Organizationsid);

        List<Organizations> GetTreeList(OrganizationsQueryDto parm);

        Organizations AddOrganizations(Organizations parm);
        int UpdateOrganizations(Organizations parm);
        
        bool TruncateOrganizations();

        (string, object, object) ImportOrganizations(List<Organizations> list);

        PagedInfo<OrganizationsDto> ExportList(OrganizationsQueryDto parm);
    }
}

