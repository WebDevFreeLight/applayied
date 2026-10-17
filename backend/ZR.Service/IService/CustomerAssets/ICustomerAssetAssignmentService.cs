using System;
using ZR.Model;
using ZR.Model.CustomerAssets;
using ZR.Model.CustomerAssets.Dto;

namespace ZR.Service.IService.CustomerAssets
{
    public interface ICustomerAssetAssignmentService : IBaseService<CustomerAssetAssignment>
    {
        PagedInfo<AssignmentListDto> GetList(AssignmentQueryDto query, PagerInfo pager);
        AssignmentDetailDto GetById(Guid id);
        CustomerAssetAssignment AssignAsset(AssignAssetDto dto, string userName);
        int ReturnAsset(ReturnAssetDto dto, string userName);
        int MarkLost(MarkLostDamagedDto dto, string userName);
        int MarkDamaged(MarkLostDamagedDto dto, string userName);
    }
}
