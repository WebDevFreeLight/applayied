using ZR.Model;
using ZR.Model.Sales;
using ZR.Service;

namespace ZR.Service.Sales.IService
{
    /// <summary>
    /// Customer Balance Service Interface - خدمة أرصدة العملاء
    /// </summary>
    public interface IErpCustomerBalanceService : IBaseService<ErpCustomerBalance>
    {
        PagedInfo<ErpCustomerBalance> GetList(ErpCustomerBalance parm, PagerInfo pager);
        ErpCustomerBalance GetInfo(long balanceId);
        ErpCustomerBalance AddTransaction(ErpCustomerBalance model);
        int Delete(long[] ids);
        List<ErpCustomerBalance> GetCustomerStatement(long customerId, DateTime? fromDate, DateTime? toDate);
        decimal GetCustomerBalance(long customerId);
        decimal GetCustomerTotalDebit(long customerId);
        decimal GetCustomerTotalCredit(long customerId);
        List<object> GetAgingReport(long? customerId = null);
        List<object> GetOverdueBalances(int daysOverdue = 30);
        int RecalculateCustomerBalance(long customerId);
    }
}

