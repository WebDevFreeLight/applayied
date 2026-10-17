using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;

namespace ZR.Service.Boulevard.IBoulevardService
{
    /// <summary>
    /// shop_categoryservice接口
    /// </summary>
    public interface IBlvBoulevardShopCategoryService : IBaseService<BlvBoulevardShopCategory>
    {
        PagedInfo<BlvBoulevardShopCategoryDto> GetList(BlvBoulevardShopCategoryQueryDto parm);

        BlvBoulevardShopCategory GetInfo(long Id);


        BlvBoulevardShopCategory AddBlvBoulevardShopCategory(BlvBoulevardShopCategory parm);
        int UpdateBlvBoulevardShopCategory(BlvBoulevardShopCategory parm);
        
        bool TruncateBlvBoulevardShopCategory();

        (string, object, object) ImportBlvBoulevardShopCategory(List<BlvBoulevardShopCategory> list);

        PagedInfo<BlvBoulevardShopCategoryDto> ExportList(BlvBoulevardShopCategoryQueryDto parm);
    }
}

