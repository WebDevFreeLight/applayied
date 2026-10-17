using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Inventory;
using ZR.Repository;
using ZR.Service.Inventory.IService;

namespace ZR.Service.Inventory
{
    /// <summary>
    /// Inventory Transaction Service - خدمة حركات المخزون
    /// </summary>
    [AppService(ServiceType = typeof(IErpInventoryTransactionService), ServiceLifetime = LifeTime.Transient)]
    public class ErpInventoryTransactionService : BaseService<ErpInventoryTransaction>, IErpInventoryTransactionService
    {
         


        public PagedInfo<ErpInventoryTransaction> GetList(ErpInventoryTransaction parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpInventoryTransaction>();
            predicate = predicate.AndIF(parm.ProductId > 0, it => it.ProductId == parm.ProductId);
            predicate = predicate.AndIF(parm.WarehouseId > 0, it => it.WarehouseId == parm.WarehouseId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TransactionType), it => it.TransactionType == parm.TransactionType);

            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.TransactionDate).ToPage(pager);
        }

        public ErpInventoryTransaction GetInfo(long transactionId) => GetFirst(x => x.TransactionId == transactionId);

        public ErpInventoryTransaction CreateTransaction(ErpInventoryTransaction model)
        {
            model.TransactionId = SnowFlakeSingle.Instance.NextId();
            model.TransactionDate = DateTime.Now;
            Insert(model);

            // Quantity can be positive (in) or negative (out)
            // Stock update logic would be handled by the application based on transaction type
            return model;
        }

        public int Delete(long[] ids) => 0; // Transactions should not be deleted

        public List<ErpInventoryTransaction> GetProductTransactions(long productId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.ProductId == productId)
                .WhereIF(fromDate.HasValue, x => x.TransactionDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.TransactionDate <= toDate.Value)
                .OrderByDescending(x => x.TransactionDate).ToList();
        }

        public List<ErpInventoryTransaction> GetWarehouseTransactions(long warehouseId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.WarehouseId == warehouseId)
                .WhereIF(fromDate.HasValue, x => x.TransactionDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.TransactionDate <= toDate.Value)
                .OrderByDescending(x => x.TransactionDate).ToList();
        }

        public int CreateOpeningBalance(long productId, long warehouseId, decimal quantity, decimal cost, string reference)
        {
            var trans = new ErpInventoryTransaction
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = "opening_balance",
                Quantity = quantity,
                UnitCost = cost,
                TotalCost = quantity * cost,
                TransactionDate = DateTime.Now,
                ReferenceNumber = reference,
                Create_time = DateTime.Now
            };
            CreateTransaction(trans);
            return 1;
        }

        public int CreatePurchaseReceipt(long productId, long warehouseId, decimal quantity, decimal cost, long? purchaseId)
        {
            var trans = new ErpInventoryTransaction
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = "purchase",
                Quantity = quantity,  // Positive for inbound
                UnitCost = cost,
                TotalCost = quantity * cost,
                TransactionDate = DateTime.Now,
                ReferenceId = purchaseId,
                Create_time = DateTime.Now
            };
            CreateTransaction(trans);
            return 1;
        }

        public int CreateSalesIssue(long productId, long warehouseId, decimal quantity, long? invoiceId)
        {
            var product = Context.Queryable<ErpProduct>().First(x => x.ProductId == productId);
            var trans = new ErpInventoryTransaction
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = "sales",
                Quantity = -quantity,  // Negative for outbound
                UnitCost = product?.CostPrice ?? 0,
                TotalCost = quantity * (product?.CostPrice ?? 0),
                TransactionDate = DateTime.Now,
                ReferenceId = invoiceId,
                Create_time = DateTime.Now
            };
            CreateTransaction(trans);
            return 1;
        }

        public int CreateTransferOut(long productId, long warehouseId, decimal quantity, long transferId)
        {
            var trans = new ErpInventoryTransaction
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = "stock_transfer",
                Quantity = -quantity,  // Negative for outbound
                TransactionDate = DateTime.Now,
                ReferenceId = transferId,
                Create_time = DateTime.Now
            };
            CreateTransaction(trans);
            return 1;
        }

        public int CreateTransferIn(long productId, long warehouseId, decimal quantity, long transferId)
        {
            var trans = new ErpInventoryTransaction
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = "stock_transfer",
                Quantity = quantity,  // Positive for inbound
                TransactionDate = DateTime.Now,
                ReferenceId = transferId,
                Create_time = DateTime.Now
            };
            CreateTransaction(trans);
            return 1;
        }

        public int CreateAdjustment(long productId, long warehouseId, decimal quantity, string reason)
        {
            var trans = new ErpInventoryTransaction
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = "adjustment",
                Quantity = quantity,  // Can be positive or negative
                TransactionDate = DateTime.Now,
                Notes = reason,
                Create_time = DateTime.Now
            };
            CreateTransaction(trans);
            return 1;
        }

        public int CreateDamaged(long productId, long warehouseId, decimal quantity, string reason)
        {
            var trans = new ErpInventoryTransaction
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = "damaged",
                Quantity = -quantity,  // Negative for outbound (loss)
                TransactionDate = DateTime.Now,
                Notes = reason,
                Create_time = DateTime.Now
            };
            CreateTransaction(trans);
            return 1;
        }

        public List<object> GetInventoryReport(DateTime asOfDate, long? warehouseId = null)
        {
            return Context.Queryable<ErpInventoryTransaction, ErpProduct>((t, p) => new JoinQueryInfos(JoinType.Inner, t.ProductId == p.ProductId))
                .Where((t, p) => t.TransactionDate <= asOfDate && t.Status == "confirmed")
                .WhereIF(warehouseId.HasValue, (t, p) => t.WarehouseId == warehouseId)
                .GroupBy((t, p) => new { t.ProductId, p.ProductName, p.ProductCode, p.CostPrice })
                .Select((t, p) => new
                {
                    t.ProductId, p.ProductName, p.ProductCode,
                    Stock = SqlFunc.AggregateSum(t.Quantity),
                    Value = SqlFunc.AggregateSum(t.Quantity) * p.CostPrice
                }).ToList().Cast<object>().ToList();
        }

        public List<object> GetMovementReport(DateTime fromDate, DateTime toDate, long? productId = null)
        {
            return Queryable()
                .Where(x => x.TransactionDate >= fromDate && x.TransactionDate <= toDate)
                .WhereIF(productId.HasValue, x => x.ProductId == productId)
                .GroupBy(x => new { x.ProductId, x.TransactionType })
                .Select(x => new
                {
                    x.ProductId, x.TransactionType,
                    TotalQuantity = SqlFunc.AggregateSum(x.Quantity),
                    TotalCost = SqlFunc.AggregateSum(x.TotalCost)
                }).ToList().Cast<object>().ToList();
        }
    }
}

