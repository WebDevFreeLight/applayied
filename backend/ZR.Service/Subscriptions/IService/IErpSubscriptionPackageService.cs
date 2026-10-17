using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Service;

namespace ZR.Service.Subscriptions.IService
{
    /// <summary>
    /// Subscription Package Service Interface - خدمة باقات الاشتراك
    /// </summary>
    public interface IErpSubscriptionPackageService : IBaseService<ErpSubscriptionPackage>
    {
        PagedInfo<ErpSubscriptionPackage> GetList(ErpSubscriptionPackage parm, PagerInfo pager);
        ErpSubscriptionPackage GetInfo(long packageId);
        ErpSubscriptionPackage AddPackage(ErpSubscriptionPackage model);
        int UpdatePackage(ErpSubscriptionPackage model);
        int Delete(long[] ids);
        List<ErpSubscriptionPackage> GetActivePackages();
        List<ErpSubscriptionPackage> GetPackagesByType(string packageType);
        List<ErpSubscriptionPackage> GetPackagesByActivity(string activityId);
        int UpdatePackageStatus(long packageId, bool isActive);
        int GetPackageSalesCount(long packageId, DateTime fromDate, DateTime toDate);
        decimal GetPackageRevenue(long packageId, DateTime fromDate, DateTime toDate);
        List<object> GetTopSellingPackages(DateTime fromDate, DateTime toDate, int count = 10);
    }
}

