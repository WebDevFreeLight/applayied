using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.Settings;
using ZR.Service.Settings.IService;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using Infrastructure.Enums;

namespace ZR.Admin.WebApi.Controllers.Settings
{
    /// <summary>
    /// Branch Management - إدارة الفروع
    /// </summary>
    [Verify]
    [Route("api/erp/branches")]
    [ApiController]
    public class ErpBranchController : BaseController
    {
        private readonly IErpBranchService _branchService;

        public ErpBranchController(IErpBranchService branchService)
        {
            _branchService = branchService;
        }
        
        /// <summary>
        /// Get branch list - قائمة الفروع
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ErpBranchDTO parm )
        {
            var data=parm.Adapt<ErpBranch>();
            var data1=parm.Adapt<PagerInfo>();
            var response = _branchService.GetList(data, data1);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get all branches - جميع الفروع
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var response = _branchService.GetAllBranches();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get branch by ID - الحصول على فرع بالمعرف
        /// </summary>
        [HttpGet("{branchId}")]
        public IActionResult GetInfo(long branchId)
        {
            var response = _branchService.GetInfo(branchId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create branch - إنشاء فرع
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Branch", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpBranch branch)
        {
            branch.Create_by = HttpContext.GetName();
            var response = _branchService.AddBranch(branch);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update branch - تحديث فرع
        /// </summary>
        [HttpPut]
        [Log(Title = "Update Branch", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpBranch branch)
        {
            branch.Update_by = HttpContext.GetName();
            branch.Update_time = DateTime.Now;
            var response = _branchService.UpdateBranch(branch);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete branches - حذف فروع
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Branches", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idArr = Tools.SpitLongArrary(ids);
            var response = _branchService.Delete(idArr);
            return ToResponse(response);
        }

        /// <summary>
        /// Get main branch - الفرع الرئيسي
        /// </summary>
        [HttpGet("main")]
        public IActionResult GetMainBranch()
        {
            var response = _branchService.GetMainBranch();
            return SUCCESS(response);
        }

        /// <summary>
        /// Set branch as main - تحديد الفرع الرئيسي
        /// </summary>
        [HttpPost("set-main/{branchId}")]
        [Log(Title = "Set Main Branch", BusinessType = BusinessType.UPDATE)]
        public IActionResult SetMainBranch(long branchId)
        {
            var response = _branchService.SetMainBranch(branchId);
            return ToResponse(response);
        }

        /// <summary>
        /// Toggle branch status - تغيير حالة الفرع
        /// </summary>
        [HttpPost("toggle-status/{branchId}")]
        [Log(Title = "Toggle Branch Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult ToggleStatus(long branchId)
        {
            var response = _branchService.ToggleStatus(branchId);
            return ToResponse(response);
        }

        /// <summary>
        /// Get branch employees - موظفو الفرع
        /// </summary>
        [HttpGet("{branchId}/employees")]
        public IActionResult GetBranchEmployees(long branchId)
        {
            var response = _branchService.GetBranchEmployees(branchId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get branch statistics - إحصائيات الفرع
        /// </summary>
        [HttpGet("{branchId}/statistics")]
        public IActionResult GetBranchStatistics(long branchId)
        {
            var response = _branchService.GetBranchStatistics(branchId);
            return SUCCESS(response);
        }
    }
}

