using ZR.Mall.Model;
using ZR.Mall.Model.Dto;

namespace ZR.Mall.Service.IService
{
    /// <summary>
    /// 商品规格service接口
    /// </summary>
    public interface IProductSpecService : IBaseService<ProductSpec>
    {
        PagedInfo<ProductSpecDto> GetList(ShoppingProductSpecQueryDto parm);
        ProductSpec AddShoppingProductspec(ProductSpec parm);
        long UpdateProductSpec(long productId, List<ProductSpec> parm);
        long DeleteSpecByProductId(long productId);
    }
}

