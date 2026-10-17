using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Finance.Treasury;
using ZR.Repository;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Service.Finance.Treasury
{



    [AppService(ServiceType = typeof(IErpTreasuryTransferService), ServiceLifetime = LifeTime.Transient)]
    public class ErpTreasuryTransferService : BaseService<ErpTreasuryTransfer>, IErpTreasuryTransferService
    {


        public PagedInfo<ErpTreasuryTransfer> GetList(ErpTreasuryTransfer parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpTreasuryTransfer>();
            predicate = predicate.AndIF(parm.FromTreasuryId > 0, it => it.FromTreasuryId == parm.FromTreasuryId || it.ToTreasuryId == parm.FromTreasuryId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), it => it.Status == parm.Status);
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.TransferDate).ToPage(pager);
        }

        public ErpTreasuryTransfer GetInfo(long transferId) => GetFirst(x => x.TransferId == transferId);

        public ErpTreasuryTransfer CreateTransfer(ErpTreasuryTransfer model)
        {
            model.TransferId = SnowFlakeSingle.Instance.NextId();
            model.Status = "pending";
            Insert(model);
            return model;
        }

        /// <summary>
        /// OBSOLETE: Transfers should be recorded through GL Journal Entries
        /// </summary>
        [Obsolete("Transfers should be recorded through GL Journal Entries")]
        public int ApproveTransfer(long transferId, string userId)
        {
            throw new Exception("OBSOLETE: Transfers should be recorded through GL Journal Entries - يجب تسجيل التحويلات من خلال قيود اليومية");
        }

        public int CancelTransfer(long transferId, string reason) => Context.Updateable<ErpTreasuryTransfer>().SetColumns(it => new ErpTreasuryTransfer { Status = "cancelled", Notes = reason, Update_time = DateTime.Now }).Where(it => it.TransferId == transferId && it.Status == "pending").ExecuteCommand();

        public List<ErpTreasuryTransfer> GetPendingTransfers(long? BranchID = null) => Queryable().Where(x => x.Status == "pending" && x.IsActive == true).WhereIF(BranchID.HasValue, x => x.BranchID == BranchID).ToList();
    }


}

