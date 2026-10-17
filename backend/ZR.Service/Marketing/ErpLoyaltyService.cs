using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Marketing;
using ZR.Model.Subscriptions;
using ZR.Repository;
using ZR.Service.Marketing.IService;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Loyalty Service - خدمة الولاء
    /// </summary>
    [AppService(ServiceType = typeof(IErpLoyaltyService), ServiceLifetime = LifeTime.Transient)]
    public class ErpLoyaltyService : BaseService<ErpLoyaltyProgram>, IErpLoyaltyService
    {
        #region Program Management

        public PagedInfo<ErpLoyaltyProgram> GetPrograms(PagerInfo pager)
        {
            return Queryable()
                .OrderByDescending(x => x.Create_time)
                .ToPage(pager);
        }

        public ErpLoyaltyProgram GetProgramInfo(long programId)
        {
            return GetFirst(x => x.ProgramId == programId);
        }

        public int CreateProgram(ErpLoyaltyProgram program)
        {
            program.ProgramId = SnowFlakeSingle.Instance.NextId();
            program.Create_time = DateTime.Now;
            return Insert(program);
        }

        public int UpdateProgram(ErpLoyaltyProgram program)
        {
            return Update(program, it => new
            {
                it.ProgramName,
                it.ProgramNameAr,
                it.Description,
                it.PointsPerAmount,
                it.AmountPerPoint,
                it.MinimumRedemptionPoints,
                it.PointsExpiryDays,
                it.IsActive,
                it.Update_by,
                it.Update_time
            }, f => f.ProgramId == program.ProgramId);
        }

        public int DeleteProgram(long programId)
        {
            return Delete(programId);
        }

        #endregion

        #region Tier Management

        public List<ErpLoyaltyTier> GetTiers(long programId)
        {
            return Context.Queryable<ErpLoyaltyTier>()
                .Where(x => x.ProgramId == programId)
                .OrderBy(x => x.SortOrder)
                .ToList();
        }

        public ErpLoyaltyTier GetTierInfo(long tierId)
        {
            return Context.Queryable<ErpLoyaltyTier>()
                .First(x => x.TierId == tierId);
        }

        public int CreateTier(ErpLoyaltyTier tier)
        {
            tier.TierId = SnowFlakeSingle.Instance.NextId();
            tier.Create_time = DateTime.Now;
            return Context.Insertable(tier).ExecuteCommand();
        }

        public int UpdateTier(ErpLoyaltyTier tier)
        {
            return Context.Updateable(tier)
                .UpdateColumns(it => new
                {
                    it.TierName,
                    it.TierNameAr,
                    it.MinimumPoints,
                    it.DiscountPercent,
                    it.PointsMultiplier,
                    it.HasPriorityBooking,
                    it.FreeSessionsPerMonth,
                    it.Color,
                    it.Icon,
                    it.SortOrder,
                    it.Update_by,
                    it.Update_time
                })
                .Where(f => f.TierId == tier.TierId)
                .ExecuteCommand();
        }

        public int DeleteTier(long tierId)
        {
            return Context.Deleteable<ErpLoyaltyTier>()
                .Where(x => x.TierId == tierId)
                .ExecuteCommand();
        }

        #endregion

        #region Member Loyalty

        public PagedInfo<ErpMemberLoyalty> GetMemberLoyalties(ErpMemberLoyalty parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpMemberLoyalty>();

            predicate = predicate.AndIF(parm.MemberId > 0, it => it.MemberId == parm.MemberId);
            predicate = predicate.AndIF(parm.ProgramId > 0, it => it.ProgramId == parm.ProgramId);
            predicate = predicate.AndIF(parm.CurrentTierId > 0, it => it.CurrentTierId == parm.CurrentTierId);

            return Context.Queryable<ErpMemberLoyalty>()
                .Where(predicate.ToExpression())
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CurrentBalance)
                .ToPage(pager);
        }

        public ErpMemberLoyalty GetMemberLoyalty(long memberId)
        {
            return Context.Queryable<ErpMemberLoyalty>()
                .First(x => x.MemberId == memberId && x.IsActive);
        }

        public int EnrollMember(long memberId, long programId)
        {
            // Check if already enrolled
            var existing = GetMemberLoyalty(memberId);
            if (existing != null)
                return 0;

            // Get default tier (lowest)
            var defaultTier = Context.Queryable<ErpLoyaltyTier>()
                .Where(x => x.ProgramId == programId)
                .OrderBy(x => x.MinimumPoints)
                .First();

            var memberLoyalty = new ErpMemberLoyalty
            {
                Id = SnowFlakeSingle.Instance.NextId(),
                MemberId = memberId,
                ProgramId = programId,
                CurrentTierId = defaultTier?.TierId,
                TotalPointsEarned = 0,
                TotalPointsRedeemed = 0,
                CurrentBalance = 0,
                EnrollmentDate = DateTime.Now,
                IsActive = true,
                Create_time = DateTime.Now
            };

            return Context.Insertable(memberLoyalty).ExecuteCommand();
        }

        public int UnenrollMember(long memberId)
        {
            return Context.Updateable<ErpMemberLoyalty>()
                .SetColumns(it => new ErpMemberLoyalty
                {
                    IsActive = false,
                    Update_time = DateTime.Now
                })
                .Where(it => it.MemberId == memberId)
                .ExecuteCommand();
        }

        #endregion

        #region Points Management

        public int EarnPoints(long memberId, int points, string referenceType, long? referenceId, decimal? amount, string description)
        {
            return Context.Ado.UseTran(() =>
            {
                var memberLoyalty = GetMemberLoyalty(memberId);
                if (memberLoyalty == null)
                    return 0;

                // Get tier multiplier
                var tier = Context.Queryable<ErpLoyaltyTier>()
                    .First(x => x.TierId == memberLoyalty.CurrentTierId);
                var multiplier = tier?.PointsMultiplier ?? 1;

                var earnedPoints = (int)(points * multiplier);

                // Get program for expiry
                var program = GetProgramInfo(memberLoyalty.ProgramId);
                var expiryDate = DateTime.Now.AddDays(program?.PointsExpiryDays ?? 365);

                // Create transaction
                var transaction = new ErpLoyaltyTransaction
                {
                    TransactionId = SnowFlakeSingle.Instance.NextId(),
                    MemberId = memberId,
                    TransactionType = "earn",
                    Points = earnedPoints,
                    BalanceAfter = memberLoyalty.CurrentBalance + earnedPoints,
                    ReferenceType = referenceType,
                    ReferenceId = referenceId,
                    Amount = amount,
                    Description = description ?? $"Earned {earnedPoints} points",
                    TransactionDate = DateTime.Now,
                    ExpiryDate = expiryDate,
                    Create_time = DateTime.Now
                };

                Context.Insertable(transaction).ExecuteCommand();

                // Update member loyalty
                Context.Updateable<ErpMemberLoyalty>()
                    .SetColumns(it => new ErpMemberLoyalty
                    {
                        TotalPointsEarned = memberLoyalty.TotalPointsEarned + earnedPoints,
                        CurrentBalance = memberLoyalty.CurrentBalance + earnedPoints,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.MemberId == memberId)
                    .ExecuteCommand();

                // Check and update tier
                UpdateMemberTier(memberId);

                return earnedPoints;
            }).Data;
        }

        public int RedeemPoints(long memberId, int points, string referenceType, long? referenceId, string description)
        {
            return Context.Ado.UseTran(() =>
            {
                var memberLoyalty = GetMemberLoyalty(memberId);
                if (memberLoyalty == null || memberLoyalty.CurrentBalance < points)
                    return 0;

                // Get program for minimum redemption
                var program = GetProgramInfo(memberLoyalty.ProgramId);
                if (program != null && points < program.MinimumRedemptionPoints)
                    return 0;

                // Create transaction
                var transaction = new ErpLoyaltyTransaction
                {
                    TransactionId = SnowFlakeSingle.Instance.NextId(),
                    MemberId = memberId,
                    TransactionType = "redeem",
                    Points = -points,
                    BalanceAfter = memberLoyalty.CurrentBalance - points,
                    ReferenceType = referenceType,
                    ReferenceId = referenceId,
                    Description = description ?? $"Redeemed {points} points",
                    TransactionDate = DateTime.Now,
                    Create_time = DateTime.Now
                };

                Context.Insertable(transaction).ExecuteCommand();

                // Update member loyalty
                Context.Updateable<ErpMemberLoyalty>()
                    .SetColumns(it => new ErpMemberLoyalty
                    {
                        TotalPointsRedeemed = memberLoyalty.TotalPointsRedeemed + points,
                        CurrentBalance = memberLoyalty.CurrentBalance - points,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.MemberId == memberId)
                    .ExecuteCommand();

                return points;
            }).Data;
        }

        public int AdjustPoints(long memberId, int points, string description)
        {
            return Context.Ado.UseTran(() =>
            {
                var memberLoyalty = GetMemberLoyalty(memberId);
                if (memberLoyalty == null)
                    return 0;

                var newBalance = memberLoyalty.CurrentBalance + points;
                if (newBalance < 0) newBalance = 0;

                // Create transaction
                var transaction = new ErpLoyaltyTransaction
                {
                    TransactionId = SnowFlakeSingle.Instance.NextId(),
                    MemberId = memberId,
                    TransactionType = "adjust",
                    Points = points,
                    BalanceAfter = newBalance,
                    Description = description ?? $"Adjustment: {points} points",
                    TransactionDate = DateTime.Now,
                    Create_time = DateTime.Now
                };

                Context.Insertable(transaction).ExecuteCommand();

                // Update member loyalty
                Context.Updateable<ErpMemberLoyalty>()
                    .SetColumns(it => new ErpMemberLoyalty
                    {
                        CurrentBalance = newBalance,
                        TotalPointsEarned = points > 0 ? memberLoyalty.TotalPointsEarned + points : memberLoyalty.TotalPointsEarned,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.MemberId == memberId)
                    .ExecuteCommand();

                return Math.Abs(points);
            }).Data;
        }

        public int ExpirePoints()
        {
            var expiredTransactions = Context.Queryable<ErpLoyaltyTransaction>()
                .Where(x => x.TransactionType == "earn" &&
                           x.ExpiryDate < DateTime.Now &&
                           !x.IsExpired)
                .ToList();

            var count = 0;
            foreach (var trans in expiredTransactions)
            {
                // Mark transaction as expired
                Context.Updateable<ErpLoyaltyTransaction>()
                    .SetColumns(it => new ErpLoyaltyTransaction { IsExpired = true })
                    .Where(it => it.TransactionId == trans.TransactionId)
                    .ExecuteCommand();

                // Create expiry transaction
                var memberLoyalty = GetMemberLoyalty(trans.MemberId);
                if (memberLoyalty != null && memberLoyalty.CurrentBalance >= trans.Points)
                {
                    var expiryTrans = new ErpLoyaltyTransaction
                    {
                        TransactionId = SnowFlakeSingle.Instance.NextId(),
                        MemberId = trans.MemberId,
                        TransactionType = "expire",
                        Points = -trans.Points,
                        BalanceAfter = memberLoyalty.CurrentBalance - trans.Points,
                        Description = $"Points expired from transaction {trans.TransactionId}",
                        TransactionDate = DateTime.Now,
                        Create_time = DateTime.Now
                    };

                    Context.Insertable(expiryTrans).ExecuteCommand();

                    // Update balance
                    Context.Updateable<ErpMemberLoyalty>()
                        .SetColumns(it => new ErpMemberLoyalty
                        {
                            CurrentBalance = memberLoyalty.CurrentBalance - trans.Points,
                            Update_time = DateTime.Now
                        })
                        .Where(it => it.MemberId == trans.MemberId)
                        .ExecuteCommand();

                    count++;
                }
            }

            return count;
        }

        public int GetMemberBalance(long memberId)
        {
            var memberLoyalty = GetMemberLoyalty(memberId);
            return memberLoyalty?.CurrentBalance ?? 0;
        }

        #endregion

        #region Transactions

        public PagedInfo<ErpLoyaltyTransaction> GetTransactions(ErpLoyaltyTransaction parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpLoyaltyTransaction>();

            predicate = predicate.AndIF(parm.MemberId > 0, it => it.MemberId == parm.MemberId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TransactionType), it => it.TransactionType == parm.TransactionType);

            return Context.Queryable<ErpLoyaltyTransaction>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.TransactionDate)
                .ToPage(pager);
        }

        public List<ErpLoyaltyTransaction> GetMemberTransactions(long memberId, DateTime? fromDate, DateTime? toDate)
        {
            return Context.Queryable<ErpLoyaltyTransaction>()
                .Where(x => x.MemberId == memberId)
                .WhereIF(fromDate.HasValue, x => x.TransactionDate >= fromDate)
                .WhereIF(toDate.HasValue, x => x.TransactionDate <= toDate)
                .OrderByDescending(x => x.TransactionDate)
                .ToList();
        }

        #endregion

        #region Tier Assignment

        public ErpLoyaltyTier CalculateMemberTier(long memberId)
        {
            var memberLoyalty = GetMemberLoyalty(memberId);
            if (memberLoyalty == null) return null;

            var tiers = GetTiers(memberLoyalty.ProgramId);

            // Find highest tier the member qualifies for
            return tiers
                .Where(t => t.MinimumPoints <= memberLoyalty.TotalPointsEarned)
                .OrderByDescending(t => t.MinimumPoints)
                .FirstOrDefault();
        }

        public int UpdateMemberTier(long memberId)
        {
            var newTier = CalculateMemberTier(memberId);
            if (newTier == null) return 0;

            return Context.Updateable<ErpMemberLoyalty>()
                .SetColumns(it => new ErpMemberLoyalty
                {
                    CurrentTierId = newTier.TierId,
                    Update_time = DateTime.Now
                })
                .Where(it => it.MemberId == memberId)
                .ExecuteCommand();
        }

        #endregion

        #region Reports

        public object GetLoyaltyDashboard()
        {
            var totalMembers = Context.Queryable<ErpMemberLoyalty>()
                .Where(x => x.IsActive)
                .Count();

            var totalPointsEarned = Context.Queryable<ErpMemberLoyalty>()
                .Where(x => x.IsActive)
                .Sum(x => x.TotalPointsEarned);

            var totalPointsRedeemed = Context.Queryable<ErpMemberLoyalty>()
                .Where(x => x.IsActive)
                .Sum(x => x.TotalPointsRedeemed);

            var currentBalances = Context.Queryable<ErpMemberLoyalty>()
                .Where(x => x.IsActive)
                .Sum(x => x.CurrentBalance);

            var tierDistribution = Context.Queryable<ErpMemberLoyalty>()
                .LeftJoin<ErpLoyaltyTier>((m, t) => m.CurrentTierId == t.TierId)
                .Where(m => m.IsActive)
                .GroupBy((m, t) => new { t.TierName })
                .Select((m, t) => new { TierName = t.TierName ?? "None", Count = SqlFunc.AggregateCount(m.Id) })
                .ToList();

            return new
            {
                TotalMembers = totalMembers,
                TotalPointsEarned = totalPointsEarned,
                TotalPointsRedeemed = totalPointsRedeemed,
                CurrentBalances = currentBalances,
                RedemptionRate = totalPointsEarned > 0 ? (decimal)totalPointsRedeemed / totalPointsEarned * 100 : 0,
                TierDistribution = tierDistribution
            };
        }

        public object GetPointsReport(DateTime fromDate, DateTime toDate)
        {
            var transactions = Context.Queryable<ErpLoyaltyTransaction>()
                .Where(x => x.TransactionDate >= fromDate && x.TransactionDate <= toDate)
                .ToList();

            return new
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalEarned = transactions.Where(t => t.TransactionType == "earn").Sum(t => t.Points),
                TotalRedeemed = Math.Abs(transactions.Where(t => t.TransactionType == "redeem").Sum(t => t.Points)),
                TotalExpired = Math.Abs(transactions.Where(t => t.TransactionType == "expire").Sum(t => t.Points)),
                TotalAdjusted = transactions.Where(t => t.TransactionType == "adjust").Sum(t => t.Points),
                TransactionCount = transactions.Count,
                DailyBreakdown = transactions
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Earned = g.Where(t => t.TransactionType == "earn").Sum(t => t.Points),
                        Redeemed = Math.Abs(g.Where(t => t.TransactionType == "redeem").Sum(t => t.Points))
                    })
                    .OrderBy(x => x.Date)
                    .ToList()
            };
        }

        public List<ErpMemberLoyalty> GetTopMembers(int count)
        {
            return Context.Queryable<ErpMemberLoyalty>()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.TotalPointsEarned)
                .Take(count)
                .ToList();
        }

        public List<ErpMemberLoyalty> GetMembersWithExpiringPoints(int days)
        {
            var expiryDate = DateTime.Now.AddDays(days);

            var memberIds = Context.Queryable<ErpLoyaltyTransaction>()
                .Where(x => x.TransactionType == "earn" &&
                           x.ExpiryDate <= expiryDate &&
                           !x.IsExpired)
                .Select(x => x.MemberId)
                .Distinct()
                .ToList();

            return Context.Queryable<ErpMemberLoyalty>()
                .Where(x => memberIds.Contains(x.MemberId) && x.IsActive)
                .ToList();
        }

        #endregion
    }
}

