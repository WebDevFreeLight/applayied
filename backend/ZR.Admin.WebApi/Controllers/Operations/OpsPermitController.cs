using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Pdf.Core;
using ZR.Service.Operations;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Operations Permits - تصاريح العمليات
    /// </summary>
    [Route("operations/permits")]
    [ApiExplorerSettings(GroupName = "operations")]
    public class OpsPermitController : BaseController
    {
        private readonly IOpsPermitService _service;

        public OpsPermitController(IOpsPermitService service)
        {
            _service = service;
        }

        /// <summary>
        /// List permits
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "operations:permit:list")]
        public IActionResult QueryList([FromQuery] OpsPermitQueryDto parm)
        {
            var response = _service.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get permit by ID
        /// </summary>
        [HttpGet("{PermitId}")]
        [ActionPermissionFilter(Permission = "operations:permit:query")]
        public IActionResult GetInfo(long PermitId)
        {
            var response = _service.GetInfo(PermitId);
            var info = response.Adapt<OpsPermitDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add permit
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "operations:permit:add")]
        [Log(Title = "Permit", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] OpsPermitDto parm)
        {
            var modal = parm.Adapt<OpsPermit>().ToCreate(HttpContext);
            var response = _service.AddOpsPermit(modal);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update permit
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "operations:permit:edit")]
        [Log(Title = "Permit", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] OpsPermitDto parm)
        {
            var modal = parm.Adapt<OpsPermit>().ToUpdate(HttpContext);
            var response = _service.UpdateOpsPermit(modal);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete permits
        /// </summary>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "operations:permit:delete")]
        [Log(Title = "Permit", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);
            return ToResponse(_service.Delete(idArr, "Delete permits"));
        }

        /// <summary>
        /// Export permits
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "operations:permit:export")]
        [Log(Title = "Permit", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public IActionResult Export([FromQuery] OpsPermitQueryDto parm)
        {
            var list = _service.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "Permits", "Permits");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// Approve permit
        /// </summary>
        [HttpPost("approve")]
        [ActionPermissionFilter(Permission = "operations:permit:approve")]
        [Log(Title = "Permit Approve", BusinessType = BusinessType.UPDATE)]
        public IActionResult Approve([FromBody] OpsPermitApproveRequest req)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var response = _service.ApprovePermit(req.PermitId, loginUser.UserId);
            return ToResponse(response);
        }

        /// <summary>
        /// Reject permit
        /// </summary>
        [HttpPost("reject")]
        [ActionPermissionFilter(Permission = "operations:permit:approve")]
        [Log(Title = "Permit Reject", BusinessType = BusinessType.UPDATE)]
        public IActionResult Reject([FromBody] OpsPermitRejectRequest req)
        {
            var response = _service.RejectPermit(req.PermitId, req.Reason);
            return ToResponse(response);
        }

        /// <summary>
        /// Get permits by customer
        /// </summary>
        [HttpGet("byCustomer/{customerId}")]
        [ActionPermissionFilter(Permission = "operations:permit:list")]
        public IActionResult GetByCustomer(string customerId)
        {
            var response = _service.GetByCustomerId(customerId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get permits by season
        /// </summary>
        [HttpGet("bySeason/{seasonId}")]
        [ActionPermissionFilter(Permission = "operations:permit:list")]
        public IActionResult GetBySeason(long seasonId)
        {
            var response = _service.GetBySeasonId(seasonId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get permits by worker
        /// </summary>
        [HttpGet("byWorker/{workerId}")]
        [ActionPermissionFilter(Permission = "operations:permit:list")]
        public IActionResult GetByWorker(long workerId)
        {
            var response = _service.GetByWorkerId(workerId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get permit stats for dashboard
        /// </summary>
        [HttpGet("stats")]
        [ActionPermissionFilter(Permission = "operations:permit:list")]
        public IActionResult GetStats()
        {
            var response = _service.GetStats();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get digital permits (approved with QR codes)
        /// </summary>
        [HttpGet("digital-permits")]
        [ActionPermissionFilter(Permission = "operations:permit:list")]
        public IActionResult GetDigitalPermits([FromQuery] OpsPermitQueryDto parm)
        {
            var response = _service.GetDigitalPermits(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get monthly report
        /// </summary>
        [HttpGet("reports/monthly")]
        [ActionPermissionFilter(Permission = "operations:permit:list")]
        public IActionResult GetMonthlyReport([FromQuery] int? year)
        {
            var response = _service.GetMonthlyReport(year ?? DateTime.Now.Year);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get permit type distribution
        /// </summary>
        [HttpGet("reports/distribution")]
        [ActionPermissionFilter(Permission = "operations:permit:list")]
        public IActionResult GetPermitTypeDistribution()
        {
            var response = _service.GetPermitTypeDistribution();
            return SUCCESS(response);
        }

        /// <summary>
        /// Create permit with workers and safety checks (single call)
        /// </summary>
        [HttpPost("create-full")]
        [ActionPermissionFilter(Permission = "operations:permit:add")]
        [Log(Title = "Permit Full Create", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateFull([FromBody] OpsPermitCreateRequest request)
        {
            var response = _service.AddPermitFull(request, HttpContext);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get permit detail with workers and safety checks
        /// </summary>
        [HttpGet("{permitId}/detail")]
        [ActionPermissionFilter(Permission = "operations:permit:query")]
        public IActionResult GetDetail(long permitId)
        {
            var response = _service.GetPermitDetail(permitId);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Permit not found");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Export permit as PDF (Sela work permit form)
        /// </summary>
        [HttpGet("{permitId}/pdf")]
        [ActionPermissionFilter(Permission = "operations:permit:export")]
        [Log(Title = "Permit PDF Export", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public async Task<IActionResult> ExportPdf(long permitId, [FromServices] IPdfRenderer pdfRenderer)
        {
            var permit = _service.GetPermitDetail(permitId);
            if (permit == null)
            {
                return ToResponse(ResultCode.FAIL, "Permit not found");
            }

            var html = WorkPermitPdfTemplate.GenerateHtml(permit);
            var pdfBytes = await pdfRenderer.GetPdfByHtmlAsync(html);
            var fileName = $"WorkPermit_{permit.PermitNumber ?? permitId.ToString()}_{DateTime.Now:yyyyMMdd}.pdf";

            Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
