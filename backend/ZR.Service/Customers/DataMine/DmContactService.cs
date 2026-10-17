using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZR.Model.Customers.DataMine;
using ZR.Model.Customers.DataMine.Dto;
using ZR.Model.Customers.Leads;
using ZR.Model.HR.EmployeeCore;
using ZR.Repository;
using ZR.Service.IService.Customers.DataMine;
using ZR.Service.HR.IService;
using MiniExcelLibs;
using System.IO;

namespace ZR.Service.Customers.DataMine
{
    [AppService(ServiceType = typeof(IDmContactService), ServiceLifetime = LifeTime.Transient)]
    public class DmContactService : BaseService<DmContact>, IDmContactService
    {
        private readonly IHrEmployeeCoreService _employeeService;

        public DmContactService(IHrEmployeeCoreService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<PagedInfo<DmContact>> GetListAsync(DmContactQueryDto query)
        {
            var pageQuery = Context.Queryable<DmContact>()
                .WhereIF(!string.IsNullOrEmpty(query.SearchTerm), c =>
                    c.BrandName.Contains(query.SearchTerm) ||
                    c.OwnerName.Contains(query.SearchTerm) ||
                    c.OwnerPhone.Contains(query.SearchTerm) ||
                    c.OwnerEmail.Contains(query.SearchTerm) ||
                    c.CompanyName.Contains(query.SearchTerm) ||
                    c.FullAddress.Contains(query.SearchTerm))
                .WhereIF(!string.IsNullOrEmpty(query.Industry), c => c.Industry == query.Industry)
                .WhereIF(!string.IsNullOrEmpty(query.City), c => c.City == query.City)
                .WhereIF(!string.IsNullOrEmpty(query.Region), c => c.Region == query.Region)
                .WhereIF(!string.IsNullOrEmpty(query.DataSource), c => c.DataSource == query.DataSource)
                .WhereIF(query.IsPulled.HasValue, c => c.IsPulled == query.IsPulled.Value)
                .WhereIF(query.IsMain.HasValue, c => c.IsMain == query.IsMain.Value)
                .WhereIF(query.ExistsInLeads == true, c => SqlFunc.Subqueryable<BpLead>().Where(l => l.OwnerPhone == c.OwnerPhone).Any())
                .WhereIF(query.ExistsInLeads == false, c => !SqlFunc.Subqueryable<BpLead>().Where(l => l.OwnerPhone == c.OwnerPhone).Any())
                .WhereIF(!string.IsNullOrEmpty(query.Tags), c => c.Tags.Contains(query.Tags))
                .WhereIF(!string.IsNullOrEmpty(query.Categories), c => c.Categories.Contains(query.Categories))
                .OrderByDescending(c => c.Create_time);

            var paged = await pageQuery.ToPageAsync(query);

            // Check which contacts already exist as leads (by phone)
            if (paged.Result != null && paged.Result.Any())
            {
                var phones = paged.Result
                    .Where(c => !string.IsNullOrEmpty(c.OwnerPhone))
                    .Select(c => c.OwnerPhone)
                    .Distinct()
                    .ToList();

                if (phones.Any())
                {
                    var matchedLeads = await Context.Queryable<BpLead>()
                        .Where(l => phones.Contains(l.OwnerPhone))
                        .Select(l => new { l.OwnerPhone, l.LeadStatus })
                        .ToListAsync();

                    var leadLookup = matchedLeads
                        .GroupBy(l => l.OwnerPhone)
                        .ToDictionary(g => g.Key, g => g.First().LeadStatus);

                    foreach (var contact in paged.Result)
                    {
                        if (!string.IsNullOrEmpty(contact.OwnerPhone) && leadLookup.TryGetValue(contact.OwnerPhone, out var status))
                        {
                            contact.ExistsInLeads = true;
                            contact.ExistingLeadStatus = status;
                        }
                    }
                }
            }

            return paged;
        }

        public async Task<DmContact> GetByIdAsync(long contactId)
        {
            return await Context.Queryable<DmContact>()
                .Where(c => c.ContactId == contactId)
                .FirstAsync();
        }

        public async Task<DmContactImportPreviewDto> PreviewExcelImportAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception("Excel file not found");

            var rowsList = await ReadExcelWithFlexibleColumns(filePath);

            var result = new DmContactImportPreviewDto
            {
                TotalRows = rowsList.Count,
                PreviewData = new List<DmContactExcelImportDto>(),
                ValidationErrors = new List<string>()
            };

            int rowNumber = 1;
            foreach (var row in rowsList)
            {
                rowNumber++;
                var validationError = ValidateContactImportRow(row, rowNumber);
                row.ValidationError = validationError;

                result.PreviewData.Add(row);

                if (!string.IsNullOrEmpty(validationError))
                {
                    result.ValidationErrors.Add($"Row {rowNumber}: {validationError}");
                }
            }

            result.ValidRows = result.PreviewData.Count(r => r.IsValid);
            result.InvalidRows = result.PreviewData.Count(r => !r.IsValid);

            return result;
        }

