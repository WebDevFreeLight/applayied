using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Subscriptions.IService;

namespace ZR.Service.Subscriptions
{
    /// <summary>
    /// Subscription Package Service - خدمة باقات الاشتراك
    /// </summary>
    [AppService(ServiceType = typeof(IErpSubscriptionPackageService), ServiceLifetime = LifeTime.Transient)]
    public class ErpSubscriptionPackageService : BaseService<ErpSubscriptionPackage>, IErpSubscriptionPackageService
    {
        


        public PagedInfo<ErpSubscriptionPackage> GetList(ErpSubscriptionPackage parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpSubscriptionPackage>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.PackageName), it => it.PackageName.Contains(parm.PackageName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.PackageType), it => it.PackageType == parm.PackageType);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.PackageName).ToPage(pager);
        }

        public ErpSubscriptionPackage GetInfo(long packageId) => GetFirst(x => x.PackageId == packageId);

        public ErpSubscriptionPackage AddPackage(ErpSubscriptionPackage model)
        {
            model.PackageId = SnowFlakeSingle.Instance.NextId();
            Insert(model);
            return model;
        }

        public int UpdatePackage(ErpSubscriptionPackage model)
        {
            return Update(model, it => new { it.PackageName, it.PackageNameAr, it.PackageType, it.DurationDays, it.MaxVisits, it.Price, it.FinalPrice, it.DiscountPercent, it.Description, it.AllowedActivities, it.Update_by, it.Update_time }, f => f.PackageId == model.PackageId);
        }

        public int Delete(long[] ids)
        {
            return Context.Updateable<ErpSubscriptionPackage>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.PackageId)).ExecuteCommand();
        }

        public List<ErpSubscriptionPackage> GetActivePackages()
        {
            return Queryable().Where(x => x.IsActive == true).OrderBy(x => x.Price).ToList();
        }

        public List<ErpSubscriptionPackage> GetPackagesByType(string packageType)
        {
            return Queryable().Where(x => x.PackageType == packageType && x.IsActive == true).ToList();
        }

        public List<ErpSubscriptionPackage> GetPackagesByActivity(string activityId)
        {
            return Queryable().Where(x => x.AllowedActivities.Contains(activityId) && x.IsActive == true).ToList();
        }

        public int UpdatePackageStatus(long packageId, bool isActive)
        {
            return Context.Updateable<ErpSubscriptionPackage>().SetColumns(it => new ErpSubscriptionPackage { IsActive = isActive, Update_time = DateTime.Now }).Where(it => it.PackageId == packageId).ExecuteCommand();
        }

        public int GetPackageSalesCount(long packageId, DateTime fromDate, DateTime toDate)
        {
            return Context.Queryable<ErpSubscription>().Where(x => x.PackageId == packageId && x.StartDate >= fromDate && x.StartDate <= toDate && x.IsActive == true).Count();
        }

        public decimal GetPackageRevenue(long packageId, DateTime fromDate, DateTime toDate)
        {
            return Context.Queryable<ErpSubscription>().Where(x => x.PackageId == packageId && x.StartDate >= fromDate && x.StartDate <= toDate && x.IsActive == true).Sum(x => x.NetAmount);
        }

        public List<object> GetTopSellingPackages(DateTime fromDate, DateTime toDate, int count = 10)
        {
            return Context.Queryable<ErpSubscription>()
                .Where(x => x.StartDate >= fromDate && x.StartDate <= toDate && x.IsActive == true)
                .GroupBy(x => new { x.PackageId })
                .Select(x => new { x.PackageId, SalesCount = SqlFunc.AggregateCount(x.SubscriptionId), TotalRevenue = SqlFunc.AggregateSum(x.NetAmount) })
                .OrderByDescending(x => x.SalesCount)
                .Take(count).ToList().Cast<object>().ToList();
        }
    }
}

