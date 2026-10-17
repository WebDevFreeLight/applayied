using ZR.Model.Sales;
using ZR.Model.Sales.Dto;

namespace ZR.Service.Sales.IService
{
    /// <summary>
    /// Customer Service Interface - واجهة خدمة العملاء
    /// </summary>
    public interface IErpCustomerService : IBaseService<ErpCustomer>
    {
        /// <summary>
        /// Get customer list with pagination
        /// </summary>
        PagedInfo<ErpCustomerDto> GetList(ErpCustomerQueryDto parm);

        /// <summary>
        /// Get customer by ID
        /// </summary>
        ErpCustomer GetInfo(long customerId);

        /// <summary>
        /// Add new customer
        /// </summary>
        ErpCustomer AddCustomer(ErpCustomer model);

        /// <summary>
        /// Update customer
        /// </summary>
        int UpdateCustomer(ErpCustomer model);

        /// <summary>
        /// Delete customers
        /// </summary>
        int Delete(long[] ids);

        /// <summary>
        /// Get customer balance summary
        /// </summary>
        ErpCustomerBalanceSummaryDto GetBalanceSummary(long customerId);

        /// <summary>
        /// Get customer statement (كشف حساب)
        /// </summary>
        PagedInfo<ErpCustomerBalance> GetStatement(long customerId, DateTime? fromDate, DateTime? toDate, PagerInfo pager);

        /// <summary>
        /// Check if customer code exists
        /// </summary>
        bool IsCustomerCodeExists(string customerCode, long? excludeId = null);

        /// <summary>
        /// Get customers with due balances
        /// </summary>
        List<ErpCustomerBalanceSummaryDto> GetCustomersWithDueBalances();

        /// <summary>
        /// Update customer balance
        /// </summary>
        int UpdateCustomerBalance(long customerId, decimal amount, string transactionType);

        /// <summary>
        /// AI Agent: Analyze customer patterns
        /// </summary>
        Task<object> AnalyzeCustomerPatternsAsync(long customerId);

        /// <summary>
        /// AI Agent: Predict customer churn risk
        /// </summary>
        Task<decimal> PredictChurnRiskAsync(long customerId);

        /// <summary>
        /// AI Agent: Get customer recommendations
        /// </summary>
        Task<List<object>> GetRecommendationsAsync(long customerId);
    }
}

