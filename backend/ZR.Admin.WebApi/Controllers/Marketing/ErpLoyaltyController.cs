using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.Marketing;
using ZR.Service.Marketing.IService;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using Infrastructure.Enums;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Loyalty Program Management - إدارة برنامج الولاء
    /// </summary>
    [Verify]
    [Route("api/erp/loyalty")]
    [ApiController]
    public class ErpLoyaltyController : BaseController
    {
        private readonly IErpLoyaltyService _loyaltyService;

        public ErpLoyaltyController(IErpLoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        #region Program Management

        /// <summary>
        /// Get loyalty programs - برامج الولاء
        /// </summary>
        [HttpGet("programs")]
        [ActionPermissionFilter("marketing:loyalty:list")]
        public IActionResult GetPrograms([FromQuery] PagerInfo pager)
        {
            var response = _loyaltyService.GetPrograms(pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get program by ID - الحصول على برنامج بالمعرف
        /// </summary>
        [HttpGet("programs/{programId}")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public IActionResult GetProgramInfo(long programId)
        {
            var response = _loyaltyService.GetProgramInfo(programId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create loyalty program - إنشاء برنامج ولاء
        /// </summary>
        [HttpPost("programs")]
        [ActionPermissionFilter("marketing:loyalty:add")]
        [Log(Title = "Create Loyalty Program", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateProgram([FromBody] ErpLoyaltyProgram program)
        {
            program.Create_by = HttpContext.GetName();
            var response = _loyaltyService.CreateProgram(program);
            return ToResponse(response);
        }

        /// <summary>
        /// Update loyalty program - تحديث برنامج الولاء
        /// </summary>
        [HttpPut("programs")]
        [ActionPermissionFilter("marketing:loyalty:edit")]
        [Log(Title = "Update Loyalty Program", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateProgram([FromBody] ErpLoyaltyProgram program)
        {
            program.Update_by = HttpContext.GetName();
            program.Update_time = DateTime.Now;
            var response = _loyaltyService.UpdateProgram(program);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete loyalty program - حذف برنامج الولاء
        /// </summary>
        [HttpDelete("programs/{programId}")]
        [ActionPermissionFilter("marketing:loyalty:delete")]
        [Log(Title = "Delete Loyalty Program", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteProgram(long programId)
        {
            var response = _loyaltyService.DeleteProgram(programId);
            return ToResponse(response);
        }

        #endregion

        #region Tier Management

        /// <summary>
        /// Get loyalty tiers - مستويات الولاء
        /// </summary>
        [HttpGet("programs/{programId}/tiers")]
        [ActionPermissionFilter("marketing:loyalty:list")]
        public IActionResult GetTiers(long programId)
        {
            var response = _loyaltyService.GetTiers(programId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get tier by ID - الحصول على مستوى بالمعرف
        /// </summary>
        [HttpGet("tiers/{tierId}")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public IActionResult GetTierInfo(long tierId)
        {
            var response = _loyaltyService.GetTierInfo(tierId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Create loyalty tier - إنشاء مستوى ولاء
        /// </summary>
        [HttpPost("tiers")]
        [ActionPermissionFilter("marketing:loyalty:add")]
        [Log(Title = "Create Loyalty Tier", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateTier([FromBody] ErpLoyaltyTier tier)
        {
            tier.Create_by = HttpContext.GetName();
            var response = _loyaltyService.CreateTier(tier);
            return ToResponse(response);
        }

        /// <summary>
        /// Update loyalty tier - تحديث مستوى الولاء
        /// </summary>
        [HttpPut("tiers")]
        [ActionPermissionFilter("marketing:loyalty:edit")]
        [Log(Title = "Update Loyalty Tier", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateTier([FromBody] ErpLoyaltyTier tier)
        {
            tier.Update_by = HttpContext.GetName();
            tier.Update_time = DateTime.Now;
            var response = _loyaltyService.UpdateTier(tier);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete loyalty tier - حذف مستوى الولاء
        /// </summary>
        [HttpDelete("tiers/{tierId}")]
        [ActionPermissionFilter("marketing:loyalty:delete")]
        [Log(Title = "Delete Loyalty Tier", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteTier(long tierId)
        {
            var response = _loyaltyService.DeleteTier(tierId);
            return ToResponse(response);
        }

        #endregion

        #region Member Loyalty

        /// <summary>
        /// Get member loyalties - ولاء الأعضاء
        /// </summary>
        [HttpGet("members")]
        [ActionPermissionFilter("marketing:loyalty:list")]
        public IActionResult GetMemberLoyalties([FromQuery] ErpMemberLoyalty parm, [FromQuery] PagerInfo pager)
        {
            var response = _loyaltyService.GetMemberLoyalties(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get member loyalty - ولاء العضو
        /// </summary>
        [HttpGet("members/{memberId}")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public IActionResult GetMemberLoyalty(long memberId)
        {
            var response = _loyaltyService.GetMemberLoyalty(memberId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Enroll member in loyalty program - تسجيل عضو في برنامج الولاء
        /// </summary>
        [HttpPost("members/enroll")]
        [ActionPermissionFilter("marketing:loyalty:enroll")]
        [Log(Title = "Enroll Member in Loyalty", BusinessType = BusinessType.INSERT)]
        public IActionResult EnrollMember([FromBody] EnrollMemberDto dto)
        {
            var response = _loyaltyService.EnrollMember(dto.MemberId, dto.ProgramId);
            return ToResponse(response);
        }

        /// <summary>
        /// Unenroll member from loyalty program - إلغاء تسجيل عضو من برنامج الولاء
        /// </summary>
        [HttpPost("members/unenroll/{memberId}")]
        [ActionPermissionFilter("marketing:loyalty:unenroll")]
        [Log(Title = "Unenroll Member from Loyalty", BusinessType = BusinessType.UPDATE)]
        public IActionResult UnenrollMember(long memberId)
        {
            var response = _loyaltyService.UnenrollMember(memberId);
            return ToResponse(response);
        }

        /// <summary>
        /// Get member balance - رصيد العضو
        /// </summary>
        [HttpGet("members/{memberId}/balance")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public IActionResult GetMemberBalance(long memberId)
        {
            var response = _loyaltyService.GetMemberBalance(memberId);
            return SUCCESS(new { MemberId = memberId, Balance = response });
        }

        #endregion

        #region Points Management

        /// <summary>
        /// Earn points - كسب نقاط
        /// </summary>
        [HttpPost("points/earn")]
        [ActionPermissionFilter("marketing:loyalty:earn")]
        [Log(Title = "Earn Loyalty Points", BusinessType = BusinessType.INSERT)]
        public IActionResult EarnPoints([FromBody] EarnPointsDto dto)
        {
            var response = _loyaltyService.EarnPoints(
                dto.MemberId,
                dto.Points,
                dto.ReferenceType,
                dto.ReferenceId,
                dto.Amount,
                dto.Description);
            return ToResponse(response);
        }

        /// <summary>
        /// Redeem points - استبدال نقاط
        /// </summary>
        [HttpPost("points/redeem")]
        [ActionPermissionFilter("marketing:loyalty:redeem")]
        [Log(Title = "Redeem Loyalty Points", BusinessType = BusinessType.INSERT)]
        public IActionResult RedeemPoints([FromBody] RedeemPointsDto dto)
        {
            var response = _loyaltyService.RedeemPoints(
                dto.MemberId,
                dto.Points,
                dto.ReferenceType,
                dto.ReferenceId,
                dto.Description);
            return ToResponse(response);
        }

        /// <summary>
        /// Adjust points - تعديل نقاط
        /// </summary>
        [HttpPost("points/adjust")]
        [ActionPermissionFilter("marketing:loyalty:adjust")]
        [Log(Title = "Adjust Loyalty Points", BusinessType = BusinessType.UPDATE)]
        public IActionResult AdjustPoints([FromBody] AdjustPointsDto dto)
        {
            var response = _loyaltyService.AdjustPoints(dto.MemberId, dto.Points, dto.Description);
            return ToResponse(response);
        }

        /// <summary>
        /// Expire points - انتهاء صلاحية النقاط
        /// </summary>
        [HttpPost("points/expire")]
        [ActionPermissionFilter("marketing:loyalty:expire")]
        [Log(Title = "Expire Loyalty Points", BusinessType = BusinessType.UPDATE)]
        public IActionResult ExpirePoints()
        {
            var response = _loyaltyService.ExpirePoints();
            return SUCCESS(new { ExpiredCount = response });
        }

        #endregion

        #region Transactions

        /// <summary>
        /// Get transactions - المعاملات
        /// </summary>
        [HttpGet("transactions")]
        [ActionPermissionFilter("marketing:loyalty:list")]
        public IActionResult GetTransactions([FromQuery] ErpLoyaltyTransaction parm, [FromQuery] PagerInfo pager)
        {
            var response = _loyaltyService.GetTransactions(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get member transactions - معاملات العضو
        /// </summary>
        [HttpGet("members/{memberId}/transactions")]
        [ActionPermissionFilter("marketing:loyalty:query")]
        public IActionResult GetMemberTransactions(long memberId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var response = _loyaltyService.GetMemberTransactions(memberId, fromDate, toDate);
            return SUCCESS(response);
        }

        #endregion

        #region Reports

        /// <summary>
        /// Get loyalty dashboard - لوحة تحكم الولاء
        /// </summary>
        [HttpGet("dashboard")]
        [ActionPermissionFilter("marketing:loyalty:report")]
        public IActionResult GetLoyaltyDashboard()
        {
            var response = _loyaltyService.GetLoyaltyDashboard();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get points report - تقرير النقاط
        /// </summary>
        [HttpGet("reports/points")]
        [ActionPermissionFilter("marketing:loyalty:report")]
        public IActionResult GetPointsReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var response = _loyaltyService.GetPointsReport(fromDate, toDate);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get top members - أفضل الأعضاء
        /// </summary>
        [HttpGet("reports/top-members")]
        [ActionPermissionFilter("marketing:loyalty:report")]
        public IActionResult GetTopMembers([FromQuery] int count = 10)
        {
            var response = _loyaltyService.GetTopMembers(count);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get members with expiring points - أعضاء بنقاط تنتهي قريباً
        /// </summary>
        [HttpGet("reports/expiring-points")]
        [ActionPermissionFilter("marketing:loyalty:report")]
        public IActionResult GetMembersWithExpiringPoints([FromQuery] int days = 30)
        {
            var response = _loyaltyService.GetMembersWithExpiringPoints(days);
            return SUCCESS(response);
        }

        #endregion
    }

    public class EnrollMemberDto
    {
        public long MemberId { get; set; }
        public long ProgramId { get; set; }
    }

    public class EarnPointsDto
    {
        public long MemberId { get; set; }
        public int Points { get; set; }
        public string ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public decimal? Amount { get; set; }
        public string Description { get; set; }
    }

    public class RedeemPointsDto
    {
        public long MemberId { get; set; }
        public int Points { get; set; }
        public string ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public string Description { get; set; }
    }

    public class AdjustPointsDto
    {
        public long MemberId { get; set; }
        public int Points { get; set; }
        public string Description { get; set; }
    }
}

