using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Settings;
using ZR.Repository;
using ZR.Service.Settings.IService;

namespace ZR.Service.Settings
{


    [AppService(ServiceType = typeof(IErpExpenseTypeService), ServiceLifetime = LifeTime.Transient)]
    public class ErpExpenseTypeService : BaseService<ErpExpenseType>, IErpExpenseTypeService
    {
        

        public PagedInfo<ErpExpenseType> GetList(ErpExpenseType parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpExpenseType>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TypeName), it => it.TypeName.Contains(parm.TypeName));
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.TypeName).ToPage(pager);
        }

        public ErpExpenseType GetInfo(long expenseTypeId) => GetFirst(x => x.ExpenseTypeId == expenseTypeId);
        public ErpExpenseType AddExpenseType(ErpExpenseType model) { model.ExpenseTypeId = SnowFlakeSingle.Instance.NextId(); Insert(model); return model; }
        public int UpdateExpenseType(ErpExpenseType model) => Update(model, it => new { it.TypeName, it.TypeNameAr, it.AccountId, it.Update_by, it.Update_time }, f => f.ExpenseTypeId == model.ExpenseTypeId);
        public int Delete(long[] ids) => Context.Updateable<ErpExpenseType>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.ExpenseTypeId)).ExecuteCommand();
        public List<ErpExpenseType> GetActiveExpenseTypes() => Queryable().Where(x => x.IsActive == true).ToList();
    }

}