        public async Task<DmContactImportResultDto> ImportFromExcelAsync(string filePath, DmContactImportRequestDto request)
        {
            if (!File.Exists(filePath))
                throw new Exception("Excel file not found");

            var rowsList = await ReadExcelWithFlexibleColumns(filePath);

            var result = new DmContactImportResultDto
            {
                TotalRows = rowsList.Count,
                SuccessCount = 0,
                FailedCount = 0,
                SkippedCount = 0,
                Errors = new List<string>(),
                ImportedContactIds = new List<long>()
            };

            var importedPhones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int rowNumber = 1;
            foreach (var row in rowsList)
            {
                rowNumber++;
                try
                {
                    var validationError = ValidateContactImportRow(row, rowNumber);

                    if (!string.IsNullOrEmpty(validationError))
                    {
                        if (request.SkipInvalidRows)
                        {
                            result.SkippedCount++;
                            result.Errors.Add($"Row {rowNumber} skipped: {validationError}");
                            continue;
                        }
                        else
                        {
                            throw new Exception($"Row {rowNumber}: {validationError}");
                        }
                    }

                    // Deduplicate by phone within the import file
                    if (importedPhones.Contains(row.OwnerPhone))
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {rowNumber} skipped: Duplicate phone in import file ({row.OwnerPhone})");
                        continue;
                    }

                    // Check for existing contact in DB by phone
                    var existingContact = await Context.Queryable<DmContact>()
                        .Where(c => c.OwnerPhone == row.OwnerPhone)
                        .FirstAsync();

                    if (existingContact != null)
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {rowNumber} skipped: Contact already exists (Phone: {row.OwnerPhone})");
                        continue;
                    }

                    var contact = new DmContact
                    {
                        ContactId = SnowFlakeSingle.Instance.NextId(),
                        BrandName = string.IsNullOrWhiteSpace(row.BrandName) ? "Unknown" : row.BrandName,
                        BrandNameAr = row.BrandNameAr,
                        OwnerName = string.IsNullOrWhiteSpace(row.OwnerName) ? "Unknown" : row.OwnerName,
                        OwnerPhone = row.OwnerPhone,
                        SecondaryPhone = row.SecondaryPhone,
                        OwnerEmail = row.OwnerEmail,
                        CompanyName = row.CompanyName,
                        Industry = row.Industry,
                        City = !string.IsNullOrWhiteSpace(request.City) ? request.City : row.City,
                        Region = row.Region,
                        CompanySize = row.CompanySize,
                        Website = row.Website,
                        SocialMedia = row.SocialMedia,
                        DataSource = !string.IsNullOrWhiteSpace(request.DataSource) ? request.DataSource : "Excel Import",
                        IsMain = !string.IsNullOrWhiteSpace(row.IsMain) &&
                                 (row.IsMain.ToLower() == "true" || row.IsMain == "1" || row.IsMain.ToLower() == "yes"),
                        Tags = row.Tags,
                        LeadSource = row.LeadSource,
                        BudgetRange = row.BudgetRange,
                        ExpectedRevenue = row.ExpectedRevenue,
                        Notes = row.Notes,
                        FullAddress = row.FullAddress,
                        Categories = !string.IsNullOrWhiteSpace(request.Categories) ? request.Categories : null,
                        IsPulled = false,
                        Create_time = DateTime.Now
                    };

                    Add(contact);
                    result.SuccessCount++;
                    result.ImportedContactIds.Add(contact.ContactId);
                    importedPhones.Add(row.OwnerPhone);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Row {rowNumber} failed: {ex.Message}");
                    if (!request.SkipInvalidRows) throw;
                }
            }

