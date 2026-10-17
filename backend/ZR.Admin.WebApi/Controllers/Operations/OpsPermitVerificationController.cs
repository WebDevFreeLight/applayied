using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Permit Verifications - التحقق من الوثائق
    /// </summary>
    [Route("operations/permit-verifications")]
    [ApiExplorerSettings(GroupName = "operations")]
    public class OpsPermitVerificationController : BaseController
    {
        private readonly IOpsPermitVerificationService _service;

        public OpsPermitVerificationController(IOpsPermitVerificationService service)
        {
            _service = service;
        }

        /// <summary>
        /// List verifications
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "operations:verification:list")]
        public IActionResult QueryList([FromQuery] OpsPermitVerificationQueryDto parm)
        {
            var response = _service.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get verification by ID
        /// </summary>
        [HttpGet("{VerificationId}")]
        [ActionPermissionFilter(Permission = "operations:verification:query")]
        public IActionResult GetInfo(long VerificationId)
        {
            var response = _service.GetInfo(VerificationId);
            var info = response.Adapt<OpsPermitVerificationDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add verification
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "operations:verification:add")]
        [Log(Title = "Permit Verification", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] OpsPermitVerificationDto parm)
        {
            var modal = parm.Adapt<OpsPermitVerification>().ToCreate(HttpContext);
            var response = _service.AddOpsPermitVerification(modal);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update verification
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "operations:verification:edit")]
        [Log(Title = "Permit Verification", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] OpsPermitVerificationDto parm)
        {
            var modal = parm.Adapt<OpsPermitVerification>().ToUpdate(HttpContext);
            var response = _service.UpdateOpsPermitVerification(modal);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete verifications
        /// </summary>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "operations:verification:delete")]
        [Log(Title = "Permit Verification", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);
            return ToResponse(_service.Delete(idArr, "Delete verifications"));
        }

        /// <summary>
        /// Verify document
        /// </summary>
        [HttpPost("verify")]
        [ActionPermissionFilter(Permission = "operations:verification:verify")]
        [Log(Title = "Verify Document", BusinessType = BusinessType.UPDATE)]
        public IActionResult Verify([FromBody] OpsVerifyDocumentRequest req)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var response = _service.VerifyDocument(req.VerificationId, loginUser.UserName);
            return ToResponse(response);
        }

        /// <summary>
        /// Reject document
        /// </summary>
        [HttpPost("reject")]
        [ActionPermissionFilter(Permission = "operations:verification:verify")]
        [Log(Title = "Reject Document", BusinessType = BusinessType.UPDATE)]
        public IActionResult Reject([FromBody] OpsRejectDocumentRequest req)
        {
            var response = _service.RejectDocument(req.VerificationId, req.Reason);
            return ToResponse(response);
        }

        /// <summary>
        /// Export verifications
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "operations:verification:export")]
        [Log(Title = "Permit Verification", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public IActionResult Export([FromQuery] OpsPermitVerificationQueryDto parm)
        {
            var list = _service.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "PermitVerifications", "PermitVerifications");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
