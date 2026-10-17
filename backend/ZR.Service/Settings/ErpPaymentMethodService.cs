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


    [AppService(ServiceType = typeof(IErpPaymentMethodService), ServiceLifetime = LifeTime.Transient)]
    public class ErpPaymentMethodService : BaseService<ErpPaymentMethod>, IErpPaymentMethodService
    {
    

        public PagedInfo<ErpPaymentMethod> GetList(ErpPaymentMethod parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpPaymentMethod>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.MethodName), it => it.MethodName.Contains(parm.MethodName));
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.SortOrder).ToPage(pager);
        }

        public ErpPaymentMethod GetInfo(long paymentMethodId) => GetFirst(x => x.PaymentMethodId == paymentMethodId);
        public ErpPaymentMethod AddPaymentMethod(ErpPaymentMethod model) { model.PaymentMethodId = SnowFlakeSingle.Instance.NextId(); Insert(model); return model; }
        public int UpdatePaymentMethod(ErpPaymentMethod model) => Update(model, it => new { it.MethodName, it.MethodNameAr, it.MethodType, it.AccountId, it.SortOrder, it.Update_by, it.Update_time }, f => f.PaymentMethodId == model.PaymentMethodId);
        public int Delete(long[] ids) => Context.Updateable<ErpPaymentMethod>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.PaymentMethodId)).ExecuteCommand();
        public List<ErpPaymentMethod> GetActivePaymentMethods() => Queryable().Where(x => x.IsActive == true).OrderBy(x => x.SortOrder).ToList();
    }

}

