using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Customers.Leads;
using ZR.Model.Customers.Dto;
using ZR.Model.Customers;
using ZR.Model.Operations;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Enums;
using ZR.Model.Audit;
using ZR.Repository;
using ZR.Service.Customers.ICustomersService;
using MiniExcelLibs;
using System.IO;
using ZR.Service.HR.IService;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.System;
using ZR.Model.EventManagement;

namespace ZR.Service.Customers.Leads
{
    [AppService(ServiceType = typeof(IBpLeadService), ServiceLifetime = LifeTime.Transient)]
    public class BpLeadService : BaseService<BpLead>, IBpLeadService
    {
        private readonly IHrEmployeeCoreService _employeeService;

        public BpLeadService(IHrEmployeeCoreService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Get employee ID by user ID
        /// </summary>
        public async Task<long?> GetEmployeeIdByUserIdAsync(long userId)
        {
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.UserId.HasValue && e.UserId.Value == userId)
                .FirstAsync();

            return employee?.EmployeeId;
        }

        public async Task<PagedInfo<BpLead>> GetLeadsListAsync(LeadQueryDto query)
        {
            long? filterEmployeeId = null;

            if (!query.IsCurrentUserAdmin && query.CurrentUserId.HasValue)
            {
                filterEmployeeId = await GetEmployeeIdByUserIdAsync(query.CurrentUserId.Value);

                if (!filterEmployeeId.HasValue)
                {
                    return new PagedInfo<BpLead>
                    {
                        Result = new List<BpLead>(),
                        TotalNum = 0,
                        PageIndex = query.PageNum,
                        PageSize = query.PageSize
                    };
                }
            }

            var now = DateTime.Now;
            var pageQuery = Context
                .Queryable<BpLead, HrEmployeeCore, HrPersonName, HrEmployeeCore, HrPersonName>(
                    (lead, emp, name, salesEmp, salesName) => new JoinQueryInfos(
                    JoinType.Left, lead.AssignedTo == emp.EmployeeId,
                    JoinType.Left, emp.PersonId == name.PersonId &&
                                   name.IsPrimary == true &&
                                   name.EffectiveFrom <= now &&
                                   (name.EffectiveTo == null || name.EffectiveTo > now),
                    JoinType.Left, lead.SalesAssignedTo == salesEmp.EmployeeId,
                    JoinType.Left, salesEmp.PersonId == salesName.PersonId &&
                                   salesName.IsPrimary == true &&
                                   salesName.EffectiveFrom <= now &&
                                   (salesName.EffectiveTo == null || salesName.EffectiveTo > now)))
                // Role-based filter: on sales page, show leads where user is SalesAssignedTo OR AssignedTo
                .WhereIF(filterEmployeeId.HasValue && query.FilterBySalesAssignment,
                    (lead, emp, name, salesEmp, salesName) => lead.SalesAssignedTo == filterEmployeeId.Value || lead.AssignedTo == filterEmployeeId.Value)
                .WhereIF(filterEmployeeId.HasValue && !query.FilterBySalesAssignment,
                    (lead, emp, name, salesEmp, salesName) => lead.AssignedTo == filterEmployeeId.Value)
                // Allowed statuses filter (for Marketing/Sales/Operations role separation)
                .WhereIF(query.AllowedStatuses != null && query.AllowedStatuses.Length > 0,
                    (lead, emp, name, salesEmp, salesName) => query.AllowedStatuses.Contains(lead.LeadStatus))
                // ReadyForOperations filter (Operations sees leads flagged as ready)
                .WhereIF(query.FilterByReadyForOperations == true,
                    (lead, emp, name, salesEmp, salesName) => lead.ReadyForOperations == true)
                // Regular filters
                .WhereIF(!string.IsNullOrEmpty(query.BrandName), (lead, emp, name, salesEmp, salesName) => lead.BrandName.Contains(query.BrandName))
                .WhereIF(query.AssignedTo.HasValue && !filterEmployeeId.HasValue, (lead, emp, name, salesEmp, salesName) => lead.AssignedTo == query.AssignedTo.Value)
                .WhereIF(!string.IsNullOrEmpty(query.LeadStatus), (lead, emp, name, salesEmp, salesName) => lead.LeadStatus == query.LeadStatus)
                .WhereIF(query.LeadType.HasValue, (lead, emp, name, salesEmp, salesName) => lead.LeadType == query.LeadType.Value)
                .WhereIF(!string.IsNullOrEmpty(query.OwnerPhone), (lead, emp, name, salesEmp, salesName) => lead.OwnerPhone.Contains(query.OwnerPhone))
                .WhereIF(!string.IsNullOrEmpty(query.OwnerEmail), (lead, emp, name, salesEmp, salesName) => lead.OwnerEmail.Contains(query.OwnerEmail))
                // General search across multiple fields
                .WhereIF(!string.IsNullOrEmpty(query.SearchTerm), (lead, emp, name, salesEmp, salesName) =>
                    lead.BrandName.Contains(query.SearchTerm) ||
                    lead.OwnerName.Contains(query.SearchTerm) ||
                    lead.OwnerPhone.Contains(query.SearchTerm) ||
                    lead.OwnerEmail.Contains(query.SearchTerm))
                .OrderByDescending((lead, emp, name, salesEmp, salesName) => lead.Create_time)
                .Select((lead, emp, name, salesEmp, salesName) => new BpLead
                {
                    LeadId = lead.LeadId,
                    LeadType = lead.LeadType,
                    BrandName = lead.BrandName,
                    BrandImage = lead.BrandImage,
                    OwnerName = lead.OwnerName,
                    OwnerPhone = lead.OwnerPhone,
                    OwnerEmail = lead.OwnerEmail,
                    AssignedTo = lead.AssignedTo,
                    AssignedToName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    SalesAssignedTo = lead.SalesAssignedTo,
                    SalesAssignedToName = salesName.FullNameAr ?? (salesName.GivenName + " " + salesName.FamilyName),
                    LeadStatus = lead.LeadStatus,
                    ReadyForOperations = lead.ReadyForOperations,
                    QualificationNotes = lead.QualificationNotes,
                    RejectionNotes = lead.RejectionNotes,
                    InternalNotes = lead.InternalNotes,
                    ConvertedPartnerId = lead.ConvertedPartnerId,
                    ConversionDate = lead.ConversionDate,
                    LeadSource = lead.LeadSource,
                    BudgetRange = lead.BudgetRange,
                    FirstContactDate = lead.FirstContactDate,
                    LastContactDate = lead.LastContactDate,
                    Priority = lead.Priority,
                    ExpectedRevenue = lead.ExpectedRevenue,
                    LostReason = lead.LostReason,
                    CompanyName = lead.CompanyName,
                    ActivityType = lead.ActivityType,
                    Interesting = lead.Interesting,
                    LocationInterested = lead.LocationInterested,
                    ContractTypeInterested = lead.ContractTypeInterested,
                    Create_by = lead.Create_by,
                    Create_time = lead.Create_time,
                    Update_by = lead.Update_by,
                    Update_time = lead.Update_time,
                    Remark = lead.Remark,
                    CompanyID = lead.CompanyID,
                    BranchID = lead.BranchID
                });

            return await pageQuery.ToPageAsync(query);
        }

