using ZR.Model.Business.Dto;
using ZR.Model.Business;

namespace ZR.Service.Business.IBusinessService
{
    /// <summary>
    /// Organizations Information Tableservice接口
    /// </summary>
    public interface IOrganizationtypeService : IBaseService<Organizationtype>
    {
        PagedInfo<OrganizationtypeDto> GetList(OrganizationtypeQueryDto parm);

        Organizationtype GetInfo(long Organizationtypeid);


        Organizationtype AddOrganizationtype(Organizationtype parm);
        int UpdateOrganizationtype(Organizationtype parm);
        
        bool TruncateOrganizationtype();

        (string, object, object) ImportOrganizationtype(List<Organizationtype> list);

        PagedInfo<OrganizationtypeDto> ExportList(OrganizationtypeQueryDto parm);
    }
}

