using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.CustomerAssets;
using ZR.Model.CustomerAssets.Dto;

namespace ZR.Service.IService.CustomerAssets
{
    public interface IAssetTypeService : IBaseService<AssetType>
    {
        PagedInfo<AssetTypeListDto> GetList(AssetTypeQueryDto query, PagerInfo pager);
        AssetType GetById(Guid id);
        AssetType Create(CreateAssetTypeDto dto, string userName);
        int Update(UpdateAssetTypeDto dto, string userName);
        int Delete(Guid[] ids);
        bool CodeExists(string code, Guid? excludeId = null);
        List<AssetTypeSelectDto> GetSelectList();
    }
}
