using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Service;

namespace ZR.Service.Subscriptions.IService
{
    /// <summary>
    /// Resource Service Interface - خدمة الموارد
    /// </summary>
    public interface IErpResourceService : IBaseService<ErpResource>
    {
        PagedInfo<ErpResource> GetList(ErpResource parm, PagerInfo pager);
        ErpResource GetInfo(long resourceId);
        ErpResource AddResource(ErpResource model);
        int UpdateResource(ErpResource model);
        int Delete(long[] ids);
        List<ErpResource> GetActiveResources(long? BranchID = null);
        List<ErpResource> GetResourcesByType(string resourceType);
        List<ErpResource> GetAvailableResources(DateTime startTime, DateTime endTime, long? BranchID = null);
        bool IsResourceAvailable(long resourceId, DateTime startTime, DateTime endTime);
        List<object> GetResourceSchedule(long resourceId, DateTime date);
        List<object> GetResourceUtilization(long resourceId, DateTime fromDate, DateTime toDate);
        int UpdateResourceStatus(long resourceId, bool isBookable);
        decimal GetResourceRevenue(long resourceId, DateTime fromDate, DateTime toDate);
    }
}

