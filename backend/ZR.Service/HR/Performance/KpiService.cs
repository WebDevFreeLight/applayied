using ZR.Model.HR.EmployeeCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Model.HR.Performance;
using ZR.Repository;
using ZR.Service.HR.IService;
using ZR.ServiceCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// KPI Service Implementation
    /// Provides comprehensive KPI management including KPI definitions, employee assignments, achievements tracking, and analytics
    /// </summary>
    [AppService(ServiceType = typeof(IKpiService), ServiceLifetime = LifeTime.Transient)]
    public class KpiService : BaseService<HrKpi>, IKpiService
    {
        private readonly IEmployeeDataService _employeeDataService;

        public KpiService(IEmployeeDataService employeeDataService)
        {
            _employeeDataService = employeeDataService;
        }
        // KPI Management
        public async Task<PagedInfo<HrKpi>> GetKpisAsync(Expression<Func<HrKpi, bool>> predicate, PagerInfo pager)
        {
            var query = Context.Queryable<HrKpi>()
                .Where(predicate)
                .OrderBy(k => k.Create_time, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }

        public async Task<List<HrKpi>> GetActiveKpisAsync()
        {
            return await Context.Queryable<HrKpi>()
                .Where(k => k.IsActive)
                .OrderBy(k => k.Category)
                .OrderBy(k => k.KpiName)
                .ToListAsync();
        }

        public async Task<HrKpi> GetKpiByIdAsync(long kpiId)
        {
            return await Context.Queryable<HrKpi>()
                .FirstAsync(k => k.KpiId == kpiId);
        }

        public async Task<int> AddKpiAsync(HrKpi kpi)
        {
            // Validate KPI data
            if (string.IsNullOrEmpty(kpi.KpiName))
                throw new ArgumentException("KPI name is required");

            if (string.IsNullOrEmpty(kpi.KpiCode))
                throw new ArgumentException("KPI code is required");

            if (string.IsNullOrEmpty(kpi.Category))
                throw new ArgumentException("KPI category is required");

            // Check for duplicate KPI codes
            var exists = await Context.Queryable<HrKpi>()
                .AnyAsync(k => k.KpiCode == kpi.KpiCode);
            if (exists)
                throw new InvalidOperationException("KPI code already exists");

            kpi.IsActive = true;
            kpi.Create_time = DateTime.Now;

            return await Context.Insertable(kpi).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateKpiAsync(HrKpi kpi)
        {
            // Validate KPI data
            if (string.IsNullOrEmpty(kpi.KpiName))
                throw new ArgumentException("KPI name is required");

            // Check for duplicate KPI codes (excluding current KPI)
            var exists = await Context.Queryable<HrKpi>()
                .AnyAsync(k => k.KpiCode == kpi.KpiCode && k.KpiId != kpi.KpiId);
            if (exists)
                throw new InvalidOperationException("KPI code already exists");

            kpi.Update_time = DateTime.Now;
            return await Context.Updateable(kpi)
                .IgnoreColumns(k => new { k.Create_time, k.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> DeleteKpiAsync(long kpiId)
        {
            // Check if KPI is assigned to employees
            var assignmentExists = await Context.Queryable<HrEmployeeKpi>()
                .AnyAsync(ek => ek.KpiId == kpiId && ek.Status == "active");
            if (assignmentExists)
                throw new InvalidOperationException("Cannot delete KPI with active assignments");

            return await Context.Deleteable<HrKpi>()
                .Where(k => k.KpiId == kpiId)
                .ExecuteCommandAsync();
        }

        public async Task<List<HrKpi>> GetKpisByCategoryAsync(string category)
        {
            return await Context.Queryable<HrKpi>()
                .Where(k => k.IsActive && k.Category == category)
                .OrderBy(k => k.KpiName)
                .ToListAsync();
        }

        public async Task<List<HrKpi>> GetDepartmentKpisAsync(long departmentId)
        {
            return await Context.Queryable<HrKpi>()
                .Where(k => k.IsActive && (k.DepartmentId == departmentId || k.IsCompanyWide))
                .OrderBy(k => k.KpiName)
                .ToListAsync();
        }

        public async Task<List<HrKpi>> GetPositionKpisAsync(long positionId)
        {
            return await Context.Queryable<HrKpi>()
                .Where(k => k.IsActive && (k.PositionId == positionId || k.IsCompanyWide))
                .OrderBy(k => k.KpiName)
                .ToListAsync();
        }

        // Employee KPI Assignment
        public async Task<List<HrEmployeeKpi>> GetEmployeeKpisAsync(long employeeId)
        {
            return await Context.Queryable<HrEmployeeKpi>()
                .LeftJoin<HrKpi>((ek, k) => ek.KpiId == k.KpiId)
                .Where((ek, k) => ek.EmployeeId == employeeId)
                .Select((ek, k) => new HrEmployeeKpi
                {
                    AssignmentId = ek.AssignmentId,
                    EmployeeId = ek.EmployeeId,
                    KpiId = ek.KpiId,
                    Year = ek.Year,
                    Quarter = ek.Quarter,
                    TargetValue = ek.TargetValue,
                    Weight = ek.Weight,
                    StartDate = ek.StartDate,
                    EndDate = ek.EndDate,
                    Status = ek.Status,
                    Notes = ek.Notes
                })
                .OrderBy(ek => ek.Status == "active" ? 0 : 1)
                .OrderBy(ek => ek.Year, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<int> AssignKpiToEmployeeAsync(HrEmployeeKpi assignment)
        {
            // Validate assignment data
            if (assignment.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (assignment.KpiId <= 0)
                throw new ArgumentException("KPI ID is required");

            if (assignment.Year <= 0)
                throw new ArgumentException("Year is required");

            // Check if assignment already exists
            var exists = await Context.Queryable<HrEmployeeKpi>()
                .AnyAsync(ek => ek.EmployeeId == assignment.EmployeeId 
                    && ek.KpiId == assignment.KpiId 
                    && ek.Year == assignment.Year 
                    && (assignment.Quarter == null || ek.Quarter == assignment.Quarter)
                    && ek.Status == "active");

            if (exists)
                throw new InvalidOperationException("KPI is already assigned to this employee for the specified period");

            // Get KPI details for default values
            var kpi = await GetKpiByIdAsync(assignment.KpiId);
            if (kpi == null)
                throw new ArgumentException("Invalid KPI ID");

            if (assignment.TargetValue <= 0)
                assignment.TargetValue = kpi.TargetValue;

            if (assignment.Weight <= 0)
                assignment.Weight = kpi.Weight;

            assignment.Status = "active";
            assignment.Create_time = DateTime.Now;

            return await Context.Insertable(assignment).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateEmployeeKpiAsync(HrEmployeeKpi assignment)
        {
            assignment.Update_time = DateTime.Now;
            return await Context.Updateable(assignment)
                .IgnoreColumns(ek => new { ek.Create_time, ek.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> RemoveEmployeeKpiAsync(long assignmentId)
        {
            return await Context.Updateable<HrEmployeeKpi>()
                .SetColumns(ek => new HrEmployeeKpi
                {
                    Status = "cancelled",
                    EndDate = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(ek => ek.AssignmentId == assignmentId)
                .ExecuteCommandAsync();
        }

        public async Task<List<HrEmployeeKpi>> GetKpiAssignmentsByPeriodAsync(int year, int? quarter)
        {
            var query = Context.Queryable<HrEmployeeKpi>()
                .LeftJoin<HrEmployeeCore>((ek, e) => ek.EmployeeId == e.EmployeeId)
                .LeftJoin<HrKpi>((ek, e, k) => ek.KpiId == k.KpiId)
                .Where((ek, e, k) => ek.Year == year && ek.Status == "active");

            if (quarter.HasValue)
                query = query.Where((ek, e, k) => ek.Quarter == quarter);

            return await query.Select((ek, e, k) => ek).ToListAsync();
        }

        // KPI Achievement
        public async Task<List<HrKpiAchievement>> GetKpiAchievementsAsync(long employeeId, DateTime periodDate)
        {
            return await Context.Queryable<HrKpiAchievement>()
                .LeftJoin<HrKpi>((ka, k) => ka.KpiId == k.KpiId)
                .Where((ka, k) => ka.EmployeeId == employeeId && ka.PeriodDate.Date == periodDate.Date)
                .Select((ka, k) => new HrKpiAchievement
                {
                    AchievementId = ka.AchievementId,
                    AssignmentId = ka.AssignmentId,
                    EmployeeId = ka.EmployeeId,
                    KpiId = ka.KpiId,
                    PeriodDate = ka.PeriodDate,
                    ActualValue = ka.ActualValue,
                    TargetValue = ka.TargetValue,
                    AchievementPercentage = ka.AchievementPercentage,
                    Score = ka.Score,
                    Comments = ka.Comments,
                    EvidenceUrl = ka.EvidenceUrl,
                    IsVerified = ka.IsVerified,
                    VerifiedBy = ka.VerifiedBy,
                    VerificationDate = ka.VerificationDate
                })
                .OrderBy(ka => ka.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<int> RecordKpiAchievementAsync(HrKpiAchievement achievement)
        {
            // Validate achievement data
            if (achievement.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (achievement.KpiId <= 0)
                throw new ArgumentException("KPI ID is required");

            if (achievement.AssignmentId <= 0)
                throw new ArgumentException("Assignment ID is required");

            // Check if achievement already exists for this period
            var exists = await Context.Queryable<HrKpiAchievement>()
                .AnyAsync(ka => ka.EmployeeId == achievement.EmployeeId 
                    && ka.KpiId == achievement.KpiId 
                    && ka.PeriodDate.Date == achievement.PeriodDate.Date);

            if (exists)
                throw new InvalidOperationException("Achievement already recorded for this period");

            // Calculate achievement percentage and score
            if (achievement.TargetValue > 0)
            {
                achievement.AchievementPercentage = (achievement.ActualValue / achievement.TargetValue) * 100;
                
                // Get KPI details for scoring logic
                var kpi = await GetKpiByIdAsync(achievement.KpiId);
                achievement.Score = CalculateKpiScore(achievement.ActualValue, achievement.TargetValue, kpi);
            }

            achievement.IsVerified = false;
            achievement.Create_time = DateTime.Now;

            return await Context.Insertable(achievement).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateKpiAchievementAsync(HrKpiAchievement achievement)
        {
            // Recalculate metrics
            if (achievement.TargetValue > 0)
            {
                achievement.AchievementPercentage = (achievement.ActualValue / achievement.TargetValue) * 100;
                
                var kpi = await GetKpiByIdAsync(achievement.KpiId);
                achievement.Score = CalculateKpiScore(achievement.ActualValue, achievement.TargetValue, kpi);
            }

            achievement.Update_time = DateTime.Now;
            return await Context.Updateable(achievement)
                .IgnoreColumns(ka => new { ka.Create_time, ka.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<decimal> CalculateKpiScoreAsync(long employeeId, DateTime periodDate)
        {
            var achievements = await GetKpiAchievementsAsync(employeeId, periodDate);
            if (!achievements.Any())
                return 0;

            // Get weighted average score
            var totalWeightedScore = 0m;
            var totalWeight = 0m;

            foreach (var achievement in achievements)
            {
                var assignment = await Context.Queryable<HrEmployeeKpi>()
                    .FirstAsync(ek => ek.AssignmentId == achievement.AssignmentId);
                
                if (assignment != null)
                {
                    totalWeightedScore += achievement.Score * assignment.Weight;
                    totalWeight += assignment.Weight;
                }
            }

            return totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
        }

        public async Task<int> VerifyKpiAchievementAsync(long achievementId, long verifiedBy)
        {
            return await Context.Updateable<HrKpiAchievement>()
                .SetColumns(ka => new HrKpiAchievement
                {
                    IsVerified = true,
                    VerifiedBy = verifiedBy,
                    VerificationDate = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(ka => ka.AchievementId == achievementId)
                .ExecuteCommandAsync();
        }

        // KPI Metrics (Legacy Support)
        public async Task<List<HrKpiMetric>> GetKpiMetricsAsync(long employeeId)
        {
            return await Context.Queryable<HrKpiMetric>()
                .Where(km => km.EmployeeId == employeeId && km.IsActive)
                .OrderBy(km => km.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<int> AddKpiMetricAsync(HrKpiMetric metric)
        {
            // Validate metric data
            if (metric.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (string.IsNullOrEmpty(metric.MetricName))
                throw new ArgumentException("Metric name is required");

            // Calculate score based on target vs actual
            if (metric.Target > 0)
            {
                metric.Score = (metric.Actual / metric.Target) * 100;
            }

            metric.IsActive = true;
            metric.Create_time = DateTime.Now;

            return await Context.Insertable(metric).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateKpiMetricAsync(HrKpiMetric metric)
        {
            // Recalculate score
            if (metric.Target > 0)
            {
                metric.Score = (metric.Actual / metric.Target) * 100;
            }

            metric.Update_time = DateTime.Now;
            return await Context.Updateable(metric)
                .IgnoreColumns(km => new { km.Create_time, km.Create_by })
                .ExecuteCommandAsync();
        }

        // KPI Records
        public async Task<List<HrKpiRecord>> GetKpiRecordsAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            return await Context.Queryable<HrKpiRecord>()
                .LeftJoin<HrKpiMetric>((kr, km) => kr.MetricId == km.MetricId)
                .Where((kr, km) => km.EmployeeId == employeeId && 
                      kr.Period >= startDate && kr.Period <= endDate)
                .Select((kr, km) => kr)
                .OrderBy(kr => kr.Period, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<int> AddKpiRecordAsync(HrKpiRecord record)
        {
            // Validate record data
            if (record.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (record.MetricId <= 0)
                throw new ArgumentException("Metric ID is required");

            record.Create_time = DateTime.Now;
            return await Context.Insertable(record).ExecuteReturnIdentityAsync();
        }








        public async Task<object> GetKpiDashboardAsync(long? employeeId, int year)
        {
            // Base query for assignments
            var assignmentsQuery = Context.Queryable<HrEmployeeKpi>()
                .LeftJoin<HrKpi>((ek, k) => ek.KpiId == k.KpiId)
                .Where((ek, k) => ek.Year == year && ek.Status == "active");

            // Apply employee filter if provided
            if (employeeId.HasValue)
            {
                assignmentsQuery = assignmentsQuery.Where((ek, k) => ek.EmployeeId == employeeId.Value);
            }

            var assignments = await assignmentsQuery
                .Select((ek, k) => new { Assignment = ek, Kpi = k })
                .ToListAsync();

            // Base query for achievements
            var achievementsQuery = Context.Queryable<HrKpiAchievement>()
                .Where(ka => ka.PeriodDate.Year == year);

            // Apply employee filter if provided
            if (employeeId.HasValue)
            {
                achievementsQuery = achievementsQuery.Where(ka => ka.EmployeeId == employeeId.Value);
            }

            var achievements = await achievementsQuery.ToListAsync();

            // Get employee IDs for grouping if no specific employee is selected
            var employeeIds = employeeId.HasValue
                ? new List<long> { employeeId.Value }
                : assignments.Select(a => a.Assignment.EmployeeId).Distinct().ToList();

            var dashboard = new
            {
                EmployeeId = employeeId,
                Year = year,
                TotalKpis = assignments.Count,
                CompletedKpis = achievements.Count(a => a.AchievementPercentage >= 100),
                AverageAchievement = achievements.Any() ? achievements.Average(a => a.AchievementPercentage) : 0,
                OverallScore = achievements.Any() ? achievements.Average(a => a.Score) : 0,
                KpiBreakdown = assignments.GroupBy(a => a.Kpi.Category).Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    AverageScore = achievements.Where(ach => g.Any(kpi => kpi.Assignment.KpiId == ach.KpiId))
                                             .DefaultIfEmpty()
                                             .Average(ach => ach?.Score ?? 0)
                }).ToList(),
                MonthlyTrends = achievements.GroupBy(a => a.PeriodDate.Month).Select(g => new
                {
                    Month = g.Key,
                    AverageScore = g.Average(a => a.Score),
                    AchievementCount = g.Count()
                }).OrderBy(m => m.Month).ToList(),
                // Additional aggregated data when no specific employee is selected
                EmployeeCount = employeeIds.Count,
                DepartmentBreakdown = !employeeId.HasValue ? assignments
                    .GroupBy(a => a.Assignment.departmentid) // Assuming there's a DepartmentId field
                    .Select(g => new
                    {
                        departmentid = g.Key,
                        KpiCount = g.Count(),
                        AverageScore = achievements.Where(ach => g.Any(kpi => kpi.Assignment.KpiId == ach.KpiId))
                                                 .DefaultIfEmpty()
                                                 .Average(ach => ach?.Score ?? 0)
                    }).ToList() : null
            };

            return dashboard;
        }




        // KPI Analytics
        public async Task<object> GetKpiDashboard22Async(long? employeeId, int year)
        {
            var assignments = await Context.Queryable<HrEmployeeKpi>()
                .LeftJoin<HrKpi>((ek, k) => ek.KpiId == k.KpiId)
                .Where((ek, k) => ek.EmployeeId == employeeId && ek.Year == year && ek.Status == "active")
                .Select((ek, k) => new { Assignment = ek, Kpi = k })
                .ToListAsync();

            var achievements = await Context.Queryable<HrKpiAchievement>()
                .Where(ka => ka.EmployeeId == employeeId && ka.PeriodDate.Year == year)
                .ToListAsync();

            var dashboard = new
            {
                EmployeeId = employeeId,
                Year = year,
                TotalKpis = assignments.Count,
                CompletedKpis = achievements.Count(a => a.AchievementPercentage >= 100),
                AverageAchievement = achievements.Any() ? achievements.Average(a => a.AchievementPercentage) : 0,
                OverallScore = achievements.Any() ? achievements.Average(a => a.Score) : 0,
                KpiBreakdown = assignments.GroupBy(a => a.Kpi.Category).Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    AverageScore = achievements.Where(ach => g.Any(kpi => kpi.Assignment.KpiId == ach.KpiId))
                                             .DefaultIfEmpty()
                                             .Average(ach => ach?.Score ?? 0)
                }).ToList(),
                MonthlyTrends = achievements.GroupBy(a => a.PeriodDate.Month).Select(g => new
                {
                    Month = g.Key,
                    AverageScore = g.Average(a => a.Score),
                    AchievementCount = g.Count()
                }).OrderBy(m => m.Month).ToList()
            };

            return dashboard;
        }

        public async Task<object> GetTeamKpiPerformanceAsync(long managerId, DateTime periodDate)
        {
            // Get team members using EmployeeDataService
            var teamMembersData = await _employeeDataService.GetTeamMembersAsync(managerId);
            var teamMembers = teamMembersData.Select(e => e.EmployeeId).ToList();

            if (!teamMembers.Any())
                return new { TeamMembers = 0, Achievements = new List<object>() };

            // Get employee dictionary for lookups
            var employeeDict = teamMembersData.ToDictionary(e => e.EmployeeId);

            var achievements = await Context.Queryable<HrKpiAchievement>()
                .LeftJoin<HrKpi>((ka, k) => ka.KpiId == k.KpiId)
                .Where((ka, k) => teamMembers.Contains(ka.EmployeeId) &&
                      ka.PeriodDate.Month == periodDate.Month && ka.PeriodDate.Year == periodDate.Year)
                .Select((ka, k) => new
                {
                    EmployeeId = ka.EmployeeId,
                    KpiName = k.KpiName,
                    Category = k.Category,
                    AchievementPercentage = ka.AchievementPercentage,
                    Score = ka.Score,
                    IsVerified = ka.IsVerified
                })
                .ToListAsync();

            // Add employee names from dictionary
            var achievementsWithNames = achievements.Select(a =>
            {
                var emp = employeeDict.GetValueOrDefault(a.EmployeeId);
                return new
                {
                    a.EmployeeId,
                    EmployeeName = emp?.NameAr ?? "",
                    a.KpiName,
                    a.Category,
                    a.AchievementPercentage,
                    a.Score,
                    a.IsVerified
                };
            }).ToList();

            return new
            {
                ManagerId = managerId,
                Period = periodDate,
                TeamMembers = teamMembers.Count,
                TotalAchievements = achievementsWithNames.Count,
                AverageTeamScore = achievementsWithNames.Any() ? achievementsWithNames.Average(a => a.Score) : 0,
                VerificationRate = achievementsWithNames.Any() ? (achievementsWithNames.Count(a => a.IsVerified) * 100.0 / achievementsWithNames.Count) : 0,
                TopPerformers = achievementsWithNames.GroupBy(a => new { a.EmployeeId, a.EmployeeName })
                                           .Select(g => new
                                           {
                                               EmployeeId = g.Key.EmployeeId,
                                               EmployeeName = g.Key.EmployeeName,
                                               AverageScore = g.Average(a => a.Score),
                                               KpiCount = g.Count()
                                           })
                                           .OrderByDescending(p => p.AverageScore)
                                           .Take(5)
                                           .ToList(),
                CategoryPerformance = achievementsWithNames.GroupBy(a => a.Category)
                                                 .Select(g => new
                                                 {
                                                     Category = g.Key,
                                                     AverageScore = g.Average(a => a.Score),
                                                     Count = g.Count()
                                                 })
                                                 .ToList()
            };
        }

        public async Task<object> GetDepartmentKpiAnalyticsAsync(long departmentId, int year)
        {
            // Get employees by department using EmployeeDataService
            var employeesData = await _employeeDataService.GetEmployeesByDepartmentAsync(departmentId);
            var employees = employeesData.Select(e => e.EmployeeId).ToList();

            if (!employees.Any())
                return new { Employees = 0, Analytics = new { } };

            var achievements = await Context.Queryable<HrKpiAchievement>()
                .LeftJoin<HrKpi>((ka, k) => ka.KpiId == k.KpiId)
                .Where((ka, k) => employees.Contains(ka.EmployeeId) && ka.PeriodDate.Year == year)
                .Select((ka, k) => new
                {
                    EmployeeId = ka.EmployeeId,
                    KpiName = k.KpiName,
                    Category = k.Category,
                    Score = ka.Score,
                    AchievementPercentage = ka.AchievementPercentage,
                    Month = ka.PeriodDate.Month
                })
                .ToListAsync();

            return new
            {
                DepartmentId = departmentId,
                Year = year,
                Employees = employees.Count,
                TotalAchievements = achievements.Count,
                DepartmentScore = achievements.Any() ? achievements.Average(a => a.Score) : 0,
                MonthlyTrends = achievements.GroupBy(a => a.Month)
                                          .Select(g => new
                                          {
                                              Month = g.Key,
                                              AverageScore = g.Average(a => a.Score),
                                              EmployeesWithKpis = g.Select(a => a.EmployeeId).Distinct().Count()
                                          })
                                          .OrderBy(m => m.Month)
                                          .ToList(),
                CategoryBreakdown = achievements.GroupBy(a => a.Category)
                                               .Select(g => new
                                               {
                                                   Category = g.Key,
                                                   AverageScore = g.Average(a => a.Score),
                                                   Count = g.Count()
                                               })
                                               .ToList()
            };
        }

        public async Task<object> GetKpiTrendsAsync(long employeeId, int months)
        {
            var startDate = DateTime.Now.AddMonths(-months);
            var achievements = await Context.Queryable<HrKpiAchievement>()
                .LeftJoin<HrKpi>((ka, k) => ka.KpiId == k.KpiId)
                .Where((ka, k) => ka.EmployeeId == employeeId && ka.PeriodDate >= startDate)
                .Select((ka, k) => new
                {
                    PeriodDate = ka.PeriodDate,
                    Score = ka.Score,
                    Category = k.Category,
                    KpiName = k.KpiName
                })
                .OrderBy(ka => ka.PeriodDate)
                .ToListAsync();

            var trends = achievements.GroupBy(a => new { a.PeriodDate.Year, a.PeriodDate.Month })
                                   .Select(g => new
                                   {
                                       Year = g.Key.Year,
                                       Month = g.Key.Month,
                                       AverageScore = g.Average(a => a.Score),
                                       KpiCount = g.Count()
                                   })
                                   .OrderBy(t => t.Year)
                                   .ThenBy(t => t.Month)
                                   .ToList();

            return new
            {
                EmployeeId = employeeId,
                Period = $"Last {months} months",
                Trends = trends,
                CategoryTrends = achievements.GroupBy(a => a.Category)
                                           .Select(g => new
                                           {
                                               Category = g.Key,
                                               MonthlyScores = g.GroupBy(a => new { a.PeriodDate.Year, a.PeriodDate.Month })
                                                              .Select(mg => new
                                                              {
                                                                  Year = mg.Key.Year,
                                                                  Month = mg.Key.Month,
                                                                  Score = mg.Average(a => a.Score)
                                                              })
                                                              .OrderBy(mg => mg.Year)
                                                              .ThenBy(mg => mg.Month)
                                                              .ToList()
                                           })
                                           .ToList()
            };
        }

        public async Task<List<object>> GetTopPerformersAsync(DateTime periodDate, int topN)
        {
            // Get achievements for the period
            var achievements = await Context.Queryable<HrKpiAchievement>()
                .Where(ka => ka.PeriodDate.Month == periodDate.Month && ka.PeriodDate.Year == periodDate.Year)
                .Select(ka => new { ka.EmployeeId, ka.Score, ka.AchievementId })
                .ToListAsync();

            // Calculate top performers
            var topPerformerIds = achievements
                .GroupBy(a => a.EmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    AverageScore = g.Average(a => a.Score),
                    KpiCount = g.Count()
                })
                .OrderByDescending(p => p.AverageScore)
                .Take(topN)
                .ToList();

            // Batch load employee data
            var employeeIds = topPerformerIds.Select(p => p.EmployeeId).ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            // Combine data
            var topPerformers = topPerformerIds.Select(p =>
            {
                var emp = employeeDict.GetValueOrDefault(p.EmployeeId);
                return new
                {
                    EmployeeId = p.EmployeeId,
                    EmployeeName = emp?.NameAr ?? "",
                    DepartmentId = emp?.DepartmentId,
                    AverageScore = p.AverageScore,
                    KpiCount = p.KpiCount
                };
            }).ToList();

            return topPerformers.Cast<object>().ToList();
        }

        public async Task<List<object>> GetUnderperformersAsync(DateTime periodDate, int bottomN)
        {
            // Get achievements for the period
            var achievements = await Context.Queryable<HrKpiAchievement>()
                .Where(ka => ka.PeriodDate.Month == periodDate.Month && ka.PeriodDate.Year == periodDate.Year)
                .Select(ka => new { ka.EmployeeId, ka.Score, ka.AchievementId })
                .ToListAsync();

            // Calculate underperformers (below 70% performance)
            var underperformerIds = achievements
                .GroupBy(a => a.EmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    AverageScore = g.Average(a => a.Score),
                    KpiCount = g.Count()
                })
                .Where(p => p.AverageScore < 70)
                .OrderBy(p => p.AverageScore)
                .Take(bottomN)
                .ToList();

            // Batch load employee data
            var employeeIds = underperformerIds.Select(p => p.EmployeeId).ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            // Combine data
            var underperformers = underperformerIds.Select(p =>
            {
                var emp = employeeDict.GetValueOrDefault(p.EmployeeId);
                return new
                {
                    EmployeeId = p.EmployeeId,
                    EmployeeName = emp?.NameAr ?? "",
                    DepartmentId = emp?.DepartmentId,
                    AverageScore = p.AverageScore,
                    KpiCount = p.KpiCount
                };
            }).ToList();

            return underperformers.Cast<object>().ToList();
        }

        // KPI Calculation
        public async Task<decimal> CalculateWeightedScoreAsync(long employeeId, DateTime periodDate)
        {
            var achievements = await Context.Queryable<HrKpiAchievement>()
                .LeftJoin<HrEmployeeKpi>((ka, ek) => ka.AssignmentId == ek.AssignmentId)
                .Where((ka, ek) => ka.EmployeeId == employeeId && 
                      ka.PeriodDate.Month == periodDate.Month && ka.PeriodDate.Year == periodDate.Year)
                .Select((ka, ek) => new
                {
                    Score = ka.Score,
                    Weight = ek.Weight
                })
                .ToListAsync();

            if (!achievements.Any())
                return 0;

            var totalWeightedScore = achievements.Sum(a => a.Score * a.Weight);
            var totalWeight = achievements.Sum(a => a.Weight);

            return totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
        }

        public async Task<int> ProcessMonthlyKpisAsync(DateTime month)
        {
            var processedCount = 0;
            var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Get all active monthly KPI assignments
            var monthlyAssignments = await Context.Queryable<HrEmployeeKpi>()
                .LeftJoin<HrKpi>((ek, k) => ek.KpiId == k.KpiId)
                .Where((ek, k) => ek.Status == "active" && k.Frequency == "monthly" &&
                      ek.StartDate <= lastDayOfMonth && (ek.EndDate == null || ek.EndDate >= firstDayOfMonth))
                .Select((ek, k) => new { Assignment = ek, Kpi = k })
                .ToListAsync();

            foreach (var item in monthlyAssignments)
            {
                // Check if achievement already recorded
                var existingAchievement = await Context.Queryable<HrKpiAchievement>()
                    .AnyAsync(ka => ka.AssignmentId == item.Assignment.AssignmentId && 
                             ka.PeriodDate.Month == month.Month && ka.PeriodDate.Year == month.Year);

                if (!existingAchievement)
                {
                    // Create placeholder achievement record for tracking
                    var achievement = new HrKpiAchievement
                    {
                        AssignmentId = item.Assignment.AssignmentId,
                        EmployeeId = item.Assignment.EmployeeId,
                        KpiId = item.Assignment.KpiId,
                        PeriodDate = lastDayOfMonth,
                        ActualValue = 0,
                        TargetValue = item.Assignment.TargetValue,
                        AchievementPercentage = 0,
                        Score = 0,
                        Comments = "Auto-generated for monthly KPI tracking",
                        IsVerified = false,
                        Create_time = DateTime.Now
                    };

                    await Context.Insertable(achievement).ExecuteCommandAsync();
                    processedCount++;
                }
            }

            return processedCount;
        }

        public async Task<int> ProcessQuarterlyKpisAsync(int year, int quarter)
        {
            var processedCount = 0;
            var quarterStartMonth = (quarter - 1) * 3 + 1;
            var quarterEndMonth = quarter * 3;
            var quarterEndDate = new DateTime(year, quarterEndMonth, DateTime.DaysInMonth(year, quarterEndMonth));

            // Get all active quarterly KPI assignments
            var quarterlyAssignments = await Context.Queryable<HrEmployeeKpi>()
                .LeftJoin<HrKpi>((ek, k) => ek.KpiId == k.KpiId)
                .Where((ek, k) => ek.Status == "active" && k.Frequency == "quarterly" &&
                      ek.Year == year && ek.Quarter == quarter)
                .Select((ek, k) => new { Assignment = ek, Kpi = k })
                .ToListAsync();

            foreach (var item in quarterlyAssignments)
            {
                // Check if achievement already recorded
                var existingAchievement = await Context.Queryable<HrKpiAchievement>()
                    .AnyAsync(ka => ka.AssignmentId == item.Assignment.AssignmentId && 
                             ka.PeriodDate.Year == year &&
                             ka.PeriodDate.Month >= quarterStartMonth && ka.PeriodDate.Month <= quarterEndMonth);

                if (!existingAchievement)
                {
                    var achievement = new HrKpiAchievement
                    {
                        AssignmentId = item.Assignment.AssignmentId,
                        EmployeeId = item.Assignment.EmployeeId,
                        KpiId = item.Assignment.KpiId,
                        PeriodDate = quarterEndDate,
                        ActualValue = 0,
                        TargetValue = item.Assignment.TargetValue,
                        AchievementPercentage = 0,
                        Score = 0,
                        Comments = $"Auto-generated for Q{quarter} {year} KPI tracking",
                        IsVerified = false,
                        Create_time = DateTime.Now
                    };

                    await Context.Insertable(achievement).ExecuteCommandAsync();
                    processedCount++;
                }
            }

            return processedCount;
        }

        public async Task<int> ProcessAnnualKpisAsync(int year)
        {
            var processedCount = 0;
            var yearEndDate = new DateTime(year, 12, 31);

            // Get all active annual KPI assignments
            var annualAssignments = await Context.Queryable<HrEmployeeKpi>()
                .LeftJoin<HrKpi>((ek, k) => ek.KpiId == k.KpiId)
                .Where((ek, k) => ek.Status == "active" && k.Frequency == "yearly" && ek.Year == year)
                .Select((ek, k) => new { Assignment = ek, Kpi = k })
                .ToListAsync();

            foreach (var item in annualAssignments)
            {
                // Check if achievement already recorded
                var existingAchievement = await Context.Queryable<HrKpiAchievement>()
                    .AnyAsync(ka => ka.AssignmentId == item.Assignment.AssignmentId && ka.PeriodDate.Year == year);

                if (!existingAchievement)
                {
                    var achievement = new HrKpiAchievement
                    {
                        AssignmentId = item.Assignment.AssignmentId,
                        EmployeeId = item.Assignment.EmployeeId,
                        KpiId = item.Assignment.KpiId,
                        PeriodDate = yearEndDate,
                        ActualValue = 0,
                        TargetValue = item.Assignment.TargetValue,
                        AchievementPercentage = 0,
                        Score = 0,
                        Comments = $"Auto-generated for {year} annual KPI tracking",
                        IsVerified = false,
                        Create_time = DateTime.Now
                    };

                    await Context.Insertable(achievement).ExecuteCommandAsync();
                    processedCount++;
                }
            }

            return processedCount;
        }

        // KPI Reports
        public async Task<byte[]> GenerateKpiReportAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            // This would integrate with a reporting engine like Crystal Reports, DevExpress, or similar
            // For now, return placeholder
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> GenerateTeamKpiReportAsync(long managerId, DateTime periodDate)
        {
            // This would integrate with a reporting engine
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportKpiDataAsync(Expression<Func<HrKpi, bool>> predicate, string format)
        {
            // This would integrate with export libraries like EPPlus for Excel, iTextSharp for PDF, etc.
            await Task.CompletedTask;
            return new byte[0];
        }

        // KPI Templates
        public async Task<List<object>> GetKpiTemplatesAsync(string category)
        {
            // This would retrieve predefined KPI templates
            var templates = new List<object>
            {
                new { TemplateId = 1, Name = "Sales Target", Category = "financial", Description = "Monthly sales target achievement" },
                new { TemplateId = 2, Name = "Customer Satisfaction", Category = "customer", Description = "Customer satisfaction score" },
                new { TemplateId = 3, Name = "Process Efficiency", Category = "process", Description = "Process completion time" },
                new { TemplateId = 4, Name = "Training Completion", Category = "learning", Description = "Training program completion rate" }
            };

            if (!string.IsNullOrEmpty(category))
            {
                //templates = templates.Where(t => t.Category == category).ToList();
            }

            return await Task.FromResult(templates);
        }

        public async Task<int> CreateKpiFromTemplateAsync(long templateId, long employeeId)
        {
            // This would create a KPI based on a predefined template
            // For now, create a basic KPI
            var kpi = new HrKpi
            {
                KpiCode = $"TEMPLATE_{templateId}_{employeeId}",
                KpiName = $"Template KPI {templateId}",
                Category = "general",
                MeasurementType = "percentage",
                TargetValue = 100,
                Weight = 1.0M,
                Frequency = "monthly",
                IsActive = true,
                Create_time = DateTime.Now
            };

            return await AddKpiAsync(kpi);
        }

        public async Task<int> BulkAssignKpisAsync(List<long> employeeIds, List<long> kpiIds)
        {
            var assignmentCount = 0;
            var currentYear = DateTime.Now.Year;

            foreach (var employeeId in employeeIds)
            {
                foreach (var kpiId in kpiIds)
                {
                    try
                    {
                        var assignment = new HrEmployeeKpi
                        {
                            EmployeeId = employeeId,
                            KpiId = kpiId,
                            Year = currentYear,
                            StartDate = DateTime.Now,
                            Status = "active"
                        };

                        await AssignKpiToEmployeeAsync(assignment);
                        assignmentCount++;
                    }
                    catch (Exception)
                    {
                        // Skip if assignment already exists or other validation errors
                        continue;
                    }
                }
            }

            return assignmentCount;
        }

        // Helper Methods
        private decimal CalculateKpiScore(decimal actualValue, decimal targetValue, HrKpi kpi)
        {
            if (targetValue <= 0)
                return 0;

            var achievementRatio = actualValue / targetValue;

            // Score based on KPI trend
            switch (kpi.Trend?.ToLower())
            {
                case "higher_better":
                    return Math.Min(achievementRatio * 100, 150); // Cap at 150%
                
                case "lower_better":
                    if (actualValue <= targetValue)
                        return 100 + ((targetValue - actualValue) / targetValue * 50); // Bonus for being under target
                    else
                        return Math.Max(100 - ((actualValue - targetValue) / targetValue * 100), 0);
                
                case "range":
                    if (actualValue >= kpi.MinValue && actualValue <= kpi.MaxValue)
                        return 100;
                    else
                        return Math.Max(50 - Math.Abs(actualValue - targetValue) / targetValue * 50, 0);
                
                default:
                    return achievementRatio * 100;
            }
        }
    }
}
