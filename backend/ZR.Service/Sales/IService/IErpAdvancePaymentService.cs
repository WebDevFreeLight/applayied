using ZR.Model;
using ZR.Model.Sales;
using ZR.Service;

namespace ZR.Service.Sales.IService
{
    /// <summary>
    /// Advance Payment Service Interface - خدمة الدفعات المقدمة
    /// </summary>
    public interface IErpAdvancePaymentService : IBaseService<ErpAdvancePayment>
    {
        PagedInfo<ErpAdvancePayment> GetList(ErpAdvancePayment parm, PagerInfo pager);
        ErpAdvancePayment GetInfo(long advanceId);
        ErpAdvancePayment AddAdvancePayment(ErpAdvancePayment model);
        int UpdateAdvancePayment(ErpAdvancePayment model);
        int Delete(long[] ids);
        string GenerateAdvanceNumber(long? BranchID);
        decimal GetCustomerAdvanceBalance(long customerId);
        List<ErpAdvancePayment> GetCustomerAdvances(long customerId, DateTime? fromDate, DateTime? toDate);
        int UseAdvance(long advanceId, decimal amount, long referenceId, string referenceType);
        int RefundAdvance(long advanceId, decimal amount, string reason);
        List<ErpAdvancePayment> GetUnusedAdvances(long? customerId = null);
        decimal GetTotalAdvances(DateTime fromDate, DateTime toDate, long? BranchID = null);
    }
}

