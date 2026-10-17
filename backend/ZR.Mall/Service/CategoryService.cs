using Infrastructure.Extensions;
using ZR.Mall.Model;
using ZR.Mall.Model.Dto;
using ZR.Mall.Service.IService;

namespace ZR.Mall.Service
{
    /// <summary>
    /// 
    /// </summary>
    [AppService(ServiceType = typeof(ICategoryService))]
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<CategoryDto> GetList(ShoppingCategoryQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .WithCache(60 * 5)
                .ToPage<Category, CategoryDto>(parm);

            return response;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public List<CategoryDto> GetTreeList(ShoppingCategoryQueryDto parm)
        {
            var predicate = QueryExp(parm);
            var response = Queryable()
                .Where(predicate.ToExpression());

            if (parm.Sort.IsNotEmpty())
            {
                response = response.OrderByPropertyName(parm.Sort, parm.SortType.Contains("desc") ? OrderByType.Desc : OrderByType.Asc);
            }
            var treeList = response.ToTree(it => it.Children, it => it.ParentId, 0);

            return treeList.Adapt<List<CategoryDto>>();
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public int AddCategory(Category parm)
        {
            var response = Add(parm);
            return response;
        }

        /// <summary>
        /// ??
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<CategoryDto> ExportList(ShoppingCategoryQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new CategoryDto()
                {
                }, true)
                .ToPage(parm);

            return response;
        }
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<Category> QueryExp(ShoppingCategoryQueryDto parm)
        {
            var predicate = Expressionable.Create<Category>();
            predicate.AndIF(parm.ShowStatus != null, m => m.ShowStatus == parm.ShowStatus);
            predicate.AndIF(parm.ParentId != null, m => m.ParentId == parm.ParentId);
            predicate.AndIF(parm.Name.IsNotEmpty(), m => m.Name.Contains(parm.Name));

            return predicate;
        }
    }
}

