using Microsoft.AspNetCore.Mvc;

using ZR.Model.Inventory.Suppliers;
using ZR.Service.Inventory.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Inventory
{
    /// <summary>
    /// Supplier Controller - متحكم الموردين
    /// </summary>
    [Verify]
    [Route("api/erp/suppliers")]
    [ApiController]
    public class ErpSupplierController : BaseController
    {
        private readonly IErpSupplierService _supplierService;

        public ErpSupplierController(IErpSupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:supplier:list")]
        public IActionResult GetList([FromQuery] ErpSupplier parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _supplierService.GetList(parm, pager);
            return SUCCESS(response);
        }

        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:supplier:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _supplierService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "Supplier not found - المورد غير موجود");
            return SUCCESS(response);
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:supplier:add")]
        [Log(Title = "Supplier", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] ErpSupplier model)
        {
            var result = _supplierService.AddSupplier(model);
            return SUCCESS(result);
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:supplier:edit")]
        [Log(Title = "Supplier", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpSupplier model)
        {
            var result = _supplierService.UpdateSupplier(model);
            return ToResponse(result);
        }

        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:supplier:delete")]
        [Log(Title = "Supplier", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_supplierService.Delete(idsArr));
        }

        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "erp:supplier:list")]
        public IActionResult GetActiveSuppliers() => SUCCESS(_supplierService.GetActiveSuppliers());

        [HttpGet("{id}/balance")]
        [ActionPermissionFilter(Permission = "erp:supplier:query")]
        public IActionResult GetBalance(long id)
        {
            var balance = _supplierService.GetSupplierBalance(id);
            return SUCCESS(new { SupplierId = id, Balance = balance });
        }

        [HttpGet("{id}/statement")]
        [ActionPermissionFilter(Permission = "erp:supplier:statement")]
        public IActionResult GetStatement(long id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var response = _supplierService.GetSupplierStatement(id, fromDate, toDate);
            return SUCCESS(response);
        }

        [HttpGet("due-balances")]
        [ActionPermissionFilter(Permission = "erp:supplier:list")]
        public IActionResult GetSuppliersWithDueBalances() => SUCCESS(_supplierService.GetSuppliersWithDueBalances());

        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:supplier:export")]
        [Log(Title = "Supplier", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpSupplier parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _supplierService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "Suppliers", "Suppliers - الموردين");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

