using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.CustomerAssets;
using ZR.Model.CustomerAssets.Dto;

namespace ZR.Service.IService.CustomerAssets
{
    public interface ICustomerAssetService : IBaseService<CustomerAsset>
    {
        PagedInfo<CustomerAssetListDto> GetList(CustomerAssetQueryDto query, PagerInfo pager);
        CustomerAssetDetailDto GetById(Guid id);
        CustomerAsset Create(CreateCustomerAssetDto dto, string userName);
        int Update(UpdateCustomerAssetDto dto, string userName);
        int Delete(Guid[] ids);
        List<AvailableAssetSelectDto> GetAvailableAssets();
        string GenerateAssetCode();
    }
}
