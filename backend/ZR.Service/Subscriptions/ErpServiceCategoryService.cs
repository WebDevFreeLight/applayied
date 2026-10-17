using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Settings;
using ZR.Repository;
using ZR.Service.Subscriptions.IService;

namespace ZR.Service.Subscriptions
{



    [AppService(ServiceType = typeof(IErpServiceCategoryService), ServiceLifetime = LifeTime.Transient)]
    public class ErpServiceCategoryService : BaseService<ErpServiceCategory>, IErpServiceCategoryService
    {
     

        public PagedInfo<ErpServiceCategory> GetList(ErpServiceCategory parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpServiceCategory>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CategoryName), it => it.CategoryName.Contains(parm.CategoryName));
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.SortOrder).ToPage(pager);
        }

        public ErpServiceCategory GetInfo(long categoryId) => GetFirst(x => x.CategoryId == categoryId);
        public ErpServiceCategory AddCategory(ErpServiceCategory model) { model.CategoryId = SnowFlakeSingle.Instance.NextId(); Insert(model); return model; }
        public int UpdateCategory(ErpServiceCategory model) => Update(model, it => new { it.CategoryName, it.CategoryNameAr, it.ParentCategoryId, it.SortOrder, it.Update_by, it.Update_time }, f => f.CategoryId == model.CategoryId);
        public int Delete(long[] ids) => Context.Updateable<ErpServiceCategory>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.CategoryId)).ExecuteCommand();
        public List<ErpServiceCategory> GetActiveCategories() => Queryable().Where(x => x.IsActive == true).OrderBy(x => x.SortOrder).ToList();
        public List<ErpServiceCategory> GetCategoryTree() => Queryable().Where(x => x.IsActive == true).OrderBy(x => x.SortOrder).ToList();
    }

}

