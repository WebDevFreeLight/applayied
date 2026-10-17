using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Sales;
using ZR.Repository;
using ZR.Service.Sales.IService;

namespace ZR.Service.Sales
{
    /// <summary>
    /// Sales Return Service - خدمة مرتجعات المبيعات
    /// </summary>
    [AppService(ServiceType = typeof(IErpSalesReturnService), ServiceLifetime = LifeTime.Transient)]
    public class ErpSalesReturnService : BaseService<ErpSalesReturn>, IErpSalesReturnService
    {
       


        public PagedInfo<ErpSalesReturn> GetList(ErpSalesReturn parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpSalesReturn>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ReturnNumber), it => it.ReturnNumber.Contains(parm.ReturnNumber));
            predicate = predicate.AndIF(parm.CustomerId > 0, it => it.CustomerId == parm.CustomerId);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.And(it => it.IsActive == true);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.ReturnDate)
                .ToPage(pager);

            return response;
        }

        public ErpSalesReturn GetInfo(long returnId)
        {
            return GetFirst(x => x.ReturnId == returnId);
        }

        public ErpSalesReturn CreateReturn(ErpSalesReturn returnData, List<ErpSalesReturnItem> items)
        {
            return Context.Ado.UseTran(() =>
            {
                if (string.IsNullOrEmpty(returnData.ReturnNumber))
                {
                    returnData.ReturnNumber = GenerateReturnNumber(returnData.BranchID);
                }

                returnData.ReturnId = SnowFlakeSingle.Instance.NextId();
                returnData.Status = "draft";

                Context.Insertable(returnData).ExecuteCommand();

                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        item.ItemId = SnowFlakeSingle.Instance.NextId();
                        item.ReturnId = returnData.ReturnId;
                    }
                    Context.Insertable(items).ExecuteCommand();
                }

                CalculateAndUpdateTotals(returnData.ReturnId);

                return returnData;
            }).Data;
        }

        public int UpdateReturn(ErpSalesReturn returnData)
        {
            return Update(returnData, it => new
            {
                it.ReturnDate,
                it.CustomerId,
                it.CustomerName,
                it.Reason,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.ReturnId == returnData.ReturnId);
        }

        public int Delete(long[] ids)
        {
            return Context.Ado.UseTran(() =>
            {
                // Delete items associated with the return
                Context.Deleteable<ErpSalesReturnItem>()
                    .Where(it => ids.Contains(it.ReturnId))
                    .ExecuteCommand();

                return Context.Updateable<ErpSalesReturn>()
                    .SetColumns(it => it.IsActive == false)
                    .Where(it => ids.Contains(it.ReturnId))
                    .ExecuteCommand();
            }).Data;
        }

        public int ConfirmReturn(long returnId, string userId)
        {
            return Context.Ado.UseTran(() =>
            {
                var returnData = GetInfo(returnId);
                if (returnData == null || returnData.Status != "draft")
                    return 0;

                // Update customer balance (credit)
                if (returnData.CustomerId.HasValue && returnData.TotalAmount > 0)
                {
                    var balance = new ErpCustomerBalance
                    {
                        BalanceId = SnowFlakeSingle.Instance.NextId(),
                        CustomerId = returnData.CustomerId.Value,
                        TransactionType = "return",
                        DebitAmount = 0,
                        CreditAmount = returnData.TotalAmount,
                        TransactionDate = DateTime.Now,
                        ReferenceId = returnData.ReturnId,
                        ReferenceType = "return",
                        Create_time = DateTime.Now
                    };
                    Context.Insertable(balance).ExecuteCommand();
                }

                // Return items to inventory
                var items = GetReturnItems(returnId);
                foreach (var item in items)
                {
                    // Add inventory transaction for return
                    // This should be handled by inventory service
                }

                return Context.Updateable<ErpSalesReturn>()
                    .SetColumns(it => new ErpSalesReturn
                    {
                        Status = "confirmed",
                        Update_by = userId,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.ReturnId == returnId)
                    .ExecuteCommand();
            }).Data;
        }

        public int CancelReturn(long returnId, string reason)
        {
            return Context.Updateable<ErpSalesReturn>()
                .SetColumns(it => new ErpSalesReturn
                {
                    Status = "cancelled",
                    Notes = reason,
                    Update_time = DateTime.Now
                })
                .Where(it => it.ReturnId == returnId && it.Status == "draft")
                .ExecuteCommand();
        }

        public string GenerateReturnNumber(long? BranchID)
        {
            var prefix = BranchID.HasValue ? $"RET-{BranchID}-" : "RET-";
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = Count(x => x.ReturnNumber.StartsWith(prefix + date)) + 1;
            return $"{prefix}{date}-{count:D4}";
        }

        public List<ErpSalesReturnItem> GetReturnItems(long returnId)
        {
            return Context.Queryable<ErpSalesReturnItem>()
                .Where(x => x.ReturnId == returnId)
                .ToList();
        }

        public int AddReturnItem(ErpSalesReturnItem item)
        {
            item.ItemId = SnowFlakeSingle.Instance.NextId();
            var result = Context.Insertable(item).ExecuteCommand();
            CalculateAndUpdateTotals(item.ReturnId);
            return result;
        }

        public int UpdateReturnItem(ErpSalesReturnItem item)
        {
            var result = Context.Updateable(item)
                .UpdateColumns(it => new
                {
                    it.ProductId,
                    it.ItemName,
                    it.Quantity,
                    it.UnitPrice,
                    it.ReturnReason,
                    it.Notes
                })
                .Where(it => it.ItemId == item.ItemId)
                .ExecuteCommand();
            CalculateAndUpdateTotals(item.ReturnId);
            return result;
        }

        public int DeleteReturnItem(long itemId)
        {
            var item = Context.Queryable<ErpSalesReturnItem>().First(x => x.ItemId == itemId);
            if (item == null) return 0;

            var result = Context.Deleteable<ErpSalesReturnItem>()
                .Where(it => it.ItemId == itemId)
                .ExecuteCommand();
            CalculateAndUpdateTotals(item.ReturnId);
            return result;
        }

        public decimal CalculateReturnTotal(long returnId)
        {
            var items = GetReturnItems(returnId);
            return items.Sum(x => x.Total);
        }

        public List<ErpSalesReturn> GetCustomerReturns(long customerId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId && x.IsActive == true)
                .WhereIF(fromDate.HasValue, x => x.ReturnDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.ReturnDate <= toDate.Value)
                .OrderByDescending(x => x.ReturnDate)
                .ToList();
        }

        public List<ErpSalesReturn> GetReturnsByInvoice(long invoiceId)
        {
            return Queryable()
                .Where(x => x.OriginalInvoiceId == invoiceId && x.IsActive == true)
                .OrderByDescending(x => x.ReturnDate)
                .ToList();
        }

        public decimal GetTotalReturns(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.ReturnDate >= fromDate && x.ReturnDate <= toDate)
                .Where(x => x.Status == "confirmed" && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .Sum(x => x.TotalAmount);
        }

        public int ProcessRefund(long returnId, long paymentMethodId, long? treasuryId)
        {
            return Context.Ado.UseTran(() =>
            {
                var returnData = GetInfo(returnId);
                if (returnData == null || returnData.Status != "confirmed")
                    return 0;

                // Create refund payment voucher or update treasury
                if (treasuryId.HasValue)
                {
                    // Deduct from treasury
                }

                return Context.Updateable<ErpSalesReturn>()
                    .SetColumns(it => new ErpSalesReturn
                    {
                        RefundMethod = paymentMethodId.ToString(),
                        RefundedAmount = it.TotalAmount,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.ReturnId == returnId)
                    .ExecuteCommand();
            }).Data;
        }

        #region Private Methods

        private void CalculateAndUpdateTotals(long returnId)
        {
            var items = GetReturnItems(returnId);
            var subtotal = items.Sum(x => x.Quantity * x.UnitPrice);
            var vatAmount = items.Sum(x => x.VatAmount);
            var total = subtotal + vatAmount;

            Context.Updateable<ErpSalesReturn>()
                .SetColumns(it => new ErpSalesReturn
                {
                    Subtotal = subtotal,
                    VatAmount = vatAmount,
                    TotalAmount = total
                })
                .Where(it => it.ReturnId == returnId)
                .ExecuteCommand();
        }

        #endregion
    }
}

