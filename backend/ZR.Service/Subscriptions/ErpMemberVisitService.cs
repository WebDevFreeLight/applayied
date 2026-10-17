using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Subscriptions.IService;

namespace ZR.Service.Subscriptions
{
    /// <summary>
    /// Member Visit Service - خدمة زيارات الأعضاء
    /// </summary>
    [AppService(ServiceType = typeof(IErpMemberVisitService), ServiceLifetime = LifeTime.Transient)]
    public class ErpMemberVisitService : BaseService<ErpMemberVisit>, IErpMemberVisitService
    {
        public PagedInfo<ErpMemberVisit> GetList(ErpMemberVisit parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpMemberVisit>();

            predicate = predicate.AndIF(parm.MemberId > 0, it => it.MemberId == parm.MemberId);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CheckInMethod), it => it.CheckInMethod == parm.CheckInMethod);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.CheckInTime)
                .ToPage(pager);

            return response;
        }

        public ErpMemberVisit GetInfo(long visitId)
        {
            return GetFirst(x => x.VisitId == visitId);
        }

        public ErpMemberVisit CheckIn(long memberId, long branchId, string checkInMethod, string deviceId = null)
        {
            return Context.Ado.UseTran(() =>
            {
                // Check if member has active visit
                var activeVisit = GetActiveVisit(memberId);
                if (activeVisit != null)
                {
                    throw new Exception("العضو متواجد بالفعل - Member is already checked in");
                }

                // Validate member access
                if (!ValidateMemberAccess(memberId))
                {
                    throw new Exception("العضو غير مصرح له بالدخول - Member access not authorized");
                }

                var visit = new ErpMemberVisit
                {
                    VisitId = SnowFlakeSingle.Instance.NextId(),
                    MemberId = memberId,
                    BranchID = branchId,
                    CheckInTime = DateTime.Now,
                    CheckInMethod = checkInMethod,
                    DeviceId = string.IsNullOrEmpty(deviceId) ? null : long.TryParse(deviceId, out var devId) ? devId : null,
                    Status = "active",
                    Create_time = DateTime.Now
                };

                Context.Insertable(visit).ExecuteCommand();

                // Update member last visit
                Context.Updateable<ErpMember>()
                    .SetColumns(it => new ErpMember
                    {
                        LastVisitDate = DateTime.Now,
                        TotalVisits = it.TotalVisits + 1,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.MemberId == memberId)
                    .ExecuteCommand();

                return visit;
            }).Data;
        }

        public int CheckOut(long visitId, string checkOutMethod)
        {
            return Context.Ado.UseTran(() =>
            {
                var visit = GetInfo(visitId);
                if (visit == null || visit.Status != "active")
                    return 0;

                var duration = (int)(DateTime.Now - visit.CheckInTime).TotalMinutes;

                return Context.Updateable<ErpMemberVisit>()
                    .SetColumns(it => new ErpMemberVisit
                    {
                        CheckOutTime = DateTime.Now,
                        CheckOutMethod = checkOutMethod,
                        DurationMinutes = duration,
                        Status = "completed",
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.VisitId == visitId)
                    .ExecuteCommand();
            }).Data;
        }

        public ErpMemberVisit GetActiveVisit(long memberId)
        {
            return GetFirst(x => x.MemberId == memberId && x.Status == "active");
        }

        public List<ErpMemberVisit> GetMemberVisits(long memberId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.MemberId == memberId)
                .WhereIF(fromDate.HasValue, x => x.CheckInTime >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.CheckInTime <= toDate.Value)
                .OrderByDescending(x => x.CheckInTime)
                .ToList();
        }

        public List<ErpMemberVisit> GetTodayVisits(long? branchId = null)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return Queryable()
                .Where(x => x.CheckInTime >= today && x.CheckInTime < tomorrow)
                .WhereIF(branchId.HasValue, x => x.BranchID == branchId)
                .OrderByDescending(x => x.CheckInTime)
                .ToList();
        }

        public int GetCurrentOccupancy(long branchId)
        {
            return Count(x => x.BranchID == branchId && x.Status == "active");
        }

        public object GetVisitStatistics(long memberId)
        {
            var visits = GetMemberVisits(memberId, null, null);

            var thisMonth = DateTime.Today.AddDays(-30);
            var thisWeek = DateTime.Today.AddDays(-7);

            return new
            {
                TotalVisits = visits.Count,
                VisitsThisMonth = visits.Count(x => x.CheckInTime >= thisMonth),
                VisitsThisWeek = visits.Count(x => x.CheckInTime >= thisWeek),
                AverageDuration = visits.Where(x => x.DurationMinutes.HasValue).Average(x => x.DurationMinutes) ?? 0,
                TotalDurationHours = (visits.Where(x => x.DurationMinutes.HasValue).Sum(x => x.DurationMinutes) ?? 0) / 60,
                LastVisit = visits.FirstOrDefault()?.CheckInTime,
                MostVisitedBranch = visits.GroupBy(x => x.BranchID)
                    .OrderByDescending(g => g.Count())
                    .Select(g => new { BranchId = g.Key, Count = g.Count() })
                    .FirstOrDefault(),
                PreferredTime = GetPreferredVisitTime(visits)
            };
        }

        public object GetBranchStatistics(long branchId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1);

            var visits = Queryable()
                .Where(x => x.BranchID == branchId &&
                           x.CheckInTime >= startOfDay &&
                           x.CheckInTime < endOfDay)
                .ToList();

            var hourlyDistribution = visits
                .GroupBy(x => x.CheckInTime.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderBy(x => x.Hour)
                .ToList();

            return new
            {
                Date = date.Date,
                BranchId = branchId,
                TotalVisits = visits.Count,
                UniqueMembers = visits.Select(x => x.MemberId).Distinct().Count(),
                CurrentOccupancy = GetCurrentOccupancy(branchId),
                AverageDuration = visits.Where(x => x.DurationMinutes.HasValue).Average(x => x.DurationMinutes) ?? 0,
                PeakHour = hourlyDistribution.OrderByDescending(x => x.Count).FirstOrDefault()?.Hour,
                HourlyDistribution = hourlyDistribution,
                CheckInMethods = visits.GroupBy(x => x.CheckInMethod)
                    .Select(g => new { Method = g.Key, Count = g.Count() })
                    .ToList()
            };
        }

        public List<object> GetPeakHours(long branchId, DateTime fromDate, DateTime toDate)
        {
            return Queryable()
                .Where(x => x.BranchID == branchId &&
                           x.CheckInTime >= fromDate &&
                           x.CheckInTime <= toDate)
                .GroupBy(x => x.CheckInTime.Hour)
                .Select(x => new
                {
                    Hour = x.CheckInTime.Hour,
                    AverageVisits = SqlFunc.AggregateCount(x.VisitId)
                })
                .OrderBy(x => x.Hour)
                .ToList()
                .Cast<object>()
                .ToList();
        }

        public List<ErpMemberVisit> GetInactiveMembers(int daysInactive, long? branchId = null)
        {
            var cutoffDate = DateTime.Today.AddDays(-daysInactive);

            // Get members whose last visit is before cutoff date
            var memberIds = Queryable()
                .WhereIF(branchId.HasValue, x => x.BranchID == branchId)
                .GroupBy(x => x.MemberId)
                .Having(x => SqlFunc.AggregateMax(x.CheckInTime) < cutoffDate)
                .Select(x => x.MemberId)
                .ToList();

            return Queryable()
                .Where(x => memberIds.Contains(x.MemberId))
                .ToList();
        }

        public bool ValidateMemberAccess(long memberId)
        {
            // Check if member has active subscription
            var hasActiveSubscription = Context.Queryable<ErpSubscription>()
                .Where(x => x.MemberId == memberId &&
                           x.Status == "active" &&
                           x.EndDate >= DateTime.Today &&
                           x.IsActive == true)
                .Any();

            if (!hasActiveSubscription)
                return false;

            // Check if subscription is frozen
            var isFrozen = Context.Queryable<ErpSubscriptionFreeze>()
                .Where(x => x.MemberId == memberId &&
                           x.Status == "active" &&
                           x.StartDate <= DateTime.Today &&
                           x.EndDate >= DateTime.Today)
                .Any();

            return !isFrozen;
        }

        #region Private Methods

        private string GetPreferredVisitTime(List<ErpMemberVisit> visits)
        {
            if (!visits.Any()) return "N/A";

            var hourCounts = visits.GroupBy(x => x.CheckInTime.Hour)
                .OrderByDescending(g => g.Count())
                .First();

            var hour = hourCounts.Key;
            return hour < 12 ? $"{hour}:00 AM - صباحاً" :
                   hour == 12 ? "12:00 PM - ظهراً" :
                   $"{hour - 12}:00 PM - مساءً";
        }

        #endregion
    }
}

