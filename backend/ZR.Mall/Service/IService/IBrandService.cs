using ZR.Mall.Model;
using ZR.Mall.Model.Dto;

namespace ZR.Mall.Service.IService
{
    /// <summary>
    /// 品牌表service接口
    /// </summary>
    public interface IBrandService : IBaseService<Brand>
    {
        PagedInfo<BrandDto> GetList(ShopBrandQueryDto parm);

        Brand GetInfo(long Id);

        Brand AddShopBrand(Brand parm);
        int UpdateShopBrand(Brand parm);

        PagedInfo<BrandDto> ExportList(ShopBrandQueryDto parm);
    }
}

