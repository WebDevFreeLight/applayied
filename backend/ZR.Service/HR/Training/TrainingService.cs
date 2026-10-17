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
using ZR.Model.HR.Training;
using ZR.Repository;
using ZR.Service.HR.IService;
using ZR.ServiceCore;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// Training Service Implementation
    /// Provides comprehensive training management including courses, sessions, enrollments, progress tracking, and analytics
    /// </summary>
    [AppService(ServiceType = typeof(ITrainingService), ServiceLifetime = LifeTime.Transient)]
    public class TrainingService : BaseService<HrTrainingCourse>, ITrainingService
    {
        private readonly IEmployeeDataService _employeeDataService;

        public TrainingService(IEmployeeDataService employeeDataService)
        {
            _employeeDataService = employeeDataService;
        }
        // Training Course Management
        public async Task<PagedInfo<HrTrainingCourse>> GetCoursesAsync(Expression<Func<HrTrainingCourse, bool>> predicate, PagerInfo pager)
        {
            var query = Context.Queryable<HrTrainingCourse>()
                .Where(predicate)
                .OrderBy(c => c.Create_time, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }

        public async Task<List<HrTrainingCourse>> GetActiveCoursesAsync()
        {
            return await Context.Queryable<HrTrainingCourse>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Category)
                .OrderBy(c => c.CourseName)
                .ToListAsync();
        }

        public async Task<HrTrainingCourse> GetCourseByIdAsync(long courseId)
        {
            return await Context.Queryable<HrTrainingCourse>()
                .FirstAsync(c => c.CourseId == courseId);
        }

        public async Task<int> CreateCourseAsync(HrTrainingCourse course)
        {
            // Validate course data
            if (string.IsNullOrEmpty(course.CourseName))
                throw new ArgumentException("Course name is required");

            if (string.IsNullOrEmpty(course.CourseCode))
                throw new ArgumentException("Course code is required");

            // Check for duplicate course codes
            var exists = await Context.Queryable<HrTrainingCourse>()
                .AnyAsync(c => c.CourseCode == course.CourseCode);
            if (exists)
                throw new InvalidOperationException("Course code already exists");

            course.IsActive = true;
            course.Create_time = DateTime.Now;

            return await Context.Insertable(course).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateCourseAsync(HrTrainingCourse course)
        {
            // Validate course data
            if (string.IsNullOrEmpty(course.CourseName))
                throw new ArgumentException("Course name is required");

            // Check for duplicate course codes (excluding current course)
            var exists = await Context.Queryable<HrTrainingCourse>()
                .AnyAsync(c => c.CourseCode == course.CourseCode && c.CourseId != course.CourseId);
            if (exists)
                throw new InvalidOperationException("Course code already exists");

            course.Update_time = DateTime.Now;
            return await Context.Updateable(course)
                .IgnoreColumns(c => new { c.Create_time, c.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> DeleteCourseAsync(long courseId)
        {
            // Check if course has active sessions
            var hasActiveSessions = await Context.Queryable<HrTrainingSession>()
                .AnyAsync(s => s.CourseId == courseId && s.Status != "cancelled" && s.Status != "completed");
            if (hasActiveSessions)
                throw new InvalidOperationException("Cannot delete course with active sessions");

            int v = await Context.Updateable<HrTrainingCourse>()
                .SetColumns(c => new HrTrainingCourse
                {
                    IsActive = false,
                    Update_time = DateTime.Now
                })
                .Where(c => c.CourseId == courseId)
                .ExecuteCommandAsync();
            return v;
        }

        public async Task<List<HrTrainingCourse>> GetCoursesByCategoryAsync(string category)
        {
            return await Context.Queryable<HrTrainingCourse>()
                .Where(c => c.IsActive && c.Category == category)
                .OrderBy(c => c.CourseName)
                .ToListAsync();
        }

        public async Task<List<HrTrainingCourse>> GetMandatoryCoursesAsync()
        {
            return await Context.Queryable<HrTrainingCourse>()
                .Where(c => c.IsActive && c.IsMandatory)
                .OrderBy(c => c.CourseName)
                .ToListAsync();
        }

        // Training Session Management
        public async Task<PagedInfo<HrTrainingSession>> GetSessionsAsync(Expression<Func<HrTrainingSession, bool>> predicate, PagerInfo pager)
        {
            var query = Context.Queryable<HrTrainingSession>()
                .LeftJoin<HrTrainingCourse>((s, c) => s.CourseId == c.CourseId)
                .Where(predicate)
                .Select((s, c) => new HrTrainingSession
                {
                    SessionId = s.SessionId,
                    CourseId = s.CourseId,
                    SessionCode = s.SessionCode,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Location = s.Location,
                    DeliveryMode = s.DeliveryMode,
                    EnrolledCount = s.EnrolledCount,
                    CompletedCount = s.CompletedCount,
                    Status = s.Status,
                    ActualCost = s.ActualCost,
                    Materials = s.Materials
                })
                .OrderBy(s => s.StartDate, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }

        public async Task<List<HrTrainingSession>> GetUpcomingSessionsAsync()
        {
            return await Context.Queryable<HrTrainingSession>()
                .LeftJoin<HrTrainingCourse>((s, c) => s.CourseId == c.CourseId)
                .Where((s, c) => s.StartDate >= DateTime.Now && s.Status == "scheduled" && c.IsActive)
                .OrderBy(s => s.StartDate)
                .Select(s => s)
                .ToListAsync();
        }

        public async Task<HrTrainingSession> GetSessionByIdAsync(long sessionId)
        {
            return await Context.Queryable<HrTrainingSession>()
                .FirstAsync(s => s.SessionId == sessionId);
        }

        public async Task<int> ScheduleSessionAsync(HrTrainingSession session)
        {
            // Validate session data
            if (session.CourseId <= 0)
                throw new ArgumentException("Course ID is required");

            if (session.StartDate <= DateTime.Now)
                throw new ArgumentException("Start date must be in the future");

            if (session.EndDate <= session.StartDate)
                throw new ArgumentException("End date must be after start date");

            // Generate session code if not provided
            if (string.IsNullOrEmpty(session.SessionCode))
            {
                var course = await GetCourseByIdAsync(session.CourseId);
                var sessionCount = await Context.Queryable<HrTrainingSession>()
                    .CountAsync(s => s.CourseId == session.CourseId);
                session.SessionCode = $"{course.CourseCode}-S{sessionCount + 1:D3}";
            }

            session.Status = "scheduled";
            session.EnrolledCount = 0;
            session.CompletedCount = 0;
            session.Create_time = DateTime.Now;

            return await Context.Insertable(session).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateSessionAsync(HrTrainingSession session)
        {
            session.Update_time = DateTime.Now;
            return await Context.Updateable(session)
                .IgnoreColumns(s => new { s.Create_time, s.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> CancelSessionAsync(long sessionId, string reason)
        {
            // Check if session can be cancelled
            var session = await GetSessionByIdAsync(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found");

            if (session.Status == "completed")
                throw new InvalidOperationException("Cannot cancel completed session");

            // Cancel all enrollments
            await Context.Updateable<HrTrainingEnrollment>()
                .SetColumns(e => new HrTrainingEnrollment
                {
                    Status = "cancelled",
                    Update_time = DateTime.Now
                })
                .Where(e => e.SessionId == sessionId && e.Status == "enrolled")
                .ExecuteCommandAsync();

            return await Context.Updateable<HrTrainingSession>()
                .SetColumns(s => new HrTrainingSession
                {
                    Status = "cancelled",
                    Update_time = DateTime.Now
                })
                .Where(s => s.SessionId == sessionId)
                .ExecuteCommandAsync();
        }

        public async Task<List<HrTrainingSession>> GetSessionsByCourseAsync(long courseId)
        {
            return await Context.Queryable<HrTrainingSession>()
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.StartDate, OrderByType.Desc)
                .ToListAsync();
        }

        // Training Enrollment
        public async Task<List<HrTrainingEnrollment>> GetEnrollmentsAsync(long sessionId)
        {
            return await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrEmployeeCore>((e, emp) => e.EmployeeId == emp.EmployeeId)
                .Where((e, emp) => e.SessionId == sessionId)
                .Select((e, emp) => new HrTrainingEnrollment
                {
                    EnrollmentId = e.EnrollmentId,
                    SessionId = e.SessionId,
                    EmployeeId = e.EmployeeId,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status,
                    Score = e.Score,
                    AttendancePercentage = e.AttendancePercentage,
                    CompletionDate = e.CompletionDate,
                    CertificateNumber = e.CertificateNumber,
                    CertificateIssueDate = e.CertificateIssueDate,
                    CertificateExpiryDate = e.CertificateExpiryDate,
                    Feedback = e.Feedback,
                    Rating = e.Rating,
                    ApprovedBy = e.ApprovedBy,
                    ApprovalDate = e.ApprovalDate
                })
                .OrderBy(e => e.EnrollmentDate, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<List<HrTrainingEnrollment>> GetEmployeeEnrollmentsAsync(long employeeId)
        {
            return await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .LeftJoin<HrTrainingCourse>((e, s, c) => s.CourseId == c.CourseId)
                .Where((e, s, c) => e.EmployeeId == employeeId)
                .Select((e, s, c) => e)
                .OrderBy(e => e.EnrollmentDate, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<int> EnrollEmployeeAsync(HrTrainingEnrollment enrollment)
        {
            // Validate enrollment data
            if (enrollment.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (enrollment.SessionId <= 0)
                throw new ArgumentException("Session ID is required");

            // Check if employee is already enrolled
            var existingEnrollment = await Context.Queryable<HrTrainingEnrollment>()
                .AnyAsync(e => e.EmployeeId == enrollment.EmployeeId 
                    && e.SessionId == enrollment.SessionId 
                    && e.Status != "cancelled");

            if (existingEnrollment)
                throw new InvalidOperationException("Employee is already enrolled in this session");

            // Check session capacity
            var session = await GetSessionByIdAsync(enrollment.SessionId);
            if (session == null)
                throw new ArgumentException("Invalid session ID");

            var course = await GetCourseByIdAsync(session.CourseId);
            if (course != null && course.MaxParticipants > 0 && session.EnrolledCount >= course.MaxParticipants)
                throw new InvalidOperationException("Session has reached maximum capacity");

            // Check enrollment eligibility
            var isEligible = await CheckEnrollmentEligibilityAsync(enrollment.EmployeeId, session.CourseId);
            if (!isEligible)
                throw new InvalidOperationException("Employee does not meet course prerequisites");

            enrollment.Status = "enrolled";
            enrollment.EnrollmentDate = DateTime.Now;
            enrollment.Create_time = DateTime.Now;

            var enrollmentId = await Context.Insertable(enrollment).ExecuteReturnIdentityAsync();

            // Update session enrolled count
            await Context.Updateable<HrTrainingSession>()
                .SetColumns(s => s.EnrolledCount == SqlFunc.Subqueryable<HrTrainingEnrollment>()
                    .Where(e => e.SessionId == s.SessionId && e.Status == "enrolled").Count())
                .Where(s => s.SessionId == enrollment.SessionId)
                .ExecuteCommandAsync();

            return enrollmentId;
        }

        public async Task<int> UpdateEnrollmentAsync(HrTrainingEnrollment enrollment)
        {
            enrollment.Update_time = DateTime.Now;
            return await Context.Updateable(enrollment)
                .IgnoreColumns(e => new { e.Create_time, e.Create_by, e.EnrollmentDate })
                .ExecuteCommandAsync();
        }

        public async Task<int> CancelEnrollmentAsync(long enrollmentId, string reason)
        {
            var enrollment = await Context.Queryable<HrTrainingEnrollment>()
                .FirstAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null)
                throw new ArgumentException("Enrollment not found");

            if (enrollment.Status == "completed")
                throw new InvalidOperationException("Cannot cancel completed enrollment");

            var result = await Context.Updateable<HrTrainingEnrollment>()
                .SetColumns(e => new HrTrainingEnrollment
                {
                    Status = "cancelled",
                    Update_time = DateTime.Now
                })
                .Where(e => e.EnrollmentId == enrollmentId)
                .ExecuteCommandAsync();

            // Update session enrolled count
            await Context.Updateable<HrTrainingSession>()
                .SetColumns(s => s.EnrolledCount == SqlFunc.Subqueryable<HrTrainingEnrollment>()
                    .Where(e => e.SessionId == s.SessionId && e.Status == "enrolled").Count())
                .Where(s => s.SessionId == enrollment.SessionId)
                .ExecuteCommandAsync();

            return result;
        }

        public async Task<int> CompleteEnrollmentAsync(long enrollmentId, decimal? score)
        {
            var enrollment = await Context.Queryable<HrTrainingEnrollment>()
                .FirstAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null)
                throw new ArgumentException("Enrollment not found");

            if (enrollment.Status == "completed")
                return 1; // Already completed

            var updateData = new HrTrainingEnrollment
            {
                Status = "completed",
                CompletionDate = DateTime.Now,
                Update_time = DateTime.Now
            };

            if (score.HasValue)
                updateData.Score = score.Value;

            var result = await Context.Updateable(updateData)
                .Where(e => e.EnrollmentId == enrollmentId)
                .ExecuteCommandAsync();

            // Update session completed count
            await Context.Updateable<HrTrainingSession>()
                .SetColumns(s => s.CompletedCount == SqlFunc.Subqueryable<HrTrainingEnrollment>()
                    .Where(e => e.SessionId == s.SessionId && e.Status == "completed").Count())
                .Where(s => s.SessionId == enrollment.SessionId)
                .ExecuteCommandAsync();

            // Issue certificate if applicable
            var session = await GetSessionByIdAsync(enrollment.SessionId);
            var course = await GetCourseByIdAsync(session.CourseId);
            if (!string.IsNullOrEmpty(course.CertificationType))
            {
                await IssueCertificateAsync(enrollmentId);
            }

            return result;
        }

        public async Task<int> BulkEnrollAsync(long sessionId, List<long> employeeIds)
        {
            var enrolledCount = 0;
            
            foreach (var employeeId in employeeIds)
            {
                try
                {
                    var enrollment = new HrTrainingEnrollment
                    {
                        SessionId = sessionId,
                        EmployeeId = employeeId
                    };

                    await EnrollEmployeeAsync(enrollment);
                    enrolledCount++;
                }
                catch (Exception)
                {
                    // Skip if enrollment fails (already enrolled, capacity, etc.)
                    continue;
                }
            }

            return enrolledCount;
        }

        public async Task<bool> CheckEnrollmentEligibilityAsync(long employeeId, long courseId)
        {
            var course = await GetCourseByIdAsync(courseId);
            if (course == null || !course.IsActive)
                return false;

            // Check prerequisites if any
            if (!string.IsNullOrEmpty(course.Prerequisites))
            {
                // This would implement prerequisite checking logic
                // For now, assume all employees are eligible
            }

            // Check if employee has already completed this course within validity period
            if (course.ValidityMonths.HasValue)
            {
                var validUntil = DateTime.Now.AddMonths(-course.ValidityMonths.Value);
                var hasValidCompletion = await Context.Queryable<HrTrainingEnrollment>()
                    .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                    .Where((e, s) => e.EmployeeId == employeeId 
                        && s.CourseId == courseId 
                        && e.Status == "completed"
                        && e.CompletionDate >= validUntil)
                    .AnyAsync();

                if (hasValidCompletion)
                    return false; // Already has valid completion
            }

            return true;
        }

        // Training Progress
        public async Task<List<HrTrainingProgress>> GetProgressAsync(long enrollmentId)
        {
            return await Context.Queryable<HrTrainingProgress>()
                .Where(p => p.EnrollmentId == enrollmentId)
                .OrderBy(p => p.ModuleName)
                .ToListAsync();
        }

        public async Task<int> UpdateProgressAsync(HrTrainingProgress progress)
        {
            progress.LastAccessDate = DateTime.Now;
            progress.Update_time = DateTime.Now;
            
            return await Context.Updateable(progress)
                .IgnoreColumns(p => new { p.Create_time, p.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<decimal> GetOverallProgressAsync(long enrollmentId)
        {
            var progresses = await GetProgressAsync(enrollmentId);
            if (!progresses.Any())
                return 0;

            return progresses.Average(p => p.ProgressPercentage);
        }

        public async Task<int> RecordModuleCompletionAsync(long enrollmentId, string moduleName, decimal score)
        {
            var existingProgress = await Context.Queryable<HrTrainingProgress>()
                .FirstAsync(p => p.EnrollmentId == enrollmentId && p.ModuleName == moduleName);

            if (existingProgress != null)
            {
                // Update existing progress
                existingProgress.ProgressPercentage = 100;
                existingProgress.CompletionDate = DateTime.Now;
                existingProgress.QuizScore = score;
                existingProgress.Status = "completed";
                existingProgress.LastAccessDate = DateTime.Now;
                existingProgress.Update_time = DateTime.Now;

                return await Context.Updateable(existingProgress)
                    .IgnoreColumns(p => new { p.Create_time, p.Create_by })
                    .ExecuteCommandAsync();
            }
            else
            {
                // Create new progress record
                var progress = new HrTrainingProgress
                {
                    EnrollmentId = enrollmentId,
                    ModuleName = moduleName,
                    ProgressPercentage = 100,
                    StartDate = DateTime.Now,
                    CompletionDate = DateTime.Now,
                    QuizScore = score,
                    Status = "completed",
                    LastAccessDate = DateTime.Now,
                    Attempts = 1,
                    Create_time = DateTime.Now
                };

                return await Context.Insertable(progress).ExecuteReturnIdentityAsync();
            }
        }

        // Training Budget
        public async Task<HrTrainingBudget> GetBudgetAsync(long? departmentId, int year)
        {
            return await Context.Queryable<HrTrainingBudget>()
                .FirstAsync(b => (departmentId == null || b.DepartmentId == departmentId) && b.Year == year);
        }

        public async Task<int> AllocateBudgetAsync(HrTrainingBudget budget)
        {
            // Check if budget already exists
            var existing = await GetBudgetAsync(budget.DepartmentId, budget.Year);
            if (existing != null)
                throw new InvalidOperationException("Budget already allocated for this department and year");

            budget.UsedBudget = 0;
            budget.RemainingBudget = budget.AllocatedBudget;
            budget.Create_time = DateTime.Now;

            return await Context.Insertable(budget).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateBudgetUsageAsync(long budgetId, decimal amount)
        {
            var budget = await Context.Queryable<HrTrainingBudget>()
                .FirstAsync(b => b.BudgetId == budgetId);

            if (budget == null)
                throw new ArgumentException("Budget not found");

            budget.UsedBudget += amount;
            budget.RemainingBudget = budget.AllocatedBudget - budget.UsedBudget;
            budget.Update_time = DateTime.Now;

            return await Context.Updateable(budget)
                .IgnoreColumns(b => new { b.Create_time, b.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<decimal> GetRemainingBudgetAsync(long? departmentId, int year)
        {
            var budget = await GetBudgetAsync(departmentId, year);
            return budget?.RemainingBudget ?? 0;
        }

        public async Task<object> GetBudgetUtilizationAsync(int year)
        {
            var budgets = await Context.Queryable<HrTrainingBudget>()
                .Where(b => b.Year == year)
                .ToListAsync();

            return new
            {
                Year = year,
                TotalAllocated = budgets.Sum(b => b.AllocatedBudget),
                TotalUsed = budgets.Sum(b => b.UsedBudget),
                TotalRemaining = budgets.Sum(b => b.RemainingBudget),
                UtilizationRate = budgets.Sum(b => b.AllocatedBudget) > 0 
                    ? budgets.Sum(b => b.UsedBudget) / budgets.Sum(b => b.AllocatedBudget) * 100 
                    : 0,
                DepartmentBreakdown = budgets.Select(b => new
                {
                    DepartmentId = b.DepartmentId,
                    Allocated = b.AllocatedBudget,
                    Used = b.UsedBudget,
                    Remaining = b.RemainingBudget,
                    UtilizationRate = b.AllocatedBudget > 0 ? b.UsedBudget / b.AllocatedBudget * 100 : 0
                }).ToList()
            };
        }

        // Development Plans
        public async Task<List<HrDevelopmentPlan>> GetDevelopmentPlansAsync(long employeeId)
        {
            return await Context.Queryable<HrDevelopmentPlan>()
                .Where(p => p.EmployeeId == employeeId)
                .OrderBy(p => p.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<HrDevelopmentPlan> GetDevelopmentPlanByIdAsync(long planId)
        {
            return await Context.Queryable<HrDevelopmentPlan>()
                .FirstAsync(p => p.PlanId == planId);
        }

        public async Task<int> CreateDevelopmentPlanAsync(HrDevelopmentPlan plan)
        {
            // Validate plan data
            if (plan.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (string.IsNullOrEmpty(plan.PlanTitle))
                throw new ArgumentException("Plan title is required");

            if (plan.EndDate <= plan.StartDate)
                throw new ArgumentException("End date must be after start date");

            plan.Status = "draft";
            plan.ProgressPercentage = 0;
            plan.Create_time = DateTime.Now;

            return await Context.Insertable(plan).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateDevelopmentPlanAsync(HrDevelopmentPlan plan)
        {
            plan.Update_time = DateTime.Now;
            return await Context.Updateable(plan)
                .IgnoreColumns(p => new { p.Create_time, p.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> UpdatePlanProgressAsync(long planId, decimal progress)
        {
            return await Context.Updateable<HrDevelopmentPlan>()
                .SetColumns(p => new HrDevelopmentPlan
                {
                    ProgressPercentage = progress,
                    Update_time = DateTime.Now
                })
                .Where(p => p.PlanId == planId)
                .ExecuteCommandAsync();
        }

        public async Task<int> ProvideMentorFeedbackAsync(long planId, string feedback)
        {
            return await Context.Updateable<HrDevelopmentPlan>()
                .SetColumns(p => new HrDevelopmentPlan
                {
                    MentorFeedback = feedback,
                    ReviewDate = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(p => p.PlanId == planId)
                .ExecuteCommandAsync();
        }

        public async Task<List<HrDevelopmentPlan>> GetPlansForMentorAsync(string mentorId)
        {
            return await Context.Queryable<HrDevelopmentPlan>()
                .Where(p => p.MentorId == mentorId && p.Status == "active")
                .OrderBy(p => p.StartDate)
                .ToListAsync();
        }

        // Training Analytics
        public async Task<object> GetTrainingMetricsAsync(DateTime startDate, DateTime endDate)
        {
            var sessions = await Context.Queryable<HrTrainingSession>()
                .LeftJoin<HrTrainingCourse>((s, c) => s.CourseId == c.CourseId)
                .Where((s, c) => s.StartDate >= startDate && s.EndDate <= endDate)
                .Select((s, c) => new { Session = s, Course = c })
                .ToListAsync();

            var enrollments = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .Where((e, s) => s.StartDate >= startDate && s.EndDate <= endDate)
                .Select((e, s) => e)
                .ToListAsync();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                TotalSessions = sessions.Count,
                TotalEnrollments = enrollments.Count,
                CompletedEnrollments = enrollments.Count(e => e.Status == "completed"),
                CompletionRate = enrollments.Count > 0 ? 
                    enrollments.Count(e => e.Status == "completed") * 100.0 / enrollments.Count : 0,
                AverageScore = enrollments.Where(e => e.Score.HasValue).Any() ?
                    enrollments.Where(e => e.Score.HasValue).Average(e => e.Score.Value) : 0,
                AverageRating = enrollments.Where(e => e.Rating.HasValue).Any() ?
                    enrollments.Where(e => e.Rating.HasValue).Average(e => e.Rating.Value) : 0,
                TotalCost = sessions.Sum(s => s.Session.ActualCost),
                CategoryBreakdown = sessions.GroupBy(s => s.Course.Category).Select(g => new
                {
                    Category = g.Key,
                    Sessions = g.Count(),
                    Cost = g.Sum(s => s.Session.ActualCost)
                }).ToList(),
                MonthlyTrends = sessions.GroupBy(s => new { s.Session.StartDate.Year, s.Session.StartDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Sessions = g.Count(),
                        Enrollments = enrollments.Count(e => g.Any(s => s.Session.SessionId == e.SessionId))
                    }).OrderBy(t => t.Year).ThenBy(t => t.Month).ToList()
            };
        }

        public async Task<object> GetEmployeeTrainingHistoryAsync(long employeeId)
        {
            var enrollments = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .LeftJoin<HrTrainingCourse>((e, s, c) => s.CourseId == c.CourseId)
                .Where((e, s, c) => e.EmployeeId == employeeId)
                .Select((e, s, c) => new
                {
                    Enrollment = e,
                    Session = s,
                    Course = c
                })
                .OrderBy(result => result.Enrollment.EnrollmentDate, OrderByType.Desc)
                .ToListAsync();

            var developmentPlans = await GetDevelopmentPlansAsync(employeeId);

            return new
            {
                EmployeeId = employeeId,
                TotalTrainings = enrollments.Count,
                CompletedTrainings = enrollments.Count(e => e.Enrollment.Status == "completed"),
                AverageScore = enrollments.Where(e => e.Enrollment.Score.HasValue).Any() ?
                    enrollments.Where(e => e.Enrollment.Score.HasValue).Average(e => e.Enrollment.Score.Value) : 0,
                TotalTrainingHours = enrollments.Where(e => e.Enrollment.Status == "completed")
                    .Sum(e => e.Course.DurationHours),
                CertificatesEarned = enrollments.Count(e => !string.IsNullOrEmpty(e.Enrollment.CertificateNumber)),
                TrainingHistory = enrollments.Select(e => new
                {
                    CourseId = e.Course.CourseId,
                    CourseName = e.Course.CourseName,
                    Category = e.Course.Category,
                    EnrollmentDate = e.Enrollment.EnrollmentDate,
                    CompletionDate = e.Enrollment.CompletionDate,
                    Status = e.Enrollment.Status,
                    Score = e.Enrollment.Score,
                    CertificateNumber = e.Enrollment.CertificateNumber
                }).ToList(),
                DevelopmentPlans = developmentPlans.Select(p => new
                {
                    PlanTitle = p.PlanTitle,
                    Status = p.Status,
                    Progress = p.ProgressPercentage,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                }).ToList()
            };
        }

        public async Task<object> GetCourseEffectivenessAsync(long courseId)
        {
            var enrollments = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .Where((e, s) => s.CourseId == courseId)
                .Select((e, s) => e)
                .ToListAsync();

            var course = await GetCourseByIdAsync(courseId);

            return new
            {
                CourseId = courseId,
                CourseName = course?.CourseName,
                TotalEnrollments = enrollments.Count,
                CompletedEnrollments = enrollments.Count(e => e.Status == "completed"),
                CompletionRate = enrollments.Count > 0 ? 
                    enrollments.Count(e => e.Status == "completed") * 100.0 / enrollments.Count : 0,
                AverageScore = enrollments.Where(e => e.Score.HasValue && e.Status == "completed").Any() ?
                    enrollments.Where(e => e.Score.HasValue && e.Status == "completed").Average(e => e.Score.Value) : 0,
                AverageRating = enrollments.Where(e => e.Rating.HasValue).Any() ?
                    enrollments.Where(e => e.Rating.HasValue).Average(e => e.Rating.Value) : 0,
                PassRate = enrollments.Where(e => e.Score.HasValue).Any() ?
                    enrollments.Where(e => e.Score.HasValue).Count(e => e.Score >= 70) * 100.0 / 
                    enrollments.Count(e => e.Score.HasValue) : 0,
                FeedbackSummary = enrollments.Where(e => !string.IsNullOrEmpty(e.Feedback))
                    .Select(e => e.Feedback).ToList()
            };
        }

        public async Task<object> GetTrainingCompletionRatesAsync(DateTime startDate, DateTime endDate)
        {
            var enrollments = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .LeftJoin<HrTrainingCourse>((e, s, c) => s.CourseId == c.CourseId)
                .Where((e, s, c) => e.EnrollmentDate >= startDate && e.EnrollmentDate <= endDate)
                .Select((e, s, c) => new { Enrollment = e, Course = c })
                .ToListAsync();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                OverallCompletionRate = enrollments.Count > 0 ?
                    enrollments.Count(e => e.Enrollment.Status == "completed") * 100.0 / enrollments.Count : 0,
                CompletionRatesByCategory = enrollments.GroupBy(e => e.Course.Category).Select(g => new
                {
                    Category = g.Key,
                    TotalEnrollments = g.Count(),
                    CompletedEnrollments = g.Count(e => e.Enrollment.Status == "completed"),
                    CompletionRate = g.Count() > 0 ? g.Count(e => e.Enrollment.Status == "completed") * 100.0 / g.Count() : 0
                }).ToList(),
                MonthlyTrends = enrollments.GroupBy(e => new { e.Enrollment.EnrollmentDate.Year, e.Enrollment.EnrollmentDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Enrollments = g.Count(),
                        Completions = g.Count(e => e.Enrollment.Status == "completed"),
                        CompletionRate = g.Count() > 0 ? g.Count(e => e.Enrollment.Status == "completed") * 100.0 / g.Count() : 0
                    }).OrderBy(t => t.Year).ThenBy(t => t.Month).ToList()
            };
        }

        public async Task<object> GetSkillGapAnalysisAsync(long? departmentId)
        {
            // This would analyze skill gaps based on job requirements vs training completions
            // For now, return a placeholder structure
            return new
            {
                DepartmentId = departmentId,
                SkillGaps = new List<object>
                {
                    new { Skill = "Project Management", GapLevel = "High", RecommendedCourses = new[] { "PMP Training", "Agile Management" } },
                    new { Skill = "Data Analysis", GapLevel = "Medium", RecommendedCourses = new[] { "Excel Advanced", "Power BI" } },
                    new { Skill = "Leadership", GapLevel = "Low", RecommendedCourses = new[] { "Leadership Essentials" } }
                }
            };
        }

        public async Task<object> GetTrainingROIAsync(long courseId)
        {
            // Calculate ROI based on training cost vs performance improvement
            // This is a simplified calculation
            var course = await GetCourseByIdAsync(courseId);
            var enrollments = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .Where((e, s) => s.CourseId == courseId && e.Status == "completed")
                .Select((e, s) => e)
                .ToListAsync();

            var totalCost = await Context.Queryable<HrTrainingSession>()
                .Where(s => s.CourseId == courseId)
                .SumAsync(s => s.ActualCost);

            return new
            {
                CourseId = courseId,
                CourseName = course?.CourseName,
                TotalCost = totalCost,
                CompletedParticipants = enrollments.Count,
                CostPerParticipant = enrollments.Count > 0 ? totalCost / enrollments.Count : 0,
                EstimatedProductivityGain = enrollments.Count * 1000, // Placeholder calculation
                EstimatedROI = totalCost > 0 ? (enrollments.Count * 1000 - totalCost) / totalCost * 100 : 0
            };
        }

        public async Task<List<object>> GetTopPerformingTraineesAsync(long sessionId)
        {
            // Get enrollments with scores first
            var enrollments = await Context.Queryable<HrTrainingEnrollment>()
                .Where(e => e.SessionId == sessionId && e.Status == "completed" && e.Score.HasValue)
                .OrderBy(e => e.Score, OrderByType.Desc)
                .Take(10)
                .ToListAsync();

            if (!enrollments.Any())
                return new List<object>();

            // Batch load employee data
            var employeeIds = enrollments.Select(e => e.EmployeeId).Distinct().ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeLookup = employees.ToDictionary(e => e.EmployeeId);

            // Combine data
            var result = enrollments.Select(e => new
            {
                EmployeeId = e.EmployeeId,
                EmployeeName = employeeLookup.ContainsKey(e.EmployeeId)
                    ? (employeeLookup[e.EmployeeId].NameAr ?? employeeLookup[e.EmployeeId].FullName)
                    : "Unknown",
                Score = e.Score,
                CompletionDate = e.CompletionDate,
                AttendancePercentage = e.AttendancePercentage
            } as object).ToList();

            return result;
        }

        // Certification Management
        public async Task<int> IssueCertificateAsync(long enrollmentId)
        {
            var enrollment = await Context.Queryable<HrTrainingEnrollment>()
                .FirstAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null || enrollment.Status != "completed")
                return 0;

            if (!string.IsNullOrEmpty(enrollment.CertificateNumber))
                return 1; // Already has certificate

            // Generate certificate number
            var sessionCount = await Context.Queryable<HrTrainingEnrollment>()
                .CountAsync(e => e.SessionId == enrollment.SessionId && !string.IsNullOrEmpty(e.CertificateNumber));
            
            var session = await GetSessionByIdAsync(enrollment.SessionId);
            var course = await GetCourseByIdAsync(session.CourseId);
            
            var certificateNumber = $"{course.CourseCode}-{DateTime.Now:yyyyMM}-{sessionCount + 1:D4}";
            
            var certificateExpiryDate = course.ValidityMonths.HasValue 
                ? DateTime.Now.AddMonths(course.ValidityMonths.Value) 
                : (DateTime?)null;

            return await Context.Updateable<HrTrainingEnrollment>()
                .SetColumns(e => new HrTrainingEnrollment
                {
                    CertificateNumber = certificateNumber,
                    CertificateIssueDate = DateTime.Now,
                    CertificateExpiryDate = certificateExpiryDate,
                    Update_time = DateTime.Now
                })
                .Where(e => e.EnrollmentId == enrollmentId)
                .ExecuteCommandAsync();
        }

        public async Task<object> GetCertificateDetailsAsync(string certificateNumber)
        {
            // Get enrollment and course data
            var enrollmentData = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .LeftJoin<HrTrainingCourse>((e, s, c) => s.CourseId == c.CourseId)
                .Where((e, s, c) => e.CertificateNumber == certificateNumber)
                .Select((e, s, c) => new
                {
                    Enrollment = e,
                    CourseName = c.CourseName
                })
                .FirstAsync();

            if (enrollmentData == null)
                return null;

            // Get employee data
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(enrollmentData.Enrollment.EmployeeId);

            var result = new
            {
                CertificateNumber = enrollmentData.Enrollment.CertificateNumber,
                EmployeeName = employee?.NameAr ?? employee?.FullName ?? "Unknown",
                CourseName = enrollmentData.CourseName,
                CompletionDate = enrollmentData.Enrollment.CompletionDate,
                IssueDate = enrollmentData.Enrollment.CertificateIssueDate,
                ExpiryDate = enrollmentData.Enrollment.CertificateExpiryDate,
                Score = enrollmentData.Enrollment.Score,
                IsValid = enrollmentData.Enrollment.CertificateExpiryDate == null ||
                         enrollmentData.Enrollment.CertificateExpiryDate >= DateTime.Now
            };

            return result;
        }

        public async Task<List<object>> GetExpiringCertificatesAsync(int daysAhead)
        {
            var expiryDate = DateTime.Now.AddDays(daysAhead);

            // Get certificates with course data
            var certificatesData = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .LeftJoin<HrTrainingCourse>((e, s, c) => s.CourseId == c.CourseId)
                .Where((e, s, c) => e.CertificateExpiryDate != null &&
                       e.CertificateExpiryDate <= expiryDate &&
                       e.CertificateExpiryDate >= DateTime.Now)
                .OrderBy(e => e.CertificateExpiryDate)
                .Select((e, s, c) => new
                {
                    Enrollment = e,
                    CourseName = c.CourseName
                })
                .ToListAsync();

            if (!certificatesData.Any())
                return new List<object>();

            // Batch load employee data
            var employeeIds = certificatesData.Select(cd => cd.Enrollment.EmployeeId).Distinct().ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeLookup = employees.ToDictionary(e => e.EmployeeId);

            // Combine data
            var result = certificatesData.Select(cd => new
            {
                EmployeeId = cd.Enrollment.EmployeeId,
                EmployeeName = employeeLookup.ContainsKey(cd.Enrollment.EmployeeId)
                    ? (employeeLookup[cd.Enrollment.EmployeeId].NameAr ?? employeeLookup[cd.Enrollment.EmployeeId].FullName)
                    : "Unknown",
                CourseName = cd.CourseName,
                CertificateNumber = cd.Enrollment.CertificateNumber,
                ExpiryDate = cd.Enrollment.CertificateExpiryDate,
                DaysToExpiry = (cd.Enrollment.CertificateExpiryDate.Value - DateTime.Now).Days
            } as object).ToList();

            return result;
        }

        public async Task<int> RenewCertificationAsync(long enrollmentId)
        {
            var enrollment = await Context.Queryable<HrTrainingEnrollment>()
                .FirstAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null || string.IsNullOrEmpty(enrollment.CertificateNumber))
                return 0;

            var session = await GetSessionByIdAsync(enrollment.SessionId);
            var course = await GetCourseByIdAsync(session.CourseId);

            if (!course.ValidityMonths.HasValue)
                return 0; // Course doesn't have expiring certificates

            var newExpiryDate = DateTime.Now.AddMonths(course.ValidityMonths.Value);

            return await Context.Updateable<HrTrainingEnrollment>()
                .SetColumns(e => new HrTrainingEnrollment
                {
                    CertificateExpiryDate = newExpiryDate,
                    Update_time = DateTime.Now
                })
                .Where(e => e.EnrollmentId == enrollmentId)
                .ExecuteCommandAsync();
        }

        // Training Feedback
        public async Task<int> SubmitTrainingFeedbackAsync(long enrollmentId, string feedback, int rating)
        {
            return await Context.Updateable<HrTrainingEnrollment>()
                .SetColumns(e => new HrTrainingEnrollment
                {
                    Feedback = feedback,
                    Rating = rating,
                    Update_time = DateTime.Now
                })
                .Where(e => e.EnrollmentId == enrollmentId)
                .ExecuteCommandAsync();
        }

        public async Task<object> GetCourseFeedbackAsync(long courseId)
        {
            var feedback = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .Where((e, s) => s.CourseId == courseId && !string.IsNullOrEmpty(e.Feedback))
                .Select((e, s) => new { e.Feedback, e.Rating, e.CompletionDate })
                .ToListAsync();

            return new
            {
                CourseId = courseId,
                TotalFeedback = feedback.Count,
                AverageRating = feedback.Where(f => f.Rating.HasValue).Any() ?
                    feedback.Where(f => f.Rating.HasValue).Average(f => f.Rating.Value) : 0,
                FeedbackItems = feedback.OrderByDescending(f => f.CompletionDate).Take(20).ToList()
            };
        }

        public async Task<object> GetInstructorRatingsAsync(string instructor)
        {
            var ratings = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .LeftJoin<HrTrainingCourse>((e, s, c) => s.CourseId == c.CourseId)
                .Where((e, s, c) => c.Instructor == instructor && e.Rating.HasValue)
                .Select((e, s, c) => new { e.Rating, c.CourseName, e.CompletionDate })
                .ToListAsync();

            return new
            {
                Instructor = instructor,
                TotalRatings = ratings.Count,
                AverageRating = ratings.Any() ? ratings.Average(r => r.Rating.Value) : 0,
                RatingDistribution = ratings.GroupBy(r => r.Rating).Select(g => new
                {
                    Rating = g.Key,
                    Count = g.Count()
                }).OrderBy(r => r.Rating).ToList()
            };
        }

        // Training Reports
        public async Task<byte[]> GenerateTrainingReportAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            // This would integrate with a reporting engine
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> GenerateSessionReportAsync(long sessionId)
        {
            // This would generate a detailed session report
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> GenerateCertificateAsync(long enrollmentId)
        {
            // This would generate a certificate PDF
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportTrainingDataAsync(Expression<Func<HrTrainingCourse, bool>> predicate, string format)
        {
            // This would export training data in various formats
            await Task.CompletedTask;
            return new byte[0];
        }

        // Training Recommendations
        public async Task<List<HrTrainingCourse>> GetRecommendedCoursesAsync(long employeeId)
        {
            // This would implement ML-based course recommendations
            // For now, return courses based on employee's department and incomplete mandatory courses
            var employee = await Context.Queryable<HrEmployeeCore>()
                .FirstAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return new List<HrTrainingCourse>();

            // Get mandatory courses not yet completed by employee
            var completedCourseIds = await Context.Queryable<HrTrainingEnrollment>()
                .LeftJoin<HrTrainingSession>((e, s) => e.SessionId == s.SessionId)
                .Where((e, s) => e.EmployeeId == employeeId && e.Status == "completed")
                .Select((e, s) => s.CourseId)
                .ToListAsync();

            var mandatoryCourses = await Context.Queryable<HrTrainingCourse>()
                .Where(c => c.IsActive && c.IsMandatory && !completedCourseIds.Contains(c.CourseId))
                .ToListAsync();

            return mandatoryCourses;
        }

        public async Task<List<HrTrainingCourse>> GetCareerPathCoursesAsync(long employeeId)
        {
            // This would recommend courses based on career path progression
            var employee = await Context.Queryable<HrEmployeeCore>()
                .FirstAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return new List<HrTrainingCourse>();

            // For now, return courses in "leadership" category for potential career growth
            return await Context.Queryable<HrTrainingCourse>()
                .Where(c => c.IsActive && c.Category.ToLower().Contains("leadership"))
                .OrderBy(c => c.CourseName)
                .ToListAsync();
        }

        public async Task<List<HrTrainingCourse>> GetSkillBasedRecommendationsAsync(long employeeId)
        {
            // This would recommend courses based on skill gaps
            var employee = await Context.Queryable<HrEmployeeCore>()
                .FirstAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return new List<HrTrainingCourse>();

            // For now, return technical courses
            return await Context.Queryable<HrTrainingCourse>()
                .Where(c => c.IsActive && (c.Category.ToLower().Contains("technical") || c.Category.ToLower().Contains("skills")))
                .OrderBy(c => c.CourseName)
                .Take(10)
                .ToListAsync();
        }
    }
}
