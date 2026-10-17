using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Marketing;
using ZR.Repository;
using ZR.Service.Marketing.IService;

namespace ZR.Service.Marketing
{

    [AppService(ServiceType = typeof(IErpPromoCodeService), ServiceLifetime = LifeTime.Transient)]
    public class ErpPromoCodeService : BaseService<ErpPromoCode>, IErpPromoCodeService
    {

        public PagedInfo<ErpPromoCode> GetList(ErpPromoCode parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpPromoCode>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Code), it => it.Code.Contains(parm.Code));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.DiscountType), it => it.DiscountType == parm.DiscountType);
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.Create_time).ToPage(pager);
        }

        public ErpPromoCode GetInfo(long promoCodeId) => GetFirst(x => x.PromoCodeId == promoCodeId);
        public ErpPromoCode GetByCode(string code) => GetFirst(x => x.Code == code && x.IsActive == true);

        public ErpPromoCode CreatePromoCode(ErpPromoCode model)
        {
            model.PromoCodeId = SnowFlakeSingle.Instance.NextId();
            model.CurrentUsageCount = 0;
            Insert(model);
            return model;
        }

        public int UpdatePromoCode(ErpPromoCode model) => Update(model, it => new { it.Code, it.DiscountType, it.DiscountValue, it.MinimumOrderAmount, it.MaxDiscountAmount, it.MaxUsageCount, it.StartDate, it.EndDate, it.Update_by, it.Update_time }, f => f.PromoCodeId == model.PromoCodeId);
        public int Delete(long[] ids) => Context.Updateable<ErpPromoCode>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.PromoCodeId)).ExecuteCommand();

        public bool ValidatePromoCode(string code, long? customerId, decimal orderAmount)
        {
            var promo = GetByCode(code);
            if (promo == null) return false;
            if (promo.StartDate > DateTime.Today) return false;
            if (promo.EndDate < DateTime.Today) return false;
            if (promo.MaxUsageCount.HasValue && promo.CurrentUsageCount >= promo.MaxUsageCount) return false;
            if (promo.MinimumOrderAmount.HasValue && orderAmount < promo.MinimumOrderAmount) return false;
            return true;
        }

        public decimal ApplyPromoCode(string code, decimal orderAmount)
        {
            var promo = GetByCode(code);
            if (promo == null || !ValidatePromoCode(code, null, orderAmount)) return 0;

            decimal discount = promo.DiscountType == "percent" ? orderAmount * (promo.DiscountValue / 100) : promo.DiscountValue;
            if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount) discount = promo.MaxDiscountAmount.Value;
            return discount;
        }

        public int RecordUsage(long promoCodeId, long customerId, long referenceId) => Context.Updateable<ErpPromoCode>().SetColumns(it => new ErpPromoCode { CurrentUsageCount = it.CurrentUsageCount + 1, Update_time = DateTime.Now }).Where(it => it.PromoCodeId == promoCodeId).ExecuteCommand();
        public List<ErpPromoCode> GetActivePromoCodes() => Queryable().Where(x => x.IsActive == true && x.EndDate >= DateTime.Today).ToList();

        public List<object> GetPromoCodeUsageReport(DateTime fromDate, DateTime toDate)
        {
            return Queryable().Where(x => x.IsActive == true)
                .Select(x => new { x.PromoCodeId, x.Code, x.DiscountType, x.DiscountValue, x.CurrentUsageCount, x.MaxUsageCount })
                .ToList().Cast<object>().ToList();
        }
    }


}

