using ZR.Model.Operations;
using ZR.Model.Operations.Dto;

namespace ZR.Service.Operations
{
    public interface IOpsCustomerWorkerService : IBaseService<OpsCustomerWorker>
    {
        PagedInfo<OpsCustomerWorkerDto> GetList(OpsCustomerWorkerQueryDto parm);
        OpsCustomerWorker GetInfo(long WorkerId);
        OpsCustomerWorker AddOpsCustomerWorker(OpsCustomerWorker model);
        int UpdateOpsCustomerWorker(OpsCustomerWorker model);
        List<OpsCustomerWorkerDto> GetByCustomerId(string customerId);
        PagedInfo<OpsCustomerWorkerDto> ExportList(OpsCustomerWorkerQueryDto parm);
        int UpdateIqamaImage(long workerId, string iqamaImageUrl);
    }
}
