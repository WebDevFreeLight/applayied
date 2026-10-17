using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Repository;

namespace ZR.Service.Operations
{
    [AppService(ServiceType = typeof(IOpsCustomerWorkerService), ServiceLifetime = LifeTime.Transient)]
    public class OpsCustomerWorkerService : BaseService<OpsCustomerWorker>, IOpsCustomerWorkerService
    {
        public PagedInfo<OpsCustomerWorkerDto> GetList(OpsCustomerWorkerQueryDto parm)
        {
            var predicate = Expressionable.Create<OpsCustomerWorker>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CustomerId), it => it.CustomerId == parm.CustomerId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.FullName), it => it.FullName.Contains(parm.FullName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NationalId), it => it.NationalId.Contains(parm.NationalId));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.DocumentType), it => it.DocumentType == parm.DocumentType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Phone), it => it.Phone.Contains(parm.Phone));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Nationality), it => it.Nationality == parm.Nationality);
            predicate = predicate.AndIF(parm.IsActive != null, it => it.IsActive == parm.IsActive);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(it => it.Create_time, OrderByType.Desc)
                .ToPage<OpsCustomerWorker, OpsCustomerWorkerDto>(parm);

            return response;
        }

        public OpsCustomerWorker GetInfo(long WorkerId)
        {
            return Queryable().Where(x => x.WorkerId == WorkerId).First();
        }

        public OpsCustomerWorker AddOpsCustomerWorker(OpsCustomerWorker model)
        {
            // Auto-generate worker code
            var year = DateTime.Now.Year;
            var count = Queryable().Where(x => x.Create_time.Year == year).Count() + 1;
            model.WorkerCode = $"WRK-{year}-{count:D5}";

            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateOpsCustomerWorker(OpsCustomerWorker model)
        {
            return Update(model, true, "Update customer worker");
        }

        public List<OpsCustomerWorkerDto> GetByCustomerId(string customerId)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId && x.IsActive)
                .OrderBy(x => x.FullName)
                .Select(x => new OpsCustomerWorkerDto
                {
                    WorkerId = x.WorkerId,
                    CustomerId = x.CustomerId,
                    WorkerCode = x.WorkerCode,
                    FullName = x.FullName,
                    FullNameAr = x.FullNameAr,
                    NationalId = x.NationalId,
                    PassportNumber = x.PassportNumber,
                    DocumentType = x.DocumentType,
                    Phone = x.Phone,
                    Email = x.Email,
                    Nationality = x.Nationality,
                    JobTitle = x.JobTitle,
                    PhotoUrl = x.PhotoUrl,
                    IqamaImageUrl = x.IqamaImageUrl,
                    IsActive = x.IsActive
                })
                .ToList();
        }

        public PagedInfo<OpsCustomerWorkerDto> ExportList(OpsCustomerWorkerQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            return GetList(parm);
        }

        public int UpdateIqamaImage(long workerId, string iqamaImageUrl)
        {
            return Context.Updateable<OpsCustomerWorker>()
                .SetColumns(x => x.IqamaImageUrl == iqamaImageUrl)
                .Where(x => x.WorkerId == workerId)
                .ExecuteCommand();
        }
    }
}