        public async Task<BpLead> GetLeadByIdAsync(long leadId)
        {
            var now = DateTime.Now;
            var lead = await Context
                .Queryable<BpLead, HrEmployeeCore, HrPersonName, HrEmployeeCore, HrPersonName>(
                    (l, emp, name, salesEmp, salesName) => new JoinQueryInfos(
                    JoinType.Left, l.AssignedTo == emp.EmployeeId,
                    JoinType.Left, emp.PersonId == name.PersonId &&
                                   name.IsPrimary == true &&
                                   name.EffectiveFrom <= now &&
                                   (name.EffectiveTo == null || name.EffectiveTo > now),
                    JoinType.Left, l.SalesAssignedTo == salesEmp.EmployeeId,
                    JoinType.Left, salesEmp.PersonId == salesName.PersonId &&
                                   salesName.IsPrimary == true &&
                                   salesName.EffectiveFrom <= now &&
                                   (salesName.EffectiveTo == null || salesName.EffectiveTo > now)))
                .Where((l, emp, name, salesEmp, salesName) => l.LeadId == leadId)
                .Select((l, emp, name, salesEmp, salesName) => new BpLead
                {
                    LeadId = l.LeadId,
                    LeadType = l.LeadType,
                    BrandName = l.BrandName,
                    BrandImage = l.BrandImage,
                    OwnerName = l.OwnerName,
                    OwnerPhone = l.OwnerPhone,
                    OwnerEmail = l.OwnerEmail,
                    AssignedTo = l.AssignedTo,
                    AssignedToName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName),
                    SalesAssignedTo = l.SalesAssignedTo,
                    SalesAssignedToName = salesName.FullNameAr ?? (salesName.GivenName + " " + salesName.FamilyName),
                    LeadStatus = l.LeadStatus,
                    ReadyForOperations = l.ReadyForOperations,
                    QualificationNotes = l.QualificationNotes,
                    RejectionNotes = l.RejectionNotes,
                    InternalNotes = l.InternalNotes,
                    ConvertedPartnerId = l.ConvertedPartnerId,
                    ConversionDate = l.ConversionDate,
                    LeadSource = l.LeadSource,
                    BudgetRange = l.BudgetRange,
                    FirstContactDate = l.FirstContactDate,
                    LastContactDate = l.LastContactDate,
                    Priority = l.Priority,
                    ExpectedRevenue = l.ExpectedRevenue,
                    LostReason = l.LostReason,
                    CompanyName = l.CompanyName,
                    ActivityType = l.ActivityType,
                    Interesting = l.Interesting,
                    LocationInterested = l.LocationInterested,
                    ContractTypeInterested = l.ContractTypeInterested,
                    Create_by = l.Create_by,
                    Create_time = l.Create_time,
                    Update_by = l.Update_by,
                    Update_time = l.Update_time,
                    Remark = l.Remark,
                    CompanyID = l.CompanyID,
                    BranchID = l.BranchID
                })
                .FirstAsync();

