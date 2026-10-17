using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Sales;
using ZR.Repository;
using ZR.Service.Sales.IService;

namespace ZR.Service.Sales
{
    /// <summary>
    /// Sales Invoice Service - خدمة فواتير المبيعات
    /// </summary>
    [AppService(ServiceType = typeof(IErpSalesInvoiceService), ServiceLifetime = LifeTime.Transient)]
    public class ErpSalesInvoiceService : BaseService<ErpSalesInvoice>, IErpSalesInvoiceService
    {
      


        public PagedInfo<ErpSalesInvoice> GetList(ErpSalesInvoice parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpSalesInvoice>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.InvoiceNumber), it => it.InvoiceNumber.Contains(parm.InvoiceNumber));
            predicate = predicate.AndIF(parm.CustomerId > 0, it => it.CustomerId == parm.CustomerId);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.InvoiceType), it => it.InvoiceType == parm.InvoiceType);
            predicate = predicate.And(it => it.IsActive == true);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.InvoiceDate)
                .ToPage(pager);

            return response;
        }

        public ErpSalesInvoice GetInfo(long invoiceId)
        {
            return GetFirst(x => x.InvoiceId == invoiceId);
        }

        public ErpSalesInvoice CreateInvoice(ErpSalesInvoice invoice, List<ErpSalesInvoiceItem> items)
        {
            return Context.Ado.UseTran(() =>
            {
                if (string.IsNullOrEmpty(invoice.InvoiceNumber))
                {
                    invoice.InvoiceNumber = GenerateInvoiceNumber(invoice.BranchID);
                }

                invoice.InvoiceId = SnowFlakeSingle.Instance.NextId();
                invoice.Status = "draft";

                Context.Insertable(invoice).ExecuteCommand();

                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        item.ItemId = SnowFlakeSingle.Instance.NextId();
                        item.InvoiceId = invoice.InvoiceId;
                    }
                    Context.Insertable(items).ExecuteCommand();
                }

                // Recalculate totals
                CalculateAndUpdateTotals(invoice.InvoiceId);

                return invoice;
            }).Data;
        }

        public int UpdateInvoice(ErpSalesInvoice invoice)
        {
            return Update(invoice, it => new
            {
                it.InvoiceDate,
                it.CustomerId,
                it.CustomerName,
                it.PaymentMethodId,
                it.TreasuryId,
                it.DiscountAmount,
                it.DiscountPercent,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.InvoiceId == invoice.InvoiceId);
        }

        public int Delete(long[] ids)
        {
            return Context.Ado.UseTran(() =>
            {
                // Delete items associated with the invoices
                Context.Deleteable<ErpSalesInvoiceItem>()
                    .Where(it => ids.Contains(it.InvoiceId))
                    .ExecuteCommand();

                // Soft delete invoices
                return Context.Updateable<ErpSalesInvoice>()
                    .SetColumns(it => it.IsActive == false)
                    .Where(it => ids.Contains(it.InvoiceId))
                    .ExecuteCommand();
            }).Data;
        }

        public int ConfirmInvoice(long invoiceId, string userId)
        {
            return Context.Ado.UseTran(() =>
            {
                var invoice = GetInfo(invoiceId);
                if (invoice == null || invoice.Status != "draft")
                    return 0;

                // Update customer balance
                if (invoice.CustomerId.HasValue && invoice.TotalAmount > 0)
                {
                    UpdateCustomerBalance(invoice.CustomerId.Value, invoice.TotalAmount);
                }

                // Update treasury if paid by cash
                if (invoice.TreasuryId.HasValue && invoice.PaidAmount > 0)
                {
                    UpdateTreasuryBalance(invoice.TreasuryId.Value, invoice.PaidAmount);
                }

                return Context.Updateable<ErpSalesInvoice>()
                    .SetColumns(it => new ErpSalesInvoice
                    {
                        Status = "confirmed",
                        Update_by = userId,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.InvoiceId == invoiceId)
                    .ExecuteCommand();
            }).Data;
        }

        public int CancelInvoice(long invoiceId, string reason)
        {
            return Context.Updateable<ErpSalesInvoice>()
                .SetColumns(it => new ErpSalesInvoice
                {
                    Status = "cancelled",
                    Notes = reason,
                    Update_time = DateTime.Now
                })
                .Where(it => it.InvoiceId == invoiceId && it.Status == "draft")
                .ExecuteCommand();
        }

        public string GenerateInvoiceNumber(long? BranchID)
        {
            var prefix = BranchID.HasValue ? $"INV-{BranchID}-" : "INV-";
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = Count(x => x.InvoiceNumber.StartsWith(prefix + date)) + 1;
            return $"{prefix}{date}-{count:D4}";
        }

        public List<ErpSalesInvoiceItem> GetInvoiceItems(long invoiceId)
        {
            return Context.Queryable<ErpSalesInvoiceItem>()
                .Where(x => x.InvoiceId == invoiceId)
                .ToList();
        }

        public int AddInvoiceItem(ErpSalesInvoiceItem item)
        {
            item.ItemId = SnowFlakeSingle.Instance.NextId();
            var result = Context.Insertable(item).ExecuteCommand();
            CalculateAndUpdateTotals(item.InvoiceId);
            return result;
        }

        public int UpdateInvoiceItem(ErpSalesInvoiceItem item)
        {
            var result = Context.Updateable(item)
                .UpdateColumns(it => new
                {
                    it.ProductId,
                    it.ItemName,
                    it.ItemNameAr,
                    it.Quantity,
                    it.UnitPrice,
                    it.DiscountAmount,
                    it.DiscountPercent,
                    it.VatPercent,
                    it.Notes
                })
                .Where(it => it.ItemId == item.ItemId)
                .ExecuteCommand();
            CalculateAndUpdateTotals(item.InvoiceId);
            return result;
        }

        public int DeleteInvoiceItem(long itemId)
        {
            var item = Context.Queryable<ErpSalesInvoiceItem>().First(x => x.ItemId == itemId);
            if (item == null) return 0;

            var result = Context.Deleteable<ErpSalesInvoiceItem>()
                .Where(it => it.ItemId == itemId)
                .ExecuteCommand();
            CalculateAndUpdateTotals(item.InvoiceId);
            return result;
        }

        public decimal CalculateInvoiceTotal(long invoiceId)
        {
            var items = GetInvoiceItems(invoiceId);
            return items.Sum(x => x.Total);
        }

        public decimal CalculateInvoiceTax(long invoiceId)
        {
            var items = GetInvoiceItems(invoiceId);
            return items.Sum(x => x.VatAmount);
        }

        public List<ErpSalesInvoice> GetCustomerInvoices(long customerId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId && x.IsActive == true)
                .WhereIF(fromDate.HasValue, x => x.InvoiceDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.InvoiceDate <= toDate.Value)
                .OrderByDescending(x => x.InvoiceDate)
                .ToList();
        }

        public List<ErpSalesInvoice> GetUnpaidInvoices(long? customerId = null)
        {
            return Queryable()
                .Where(x => x.RemainingAmount > 0 && x.IsActive == true && x.Status == "confirmed")
                .WhereIF(customerId.HasValue, x => x.CustomerId == customerId)
                .OrderByDescending(x => x.InvoiceDate)
                .ToList();
        }

        public int ApplyPayment(long invoiceId, decimal amount, long paymentMethodId)
        {
            return Context.Ado.UseTran(() =>
            {
                var invoice = GetInfo(invoiceId);
                if (invoice == null) return 0;

                var newPaidAmount = invoice.PaidAmount + amount;
                var newRemainingAmount = invoice.TotalAmount - newPaidAmount;

                return Context.Updateable<ErpSalesInvoice>()
                    .SetColumns(it => new ErpSalesInvoice
                    {
                        PaidAmount = newPaidAmount,
                        RemainingAmount = newRemainingAmount,
                        PaymentMethodId = paymentMethodId,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.InvoiceId == invoiceId)
                    .ExecuteCommand();
            }).Data;
        }

        public decimal GetTotalSales(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.InvoiceDate >= fromDate && x.InvoiceDate <= toDate)
                .Where(x => x.Status == "confirmed" && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .Sum(x => x.TotalAmount);
        }

        public List<object> GetSalesByProduct(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Context.Queryable<ErpSalesInvoice, ErpSalesInvoiceItem>((inv, item) =>
                    new JoinQueryInfos(JoinType.Inner, inv.InvoiceId == item.InvoiceId))
                .Where((inv, item) => inv.InvoiceDate >= fromDate && inv.InvoiceDate <= toDate)
                .Where((inv, item) => inv.Status == "confirmed" && inv.IsActive == true)
                .WhereIF(BranchID.HasValue, (inv, item) => inv.BranchID == BranchID)
                .GroupBy((inv, item) => new { item.ProductId, item.ItemName })
                .Select((inv, item) => new
                {
                    item.ProductId,
                    item.ItemName,
                    TotalQuantity = SqlFunc.AggregateSum(item.Quantity),
                    TotalAmount = SqlFunc.AggregateSum(item.Total)
                })
                .ToList()
                .Cast<object>()
                .ToList();
        }

        public List<object> GetSalesByEmployee(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.InvoiceDate >= fromDate && x.InvoiceDate <= toDate)
                .Where(x => x.Status == "confirmed" && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .GroupBy(x => new { x.SalespersonId })
                .Select(x => new
                {
                    x.SalespersonId,
                    TotalInvoices = SqlFunc.AggregateCount(x.InvoiceId),
                    TotalAmount = SqlFunc.AggregateSum(x.TotalAmount)
                })
                .ToList()
                .Cast<object>()
                .ToList();
        }

        public async Task<bool> SubmitToZatcaAsync(long invoiceId)
        {
            // ZATCA integration implementation
            var invoice = GetInfo(invoiceId);
            if (invoice == null) return false;

            // TODO: Implement ZATCA API integration
            // This would involve:
            // 1. Generate XML invoice
            // 2. Sign with certificate
            // 3. Submit to ZATCA
            // 4. Update invoice with ZATCA response

            await Task.CompletedTask;
            return true;
        }

        #region Private Methods

        private void CalculateAndUpdateTotals(long invoiceId)
        {
            var items = GetInvoiceItems(invoiceId);
            var subtotal = items.Sum(x => x.Quantity * x.UnitPrice);
            var discount = items.Sum(x => x.DiscountAmount);
            var vatAmount = items.Sum(x => x.VatAmount);
            var total = subtotal - discount + vatAmount;

            Context.Updateable<ErpSalesInvoice>()
                .SetColumns(it => new ErpSalesInvoice
                {
                    Subtotal = subtotal,
                    DiscountAmount = discount,
                    VatAmount = vatAmount,
                    TotalAmount = total,
                    RemainingAmount = total - it.PaidAmount
                })
                .Where(it => it.InvoiceId == invoiceId)
                .ExecuteCommand();
        }

        private void UpdateCustomerBalance(long customerId, decimal amount)
        {
            // Add to customer balance (debit)
            var balance = new ErpCustomerBalance
            {
                BalanceId = SnowFlakeSingle.Instance.NextId(),
                CustomerId = customerId,
                TransactionType = "invoice",
                DebitAmount = amount,
                CreditAmount = 0,
                TransactionDate = DateTime.Now,
                Create_time = DateTime.Now
            };
            Context.Insertable(balance).ExecuteCommand();
        }

        private void UpdateTreasuryBalance(long treasuryId, decimal amount)
        {
            // Add treasury transaction
            var transaction = new
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                TreasuryId = treasuryId,
                TransactionType = "receipt",
                Amount = amount,
                TransactionDate = DateTime.Now,
                Create_time = DateTime.Now
            };
            // Note: This should use the actual ErpTreasuryTransaction model
        }

        #endregion
    }
}

