using ZR.Model;
using ZR.Model.Operations;
using ZR.Service;

namespace ZR.Service.Operations
{
    public interface IErpShiftService : IBaseService<ErpShift>
    {
        PagedInfo<ErpShift> GetList(ErpShift parm, PagerInfo pager);
        ErpShift GetInfo(long shiftId);
        ErpShift OpenShift(long employeeId, long treasuryId, decimal openingBalance, long BranchID);
        int CloseShift(long shiftId, decimal closingBalance, string notes);
        ErpShift GetCurrentShift(long employeeId);
        ErpShift GetCurrentShiftByTreasury(long treasuryId);
        List<ErpShift> GetShiftsByDate(DateTime date, long? BranchID = null);
        decimal GetShiftSales(long shiftId);
        decimal GetShiftReceipts(long shiftId);
        decimal GetShiftPayments(long shiftId);
        List<object> GetShiftSummary(long shiftId);
    }

    public interface IErpDailyReportService : IBaseService<ErpDailyReport>
    {
        PagedInfo<ErpDailyReport> GetList(ErpDailyReport parm, PagerInfo pager);
        ErpDailyReport GetInfo(long reportId);
        ErpDailyReport GenerateDailyReport(DateTime reportDate, long? BranchID);
        int CloseDailyReport(long reportId, string userId);
        ErpDailyReport GetReportByDate(DateTime date, long? BranchID);
        List<object> GetMonthlySummary(int year, int month, long? BranchID = null);
        List<object> GetYearlySummary(int year, long? BranchID = null);
    }
}

