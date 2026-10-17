using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Customers.DataMine;
using ZR.Model.Customers.DataMine.Dto;
using ZR.Service.IService.Customers.DataMine;
using ZR.Service.Customers.DataMine;
using MiniExcelLibs;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Model;
using ZR.Common;

namespace ZR.Admin.WebApi.Controllers.Customers.DataMine
{
    [Verify]
    [Route("api/datamine/contacts")]
    public class DmContactController : BaseController
    {
        private readonly IDmContactService _contactService;
        private readonly IDmPhoneRevealService _phoneRevealService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DmContactController(
            IDmContactService contactService,
            IDmPhoneRevealService phoneRevealService,
            IWebHostEnvironment webHostEnvironment)
        {
            _contactService = contactService;
            _phoneRevealService = phoneRevealService;
            _webHostEnvironment = webHostEnvironment;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        /// <summary>
        /// Get paginated list of contacts with filters
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("datamine:contacts:list")]
        public async Task<IActionResult> GetList([FromQuery] DmContactQueryDto query)
        {
            var result = await _contactService.GetListAsync(query);

            // Mask phones for non-admin users
            var currentUser = GetCurrentUser();
            if (!currentUser.IsAdmin())
            {
                var revealedIds = await _phoneRevealService.GetTodayRevealedContactIds(currentUser.UserId);
                DmContactService.MaskPhonesInResults(result.Result?.ToList(), new HashSet<long>(revealedIds));
            }

            return SUCCESS(result);
        }

        /// <summary>
        /// Get a single contact by ID
        /// </summary>
        [HttpGet("{contactId}")]
        [ActionPermissionFilter("datamine:contacts:list")]
        public async Task<IActionResult> GetById(long contactId)
        {
            var contact = await _contactService.GetByIdAsync(contactId);
            if (contact == null)
                return ToResponse(ResultCode.FAIL, "Contact not found");
            return SUCCESS(contact);
        }

        /// <summary>
        /// Get KPI statistics
        /// </summary>
        [HttpGet("statistics")]
        [ActionPermissionFilter("datamine:contacts:list")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _contactService.GetStatisticsAsync();
            return SUCCESS(stats);
        }

