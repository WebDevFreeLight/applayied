using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Service;

namespace ZR.Service.Subscriptions.IService
{
    /// <summary>
    /// Subscription Service Interface - خدمة الاشتراكات
    /// </summary>
    public interface IErpSubscriptionService : IBaseService<ErpSubscription>
    {
        PagedInfo<ErpSubscription> GetList(ErpSubscription parm, PagerInfo pager);
        ErpSubscription GetInfo(long subscriptionId);
        ErpSubscription CreateSubscription(ErpSubscription model);
        int UpdateSubscription(ErpSubscription model);
        int Delete(long[] ids);
        string GenerateSubscriptionNumber(long? BranchID);
        List<ErpSubscription> GetMemberSubscriptions(long memberId);
        ErpSubscription GetActiveSubscription(long memberId);
        int ActivateSubscription(long subscriptionId);
        int SuspendSubscription(long subscriptionId, string reason);
        int CancelSubscription(long subscriptionId, string reason);
        int RenewSubscription(long subscriptionId, int additionalDays);
        int FreezeSubscription(long subscriptionId, DateTime freezeStart, DateTime freezeEnd, string reason);
        int UnfreezeSubscription(long subscriptionId);
        List<ErpSubscriptionFreeze> GetFreezeHistory(long subscriptionId);
        List<ErpSubscription> GetExpiringSubscriptions(int days = 7, long? BranchID = null);
        List<ErpSubscription> GetExpiredSubscriptions(long? BranchID = null);
        int RecordVisit(long subscriptionId);
        int GetRemainingVisits(long subscriptionId);
        int GetRemainingDays(long subscriptionId);
        decimal CalculateRefundAmount(long subscriptionId);
        List<object> GetSubscriptionsByPackage(DateTime fromDate, DateTime toDate, long? BranchID = null);
        decimal GetTotalSubscriptionRevenue(DateTime fromDate, DateTime toDate, long? BranchID = null);
    }
}

