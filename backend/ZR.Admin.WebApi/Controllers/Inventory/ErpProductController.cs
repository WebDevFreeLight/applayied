using Microsoft.AspNetCore.Mvc;
using ZR.Model.Inventory;
using ZR.Service.Inventory.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Inventory
{
    /// <summary>
    /// Product Controller - متحكم المنتجات
    /// </summary>
    [Verify]
    [Route("api/erp/products")]
    [ApiController]
    public class ErpProductController : BaseController
    {
        private readonly IErpProductService _productService;

        public ErpProductController(IErpProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:product:list")]
        public IActionResult GetList([FromQuery] ErpProduct parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _productService.GetList(parm, pager);
            return SUCCESS(response);
        }

        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:product:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _productService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "Product not found - المنتج غير موجود");
            return SUCCESS(response);
        }

        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:product:add")]
        [Log(Title = "Product", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] ErpProduct model)
        {
            var result = _productService.AddProduct(model);
            return SUCCESS(result);
        }

        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:product:edit")]
        [Log(Title = "Product", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpProduct model)
        {
            var result = _productService.UpdateProduct(model);
            return ToResponse(result);
        }

        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:product:delete")]
        [Log(Title = "Product", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_productService.Delete(idsArr));
        }

        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "erp:product:list")]
        public IActionResult GetActiveProducts() => SUCCESS(_productService.GetActiveProducts());

        [HttpGet("category/{categoryId}")]
        [ActionPermissionFilter(Permission = "erp:product:list")]
        public IActionResult GetByCategory(long categoryId) => SUCCESS(_productService.GetProductsByCategory(categoryId));

        [HttpGet("{id}/stock")]
        [ActionPermissionFilter(Permission = "erp:product:query")]
        public IActionResult GetStock(long id, [FromQuery] long? warehouseId)
        {
            var stock = _productService.GetProductStock(id, warehouseId);
            return SUCCESS(new { ProductId = id, Stock = stock });
        }

        [HttpPost("{id}/stock")]
        [ActionPermissionFilter(Permission = "erp:product:stock")]
        [Log(Title = "Update Stock", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateStock(long id, [FromBody] StockUpdateRequest request)
        {
            var result = _productService.UpdateStock(id, request.WarehouseId, request.Quantity, request.TransactionType, request.Reference);
            return ToResponse(result);
        }

        [HttpGet("low-stock")]
        [ActionPermissionFilter(Permission = "erp:product:list")]
        public IActionResult GetLowStockProducts() => SUCCESS(_productService.GetLowStockProducts());

        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:product:export")]
        [Log(Title = "Product", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpProduct parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _productService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "Products", "Products - المنتجات");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class StockUpdateRequest
    {
        public long WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string TransactionType { get; set; }
        public string Reference { get; set; }
    }
}

