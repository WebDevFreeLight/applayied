using Microsoft.AspNetCore.Mvc;
using ZR.Model.Notification;
using ZR.Service.Notification.IService;
using ZR.Admin.WebApi.Filters;
using ZR.Common;

namespace ZR.Admin.WebApi.Controllers.Notification
{
    /// <summary>
    /// Notification Management - إدارة الإشعارات
    /// </summary>
    [Verify]
    [Route("api/erp/notifications")]
    [ApiController]
    public class ErpNotificationController : BaseController
    {
        private readonly IErpNotificationService _notificationService;

        public ErpNotificationController(IErpNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get notification by ID - الحصول على إشعار بالمعرف
        /// </summary>
        [HttpGet("{notificationId}")]
        public IActionResult GetInfo(long notificationId)
        {
            var response = _notificationService.GetInfo(notificationId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get user notifications - إشعارات المستخدم
        /// </summary>
        [HttpGet("user/{userId}")]
        public IActionResult GetUserNotifications(long userId, [FromQuery] bool unreadOnly = false)
        {
            var response = _notificationService.GetUserNotifications(userId, unreadOnly);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get unread count - عدد الإشعارات غير المقروءة
        /// </summary>
        [HttpGet("unread-count/{userId}")]
        public IActionResult GetUnreadCount(long userId)
        {
            var response = _notificationService.GetUnreadCount(userId);
            return SUCCESS(new { UserId = userId, UnreadCount = response });
        }

        /// <summary>
        /// Mark notification as read - تحديد الإشعار كمقروء
        /// </summary>
        [HttpPost("read/{recipientId}")]
        public IActionResult MarkAsRead(long recipientId)
        {
            long userId = HttpContext.GetUId();
            var response = _notificationService.MarkAsRead(recipientId, userId);
            return ToResponse(response);
        }

        /// <summary>
        /// Mark all notifications as read - تحديد جميع الإشعارات كمقروءة
        /// </summary>
        [HttpPost("read-all/{userId}")]
        public IActionResult MarkAllAsRead(long userId)
        {
            var response = _notificationService.MarkAllAsRead(userId);
            return ToResponse(response);
        }

        /// <summary>
        /// Soft delete a notification for a user - حذف إشعار للمستخدم
        /// </summary>
        [HttpDelete("recipient/{recipientId}")]
        public IActionResult SoftDeleteRecipient(long recipientId)
        {
            long userId = HttpContext.GetUId();
            var response = _notificationService.SoftDeleteRecipient(recipientId, userId);
            return ToResponse(response);
        }
    }
}
