using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Inventory.Suppliers;
using ZR.Repository;
using ZR.Service.Inventory.IService;

namespace ZR.Service.Inventory
{
    [AppService(ServiceType = typeof(IErpSupplierService), ServiceLifetime = LifeTime.Transient)]
    public class ErpSupplierService : BaseService<ErpSupplier>, IErpSupplierService
    {
       

        public PagedInfo<ErpSupplier> GetList(ErpSupplier parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpSupplier>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.SupplierCode), it => it.SupplierCode.Contains(parm.SupplierCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.SupplierName), it => it.SupplierName.Contains(parm.SupplierName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Phone), it => it.Phone.Contains(parm.Phone));
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.SupplierName).ToPage(pager);
        }

        public ErpSupplier GetInfo(long supplierId) => GetFirst(x => x.SupplierId == supplierId);
        public ErpSupplier GetByCode(string supplierCode) => GetFirst(x => x.SupplierCode == supplierCode && x.IsActive == true);

        public ErpSupplier AddSupplier(ErpSupplier model)
        {
            if (string.IsNullOrEmpty(model.SupplierCode)) model.SupplierCode = GenerateSupplierCode();
            model.SupplierId = SnowFlakeSingle.Instance.NextId();
            Insert(model);

            // Add opening balance transaction if CurrentBalance is set
            if (model.CurrentBalance != 0)
            {
                var balance = new ErpSupplierBalance
                {
                    BalanceId = SnowFlakeSingle.Instance.NextId(),
                    SupplierId = model.SupplierId,
                    TransactionType = "opening_balance",
                    DebitAmount = model.CurrentBalance > 0 ? 0 : Math.Abs(model.CurrentBalance),
                    CreditAmount = model.CurrentBalance > 0 ? model.CurrentBalance : 0,
                    TransactionDate = DateTime.Now,
                    Description = "رصيد افتتاحي - Opening Balance",
                    Balance = model.CurrentBalance,
                    Create_time = DateTime.Now
                };
                Context.Insertable(balance).ExecuteCommand();
            }
            return model;
        }

        public int UpdateSupplier(ErpSupplier model) => Update(model, it => new { it.SupplierName, it.SupplierNameAr, it.ContactPerson, it.Phone, it.Email, it.Address, it.VatNumber, it.PaymentTermsDays, it.CreditLimit, it.Notes, it.Update_by, it.Update_time }, f => f.SupplierId == model.SupplierId);
        public int Delete(long[] ids) => Context.Updateable<ErpSupplier>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.SupplierId)).ExecuteCommand();

        public string GenerateSupplierCode()
        {
            var count = Count(x => true) + 1;
            return $"SUP-{count:D5}";
        }

        public List<ErpSupplier> GetActiveSuppliers() => Queryable().Where(x => x.IsActive == true).OrderBy(x => x.SupplierName).ToList();

        public decimal GetSupplierBalance(long supplierId)
        {
            var supplier = GetInfo(supplierId);
            return supplier?.CurrentBalance ?? 0;
        }

        public List<ErpSupplierBalance> GetSupplierStatement(long supplierId, DateTime? fromDate, DateTime? toDate)
        {
            return Context.Queryable<ErpSupplierBalance>()
                .Where(x => x.SupplierId == supplierId)
                .WhereIF(fromDate.HasValue, x => x.TransactionDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.TransactionDate <= toDate.Value)
                .OrderBy(x => x.TransactionDate)
                .ToList();
        }

        public int UpdateSupplierBalance(long supplierId, decimal amount, string transactionType, long? referenceId, string referenceType)
        {
            return Context.Ado.UseTran(() =>
            {
                var currentBalance = GetSupplierBalance(supplierId);
                var isCredit = transactionType == "purchase" || transactionType == "debit_note";
                var newBalance = isCredit ? currentBalance + amount : currentBalance - amount;

                var balance = new ErpSupplierBalance
                {
                    BalanceId = SnowFlakeSingle.Instance.NextId(),
                    SupplierId = supplierId,
                    TransactionType = transactionType,
                    DebitAmount = isCredit ? 0 : amount,
                    CreditAmount = isCredit ? amount : 0,
                    TransactionDate = DateTime.Now,
                    ReferenceId = referenceId,
                    ReferenceType = referenceType,
                    Balance = newBalance,
                    Create_time = DateTime.Now
                };
                Context.Insertable(balance).ExecuteCommand();

                return Context.Updateable<ErpSupplier>()
                    .SetColumns(it => new ErpSupplier { CurrentBalance = newBalance, Update_time = DateTime.Now })
                    .Where(it => it.SupplierId == supplierId).ExecuteCommand();
            }).Data;
        }

        public List<object> GetSuppliersWithDueBalances()
        {
            return Queryable().Where(x => x.CurrentBalance > 0 && x.IsActive == true)
                .Select(x => new { x.SupplierId, x.SupplierCode, x.SupplierName, x.CurrentBalance, x.CreditLimit })
                .ToList().Cast<object>().ToList();
        }
    }


}

