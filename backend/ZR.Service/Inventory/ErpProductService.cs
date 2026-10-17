using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Inventory;
using ZR.Repository;
using ZR.Service.Inventory.IService;

namespace ZR.Service.Inventory
{
    /// <summary>
    /// Product Service - خدمة المنتجات
    /// </summary>
    [AppService(ServiceType = typeof(IErpProductService), ServiceLifetime = LifeTime.Transient)]
    public class ErpProductService : BaseService<ErpProduct>, IErpProductService
    {
      
        public PagedInfo<ErpProduct> GetList(ErpProduct parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpProduct>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ProductCode), it => it.ProductCode.Contains(parm.ProductCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.ProductName), it => it.ProductName.Contains(parm.ProductName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Barcode), it => it.Barcode == parm.Barcode);
            predicate = predicate.AndIF(parm.CategoryId > 0, it => it.CategoryId == parm.CategoryId);
            predicate = predicate.And(it => it.IsActive == true);

            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.ProductName).ToPage(pager);
        }

        public ErpProduct GetInfo(long productId) => GetFirst(x => x.ProductId == productId);
        public ErpProduct GetByCode(string productCode) => GetFirst(x => x.ProductCode == productCode && x.IsActive == true);
        public ErpProduct GetByBarcode(string barcode) => GetFirst(x => x.Barcode == barcode && x.IsActive == true);

        public ErpProduct AddProduct(ErpProduct model)
        {
            model.ProductId = SnowFlakeSingle.Instance.NextId();
            Insert(model);
            return model;
        }

        public int UpdateProduct(ErpProduct model)
        {
            return Update(model, it => new { it.ProductName, it.ProductNameAr, it.CategoryId, it.UnitOfMeasure, it.SellingPrice, it.CostPrice, it.ReorderLevel, it.MaximumStock, it.Description, it.Update_by, it.Update_time }, f => f.ProductId == model.ProductId);
        }

        public int Delete(long[] ids) => Context.Updateable<ErpProduct>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.ProductId)).ExecuteCommand();

        public List<ErpProduct> GetActiveProducts()
        {
            return Queryable().Where(x => x.IsActive == true).ToList();
        }

        public List<ErpProduct> GetProductsByCategory(long categoryId)
        {
            return Queryable().Where(x => x.CategoryId == categoryId && x.IsActive == true).ToList();
        }

        public List<ErpProduct> GetLowStockProducts()
        {
            // Low stock is determined by comparing inventory sum to ReorderLevel
            var products = Queryable().Where(x => x.IsActive == true && x.TrackInventory == true).ToList();
            return products.Where(p => GetProductStock(p.ProductId) <= p.ReorderLevel).ToList();
        }

        public decimal GetProductStock(long productId, long? warehouseId = null)
        {
            var query = Context.Queryable<ErpInventoryTransaction>().Where(x => x.ProductId == productId);
            if (warehouseId.HasValue)
            {
                query = query.Where(x => x.WarehouseId == warehouseId);
            }
            return query.Sum(x => x.Quantity);
        }

        public int UpdateStock(long productId, long warehouseId, decimal quantity, string transactionType, string reference)
        {
            var trans = new ErpInventoryTransaction
            {
                TransactionId = SnowFlakeSingle.Instance.NextId(),
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = transactionType,
                Quantity = quantity,  // Positive for in, negative for out
                TransactionDate = DateTime.Now,
                ReferenceNumber = reference,
                Create_time = DateTime.Now
            };
            return Context.Insertable(trans).ExecuteCommand();
        }

        public List<object> GetStockByWarehouse(long productId)
        {
            return Context.Queryable<ErpInventoryTransaction>()
                .Where(x => x.ProductId == productId)
                .GroupBy(x => x.WarehouseId)
                .Select(x => new { WarehouseId = x.WarehouseId, Stock = SqlFunc.AggregateSum(x.Quantity) })
                .ToList().Cast<object>().ToList();
        }

        public decimal GetProductCost(long productId) => GetInfo(productId)?.CostPrice ?? 0;

        public int UpdateProductPrice(long productId, decimal newPrice)
        {
            return Context.Updateable<ErpProduct>()
                .SetColumns(it => new ErpProduct { SellingPrice = newPrice, Update_time = DateTime.Now })
                .Where(it => it.ProductId == productId).ExecuteCommand();
        }

        public List<object> GetProductMovement(long productId, DateTime fromDate, DateTime toDate)
        {
            return Context.Queryable<ErpInventoryTransaction>()
                .Where(x => x.ProductId == productId && x.TransactionDate >= fromDate && x.TransactionDate <= toDate)
                .OrderBy(x => x.TransactionDate)
                .Select(x => new { x.TransactionId, x.TransactionType, x.Quantity, x.TransactionDate, x.ReferenceNumber })
                .ToList().Cast<object>().ToList();
        }
    }
}

