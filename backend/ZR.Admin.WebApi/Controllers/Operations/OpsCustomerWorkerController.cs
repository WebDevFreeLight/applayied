using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Customer Workers - عمال العميل
    /// </summary>
    [Route("operations/customer-workers")]
    [ApiExplorerSettings(GroupName = "operations")]
    public class OpsCustomerWorkerController : BaseController
    {
        private readonly IOpsCustomerWorkerService _service;

        public OpsCustomerWorkerController(IOpsCustomerWorkerService service)
        {
            _service = service;
        }

        /// <summary>
        /// List customer workers
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "operations:customerworker:list")]
        public IActionResult QueryList([FromQuery] OpsCustomerWorkerQueryDto parm)
        {
            var response = _service.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get customer worker by ID
        /// </summary>
        [HttpGet("{WorkerId}")]
        [ActionPermissionFilter(Permission = "operations:customerworker:query")]
        public IActionResult GetInfo(long WorkerId)
        {
            var response = _service.GetInfo(WorkerId);
            var info = response.Adapt<OpsCustomerWorkerDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Get workers by customer ID
        /// </summary>
        [HttpGet("byCustomer/{customerId}")]
        [ActionPermissionFilter(Permission = "operations:customerworker:list")]
        public IActionResult GetByCustomer(string customerId)
        {
            var response = _service.GetByCustomerId(customerId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Add customer worker
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "operations:customerworker:add")]
        [Log(Title = "Customer Worker", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] OpsCustomerWorkerDto parm)
        {
            var modal = parm.Adapt<OpsCustomerWorker>().ToCreate(HttpContext);
            modal.IsActive = true; // Ensure new workers are active by default
            var response = _service.AddOpsCustomerWorker(modal);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update customer worker
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "operations:customerworker:edit")]
        [Log(Title = "Customer Worker", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] OpsCustomerWorkerDto parm)
        {
            var modal = parm.Adapt<OpsCustomerWorker>().ToUpdate(HttpContext);
            var response = _service.UpdateOpsCustomerWorker(modal);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete customer workers
        /// </summary>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "operations:customerworker:delete")]
        [Log(Title = "Customer Worker", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);
            return ToResponse(_service.Delete(idArr, "Delete customer workers"));
        }

        /// <summary>
        /// Export customer workers
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "operations:customerworker:export")]
        [Log(Title = "Customer Worker", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public IActionResult Export([FromQuery] OpsCustomerWorkerQueryDto parm)
        {
            var list = _service.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "CustomerWorkers", "CustomerWorkers");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// Update worker iqama image
        /// </summary>
        [HttpPut("{workerId}/iqama")]
        [ActionPermissionFilter(Permission = "operations:customerworker:edit")]
        [Log(Title = "Worker Iqama Image", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateIqama(long workerId, [FromBody] UpdateIqamaRequest request)
        {
            var response = _service.UpdateIqamaImage(workerId, request.IqamaImageUrl);
            return ToResponse(response);
        }
    }

    public class UpdateIqamaRequest
    {
        public string IqamaImageUrl { get; set; }
    }
}
