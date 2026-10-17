using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Subscriptions.IService;

namespace ZR.Service.Subscriptions
{
    /// <summary>
    /// Member Service - خدمة الأعضاء
    /// </summary>
    [AppService(ServiceType = typeof(IErpMemberService), ServiceLifetime = LifeTime.Transient)]
    public class ErpMemberService : BaseService<ErpMember>, IErpMemberService
    {
        

        public PagedInfo<ErpMember> GetList(ErpMember parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpMember>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.MemberCode), it => it.MemberCode.Contains(parm.MemberCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.FullName), it => it.FullName.Contains(parm.FullName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Phone), it => it.Phone.Contains(parm.Phone));
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.AndIF(parm.MemberGroupId > 0, it => it.MemberGroupId == parm.MemberGroupId);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage(pager);
        }

        public ErpMember GetInfo(long memberId)
        {
            return GetFirst(x => x.MemberId == memberId);
        }

        public ErpMember AddMember(ErpMember model)
        {
            if (string.IsNullOrEmpty(model.MemberCode))
            {
                model.MemberCode = GenerateMemberCode(model.BranchID);
            }

            model.MemberId = SnowFlakeSingle.Instance.NextId();
            model.MembershipStartDate = DateTime.Now;

            Insert(model);
            return model;
        }

        public int UpdateMember(ErpMember model)
        {
            return Update(model, it => new
            {
                it.FullName,
                it.FullNameAr,
                it.Phone,
                it.Email,
                it.Gender,
                it.BirthDate,
                it.NationalId,
                it.Address,
                it.EmergencyContact,
                it.EmergencyPhone,
                it.MemberGroupId,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.MemberId == model.MemberId);
        }

        public int Delete(long[] ids)
        {
            return Context.Updateable<ErpMember>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.MemberId))
                .ExecuteCommand();
        }

        public string GenerateMemberCode(long? BranchID)
        {
            var prefix = BranchID.HasValue ? $"MEM-{BranchID}-" : "MEM-";
            var count = Count(x => x.MemberCode.StartsWith(prefix)) + 1;
            return $"{prefix}{count:D6}";
        }

        public ErpMember GetByCode(string memberCode)
        {
            return GetFirst(x => x.MemberCode == memberCode && x.IsActive == true);
        }

        public ErpMember GetByPhone(string phone)
        {
            return GetFirst(x => x.Phone == phone && x.IsActive == true);
        }

        public ErpMember GetByEmail(string email)
        {
            return GetFirst(x => x.Email == email && x.IsActive == true);
        }

        public List<ErpMemberVisit> GetVisitHistory(long memberId, DateTime? fromDate, DateTime? toDate)
        {
            return Context.Queryable<ErpMemberVisit>()
                .Where(x => x.MemberId == memberId)
                .WhereIF(fromDate.HasValue, x => x.CheckInTime >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.CheckInTime <= toDate.Value)
                .OrderByDescending(x => x.CheckInTime)
                .ToList();
        }

        public int RecordVisit(long memberId, long? BranchID, string checkType)
        {
            var visit = new ErpMemberVisit
            {
                VisitId = SnowFlakeSingle.Instance.NextId(),
                MemberId = memberId,
                BranchID = BranchID,
                CheckInTime = DateTime.Now,
                CheckInMethod = checkType,
                Create_time = DateTime.Now
            };

            // Update member's last visit
            Context.Updateable<ErpMember>()
                .SetColumns(it => new ErpMember
                {
                    LastVisitDate = DateTime.Now,
                    TotalVisits = it.TotalVisits + 1
                })
                .Where(it => it.MemberId == memberId)
                .ExecuteCommand();

            return Context.Insertable(visit).ExecuteCommand();
        }

        public int RecordCheckOut(long memberId)
        {
            // Find the latest visit without checkout
            var visit = Context.Queryable<ErpMemberVisit>()
                .Where(x => x.MemberId == memberId && x.CheckOutTime == null)
                .OrderByDescending(x => x.CheckInTime)
                .First();

            if (visit == null) return 0;

            return Context.Updateable<ErpMemberVisit>()
                .SetColumns(it => new ErpMemberVisit
                {
                    CheckOutTime = DateTime.Now,
                    DurationMinutes = (int)(DateTime.Now - visit.CheckInTime).TotalMinutes
                })
                .Where(it => it.VisitId == visit.VisitId)
                .ExecuteCommand();
        }

        public List<ErpMember> GetActiveMembers(long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.IsActive == true && !x.IsBlacklisted)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .ToList();
        }

        public List<ErpMember> GetExpiringSoonMembers(int days = 7, long? BranchID = null)
        {
            var expiryDate = DateTime.Today.AddDays(days);

            return Context.Queryable<ErpMember, ErpSubscription>((m, s) =>
                    new JoinQueryInfos(JoinType.Inner, m.MemberId == s.MemberId))
                .Where((m, s) => s.EndDate <= expiryDate && s.EndDate >= DateTime.Today)
                .Where((m, s) => s.Status == "active" && m.IsActive == true)
                .WhereIF(BranchID.HasValue, (m, s) => m.BranchID == BranchID)
                .Select((m, s) => m)
                .ToList();
        }

        public List<ErpMember> GetExpiredMembers(long? BranchID = null)
        {
            return Context.Queryable<ErpMember, ErpSubscription>((m, s) =>
                    new JoinQueryInfos(JoinType.Inner, m.MemberId == s.MemberId))
                .Where((m, s) => s.EndDate < DateTime.Today)
                .Where((m, s) => s.Status == "active" && m.IsActive == true)
                .WhereIF(BranchID.HasValue, (m, s) => m.BranchID == BranchID)
                .Select((m, s) => m)
                .Distinct()
                .ToList();
        }

        public int UpdateMemberStatus(long memberId, bool isBlacklisted)
        {
            return Context.Updateable<ErpMember>()
                .SetColumns(it => new ErpMember
                {
                    IsBlacklisted = isBlacklisted,
                    Update_time = DateTime.Now
                })
                .Where(it => it.MemberId == memberId)
                .ExecuteCommand();
        }

        public int FreezeMembership(long memberId, DateTime freezeStart, DateTime freezeEnd, string reason)
        {
            return Context.Ado.UseTran(() =>
            {
                // Update member - mark as having frozen subscription
                Context.Updateable<ErpMember>()
                    .SetColumns(it => new ErpMember
                    {
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.MemberId == memberId)
                    .ExecuteCommand();

                // Get active subscription
                var subscription = Context.Queryable<ErpSubscription>()
                    .Where(x => x.MemberId == memberId && x.Status == "active")
                    .First();

                if (subscription != null)
                {
                    // Create freeze record
                    var freeze = new ErpSubscriptionFreeze
                    {
                        FreezeId = SnowFlakeSingle.Instance.NextId(),
                        SubscriptionId = subscription.SubscriptionId,
                        MemberId = memberId,
                        StartDate = freezeStart,
                        EndDate = freezeEnd,
                        Reason = reason,
                        RequestedDays = (int)(freezeEnd - freezeStart).TotalDays,
                        Status = "active",
                        Create_time = DateTime.Now
                    };
                    Context.Insertable(freeze).ExecuteCommand();

                    // Update subscription
                    return Context.Updateable<ErpSubscription>()
                        .SetColumns(it => new ErpSubscription
                        {
                            Status = "frozen",
                            IsCurrentlyFrozen = true,
                            FreezeStartDate = freezeStart,
                            FreezeEndDate = freezeEnd,
                            Update_time = DateTime.Now
                        })
                        .Where(it => it.SubscriptionId == subscription.SubscriptionId)
                        .ExecuteCommand();
                }

                return 1;
            }).Data;
        }

        public int UnfreezeMembership(long memberId)
        {
            return Context.Ado.UseTran(() =>
            {
                // Update member
                Context.Updateable<ErpMember>()
                    .SetColumns(it => new ErpMember
                    {
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.MemberId == memberId)
                    .ExecuteCommand();

                // Get frozen subscription
                var subscription = Context.Queryable<ErpSubscription>()
                    .Where(x => x.MemberId == memberId && x.Status == "frozen")
                    .First();

                if (subscription != null)
                {
                    // End freeze record
                    var freeze = Context.Queryable<ErpSubscriptionFreeze>()
                        .Where(x => x.SubscriptionId == subscription.SubscriptionId && x.Status == "active")
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
                            .Where(it => it.SubscriptionId == subscription.SubscriptionId)
                            .ExecuteCommand();
                    }
                }

                return 1;
            }).Data;
        }

        public decimal GetMemberBalance(long memberId)
        {
            // This would be calculated from advances minus used amounts
            return 0;
        }

        public List<ErpMember> GetMembersByGroup(long memberGroupId)
        {
            return Queryable()
                .Where(x => x.MemberGroupId == memberGroupId && x.IsActive == true)
                .ToList();
        }

        public int AssignToGroup(long memberId, long memberGroupId)
        {
            return Context.Updateable<ErpMember>()
                .SetColumns(it => new ErpMember
                {
                    MemberGroupId = memberGroupId,
                    Update_time = DateTime.Now
                })
                .Where(it => it.MemberId == memberId)
                .ExecuteCommand();
        }

        public async Task<object> GetMemberAnalyticsAsync(long memberId)
        {
            var member = GetInfo(memberId);
            if (member == null) return null;

            var visits = GetVisitHistory(memberId, DateTime.Today.AddMonths(-12), DateTime.Today);
            var subscriptions = Context.Queryable<ErpSubscription>()
                .Where(x => x.MemberId == memberId)
                .ToList();

            await Task.CompletedTask;

            return new
            {
                member.MemberId,
                member.FullName,
                TotalVisits = member.TotalVisits,
                AverageVisitsPerMonth = visits.Count / 12.0,
                LastVisitDate = member.LastVisitDate,
                TotalSubscriptions = subscriptions.Count,
                ActiveSubscription = subscriptions.FirstOrDefault(x => x.Status == "active"),
                VisitTrend = visits.GroupBy(x => x.CheckInTime.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .ToList()
            };
        }
    }
}

