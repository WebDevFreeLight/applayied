using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Inventory;
using ZR.Repository;
using ZR.Service.Inventory.IService;

namespace ZR.Service.Inventory
{
    /// <summary>
    /// Purchase Order Service - خدمة أوامر الشراء
    /// </summary>
    [AppService(ServiceType = typeof(IErpPurchaseOrderService), ServiceLifetime = LifeTime.Transient)]
    public class ErpPurchaseOrderService : BaseService<ErpPurchaseOrder>, IErpPurchaseOrderService
    {
        public PagedInfo<ErpPurchaseOrder> GetList(ErpPurchaseOrder parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpPurchaseOrder>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.PurchaseOrderNumber), it => it.PurchaseOrderNumber.Contains(parm.PurchaseOrderNumber));
            predicate = predicate.AndIF(parm.SupplierId > 0, it => it.SupplierId == parm.SupplierId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.OrderDate)
                .ToPage(pager);
        }

        public ErpPurchaseOrder GetInfo(long purchaseOrderId)
        {
            var order = GetFirst(x => x.PurchaseOrderId == purchaseOrderId);
            if (order != null)
            {
                order.Items = GetPurchaseOrderItems(purchaseOrderId);
            }
            return order;
        }

        public ErpPurchaseOrder AddPurchaseOrder(ErpPurchaseOrder model)
        {
            return Context.Ado.UseTran(() =>
            {
                model.PurchaseOrderId = SnowFlakeSingle.Instance.NextId();
                model.Status = "draft";
                model.Create_time = DateTime.Now;

                // Calculate totals
                CalculateTotals(model);

                Insert(model);

                // Insert items
                if (model.Items != null && model.Items.Count > 0)
                {
                    foreach (var item in model.Items)
                    {
                        item.ItemId = SnowFlakeSingle.Instance.NextId();
                        item.PurchaseOrderId = model.PurchaseOrderId;
                        item.TotalPrice = item.OrderedQuantity * item.UnitPrice * (1 - item.DiscountPercentage / 100) * (1 + item.TaxPercentage / 100);
                        item.Create_time = DateTime.Now;
                    }
                    Context.Insertable(model.Items).ExecuteCommand();
                }

                return model;
            }).Data;
        }

        public int UpdatePurchaseOrder(ErpPurchaseOrder model)
        {
            return Context.Ado.UseTran(() =>
            {
                // Calculate totals
                CalculateTotals(model);

                var result = Update(model, it => new
                {
                    it.SupplierId,
                    it.WarehouseId,
                    it.OrderDate,
                    it.ExpectedDeliveryDate,
                    it.Subtotal,
                    it.DiscountAmount,
                    it.TaxAmount,
                    it.TotalAmount,
                    it.Notes,
                    it.Update_by,
                    it.Update_time
                }, f => f.PurchaseOrderId == model.PurchaseOrderId);

                // Update items
                if (model.Items != null)
                {
                    // Delete existing items
                    Context.Deleteable<ErpPurchaseOrderItem>()
                        .Where(x => x.PurchaseOrderId == model.PurchaseOrderId)
                        .ExecuteCommand();

                    // Insert new items
                    foreach (var item in model.Items)
                    {
                        item.ItemId = SnowFlakeSingle.Instance.NextId();
                        item.PurchaseOrderId = model.PurchaseOrderId;
                        item.TotalPrice = item.OrderedQuantity * item.UnitPrice * (1 - item.DiscountPercentage / 100) * (1 + item.TaxPercentage / 100);
                        item.Create_time = DateTime.Now;
                    }
                    Context.Insertable(model.Items).ExecuteCommand();
                }

                return result;
            }).Data;
        }

        public int Delete(long[] ids)
        {
            return Context.Updateable<ErpPurchaseOrder>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.PurchaseOrderId))
                .ExecuteCommand();
        }

        public string GeneratePurchaseOrderNumber(long? BranchID)
        {
            var prefix = $"PO{DateTime.Now:yyyyMMdd}";
            var lastOrder = Queryable()
                .Where(x => x.PurchaseOrderNumber.StartsWith(prefix))
                .OrderByDescending(x => x.PurchaseOrderNumber)
                .First();

            var sequence = 1;
            if (lastOrder != null && lastOrder.PurchaseOrderNumber.Length > prefix.Length)
            {
                int.TryParse(lastOrder.PurchaseOrderNumber.Substring(prefix.Length), out sequence);
                sequence++;
            }

            return $"{prefix}{sequence:D4}";
        }

        public int ApprovePurchaseOrder(long purchaseOrderId, string userId)
        {
            return Context.Updateable<ErpPurchaseOrder>()
                .SetColumns(it => new ErpPurchaseOrder
                {
                    Status = "approved",
                    ApprovedBy = userId,
                    ApprovedDate = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(it => it.PurchaseOrderId == purchaseOrderId && it.Status == "pending")
                .ExecuteCommand();
        }

        public int RejectPurchaseOrder(long purchaseOrderId, string reason)
        {
            return Context.Updateable<ErpPurchaseOrder>()
                .SetColumns(it => new ErpPurchaseOrder
                {
                    Status = "rejected",
                    Notes = reason,
                    Update_time = DateTime.Now
                })
                .Where(it => it.PurchaseOrderId == purchaseOrderId && it.Status == "pending")
                .ExecuteCommand();
        }

        public int CancelPurchaseOrder(long purchaseOrderId, string reason)
        {
            return Context.Updateable<ErpPurchaseOrder>()
                .SetColumns(it => new ErpPurchaseOrder
                {
                    Status = "cancelled",
                    Notes = reason,
                    Update_time = DateTime.Now
                })
                .Where(it => it.PurchaseOrderId == purchaseOrderId && it.Status != "received")
                .ExecuteCommand();
        }

        public int ReceivePurchaseOrder(long purchaseOrderId, List<ErpPurchaseOrderItem> receivedItems)
        {
            return Context.Ado.UseTran(() =>
            {
                // Update received quantities for each item
                foreach (var item in receivedItems)
                {
                    Context.Updateable<ErpPurchaseOrderItem>()
                        .SetColumns(it => new ErpPurchaseOrderItem
                        {
                            ReceivedQuantity = item.ReceivedQuantity,
                            Update_time = DateTime.Now
                        })
                        .Where(it => it.ItemId == item.ItemId)
                        .ExecuteCommand();
                }

                // Check if all items received
                var orderItems = GetPurchaseOrderItems(purchaseOrderId);
                var allReceived = orderItems.All(x => x.ReceivedQuantity >= x.OrderedQuantity);

                // Update order status
                var status = allReceived ? "received" : "partially_received";
                return Context.Updateable<ErpPurchaseOrder>()
                    .SetColumns(it => new ErpPurchaseOrder
                    {
                        Status = status,
                        ActualDeliveryDate = allReceived ? DateTime.Now : null,
                        Update_time = DateTime.Now
                    })
                    .Where(it => it.PurchaseOrderId == purchaseOrderId)
                    .ExecuteCommand();
            }).Data;
        }

        public List<ErpPurchaseOrder> GetSupplierPurchaseOrders(long supplierId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.SupplierId == supplierId && x.IsActive == true)
                .WhereIF(fromDate.HasValue, x => x.OrderDate >= fromDate)
                .WhereIF(toDate.HasValue, x => x.OrderDate <= toDate)
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        public List<ErpPurchaseOrder> GetPendingPurchaseOrders(long? BranchID = null)
        {
            return Queryable()
                .Where(x => (x.Status == "pending" || x.Status == "approved") && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        public List<ErpPurchaseOrderItem> GetPurchaseOrderItems(long purchaseOrderId)
        {
            return Context.Queryable<ErpPurchaseOrderItem>()
                .Where(x => x.PurchaseOrderId == purchaseOrderId)
                .ToList();
        }

        public decimal GetTotalPurchases(DateTime fromDate, DateTime toDate, long? supplierId = null)
        {
            return Queryable()
                .Where(x => x.OrderDate >= fromDate && x.OrderDate <= toDate)
                .Where(x => x.Status == "received" && x.IsActive == true)
                .WhereIF(supplierId.HasValue, x => x.SupplierId == supplierId)
                .Sum(x => x.TotalAmount);
        }

        public List<object> GetPurchaseReport(DateTime fromDate, DateTime toDate, long? BranchID = null)
        {
            return Queryable()
                .Where(x => x.OrderDate >= fromDate && x.OrderDate <= toDate && x.IsActive == true)
                .WhereIF(BranchID.HasValue, x => x.BranchID == BranchID)
                .GroupBy(x => x.Status)
                .Select(x => new
                {
                    Status = x.Status,
                    Count = SqlFunc.AggregateCount(x.PurchaseOrderId),
                    TotalAmount = SqlFunc.AggregateSum(x.TotalAmount)
                })
                .ToList()
                .Cast<object>()
                .ToList();
        }

        private void CalculateTotals(ErpPurchaseOrder model)
        {
            if (model.Items != null && model.Items.Count > 0)
            {
                model.Subtotal = model.Items.Sum(x => x.OrderedQuantity * x.UnitPrice);
                model.TaxAmount = model.Items.Sum(x => x.OrderedQuantity * x.UnitPrice * x.TaxPercentage / 100);
                model.TotalAmount = model.Subtotal - model.DiscountAmount + model.TaxAmount;
            }
        }
    }
}

