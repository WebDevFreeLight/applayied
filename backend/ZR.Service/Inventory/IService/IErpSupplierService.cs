using ZR.Model;
using ZR.Model.Inventory.Suppliers;
using ZR.Service;

namespace ZR.Service.Inventory.IService
{
    public interface IErpSupplierService : IBaseService<ErpSupplier>
    {
        PagedInfo<ErpSupplier> GetList(ErpSupplier parm, PagerInfo pager);
        ErpSupplier GetInfo(long supplierId);
        ErpSupplier AddSupplier(ErpSupplier model);
        int UpdateSupplier(ErpSupplier model);
        int Delete(long[] ids);
        string GenerateSupplierCode();
        ErpSupplier GetByCode(string supplierCode);
        List<ErpSupplier> GetActiveSuppliers();
        decimal GetSupplierBalance(long supplierId);
        List<ErpSupplierBalance> GetSupplierStatement(long supplierId, DateTime? fromDate, DateTime? toDate);
        int UpdateSupplierBalance(long supplierId, decimal amount, string transactionType, long? referenceId, string referenceType);
        List<object> GetSuppliersWithDueBalances();
    }

    public interface IErpSupplierBalanceService : IBaseService<ErpSupplierBalance>
    {
        PagedInfo<ErpSupplierBalance> GetList(ErpSupplierBalance parm, PagerInfo pager);
        ErpSupplierBalance GetInfo(long balanceId);
        ErpSupplierBalance AddTransaction(ErpSupplierBalance model);
        List<ErpSupplierBalance> GetSupplierStatement(long supplierId, DateTime? fromDate, DateTime? toDate);
        decimal GetSupplierBalance(long supplierId);
        decimal GetSupplierTotalDebit(long supplierId);
        decimal GetSupplierTotalCredit(long supplierId);
        List<object> GetAgingReport(long? supplierId = null);
        int RecalculateSupplierBalance(long supplierId);
    }
}

