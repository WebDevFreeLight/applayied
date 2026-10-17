using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Marketing;
using ZR.Repository;
using ZR.Service.Marketing.IService;

namespace ZR.Service.Marketing
{
    [AppService(ServiceType = typeof(IErpGiftCardService), ServiceLifetime = LifeTime.Transient)]
    public class ErpGiftCardService : BaseService<ErpGiftCard>, IErpGiftCardService
    {
       

        public PagedInfo<ErpGiftCard> GetList(ErpGiftCard parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpGiftCard>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CardCode), it => it.CardCode.Contains(parm.CardCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.Create_time).ToPage(pager);
        }

        public ErpGiftCard GetInfo(long giftCardId) => GetFirst(x => x.GiftCardId == giftCardId);
        public ErpGiftCard GetByCode(string cardCode) => GetFirst(x => x.CardCode == cardCode && x.IsActive == true);

        public ErpGiftCard CreateGiftCard(ErpGiftCard model)
        {
            model.GiftCardId = SnowFlakeSingle.Instance.NextId();
            if (string.IsNullOrEmpty(model.CardCode)) model.CardCode = GenerateCardCode();
            model.CurrentBalance = model.InitialValue;
            model.Status = "active";
            Insert(model);
            return model;
        }

        private string GenerateCardCode() => $"GC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        public int ActivateGiftCard(long giftCardId) => Context.Updateable<ErpGiftCard>().SetColumns(it => new ErpGiftCard { Status = "active", ActivationDate = DateTime.Now, Update_time = DateTime.Now }).Where(it => it.GiftCardId == giftCardId).ExecuteCommand();
        public int DeactivateGiftCard(long giftCardId) => Context.Updateable<ErpGiftCard>().SetColumns(it => new ErpGiftCard { Status = "inactive", Update_time = DateTime.Now }).Where(it => it.GiftCardId == giftCardId).ExecuteCommand();

        public decimal UseGiftCard(string cardCode, decimal amount, long referenceId, string referenceType)
        {
            var card = GetByCode(cardCode);
            if (card == null || card.Status != "active" || card.CurrentBalance <= 0) return 0;
            if (card.ExpiryDate.HasValue && card.ExpiryDate < DateTime.Today) return 0;

            var useAmount = Math.Min(amount, card.CurrentBalance);
            Context.Updateable<ErpGiftCard>()
                .SetColumns(it => new ErpGiftCard { CurrentBalance = it.CurrentBalance - useAmount, RedeemedDate = DateTime.Now, Update_time = DateTime.Now })
                .Where(it => it.GiftCardId == card.GiftCardId).ExecuteCommand();
            return useAmount;
        }

        public decimal GetGiftCardBalance(string cardCode) => GetByCode(cardCode)?.CurrentBalance ?? 0;
        public List<ErpGiftCard> GetExpiringSoonCards(int days = 30) => Queryable().Where(x => x.ExpiryDate <= DateTime.Today.AddDays(days) && x.ExpiryDate > DateTime.Today && x.Status == "active" && x.CurrentBalance > 0).ToList();

        public List<object> GetGiftCardReport(DateTime fromDate, DateTime toDate)
        {
            return Queryable().Where(x => x.Create_time >= fromDate && x.Create_time <= toDate)
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Status, Count = SqlFunc.AggregateCount(x.GiftCardId), TotalAmount = SqlFunc.AggregateSum(x.InitialValue), TotalBalance = SqlFunc.AggregateSum(x.CurrentBalance) })
                .ToList().Cast<object>().ToList();
        }
    }

 
}