            return lead;
        }

        public async Task<List<BpLead>> GetLeadsByMarketerAsync(long assignedTo)
        {
            return await GetListAsync(it => it.AssignedTo == assignedTo);
        }

        public async Task<BpLead> AddLeadAsync(BpLead lead, long? currentUserId = null, bool isAdmin = true)
        {
            lead.LeadId = SnowFlakeSingle.Instance.NextId();
            lead.Create_time = DateTime.Now;

            if (string.IsNullOrEmpty(lead.LeadStatus))
            {
                lead.LeadStatus = "New";
            }

            if (!isAdmin && lead.AssignedTo == 0 && currentUserId.HasValue)
            {
                var employeeId = await GetEmployeeIdByUserIdAsync(currentUserId.Value);
                if (employeeId.HasValue)
                {
                    lead.AssignedTo = employeeId.Value;
                }
            }

            Add(lead);
            return await GetLeadByIdAsync(lead.LeadId);
        }

        public async Task<int> UpdateLeadAsync(BpLead lead)
        {
            lead.Update_time = DateTime.Now;
            return await Task.FromResult(Update(lead, true));
        }

        public async Task<int> DeleteLeadsAsync(long[] leadIds)
        {
            return await Context.Updateable<BpLead>()
                .SetColumns(l => new BpLead { IsDeleted = true, DeletedAt = DateTime.Now })
                .Where(l => leadIds.Contains(l.LeadId))
                .ExecuteCommandAsync();
        }

