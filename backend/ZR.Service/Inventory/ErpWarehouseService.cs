using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Inventory;
using ZR.Repository;
using ZR.Service.Inventory.IService;

namespace ZR.Service.Inventory
{
    /// <summary>
    /// Warehouse Service - خدمة المستودعات
    /// </summary>
    [AppService(ServiceType = typeof(IErpWarehouseService), ServiceLifetime = LifeTime.Transient)]
    public class ErpWarehouseService : BaseService<ErpWarehouse>, IErpWarehouseService
    {
      


        public PagedInfo<ErpWarehouse> GetList(ErpWarehouse parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpWarehouse>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.WarehouseName), it => it.WarehouseName.Contains(parm.WarehouseName));
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.WarehouseName).ToPage(pager);
        }

        public ErpWarehouse GetInfo(long warehouseId) => GetFirst(x => x.WarehouseId == warehouseId);

        public ErpWarehouse AddWarehouse(ErpWarehouse model)
        {
            model.WarehouseId = SnowFlakeSingle.Instance.NextId();
            Insert(model);
            return model;
        }

        public int UpdateWarehouse(ErpWarehouse model)
        {
            return Update(model, it => new { it.WarehouseName, it.WarehouseNameAr, it.Address, it.ManagerId, it.Update_by, it.Update_time }, f => f.WarehouseId == model.WarehouseId);
        }

        public int Delete(long[] ids) => Context.Updateable<ErpWarehouse>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.WarehouseId)).ExecuteCommand();

        public List<ErpWarehouse> GetActiveWarehouses(long? BranchID = null)
        {
            return Queryable().Where(x => x.IsActive == true).WhereIF(BranchID.HasValue, x => x.BranchID == BranchID).ToList();
        }

        public List<object> GetWarehouseStock(long warehouseId)
        {
            // Quantity is positive for inbound transactions (purchase, transfer_in) and negative for outbound (sale, transfer_out)
            return Context.Queryable<ErpInventoryTransaction, ErpProduct>((t, p) => new JoinQueryInfos(JoinType.Inner, t.ProductId == p.ProductId))
                .Where((t, p) => t.WarehouseId == warehouseId && t.Status == "confirmed")
                .GroupBy((t, p) => new { t.ProductId, p.ProductName, p.ProductCode })
                .Select((t, p) => new { t.ProductId, p.ProductName, p.ProductCode, Stock = SqlFunc.AggregateSum(t.Quantity) })
                .ToList().Cast<object>().ToList();
        }

        public decimal GetWarehouseValue(long warehouseId)
        {
            return Context.Queryable<ErpInventoryTransaction, ErpProduct>((t, p) => new JoinQueryInfos(JoinType.Inner, t.ProductId == p.ProductId))
                .Where((t, p) => t.WarehouseId == warehouseId && t.Status == "confirmed")
                .GroupBy((t, p) => new { t.ProductId, p.CostPrice })
                .Select((t, p) => new { Stock = SqlFunc.AggregateSum(t.Quantity), p.CostPrice })
                .ToList().Sum(x => x.Stock * x.CostPrice);
        }

        public int TransferStock(long fromWarehouseId, long toWarehouseId, long productId, decimal quantity, string reference)
        {
            return Context.Ado.UseTran(() =>
            {
                var outTrans = new ErpInventoryTransaction
                {
                    TransactionId = SnowFlakeSingle.Instance.NextId(),
                    ProductId = productId,
                    WarehouseId = fromWarehouseId,
                    TransactionType = "stock_transfer",
                    Quantity = -quantity,  // Negative for outbound
                    FromWarehouseId = fromWarehouseId,
                    ToWarehouseId = toWarehouseId,
                    TransactionDate = DateTime.Now,
                    ReferenceNumber = reference,
                    Status = "confirmed",
                    Create_time = DateTime.Now
                };
                Context.Insertable(outTrans).ExecuteCommand();

                var inTrans = new ErpInventoryTransaction
                {
                    TransactionId = SnowFlakeSingle.Instance.NextId(),
                    ProductId = productId,
                    WarehouseId = toWarehouseId,
                    TransactionType = "stock_transfer",
                    Quantity = quantity,  // Positive for inbound
                    FromWarehouseId = fromWarehouseId,
                    ToWarehouseId = toWarehouseId,
                    TransactionDate = DateTime.Now,
                    ReferenceNumber = reference,
                    Status = "confirmed",
                    Create_time = DateTime.Now
                };
                return Context.Insertable(inTrans).ExecuteCommand();
            }).Data;
        }
    }
}

