using ZR.Model;
using ZR.Model.Inventory;
using ZR.Service;

namespace ZR.Service.Inventory.IService
{
    /// <summary>
    /// Product Service Interface - خدمة المنتجات
    /// </summary>
    public interface IErpProductService : IBaseService<ErpProduct>
    {
        PagedInfo<ErpProduct> GetList(ErpProduct parm, PagerInfo pager);
        ErpProduct GetInfo(long productId);
        ErpProduct AddProduct(ErpProduct model);
        int UpdateProduct(ErpProduct model);
        int Delete(long[] ids);
        ErpProduct GetByCode(string productCode);
        ErpProduct GetByBarcode(string barcode);
        List<ErpProduct> GetActiveProducts();
        List<ErpProduct> GetProductsByCategory(long categoryId);
        List<ErpProduct> GetLowStockProducts();
        decimal GetProductStock(long productId, long? warehouseId = null);
        int UpdateStock(long productId, long warehouseId, decimal quantity, string transactionType, string reference);
        List<object> GetStockByWarehouse(long productId);
        decimal GetProductCost(long productId);
        int UpdateProductPrice(long productId, decimal newPrice);
        List<object> GetProductMovement(long productId, DateTime fromDate, DateTime toDate);
    }
}

