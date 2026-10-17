using Microsoft.AspNetCore.Mvc;
using ZR.Model.Settings;
using ZR.Service.Settings.IService;
using ZR.Service.Inventory.IService;
using ZR.Service.Subscriptions.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Settings
{
    /// <summary>
    /// Settings Controller - متحكم الإعدادات
    /// </summary>
    [Verify]
    [Route("api/erp/settings")]
    [ApiController]
    public class ErpSettingsController : BaseController
    {
        private readonly IErpBranchService _branchService;
        private readonly IErpPaymentMethodService _paymentMethodService;
        private readonly IErpExpenseTypeService _expenseTypeService;
        private readonly IErpUnitOfMeasureService _unitService;
        private readonly IErpServiceCategoryService _categoryService;

        public ErpSettingsController(
            IErpBranchService branchService,
            IErpPaymentMethodService paymentMethodService,
            IErpExpenseTypeService expenseTypeService,
            IErpUnitOfMeasureService unitService,
            IErpServiceCategoryService categoryService)
        {
            _branchService = branchService;
            _paymentMethodService = paymentMethodService;
            _expenseTypeService = expenseTypeService;
            _unitService = unitService;
            _categoryService = categoryService;
        }

        #region Branches - الفروع
        //[HttpGet("branches")]
        //[ActionPermissionFilter(Permission = "erp:settings:list")]
        //public IActionResult GetBranchesAll([FromQuery] ErpBranch parm, [FromQuery] Model.PagerInfo pager) { 
        
        //    return  SUCCESS(_branchService.GetList(parm, pager));
        //}





        [HttpGet("branches/{id}")]
        [ActionPermissionFilter(Permission = "erp:settings:query")]
        public IActionResult GetBranch(long id) => SUCCESS(_branchService.GetInfo(id));

        [HttpGet("branches/active")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetActiveBranches() => SUCCESS(_branchService.GetActiveBranches());

        [HttpGet("branches/main")]
        [ActionPermissionFilter(Permission = "erp:settings:query")]
        public IActionResult GetMainBranch() => SUCCESS(_branchService.GetMainBranch());

        [HttpPost("branches")]
        [ActionPermissionFilter(Permission = "erp:settings:add")]
        [Log(Title = "Branch", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBranch([FromBody] ErpBranch model) => SUCCESS(_branchService.AddBranch(model));

        [HttpPut("branches")]
        [ActionPermissionFilter(Permission = "erp:settings:edit")]
        [Log(Title = "Branch", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBranch([FromBody] ErpBranch model) => ToResponse(_branchService.UpdateBranch(model));

        [HttpDelete("branches/{ids}")]
        [ActionPermissionFilter(Permission = "erp:settings:delete")]
        [Log(Title = "Branch", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBranches(string ids) => ToResponse(_branchService.Delete(Tools.SpitLongArrary(ids)));
        #endregion

        #region Payment Methods - طرق الدفع
        [HttpGet("payment-methods")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetPaymentMethods([FromQuery] ErpPaymentMethod parm, [FromQuery] Model.PagerInfo pager) => SUCCESS(_paymentMethodService.GetList(parm, pager));

        [HttpGet("payment-methods/active")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetActivePaymentMethods() => SUCCESS(_paymentMethodService.GetActivePaymentMethods());

        [HttpPost("payment-methods")]
        [ActionPermissionFilter(Permission = "erp:settings:add")]
        [Log(Title = "Payment Method", BusinessType = BusinessType.INSERT)]
        public IActionResult AddPaymentMethod([FromBody] ErpPaymentMethod model) => SUCCESS(_paymentMethodService.AddPaymentMethod(model));

        [HttpPut("payment-methods")]
        [ActionPermissionFilter(Permission = "erp:settings:edit")]
        [Log(Title = "Payment Method", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdatePaymentMethod([FromBody] ErpPaymentMethod model) => ToResponse(_paymentMethodService.UpdatePaymentMethod(model));

        [HttpDelete("payment-methods/{ids}")]
        [ActionPermissionFilter(Permission = "erp:settings:delete")]
        [Log(Title = "Payment Method", BusinessType = BusinessType.DELETE)]
        public IActionResult DeletePaymentMethods(string ids) => ToResponse(_paymentMethodService.Delete(Tools.SpitLongArrary(ids)));
        #endregion

        #region Expense Types - أنواع المصروفات
        [HttpGet("expense-types")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetExpenseTypes([FromQuery] ErpExpenseType parm, [FromQuery] Model.PagerInfo pager) => SUCCESS(_expenseTypeService.GetList(parm, pager));

        [HttpGet("expense-types/active")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetActiveExpenseTypes() => SUCCESS(_expenseTypeService.GetActiveExpenseTypes());

        [HttpPost("expense-types")]
        [ActionPermissionFilter(Permission = "erp:settings:add")]
        [Log(Title = "Expense Type", BusinessType = BusinessType.INSERT)]
        public IActionResult AddExpenseType([FromBody] ErpExpenseType model) => SUCCESS(_expenseTypeService.AddExpenseType(model));

        [HttpPut("expense-types")]
        [ActionPermissionFilter(Permission = "erp:settings:edit")]
        [Log(Title = "Expense Type", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateExpenseType([FromBody] ErpExpenseType model) => ToResponse(_expenseTypeService.UpdateExpenseType(model));

        [HttpDelete("expense-types/{ids}")]
        [ActionPermissionFilter(Permission = "erp:settings:delete")]
        [Log(Title = "Expense Type", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteExpenseTypes(string ids) => ToResponse(_expenseTypeService.Delete(Tools.SpitLongArrary(ids)));
        #endregion

        #region Units of Measure - وحدات القياس
        [HttpGet("units")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetUnits([FromQuery] ErpUnitOfMeasure parm, [FromQuery] Model.PagerInfo pager) => SUCCESS(_unitService.GetList(parm, pager));

        [HttpGet("units/active")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetActiveUnits() => SUCCESS(_unitService.GetActiveUnits());

        [HttpPost("units")]
        [ActionPermissionFilter(Permission = "erp:settings:add")]
        [Log(Title = "Unit", BusinessType = BusinessType.INSERT)]
        public IActionResult AddUnit([FromBody] ErpUnitOfMeasure model) => SUCCESS(_unitService.AddUnit(model));

        [HttpPut("units")]
        [ActionPermissionFilter(Permission = "erp:settings:edit")]
        [Log(Title = "Unit", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateUnit([FromBody] ErpUnitOfMeasure model) => ToResponse(_unitService.UpdateUnit(model));

        [HttpDelete("units/{ids}")]
        [ActionPermissionFilter(Permission = "erp:settings:delete")]
        [Log(Title = "Unit", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteUnits(string ids) => ToResponse(_unitService.Delete(Tools.SpitLongArrary(ids)));
        #endregion

        #region Service Categories - فئات الخدمات
        [HttpGet("categories")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetCategories([FromQuery] ErpServiceCategory parm, [FromQuery] Model.PagerInfo pager) => SUCCESS(_categoryService.GetList(parm, pager));

        [HttpGet("categories/active")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetActiveCategories() => SUCCESS(_categoryService.GetActiveCategories());

        [HttpGet("categories/tree")]
        [ActionPermissionFilter(Permission = "erp:settings:list")]
        public IActionResult GetCategoryTree() => SUCCESS(_categoryService.GetCategoryTree());

        [HttpPost("categories")]
        [ActionPermissionFilter(Permission = "erp:settings:add")]
        [Log(Title = "Category", BusinessType = BusinessType.INSERT)]
        public IActionResult AddCategory([FromBody] ErpServiceCategory model) => SUCCESS(_categoryService.AddCategory(model));

        [HttpPut("categories")]
        [ActionPermissionFilter(Permission = "erp:settings:edit")]
        [Log(Title = "Category", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateCategory([FromBody] ErpServiceCategory model) => ToResponse(_categoryService.UpdateCategory(model));

        [HttpDelete("categories/{ids}")]
        [ActionPermissionFilter(Permission = "erp:settings:delete")]
        [Log(Title = "Category", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteCategories(string ids) => ToResponse(_categoryService.Delete(Tools.SpitLongArrary(ids)));
        #endregion
    }
}

