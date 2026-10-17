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
    /// Purchase Order Management - إدارة أوامر الشراء
    /// </summary>
    [Verify]
    [Route("api/erp/purchase-orders")]
    [ApiController]
    public class ErpPurchaseOrderController : BaseController
    {
        private readonly IErpPurchaseOrderService _purchaseOrderService;

        public ErpPurchaseOrderController(IErpPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        /// <summary>
        /// Get purchase order list - قائمة أوامر الشراء
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ErpPurchaseOrder parm, [FromQuery] PagerInfo pager)
        {
            var response = _purchaseOrderService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get purchase order by ID - الحصول على أمر شراء بالمعرف
        /// </summary>
        [HttpGet("{purchaseOrderId}")]
        public IActionResult GetInfo(long purchaseOrderId)
        {
            var response = _purchaseOrderService.GetInfo(purchaseOrderId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create purchase order - إنشاء أمر شراء
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Purchase Order", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpPurchaseOrder purchaseOrder)
        {
            purchaseOrder.Create_by = HttpContext.GetName();
            purchaseOrder.PurchaseOrderNumber = _purchaseOrderService.GeneratePurchaseOrderNumber(purchaseOrder.BranchID);
            var response = _purchaseOrderService.AddPurchaseOrder(purchaseOrder);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update purchase order - تحديث أمر شراء
        /// </summary>
        [HttpPut]
        [Log(Title = "Update Purchase Order", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpPurchaseOrder purchaseOrder)
        {
            purchaseOrder.Update_by = HttpContext.GetName();
            purchaseOrder.Update_time = DateTime.Now;
            var response = _purchaseOrderService.UpdatePurchaseOrder(purchaseOrder);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete purchase orders - حذف أوامر الشراء
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Delete Purchase Orders", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idArr = Tools.SpitLongArrary(ids);
            var response = _purchaseOrderService.Delete(idArr);
            return ToResponse(response);
        }

        /// <summary>
        /// Approve purchase order - الموافقة على أمر الشراء
        /// </summary>
        [HttpPost("approve/{purchaseOrderId}")]
        [Log(Title = "Approve Purchase Order", BusinessType = BusinessType.UPDATE)]
        public IActionResult ApprovePurchaseOrder(long purchaseOrderId)
        {
            var userId = HttpContext.GetName();
            var response = _purchaseOrderService.ApprovePurchaseOrder(purchaseOrderId, userId);
            return ToResponse(response);
        }

        /// <summary>
        /// Reject purchase order - رفض أمر الشراء
        /// </summary>
        [HttpPost("reject/{purchaseOrderId}")]
        [Log(Title = "Reject Purchase Order", BusinessType = BusinessType.UPDATE)]
        public IActionResult RejectPurchaseOrder(long purchaseOrderId, [FromBody] RejectPurchaseOrderDto dto)
        {
            var response = _purchaseOrderService.RejectPurchaseOrder(purchaseOrderId, dto.Reason);
            return ToResponse(response);
        }

        /// <summary>
        /// Cancel purchase order - إلغاء أمر الشراء
        /// </summary>
        [HttpPost("cancel/{purchaseOrderId}")]
        [Log(Title = "Cancel Purchase Order", BusinessType = BusinessType.UPDATE)]
        public IActionResult CancelPurchaseOrder(long purchaseOrderId, [FromBody] CancelPurchaseOrderDto dto)
        {
            var response = _purchaseOrderService.CancelPurchaseOrder(purchaseOrderId, dto.Reason);
            return ToResponse(response);
        }

        /// <summary>
        /// Receive purchase order - استلام أمر الشراء
        /// </summary>
        [HttpPost("receive/{purchaseOrderId}")]
        [Log(Title = "Receive Purchase Order", BusinessType = BusinessType.UPDATE)]
        public IActionResult ReceivePurchaseOrder(long purchaseOrderId, [FromBody] List<ErpPurchaseOrderItem> receivedItems)
        {
            var response = _purchaseOrderService.ReceivePurchaseOrder(purchaseOrderId, receivedItems);
            return ToResponse(response);
        }

        /// <summary>
        /// Get supplier purchase orders - أوامر شراء المورد
        /// </summary>
        [HttpGet("supplier/{supplierId}")]
        public IActionResult GetSupplierPurchaseOrders(long supplierId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var response = _purchaseOrderService.GetSupplierPurchaseOrders(supplierId, fromDate, toDate);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get pending purchase orders - أوامر الشراء المعلقة
        /// </summary>
        [HttpGet("pending")]
        public IActionResult GetPendingPurchaseOrders([FromQuery] long? branchId)
        {
            var response = _purchaseOrderService.GetPendingPurchaseOrders(branchId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get purchase order items - أصناف أمر الشراء
        /// </summary>
        [HttpGet("{purchaseOrderId}/items")]
        public IActionResult GetPurchaseOrderItems(long purchaseOrderId)
        {
            var response = _purchaseOrderService.GetPurchaseOrderItems(purchaseOrderId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get total purchases - إجمالي المشتريات
        /// </summary>
        [HttpGet("total")]
        public IActionResult GetTotalPurchases([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] long? supplierId)
        {
            var response = _purchaseOrderService.GetTotalPurchases(fromDate, toDate, supplierId);
            return SUCCESS(new { FromDate = fromDate, ToDate = toDate, TotalPurchases = response });
        }

        /// <summary>
        /// Get purchase report - تقرير المشتريات
        /// </summary>
        [HttpGet("report")]
        public IActionResult GetPurchaseReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] long? branchId)
        {
            var response = _purchaseOrderService.GetPurchaseReport(fromDate, toDate, branchId);
            return SUCCESS(response);
        }
    }

    public class RejectPurchaseOrderDto
    {
        public string Reason { get; set; }
    }

    public class CancelPurchaseOrderDto
    {
        public string Reason { get; set; }
    }
}

