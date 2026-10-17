using Infrastructure.Attribute;
using ZR.Model.Sales;
using ZR.Model.Sales.Dto;
using ZR.Repository;
using ZR.Service.Sales.IService;

namespace ZR.Service.Sales
{
    /// <summary>
    /// Customer Service - خدمة العملاء
    /// </summary>
    [AppService(ServiceType = typeof(IErpCustomerService), ServiceLifetime = LifeTime.Transient)]
    public class ErpCustomerService : BaseService<ErpCustomer>, IErpCustomerService
    {
        /// <summary>
        /// Get customer list with pagination
        /// </summary>
        public PagedInfo<ErpCustomerDto> GetList(ErpCustomerQueryDto parm)
        {
            var predicate = Expressionable.Create<ErpCustomer>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CustomerCode),
                it => it.CustomerCode.Contains(parm.CustomerCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CustomerName),
                it => it.CustomerName.Contains(parm.CustomerName) || it.CustomerNameAr.Contains(parm.CustomerName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CustomerType),
                it => it.CustomerType == parm.CustomerType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Phone),
                it => it.Phone.Contains(parm.Phone) || it.Mobile.Contains(parm.Phone));
            predicate = predicate.AndIF(parm.IsActive.HasValue,
                it => it.IsActive == parm.IsActive);
            predicate = predicate.AndIF(parm.BeginCreateTime != null,
                it => it.Create_time >= parm.BeginCreateTime);
            predicate = predicate.AndIF(parm.EndCreateTime != null,
                it => it.Create_time <= parm.EndCreateTime);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<ErpCustomer, ErpCustomerDto>(parm);

            return response;
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        public ErpCustomer GetInfo(long customerId)
        {
            return GetFirst(x => x.CustomerId == customerId);
        }

        /// <summary>
        /// Add new customer
        /// </summary>
        public ErpCustomer AddCustomer(ErpCustomer model)
        {
            model.CustomerId = SnowFlakeSingle.Instance.NextId();
            return Insertable(model).ExecuteReturnEntity();
        }

        /// <summary>
        /// Update customer
        /// </summary>
        public int UpdateCustomer(ErpCustomer model)
        {
            return Update(model, true);
        }

        /// <summary>
        /// Delete customers
        /// </summary>
        public int Delete(long[] ids)
        {
            return Delete(ids);
        }

        /// <summary>
        /// Get customer balance summary
        /// </summary>
        public ErpCustomerBalanceSummaryDto GetBalanceSummary(long customerId)
        {
            var customer = GetInfo(customerId);
            if (customer == null) return null;

            // Get transaction counts from customer_balances table
            var balances = Context.Queryable<ErpCustomerBalance>()
                .Where(x => x.CustomerId == customerId)
                .ToList();

            return new ErpCustomerBalanceSummaryDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                TotalDebit = balances.Sum(x => x.DebitAmount),
                TotalCredit = balances.Sum(x => x.CreditAmount),
                CurrentBalance = customer.CurrentBalance,
                CreditLimit = customer.CreditLimit,
                AvailableCredit = customer.CreditLimit - customer.CurrentBalance,
                TotalInvoices = balances.Count(x => x.TransactionType == "invoice"),
                TotalPayments = balances.Count(x => x.TransactionType == "payment"),
                LastTransactionDate = balances.OrderByDescending(x => x.TransactionDate).FirstOrDefault()?.TransactionDate
            };
        }

        /// <summary>
        /// Get customer statement (كشف حساب)
        /// </summary>
        public PagedInfo<ErpCustomerBalance> GetStatement(long customerId, DateTime? fromDate, DateTime? toDate, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpCustomerBalance>();
            predicate = predicate.And(x => x.CustomerId == customerId);
            predicate = predicate.AndIF(fromDate.HasValue, x => x.TransactionDate >= fromDate);
            predicate = predicate.AndIF(toDate.HasValue, x => x.TransactionDate <= toDate);

            return Context.Queryable<ErpCustomerBalance>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.TransactionDate)
                .ToPage(pager);
        }

        /// <summary>
        /// Check if customer code exists
        /// </summary>
        public bool IsCustomerCodeExists(string customerCode, long? excludeId = null)
        {
            return Queryable()
                .Where(x => x.CustomerCode == customerCode)
                .WhereIF(excludeId.HasValue, x => x.CustomerId != excludeId)
                .Any();
        }

        /// <summary>
        /// Get customers with due balances
        /// </summary>
        public List<ErpCustomerBalanceSummaryDto> GetCustomersWithDueBalances()
        {
            return Queryable()
                .Where(x => x.CurrentBalance > 0 && x.IsActive)
                .OrderByDescending(x => x.CurrentBalance)
                .Select(x => new ErpCustomerBalanceSummaryDto
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    CurrentBalance = x.CurrentBalance,
                    CreditLimit = x.CreditLimit,
                    AvailableCredit = x.CreditLimit - x.CurrentBalance
                })
                .ToList();
        }

        /// <summary>
        /// Update customer balance
        /// </summary>
        public int UpdateCustomerBalance(long customerId, decimal amount, string transactionType)
        {
            var customer = GetInfo(customerId);
            if (customer == null) return 0;

            // Invoice increases balance (debit), Payment decreases balance (credit)
            if (transactionType == "invoice" || transactionType == "debit")
            {
                customer.CurrentBalance += amount;
            }
            else if (transactionType == "payment" || transactionType == "credit" || transactionType == "return")
            {
                customer.CurrentBalance -= amount;
            }

            return Update(customer, true, "CurrentBalance");
        }

        /// <summary>
        /// AI Agent: Analyze customer patterns
        /// </summary>
        public async Task<object> AnalyzeCustomerPatternsAsync(long customerId)
        {
            // This would integrate with AI Agent service
            var customer = GetInfo(customerId);
            var balances = Context.Queryable<ErpCustomerBalance>()
                .Where(x => x.CustomerId == customerId)
                .OrderBy(x => x.TransactionDate)
                .ToList();

            // Basic pattern analysis
            return await Task.FromResult(new
            {
                CustomerId = customerId,
                CustomerName = customer?.CustomerName,
                TotalTransactions = balances.Count,
                AverageTransactionAmount = balances.Any() ? balances.Average(x => x.DebitAmount + x.CreditAmount) : 0,
                TransactionFrequency = balances.Count > 1
                    ? (balances.Max(x => x.TransactionDate) - balances.Min(x => x.TransactionDate)).TotalDays / balances.Count
                    : 0,
                TopTransactionType = balances.GroupBy(x => x.TransactionType)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key,
                Analysis = "Pattern analysis completed by AI Agent"
            });
        }

        /// <summary>
        /// AI Agent: Predict customer churn risk
        /// </summary>
        public async Task<decimal> PredictChurnRiskAsync(long customerId)
        {
            // This would integrate with AI Agent service for ML predictions
            var customer = GetInfo(customerId);
            if (customer == null) return 0;

            // Basic risk calculation based on activity
            var lastTransaction = Context.Queryable<ErpCustomerBalance>()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.TransactionDate)
                .First();

            if (lastTransaction == null) return 0.8m; // High risk if no transactions

            var daysSinceLastTransaction = (DateTime.Now - lastTransaction.TransactionDate).TotalDays;

            // Simple risk calculation
            decimal risk = 0;
            if (daysSinceLastTransaction > 90) risk = 0.8m;
            else if (daysSinceLastTransaction > 60) risk = 0.5m;
            else if (daysSinceLastTransaction > 30) risk = 0.3m;
            else risk = 0.1m;

            return await Task.FromResult(risk);
        }

        /// <summary>
        /// AI Agent: Get customer recommendations
        /// </summary>
        public async Task<List<object>> GetRecommendationsAsync(long customerId)
        {
            // This would integrate with AI Agent service for personalized recommendations
            var recommendations = new List<object>();

            var customer = GetInfo(customerId);
            if (customer == null) return recommendations;

            // Basic recommendations based on customer data
            if (customer.CurrentBalance > customer.CreditLimit * 0.8m)
            {
                recommendations.Add(new { Type = "Payment", Message = "يُنصح بسداد جزء من الرصيد المستحق" });
            }

            if (customer.CreditLimit == 0)
            {
                recommendations.Add(new { Type = "CreditLimit", Message = "يمكن طلب حد ائتماني لتسهيل المعاملات" });
            }

            return await Task.FromResult(recommendations);
        }
    }
}

