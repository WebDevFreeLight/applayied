using ZR.Model;
using ZR.Model.Marketing;
using ZR.Service;

namespace ZR.Service.Marketing.IService
{
    /// <summary>
    /// Loyalty Service Interface - خدمة الولاء
    /// </summary>
    public interface IErpLoyaltyService : IBaseService<ErpLoyaltyProgram>
    {
        // Program Management
        PagedInfo<ErpLoyaltyProgram> GetPrograms(PagerInfo pager);
        ErpLoyaltyProgram GetProgramInfo(long programId);
        int CreateProgram(ErpLoyaltyProgram program);
        int UpdateProgram(ErpLoyaltyProgram program);
        int DeleteProgram(long programId);

        // Tier Management
        List<ErpLoyaltyTier> GetTiers(long programId);
        ErpLoyaltyTier GetTierInfo(long tierId);
        int CreateTier(ErpLoyaltyTier tier);
        int UpdateTier(ErpLoyaltyTier tier);
        int DeleteTier(long tierId);

        // Member Loyalty
        PagedInfo<ErpMemberLoyalty> GetMemberLoyalties(ErpMemberLoyalty parm, PagerInfo pager);
        ErpMemberLoyalty GetMemberLoyalty(long memberId);
        int EnrollMember(long memberId, long programId);
        int UnenrollMember(long memberId);

        // Points Management
        int EarnPoints(long memberId, int points, string referenceType, long? referenceId, decimal? amount, string description);
        int RedeemPoints(long memberId, int points, string referenceType, long? referenceId, string description);
        int AdjustPoints(long memberId, int points, string description);
        int ExpirePoints();
        int GetMemberBalance(long memberId);

        // Transactions
        PagedInfo<ErpLoyaltyTransaction> GetTransactions(ErpLoyaltyTransaction parm, PagerInfo pager);
        List<ErpLoyaltyTransaction> GetMemberTransactions(long memberId, DateTime? fromDate, DateTime? toDate);

        // Tier Assignment
        ErpLoyaltyTier CalculateMemberTier(long memberId);
        int UpdateMemberTier(long memberId);

        // Reports
        object GetLoyaltyDashboard();
        object GetPointsReport(DateTime fromDate, DateTime toDate);
        List<ErpMemberLoyalty> GetTopMembers(int count);
        List<ErpMemberLoyalty> GetMembersWithExpiringPoints(int days);
    }
}

