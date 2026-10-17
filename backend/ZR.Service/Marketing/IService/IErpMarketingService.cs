using ZR.Model;
using ZR.Model.Marketing;
using ZR.Service;

namespace ZR.Service.Marketing.IService
{
    public interface IErpGiftCardService : IBaseService<ErpGiftCard>
    {
        PagedInfo<ErpGiftCard> GetList(ErpGiftCard parm, PagerInfo pager);
        ErpGiftCard GetInfo(long giftCardId);
        ErpGiftCard CreateGiftCard(ErpGiftCard model);
        int ActivateGiftCard(long giftCardId);
        int DeactivateGiftCard(long giftCardId);
        ErpGiftCard GetByCode(string cardCode);
        decimal UseGiftCard(string cardCode, decimal amount, long referenceId, string referenceType);
        decimal GetGiftCardBalance(string cardCode);
        List<ErpGiftCard> GetExpiringSoonCards(int days = 30);
        List<object> GetGiftCardReport(DateTime fromDate, DateTime toDate);
    }

    public interface IErpPromoCodeService : IBaseService<ErpPromoCode>
    {
        PagedInfo<ErpPromoCode> GetList(ErpPromoCode parm, PagerInfo pager);
        ErpPromoCode GetInfo(long promoCodeId);
        ErpPromoCode CreatePromoCode(ErpPromoCode model);
        int UpdatePromoCode(ErpPromoCode model);
        int Delete(long[] ids);
        ErpPromoCode GetByCode(string code);
        bool ValidatePromoCode(string code, long? customerId, decimal orderAmount);
        decimal ApplyPromoCode(string code, decimal orderAmount);
        int RecordUsage(long promoCodeId, long customerId, long referenceId);
        List<ErpPromoCode> GetActivePromoCodes();
        List<object> GetPromoCodeUsageReport(DateTime fromDate, DateTime toDate);
    }

}

