using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.Inventory;
using ZR.Service.Inventory.IService;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using Infrastructure.Enums;

namespace ZR.Admin.WebApi.Controllers.Inventory
{
    /// <summary>
    /// Stock Movement Management - إدارة حركات المخزون
    /// </summary>
    [Verify]
    [Route("api/erp/stock-movements")]
    [ApiController]
    public class ErpStockMovementController : BaseController
    {
        private readonly IErpInventoryTransactionService _transactionService;

        public ErpStockMovementController(IErpInventoryTransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Get stock movement list - قائمة حركات المخزون
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ErpInventoryTransaction parm, [FromQuery] PagerInfo pager)
        {
            var response = _transactionService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get stock movement by ID - الحصول على حركة مخزون بالمعرف
        /// </summary>
        [HttpGet("{transactionId}")]
        public IActionResult GetInfo(long transactionId)
        {
            var response = _transactionService.GetInfo(transactionId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create stock movement - إنشاء حركة مخزون
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Stock Movement", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpInventoryTransaction transaction)
        {
            transaction.Create_by = HttpContext.GetName();
            var response = _transactionService.CreateTransaction(transaction);
            return SUCCESS(response);
        }

        /// <summary>
        /// Delete stock movements - حذف حركات المخزون
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Stock Movements", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idArr = Tools.SpitLongArrary(ids);
            var response = _transactionService.Delete(idArr);
            return ToResponse(response);
        }

        /// <summary>
        /// Get product transactions - حركات المنتج
        /// </summary>
        [HttpGet("product/{productId}")]
        public IActionResult GetProductTransactions(long productId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var response = _transactionService.GetProductTransactions(productId, fromDate, toDate);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get warehouse transactions - حركات المستودع
        /// </summary>
        [HttpGet("warehouse/{warehouseId}")]
        public IActionResult GetWarehouseTransactions(long warehouseId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var response = _transactionService.GetWarehouseTransactions(warehouseId, fromDate, toDate);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create opening balance - رصيد افتتاحي
        /// </summary>
        [HttpPost("opening-balance")]
        [Log(Title = "Create Opening Balance", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateOpeningBalance([FromBody] OpeningBalanceDto dto)
        {
            var response = _transactionService.CreateOpeningBalance(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.Cost, dto.Reference);
            return ToResponse(response);
        }

        /// <summary>
        /// Create purchase receipt - استلام مشتريات
        /// </summary>
        [HttpPost("purchase-receipt")]
        [Log(Title = "Create Purchase Receipt", BusinessType = BusinessType.INSERT)]
        public IActionResult CreatePurchaseReceipt([FromBody] PurchaseReceiptDto dto)
        {
            var response = _transactionService.CreatePurchaseReceipt(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.Cost, dto.PurchaseId);
            return ToResponse(response);
        }

        /// <summary>
        /// Create sales issue - صرف مبيعات
        /// </summary>
        [HttpPost("sales-issue")]
        [Log(Title = "Create Sales Issue", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateSalesIssue([FromBody] SalesIssueDto dto)
        {
            var response = _transactionService.CreateSalesIssue(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.InvoiceId);
            return ToResponse(response);
        }

        /// <summary>
        /// Create transfer out - تحويل صادر
        /// </summary>
        [HttpPost("transfer-out")]
        [Log(Title = "Create Transfer Out", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateTransferOut([FromBody] TransferDto dto)
        {
            var response = _transactionService.CreateTransferOut(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.TransferId);
            return ToResponse(response);
        }

        /// <summary>
        /// Create transfer in - تحويل وارد
        /// </summary>
        [HttpPost("transfer-in")]
        [Log(Title = "Create Transfer In", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateTransferIn([FromBody] TransferDto dto)
        {
            var response = _transactionService.CreateTransferIn(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.TransferId);
            return ToResponse(response);
        }

        /// <summary>
        /// Create adjustment - تسوية مخزون
        /// </summary>
        [HttpPost("adjustment")]
        [Log(Title = "Create Adjustment", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateAdjustment([FromBody] AdjustmentDto dto)
        {
            var response = _transactionService.CreateAdjustment(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.Reason);
            return ToResponse(response);
        }

        /// <summary>
        /// Create damaged record - تسجيل تالف
        /// </summary>
        [HttpPost("damaged")]
        [Log(Title = "Create Damaged Record", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateDamaged([FromBody] AdjustmentDto dto)
        {
            var response = _transactionService.CreateDamaged(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.Reason);
            return ToResponse(response);
        }

        /// <summary>
        /// Get inventory report - تقرير المخزون
        /// </summary>
        [HttpGet("inventory-report")]
        public IActionResult GetInventoryReport([FromQuery] DateTime asOfDate, [FromQuery] long? warehouseId)
        {
            var response = _transactionService.GetInventoryReport(asOfDate, warehouseId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get movement report - تقرير الحركات
        /// </summary>
        [HttpGet("movement-report")]
        public IActionResult GetMovementReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] long? productId)
        {
            var response = _transactionService.GetMovementReport(fromDate, toDate, productId);
            return SUCCESS(response);
        }
    }

    public class OpeningBalanceDto
    {
        public long ProductId { get; set; }
        public long WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public string Reference { get; set; }
    }

    public class PurchaseReceiptDto
    {
        public long ProductId { get; set; }
        public long WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public long? PurchaseId { get; set; }
    }

    public class SalesIssueDto
    {
        public long ProductId { get; set; }
        public long WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public long? InvoiceId { get; set; }
    }

    public class TransferDto
    {
        public long ProductId { get; set; }
        public long WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public long TransferId { get; set; }
    }

    public class AdjustmentDto
    {
        public long ProductId { get; set; }
        public long WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string Reason { get; set; }
    }
}

