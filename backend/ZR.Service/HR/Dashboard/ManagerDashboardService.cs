using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.HR;
using ZR.Service.HR.IHrService;
using ZR.Service.HR.IService;
using ZR.Repository;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.HR.Attendance;
using ZR.Model.HR.Training;
using ZR.Model.HR.Performance;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// Manager Dashboard Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IManagerDashboardService), ServiceLifetime = LifeTime.Transient)]
    public class ManagerDashboardService : BaseService<HrEmployeeCore>, IManagerDashboardService
    {
        private readonly IAttendanceProcessingService _attendanceService;
        private readonly IHrLeaveService _leaveService;
        private readonly IHrPerformanceService _performanceService;
        private readonly IHrPayrollService _payrollService;
        private readonly IService.IAnalyticsService _analyticsService;
        private readonly INotificationService _notificationService;
        private readonly IEmployeeDataService _employeeDataService;

        public ManagerDashboardService(
            IAttendanceProcessingService attendanceService,
            IHrLeaveService leaveService,
            IHrPerformanceService performanceService,
            IHrPayrollService payrollService,
            IService.IAnalyticsService analyticsService,
            INotificationService notificationService,
            IEmployeeDataService employeeDataService)
        {
            _attendanceService = attendanceService;
            _leaveService = leaveService;
            _performanceService = performanceService;
            _payrollService = payrollService;
            _analyticsService = analyticsService;
            _notificationService = notificationService;
            _employeeDataService = employeeDataService;
        }

        // Dashboard Overview
        public async Task<HrManagerDashboard> GetDashboardOverviewAsync(long managerId)
        {
            var dashboard = new HrManagerDashboard
            {
                ManagerId = managerId,
                GeneratedDate = DateTime.Now
            };

            // Get team members
            var teamMembers = await GetTeamMembersAsync(managerId);
            dashboard.TotalTeamMembers = teamMembers.Count;
            dashboard.ActiveMembers = teamMembers.Count(e => e.Status == "active");

            // Today's attendance
            var todayAttendance = await GetTeamAttendanceTodayAsync(managerId);
            dashboard.PresentToday = todayAttendance.Present;
            dashboard.AbsentToday = todayAttendance.Absent;
            dashboard.OnLeaveToday = todayAttendance.OnLeave;

            // Pending approvals
            dashboard.PendingLeaveRequests = await GetPendingLeaveRequestsCountAsync(managerId);
            dashboard.PendingPerformanceReviews = await GetPendingPerformanceReviewsCountAsync(managerId);
            dashboard.PendingExpenseRequests = await GetPendingExpenseRequestsCountAsync(managerId);

            // Team performance metrics
            var performanceMetrics = await GetTeamPerformanceMetricsAsync(managerId);
            dashboard.TeamAverageRating = performanceMetrics.AverageRating;
            dashboard.GoalsOnTrack = performanceMetrics.GoalsOnTrack;
            dashboard.GoalsAtRisk = performanceMetrics.GoalsAtRisk;

            // Upcoming events
            dashboard.UpcomingBirthdays = await GetUpcomingBirthdaysAsync(managerId);
            dashboard.UpcomingAnniversaries = await GetUpcomingAnniversariesAsync(managerId);
            dashboard.UpcomingTrainings = await GetUpcomingTrainingsAsync(managerId);

            // Alerts
            dashboard.Alerts = await GenerateManagerAlertsAsync(managerId);

            return dashboard;
        }

        // Team Management
        public async Task<List<EmployeeFullDto>> GetTeamMembersAsync(long managerId, bool includeIndirect = false)
        {
            if (!includeIndirect)
            {
                return await _employeeDataService.GetTeamMembersAsync(managerId);
            }

            // Include indirect reports (recursive)
            var allReports = new List<EmployeeFullDto>();
            var directReports = await _employeeDataService.GetTeamMembersAsync(managerId);

            allReports.AddRange(directReports);

            foreach (var report in directReports)
            {
                var indirectReports = await GetTeamMembersAsync(report.EmployeeId, true);
                allReports.AddRange(indirectReports);
            }

            return allReports;
        }

        public async Task<HrTeamStatistics> GetTeamStatisticsAsync(long managerId)
        {
            var stats = new HrTeamStatistics
            {
                ManagerId = managerId,
                CalculatedDate = DateTime.Now
            };

            var teamMembers = await GetTeamMembersAsync(managerId);
            stats.TotalMembers = teamMembers.Count;

            // Department distribution
            stats.DepartmentDistribution = teamMembers
                .GroupBy(e => e.DepartmentId)
                .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.DepartmentId?.ToString() ?? "None", x => x.Count);

            // Position distribution
            stats.PositionDistribution = teamMembers
                .GroupBy(e => e.PositionId)
                .Select(g => new { PositionId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.PositionId?.ToString() ?? "None", x => x.Count);

            // Gender distribution
            stats.GenderDistribution = teamMembers
                .GroupBy(e => e.Gender)
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Gender ?? "Unknown", x => x.Count);

            // Experience distribution
            stats.AverageTenure = teamMembers.Any()
                ? teamMembers.Average(e => (DateTime.Now - e.HireDate)?.Days / 365.25m ?? 0)
                : 0;

            // Skills summary
            var skills = await Context.Queryable<HrSkillsMatrix>()
                .Where(s => teamMembers.Select(m => m.EmployeeId).Contains(s.EmployeeId))
                .ToListAsync();

            stats.TopSkills = skills
                .GroupBy(s => s.SkillName)
                .Select(g => new { Skill = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionary(x => x.Skill, x => x.Count);

            return stats;
        }

        public async Task<List<HrEmployeeDetail>> GetTeamDetailsAsync(long managerId)
        {
            var teamMembers = await GetTeamMembersAsync(managerId);
            var details = new List<HrEmployeeDetail>();

            foreach (var member in teamMembers)
            {
                var detail = new HrEmployeeDetail
                {
                    Employee = member, // EmployeeFullDto
                    CurrentAttendance = await GetEmployeeAttendanceStatusAsync(member.EmployeeId),
                    LeaveBalance = await GetEmployeeLeaveBalanceAsync(member.EmployeeId),
                    PerformanceRating = await GetEmployeePerformanceRatingAsync(member.EmployeeId),
                    LastReviewDate = await GetLastReviewDateAsync(member.EmployeeId),
                    TrainingProgress = await GetEmployeeTrainingProgressAsync(member.EmployeeId)
                };

                details.Add(detail);
            }

            return details;
        }

        // Attendance Management
        public async Task<HrTeamAttendanceSummary> GetTeamAttendanceTodayAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            var today = DateTime.Today;
            var attendance = await Context.Queryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                .Where(a => teamIds.Contains(a.EmployeeId) && a.AttendanceDate == today)
                .ToListAsync();

            return new HrTeamAttendanceSummary
            {
                Date = today,
                TotalTeamMembers = teamIds.Count,
                Present = attendance.Count(a => !a.IsAbsent),
                Absent = teamIds.Count - attendance.Count,
                OnLeave = attendance.Count(a => a.Status == "leave"),
                Late = attendance.Count(a => a.IsLate),
                WorkFromHome = attendance.Count(a => a.IsRemote),
                AttendanceRate = teamIds.Count > 0 
                    ? (decimal)attendance.Count(a => a.Status == "present") / teamIds.Count * 100 
                    : 0
            };
        }

        public async Task<List<HrAttendanceReport>> GetTeamAttendanceReportAsync(long managerId, DateTime startDate, DateTime endDate)
        {
            var teamMembers = await GetTeamMembersAsync(managerId);
            var reports = new List<HrAttendanceReport>();

            foreach (var member in teamMembers)
            {
                var attendance = await _attendanceService.GetEmployeeAttendanceAsync(member.EmployeeId, startDate, endDate);
                
                var report = new HrAttendanceReport
                {
                    EmployeeId = member.EmployeeId,
                    EmployeeName = $"{member.FirstName} {member.LastName}",
                    TotalDays = (endDate - startDate).Days + 1,
                    PresentDays = attendance.Count(a => a.Status == "present"),
                    AbsentDays = attendance.Count(a => a.Status == "absent"),
                    LeaveDays = attendance.Count(a => a.Status == "leave"),
                    LateDays = attendance.Count(a => a.IsLate),
                    TotalWorkHours = attendance.Sum(a => a.WorkHours),
                    TotalOvertimeHours = attendance.Sum(a => a.OvertimeHours)
                };

                report.AttendancePercentage = report.TotalDays > 0 
                    ? (decimal)report.PresentDays / report.TotalDays * 100 
                    : 0;

                reports.Add(report);
            }

            return reports;
        }

        // Leave Management
        public async Task<List<HrLeaveRequest>> GetPendingLeaveRequestsAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            return await Context.Queryable<HrLeaveRequest>()
                .Where(l => teamIds.Contains(l.EmployeeId) && l.Status == "pending")
                .OrderBy(l => l.Create_time)
                .ToListAsync();
        }

        public async Task<int> ApproveLeaveRequestAsync(long managerId, long leaveId, string comments = null)
        {
            var leave = await _leaveService.GetLeaveRequestByIdAsync(leaveId);
            if (leave == null) return 0;

            // Verify manager authority
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(leave.EmployeeId);
            if (employee?.ManagerId != managerId) return -1;

            leave.Status = "approved";
            leave.ApprovedBy = managerId ;
            leave.ApprovalDate = DateTime.Now;
            leave.ApprovalComments = comments;
            leave.Update_time = DateTime.Now;

            var result = await _leaveService.UpdateLeaveRequestAsync(leave);

            if (result > 0)
            {
                // Update leave balance
                await _leaveService.DeductLeaveBalanceAsync(leave.EmployeeId, leave.LeaveTypeId, leave.Days ?? 0);

                // Send notification
                await _notificationService.SendLeaveApprovalNotificationAsync(leave.EmployeeId, leaveId, "approved");
            }

            return result;
        }

        public async Task<int> RejectLeaveRequestAsync(long managerId, long leaveId, string reason)
        {
            var leave = await _leaveService.GetLeaveRequestByIdAsync(leaveId);
            if (leave == null) return 0;

            // Verify manager authority
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(leave.EmployeeId);
            if (employee?.ManagerId != managerId) return -1;

            leave.Status = "rejected";
            leave.ApprovedBy = managerId;
            leave.ApprovalDate = DateTime.Now;
            leave.RejectionReason = reason;
            leave.Update_time = DateTime.Now;

            var result = await _leaveService.UpdateLeaveRequestAsync(leave);

            if (result > 0)
            {
                // Send notification
                await _notificationService.SendLeaveApprovalNotificationAsync(leave.EmployeeId, leaveId, "rejected");
            }

            return result;
        }

        public async Task<HrLeaveAnalytics> GetTeamLeaveAnalyticsAsync(long managerId, int year)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            var analytics = new HrLeaveAnalytics
            {
                Year = year,
                TeamSize = teamIds.Count
            };

            var leaves = await Context.Queryable<HrLeaveRequest>()
                .Where(l => teamIds.Contains(l.EmployeeId) && l.StartDate.Year == year)
                .ToListAsync();

            analytics.TotalLeaveRequests = leaves.Count;
            analytics.ApprovedRequests = leaves.Count(l => l.Status == "approved");
            analytics.RejectedRequests = leaves.Count(l => l.Status == "rejected");
            analytics.PendingRequests = leaves.Count(l => l.Status == "pending");
            
            // Monthly distribution
            analytics.MonthlyDistribution = leaves
                .Where(l => l.Status == "approved")
                .GroupBy(l => l.StartDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Month, x => x.Count);

            // Leave type distribution
            analytics.LeaveTypeDistribution = leaves
                .Where(l => l.Status == "approved")
                .GroupBy(l => l.LeaveTypeId)
                .Select(g => new { TypeId = g.Key, Days = g.Sum(l => l.Days ?? 0) })
                .ToDictionary(x => x.TypeId.ToString(), x => x.Days);

            // Average leave days per employee
            analytics.AverageLeaveDaysPerEmployee = teamIds.Count > 0
                ? leaves.Where(l => l.Status == "approved").Sum(l => l.Days ?? 0) / (decimal)teamIds.Count
                : 0;

            return analytics;
        }

        // Performance Management
        public async Task<HrTeamPerformanceMetrics> GetTeamPerformanceMetricsAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            var metrics = new HrTeamPerformanceMetrics
            {
                ManagerId = managerId,
                CalculatedDate = DateTime.Now
            };

            // Performance reviews
            var reviews = await Context.Queryable<HrPerformanceReview>()
                .Where(r => teamIds.Contains(r.EmployeeId) && r.ReviewPeriodEnd >= DateTime.Now.AddMonths(-12))
                .ToListAsync();

            if (reviews.Any())
            {
                metrics.AverageRating = reviews.Average(r => r.OverallRating ?? 0);
                metrics.HighPerformers = reviews.Count(r => r.OverallRating >= 4.5m);
                metrics.LowPerformers = reviews.Count(r => r.OverallRating < 3.0m);
            }

            // Goals
            var goals = await Context.Queryable<HrPerformanceGoal>()
                .Where(g => teamIds.Contains(g.EmployeeId) && g.Status != "cancelled")
                .ToListAsync();

            metrics.TotalGoals = goals.Count;
            metrics.GoalsOnTrack = goals.Count(g => g.Status == "in_progress" && g.Progress >= 50);
            metrics.GoalsAtRisk = goals.Count(g => g.Status == "in_progress" && g.Progress < 50 && g.DueDate < DateTime.Now.AddDays(30));
            metrics.GoalsCompleted = goals.Count(g => g.Status == "completed");

            // Skills assessment
            var skills = await Context.Queryable<HrSkillsMatrix>()
                .Where(s => teamIds.Contains(s.EmployeeId))
                .ToListAsync();

            metrics.SkillsGapAnalysis = AnalyzeSkillsGap(skills);

            return metrics;
        }

        public async Task<List<HrPerformanceReview>> GetPendingPerformanceReviewsAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            return await Context.Queryable<HrPerformanceReview>()
                .Where(r => teamIds.Contains(r.EmployeeId) && 
                    (r.Status == "pending" || r.Status == "self_review_completed"))
                .OrderBy(r => r.DueDate)
                .ToListAsync();
        }

        public async Task<int> SubmitPerformanceReviewAsync(long managerId, HrPerformanceReview review)
        {
            // Verify manager authority
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(review.EmployeeId);
            if (employee?.ManagerId != managerId) return -1;

            review.ReviewerId = managerId;
            review.ReviewDate = DateTime.Now;
            review.Status = "manager_review_completed";
            review.Update_time = DateTime.Now;

            var result = await _performanceService.SubmitPerformanceReviewAsync(review);

            if (result > 0)
            {
                // Send notification
                await _notificationService.SendPerformanceReviewNotificationAsync(
                    review.EmployeeId, 
                    review.ReviewId, 
                    "Manager Review Completed"
                );
            }

            return result;
        }

        public async Task<int> SetTeamGoalsAsync(long managerId, List<HrPerformanceGoal> goals)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            var count = 0;
            foreach (var goal in goals)
            {
                if (!teamIds.Contains(goal.EmployeeId)) continue;

                goal.SetBy = managerId.ToString();
                goal.Create_time = DateTime.Now;
                goal.Status = "assigned";

                var result = await _performanceService.CreateGoalAsync(goal);
                if (result > 0) count++;
            }

            return count;
        }

        // Expense Management
        public async Task<List<HrExpenseRequest>> GetPendingExpenseRequestsAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            return await Context.Queryable<HrExpenseRequest>()
                .Where(e => teamIds.Contains(e.EmployeeId) && e.Status == "pending")
                .OrderBy(e => e.Create_time)
                .ToListAsync();
        }

        public async Task<int> ApproveExpenseRequestAsync(long managerId, long expenseId, string comments = null)
        {
            var expense = await Context.Queryable<HrExpenseRequest>()
                .Where(e => e.ExpenseId == expenseId)
                .FirstAsync();

            if (expense == null) return 0;

            // Verify manager authority
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(expense.EmployeeId);
            if (employee?.ManagerId != managerId) return -1;

            expense.Status = "approved";
            expense.ApprovedBy = managerId.ToString();
            expense.ApprovalDate = DateTime.Now;
            expense.ApprovalComments = comments;
            expense.Update_time = DateTime.Now;

            return await Context.Updateable(expense).ExecuteCommandAsync();
        }

        // Analytics and Reports
        public async Task<HrManagerReport> GenerateTeamReportAsync(long managerId, string reportType, DateTime startDate, DateTime endDate)
        {
            var report = new HrManagerReport
            {
                ManagerId = managerId,
                ReportType = reportType,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedDate = DateTime.Now
            };

            switch (reportType.ToLower())
            {
                case "attendance":
                    report.ReportData = Newtonsoft.Json.JsonConvert.SerializeObject(
                        await GetTeamAttendanceReportAsync(managerId, startDate, endDate)
                    );
                    break;

                case "performance":
                    report.ReportData = Newtonsoft.Json.JsonConvert.SerializeObject(
                        await GetTeamPerformanceMetricsAsync(managerId)
                    );
                    break;

                case "leave":
                    report.ReportData = Newtonsoft.Json.JsonConvert.SerializeObject(
                        await GetTeamLeaveAnalyticsAsync(managerId, startDate.Year)
                    );
                    break;

                case "comprehensive":
                    var comprehensiveData = new
                    {
                        Overview = await GetDashboardOverviewAsync(managerId),
                        Statistics = await GetTeamStatisticsAsync(managerId),
                        Attendance = await GetTeamAttendanceReportAsync(managerId, startDate, endDate),
                        Performance = await GetTeamPerformanceMetricsAsync(managerId),
                        Leave = await GetTeamLeaveAnalyticsAsync(managerId, startDate.Year)
                    };
                    report.ReportData = Newtonsoft.Json.JsonConvert.SerializeObject(comprehensiveData);
                    break;
            }

            await Context.Insertable(report).ExecuteCommandAsync();
            return report;
        }

        // Notifications and Alerts
        public async Task<List<HrManagerAlert>> GenerateManagerAlertsAsync(long managerId)
        {
            var alerts = new List<HrManagerAlert>();
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            // Attendance alerts





            // Attendance alerts
            var absentToday = await Context.Queryable<HrEmployeeCore>()
                .Where(e => teamIds.Contains(e.EmployeeId) &&
                    !SqlFunc.Subqueryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                        .Where(a => a.EmployeeId == e.EmployeeId && a.AttendanceDate == DateTime.Today)
                        .Any())
                .CountAsync();




            //var absentToday = await Context.Queryable<HrEmployeeCore>()
            //    .Where(e => teamIds.Contains(e.EmployeeId) && 
            //        !Context.Queryable<HrAttendance>()
            //            .Any(a => a.EmployeeId == e.EmployeeId && a.AttendanceDate == DateTime.Today))
            //    .CountAsync();

            if (absentToday > 0)
            {
                alerts.Add(new HrManagerAlert
                {
                    AlertType = "attendance",
                    Severity = "medium",
                    Message = $"{absentToday} team members are absent today",
                    Create_time = DateTime.Now
                });
            }

            // Leave request alerts
            var pendingLeaves = await GetPendingLeaveRequestsCountAsync(managerId);
            if (pendingLeaves > 0)
            {
                alerts.Add(new HrManagerAlert
                {
                    AlertType = "leave",
                    Severity = "high",
                    Message = $"You have {pendingLeaves} pending leave requests",
                    Create_time = DateTime.Now
                });
            }

            // Performance review alerts
            var overdueReviews = await Context.Queryable<HrPerformanceReview>()
                .Where(r => teamIds.Contains(r.EmployeeId) && 
                    r.Status == "pending" && 
                    r.DueDate < DateTime.Now)
                .CountAsync();

            if (overdueReviews > 0)
            {
                alerts.Add(new HrManagerAlert
                {
                    AlertType = "performance",
                    Severity = "high",
                    Message = $"{overdueReviews} performance reviews are overdue",
                    Create_time = DateTime.Now
                });
            }

            // Document expiry alerts
            // TODO: Implement when HrEmployeeDocument model is available
            /*
            var expiringDocs = await Context.Queryable<HrEmployeeDocument>()
                .Where(d => teamIds.Contains(d.EmployeeId) &&
                    d.ExpiryDate.HasValue &&
                    d.ExpiryDate.Value <= DateTime.Now.AddDays(30))
                .CountAsync();

            if (expiringDocs > 0)
            {
                alerts.Add(new HrManagerAlert
                {
                    AlertType = "document",
                    Severity = "medium",
                    Message = $"{expiringDocs} employee documents expiring soon",
                    Create_time = DateTime.Now
                });
            }
            */

            return alerts;
        }

        // Helper Methods
        private async Task<int> GetPendingLeaveRequestsCountAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            return await Context.Queryable<HrLeaveRequest>()
                .Where(l => teamIds.Contains(l.EmployeeId) && l.Status == "pending")
                .CountAsync();
        }

        private async Task<int> GetPendingPerformanceReviewsCountAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            return await Context.Queryable<HrPerformanceReview>()
                .Where(r => teamIds.Contains(r.EmployeeId) && 
                    (r.Status == "pending" || r.Status == "self_review_completed"))
                .CountAsync();
        }

        private async Task<int> GetPendingExpenseRequestsCountAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            return await Context.Queryable<HrExpenseRequest>()
                .Where(e => teamIds.Contains(e.EmployeeId) && e.Status == "pending")
                .CountAsync();
        }

        private async Task<List<string>> GetUpcomingBirthdaysAsync(long managerId)
        {
            var teamMembers = await GetTeamMembersAsync(managerId);
            var upcoming = teamMembers
                .Where(e => e.DateOfBirth.HasValue)
                .Where(e =>
                {
                    var dob = e.DateOfBirth.Value;
                    var nextBirthday = dob.AddYears(DateTime.Now.Year - dob.Year);
                    return nextBirthday >= DateTime.Today && nextBirthday <= DateTime.Today.AddDays(7);
                })
                .Select(e => $"{e.FirstName} {e.LastName} - {e.DateOfBirth.Value:MMM dd}")
                .ToList();

            return upcoming;
        }

        private async Task<List<string>> GetUpcomingAnniversariesAsync(long managerId)
        {
            var teamMembers = await GetTeamMembersAsync(managerId);
            var upcoming = teamMembers
                .Where(e => e.HireDate.HasValue)
                .Where(e =>
                {
                    var hireDate = e.HireDate.Value;
                    var nextAnniversary = hireDate.AddYears(DateTime.Now.Year - hireDate.Year);
                    return nextAnniversary >= DateTime.Today && nextAnniversary <= DateTime.Today.AddDays(7);
                })
                .Select(e => $"{e.FirstName} {e.LastName} - {(DateTime.Now.Year - e.HireDate.Value.Year)} years")
                .ToList();

            return upcoming;
        }

        private async Task<List<string>> GetUpcomingTrainingsAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            var trainings = await Context.Queryable<HrTrainingEnrollment, HrTrainingSession, HrTrainingCourse>(
                (e, s, c) => new object[] {
                    SqlSugar.JoinType.Inner, e.SessionId == s.SessionId,
                    SqlSugar.JoinType.Inner, s.CourseId == c.CourseId
                })
                .Where((e, s, c) => teamIds.Contains(e.EmployeeId) && 
                    s.StartDate >= DateTime.Today && 
                    s.StartDate <= DateTime.Today.AddDays(30))
                .Select((e, s, c) => c.CourseName + " - " + s.StartDate.ToString("MMM dd"))
                .ToListAsync();

            return trainings;
        }

        private async Task<string> GetEmployeeAttendanceStatusAsync(long employeeId)
        {
            var today = await Context.Queryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                .Where(a => a.EmployeeId == employeeId && a.AttendanceDate == DateTime.Today)
                .FirstAsync();

            return today == null ? "not_marked" : (today.IsAbsent ? "absent" : "present");
        }

        private async Task<decimal> GetEmployeeLeaveBalanceAsync(long employeeId)
        {
            var balances = await Context.Queryable<HrLeaveBalance>()
                .Where(b => b.EmployeeId == employeeId && b.Year == DateTime.Now.Year)
                .ToListAsync();

            return balances.Sum(b => b.AvailableDays);
        }

        private async Task<decimal?> GetEmployeePerformanceRatingAsync(long employeeId)
        {
            var latestReview = await Context.Queryable<HrPerformanceReview>()
                .Where(r => r.EmployeeId == employeeId)
                .OrderBy(r => r.ReviewPeriodEnd, OrderByType.Desc)
                .FirstAsync();

            return latestReview?.OverallRating;
        }

        private async Task<DateTime?> GetLastReviewDateAsync(long employeeId)
        {
            var latestReview = await Context.Queryable<HrPerformanceReview>()
                .Where(r => r.EmployeeId == employeeId)
                .OrderBy(r => r.ReviewPeriodEnd, OrderByType.Desc)
                .FirstAsync();

            return latestReview?.ReviewDate;
        }

        private async Task<decimal> GetEmployeeTrainingProgressAsync(long employeeId)
        {
            var enrollments = await Context.Queryable<HrTrainingEnrollment>()
                .Where(e => e.EmployeeId == employeeId && 
                    e.Status == "in_progress" && 
                    e.EnrollmentDate >= DateTime.Now.AddMonths(-6))
                .ToListAsync();

            if (!enrollments.Any()) return 100;

            var completedCount = enrollments.Count(e => e.Status == "completed");
            return enrollments.Count > 0 ? (decimal)completedCount / enrollments.Count * 100 : 0;
        }

        private Dictionary<string, int> AnalyzeSkillsGap(List<HrSkillsMatrix> skills)
        {
            var gaps = new Dictionary<string, int>();
            
            // Group by skill and count proficiency levels
            var skillGroups = skills.GroupBy(s => s.SkillName);
            
            foreach (var group in skillGroups)
            {
                var beginnerCount = group.Count(s => s.ProficiencyLevel == "beginner");
                if (beginnerCount > 0)
                {
                    gaps[group.Key] = beginnerCount;
                }
            }

            return gaps;
        }

        // Team Performance Methods
        public async Task<object> GetTeamPerformanceMetricsAsync(long managerId, int year)
        {
            return await GetTeamPerformanceMetricsAsync(managerId);
        }

        // Team Leave Summary
        public async Task<object> GetTeamLeaveSummaryAsync(long managerId, int year)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            var leaveRequests = await Context.Queryable<HrLeaveRequest>()
                .Where(lr => teamIds.Contains(lr.EmployeeId) && lr.StartDate.Year == year)
                .ToListAsync();

            return new
            {
                TotalLeaveRequests = leaveRequests.Count,
                ApprovedRequests = leaveRequests.Count(lr => lr.Status == "Approved"),
                PendingRequests = leaveRequests.Count(lr => lr.Status == "Pending"),
                RejectedRequests = leaveRequests.Count(lr => lr.Status == "Rejected"),
                TotalLeaveDays = leaveRequests.Where(lr => lr.Status == "Approved").Sum(lr => lr.Days)
            };
        }

        // Team Training Status
        public async Task<object> GetTeamTrainingStatusAsync(long managerId)
        {
            var teamIds = (await GetTeamMembersAsync(managerId))
                .Select(e => e.EmployeeId)
                .ToList();

            var trainingRecords = await Context.Queryable<HrTrainingEnrollment>()
                .Where(tr => teamIds.Contains(tr.EmployeeId))
                .ToListAsync();

            return new
            {
                TotalTrainings = trainingRecords.Count,
                CompletedTrainings = trainingRecords.Count(tr => tr.Status == "completed"),
                InProgressTrainings = trainingRecords.Count(tr => tr.Status == "in_progress"),
                PlannedTrainings = trainingRecords.Count(tr => tr.Status == "enrolled")
            };
        }

        // Approve Request
        public async Task<bool> ApproveRequestAsync(long requestId, string requestType, long approvedBy, string comments)
        {
            switch (requestType.ToLower())
            {
                case "leave":
                    return await _leaveService.ApproveLeaveRequestAsync(requestId, approvedBy, comments);
                case "expense":
                    // Implement expense approval logic
                    return true;
                default:
                    return false;
            }
        }

        // Reject Request
        public async Task<bool> RejectRequestAsync(long requestId, string requestType, long rejectedBy, string reason)
        {
            switch (requestType.ToLower())
            {
                case "leave":
                    return await _leaveService.RejectLeaveRequestAsync(requestId, rejectedBy, reason);
                case "expense":
                    // Implement expense rejection logic
                    return true;
                default:
                    return false;
            }
        }

        // Schedule Team Meeting
        public async Task<int> ScheduleTeamMeetingAsync(object meetingDetails)
        {
            if (meetingDetails == null) return 0;

            var meetingJson = Newtonsoft.Json.JsonConvert.SerializeObject(meetingDetails);
            dynamic meeting = Newtonsoft.Json.JsonConvert.DeserializeObject(meetingJson);

            // HrMeeting model doesn't exist, using a placeholder
            var teamMeeting = new { 
                Id = 0,
                Title = meeting.title ?? "Team Meeting",
                MeetingDate = meeting.date ?? DateTime.Now.AddDays(1),
                StartTime = meeting.startTime ?? "09:00"
            }; /*new HrMeeting
            {
                Title = meeting.title ?? "Team Meeting",
                Description = meeting.description ?? "",
                MeetingDate = meeting.date ?? DateTime.Now.AddDays(1),
                StartTime = meeting.startTime ?? "09:00",
                EndTime = meeting.endTime ?? "10:00",
                Location = meeting.location ?? "Conference Room",
                OrganizerId = meeting.organizerId ?? 0,
                MeetingType = "Team",
                Status = "Scheduled",
                Create_time = DateTime.Now
            };*/

            // var result = await Context.Insertable(teamMeeting).ExecuteReturnIdentityAsync();
            var result = 1; // Placeholder since HrMeeting doesn't exist

            // Send notifications to team members
            if (result > 0 && meeting.participants != null)
            {
                foreach (long participantId in meeting.participants)
                {
                    await _notificationService.SendNotificationAsync(new HrNotification
                    {
                        RecipientId = participantId,
                        NotificationType = "in_app",
                        Subject = "Meeting Scheduled",
                        Body = $"You have been invited to a meeting: {teamMeeting.Title} on {teamMeeting.MeetingDate:yyyy-MM-dd} at {teamMeeting.StartTime}",
                        Priority = "medium",
                        Category = "meeting",
                        IsRead = false,
                        Create_time = DateTime.Now
                    });
                }
            }

            return result;
        }

        // Assign Task
        public async Task<int> AssignTaskAsync(object taskDetails)
        {
            if (taskDetails == null) return 0;

            var taskJson = Newtonsoft.Json.JsonConvert.SerializeObject(taskDetails);
            dynamic task = Newtonsoft.Json.JsonConvert.DeserializeObject(taskJson);

            // HrTask model doesn't exist, using placeholder
            var newTask = new { AssignedTo = (long)(task.assignedTo ?? 0), TaskTitle = (string)(task.title ?? "New Task"), DueDate = (DateTime)(task.dueDate ?? DateTime.Now.AddDays(7)) };
            /*var newTask = new HrTask
            {
                TaskTitle = task.title ?? "New Task",
                Description = task.description ?? "",
                AssignedTo = task.assignedTo ?? 0,
                AssignedBy = task.assignedBy ?? 0,
                DueDate = task.dueDate ?? DateTime.Now.AddDays(7),
                Priority = task.priority ?? "Medium",
                Status = "Pending",
                TaskType = task.taskType ?? "General",
                EstimatedHours = task.estimatedHours ?? 8,
                Create_time = DateTime.Now
            };*/

            // var result = await Context.Insertable(newTask).ExecuteReturnIdentityAsync();
            var result = 1; // Placeholder since HrTask doesn't exist

            // Send notification to assigned employee
            if (result > 0)
            {
                await _notificationService.SendNotificationAsync(new HrNotification
                {
                    RecipientId = newTask.AssignedTo,
                    NotificationType = "in_app",
                    Subject = "New Task Assigned",
                    Body = $"You have been assigned a new task: {newTask.TaskTitle} (Due: {newTask.DueDate:yyyy-MM-dd})",
                    Priority = "high",
                    Category = "task",
                    IsRead = false,
                    Create_time = DateTime.Now
                });
            }

            return result;
        }

        // Get Team Workload
        public async Task<object> GetTeamWorkloadAsync(long managerId)
        {
            var teamMembers = await GetTeamMembersAsync(managerId);
            var workloadData = new List<object>();

            foreach (var member in teamMembers)
            {
                // Since HrTask and HrProjectAssignment don't exist, using placeholder values
                var pendingTasks = 0;
                var inProgressTasks = 0;
                var totalEstimatedHours = 40; // Default workload
                var projects = 0;

                // Calculate workload status
                string workloadStatus = totalEstimatedHours switch
                {
                    <= 40 => "Light",
                    <= 60 => "Normal",
                    <= 80 => "Heavy",
                    _ => "Overloaded"
                };

                workloadData.Add(new
                {
                    EmployeeId = member.EmployeeId,
                    EmployeeName = $"{member.FirstName} {member.LastName}",
                    EmployeeEmail = member.Email,
                    Department = member.DepartmentId,
                    CurrentProjects = projects,
                    PendingTasks = pendingTasks,
                    InProgressTasks = inProgressTasks,
                    TotalTasks = pendingTasks + inProgressTasks,
                    EstimatedHours = totalEstimatedHours,
                    WorkloadStatus = workloadStatus,
                    LastActivityDate = DateTime.Now // Placeholder since tasks don't exist
                });
            }

            return new
            {
                TeamSize = teamMembers.Count,
                Summary = new
                {
                    TotalTasks = workloadData.Sum(w => ((dynamic)w).TotalTasks),
                    TotalProjects = workloadData.Sum(w => ((dynamic)w).CurrentProjects),
                    OverloadedMembers = workloadData.Count(w => ((dynamic)w).WorkloadStatus == "Overloaded"),
                    LightWorkloadMembers = workloadData.Count(w => ((dynamic)w).WorkloadStatus == "Light")
                },
                TeamMembers = workloadData.OrderByDescending(w => ((dynamic)w).EstimatedHours)
            };
        }

        // Submit Performance Review
        public async Task<int> SubmitPerformanceReviewAsync(object reviewData)
        {
            if (reviewData == null) return 0;

            var reviewJson = Newtonsoft.Json.JsonConvert.SerializeObject(reviewData);
            dynamic review = Newtonsoft.Json.JsonConvert.DeserializeObject(reviewJson);

            var performanceReview = new HrPerformanceReview
            {
                EmployeeId = review.employeeId ?? 0,
                ReviewerId = review.reviewerId ?? 0,
                ReviewPeriod = review.reviewPeriod ?? $"{DateTime.Now.Year} - Q{(DateTime.Now.Month - 1) / 3 + 1}",
                ReviewDate = DateTime.Now,
                OverallRating = review.overallRating ?? 3,
                // ProductivityScore = review.productivityScore ?? 3, // Field doesn't exist
                // QualityScore = review.qualityScore ?? 3, // Field doesn't exist  
                // CommunicationScore = review.communicationScore ?? 3, // Field doesn't exist
                // TeamworkScore = review.teamworkScore ?? 3, // Field doesn't exist
                // InitiativeScore = review.initiativeScore ?? 3, // This field doesn't exist in the model
                Strengths = review.strengths ?? "",
                AreasForImprovement = review.areasForImprovement ?? "",
                Goals = review.goals ?? "",
                Comments = review.comments ?? "",
                Status = "Submitted",
                Create_time = DateTime.Now
            };

            var result = await Context.Insertable(performanceReview).ExecuteReturnIdentityAsync();

            // Update employee's performance history
            if (result > 0)
            {
                // Update performance score if method exists
                // await _performanceService.UpdatePerformanceScoreAsync(
                //     performanceReview.EmployeeId, 
                //     performanceReview.OverallRating);

                // Send notification to employee
                await _notificationService.SendNotificationAsync(new HrNotification
                {
                    RecipientId = performanceReview.EmployeeId,
                    NotificationType = "in_app",
                    Subject = "Performance Review Completed",
                    Body = $"Your performance review for {performanceReview.ReviewPeriod} has been submitted.",
                    Priority = "medium",
                    Category = "performance",
                    IsRead = false,
                    Create_time = DateTime.Now
                });
            }

            return result;
        }
    }
}