        /// <summary>
        /// Download Excel import template
        /// </summary>
        [HttpGet("import-template")]
        [ActionPermissionFilter("datamine:contacts:import")]
        public IActionResult DownloadTemplate()
        {
            var templateData = new List<DmContactExcelImportDto>
            {
                new DmContactExcelImportDto
                {
                    BrandName = "Example Brand",
                    BrandNameAr = "مثال",
                    OwnerName = "John Doe",
                    OwnerPhone = "+966500000000",
                    OwnerEmail = "john@example.com",
                    CompanyName = "Example Co.",
                    Industry = "Food & Beverage",
                    City = "Riyadh",
                    Region = "Central",
                    CompanySize = "Medium",
                    Website = "https://example.com",
                    SocialMedia = "@example",
                    IsMain = "false",
                    Tags = "vip,retail",
                    LeadSource = "Referral",
                    BudgetRange = "100K-500K",
                    ExpectedRevenue = 250000,
                    Notes = "Sample notes",
                    FullAddress = "King Fahd Road, Riyadh"
                }
            };

            var memoryStream = new MemoryStream();
            MiniExcel.SaveAs(memoryStream, templateData);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return File(memoryStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DataMine_Contacts_Import_Template.xlsx");
        }

        /// <summary>
        /// Preview uploaded Excel file before import
        /// </summary>
        [HttpPost("import-preview")]
        [ActionPermissionFilter("datamine:contacts:import")]
        public async Task<IActionResult> PreviewImport([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ToResponse(ResultCode.FAIL, "No file uploaded");

                var ext = global::System.IO.Path.GetExtension(file.FileName).ToLower();
                if (ext != ".xlsx" && ext != ".xls")
                    return ToResponse(ResultCode.FAIL, "Only Excel files (.xlsx, .xls) are allowed");

                var uploadFolder = global::System.IO.Path.Combine(
                    _webHostEnvironment.WebRootPath, "uploads", "datamine");
                global::System.IO.Directory.CreateDirectory(uploadFolder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = global::System.IO.Path.Combine(uploadFolder, fileName);

                using (var stream = new global::System.IO.FileStream(
                    filePath, global::System.IO.FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var preview = await _contactService.PreviewExcelImportAsync(filePath);
                return SUCCESS(new { preview, tempFileName = fileName });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Preview failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Execute import from previously uploaded Excel file
        /// </summary>
        [HttpPost("import")]
        [ActionPermissionFilter("datamine:contacts:import")]
        [Log(Title = "DataMine Contact Import", BusinessType = BusinessType.IMPORT)]
        public async Task<IActionResult> Import([FromBody] DmContactImportRequestWrapper request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TempFileName))
                    return ToResponse(ResultCode.FAIL, "No file specified");

                var uploadFolder = global::System.IO.Path.Combine(
                    _webHostEnvironment.WebRootPath, "uploads", "datamine");
                var filePath = global::System.IO.Path.Combine(uploadFolder, request.TempFileName);

                if (!global::System.IO.File.Exists(filePath))
                    return ToResponse(ResultCode.FAIL, "Import file not found. Please upload again.");

                var result = await _contactService.ImportFromExcelAsync(filePath, request.ImportRequest);

                try
                {
                    if (global::System.IO.File.Exists(filePath))
                        global::System.IO.File.Delete(filePath);
                }
                catch { }

                return SUCCESS(result,
                    $"Import complete: {result.SuccessCount} succeeded, {result.FailedCount} failed, {result.SkippedCount} skipped");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Import failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Export contacts to Excel
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter("datamine:contacts:export")]
        [Log(Title = "DataMine Contact Export", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> Export([FromQuery] DmContactQueryDto query)
        {
            try
            {
                query.PageSize = 10000;
                var paged = await _contactService.GetListAsync(query);
                var contacts = paged.Result;

                if (contacts == null || !contacts.Any())
                    return ToResponse(ResultCode.FAIL, "No contacts found to export");

                var exportData = contacts.Select(c => new
                {
                    c.BrandName,
                    c.BrandNameAr,
                    c.OwnerName,
                    c.OwnerPhone,
                    c.SecondaryPhone,
                    c.OwnerEmail,
                    c.CompanyName,
                    c.Industry,
                    c.City,
                    c.Region,
                    c.FullAddress,
                    c.CompanySize,
                    c.Website,
                    c.SocialMedia,
                    c.DataSource,
                    IsMain = c.IsMain ? "Yes" : "No",
                    c.Tags,
                    c.Categories,
                    c.LeadSource,
                    c.BudgetRange,
                    c.ExpectedRevenue,
                    c.Notes,
                    IsPulled = c.IsPulled ? "Yes" : "No",
                    c.PulledAt,
                    CreatedDate = c.Create_time
                }).ToList();

                var memoryStream = new MemoryStream();
                await MiniExcel.SaveAsAsync(memoryStream, exportData);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return File(memoryStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DataMine_Contacts_Export_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Export failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Pull selected contacts to leads pipeline
        /// </summary>
        [HttpPost("pull-to-leads")]
        [ActionPermissionFilter("datamine:contacts:pull")]
        [Log(Title = "DataMine Pull to Leads", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> PullToLeads([FromBody] PullToLeadsRequestDto request)
        {
            try
            {
                if (request.ContactIds == null || request.ContactIds.Length == 0)
                    return ToResponse(ResultCode.FAIL, "No contacts selected");

                var currentUser = GetCurrentUser();
                var result = await _contactService.PullToLeadsAsync(request, currentUser.UserId);

                return SUCCESS(result,
                    $"Pull complete: {result.SuccessCount} leads created, {result.FailedCount} failed");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Pull to leads failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Reveal a masked phone number (non-admin, daily limit enforced)
        /// </summary>
        [HttpPost("reveal-phone")]
        [ActionPermissionFilter("datamine:contacts:list")]
        public async Task<IActionResult> RevealPhone([FromBody] RevealPhoneRequest request)
        {
            var currentUser = GetCurrentUser();

            // Admins don't need to reveal — they see everything
            if (currentUser.IsAdmin())
            {
                var contact = await _contactService.GetByIdAsync(request.ContactId);
                return SUCCESS(new { phone = contact?.OwnerPhone, remaining = -1 });
            }

            var phone = await _phoneRevealService.RevealPhone(currentUser.UserId, request.ContactId);
            if (phone == null)
            {
                return ToResponse(ResultCode.FAIL, "Daily reveal limit reached (20/day)");
            }

            var remaining = await _phoneRevealService.GetRemainingReveals(currentUser.UserId);
            return SUCCESS(new { phone, remaining });
        }

        /// <summary>
        /// Get current user's reveal status (remaining reveals + already revealed contacts today)
        /// </summary>
        [HttpGet("reveal-status")]
        [ActionPermissionFilter("datamine:contacts:list")]
        public async Task<IActionResult> GetRevealStatus()
        {
            var currentUser = GetCurrentUser();

            if (currentUser.IsAdmin())
            {
                return SUCCESS(new { remaining = -1, limit = -1, revealedContactIds = new List<long>(), isAdmin = true });
            }

            var remaining = await _phoneRevealService.GetRemainingReveals(currentUser.UserId);
            var revealedIds = await _phoneRevealService.GetTodayRevealedContactIds(currentUser.UserId);

            return SUCCESS(new { remaining, limit = 20, revealedContactIds = revealedIds, isAdmin = false });
        }

        /// <summary>
        /// Delete contacts
        /// </summary>
        [HttpPost("delete")]
        [ActionPermissionFilter("datamine:contacts:delete")]
        [Log(Title = "DataMine Contact Delete", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> Delete([FromBody] long[] contactIds)
        {
            if (contactIds == null || contactIds.Length == 0)
                return ToResponse(ResultCode.FAIL, "No contacts specified");

            var count = await _contactService.DeleteAsync(contactIds);
            return SUCCESS(count);
        }
    }

    public class DmContactImportRequestWrapper
    {
        public string TempFileName { get; set; }
        public DmContactImportRequestDto ImportRequest { get; set; }
    }

    public class RevealPhoneRequest
    {
        public long ContactId { get; set; }
    }
}
