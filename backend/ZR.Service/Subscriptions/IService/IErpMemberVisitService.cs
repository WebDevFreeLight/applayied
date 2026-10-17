using ZR.Model;
using ZR.Model.Subscriptions;

namespace ZR.Service.Subscriptions.IService
{
    /// <summary>
    /// Member Visit Service Interface - واجهة خدمة زيارات الأعضاء
    /// </summary>
    public interface IErpMemberVisitService : IBaseService<ErpMemberVisit>
    {
        PagedInfo<ErpMemberVisit> GetList(ErpMemberVisit parm, PagerInfo pager);
        ErpMemberVisit GetInfo(long visitId);
        ErpMemberVisit CheckIn(long memberId, long branchId, string checkInMethod, string deviceId = null);
        int CheckOut(long visitId, string checkOutMethod);
        ErpMemberVisit GetActiveVisit(long memberId);
        List<ErpMemberVisit> GetMemberVisits(long memberId, DateTime? fromDate, DateTime? toDate);
        List<ErpMemberVisit> GetTodayVisits(long? branchId = null);
        int GetCurrentOccupancy(long branchId);
        object GetVisitStatistics(long memberId);
        object GetBranchStatistics(long branchId, DateTime date);
        List<object> GetPeakHours(long branchId, DateTime fromDate, DateTime toDate);
        List<ErpMemberVisit> GetInactiveMembers(int daysInactive, long? branchId = null);
        bool ValidateMemberAccess(long memberId);
    }
}

