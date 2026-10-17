using Infrastructure.Attribute;
using NLog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Customers.Leads;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.Marketing.Dto;
using ZR.Model.Operations;
using ZR.Model.Sales;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Daily Activity Report Service
    /// Aggregates employee activities from 7 business tables into a unified timeline
    /// </summary>
    [AppService(ServiceType = typeof(IDailyActivityReportService), ServiceLifetime = LifeTime.Transient)]
    public class DailyActivityReportService : BaseService<BpLeadActivity>, IDailyActivityReportService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public DailyActivityReportService()
        {
        }

        public async Task<EmployeeActivityReportDto> GetDailyReportAsync(EmployeeActivityQueryDto query)
        {
            var timeline = await GetTimelineAsync(query);
            var summary = BuildSummary(timeline);

            var teamPerformance = new List<EmployeeDailyPerformanceDto>();
            try
            {
                teamPerformance = await GetTeamPerformanceAsync(query.StartDate, query.EndDate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DailyActivityReport: Failed to get team performance");
            }

            return new EmployeeActivityReportDto
            {
                Summary = summary,
                Timeline = timeline,
                TeamPerformance = teamPerformance
            };
        }

        public async Task<ActivitySummaryDto> GetSummaryAsync(EmployeeActivityQueryDto query)
        {
            var timeline = await GetTimelineAsync(query);
            return BuildSummary(timeline);
        }

        public async Task<List<ActivityTimelineItemDto>> GetTimelineAsync(EmployeeActivityQueryDto query)
        {
            var endDate = query.EndDate.Date.AddDays(1); // Include the full end day
            var allItems = new List<ActivityTimelineItemDto>();

            // Source 1: Lead Activities (direct employee_id match)
            if (string.IsNullOrEmpty(query.ActivitySource) || query.ActivitySource == "LeadActivity")
            {
                try
                {
                    var activities = await Context.Queryable<BpLeadActivity, BpLead>(
                        (a, l) => new JoinQueryInfos(JoinType.Left, a.LeadId == l.LeadId))
                        .WhereIF(query.EmployeeId.HasValue, (a, l) => a.EmployeeId == query.EmployeeId.Value)
                        .Where((a, l) => a.ActivityDate >= query.StartDate && a.ActivityDate < endDate)
                        .WhereIF(!string.IsNullOrEmpty(query.ActivityType), (a, l) => a.ActivityType == query.ActivityType)
                        .Select((a, l) => new ActivityTimelineItemDto
                        {
                            Id = a.ActivityId,
                            Source = "LeadActivity",
                            ActivityType = a.ActivityType,
                            Timestamp = a.ActivityDate,
                            Description = a.ActivityType + " with " + l.BrandName,
                            EntityName = l.BrandName,
                            EntityId = a.LeadId,
                            Outcome = a.Outcome,
                            DurationMinutes = a.DurationMinutes,
                            Notes = a.Notes,
                            EmployeeId = a.EmployeeId
                        }).ToListAsync();

                    allItems.AddRange(activities);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "DailyActivityReport: Failed to query LeadActivity source");
                }
            }

            // Source 2: Leads created (match by AssignedTo)
            if (string.IsNullOrEmpty(query.ActivitySource) || query.ActivitySource == "Lead")
            {
                if (string.IsNullOrEmpty(query.ActivityType) || query.ActivityType == "Create")
                {
                    try
                    {
                        var leadsCreated = await Context.Queryable<BpLead>()
                            .Where(l => l.Create_time >= query.StartDate && l.Create_time < endDate)
                            .WhereIF(query.EmployeeId.HasValue, l => l.AssignedTo == query.EmployeeId.Value)
                            .Select(l => new ActivityTimelineItemDto
                            {
                                Id = l.LeadId,
                                Source = "LeadCreated",
                                ActivityType = "Create",
                                Timestamp = l.Create_time,
                                Description = "Created lead: " + l.BrandName,
                                EntityName = l.BrandName,
                                EntityId = l.LeadId,
                                EmployeeId = l.AssignedTo
                            }).ToListAsync();

                        allItems.AddRange(leadsCreated);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "DailyActivityReport: Failed to query LeadCreated source");
                    }
                }
            }

            // Source 3: Lead status updates (match by AssignedTo, only where update_time differs from create_time)
            if (string.IsNullOrEmpty(query.ActivitySource) || query.ActivitySource == "Lead")
            {
                if (string.IsNullOrEmpty(query.ActivityType) || query.ActivityType == "Update")
                {
                    try
                    {
                        var leadsUpdated = await Context.Queryable<BpLead>()
                            .Where(l => l.Update_time != null && l.Update_time >= query.StartDate && l.Update_time < endDate)
                            .Where(l => l.Update_time != l.Create_time)
                            .WhereIF(query.EmployeeId.HasValue, l => l.AssignedTo == query.EmployeeId.Value)
                            .Select(l => new ActivityTimelineItemDto
                            {
                                Id = l.LeadId,
                                Source = "LeadUpdated",
                                ActivityType = "Update",
                                Timestamp = l.Update_time.Value,
                                Description = "Updated lead: " + l.BrandName + " (Status: " + l.LeadStatus + ")",
                                EntityName = l.BrandName,
                                EntityId = l.LeadId,
                                EmployeeId = l.AssignedTo
                            }).ToListAsync();

                        allItems.AddRange(leadsUpdated);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "DailyActivityReport: Failed to query LeadUpdated source");
                    }
                }
            }

            // Source 4: Deals created (match by AssignedTo)
            if (string.IsNullOrEmpty(query.ActivitySource) || query.ActivitySource == "Deal")
            {
                if (string.IsNullOrEmpty(query.ActivityType) || query.ActivityType == "Create")
                {
                    try
                    {
                        var dealsCreated = await Context.Queryable<SalesDeal>()
                            .Where(d => d.Create_time >= query.StartDate && d.Create_time < endDate)
                            .WhereIF(query.EmployeeId.HasValue, d => d.AssignedTo == query.EmployeeId.Value)
                            .Select(d => new ActivityTimelineItemDto
                            {
                                Id = d.DealId,
                                Source = "DealCreated",
                                ActivityType = "Create",
                                Timestamp = d.Create_time,
                                Description = "Created deal: " + d.DealTitle,
                                EntityName = d.DealTitle,
                                EntityId = d.DealId,
                                EmployeeId = d.AssignedTo
                            }).ToListAsync();

                        allItems.AddRange(dealsCreated);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "DailyActivityReport: Failed to query DealCreated source");
                    }
                }
            }

            // Source 5: Deal status updates
            if (string.IsNullOrEmpty(query.ActivitySource) || query.ActivitySource == "Deal")
            {
                if (string.IsNullOrEmpty(query.ActivityType) || query.ActivityType == "Update")
                {
                    try
                    {
                        var dealsUpdated = await Context.Queryable<SalesDeal>()
                            .Where(d => d.Update_time != null && d.Update_time >= query.StartDate && d.Update_time < endDate)
                            .Where(d => d.Update_time != d.Create_time)
                            .WhereIF(query.EmployeeId.HasValue, d => d.AssignedTo == query.EmployeeId.Value)
                            .Select(d => new ActivityTimelineItemDto
                            {
                                Id = d.DealId,
                                Source = "DealUpdated",
                                ActivityType = "Update",
                                Timestamp = d.Update_time.Value,
                                Description = "Updated deal: " + d.DealTitle + " (Status: " + d.DealStatus + ")",
                                EntityName = d.DealTitle,
                                EntityId = d.DealId,
                                EmployeeId = d.AssignedTo
                            }).ToListAsync();

                        allItems.AddRange(dealsUpdated);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "DailyActivityReport: Failed to query DealUpdated source");
                    }
                }
            }

            // Source 6: Site visits (match by AssignedStaffId)
            if (string.IsNullOrEmpty(query.ActivitySource) || query.ActivitySource == "Visit")
            {
                if (string.IsNullOrEmpty(query.ActivityType) || query.ActivityType == "Visit")
                {
                    try
                    {
                        var siteVisits = await Context.Queryable<SiteVisit, BpLead>(
                            (v, l) => new JoinQueryInfos(JoinType.Left, v.LeadId == l.LeadId))
                            .Where((v, l) =>
                                (v.CompletedDate != null && v.CompletedDate >= query.StartDate && v.CompletedDate < endDate) ||
                                (v.CompletedDate == null && v.Create_time >= query.StartDate && v.Create_time < endDate))
                            .WhereIF(query.EmployeeId.HasValue, (v, l) => v.AssignedStaffId == query.EmployeeId.Value)
                            .Select((v, l) => new ActivityTimelineItemDto
                            {
                                Id = v.VisitId,
                                Source = "SiteVisit",
                                ActivityType = "Visit",
                                Timestamp = v.CompletedDate ?? v.Create_time,
                                Description = "Site visit: " + l.BrandName + " (" + v.VisitStatus + ")",
                                EntityName = l.BrandName,
                                EntityId = v.VisitId,
                                Notes = v.CompletionNotes ?? v.VisitNotes,
                                EmployeeId = v.AssignedStaffId ?? 0
                            }).ToListAsync();

                        allItems.AddRange(siteVisits);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "DailyActivityReport: Failed to query SiteVisit source");
                    }
                }
            }

            // Source 7: Lead assignments received (match by ToEmployeeId)
            if (string.IsNullOrEmpty(query.ActivitySource) || query.ActivitySource == "Assignment")
            {
                if (string.IsNullOrEmpty(query.ActivityType) || query.ActivityType == "Assign")
                {
                    try
                    {
                        var assignments = await Context.Queryable<BpLeadAssignmentLog, BpLead>(
                            (a, l) => new JoinQueryInfos(JoinType.Left, a.LeadId == l.LeadId))
                            .Where((a, l) => a.AssignmentDate >= query.StartDate && a.AssignmentDate < endDate)
                            .WhereIF(query.EmployeeId.HasValue, (a, l) => a.ToEmployeeId == query.EmployeeId.Value)
                            .Select((a, l) => new ActivityTimelineItemDto
                            {
                                Id = a.AssignmentLogId,
                                Source = "Assignment",
                                ActivityType = "Assign",
                                Timestamp = a.AssignmentDate,
                                Description = "Assigned lead: " + l.BrandName + " (" + a.ReassignmentType + ")",
                                EntityName = l.BrandName,
                                EntityId = a.LeadId,
                                Notes = a.Reason,
                                EmployeeId = a.ToEmployeeId
                            }).ToListAsync();

                        allItems.AddRange(assignments);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "DailyActivityReport: Failed to query Assignment source");
                    }
                }
            }

            // Enrich employee names via a single lookup
            try
            {
                var employeeIds = allItems.Select(i => i.EmployeeId).Where(id => id > 0).Distinct().ToList();
                if (employeeIds.Count > 0)
                {
                    var employeeNames = await Context.Queryable<HrEmployeeCore, HrPersonName>(
                        (e, n) => new JoinQueryInfos(JoinType.Left, e.PersonId == n.PersonId && n.IsPrimary == true))
                        .Where((e, n) => employeeIds.Contains(e.EmployeeId))
                        .Select((e, n) => new
                        {
                            e.EmployeeId,
                            FullName = n.GivenName + " " + n.FamilyName
                        }).ToListAsync();

                    var nameMap = employeeNames.GroupBy(x => x.EmployeeId).ToDictionary(g => g.Key, g => g.First().FullName);

                    foreach (var item in allItems)
                    {
                        if (nameMap.TryGetValue(item.EmployeeId, out var name))
                        {
                            item.EmployeeName = name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DailyActivityReport: Failed to enrich employee names");
            }

            // Sort by timestamp descending
            return allItems.OrderByDescending(i => i.Timestamp).ToList();
        }

        public async Task<List<EmployeeDailyPerformanceDto>> GetTeamPerformanceAsync(DateTime startDate, DateTime endDate)
        {
            var endDateExclusive = endDate.Date.AddDays(1);

            // Get all activities in range
            var activities = await Context.Queryable<BpLeadActivity>()
                .Where(a => a.ActivityDate >= startDate && a.ActivityDate < endDateExclusive)
                .ToListAsync();

            // Get all leads created in range
            var leadsCreated = await Context.Queryable<BpLead>()
                .Where(l => l.Create_time >= startDate && l.Create_time < endDateExclusive)
                .Select(l => new { l.AssignedTo })
                .ToListAsync();

            // Get all deals created in range
            var dealsCreated = await Context.Queryable<SalesDeal>()
                .Where(d => d.Create_time >= startDate && d.Create_time < endDateExclusive)
                .Select(d => new { d.AssignedTo })
                .ToListAsync();

            // Get all site visits in range
            var siteVisits = await Context.Queryable<SiteVisit>()
                .Where(v =>
                    (v.CompletedDate != null && v.CompletedDate >= startDate && v.CompletedDate < endDateExclusive) ||
                    (v.CompletedDate == null && v.Create_time >= startDate && v.Create_time < endDateExclusive))
                .Select(v => new { EmployeeId = v.AssignedStaffId ?? 0L })
                .ToListAsync();

            // Collect all employee IDs
            var allEmployeeIds = activities.Select(a => a.EmployeeId)
                .Concat(leadsCreated.Select(l => l.AssignedTo))
                .Concat(dealsCreated.Select(d => d.AssignedTo))
                .Concat(siteVisits.Select(v => v.EmployeeId))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (allEmployeeIds.Count == 0)
                return new List<EmployeeDailyPerformanceDto>();

            // Get employee details
            var employees = await Context.Queryable<HrEmployeeCore, HrPersonName>(
                (e, n) => new JoinQueryInfos(JoinType.Left, e.PersonId == n.PersonId && n.IsPrimary == true))
                .Where((e, n) => allEmployeeIds.Contains(e.EmployeeId))
                .Select((e, n) => new
                {
                    e.EmployeeId,
                    e.EmployeeCode,
                    FullName = n.GivenName + " " + n.FamilyName
                }).ToListAsync();

            var empMap = employees.GroupBy(x => x.EmployeeId).ToDictionary(g => g.Key, g => g.First());

            // Build performance rows
            var result = new List<EmployeeDailyPerformanceDto>();

            foreach (var empId in allEmployeeIds)
            {
                var empActivities = activities.Where(a => a.EmployeeId == empId).ToList();
                var emp = empMap.GetValueOrDefault(empId);

                result.Add(new EmployeeDailyPerformanceDto
                {
                    EmployeeId = empId,
                    EmployeeName = emp?.FullName ?? "Unknown",
                    EmployeeCode = emp?.EmployeeCode ?? "",
                    TotalActions = empActivities.Count
                        + leadsCreated.Count(l => l.AssignedTo == empId)
                        + dealsCreated.Count(d => d.AssignedTo == empId)
                        + siteVisits.Count(v => v.EmployeeId == empId),
                    Calls = empActivities.Count(a => a.ActivityType == "Call"),
                    Meetings = empActivities.Count(a => a.ActivityType == "Meeting"),
                    Emails = empActivities.Count(a => a.ActivityType == "Email"),
                    SiteVisits = siteVisits.Count(v => v.EmployeeId == empId),
                    LeadsCreated = leadsCreated.Count(l => l.AssignedTo == empId),
                    DealsCreated = dealsCreated.Count(d => d.AssignedTo == empId),
                    TotalMinutes = empActivities.Where(a => a.DurationMinutes.HasValue).Sum(a => a.DurationMinutes.Value)
                });
            }

            return result.OrderByDescending(r => r.TotalActions).ToList();
        }

        private ActivitySummaryDto BuildSummary(List<ActivityTimelineItemDto> timeline)
        {
            var summary = new ActivitySummaryDto
            {
                TotalActivities = timeline.Count,
                LeadsCreated = timeline.Count(t => t.Source == "LeadCreated"),
                LeadsContacted = timeline.Count(t => t.Source == "LeadActivity"),
                LeadsConverted = timeline.Count(t => t.Source == "LeadUpdated" && (t.Description?.Contains("Converted") ?? false)),
                CallsMade = timeline.Count(t => t.ActivityType == "Call"),
                MeetingsHeld = timeline.Count(t => t.ActivityType == "Meeting"),
                EmailsSent = timeline.Count(t => t.ActivityType == "Email"),
                SiteVisitsCompleted = timeline.Count(t => t.Source == "SiteVisit"),
                DealsCreated = timeline.Count(t => t.Source == "DealCreated"),
                DealsWon = timeline.Count(t => t.Source == "DealUpdated" && (t.Description?.Contains("Won") ?? false)),
                AssignmentsReceived = timeline.Count(t => t.Source == "Assignment"),
                TotalDurationMinutes = timeline.Where(t => t.DurationMinutes.HasValue).Sum(t => t.DurationMinutes.Value),
                BySource = timeline.GroupBy(t => t.Source).ToDictionary(g => g.Key, g => g.Count()),
                ByActivityType = timeline.GroupBy(t => t.ActivityType).ToDictionary(g => g.Key, g => g.Count()),
                ByHour = timeline.GroupBy(t => t.Timestamp.Hour.ToString("D2")).ToDictionary(g => g.Key, g => g.Count())
            };

            return summary;
        }
    }
}