        public async Task<int> UpdateLeadStatusWithSideEffectsAsync(
            long leadId, string newStatus, string notes, string updatedBy)
        {
            var result = await UseTranAsync(async () =>
            {
                var lead = await GetByIdAsync(leadId);
                if (lead == null)
                    throw new Exception("Lead not found");

                var oldStatus = lead.LeadStatus;
                lead.LeadStatus = newStatus;
                lead.Update_by = updatedBy;
                lead.Update_time = DateTime.Now;

                if (!string.IsNullOrEmpty(notes))
                {
                    var statusNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] Status → {newStatus}: {notes}";
                    lead.InternalNotes = string.IsNullOrEmpty(lead.InternalNotes)
                        ? statusNote
                        : $"{lead.InternalNotes}\n{statusNote}";
                }

                // Lost cleanup
                if (newStatus == "Lost")
                {
                    lead.ReadyForOperations = false;
                    lead.LostReason = notes;

                    // Cancel all Scheduled/InProgress visits for this lead
                    await Context.Updateable<SiteVisit>()
                        .SetColumns(v => new SiteVisit
                        {
                            VisitStatus = "Cancelled",
                            CancellationReason = $"Lead marked as Lost: {notes}",
                            Update_time = DateTime.Now
                        })
                        .Where(v => v.LeadId == leadId
                            && (v.VisitStatus == "Scheduled" || v.VisitStatus == "InProgress"))
                        .ExecuteCommandAsync();

                    // Deactivate customer if no active contracts
                    var customer = await Context.Queryable<Customer>()
                        .Where(c => c.LeadId == leadId && c.IsActive)
                        .FirstAsync();

                    if (customer != null)
                    {
                        var hasActiveContracts = await Context.Queryable<ErpContract>()
                            .Where(c => c.CustomerId == customer.Id
                                && c.Status != ContractStatus.Terminated
                                && c.Status != ContractStatus.Expired)
                            .AnyAsync();

                        if (!hasActiveContracts)
                        {
                            await Context.Updateable<Customer>()
                                .SetColumns(c => new Customer { IsActive = false, Update_time = DateTime.Now })
                                .Where(c => c.Id == customer.Id)
                                .ExecuteCommandAsync();
                        }
                    }
                }

                // Confirmed: auto-set readyForOperations
                if (newStatus == "Confirmed")
                {
                    lead.ReadyForOperations = true;
                }

                Update(lead, true);

                // Audit log
                await Context.Insertable(new EntityStatusAudit
                {
                    Id = SnowFlakeSingle.Instance.NextId(),
                    EntityType = "Lead",
                    EntityId = leadId.ToString(),
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Notes = notes,
                    ChangedBy = updatedBy,
                    ChangedAt = DateTime.Now
                }).ExecuteCommandAsync();
            });

            if (!result.IsSuccess)
                throw new Exception($"Transaction failed: {result.ErrorMessage}");

