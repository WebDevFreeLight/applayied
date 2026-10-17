using Microsoft.AspNetCore.Mvc;
using ZR.Model.Subscriptions;
using ZR.Service.Subscriptions.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Subscriptions
{
    /// <summary>
    /// Subscription Controller - متحكم الاشتراكات
    /// </summary>
    [Verify]
    [Route("api/erp/subscriptions")]
    [ApiController]
    public class ErpSubscriptionController : BaseController
    {
        private readonly IErpSubscriptionService _subscriptionService;

        public ErpSubscriptionController(IErpSubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Get subscription list - قائمة الاشتراكات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:subscription:list")]
        public IActionResult GetList([FromQuery] ErpSubscription parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _subscriptionService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get subscription by ID - الحصول على اشتراك
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:subscription:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _subscriptionService.GetInfo(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Subscription not found - الاشتراك غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create subscription - إنشاء اشتراك
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:subscription:add")]
        [Log(Title = "Subscription", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpSubscription model)
        {
            var result = _subscriptionService.CreateSubscription(model);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update subscription - تعديل اشتراك
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:subscription:edit")]
        [Log(Title = "Subscription", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpSubscription model)
        {
            var result = _subscriptionService.UpdateSubscription(model);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete subscriptions - حذف اشتراكات
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:subscription:delete")]
        [Log(Title = "Subscription", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_subscriptionService.Delete(idsArr));
        }

        /// <summary>
        /// Activate subscription - تفعيل الاشتراك
        /// </summary>
        [HttpPost("{id}/activate")]
        [ActionPermissionFilter(Permission = "erp:subscription:activate")]
        [Log(Title = "Activate Subscription", BusinessType = BusinessType.UPDATE)]
        public IActionResult Activate(long id)
        {
            var result = _subscriptionService.ActivateSubscription(id);
            return ToResponse(result);
        }

        /// <summary>
        /// Cancel subscription - إلغاء الاشتراك
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ActionPermissionFilter(Permission = "erp:subscription:cancel")]
        [Log(Title = "Cancel Subscription", BusinessType = BusinessType.UPDATE)]
        public IActionResult Cancel(long id, [FromBody] CancelSubscriptionRequest request)
        {
            var result = _subscriptionService.CancelSubscription(id, request.Reason);
            return ToResponse(result);
        }

        /// <summary>
        /// Freeze subscription - تجميد الاشتراك
        /// </summary>
        [HttpPost("{id}/freeze")]
        [ActionPermissionFilter(Permission = "erp:subscription:freeze")]
        [Log(Title = "Freeze Subscription", BusinessType = BusinessType.UPDATE)]
        public IActionResult Freeze(long id, [FromBody] FreezeSubscriptionRequest request)
        {
            var result = _subscriptionService.FreezeSubscription(id, request.StartDate, request.EndDate, request.Reason);
            return ToResponse(result);
        }

        /// <summary>
        /// Unfreeze subscription - إلغاء تجميد الاشتراك
        /// </summary>
        [HttpPost("{id}/unfreeze")]
        [ActionPermissionFilter(Permission = "erp:subscription:freeze")]
        [Log(Title = "Unfreeze Subscription", BusinessType = BusinessType.UPDATE)]
        public IActionResult Unfreeze(long id)
        {
            var result = _subscriptionService.UnfreezeSubscription(id);
            return ToResponse(result);
        }

        /// <summary>
        /// Renew subscription - تجديد الاشتراك
        /// </summary>
        [HttpPost("{id}/renew")]
        [ActionPermissionFilter(Permission = "erp:subscription:renew")]
        [Log(Title = "Renew Subscription", BusinessType = BusinessType.INSERT)]
        public IActionResult Renew(long id, [FromBody] RenewSubscriptionRequest request)
        {
            var result = _subscriptionService.RenewSubscription(id, request.AdditionalDays);
            return ToResponse(result);
        }

        /// <summary>
        /// Get member subscriptions - اشتراكات العضو
        /// </summary>
        [HttpGet("member/{memberId}")]
        [ActionPermissionFilter(Permission = "erp:subscription:list")]
        public IActionResult GetMemberSubscriptions(long memberId)
        {
            var result = _subscriptionService.GetMemberSubscriptions(memberId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get active subscription - الاشتراك النشط للعضو
        /// </summary>
        [HttpGet("member/{memberId}/active")]
        [ActionPermissionFilter(Permission = "erp:subscription:query")]
        public IActionResult GetActiveSubscription(long memberId)
        {
            var result = _subscriptionService.GetActiveSubscription(memberId);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "No active subscription - لا يوجد اشتراك نشط");
            return SUCCESS(result);
        }

        /// <summary>
        /// Get remaining days of active subscription - الأيام المتبقية للاشتراك
        /// </summary>
        [HttpGet("{id}/remaining-days")]
        [ActionPermissionFilter(Permission = "erp:subscription:query")]
        public IActionResult GetRemainingDays(long id)
        {
            var remaining = _subscriptionService.GetRemainingDays(id);
            return SUCCESS(new { SubscriptionId = id, RemainingDays = remaining });
        }

        /// <summary>
        /// Get expiring subscriptions - الاشتراكات المنتهية قريباً
        /// </summary>
        [HttpGet("expiring")]
        [ActionPermissionFilter(Permission = "erp:subscription:list")]
        public IActionResult GetExpiringSubscriptions([FromQuery] int days = 7, [FromQuery] long? BranchID = null)
        {
            var result = _subscriptionService.GetExpiringSubscriptions(days, BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get expired subscriptions - الاشتراكات المنتهية
        /// </summary>
        [HttpGet("expired")]
        [ActionPermissionFilter(Permission = "erp:subscription:list")]
        public IActionResult GetExpiredSubscriptions([FromQuery] long? BranchID)
        {
            var result = _subscriptionService.GetExpiredSubscriptions(BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Export subscriptions - تصدير الاشتراكات
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:subscription:export")]
        [Log(Title = "Subscription", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpSubscription parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _subscriptionService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "Subscriptions", "Subscriptions - الاشتراكات");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class CancelSubscriptionRequest { public string Reason { get; set; } }
    public class FreezeSubscriptionRequest { public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public string Reason { get; set; } }
    public class RenewSubscriptionRequest { public int AdditionalDays { get; set; } }
}

