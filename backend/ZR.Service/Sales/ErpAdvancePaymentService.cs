using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Sales;
using ZR.Repository;
using ZR.Service.Sales.IService;

namespace ZR.Service.Sales
{
    /// <summary>
    /// Advance Payment Service - خدمة الدفعات المقدمة
    /// </summary>
    [AppService(ServiceType = typeof(IErpAdvancePaymentService), ServiceLifetime = LifeTime.Transient)]
    public class ErpAdvancePaymentService : BaseService<ErpAdvancePayment>, IErpAdvancePaymentService
    {
         


        public PagedInfo<ErpAdvancePayment> GetList(ErpAdvancePayment parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpAdvancePayment>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AdvanceNumber), it => it.AdvanceNumber.Contains(parm.AdvanceNumber));
            predicate = predicate.AndIF(parm.CustomerId > 0, it => it.CustomerId == parm.CustomerId);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.And(it => it.IsActive == true);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.PaymentDate)
                .ToPage(pager);

            return response;
        }

        public ErpAdvancePayment GetInfo(long advanceId)
        {
            return GetFirst(x => x.AdvanceId == advanceId);
        }

        public ErpAdvancePayment AddAdvancePayment(ErpAdvancePayment model)
        {
            return Context.Ado.UseTran(() =>
            {
                if (string.IsNullOrEmpty(model.AdvanceNumber))
                {
                    model.AdvanceNumber = GenerateAdvanceNumber(model.BranchID);
                }

                model.AdvanceId = SnowFlakeSingle.Instance.NextId();
                model.RemainingAmount = model.Amount;
                model.UsedAmount = 0;
                model.Status = "active";

                Context.Insertable(model).ExecuteCommand();

                // Add to customer balance (credit - advance received)
                if (model.CustomerId > 0)
                {
                    var balance = new ErpCustomerBalance
                    {
                        BalanceId = SnowFlakeSingle.Instance.NextId(),
                        CustomerId = model.CustomerId,
                        TransactionType = "advance",
                        DebitAmount = 0,
                        CreditAmount = model.Amount,
                        TransactionDate = model.PaymentDate,
                        ReferenceId = model.AdvanceId,
                        ReferenceType = "advance",
                        Description = $"دفعة مقدمة رقم {model.AdvanceNumber}",
                        Create_time = DateTime.Now
                    };
                    Context.Insertable(balance).ExecuteCommand();
                }

                // Add to treasury if cash payment
                if (model.TreasuryId.HasValue)
                {
                    // Add treasury transaction
                }

                return model;
            }).Data;
        }

        public int UpdateAdvancePayment(ErpAdvancePayment model)
        {
            return Update(model, it => new
            {
                it.PaymentDate,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.AdvanceId == model.AdvanceId && f.Status == "active");
        }

        public int Delete(long[] ids)
        {
            return Context.Updateable<ErpAdvancePayment>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.AdvanceId) && it.UsedAmount == 0)
                .ExecuteCommand();
        }

        public string GenerateAdvanceNumber(long? BranchID)
        {
            var prefix = BranchID.HasValue ? $"ADV-{BranchID}-" : "ADV-";
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = Count(x => x.AdvanceNumber.StartsWith(prefix + date)) + 1;
            return $"{prefix}{date}-{count:D4}";
        }

        public decimal GetCustomerAdvanceBalance(long customerId)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId && x.IsActive == true && x.Status == "active")
                .Sum(x => x.RemainingAmount);
        }

        public List<ErpAdvancePayment> GetCustomerAdvances(long customerId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId && x.IsActive == true)
                .WhereIF(fromDate.HasValue, x => x.PaymentDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.PaymentDate <= toDate.Value)
                .OrderByDescending(x => x.PaymentDate)
                .ToList();
        }

        public int UseAdvance(long advanceId, decimal amount, long referenceId, string referenceType)
        {
            return Context.Ado.UseTran(() =>
            {
                var advance = GetInfo(advanceId);
                if (advance == null || advance.RemainingAmount < amount)
                    return 0;

                var newUsedAmount = advance.UsedAmount + amount;
                var newRemainingAmount = advance.Amount - newUsedAmount;
                var newStatus = newRemainingAmount <= 0 ? "used" : "active";

                // Update customer balance (debit - advance used)
                if (advance.CustomerId > 0)
                {
                    var balance = new ErpCustomerBalance
                    {
                        BalanceId = SnowFlakeSingle.Instance.NextId(),
                        CustomerId = advance.CustomerId,
                        TransactionType = "advance_used",
                        DebitAmount = amount,
                        CreditAmount = 0,
                        TransactionDate = DateTime.Now,
                        ReferenceId = referenceId,
                        ReferenceType = referenceType,
                        Description = $"استخدام دفعة مقدمة رقم {advance.AdvanceNumber}",
                        Create_time = DateTime.Now
                    };
                    Context.Insertable(balance).ExecuteCommand();
                }

                return Context.Updateable<ErpAdvancePayment>()
                    .SetColumns(it => new ErpAdvancePayment
                    {
                        UsedAmount = newUsedAmount,
                        RemainingAmount = newRemainingAmount,
                        Status = newStatus,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.AdvanceId == advanceId)
                    .ExecuteCommand();
            }).Data;
        }

        public int RefundAdvance(long advanceId, decimal amount, string reason)
        {
            return Context.Ado.UseTran(() =>
            {
                var advance = GetInfo(advanceId);
                if (advance == null || advance.RemainingAmount < amount)
                    return 0;

                var newRemainingAmount = advance.RemainingAmount - amount;
                var newStatus = newRemainingAmount <= 0 ? "cancelled" : "active";

                // Reverse customer balance (debit - refund)
                if (advance.CustomerId > 0)
                {
                    var balance = new ErpCustomerBalance
                    {
                        BalanceId = SnowFlakeSingle.Instance.NextId(),
                        CustomerId = advance.CustomerId,
                        TransactionType = "advance_refund",
                        DebitAmount = amount,
                        CreditAmount = 0,
                        TransactionDate = DateTime.Now,
                        ReferenceId = advance.AdvanceId,
                        ReferenceType = "advance_refund",
                        Description = $"استرداد دفعة مقدمة رقم {advance.AdvanceNumber} - {reason}",
                        Create_time = DateTime.Now
                    };
                    Context.Insertable(balance).ExecuteCommand();
                }

                return Context.Updateable<ErpAdvancePayment>()
                    .SetColumns(it => new ErpAdvancePayment
                    {
                        RemainingAmount = newRemainingAmount,
                        Status = newStatus,
                        Notes = reason,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.AdvanceId == advanceId)
                    .ExecuteCommand();
            }).Data;
        }

        public List<ErpAdvancePayment> GetUnusedAdvances(long? customerId = null)
        {
            return Queryable()
                .Where(x => x.RemainingAmount > 0 && x.IsActive == true && x.Status == "active")
                .WhereIF(customerId.HasValue, x => x.CustomerId == customerId.Value)
                .OrderBy(x => x.PaymentDate)
                .ToList();
        }

        public decimal GetTotalAdvances(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.PaymentDate >= fromDate && x.PaymentDate <= toDate)
                .Where(x => x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .Sum(x => x.Amount);
        }
    }
}

