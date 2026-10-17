using Infrastructure.Exceptions;
using ZR.Model.HR;
using ZR.Service.HR.IHrService;
using Infrastructure.Attribute;
using ZR.Service.HR.IService;
using ZR.Model.HR.Payroll;
using ZR.Model.HR.Training;
using ZR.Model.HR.Performance;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Model.HR.EmployeeCore;
using ZR.Service.HR.EmployeeCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// Employee Self-Service Portal Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IEmployeeSelfServiceService), ServiceLifetime = LifeTime.Transient)]
    public class EmployeeSelfServiceService : BaseService<HrEmployeeCore>, IEmployeeSelfServiceService
    {
        private readonly IEmployeeApiMappingService _employeeMappingService;
        private readonly IHrEmployeeCoreService _employeeCoreService;
        private readonly IHrLeaveService _leaveService;
        private readonly IAttendanceProcessingService _attendanceService;
        private readonly IHrPayrollService _payrollService;
        private readonly INotificationService _notificationService;

        public EmployeeSelfServiceService(
            IEmployeeApiMappingService employeeMappingService,
            IHrEmployeeCoreService employeeCoreService,
            IHrLeaveService leaveService,
            IAttendanceProcessingService attendanceService,
            IHrPayrollService payrollService,
            INotificationService notificationService)
        {
            _employeeMappingService = employeeMappingService;
            _employeeCoreService = employeeCoreService;
            _leaveService = leaveService;
            _attendanceService = attendanceService;
            _payrollService = payrollService;
            _notificationService = notificationService;
        }

        // Profile Management
        public async Task<EmployeeApiDto> GetMyProfileAsync(long employeeId)
        {
            return await _employeeMappingService.ToApiDtoAsync(employeeId);
        }

        public async Task<int> UpdateMyProfileAsync(long employeeId, HrEmployeeProfile profile)
        {
            // TODO: Update to use EmployeeCoreOrchestrationService for profile updates
            // For now, returning success - needs implementation with new structure
            await LogActivityAsync(employeeId, "profile_update", "Employee updated personal information");
            return 1;
        }

        public async Task<int> UpdateEmergencyContactAsync(long employeeId, HrEmergencyContact contact)
        {
            contact.EmployeeId = employeeId;
            contact.Create_time = DateTime.Now;
            
            var existing = await Context.Queryable<HrEmergencyContact>()
                .Where(c => c.EmployeeId == employeeId && c.IsPrimary)
                .FirstAsync();

            if (existing != null)
            {
                existing.ContactName = contact.ContactName;
                existing.Relationship = contact.Relationship;
                existing.PhoneNumber = contact.PhoneNumber;
                existing.AlternatePhone = contact.AlternatePhone;
                existing.Email = contact.Email;
                existing.Address = contact.Address;
                existing.Update_time = DateTime.Now;
                
                var result = await Context.Updateable(existing).ExecuteCommandAsync();
                return result;
            }
            else
            {
                contact.IsPrimary = true;
                var result = await Context.Insertable(contact).ExecuteCommandAsync();
                return result;
            }
        }

        public async Task<List<HrEmergencyContact>> GetEmergencyContactsAsync(long employeeId)
        {
            return await Context.Queryable<HrEmergencyContact>()
                .Where(c => c.EmployeeId == employeeId)
                .OrderBy(c => c.IsPrimary, OrderByType.Desc)
                .ToListAsync();
        }

        // Leave Management
        public async Task<List<HrLeaveBalance>> GetMyLeaveBalanceAsync(long employeeId)
        {
            return await Context.Queryable<HrLeaveBalance>()
                .Where(b => b.EmployeeId == employeeId && b.Year == DateTime.Now.Year)
                .ToListAsync();
        }

        public async Task<List<HrLeaveRequest>> GetMyLeaveRequestsAsync(long employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = Context.Queryable<HrLeaveRequest>()
                .Where(l => l.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(l => l.StartDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(l => l.EndDate <= endDate.Value);

            return await query.OrderBy(l => l.Create_time, OrderByType.Desc).ToListAsync();
        }

        public async Task<int> SubmitLeaveRequestAsync(HrLeaveRequest request)
        {
            // Validate leave balance
            var balance = await Context.Queryable<HrLeaveBalance>()
                .Where(b => b.EmployeeId == request.EmployeeId 
                    && b.LeaveTypeId == request.LeaveTypeId 
                    && b.Year == DateTime.Now.Year)
                .FirstAsync();

            if (balance == null || balance.AvailableDays < request.Days)
            {
                return -1; // Insufficient balance
            }

            request.Status = "pending";
            request.Create_time = DateTime.Now;
            
            var result = await _leaveService.SubmitLeaveRequestAsync(request);
            
            if (result > 0)
            {
                // TODO: Re-enable manager notification after refactoring with IEmployeeApiMappingService
                // Send notification to manager
                // var employee = await _employeeService.GetByIdAsync(request.EmployeeId);
                // if (employee?.ManagerId != null)
                // {
                //     await _notificationService.SendNotificationAsync(new HrNotification
                //     {
                //         RecipientId = employee.ManagerId.Value,
                //         NotificationType = "in_app",
                //         Subject = "New Leave Request",
                //         Body = $"{employee.FirstName} {employee.LastName} has submitted a leave request",
                //         Priority = "medium",
                //         Category = "leave",
                //         Create_by = "System",
                //         Create_time = DateTime.Now
                //     });
                // }

                await LogActivityAsync(request.EmployeeId, "leave_request", $"Submitted leave request for {request.Days} days");
            }
            
            return result;
        }

        public async Task<int> CancelLeaveRequestAsync(long employeeId, long leaveId)
        {
            var leave = await _leaveService.GetLeaveRequestByIdAsync(leaveId);
            if (leave == null || leave.EmployeeId != employeeId) return 0;
            
            if (leave.Status != "pending")
            {
                return -1; // Cannot cancel non-pending requests
            }

            leave.Status = "cancelled";
            leave.Update_time = DateTime.Now;
            
            var result = await _leaveService.UpdateLeaveRequestAsync(leave);
            
            if (result > 0)
            {
                await LogActivityAsync(employeeId, "leave_cancel", $"Cancelled leave request #{leaveId}");
            }
            
            return result;
        }

        // Attendance Management
        public async Task<List<ZR.Model.HR.Dto.Attendance.HrAttendanceDayDto>> GetMyAttendanceAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            return await _attendanceService.GetAttendanceSummary(employeeId, startDate, endDate);
        }

        public async Task<HrAttendanceSummary> GetMyAttendanceSummaryAsync(long employeeId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var attendance = await GetMyAttendanceAsync(employeeId, startDate, endDate);
            
            var summary = new HrAttendanceSummary
            {
                EmployeeId = employeeId,
                Year = year,
                Month = month,
                TotalDays = attendance.Count,
                PresentDays = attendance.Count(a => a.Status == "present"),
                AbsentDays = attendance.Count(a => a.Status == "absent"),
                LateDays = attendance.Count(a => a.IsLate),
                EarlyDays = attendance.Count(a => a.IsEarlyOut),
                LeaveDays = attendance.Count(a => a.Status == "leave"),
                HolidayDays = attendance.Count(a => a.Status == "holiday"),
                WeekendDays = attendance.Count(a => a.Status == "weekend")
            };
            
            if (attendance.Any())
            {
                // Calculate work hours from minutes
                summary.TotalWorkHours = attendance.Sum(a => a.WorkedMinutes / 60m);
                summary.TotalOvertimeHours = attendance.Sum(a => a.OvertimeMinutes / 60m);
                summary.AverageWorkHours = summary.PresentDays > 0 ? summary.TotalWorkHours / summary.PresentDays : 0;
            }
            
            return summary;
        }

        public async Task<int> CheckInAsync(long employeeId, string location = null)
        {
            var today = DateTime.Today;
            var existing = await _attendanceService.GetEmployeeAttendanceAsync(employeeId, today, today);
            
            if (existing.Any())
            {
                return -1; // Already checked in
            }

            var attendanceDto = new ZR.Model.HR.Dto.Attendance.CreateHrAttendanceDayDto
            {
                EmployeeId = employeeId,
                AttendanceDate = today,
                CheckInTime = DateTime.Now,
                CheckInLocation = location,
                IsRemote = !string.IsNullOrEmpty(location)
            };

            var result = await _attendanceService.CheckInAsync(attendanceDto);
            
            if (result > 0)
            {
                await LogActivityAsync(employeeId, "check_in", $"Checked in at {DateTime.Now:HH:mm}");
            }
            
            return result;
        }

        public async Task<int> CheckOutAsync(long employeeId, string location = null)
        {
            var today = DateTime.Today;
            var attendance = (await _attendanceService.GetEmployeeAttendanceAsync(employeeId, today, today)).FirstOrDefault();

            if (attendance == null || attendance.CheckOutTime.HasValue)
            {
                return -1; // Not checked in or already checked out
            }

            var updateDto = new ZR.Model.HR.Dto.Attendance.UpdateHrAttendanceDayDto
            {
                EmployeeId = employeeId,
                AttendanceDate = today,
                CheckOutTime = DateTime.Now,
                CheckOutLocation = location
            };

            var result = await _attendanceService.CheckOutAsync(updateDto);
            
            if (result > 0)
            {
                await LogActivityAsync(employeeId, "check_out", $"Checked out at {DateTime.Now:HH:mm}");
            }
            
            return result;
        }

        // Payroll Management
        public async Task<List<HrPayrollDetail>> GetMyPayslipsAsync(long employeeId, int year)
        {
            return await Context.Queryable<HrPayrollDetail>()
                .Where(p => p.EmployeeId == employeeId && p.PayPeriodStart.Year == year)
                .OrderBy(p => p.PayPeriodEnd, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<HrPayrollDetail> GetPayslipDetailAsync(long employeeId, long payrollId)
        {
            var payslip = await Context.Queryable<HrPayrollDetail>()
                .Where(p => p.PayrollId == payrollId && p.EmployeeId == employeeId)
                .FirstAsync();

            if (payslip != null)
            {
                await LogActivityAsync(employeeId, "payslip_view", $"Viewed payslip for {payslip.PayPeriodStart:MMM yyyy}");
            }

            return payslip;
        }

        public async Task<HrCompensationSummary> GetCompensationSummaryAsync(long employeeId)
        {
            var summary = new HrCompensationSummary
            {
                EmployeeId = employeeId,
                Year = DateTime.Now.Year
            };

            var ytdPayroll = await Context.Queryable<HrPayrollDetail>()
                .Where(p => p.EmployeeId == employeeId && p.PayPeriodStart.Year == DateTime.Now.Year)
                .ToListAsync();

            if (ytdPayroll.Any())
            {
                summary.YtdGrossEarnings = ytdPayroll.Sum(p => p.GrossPay);
                summary.YtdNetEarnings = ytdPayroll.Sum(p => p.NetPay);
                summary.YtdTaxDeductions = ytdPayroll.Sum(p => p.IncomeTax);
                summary.YtdBenefitDeductions = ytdPayroll.Sum(p => p.TotalDeductions - p.IncomeTax);
                summary.CurrentBaseSalary = ytdPayroll.OrderByDescending(p => p.PayPeriodEnd).First().BasicSalary;
            }

            // Get benefits enrollment
            var benefits = await Context.Queryable<HrBenefitEnrollment>()
                .Where(b => b.EmployeeId == employeeId && b.Status == "active")
                .ToListAsync();

            summary.EnrolledBenefits = benefits.Count;
            summary.MonthlyBenefitCost = benefits.Sum(b => b.EmployeeContribution);

            return summary;
        }

        // Document Management
        // TODO: Implement document management with new document system
        public async Task<List<object>> GetMyDocumentsAsync(long employeeId)
        {
            await Task.CompletedTask;
            return new List<object>();
        }

        public async Task<int> UploadDocumentAsync(object document)
        {
            await Task.CompletedTask;
            // TODO: Implement with new document management system
            return 0;
        }

        public async Task<object> DownloadDocumentAsync(long employeeId, long documentId)
        {
            await Task.CompletedTask;
            // TODO: Implement with new document management system
            return null;
        }

        // Training & Development
        public async Task<List<HrTrainingEnrollment>> GetMyTrainingsAsync(long employeeId)
        {
            return await Context.Queryable<HrTrainingEnrollment>()
                .Where(e => e.EmployeeId == employeeId)
                .OrderBy(e => e.EnrollmentDate, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<List<HrTrainingCourse>> GetAvailableCoursesAsync(long employeeId)
        {
            // Get employee's position and department for filtering relevant courses
            // TODO: Implement employee lookup with new EmployeeCore structure if needed for filtering

            var enrolledCourses = await Context.Queryable<HrTrainingEnrollment, HrTrainingSession>((e, s) => new object[] {
                SqlSugar.JoinType.Inner, e.SessionId == s.SessionId
            })
            .Where((e, s) => e.EmployeeId == employeeId)
            .Select((e, s) => s.CourseId)
            .ToListAsync();

            return await Context.Queryable<HrTrainingCourse>()
                .Where(c => c.IsActive && !enrolledCourses.Contains(c.CourseId))
                .ToListAsync();
        }

        public async Task<int> EnrollInTrainingAsync(long employeeId, long sessionId)
        {
            // Check if already enrolled
            var existing = await Context.Queryable<HrTrainingEnrollment>()
                .Where(e => e.EmployeeId == employeeId && e.SessionId == sessionId)
                .AnyAsync();

            if (existing) return -1;

            // Check session availability
            var session = await Context.Queryable<HrTrainingSession>()
                .Where(s => s.SessionId == sessionId && s.Status == "scheduled")
                .FirstAsync();

            if (session == null) return -2;

            var course = await Context.Queryable<HrTrainingCourse>()
                .Where(c => c.CourseId == session.CourseId)
                .FirstAsync();

            if (session.EnrolledCount >= course.MaxParticipants) return -3;

            var enrollment = new HrTrainingEnrollment
            {
                SessionId = sessionId,
                EmployeeId = employeeId,
                EnrollmentDate = DateTime.Now,
                Status = "enrolled",
                Create_by = employeeId.ToString(),
                Create_time = DateTime.Now
            };

            var result = await Context.Insertable(enrollment).ExecuteCommandAsync();

            if (result > 0)
            {
                // Update session enrolled count
                session.EnrolledCount++;
                await Context.Updateable(session).ExecuteCommandAsync();

                await LogActivityAsync(employeeId, "training_enrollment", $"Enrolled in training session #{sessionId}");
                
                // Send confirmation notification
                await _notificationService.SendTrainingNotificationAsync(employeeId, sessionId, course.CourseName);
            }

            return result;
        }

        public async Task<HrSkillsMatrix> GetMySkillsAsync(long employeeId)
        {
            return await Context.Queryable<HrSkillsMatrix>()
                .Where(s => s.EmployeeId == employeeId)
                .FirstAsync();
        }

        public async Task<int> UpdateMySkillsAsync(long employeeId, List<HrSkillsMatrix> skills)
        {
            var updatedCount = 0;
            
            foreach (var skill in skills)
            {
                skill.EmployeeId = employeeId;
                skill.Update_time = DateTime.Now;
                
                var existing = await Context.Queryable<HrSkillsMatrix>()
                    .Where(s => s.EmployeeId == employeeId && s.SkillName == skill.SkillName)
                    .FirstAsync();

                if (existing != null)
                {
                    existing.ProficiencyLevel = skill.ProficiencyLevel;
                    existing.Update_time = DateTime.Now;
                    await Context.Updateable(existing).ExecuteCommandAsync();
                }
                else
                {
                    skill.Create_time = DateTime.Now;
                    await Context.Insertable(skill).ExecuteCommandAsync();
                }
                
                updatedCount++;
            }

            if (updatedCount > 0)
            {
                await LogActivityAsync(employeeId, "skills_update", $"Updated {updatedCount} skills");
            }

            return updatedCount;
        }

        // Performance Management
        public async Task<List<HrPerformanceReview>> GetMyPerformanceReviewsAsync(long employeeId)
        {
            return await Context.Queryable<HrPerformanceReview>()
                .Where(r => r.EmployeeId == employeeId)
                .OrderBy(r => r.ReviewPeriodEnd, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<List<HrPerformanceGoal>> GetMyGoalsAsync(long employeeId)
        {
            return await Context.Queryable<HrPerformanceGoal>()
                .Where(g => g.EmployeeId == employeeId)
                .OrderBy(g => g.DueDate)
                .ToListAsync();
        }

        public async Task<int> UpdateGoalProgressAsync(long employeeId, long goalId, decimal progress, string notes)
        {
            var goal = await Context.Queryable<HrPerformanceGoal>()
                .Where(g => g.GoalId == goalId && g.EmployeeId == employeeId)
                .FirstAsync();

            if (goal == null) return 0;

            goal.Progress =(int) progress;
            goal.EmployeeNotes = notes;
            goal.Update_time = DateTime.Now;

            if (progress >= 100)
            {
                goal.Status = "completed";
                goal.CompletionDate = DateTime.Now;
            }
            else if (progress > 0)
            {
                goal.Status = "in_progress";
            }

            var result = await Context.Updateable(goal).ExecuteCommandAsync();

            if (result > 0)
            {
                await LogActivityAsync(employeeId, "goal_update", $"Updated goal #{goalId} progress to {progress}%");
            }

            return result;
        }

        public async Task<int> SubmitSelfReviewAsync(HrPerformanceReview review)
        {
            review.Status = "self_review_completed";
            review.SelfReviewDate = DateTime.Now;
            review.Update_time = DateTime.Now;

            var result = await Context.Updateable(review).ExecuteCommandAsync();

            if (result > 0)
            {
                await LogActivityAsync(review.EmployeeId, "self_review", "Submitted self performance review");

                // TODO: Re-enable manager notification after refactoring with IEmployeeApiMappingService
                // Notify manager
                // var employee = await _employeeService.GetByIdAsync(review.EmployeeId);
                // if (employee?.ManagerId != null)
                // {
                //     await _notificationService.SendPerformanceReviewNotificationAsync(
                //         employee.ManagerId.Value,
                //         review.ReviewId,
                //         "Self Review Completed"
                //     );
                // }
            }

            return result;
        }

        // Team Management (for managers)
        public async Task<List<EmployeeListDto>> GetMyTeamAsync(long managerId)
        {
            // Get team employee IDs from current assignments
            var now = DateTime.Now;
            var teamIds = await Context.Queryable<HrEmployeeAssignment>()
                .Where(a => a.ManagerEmployeeId == managerId)
                .Where(a => a.EffectiveFrom <= now && (a.EffectiveTo == null || a.EffectiveTo > now))
                .Select(a => a.EmployeeId)
                .ToListAsync();

            // Convert to DTOs
            return await _employeeMappingService.ToListDtoAsync(teamIds);
        }

        public async Task<List<HrLeaveRequest>> GetTeamLeaveRequestsAsync(long managerId)
        {
            var now = DateTime.Now;
            var teamIds = await Context.Queryable<HrEmployeeAssignment>()
                .Where(a => a.ManagerEmployeeId == managerId)
                .Where(a => a.EffectiveFrom <= now && (a.EffectiveTo == null || a.EffectiveTo > now))
                .Select(a => a.EmployeeId)
                .ToListAsync();

            return await Context.Queryable<HrLeaveRequest>()
                .Where(l => teamIds.Contains(l.EmployeeId) && l.Status == "pending")
                .OrderBy(l => l.Create_time)
                .ToListAsync();
        }

        public async Task<HrTeamAttendanceSummary> GetTeamAttendanceSummaryAsync(long managerId, DateTime date)
        {
            var now = DateTime.Now;
            var teamIds = await Context.Queryable<HrEmployeeAssignment>()
                .Where(a => a.ManagerEmployeeId == managerId)
                .Where(a => a.EffectiveFrom <= now && (a.EffectiveTo == null || a.EffectiveTo > now))
                .Select(a => a.EmployeeId)
                .ToListAsync();

            var attendance = await Context.Queryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                .Where(a => teamIds.Contains(a.EmployeeId) && a.AttendanceDate == date)
                .ToListAsync();

            var summary = new HrTeamAttendanceSummary
            {
                Date = date,
                TotalTeamMembers = teamIds.Count,
                Present = attendance.Count(a => !a.IsAbsent),
                Absent = attendance.Count(a => a.Status == "absent"),
                OnLeave = attendance.Count(a => a.Status == "leave"),
                Late = attendance.Count(a => a.IsLate),
                WorkFromHome = attendance.Count(a => a.IsRemote)
            };

            summary.AttendanceRate = summary.TotalTeamMembers > 0 
                ? (decimal)summary.Present / summary.TotalTeamMembers * 100 
                : 0;

            return summary;
        }

        // Notifications
        public async Task<List<HrNotification>> GetMyNotificationsAsync(long employeeId, bool unreadOnly = false)
        {
            if (unreadOnly)
            {
                return await _notificationService.GetUnreadNotificationsAsync(employeeId);
            }
            
            return await _notificationService.GetUserNotificationsAsync(employeeId);
        }

        public async Task<int> MarkNotificationAsReadAsync(long employeeId, long notificationId)
        {
            var notification = await Context.Queryable<HrNotification>()
                .Where(n => n.NotificationId == notificationId && n.RecipientId == employeeId)
                .FirstAsync();

            if (notification == null) return 0;

            return await _notificationService.MarkAsReadAsync(notificationId);
        }

        // Activity Logging
        private async Task LogActivityAsync(long employeeId, string activityType, string description)
        {
            var activity = new HrEmployeeActivity
            {
                EmployeeId = employeeId,
                ActivityType = activityType,
                Description = description,
                ActivityDate = DateTime.Now,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                Create_time = DateTime.Now
            };

            await Context.Insertable(activity).ExecuteCommandAsync();
        }

        // Helper methods
        private async Task<ZR.Model.HR.Attendance.Shifts_Scheduling.HrShift> GetEmployeeShiftAsync(long employeeId)
        {
            // TODO: Reimplement after refactoring with proper joins or IEmployeeApiMappingService
            // var employee = await _employeeService.GetByIdAsync(employeeId);
            // if (employee?.ShiftId == null) return null;
            // return await Context.Queryable<ZR.Model.HR.Attendance.Shifts_Scheduling.HrShift>()
            //     .Where(s => s.ShiftId == employee.ShiftId)
            //     .FirstAsync();
            return null; // Temporarily disabled
        }

        private string GetClientIpAddress()
        {
            // Implementation depends on web framework
            return "127.0.0.1";
        }

        private string GetUserAgent()
        {
            // Implementation depends on web framework
            return "Employee Portal";
        }

        // Upload Document
        public async Task<int> UploadMyDocumentAsync(long employeeId, object documentData)
        {
            if (documentData == null) return 0;

            // TODO: Implement document upload with new document management system
            await LogActivityAsync(employeeId, "Document Upload", "Document uploaded");
            return 1;
        }

        // Get Benefits
        public async Task<object> GetMyBenefitsAsync(long employeeId)
        {
            var benefits = await Context.Queryable<HrBenefitEnrollment, HrBenefitPlan>(
                    (e, p) => new object[] { SqlSugar.JoinType.Inner, e.PlanId == p.PlanId })
                .Where((e, p) => e.EmployeeId == employeeId)
                .Select((e, p) => new
                {
                    EnrollmentId = e.EnrollmentId,
                    PlanName = p.PlanName,
                    PlanType = p.PlanType,
                    Provider = p.Provider,
                    Coverage = p.Description, // Using Description instead of CoverageDetails
                    EmployeeContribution = e.EmployeeContribution,
                    EmployerContribution = e.EmployerContribution,
                    EnrollmentDate = e.EnrollmentDate,
                    EffectiveDate = e.EnrollmentDate, // Using EnrollmentDate as EffectiveDate
                    Status = e.Status
                })
                .ToListAsync();

            // Calculate totals
            var totalEmployeeContribution = benefits.Sum(b => b.EmployeeContribution);
            var totalEmployerContribution = benefits.Sum(b => b.EmployerContribution);
            var totalValue = totalEmployeeContribution + totalEmployerContribution;

            // Get available benefits not enrolled
            var allPlans = await Context.Queryable<HrBenefitPlan>()
                .Where(p => p.IsActive == true)
                .ToListAsync();

            var enrolledPlanIds = benefits.Select(b => b.PlanName).ToList();
            var availablePlans = allPlans.Where(p => !enrolledPlanIds.Contains(p.PlanName)).ToList();

            return new
            {
                EnrolledBenefits = benefits,
                AvailablePlans = availablePlans,
                Summary = new
                {
                    TotalPlans = benefits.Count,
                    MonthlyEmployeeContribution = totalEmployeeContribution,
                    MonthlyEmployerContribution = totalEmployerContribution,
                    TotalMonthlyValue = totalValue,
                    AnnualValue = totalValue * 12
                },
                LastUpdated = benefits.Any() ? benefits.Max(b => b.EnrollmentDate) : (DateTime?)null
            };
        }

        // Get Training History
        public async Task<object> GetMyTrainingHistoryAsync(long employeeId)
        {
            var trainingHistory = await Context.Queryable<HrTrainingEnrollment, HrTrainingSession, HrTrainingCourse>(
                    (e, s, c) => new object[]
                    {
                        SqlSugar.JoinType.Inner, e.SessionId == s.SessionId,
                        SqlSugar.JoinType.Inner, s.CourseId == c.CourseId
                    })
                .Where((e, s, c) => e.EmployeeId == employeeId)
                .Select((e, s, c) => new
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Category = c.Category,
                    Duration = c.DurationHours,
                    Instructor = s.Instructor ?? "TBD", // Using Instructor field
                    Location = s.Location,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status,
                    Score = e.Score,
                    AttendancePercentage = e.AttendancePercentage,
                    CompletionDate = e.CompletionDate,
                    CertificateNumber = e.CertificateNumber,
                    CertificateIssueDate = e.CertificateIssueDate,
                    CertificateExpiryDate = e.CertificateExpiryDate,
                    Rating = e.Rating,
                    Feedback = e.Feedback
                })
                .OrderBy(t => t.StartDate, OrderByType.Desc)
                .ToListAsync();

            // Calculate statistics
            var completedCourses = trainingHistory.Count(t => t.Status == "completed");
            var inProgressCourses = trainingHistory.Count(t => t.Status == "in_progress");
            var totalHours = trainingHistory.Where(t => t.Status == "completed").Sum(t => t.Duration);
            var averageScore = trainingHistory.Where(t => t.Score.HasValue).Average(t => t.Score ?? 0);

            // Get upcoming trainings
            var upcomingTrainings = await Context.Queryable<HrTrainingEnrollment, HrTrainingSession, HrTrainingCourse>(
                    (e, s, c) => new object[]
                    {
                        SqlSugar.JoinType.Inner, e.SessionId == s.SessionId,
                        SqlSugar.JoinType.Inner, s.CourseId == c.CourseId
                    })
                .Where((e, s, c) => e.EmployeeId == employeeId && 
                                   s.StartDate > DateTime.Now && 
                                   e.Status == "enrolled")
                .Select((e, s, c) => new
                {
                    CourseName = c.CourseName,
                    StartDate = s.StartDate,
                    Location = s.Location
                })
                .ToListAsync();

            return new
            {
                TrainingHistory = trainingHistory,
                UpcomingTrainings = upcomingTrainings,
                Statistics = new
                {
                    TotalCourses = trainingHistory.Count,
                    CompletedCourses = completedCourses,
                    InProgressCourses = inProgressCourses,
                    TotalTrainingHours = totalHours,
                    AverageScore = Math.Round(averageScore, 2),
                    CertificationsEarned = trainingHistory.Count(t => !string.IsNullOrEmpty(t.CertificateNumber))
                }
            };
        }

        // Submit Time Off Request
        public async Task<int> SubmitTimeOffRequestAsync(object requestData)
        {
            if (requestData == null) return 0;

            var reqJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            dynamic req = Newtonsoft.Json.JsonConvert.DeserializeObject(reqJson);

            // Check leave balance
            long employeeId = req.employeeId ?? 0;
            long leaveTypeId = req.leaveTypeId ?? 1;
            decimal requestedDays = req.days ?? 0;

            var leaveBalance = await Context.Queryable<HrLeaveBalance>()
                .Where(lb => lb.EmployeeId == employeeId && 
                            lb.LeaveTypeId == leaveTypeId && 
                            lb.Year == DateTime.Now.Year)
                .FirstAsync();

            if (leaveBalance == null || leaveBalance.AvailableDays < requestedDays)
            {
                throw new AppException(ExceptionCode.HR_LEAVE_INSUFFICIENT_BALANCE)
                    .WithParameter("AvailableDays", leaveBalance?.AvailableDays ?? 0)
                    .WithParameter("RequestedDays", requestedDays);
            }

            // Create leave request
            var leaveRequest = new HrLeaveRequest
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                StartDate = req.startDate ?? DateTime.Now.AddDays(1),
                EndDate = req.endDate ?? DateTime.Now.AddDays(1),
                Days = requestedDays,
                Reason = req.reason ?? "Personal",
                Status = "Pending",
                // RequestDate = DateTime.Now, // Field doesn't exist
                // IsHalfDay = req.isHalfDay ?? false, // Field doesn't exist
                // ContactDuringLeave = req.contactNumber ?? "", // Field doesn't exist
                Create_time = DateTime.Now
            };

            var result = await Context.Insertable(leaveRequest).ExecuteReturnIdentityAsync();

            if (result > 0)
            {
                // TODO: Re-enable manager notification after refactoring with IEmployeeApiMappingService
                // Get manager ID
                // var employee = await _employeeService.GetByIdAsync(employeeId);
                // if (employee?.ManagerId != null)
                // {
                //     // Send notification to manager
                //     await _notificationService.SendNotificationAsync(new HrNotification
                //     {
                //         RecipientId = employee.ManagerId.Value,
                //         NotificationType = "in_app",
                //         Subject = "New Leave Request",
                //         Body = $"{employee.FirstName} {employee.LastName} has submitted a leave request for {requestedDays} day(s) from {leaveRequest.StartDate:yyyy-MM-dd}",
                //         Priority = "high",
                //         Category = "leave",
                //         IsRead = false,
                //         Create_time = DateTime.Now
                //     });
                // }

                // Log activity
                await LogActivityAsync(employeeId, "Leave Request", 
                    $"Submitted leave request for {requestedDays} day(s)");
            }

            return result;
        }
    }
}
