using ZR.Mall.Model;
using ZR.Mall.Model.Dto;
using ZR.Mall.Service.IService;

namespace ZR.Mall.Service
{
    /// <summary>
    /// 品牌表Service业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IBrandService))]
    public class BrandService : BaseService<Brand>, IBrandService
    {
        /// <summary>
        /// 查询品牌表列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BrandDto> GetList(ShopBrandQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .ToPage<Brand, BrandDto>(parm);

            return response;
        }


        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Brand GetInfo(long Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 添加品牌表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Brand AddShopBrand(Brand model)
        {
            return Insertable(model).ExecuteReturnEntity();
        }

        /// <summary>
        /// 修改品牌表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateShopBrand(Brand model)
        {
            return Update(model, true, "修改品牌表");
        }

        /// <summary>
        /// 导出品牌表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BrandDto> ExportList(ShopBrandQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BrandDto()
                {
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<Brand> QueryExp(ShopBrandQueryDto parm)
        {
            var predicate = Expressionable.Create<Brand>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Name), it => it.Name == parm.Name);
            return predicate;
        }
    }
}
