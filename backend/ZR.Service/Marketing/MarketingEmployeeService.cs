using Infrastructure.Attribute;
using Infrastructure.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.Marketing.Dto;
using ZR.Model.Customers.Leads;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;
using ZR.ServiceCore.Services;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Marketing Employee Service Implementation
    /// Handles marketing team employee data with complex joins and filtering
    /// </summary>
    [AppService(ServiceType = typeof(IMarketingEmployeeService), ServiceLifetime = LifeTime.Transient)]
    public class MarketingEmployeeService : BaseService<HrEmployeeCore>, IMarketingEmployeeService
    {
        private readonly ISysDeptService _deptService;

        public MarketingEmployeeService(ISysDeptService deptService)
        {
            _deptService = deptService;
        }

        /// <summary>
        /// Get marketing employees with pagination and filtering
        /// </summary>
        public async Task<PagedInfo<MarketingEmployeeDto>> GetMarketingEmployeesAsync(MarketingEmployeeQueryDto queryDto)
        {
            var now = DateTime.Now;

            // Build the base query with joins
            var query = Context.Queryable<HrEmployeeCore, HrPerson, HrPersonName, HrEmployeeAssignment, SysDept, SysPost, HrPersonContact, HrPersonContact>(
                (emp, per, name, assign, dept, pos, emailContact, phoneContact) => new JoinQueryInfos(
                    JoinType.Inner, emp.PersonId == per.PersonId,
                    JoinType.Left, per.PersonId == name.PersonId && name.IsPrimary == true,
                    JoinType.Left, emp.EmployeeId == assign.EmployeeId && assign.IsPrimary == true,
                    JoinType.Left, assign.DepartmentId == dept.DeptId,
                    JoinType.Left, assign.PositionId == pos.PostId,
                    JoinType.Left, per.PersonId == emailContact.PersonId && emailContact.ContactTypeDictCode == 1 && emailContact.IsPrimary == true && emailContact.EffectiveFrom <= now && (emailContact.EffectiveTo == null || emailContact.EffectiveTo > now),
                    JoinType.Left, per.PersonId == phoneContact.PersonId && phoneContact.ContactTypeDictCode == 2 && phoneContact.IsPrimary == true && phoneContact.EffectiveFrom <= now && (phoneContact.EffectiveTo == null || phoneContact.EffectiveTo > now)
                ))
                .Where((emp, per, name, assign, dept, pos, emailContact, phoneContact) => emp.IsActive);

            // Apply filters
            // Department filter
            if (queryDto.DepartmentId.HasValue)
            {
                query = query.Where((emp, per, name, assign, dept, pos, emailContact, phoneContact) =>
                    assign.DepartmentId == queryDto.DepartmentId.Value);
            }

            // Search query filter (name or email)
            if (!string.IsNullOrWhiteSpace(queryDto.SearchQuery))
            {
                var searchTerm = queryDto.SearchQuery;
                query = query.Where((emp, per, name, assign, dept, pos, emailContact, phoneContact) =>
                    (name.FullNameAr != null && name.FullNameAr.Contains(searchTerm)) ||
                    (name.GivenName != null && name.GivenName.Contains(searchTerm)) ||
                    (name.FamilyName != null && name.FamilyName.Contains(searchTerm)) ||
                    (emailContact.ContactValue != null && emailContact.ContactValue.Contains(searchTerm)));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(queryDto.StatusFilter))
            {
                if (queryDto.StatusFilter.ToLower() == "active")
                {
                    query = query.Where((emp, per, name, assign, dept, pos, emailContact, phoneContact) =>
                        emp.IsActive == true);
                }
                else if (queryDto.StatusFilter.ToLower() == "inactive")
                {
                    query = query.Where((emp, per, name, assign, dept, pos, emailContact, phoneContact) =>
                        emp.IsActive == false);
                }
            }

            // Select and map to DTO
            var allEmployees = await query
                .Select((emp, per, name, assign, dept, pos, emailContact, phoneContact) => new MarketingEmployeeDto
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeCode = emp.EmployeeCode,
                    HireDate = emp.HireDate,
                    GivenName = name.GivenName,
                    FamilyName = name.FamilyName,
                    FullNameAr = name.FullNameAr,
                    Email = emailContact.ContactValue,
                    Phone = phoneContact.ContactValue,
                    DepartmentId = assign.DepartmentId,
                    DepartmentName = dept.DeptName,
                    PositionId = assign.PositionId,
                    PositionName = pos.PostName,
                    ManagerEmployeeId = assign.ManagerEmployeeId,
                    IsActive = emp.IsActive
                })
                .MergeTable()
                .OrderBy(e => e.GivenName)
                .ToListAsync();

            // Deduplicate by EmployeeId in memory
            var uniqueEmployees = allEmployees
                .GroupBy(e => e.EmployeeId)
                .Select(g => g.First())
                .ToList();

            // Get accurate total count after deduplication
            var totalCount = uniqueEmployees.Count;

            // Apply pagination
            var employees = uniqueEmployees
                .Skip((queryDto.PageNum - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .ToList();

            return new PagedInfo<MarketingEmployeeDto>
            {
                Result = employees,
                TotalNum = totalCount,
                PageIndex = queryDto.PageNum,
                PageSize = queryDto.PageSize
            };
        }

        /// <summary>
        /// Get employee by ID with full details
        /// </summary>
        public async Task<MarketingEmployeeDto> GetEmployeeByIdAsync(long id)
        {
            var now = DateTime.Now;

            var employee = await Context.Queryable<HrEmployeeCore, HrPerson, HrPersonName, HrEmployeeAssignment, SysDept, SysPost, HrPersonContact, HrPersonContact, HrPersonAddress>(
                (emp, per, name, assign, dept, pos, emailContact, phoneContact, addr) => new JoinQueryInfos(
                    JoinType.Inner, emp.PersonId == per.PersonId,
                    JoinType.Left, per.PersonId == name.PersonId && name.IsPrimary == true,
                    JoinType.Left, emp.EmployeeId == assign.EmployeeId && assign.IsPrimary == true,
                    JoinType.Left, assign.DepartmentId == dept.DeptId,
                    JoinType.Left, assign.PositionId == pos.PostId,
                    JoinType.Left, per.PersonId == emailContact.PersonId && emailContact.ContactTypeDictCode == 1 && emailContact.IsPrimary == true && emailContact.EffectiveFrom <= now && (emailContact.EffectiveTo == null || emailContact.EffectiveTo > now),
                    JoinType.Left, per.PersonId == phoneContact.PersonId && phoneContact.ContactTypeDictCode == 2 && phoneContact.IsPrimary == true && phoneContact.EffectiveFrom <= now && (phoneContact.EffectiveTo == null || phoneContact.EffectiveTo > now),
                    JoinType.Left, per.PersonId == addr.PersonId && addr.IsPrimary == true && addr.EffectiveFrom <= now && (addr.EffectiveTo == null || addr.EffectiveTo > now)
                ))
                .Where((emp, per, name, assign, dept, pos, emailContact, phoneContact, addr) =>
                    emp.EmployeeId == id && emp.IsActive)
                .Select((emp, per, name, assign, dept, pos, emailContact, phoneContact, addr) => new MarketingEmployeeDto
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeCode = emp.EmployeeCode,
                    HireDate = emp.HireDate,
                    GivenName = name.GivenName,
                    FamilyName = name.FamilyName,
                    FullNameAr = name.FullNameAr,
                    Email = emailContact.ContactValue,
                    Phone = phoneContact.ContactValue,
                    DepartmentId = assign.DepartmentId,
                    DepartmentName = dept.DeptName,
                    PositionId = assign.PositionId,
                    PositionName = pos.PostName,
                    ManagerEmployeeId = assign.ManagerEmployeeId,
                    IsActive = emp.IsActive
                })
                .FirstAsync();

            return employee;
        }

        /// <summary>
        /// Get all departments for filtering
        /// </summary>
        public List<SysDeptDto> GetDepartments()
        {
            var deptQuery = new SysDeptQueryDto
            {
                Status = 0,
                DelFlag = 0
            };

            return _deptService.GetList(deptQuery)
                .OrderBy(d => d.DeptName)
                .ToList();
        }

        /// <summary>
        /// Get employee KPIs (Key Performance Indicators)
        /// </summary>
        public async Task<EmployeeKpiDto> GetEmployeeKpisAsync(long employeeId, int periodDays = 30)
        {
            var startDate = DateTime.Now.AddDays(-periodDays);
            var now = DateTime.Now;

            // Get employee name
            var employee = await GetEmployeeByIdAsync(employeeId);
            var employeeName = employee?.FullNameAr ?? $"{employee?.GivenName} {employee?.FamilyName}";

            // Get all leads for this employee
            var allLeads = await Context.Queryable<BpLead>()
                .Where(l => l.AssignedTo == employeeId)
                .ToListAsync();

            // Get leads in current period
            var periodLeads = allLeads.Where(l => l.Create_time >= startDate).ToList();

            // Get all lead IDs belonging to this employee
            var leadIds = allLeads.Select(l => l.LeadId).ToList();

            // Get activities by BOTH: direct employee match OR belonging to this employee's leads
            var allActivities = leadIds.Count > 0
                ? await Context.Queryable<BpLeadActivity>()
                    .Where(a => a.EmployeeId == employeeId || leadIds.Contains(a.LeadId))
                    .ToListAsync()
                : await Context.Queryable<BpLeadActivity>()
                    .Where(a => a.EmployeeId == employeeId)
                    .ToListAsync();

            // Deduplicate in case of overlapping matches
            allActivities = allActivities
                .GroupBy(a => a.ActivityId)
                .Select(g => g.First())
                .ToList();

            // Get activities in current period
            var periodActivities = allActivities.Where(a => a.ActivityDate >= startDate).ToList();

            // Calculate Lead Metrics
            var leadMetrics = new LeadMetrics
            {
                TotalLeads = allLeads.Count,
                NewLeads = periodLeads.Count,
                LeadsByStatus = allLeads
                    .GroupBy(l => l.LeadStatus ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                LeadsBySource = allLeads
                    .Where(l => !string.IsNullOrEmpty(l.LeadSource))
                    .GroupBy(l => l.LeadSource)
                    .ToDictionary(g => g.Key, g => g.Count()),
                LeadsByPriority = allLeads
                    .Where(l => !string.IsNullOrEmpty(l.Priority))
                    .GroupBy(l => l.Priority)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Calculate Conversion Metrics
            var convertedLeads = allLeads.Where(l => l.LeadStatus?.ToLower() == "converted").ToList();
            var lostLeads = allLeads.Where(l => l.LeadStatus?.ToLower() == "lost").ToList();
            var totalRevenue = convertedLeads.Where(l => l.ExpectedRevenue.HasValue).Sum(l => l.ExpectedRevenue.Value);

            var conversionMetrics = new ConversionMetrics
            {
                ConvertedLeads = convertedLeads.Count,
                LostLeads = lostLeads.Count,
                ConversionRate = allLeads.Count > 0 ? (decimal)convertedLeads.Count / allLeads.Count * 100 : 0,
                LostRate = allLeads.Count > 0 ? (decimal)lostLeads.Count / allLeads.Count * 100 : 0,
                TotalExpectedRevenue = totalRevenue,
                AverageDealSize = convertedLeads.Count > 0 ? totalRevenue / convertedLeads.Count : 0,
                AverageTimeToConvert = convertedLeads
                    .Where(l => l.ConversionDate.HasValue && l.Create_time > DateTime.MinValue)
                    .Select(l => (decimal)(l.ConversionDate.Value - l.Create_time).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average()
            };

            // Calculate Activity Metrics
            var activityMetrics = new ActivityMetrics
            {
                TotalActivities = allActivities.Count,
                RecentActivities = periodActivities.Count,
                ActivitiesByType = allActivities
                    .Where(a => !string.IsNullOrEmpty(a.ActivityType))
                    .GroupBy(a => a.ActivityType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ActivitiesByOutcome = allActivities
                    .Where(a => !string.IsNullOrEmpty(a.Outcome))
                    .GroupBy(a => a.Outcome)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ActivitiesPerLead = allLeads.Count > 0 ? (decimal)allActivities.Count / allLeads.Count : 0,
                FollowUpComplianceRate = CalculateFollowUpCompliance(allActivities)
            };

            // Calculate Performance Trends
            var trends = new PerformanceTrends
            {
                WeeklyLeadsTrend = CalculateWeeklyTrend(allLeads.Where(l => l.Create_time > DateTime.MinValue).Select(l => (l.Create_time, 1m)).ToList(), 12),
                WeeklyConversionTrend = CalculateWeeklyTrend(
                    convertedLeads.Where(l => l.ConversionDate.HasValue)
                        .Select(l => (l.ConversionDate.Value, 1m)).ToList(), 12),
                WeeklyActivityTrend = CalculateWeeklyTrend(
                    allActivities.Select(a => (a.ActivityDate, 1m)).ToList(), 12),
                MonthlyRevenueTrend = CalculateMonthlyRevenueTrend(convertedLeads, 6)
            };

            // Calculate Quality Metrics
            var qualifiedLeads = allLeads.Where(l => l.LeadStatus?.ToLower() == "qualified").ToList();
            var activeLeads = allLeads.Where(l => l.LeadStatus?.ToLower() != "converted" && l.LeadStatus?.ToLower() != "lost").ToList();
            var staleLeads = activeLeads.Where(l => !l.LastContactDate.HasValue || (now - l.LastContactDate.Value).TotalDays > 30).ToList();

            var qualityMetrics = new QualityMetrics
            {
                QualificationRate = allLeads.Count > 0 ? (decimal)qualifiedLeads.Count / allLeads.Count * 100 : 0,
                AverageResponseTime = CalculateAverageResponseTime(allLeads, allActivities),
                AverageDaysSinceLastContact = activeLeads.Count > 0
                    ? (decimal)activeLeads.Where(l => l.LastContactDate.HasValue)
                        .Select(l => (now - l.LastContactDate.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average()
                    : 0,
                StaleLeads = staleLeads.Count
            };

            return new EmployeeKpiDto
            {
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                Period = $"Last {periodDays} Days",
                LeadMetrics = leadMetrics,
                ConversionMetrics = conversionMetrics,
                ActivityMetrics = activityMetrics,
                Trends = trends,
                QualityMetrics = qualityMetrics
            };
        }

        private decimal CalculateFollowUpCompliance(List<BpLeadActivity> activities)
        {
            var activitiesWithFollowUp = activities.Where(a => a.NextFollowUpDate.HasValue).ToList();
            if (activitiesWithFollowUp.Count == 0) return 100;

            var compliantFollowUps = activitiesWithFollowUp.Count(a =>
            {
                var followUpDate = a.NextFollowUpDate.Value;
                var actualFollowUp = activities
                    .FirstOrDefault(fa => fa.LeadId == a.LeadId && fa.ActivityDate >= followUpDate && fa.ActivityDate <= followUpDate.AddDays(3));
                return actualFollowUp != null;
            });

            return (decimal)compliantFollowUps / activitiesWithFollowUp.Count * 100;
        }

        private decimal CalculateAverageResponseTime(List<BpLead> leads, List<BpLeadActivity> activities)
        {
            var responseTimes = leads
                .Where(l => l.FirstContactDate.HasValue && l.Create_time > DateTime.MinValue)
                .Select(l => (decimal)(l.FirstContactDate.Value - l.Create_time).TotalHours)
                .Where(h => h >= 0 && h < 720) // Filter outliers (max 30 days)
                .ToList();

            return responseTimes.Count > 0 ? responseTimes.Average() : 0;
        }

        private List<TrendDataPoint> CalculateWeeklyTrend(List<(DateTime date, decimal value)> data, int weeks)
        {
            var result = new List<TrendDataPoint>();
            var now = DateTime.Now;

            for (int i = weeks - 1; i >= 0; i--)
            {
                var weekStart = now.AddDays(-7 * (i + 1));
                var weekEnd = now.AddDays(-7 * i);

                var weekData = data.Where(d => d.date >= weekStart && d.date < weekEnd).ToList();
                var weekValue = weekData.Sum(d => d.value);

                result.Add(new TrendDataPoint
                {
                    Label = $"Week {weeks - i}",
                    Date = weekStart,
                    Value = weekValue
                });
            }

            return result;
        }

        private List<TrendDataPoint> CalculateMonthlyRevenueTrend(List<BpLead> convertedLeads, int months)
        {
            var result = new List<TrendDataPoint>();
            var now = DateTime.Now;

            for (int i = months - 1; i >= 0; i--)
            {
                var monthStart = now.AddMonths(-(i + 1));
                var monthEnd = now.AddMonths(-i);

                var monthRevenue = convertedLeads
                    .Where(l => l.ConversionDate.HasValue &&
                               l.ConversionDate.Value >= monthStart &&
                               l.ConversionDate.Value < monthEnd)
                    .Sum(l => l.ExpectedRevenue ?? 0);

                result.Add(new TrendDataPoint
                {
                    Label = monthStart.ToString("MMM yyyy"),
                    Date = monthStart,
                    Value = monthRevenue
                });
            }

            return result;
        }
    }
}
