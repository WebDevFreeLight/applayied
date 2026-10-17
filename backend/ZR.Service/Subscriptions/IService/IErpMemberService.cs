using ZR.Model;
using ZR.Model.Subscriptions;
using ZR.Service;

namespace ZR.Service.Subscriptions.IService
{
    /// <summary>
    /// Member Service Interface - خدمة الأعضاء
    /// </summary>
    public interface IErpMemberService : IBaseService<ErpMember>
    {
        PagedInfo<ErpMember> GetList(ErpMember parm, PagerInfo pager);
        ErpMember GetInfo(long memberId);
        ErpMember AddMember(ErpMember model);
        int UpdateMember(ErpMember model);
        int Delete(long[] ids);
        string GenerateMemberCode(long? BranchID);
        ErpMember GetByCode(string memberCode);
        ErpMember GetByPhone(string phone);
        ErpMember GetByEmail(string email);
        List<ErpMemberVisit> GetVisitHistory(long memberId, DateTime? fromDate, DateTime? toDate);
        int RecordVisit(long memberId, long? BranchID, string checkType);
        int RecordCheckOut(long memberId);
        List<ErpMember> GetActiveMembers(long? BranchID = null);
        List<ErpMember> GetExpiringSoonMembers(int days = 7, long? BranchID = null);
        List<ErpMember> GetExpiredMembers(long? BranchID = null);
        int UpdateMemberStatus(long memberId, bool isBlacklisted);
        int FreezeMembership(long memberId, DateTime freezeStart, DateTime freezeEnd, string reason);
        int UnfreezeMembership(long memberId);
        decimal GetMemberBalance(long memberId);
        List<ErpMember> GetMembersByGroup(long memberGroupId);
        int AssignToGroup(long memberId, long memberGroupId);
        Task<object> GetMemberAnalyticsAsync(long memberId);
    }
}

