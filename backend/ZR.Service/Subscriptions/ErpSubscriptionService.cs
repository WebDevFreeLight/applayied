using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Subscriptions.IService;

namespace ZR.Service.Subscriptions
{
    /// <summary>
    /// Subscription Service - خدمة الاشتراكات
    /// </summary>
    [AppService(ServiceType = typeof(IErpSubscriptionService), ServiceLifetime = LifeTime.Transient)]
    public class ErpSubscriptionService : BaseService<ErpSubscription>, IErpSubscriptionService
    {
     



        public PagedInfo<ErpSubscription> GetList(ErpSubscription parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpSubscription>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.SubscriptionNumber), it => it.SubscriptionNumber.Contains(parm.SubscriptionNumber));
            predicate = predicate.AndIF(parm.MemberId > 0, it => it.MemberId == parm.MemberId);
            predicate = predicate.AndIF(parm.PackageId > 0, it => it.PackageId == parm.PackageId);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage(pager);
        }

        public ErpSubscription GetInfo(long subscriptionId)
        {
            return GetFirst(x => x.SubscriptionId == subscriptionId);
        }

        public ErpSubscription CreateSubscription(ErpSubscription model)
        {
            return Context.Ado.UseTran(() =>
            {
                if (string.IsNullOrEmpty(model.SubscriptionNumber))
                {
                    model.SubscriptionNumber = GenerateSubscriptionNumber(model.BranchID);
                }

                model.SubscriptionId = SnowFlakeSingle.Instance.NextId();
                model.Status = "active";
                model.RemainingVisits = model.MaxVisits;

                Context.Insertable(model).ExecuteCommand();

                // Update member - mark as active (IsBlacklisted = false)
                Context.Updateable<ErpMember>()
                    .SetColumns(it => new ErpMember
                    {
                        IsActive = true,
                        IsBlacklisted = false,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.MemberId == model.MemberId)
                    .ExecuteCommand();

                return model;
            }).Data;
        }

        public int UpdateSubscription(ErpSubscription model)
        {
            return Update(model, it => new
            {
                it.EndDate,
                it.MaxVisits,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.SubscriptionId == model.SubscriptionId);
        }

        public int Delete(long[] ids)
        {
            return Context.Updateable<ErpSubscription>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.SubscriptionId))
                .ExecuteCommand();
        }

        public string GenerateSubscriptionNumber(long? BranchID)
        {
            var prefix = BranchID.HasValue ? $"SUB-{BranchID}-" : "SUB-";
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = Count(x => x.SubscriptionNumber.StartsWith(prefix + date)) + 1;
            return $"{prefix}{date}-{count:D4}";
        }

        public List<ErpSubscription> GetMemberSubscriptions(long memberId)
        {
            return Queryable()
                .Where(x => x.MemberId == memberId && x.IsActive == true)
                .OrderByDescending(x => x.StartDate)
                .ToList();
        }

        public ErpSubscription GetActiveSubscription(long memberId)
        {
            return GetFirst(x => x.MemberId == memberId && x.Status == "active" && x.IsActive == true);
        }

        public int ActivateSubscription(long subscriptionId)
        {
            return Context.Updateable<ErpSubscription>()
                .SetColumns(it => new ErpSubscription
                {
                    Status = "active",
                    Update_time = DateTime.Now
                })
                .Where(it => it.SubscriptionId == subscriptionId)
                .ExecuteCommand();
        }

        public int SuspendSubscription(long subscriptionId, string reason)
        {
            return Context.Updateable<ErpSubscription>()
                .SetColumns(it => new ErpSubscription
                {
                    Status = "suspended",
                    Notes = reason,
                    Update_time = DateTime.Now
                })
                .Where(it => it.SubscriptionId == subscriptionId)
                .ExecuteCommand();
        }

        public int CancelSubscription(long subscriptionId, string reason)
        {
            return Context.Ado.UseTran(() =>
            {
                var subscription = GetInfo(subscriptionId);
                if (subscription == null) return 0;

                Context.Updateable<ErpSubscription>()
                    .SetColumns(it => new ErpSubscription
                    {
                        Status = "cancelled",
                        Notes = reason,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.SubscriptionId == subscriptionId)
                    .ExecuteCommand();

                // Check if member has other active subscriptions
                var otherActive = Count(x => x.MemberId == subscription.MemberId && x.Status == "active" && x.SubscriptionId != subscriptionId);
                if (otherActive == 0)
                {
                    // No other active subscriptions - member is now inactive (can keep IsActive = true for record keeping)
                    Context.Updateable<ErpMember>()
                        .SetColumns(it => new ErpMember
                        {
                            Update_time = DateTime.Now
                        })
                        .Where(it => it.MemberId == subscription.MemberId)
                        .ExecuteCommand();
                }

                return 1;
            }).Data;
        }

        public int RenewSubscription(long subscriptionId, int additionalDays)
        {
            return Context.Updateable<ErpSubscription>()
                .SetColumns(it => new ErpSubscription
                {
                    EndDate = it.EndDate.AddDays(additionalDays),
                    Update_time = DateTime.Now
                })
                .Where(it => it.SubscriptionId == subscriptionId)
                .ExecuteCommand();
        }

        public int FreezeSubscription(long subscriptionId, DateTime freezeStart, DateTime freezeEnd, string reason)
        {
            return Context.Ado.UseTran(() =>
            {
                var subscription = GetInfo(subscriptionId);
                if (subscription == null) return 0;

                var freeze = new ErpSubscriptionFreeze
                {
                    FreezeId = SnowFlakeSingle.Instance.NextId(),
                    SubscriptionId = subscriptionId,
                    MemberId = subscription.MemberId,
                    StartDate = freezeStart,
                    EndDate = freezeEnd,
                    Reason = reason,
                    RequestedDays = (int)(freezeEnd - freezeStart).TotalDays,
                    Status = "active",
                    Create_time = DateTime.Now
                };
                Context.Insertable(freeze).ExecuteCommand();

                return Context.Updateable<ErpSubscription>()
                    .SetColumns(it => new ErpSubscription
                    {
                        Status = "frozen",
                        IsCurrentlyFrozen = true,
                        FreezeStartDate = freezeStart,
                        FreezeEndDate = freezeEnd,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.SubscriptionId == subscriptionId)
                    .ExecuteCommand();
            }).Data;
        }

        public int UnfreezeSubscription(long subscriptionId)
        {
            return Context.Ado.UseTran(() =>
            {
                var freeze = Context.Queryable<ErpSubscriptionFreeze>()
                    .Where(x => x.SubscriptionId == subscriptionId && x.Status == "active")
                    .First();

                if (freeze != null)
                {
                    var actualDays = (int)(DateTime.Now - freeze.StartDate).TotalDays;

                    Context.Updateable<ErpSubscriptionFreeze>()
                        .SetColumns(it => new ErpSubscriptionFreeze
                        {
                            Status = "completed",
                            EndDate = DateTime.Now,
                            UnfrozenDate = DateTime.Now,
                            ActualDays = actualDays,
                            Update_time = DateTime.Now
                        })
                        .Where(it => it.FreezeId == freeze.FreezeId)
                        .ExecuteCommand();

                    return Context.Updateable<ErpSubscription>()
                        .SetColumns(it => new ErpSubscription
                        {
                            Status = "active",
                            IsCurrentlyFrozen = false,
                            FreezeStartDate = null,
                            FreezeEndDate = null,
                            EndDate = it.EndDate.AddDays(actualDays),
                            UsedFreezeDays = it.UsedFreezeDays + actualDays,
                            Update_time = DateTime.Now
                        })
                        .Where(it => it.SubscriptionId == subscriptionId)
                        .ExecuteCommand();
                }

                return 0;
            }).Data;
        }

        public List<ErpSubscriptionFreeze> GetFreezeHistory(long subscriptionId)
        {
            return Context.Queryable<ErpSubscriptionFreeze>()
                .Where(x => x.SubscriptionId == subscriptionId)
                .OrderByDescending(x => x.StartDate)
                .ToList();
        }

        public List<ErpSubscription> GetExpiringSubscriptions(int days = 7, long? BranchID = null)
        {
            var expiryDate = DateTime.Today.AddDays(days);
            return Queryable()
                .Where(x => x.EndDate <= expiryDate && x.EndDate >= DateTime.Today)
                .Where(x => x.Status == "active" && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .ToList();
        }

        public List<ErpSubscription> GetExpiredSubscriptions(long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.EndDate < DateTime.Today)
                .Where(x => x.Status == "active" && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .ToList();
        }

        public int RecordVisit(long subscriptionId)
        {
            return Context.Updateable<ErpSubscription>()
                .SetColumns(it => new ErpSubscription
                {
                    UsedVisits = it.UsedVisits + 1,
                    RemainingVisits = it.RemainingVisits - 1,
                    Update_time = DateTime.Now
                })
                .Where(it => it.SubscriptionId == subscriptionId && it.RemainingVisits > 0)
                .ExecuteCommand();
        }

        public int GetRemainingVisits(long subscriptionId)
        {
            var subscription = GetInfo(subscriptionId);
            return subscription?.RemainingVisits ?? 0;
        }

        public int GetRemainingDays(long subscriptionId)
        {
            var subscription = GetInfo(subscriptionId);
            if (subscription == null) return 0;
            return Math.Max(0, (subscription.EndDate - DateTime.Today).Days);
        }

        public decimal CalculateRefundAmount(long subscriptionId)
        {
            var subscription = GetInfo(subscriptionId);
            if (subscription == null) return 0;

            var totalDays = (subscription.EndDate - subscription.StartDate).Days;
            var remainingDays = GetRemainingDays(subscriptionId);

            if (totalDays <= 0) return 0;

            var dailyRate = subscription.NetAmount / totalDays;
            return dailyRate * remainingDays;
        }

        public List<object> GetSubscriptionsByPackage(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.StartDate >= fromDate && x.StartDate <= toDate)
                .Where(x => x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .GroupBy(x => new { x.PackageId })
                .Select(x => new
                {
                    x.PackageId,
                    Count = SqlFunc.AggregateCount(x.SubscriptionId),
                    TotalAmount = SqlFunc.AggregateSum(x.NetAmount)
                })
                .ToList()
                .Cast<object>()
                .ToList();
        }

        public decimal GetTotalSubscriptionRevenue(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.StartDate >= fromDate && x.StartDate <= toDate)
                .Where(x => x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .Sum(x => x.NetAmount);
        }
    }
}

