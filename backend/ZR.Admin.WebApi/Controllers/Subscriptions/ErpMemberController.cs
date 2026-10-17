using Microsoft.AspNetCore.Mvc;
using ZR.Model.Subscriptions;
using ZR.Service.Subscriptions.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Subscriptions
{
    /// <summary>
    /// Member Controller - متحكم الأعضاء
    /// </summary>
    [Verify]
    [Route("api/erp/members")]
    [ApiController]
    public class ErpMemberController : BaseController
    {
        private readonly IErpMemberService _memberService;

        public ErpMemberController(IErpMemberService memberService)
        {
            _memberService = memberService;
        }

        /// <summary>
        /// Get member list - قائمة الأعضاء
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:member:list")]
        public IActionResult GetList([FromQuery] ErpMember parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _memberService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get member by ID - الحصول على عضو
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:member:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _memberService.GetInfo(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Member not found - العضو غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get member by code - الحصول على عضو بالرمز
        /// </summary>
        [HttpGet("code/{code}")]
        [ActionPermissionFilter(Permission = "erp:member:query")]
        public IActionResult GetByCode(string code)
        {
            var response = _memberService.GetByCode(code);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Member not found - العضو غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get member by phone - الحصول على عضو بالهاتف
        /// </summary>
        [HttpGet("phone/{phone}")]
        [ActionPermissionFilter(Permission = "erp:member:query")]
        public IActionResult GetByPhone(string phone)
        {
            var response = _memberService.GetByPhone(phone);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Member not found - العضو غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Add member - إضافة عضو
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:member:add")]
        [Log(Title = "Member", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] ErpMember model)
        {
            var result = _memberService.AddMember(model);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update member - تعديل عضو
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:member:edit")]
        [Log(Title = "Member", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpMember model)
        {
            var result = _memberService.UpdateMember(model);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete members - حذف أعضاء
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:member:delete")]
        [Log(Title = "Member", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_memberService.Delete(idsArr));
        }

        /// <summary>
        /// Record visit (check-in) - تسجيل زيارة
        /// </summary>
        [HttpPost("{id}/checkin")]
        [ActionPermissionFilter(Permission = "erp:member:checkin")]
        [Log(Title = "Member Check-in", BusinessType = BusinessType.OTHER)]
        public IActionResult CheckIn(long id, [FromQuery] long? BranchID, [FromQuery] string checkType = "manual")
        {
            var result = _memberService.RecordVisit(id, BranchID, checkType);
            return ToResponse(result);
        }

        /// <summary>
        /// Record check-out - تسجيل خروج
        /// </summary>
        [HttpPost("{id}/checkout")]
        [ActionPermissionFilter(Permission = "erp:member:checkin")]
        [Log(Title = "Member Check-out", BusinessType = BusinessType.OTHER)]
        public IActionResult CheckOut(long id)
        {
            var result = _memberService.RecordCheckOut(id);
            return ToResponse(result);
        }

        /// <summary>
        /// Get visit history - سجل الزيارات
        /// </summary>
        [HttpGet("{id}/visits")]
        [ActionPermissionFilter(Permission = "erp:member:query")]
        public IActionResult GetVisitHistory(long id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = _memberService.GetVisitHistory(id, fromDate, toDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Freeze membership - تجميد العضوية
        /// </summary>
        [HttpPost("{id}/freeze")]
        [ActionPermissionFilter(Permission = "erp:member:freeze")]
        [Log(Title = "Freeze Membership", BusinessType = BusinessType.UPDATE)]
        public IActionResult FreezeMembership(long id, [FromBody] FreezeRequest request)
        {
            var result = _memberService.FreezeMembership(id, request.StartDate, request.EndDate, request.Reason);
            return ToResponse(result);
        }

        /// <summary>
        /// Unfreeze membership - إلغاء التجميد
        /// </summary>
        [HttpPost("{id}/unfreeze")]
        [ActionPermissionFilter(Permission = "erp:member:freeze")]
        [Log(Title = "Unfreeze Membership", BusinessType = BusinessType.UPDATE)]
        public IActionResult UnfreezeMembership(long id)
        {
            var result = _memberService.UnfreezeMembership(id);
            return ToResponse(result);
        }

        /// <summary>
        /// Update member status (blacklist) - تحديث حالة العضو
        /// </summary>
        [HttpPost("{id}/status")]
        [ActionPermissionFilter(Permission = "erp:member:status")]
        [Log(Title = "Member Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateStatus(long id, [FromQuery] bool isBlacklisted)
        {
            var result = _memberService.UpdateMemberStatus(id, isBlacklisted);
            return ToResponse(result);
        }

        /// <summary>
        /// Assign to group - تعيين لمجموعة
        /// </summary>
        [HttpPost("{id}/group")]
        [ActionPermissionFilter(Permission = "erp:member:edit")]
        public IActionResult AssignToGroup(long id, [FromQuery] long groupId)
        {
            var result = _memberService.AssignToGroup(id, groupId);
            return ToResponse(result);
        }

        /// <summary>
        /// Get member balance - رصيد العضو
        /// </summary>
        [HttpGet("{id}/balance")]
        [ActionPermissionFilter(Permission = "erp:member:query")]
        public IActionResult GetBalance(long id)
        {
            var balance = _memberService.GetMemberBalance(id);
            return SUCCESS(new { MemberId = id, Balance = balance });
        }

        /// <summary>
        /// Get active members - الأعضاء النشطين
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "erp:member:list")]
        public IActionResult GetActiveMembers([FromQuery] long? BranchID)
        {
            var result = _memberService.GetActiveMembers(BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get expiring soon members - الأعضاء المنتهية اشتراكاتهم قريباً
        /// </summary>
        [HttpGet("expiring-soon")]
        [ActionPermissionFilter(Permission = "erp:member:list")]
        public IActionResult GetExpiringSoonMembers([FromQuery] int days = 7, [FromQuery] long? BranchID = null)
        {
            var result = _memberService.GetExpiringSoonMembers(days, BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get expired members - الأعضاء المنتهية اشتراكاتهم
        /// </summary>
        [HttpGet("expired")]
        [ActionPermissionFilter(Permission = "erp:member:list")]
        public IActionResult GetExpiredMembers([FromQuery] long? BranchID)
        {
            var result = _memberService.GetExpiredMembers(BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get members by group - الأعضاء حسب المجموعة
        /// </summary>
        [HttpGet("group/{groupId}")]
        [ActionPermissionFilter(Permission = "erp:member:list")]
        public IActionResult GetMembersByGroup(long groupId)
        {
            var result = _memberService.GetMembersByGroup(groupId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get member analytics - تحليلات العضو
        /// </summary>
        [HttpGet("{id}/analytics")]
        [ActionPermissionFilter(Permission = "erp:member:analytics")]
        public async Task<IActionResult> GetAnalytics(long id)
        {
            var result = await _memberService.GetMemberAnalyticsAsync(id);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Member not found - العضو غير موجود");
            return SUCCESS(result);
        }

        /// <summary>
        /// Export members - تصدير الأعضاء
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:member:export")]
        [Log(Title = "Member", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpMember parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _memberService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "Members", "Members - الأعضاء");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class FreezeRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
    }
}