            return result;
        }

        public async Task<PullToLeadsResultDto> PullToLeadsAsync(PullToLeadsRequestDto request, long currentUserId)
        {
            var result = new PullToLeadsResultDto
            {
                SuccessCount = 0,
                FailedCount = 0,
                Errors = new List<string>(),
                CreatedLeadIds = new List<long>()
            };

            // Determine assignee
            long assigneeId = 0;
            if (request.AssignedToEmployeeId.HasValue && request.AssignedToEmployeeId.Value > 0)
            {
                assigneeId = request.AssignedToEmployeeId.Value;
            }
            else
            {
                // Auto-assign to current user's employee record
                var employee = await Context.Queryable<HrEmployeeCore>()
                    .Where(e => e.UserId.HasValue && e.UserId.Value == currentUserId)
                    .FirstAsync();

                if (employee != null)
                {
                    assigneeId = employee.EmployeeId;
                }
            }

            // Load selected contacts that haven't been pulled yet
            var contacts = await Context.Queryable<DmContact>()
                .Where(c => request.ContactIds.Contains(c.ContactId) && c.IsPulled == false)
                .ToListAsync();

            foreach (var contact in contacts)
            {
                try
                {
                    // Create a new BpLead from the contact
                    var lead = new BpLead
                    {
                        LeadId = SnowFlakeSingle.Instance.NextId(),
                        LeadType = !string.IsNullOrWhiteSpace(contact.CompanyName) ? 1 : 2,
                        BrandName = contact.BrandName,
                        OwnerName = contact.OwnerName,
                        OwnerPhone = contact.OwnerPhone,
                        OwnerEmail = contact.OwnerEmail,
                        CompanyName = contact.CompanyName,
                        LeadSource = "Data Mine",
                        BudgetRange = contact.BudgetRange,
                        ExpectedRevenue = contact.ExpectedRevenue,
                        InternalNotes = contact.Notes,
                        AssignedTo = assigneeId,
                        LeadStatus = "New",
                        Priority = "Medium",
                        Create_by = currentUserId.ToString(),
                        Create_time = DateTime.Now
                    };

                    await Context.Insertable(lead).ExecuteCommandAsync();

                    // Mark the contact as pulled
                    contact.IsPulled = true;
                    contact.PulledAt = DateTime.Now;
                    contact.PulledBy = currentUserId.ToString();
                    contact.PulledLeadId = lead.LeadId;
                    contact.Update_time = DateTime.Now;

                    await Context.Updateable(contact)
                        .UpdateColumns(c => new { c.IsPulled, c.PulledAt, c.PulledBy, c.PulledLeadId, c.Update_time })
                        .ExecuteCommandAsync();

                    result.SuccessCount++;
                    result.CreatedLeadIds.Add(lead.LeadId);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Contact {contact.ContactId} failed: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<int> DeleteAsync(long[] contactIds)
        {
            return await Context.Deleteable<DmContact>()
                .Where(c => contactIds.Contains(c.ContactId))
                .ExecuteCommandAsync();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var all = await Context.Queryable<DmContact>().ToListAsync();

            var now = DateTime.Now;
            var thisMonth = now.Month;
            var thisYear = now.Year;

            var totalContacts = all.Count;
            var importedThisMonth = all.Count(c => c.Create_time.Month == thisMonth && c.Create_time.Year == thisYear);
            var alreadyPulled = all.Count(c => c.IsPulled);
            var mainCustomers = all.Count(c => c.IsMain);
            var nonMainCustomers = all.Count(c => !c.IsMain);

            // Count contacts whose phone already exists in bp_lead
            var allPhones = all.Where(c => !string.IsNullOrEmpty(c.OwnerPhone)).Select(c => c.OwnerPhone).Distinct().ToList();
            var phonesInLeads = allPhones.Any()
                ? (await Context.Queryable<BpLead>()
                    .Where(l => allPhones.Contains(l.OwnerPhone))
                    .Select(l => l.OwnerPhone)
                    .Distinct()
                    .ToListAsync())
                    .ToHashSet()
                : new HashSet<string>();
            var existsInLeadsCount = all.Count(c => !string.IsNullOrEmpty(c.OwnerPhone) && phonesInLeads.Contains(c.OwnerPhone));
            var availableToPull = all.Count(c => !c.IsPulled) - existsInLeadsCount;
            if (availableToPull < 0) availableToPull = 0;

            var byIndustry = all
                .Where(c => !string.IsNullOrEmpty(c.Industry))
                .GroupBy(c => c.Industry)
                .Select(g => new { name = g.Key, value = g.Count() })
                .OrderByDescending(x => x.value)
                .Take(10)
                .ToList();

            var byRegion = all
                .Where(c => !string.IsNullOrEmpty(c.Region))
                .GroupBy(c => c.Region)
                .Select(g => new { name = g.Key, value = g.Count() })
                .OrderByDescending(x => x.value)
                .Take(10)
                .ToList();

            var byDataSource = all
                .Where(c => !string.IsNullOrEmpty(c.DataSource))
                .GroupBy(c => c.DataSource)
                .Select(g => new { name = g.Key, value = g.Count() })
                .OrderByDescending(x => x.value)
                .ToList();

            return new
            {
                totalContacts,
                importedThisMonth,
                alreadyPulled,
                availableToPull,
                existsInLeadsCount,
                mainCustomers,
                nonMainCustomers,
                byIndustry,
                byRegion,
                byDataSource
            };
        }

        // ── Flexible Excel Column Mapping ──

        /// <summary>
        /// Reads Excel dynamically and maps columns by flexible name matching.
        /// Supports: Name/BrandName/الاسم → BrandName, Phones/Phone/OwnerPhone/الهاتف → OwnerPhone,
        /// FullAddress/Address/العنوان → FullAddress, etc.
        /// </summary>
        private async Task<List<DmContactExcelImportDto>> ReadExcelWithFlexibleColumns(string filePath)
        {
            var dynamicRows = await MiniExcel.QueryAsync(filePath, useHeaderRow: true);
            var rowsList = new List<DmContactExcelImportDto>();

            foreach (var row in dynamicRows)
            {
                var dict = row as IDictionary<string, object>;
                if (dict == null) continue;

                var dto = new DmContactExcelImportDto();

                foreach (var kvp in dict)
                {
                    var colName = (kvp.Key ?? "").Trim();
                    var value = kvp.Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(value)) continue;

                    switch (NormalizeColumnName(colName))
                    {
                        case "brandname":
                            dto.BrandName = value;
                            break;
                        case "brandnamear":
                            dto.BrandNameAr = value;
                            break;
                        case "ownername":
                            dto.OwnerName = value;
                            break;
                        case "ownerphone":
                            // Apply phone normalization
                            var (primary, secondary) = NormalizePhones(value);
                            dto.OwnerPhone = primary;
                            dto.SecondaryPhone = secondary;
                            break;
                        case "owneremail":
                            dto.OwnerEmail = value;
                            break;
                        case "companyname":
                            dto.CompanyName = value;
                            break;
                        case "industry":
                            dto.Industry = value;
                            break;
                        case "city":
                            dto.City = value;
                            break;
                        case "region":
                            dto.Region = value;
                            break;
                        case "companysize":
                            dto.CompanySize = value;
                            break;
                        case "website":
                            dto.Website = value;
                            break;
                        case "socialmedia":
                            dto.SocialMedia = value;
                            break;
                        case "ismain":
                            dto.IsMain = value;
                            break;
                        case "tags":
                            dto.Tags = value;
                            break;
                        case "leadsource":
                            dto.LeadSource = value;
                            break;
                        case "budgetrange":
                            dto.BudgetRange = value;
                            break;
                        case "expectedrevenue":
                            if (decimal.TryParse(value, out var rev))
                                dto.ExpectedRevenue = rev;
                            break;
                        case "notes":
                            dto.Notes = value;
                            break;
                        case "fulladdress":
                            dto.FullAddress = value;
                            break;
                    }
                }

                // Skip completely empty rows
                if (!IsEmptyRow(dto))
                    rowsList.Add(dto);
            }

            return rowsList;
        }

        /// <summary>
        /// Maps various column header names to a canonical key.
        /// Supports English, Arabic, and common variations.
        /// </summary>
        private string NormalizeColumnName(string colName)
        {
            if (string.IsNullOrWhiteSpace(colName)) return "";

            // Strip whitespace, dashes, underscores and lowercase
            var normalized = colName.Trim().ToLowerInvariant()
                .Replace(" ", "").Replace("-", "").Replace("_", "");

            // Name / BrandName / الاسم → brandname
            if (normalized == "name" || normalized == "brandname" || normalized == "brand"
                || colName.Trim() == "الاسم" || colName.Trim() == "اسم العلامة")
                return "brandname";

            if (normalized == "brandnamear" || normalized == "namear" || normalized == "arabicname"
                || colName.Trim() == "الاسم بالعربي")
                return "brandnamear";

            // OwnerName
            if (normalized == "ownername" || normalized == "owner" || normalized == "contactname"
                || colName.Trim() == "اسم المالك")
                return "ownername";

            // Phones / Phone / OwnerPhone / الهاتف → ownerphone
            if (normalized == "phones" || normalized == "phone" || normalized == "ownerphone"
                || normalized == "phonenumber" || normalized == "mobile" || normalized == "tel"
                || colName.Trim() == "الهاتف" || colName.Trim() == "رقم الهاتف" || colName.Trim() == "الجوال")
                return "ownerphone";

            if (normalized == "email" || normalized == "owneremail" || normalized == "emailaddress"
                || colName.Trim() == "البريد الإلكتروني")
                return "owneremail";

            if (normalized == "company" || normalized == "companyname"
                || colName.Trim() == "اسم الشركة")
                return "companyname";

            if (normalized == "industry" || normalized == "sector"
                || colName.Trim() == "القطاع" || colName.Trim() == "الصناعة")
                return "industry";

            if (normalized == "city" || colName.Trim() == "المدينة")
                return "city";

            if (normalized == "region" || normalized == "area"
                || colName.Trim() == "المنطقة")
                return "region";

            if (normalized == "companysize" || normalized == "size")
                return "companysize";

            if (normalized == "website" || normalized == "url" || normalized == "site"
                || colName.Trim() == "الموقع")
                return "website";

            if (normalized == "socialmedia" || normalized == "social" || normalized == "instagram"
                || colName.Trim() == "وسائل التواصل")
                return "socialmedia";

            if (normalized == "ismain" || normalized == "main" || normalized == "maincustomer")
                return "ismain";

            if (normalized == "tags" || normalized == "tag" || colName.Trim() == "الوسوم")
                return "tags";

            if (normalized == "leadsource" || normalized == "source")
                return "leadsource";

            if (normalized == "budgetrange" || normalized == "budget")
                return "budgetrange";

            if (normalized == "expectedrevenue" || normalized == "revenue")
                return "expectedrevenue";

            if (normalized == "notes" || normalized == "note" || normalized == "remarks"
                || colName.Trim() == "ملاحظات")
                return "notes";

            // FullAddress / Address / العنوان → fulladdress
            if (normalized == "fulladdress" || normalized == "address" || normalized == "location"
                || colName.Trim() == "العنوان" || colName.Trim() == "العنوان الكامل")
                return "fulladdress";

            return normalized;
        }

        // ── Phone Normalization ──

        /// <summary>
        /// Normalizes a phone string that may contain one or two comma-separated numbers.
        /// Handles Saudi local (05X...) → international (+9665X...) conversion.
        /// Returns (primary, secondary) where secondary is null if both numbers are the same.
        /// </summary>
        public static (string primary, string secondary) NormalizePhones(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return (null, null);

            var parts = raw.Split(new[] { ',', '،', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return (null, null);

            var phone1 = NormalizeSinglePhone(parts[0]);

            if (parts.Length == 1)
                return (phone1, null);

            var phone2 = NormalizeSinglePhone(parts[1]);

            // Compare normalized forms — if same number, return only primary
            if (AreSamePhone(phone1, phone2))
                return (phone1, null);

            // Different numbers — return both
            return (phone1, phone2);
        }

        /// <summary>
        /// Normalizes a single phone number: strips spaces/dashes,
        /// converts Saudi local 05X → +9665X format.
        /// </summary>
        private static string NormalizeSinglePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            // Strip spaces, dashes, dots, parentheses
            var cleaned = Regex.Replace(phone.Trim(), @"[\s\-\.\(\)]+", "");

            // Handle Saudi local format: 05XXXXXXXX → +9665XXXXXXXX
            if (cleaned.StartsWith("05") && cleaned.Length == 10)
            {
                cleaned = "+966" + cleaned.Substring(1); // drop leading 0
            }
            // Handle 9665XXXXXXXX without + prefix
            else if (cleaned.StartsWith("9665") && cleaned.Length == 12)
            {
                cleaned = "+" + cleaned;
            }
            // Handle 009665XXXXXXXX
            else if (cleaned.StartsWith("009665"))
            {
                cleaned = "+" + cleaned.Substring(2);
            }

            return cleaned;
        }

        /// <summary>
        /// Compares two normalized phones — considers them the same if the
        /// digits (ignoring leading +) match.
        /// </summary>
        private static bool AreSamePhone(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;

            var digitsA = Regex.Replace(a, @"[^\d]", "");
            var digitsB = Regex.Replace(b, @"[^\d]", "");

            return digitsA == digitsB;
        }

        // ── Phone Masking ──

        /// <summary>
        /// Masks a phone number: keeps first 5 chars + last 2 digits, masks the rest with *.
        /// e.g. "+966 512 345 678" → "+966 5** *** *78"
        /// </summary>
        public static string MaskPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;

            // Strip spaces for processing
            var digits = phone.Replace(" ", "");
            if (digits.Length <= 7) return phone; // too short to mask meaningfully

            var prefix = digits.Substring(0, 5);  // e.g. "+9665"
            var suffix = digits.Substring(digits.Length - 2); // e.g. "78"
            var middleLen = digits.Length - 5 - 2;
            var masked = prefix + new string('*', middleLen) + suffix;

            // Re-format with spaces like original Saudi format: +966 5** *** *78
            if (masked.StartsWith("+") && masked.Length >= 13)
            {
                return masked.Substring(0, 4) + " " + masked.Substring(4, 3) + " " + masked.Substring(7, 3) + " " + masked.Substring(10);
            }
            return masked;
        }

        /// <summary>
        /// Masks OwnerPhone and SecondaryPhone in a list of contacts,
        /// except for contacts whose IDs are in the revealedContactIds set.
        /// </summary>
        public static void MaskPhonesInResults(List<DmContact> contacts, HashSet<long> revealedContactIds)
        {
            if (contacts == null) return;
            foreach (var c in contacts)
            {
                if (revealedContactIds != null && revealedContactIds.Contains(c.ContactId))
                    continue;

                c.OwnerPhone = MaskPhone(c.OwnerPhone);
                c.SecondaryPhone = MaskPhone(c.SecondaryPhone);
            }
        }

        // ── Validation Helpers ──

        private string ValidateContactImportRow(DmContactExcelImportDto row, int rowNumber)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(row.BrandName) && string.IsNullOrWhiteSpace(row.OwnerName))
                errors.Add("BrandName or OwnerName is required");

            if (string.IsNullOrWhiteSpace(row.OwnerPhone))
                errors.Add("OwnerPhone is required");

            if (!string.IsNullOrEmpty(row.CompanySize))
            {
                var validSizes = new[] { "small", "medium", "large", "enterprise" };
                if (!validSizes.Contains(row.CompanySize.ToLower()))
                    errors.Add("CompanySize must be Small, Medium, Large, or Enterprise");
            }

            return errors.Any() ? string.Join("; ", errors) : null;
        }

        private bool IsEmptyRow(DmContactExcelImportDto row)
        {
            return string.IsNullOrWhiteSpace(row.OwnerPhone) &&
                   string.IsNullOrWhiteSpace(row.BrandName) &&
                   string.IsNullOrWhiteSpace(row.OwnerName);
        }
    }
}
