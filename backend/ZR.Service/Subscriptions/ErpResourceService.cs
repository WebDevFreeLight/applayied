using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Subscriptions.IService;

namespace ZR.Service.Subscriptions
{
    /// <summary>
    /// Resource Service - خدمة الموارد
    /// </summary>
    [AppService(ServiceType = typeof(IErpResourceService), ServiceLifetime = LifeTime.Transient)]
    public class ErpResourceService : BaseService<ErpResource>, IErpResourceService
    { 





        public PagedInfo<ErpResource> GetList(ErpResource parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpResource>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ResourceName), it => it.ResourceName.Contains(parm.ResourceName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ResourceType), it => it.ResourceType == parm.ResourceType);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.ResourceName).ToPage(pager);
        }

        public ErpResource GetInfo(long resourceId) => GetFirst(x => x.ResourceId == resourceId);

        public ErpResource AddResource(ErpResource model)
        {
            model.ResourceId = SnowFlakeSingle.Instance.NextId();
            Insert(model);
            return model;
        }

        public int UpdateResource(ErpResource model)
        {
            return Update(model, it => new { it.ResourceName, it.ResourceNameAr, it.ResourceType, it.Capacity, it.HourlyRate, it.Description, it.Update_by, it.Update_time }, f => f.ResourceId == model.ResourceId);
        }

        public int Delete(long[] ids)
        {
            return Context.Updateable<ErpResource>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.ResourceId)).ExecuteCommand();
        }

        public List<ErpResource> GetActiveResources(long? BranchID = null)
        {
            return Queryable().Where(x => x.IsActive == true && x.IsBookable == true).WhereIF(BranchID.HasValue, x => x.BranchID == BranchID).ToList();
        }

        public List<ErpResource> GetResourcesByType(string resourceType)
        {
            return Queryable().Where(x => x.ResourceType == resourceType && x.IsActive == true).ToList();
        }

        public List<ErpResource> GetAvailableResources(DateTime startTime, DateTime endTime, long? BranchID = null)
        {
            var bookedResources = Context.Queryable<ErpBooking>()
                .Where(b => b.Status != "cancelled" && b.IsActive == true)
                .Where(b => b.StartTime < endTime && b.EndTime > startTime)
                .Select(b => b.ResourceId).ToList();

            return Queryable().Where(x => x.IsActive == true && x.IsBookable == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .Where(x => !bookedResources.Contains(x.ResourceId)).ToList();
        }

        public bool IsResourceAvailable(long resourceId, DateTime startTime, DateTime endTime)
        {
            return !Context.Queryable<ErpBooking>()
                .Where(b => b.ResourceId == resourceId && b.Status != "cancelled" && b.IsActive == true)
                .Where(b => b.StartTime < endTime && b.EndTime > startTime).Any();
        }

        public List<object> GetResourceSchedule(long resourceId, DateTime date)
        {
            return Context.Queryable<ErpBooking>()
                .Where(x => x.ResourceId == resourceId && x.BookingDate.Date == date.Date && x.Status != "cancelled" && x.IsActive == true)
                .Select(x => new { x.BookingId, x.StartTime, x.EndTime, x.MemberId, x.Status })
                .ToList().Cast<object>().ToList();
        }

        public List<object> GetResourceUtilization(long resourceId, DateTime fromDate, DateTime toDate)
        {
            return Context.Queryable<ErpBooking>()
                .Where(x => x.ResourceId == resourceId && x.BookingDate >= fromDate && x.BookingDate <= toDate && x.IsActive == true)
                .GroupBy(x => x.BookingDate.Date)
                .Select(x => new { Date = x.BookingDate.Date, BookingCount = SqlFunc.AggregateCount(x.BookingId), TotalHours = SqlFunc.AggregateSum(SqlFunc.DateDiff(DateType.Minute, x.StartTime, x.EndTime)) / 60.0 })
                .ToList().Cast<object>().ToList();
        }

        public int UpdateResourceStatus(long resourceId, bool isBookable)
        {
            return Context.Updateable<ErpResource>().SetColumns(it => new ErpResource { IsBookable = isBookable, Update_time = DateTime.Now }).Where(it => it.ResourceId == resourceId).ExecuteCommand();
        }

        public decimal GetResourceRevenue(long resourceId, DateTime fromDate, DateTime toDate)
        {
            return Context.Queryable<ErpBooking>().Where(x => x.ResourceId == resourceId && x.BookingDate >= fromDate && x.BookingDate <= toDate && x.Status == "completed" && x.IsActive == true).Sum(x => x.NetAmount);
        }
    }
}

