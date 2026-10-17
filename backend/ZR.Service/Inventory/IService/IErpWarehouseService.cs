using ZR.Model;
using ZR.Model.Inventory;
using ZR.Service;

namespace ZR.Service.Inventory.IService
{
    /// <summary>
    /// Warehouse Service Interface - خدمة المستودعات
    /// </summary>
    public interface IErpWarehouseService : IBaseService<ErpWarehouse>
    {
        PagedInfo<ErpWarehouse> GetList(ErpWarehouse parm, PagerInfo pager);
        ErpWarehouse GetInfo(long warehouseId);
        ErpWarehouse AddWarehouse(ErpWarehouse model);
        int UpdateWarehouse(ErpWarehouse model);
        int Delete(long[] ids);
        List<ErpWarehouse> GetActiveWarehouses(long? BranchID = null);
        List<object> GetWarehouseStock(long warehouseId);
        decimal GetWarehouseValue(long warehouseId);
        int TransferStock(long fromWarehouseId, long toWarehouseId, long productId, decimal quantity, string reference);
    }
}

