using ZR.Mall.Model;
using ZR.Mall.Model.Dto;
using ZR.Mall.Service.IService;

namespace ZR.Mall.Service
{
    /// <summary>
    /// 商品库存Service业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(ISkusService))]
    public class SkusService : BaseService<Skus>, ISkusService
    {
        /// <summary>
        /// 查询商品库存列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<SkusDto> GetList(ShoppingSkusQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .ToPage<Skus, SkusDto>(parm);

            return response;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="SkuId"></param>
        /// <returns></returns>
        public Skus GetInfo(long SkuId)
        {
            var response = Queryable()
                .Where(x => x.SkuId == SkuId)
                .First();

            return response;
        }

        /// <summary>
        /// 添加商品库存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Skus AddShoppingSkus(Skus model)
        {
            return Insertable(model).ExecuteReturnEntity();
        }

        /// <summary>
        /// 修改商品库存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public long UpdateShoppingSkus(Skus model)
        {
            return Update(model, true);
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<Skus> QueryExp(ShoppingSkusQueryDto parm)
        {
            var predicate = Expressionable.Create<Skus>();

            return predicate;
        }

        public List<Skus> GetSkus(long productId)
        {
            return Queryable()
                .Where(f => f.ProductId == productId && f.IsDelete == 0)
                .ToList();
        }
    }
}
