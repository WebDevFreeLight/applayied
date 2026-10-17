using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Settings;
using ZR.Repository;
using ZR.Service.Inventory.IService;

namespace ZR.Service.Inventory
{






    [AppService(ServiceType = typeof(IErpUnitOfMeasureService), ServiceLifetime = LifeTime.Transient)]
    public class ErpUnitOfMeasureService : BaseService<ErpUnitOfMeasure>, IErpUnitOfMeasureService
    {
        

        public PagedInfo<ErpUnitOfMeasure> GetList(ErpUnitOfMeasure parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpUnitOfMeasure>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.UnitName), it => it.UnitName.Contains(parm.UnitName));
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.UnitName).ToPage(pager);
        }

        public ErpUnitOfMeasure GetInfo(long unitId) => GetFirst(x => x.UnitId == unitId);
        public ErpUnitOfMeasure AddUnit(ErpUnitOfMeasure model) { model.UnitId = SnowFlakeSingle.Instance.NextId(); Insert(model); return model; }
        public int UpdateUnit(ErpUnitOfMeasure model) => Update(model, it => new { it.UnitName, it.UnitNameAr, it.UnitSymbol, it.Update_by, it.Update_time }, f => f.UnitId == model.UnitId);
        public int Delete(long[] ids) => Context.Updateable<ErpUnitOfMeasure>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.UnitId)).ExecuteCommand();
        public List<ErpUnitOfMeasure> GetActiveUnits() => Queryable().Where(x => x.IsActive == true).ToList();
    }





}