            return 1;
        }

        public async Task<List<object>> GetLocationsLookupAsync()
        {
            var locations = await Context.Queryable<EventLocation>()
                .Where(l => l.Isactive == "Y" || l.Isactive == "1" || l.Isactive == "true")
                .Select(l => new
                {
                    locationId = l.Locationid,
                    locationName = l.Locationname,
                    city = l.City
                })
                .ToListAsync();

            return locations.Cast<object>().ToList();
        }

        public async Task<int> SyncCustomersToLeadsAsync(string userName)
        {
            // Find active customers that don't have a corresponding lead
            var customersWithoutLeads = await Context.Queryable<Customer>()
                .Where(c => c.IsActive && !c.IsDeleted && (c.LeadId == null || c.LeadId == 0))
                .ToListAsync();

            if (customersWithoutLeads.Count == 0) return 0;

            int synced = 0;
            foreach (var customer in customersWithoutLeads)
            {
                // Check if a lead already exists for this customer by phone/email
                BpLead existingLead = null;
                if (!string.IsNullOrEmpty(customer.Phone))
                {
                    existingLead = await Context.Queryable<BpLead>()
                        .Where(l => l.OwnerPhone == customer.Phone)
                        .FirstAsync();
                }

                if (existingLead != null)
                {
                    // Link customer to existing lead
                    await Context.Updateable<Customer>()
                        .SetColumns(c => new Customer { LeadId = existingLead.LeadId, Update_time = DateTime.Now })
                        .Where(c => c.Id == customer.Id)
                        .ExecuteCommandAsync();
                }
                else
                {
                    // Create a new Confirmed lead for this customer
                    var lead = new BpLead
                    {
                        LeadId = SnowFlakeSingle.Instance.NextId(),
                        LeadType = 1, // Company
                        BrandName = customer.BrandName ?? customer.NameAr ?? customer.NameEn,
                        OwnerName = customer.ContactPerson ?? customer.NameAr ?? customer.NameEn,
                        OwnerPhone = customer.Phone,
                        OwnerEmail = customer.Email,
                        LeadStatus = "Confirmed",
                        ReadyForOperations = true,
                        LeadSource = "Finance",
                        Create_by = userName,
                        Create_time = DateTime.Now
                    };

                    await Context.Insertable(lead).ExecuteCommandAsync();

                    // Link customer back to the new lead
                    await Context.Updateable<Customer>()
                        .SetColumns(c => new Customer { LeadId = lead.LeadId, Update_time = DateTime.Now })
                        .Where(c => c.Id == customer.Id)
                        .ExecuteCommandAsync();
                }

                synced++;
            }

            return synced;
        }

        #region Excel Import/Export

        public async Task<List<LeadExcelExportDto>> GetLeadsForExportAsync(LeadQueryDto query)
        {
            var now = DateTime.Now;
            var leads = await Context
                .Queryable<BpLead, HrEmployeeCore, HrPersonName>((lead, emp, name) => new JoinQueryInfos(
                    JoinType.Left, lead.AssignedTo == emp.EmployeeId,
                    JoinType.Left, emp.PersonId == name.PersonId &&
                                   name.IsPrimary == true &&
                                   name.EffectiveFrom <= now &&
                                   (name.EffectiveTo == null || name.EffectiveTo > now)))
                .WhereIF(!string.IsNullOrEmpty(query.BrandName),
                    (lead, emp, name) => lead.BrandName.Contains(query.BrandName))
                .WhereIF(query.AssignedTo.HasValue,
                    (lead, emp, name) => lead.AssignedTo == query.AssignedTo.Value)
                .WhereIF(!string.IsNullOrEmpty(query.LeadStatus),
                    (lead, emp, name) => lead.LeadStatus == query.LeadStatus)
                .WhereIF(query.LeadType.HasValue,
                    (lead, emp, name) => lead.LeadType == query.LeadType.Value)
                .WhereIF(query.AllowedStatuses != null && query.AllowedStatuses.Length > 0,
                    (lead, emp, name) => query.AllowedStatuses.Contains(lead.LeadStatus))
                .Select((lead, emp, name) => new LeadExcelExportDto
                {
                    LeadType = lead.LeadType == 1 ? "Company" : "Individual",
                    BrandName = lead.BrandName,
                    OwnerName = lead.OwnerName,
                    OwnerPhone = lead.OwnerPhone,
                    OwnerEmail = lead.OwnerEmail,
                    AssignedToName = name.FullNameAr ?? (name.GivenName + " " + name.FamilyName) ?? emp.EmployeeCode ?? "",
                    LeadStatus = lead.LeadStatus,
                    LeadSource = lead.LeadSource,
                    BudgetRange = lead.BudgetRange,
                    Priority = lead.Priority,
                    ExpectedRevenue = lead.ExpectedRevenue,
                    FirstContactDate = lead.FirstContactDate,
                    LastContactDate = lead.LastContactDate,
                    InternalNotes = lead.InternalNotes,
                    CreateTime = lead.Create_time
                })
                .ToListAsync();

            return leads;
        }

        public async Task<LeadImportPreviewDto> PreviewExcelImportAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception("Excel file not found");

            var rows = await MiniExcel.QueryAsync<LeadExcelImportDto>(filePath);
            var rowsList = rows.Where(r => !IsEmptyRow(r)).ToList();

            var result = new LeadImportPreviewDto
            {
                TotalRows = rowsList.Count,
                PreviewData = new List<LeadExcelImportDto>(),
                ValidationErrors = new List<string>()
            };

            int rowNumber = 1;
            foreach (var row in rowsList.Take(100))
            {
                rowNumber++;
                var validationError = await ValidateLeadImportRow(row, rowNumber);
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

        public async Task<LeadImportResultDto> ImportLeadsFromExcelAsync(string filePath, LeadImportRequestDto request)
        {
            if (!File.Exists(filePath))
                throw new Exception("Excel file not found");

            var rows = await MiniExcel.QueryAsync<LeadExcelImportDto>(filePath);
            var rowsList = rows.Where(r => !IsEmptyRow(r)).ToList();

            var result = new LeadImportResultDto
            {
                TotalRows = rowsList.Count,
                SuccessCount = 0,
                FailedCount = 0,
                SkippedCount = 0,
                Errors = new List<string>(),
                ImportedLeadIds = new List<long>()
            };

            var employees = await _employeeService.GetListAsync(e => true);
            var employeeByCodeDict = employees
                .Where(e => !string.IsNullOrEmpty(e.EmployeeCode))
                .ToDictionary(e => e.EmployeeCode.ToLower(), e => e.EmployeeId);

            var employeeWithNames = await Context.Queryable<HrEmployeeCore>()
                .LeftJoin<HrPersonName>((e, n) => e.PersonId == n.PersonId && n.IsPrimary)
                .Select((e, n) => new { e.EmployeeId, e.EmployeeCode, FullName = (n.GivenName ?? "") + " " + (n.FamilyName ?? ""), n.GivenName, n.FamilyName, n.FullNameAr })
                .ToListAsync();

            var employeeByNameDict = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (var emp in employeeWithNames)
            {
                var fullName = emp.FullName?.Trim();
                var givenName = emp.GivenName?.Trim();
                var familyName = emp.FamilyName?.Trim();
                var arabicName = emp.FullNameAr?.Trim();

                if (!string.IsNullOrEmpty(fullName) && !employeeByNameDict.ContainsKey(fullName))
                    employeeByNameDict[fullName] = emp.EmployeeId;
                if (!string.IsNullOrEmpty(givenName) && !employeeByNameDict.ContainsKey(givenName))
                    employeeByNameDict[givenName] = emp.EmployeeId;
                if (!string.IsNullOrEmpty(familyName) && !employeeByNameDict.ContainsKey(familyName))
                    employeeByNameDict[familyName] = emp.EmployeeId;
                if (!string.IsNullOrEmpty(arabicName) && !employeeByNameDict.ContainsKey(arabicName))
                    employeeByNameDict[arabicName] = emp.EmployeeId;
                if (!string.IsNullOrEmpty(emp.EmployeeCode) && !employeeByNameDict.ContainsKey(emp.EmployeeCode))
                    employeeByNameDict[emp.EmployeeCode] = emp.EmployeeId;
            }

            var importedPhones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int rowNumber = 1;
            foreach (var row in rowsList)
            {
                rowNumber++;
                try
                {
                    var validationError = await ValidateLeadImportRow(row, rowNumber);

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

                    if (importedPhones.Contains(row.OwnerPhone))
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {rowNumber} skipped: Duplicate phone number in import file (Phone: {row.OwnerPhone})");
                        continue;
                    }

                    var existingLead = await Context.Queryable<BpLead>()
                        .Where(l => l.OwnerPhone == row.OwnerPhone ||
                                   (!string.IsNullOrEmpty(row.OwnerEmail) && l.OwnerEmail == row.OwnerEmail))
                        .FirstAsync();

                    if (existingLead != null && !request.OverwriteExisting)
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {rowNumber} skipped: Lead already exists in database (Phone: {row.OwnerPhone})");
                        continue;
                    }

                    long assignedToId = 0;
                    if (!string.IsNullOrEmpty(request.AssignedToEmployeeId) && long.TryParse(request.AssignedToEmployeeId, out long requestEmpId))
                    {
                        assignedToId = requestEmpId;
                    }
                    else if (!string.IsNullOrEmpty(row.AssignedToEmployeeCode))
                    {
                        var empCode = row.AssignedToEmployeeCode.ToLower();
                        if (employeeByCodeDict.ContainsKey(empCode))
                        {
                            assignedToId = employeeByCodeDict[empCode];
                        }
                    }
                    else if (!string.IsNullOrEmpty(row.AssignToName))
                    {
                        if (employeeByNameDict.TryGetValue(row.AssignToName.Trim(), out long empId))
                        {
                            assignedToId = empId;
                        }
                    }

                    var lead = existingLead ?? new BpLead { LeadId = SnowFlakeSingle.Instance.NextId() };

                    lead.LeadType = string.IsNullOrWhiteSpace(row.LeadType) ? 2 :
                                   (row.LeadType.ToLower() == "company" || row.LeadType == "1" ? 1 : 2);
                    lead.BrandName = string.IsNullOrWhiteSpace(row.BrandName) ? "غير محدد" : row.BrandName;
                    lead.OwnerName = string.IsNullOrWhiteSpace(row.OwnerName) ? "غير محدد" : row.OwnerName;
                    lead.OwnerPhone = row.OwnerPhone;
                    lead.OwnerEmail = row.OwnerEmail;
                    lead.LeadSource = row.LeadSource;
                    lead.BudgetRange = row.BudgetRange;
                    lead.Priority = string.IsNullOrWhiteSpace(row.Priority) ? "Medium" : row.Priority;
                    lead.ExpectedRevenue = row.ExpectedRevenue;
                    lead.InternalNotes = row.InternalNotes;
                    lead.AssignedTo = assignedToId;
                    lead.LeadStatus = existingLead != null ? existingLead.LeadStatus : "New";

                    if (existingLead != null)
                    {
                        lead.Update_time = DateTime.Now;
                        Update(lead, true);
                    }
                    else
                    {
                        lead.Create_time = DateTime.Now;
                        Add(lead);
                    }

                    result.SuccessCount++;
                    result.ImportedLeadIds.Add(lead.LeadId);
                    importedPhones.Add(row.OwnerPhone);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Row {rowNumber} failed: {ex.Message}");

                    if (!request.SkipInvalidRows)
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        private async Task<string> ValidateLeadImportRow(LeadExcelImportDto row, int rowNumber)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(row.OwnerPhone))
                errors.Add("OwnerPhone is required");

            if (!string.IsNullOrEmpty(row.LeadType))
            {
                var validTypes = new[] { "company", "individual", "1", "2" };
                if (!validTypes.Contains(row.LeadType.ToLower()))
                    errors.Add("LeadType must be 'Company', 'Individual', '1', or '2'");
            }

            if (!string.IsNullOrEmpty(row.Priority))
            {
                var validPriorities = new[] { "low", "medium", "high", "urgent" };
                if (!validPriorities.Contains(row.Priority.ToLower()))
                    errors.Add("Priority must be 'Low', 'Medium', 'High', or 'Urgent'");
            }

            return errors.Any() ? string.Join("; ", errors) : null;
        }

        private bool IsEmptyRow(LeadExcelImportDto row)
        {
            return string.IsNullOrWhiteSpace(row.OwnerPhone) &&
                   string.IsNullOrWhiteSpace(row.BrandName) &&
                   string.IsNullOrWhiteSpace(row.OwnerName) &&
                   string.IsNullOrWhiteSpace(row.OwnerEmail) &&
                   string.IsNullOrWhiteSpace(row.LeadType) &&
                   string.IsNullOrWhiteSpace(row.LeadSource);
        }

        #endregion
    }

}
