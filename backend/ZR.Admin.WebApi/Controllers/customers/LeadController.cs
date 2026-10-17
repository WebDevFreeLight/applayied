using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Customers.Dto;
using ZR.Model.Customers.Leads;
using ZR.Service.Customers.ICustomersService;
using MiniExcelLibs;
using System.IO;
using Infrastructure;
using Infrastructure.Model;
using ZR.Common;
using ZR.Service.Notification;

namespace ZR.Admin.WebApi.Controllers.Customers
{
    [Verify]
    [Route("api/customers/leads")]
    public class LeadController : BaseController
    {
        private static readonly string[] MarketingAllowedStatuses = { "New", "Contacted", "Qualifying", "Lost" };

        private readonly IBpLeadService _bpLeadService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IBpLeadActivityService _activityService;
        private readonly INotificationDispatcher _notificationDispatcher;

        public LeadController(IBpLeadService bpLeadService, IWebHostEnvironment webHostEnvironment, IBpLeadActivityService activityService, INotificationDispatcher notificationDispatcher)
        {
            _bpLeadService = bpLeadService;
            _webHostEnvironment = webHostEnvironment;
            _activityService = activityService;
            _notificationDispatcher = notificationDispatcher;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        private bool IsCurrentUserAdmin()
        {
            var user = GetCurrentUser();
            return user?.IsAdmin() ?? false;
        }

        [HttpGet("list")]
        [ActionPermissionFilter("marketing:leads:list")]
        public async Task<IActionResult> GetLeadsList([FromQuery] LeadQueryDto dto)
        {
            var currentUser = GetCurrentUser();
            dto.CurrentUserId = currentUser?.UserId;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
            dto.IsCurrentUserAdmin = isAdmin || hasAllDataScope;

            // Marketing only sees pre-qualification leads
            dto.AllowedStatuses = MarketingAllowedStatuses;

            var result = await _bpLeadService.GetLeadsListAsync(dto);
            return SUCCESS(result);
        }

        [HttpGet("{leadId}")]
        [ActionPermissionFilter("marketing:leads:view")]
        public async Task<IActionResult> GetLead(long leadId)
        {
            return SUCCESS(await _bpLeadService.GetLeadByIdAsync(leadId));
        }

        [HttpPost("add")]
        [ActionPermissionFilter("marketing:leads:add")]
        public async Task<IActionResult> AddLead([FromBody] BpLead lead)
        {
            var currentUser = GetCurrentUser();
            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
            bool isManager = isAdmin || hasAllDataScope;

            if (!isManager && lead.AssignedTo == 0)
            {
                lead.Create_by = currentUser?.UserName;
            }

            var createdLead = await _bpLeadService.AddLeadAsync(lead, currentUser?.UserId, isManager);

            // Auto-log lead creation as an activity
            try
            {
                if (createdLead != null && createdLead.AssignedTo > 0)
                {
                    var activityDto = new CreateLeadActivityDto
                    {
                        LeadId = createdLead.LeadId,
                        EmployeeId = createdLead.AssignedTo,
                        ActivityType = "Status Change",
                        ActivityDate = DateTime.Now,
                        Outcome = "New",
                        Notes = "Lead created"
                    };
                    _activityService.CreateActivity(activityDto);
                }
            }
            catch
            {
                // Don't fail lead creation if activity logging fails
            }

            return SUCCESS(createdLead);
        }

        [HttpPost("update")]
        [ActionPermissionFilter("marketing:leads:edit")]
        public async Task<IActionResult> UpdateLead([FromBody] BpLead lead)
        {
            return SUCCESS(await _bpLeadService.UpdateLeadAsync(lead));
        }

        [HttpPost("delete")]
        [ActionPermissionFilter("marketing:leads:delete")]
        public async Task<IActionResult> Delete([FromBody] long[] leadIds)
        {
            return SUCCESS(await _bpLeadService.DeleteLeadsAsync(leadIds));
        }

        [HttpPost("update-status")]
        [ActionPermissionFilter("marketing:leads:edit")]
        public async Task<IActionResult> UpdateLeadStatus([FromBody] UpdateLeadStatusDto dto)
        {
            try
            {
                var lead = await _bpLeadService.GetLeadByIdAsync(dto.LeadId);
                if (lead == null)
                {
                    return ToResponse(ResultCode.FAIL, "العميل المحتمل غير موجود");
                }

                lead.LeadStatus = dto.NewStatus;

                if (dto.NewStatus?.ToLower() == "qualified")
                {
                    // Validate required qualification fields
                    var missingFields = new List<string>();
                    if (string.IsNullOrWhiteSpace(dto.ActivityType)) missingFields.Add("ActivityType");
                    if (string.IsNullOrWhiteSpace(dto.Interesting)) missingFields.Add("Interesting");
                    if (string.IsNullOrWhiteSpace(dto.ContractTypeInterested)) missingFields.Add("ContractTypeInterested");

                    if (missingFields.Any())
                    {
                        return ToResponse(ResultCode.FAIL, $"الحقول التالية مطلوبة عند التأهيل: {string.Join(", ", missingFields)}");
                    }

                    // Set qualification fields
                    lead.ActivityType = dto.ActivityType;
                    lead.Interesting = dto.Interesting;
                    lead.ContractTypeInterested = dto.ContractTypeInterested;

                    // Set optional fields if provided
                    if (!string.IsNullOrWhiteSpace(dto.LocationInterested))
                        lead.LocationInterested = dto.LocationInterested;

                    // Set optional fields if provided
                    if (!string.IsNullOrWhiteSpace(dto.CompanyName))
                        lead.CompanyName = dto.CompanyName;
                    if (!string.IsNullOrWhiteSpace(dto.BrandName))
                        lead.BrandName = dto.BrandName;

                    // Set sales person assignment
                    if (dto.SalesAssignedTo.HasValue && dto.SalesAssignedTo.Value > 0)
                        lead.SalesAssignedTo = dto.SalesAssignedTo.Value;

                    if (!string.IsNullOrEmpty(dto.Notes))
                    {
                        lead.QualificationNotes = dto.Notes;
                    }
                }
                else if (dto.NewStatus?.ToLower() == "lost" && !string.IsNullOrEmpty(dto.Notes))
                {
                    lead.RejectionNotes = dto.Notes;
                }
                else if (!string.IsNullOrEmpty(dto.Notes))
                {
                    var statusNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] Status changed to {dto.NewStatus}: {dto.Notes}";
                    lead.InternalNotes = string.IsNullOrEmpty(lead.InternalNotes)
                        ? statusNote
                        : $"{lead.InternalNotes}\n{statusNote}";
                }

                var result = await _bpLeadService.UpdateLeadAsync(lead);

                // Auto-log status change as an activity
                try
                {
                    var activityDto = new CreateLeadActivityDto
                    {
                        LeadId = lead.LeadId,
                        EmployeeId = lead.AssignedTo > 0 ? lead.AssignedTo : 0,
                        ActivityType = "Status Change",
                        ActivityDate = DateTime.Now,
                        Outcome = dto.NewStatus,
                        Notes = $"Status changed to {dto.NewStatus}" + (!string.IsNullOrEmpty(dto.Notes) ? $": {dto.Notes}" : "")
                    };
                    _activityService.CreateActivity(activityDto);
                }
                catch
                {
                    // Don't fail the status update if activity logging fails
                }

                // Send notification when lead is qualified
                if (dto.NewStatus?.ToLower() == "qualified")
                {
                    try
                    {
                        var request = new NotificationRequest
                        {
                            Payload = new NotificationPayload
                            {
                                Title = $"Lead Qualified: {lead.BrandName ?? lead.OwnerName}",
                                TitleAr = $"تم تأهيل العميل المحتمل: {lead.BrandName ?? lead.OwnerName}",
                                Message = $"Lead '{lead.BrandName ?? lead.OwnerName}' has been qualified and is ready for sales follow-up.",
                                MessageAr = $"تم تأهيل العميل المحتمل '{lead.BrandName ?? lead.OwnerName}' وهو جاهز لمتابعة المبيعات.",
                                NotificationType = "system",
                                ReferenceType = "lead",
                                ReferenceId = lead.LeadId
                            },
                            RoleKeys = new List<string> { "admin", "sale-manager" },
                            EmployeeIds = lead.SalesAssignedTo.HasValue && lead.SalesAssignedTo.Value > 0
                                ? new List<long> { lead.SalesAssignedTo.Value }
                                : new List<long>()
                        };
                        await _notificationDispatcher.SendAsync(request);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LeadController] Failed to send notification: {ex.Message}");
                        NLog.LogManager.GetCurrentClassLogger().Error(ex, "Failed to send lead qualified notification");
                    }
                }

                return SUCCESS(result, "تم تحديث حالة العميل المحتمل بنجاح");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"فشل تحديث حالة العميل المحتمل: {ex.Message}");
            }
        }

        [HttpGet("locations-lookup")]
        [ActionPermissionFilter("marketing:leads:list")]
        public async Task<IActionResult> GetLocationsLookup()
        {
            var locations = await _bpLeadService.GetLocationsLookupAsync();
            return SUCCESS(locations);
        }

        #region Excel Import/Export

        [HttpGet("export")]
        [ActionPermissionFilter("marketing:leads:export")]
        [Log(Title = "Lead Export", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportLeads([FromQuery] LeadQueryDto query)
        {
            try
            {
                var currentUser = GetCurrentUser();
                query.CurrentUserId = currentUser?.UserId;

                bool isAdmin = currentUser?.IsAdmin() ?? false;
                bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
                query.IsCurrentUserAdmin = isAdmin || hasAllDataScope;

                // Marketing only sees pre-qualification leads
                query.AllowedStatuses = MarketingAllowedStatuses;

                var leads = await _bpLeadService.GetLeadsForExportAsync(query);
                if (leads == null || !leads.Any())
                {
                    return ToResponse(ResultCode.FAIL, "No leads found to export");
                }
                var result = await ExportExcelMiniAsync(leads, "Leads", "Leads_Export");
                return ExportExcel(result.Item2, result.Item1);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Export failed: {ex.Message}");
            }
        }

        [HttpGet("import-template")]
        [ActionPermissionFilter("marketing:leads:import")]
        public IActionResult DownloadImportTemplate()
        {
            try
            {
                var template = new List<LeadExcelImportDto>
                {
                    new LeadExcelImportDto
                    {
                        LeadType = "Company",
                        BrandName = "Sample Company Ltd",
                        OwnerName = "John Doe",
                        OwnerPhone = "+966501234567",
                        OwnerEmail = "john@example.com",
                        LeadSource = "Website",
                        BudgetRange = "100000-500000",
                        Priority = "Medium",
                        ExpectedRevenue = 250000,
                        InternalNotes = "Sample lead for template",
                        AssignedToEmployeeCode = "EMP001"
                    }
                };

                var result = DownloadImportTemplate(template, "LeadsImportTemplate");
                return ExportExcel(result.Item2, result.Item1);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Template download failed: {ex.Message}");
            }
        }

        [HttpPost("import-preview")]
        [ActionPermissionFilter("marketing:leads:import")]
        public async Task<IActionResult> PreviewImport([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ToResponse(ResultCode.FAIL, "No file uploaded");

                var ext = global::System.IO.Path.GetExtension(file.FileName).ToLower();
                if (ext != ".xlsx" && ext != ".xls")
                    return ToResponse(ResultCode.FAIL, "Only Excel files (.xlsx, .xls) are allowed");

                var uploadFolder = global::System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "leads");
                global::System.IO.Directory.CreateDirectory(uploadFolder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = global::System.IO.Path.Combine(uploadFolder, fileName);

                using (var stream = new global::System.IO.FileStream(filePath, global::System.IO.FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var preview = await _bpLeadService.PreviewExcelImportAsync(filePath);

                return SUCCESS(new { preview, tempFileName = fileName });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Preview failed: {ex.Message}");
            }
        }

        [HttpPost("import")]
        [ActionPermissionFilter("marketing:leads:import")]
        [Log(Title = "Lead Import", BusinessType = BusinessType.IMPORT)]
        public async Task<IActionResult> ImportLeads([FromBody] ImportLeadsRequestWrapper request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TempFileName))
                    return ToResponse(ResultCode.FAIL, "No file specified for import");

                var uploadFolder = global::System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "leads");
                var filePath = global::System.IO.Path.Combine(uploadFolder, request.TempFileName);

                if (!global::System.IO.File.Exists(filePath))
                    return ToResponse(ResultCode.FAIL, "Import file not found. Please upload again.");

                var result = await _bpLeadService.ImportLeadsFromExcelAsync(filePath, request.ImportRequest);

                try
                {
                    if (global::System.IO.File.Exists(filePath))
                        global::System.IO.File.Delete(filePath);
                }
                catch { }

                return SUCCESS(result, $"Import completed: {result.SuccessCount} succeeded, {result.FailedCount} failed, {result.SkippedCount} skipped");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Import failed: {ex.Message}");
            }
        }

        #endregion
    }

    public class ImportLeadsRequestWrapper
    {
        public string TempFileName { get; set; }
        public LeadImportRequestDto ImportRequest { get; set; }
    }
}
