using Microsoft.AspNetCore.Mvc;
using ZR.Model.Inventory;
using ZR.Service.Inventory.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Inventory
{
    /// <summary>
    /// Warehouse Controller - متحكم المستودعات
    /// </summary>
    [Verify]
    [Route("api/erp/warehouses")]
    [ApiController]
    public class ErpWarehouseController : BaseController
    {
        private readonly IErpWarehouseService _warehouseService;

        public ErpWarehouseController(IErpWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:warehouse:list")]
        public IActionResult GetList([FromQuery] ErpWarehouse parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _warehouseService.GetList(parm, pager);
            return SUCCESS(response);
        }

        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:warehouse:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _warehouseService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "Warehouse not found - المستودع غير موجود");
            return SUCCESS(response);
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:warehouse:add")]
        [Log(Title = "Warehouse", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] ErpWarehouse model)
        {
            var result = _warehouseService.AddWarehouse(model);
            return SUCCESS(result);
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:warehouse:edit")]
        [Log(Title = "Warehouse", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpWarehouse model)
        {
            var result = _warehouseService.UpdateWarehouse(model);
            return ToResponse(result);
        }

        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:warehouse:delete")]
        [Log(Title = "Warehouse", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_warehouseService.Delete(idsArr));
        }

        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "erp:warehouse:list")]
        public IActionResult GetActiveWarehouses([FromQuery] long? BranchID) => SUCCESS(_warehouseService.GetActiveWarehouses(BranchID));

        [HttpGet("{id}/inventory")]
        [ActionPermissionFilter(Permission = "erp:warehouse:query")]
        public IActionResult GetInventory(long id) => SUCCESS(_warehouseService.GetWarehouseStock(id));

        [HttpPost("transfer")]
        [ActionPermissionFilter(Permission = "erp:warehouse:transfer")]
        [Log(Title = "Warehouse Transfer", BusinessType = BusinessType.OTHER)]
        public IActionResult Transfer([FromBody] WarehouseTransferRequest request)
        {
            var result = _warehouseService.TransferStock(request.FromWarehouseId, request.ToWarehouseId, request.ProductId, request.Quantity, request.Notes);
            return ToResponse(result);
        }

        [HttpGet("{id}/value")]
        [ActionPermissionFilter(Permission = "erp:warehouse:report")]
        public IActionResult GetInventoryValue(long id)
        {
            var value = _warehouseService.GetWarehouseValue(id);
            return SUCCESS(new { WarehouseId = id, TotalValue = value });
        }

        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:warehouse:export")]
        [Log(Title = "Warehouse", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpWarehouse parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _warehouseService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "Warehouses", "Warehouses - المستودعات");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class WarehouseTransferRequest
    {
        public long FromWarehouseId { get; set; }
        public long ToWarehouseId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string Notes { get; set; }
    }
}

