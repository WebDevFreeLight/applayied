using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Performance;
using ZR.Repository;
using ZR.Model.HR.EmployeeCore;
using ZR.Service.HR.IHrService;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// Workflow Automation Service
    /// </summary>
    [AppService(ServiceType = typeof(IWorkflowService), ServiceLifetime = LifeTime.Transient)]
    public partial class WorkflowService : BaseService<HrWorkflow>, IWorkflowService
    {
        private readonly INotificationService _notificationService;
        private readonly IHrLeaveService _leaveService;
        private readonly IPerformanceService _performanceService;



        public WorkflowService(
            INotificationService notificationService,
            IHrLeaveService leaveService,
            IPerformanceService performanceService)
        {
            _notificationService = notificationService;
            _leaveService = leaveService;
            _performanceService = performanceService;
        }

        #region Employee Onboarding Workflow

        public async Task<OnboardingWorkflowDto> InitiateOnboardingWorkflow(long employeeId)
        {
            // TODO: Update to use EmployeeApiMappingService or HrEmployeeCoreService
            // var employee = await _employeeService.GetByIdAsync(employeeId);
            // if (employee == null)
            //     throw new Exception("Employee not found");

            // Create onboarding workflow instance
            var workflow = new HrWorkflow
            {
                WorkflowType = "employee_onboarding",
                EntityId = employeeId,
                EntityType = "employee",
                Status = "initiated",
                CurrentStep = "personal_info_verification",
                InitiatedBy = "System",
                InitiatedDate = DateTime.Now,
                Create_time = DateTime.Now
            };

            await InsertAsync(workflow);

            // Create onboarding tasks
            var tasks = new List<HrWorkflowTask>
            {
                new HrWorkflowTask
                {
                    WorkflowId = workflow.WorkflowId,
                    TaskName = "Verify Personal Information",
                    TaskType = "verification",
                    AssignedTo = "HR",
                    DueDate = DateTime.Now.AddDays(1),
                    Priority = "high",
                    Status = "pending",
                    Sequence = 1,
                    Create_time = DateTime.Now
                },
                new HrWorkflowTask
                {
                    WorkflowId = workflow.WorkflowId,
                    TaskName = "Create Employee ID and Credentials",
                    TaskType = "system",
                    AssignedTo = "IT",
                    DueDate = DateTime.Now.AddDays(2),
                    Priority = "high",
                    Status = "pending",
                    Sequence = 2,
                    Create_time = DateTime.Now
                },
                new HrWorkflowTask
                {
                    WorkflowId = workflow.WorkflowId,
                    TaskName = "Assign to Department and Manager",
                    TaskType = "assignment",
                    AssignedTo = "HR",
                    DueDate = DateTime.Now.AddDays(1),
                    Priority = "high",
                    Status = "pending",
                    Sequence = 3,
                    Create_time = DateTime.Now
                },
                new HrWorkflowTask
                {
                    WorkflowId = workflow.WorkflowId,
                    TaskName = "Setup IT Assets",
                    TaskType = "provisioning",
                    AssignedTo = "IT",
                    DueDate = DateTime.Now.AddDays(3),
                    Priority = "medium",
                    Status = "pending",
                    Sequence = 4,
                    Create_time = DateTime.Now
                },
                new HrWorkflowTask
                {
                    WorkflowId = workflow.WorkflowId,
                    TaskName = "Schedule Orientation Session",
                    TaskType = "scheduling",
                    AssignedTo = "HR",
                    DueDate = DateTime.Now.AddDays(5),
                    Priority = "medium",
                    Status = "pending",
                    Sequence = 5,
                    Create_time = DateTime.Now
                },
                new HrWorkflowTask
                {
                    WorkflowId = workflow.WorkflowId,
                    TaskName = "Setup Benefits Enrollment",
                    TaskType = "enrollment",
                    AssignedTo = "HR",
                    DueDate = DateTime.Now.AddDays(7),
                    Priority = "medium",
                    Status = "pending",
                    Sequence = 6,
                    Create_time = DateTime.Now
                },
                new HrWorkflowTask
                {
                    WorkflowId = workflow.WorkflowId,
                    TaskName = "Generate Welcome Package",
                    TaskType = "documentation",
                    AssignedTo = "HR",
                    DueDate = DateTime.Now.AddDays(1),
                    Priority = "low",
                    Status = "pending",
                    Sequence = 7,
                    Create_time = DateTime.Now
                }
            };

            await Context.Insertable(tasks).ExecuteCommandAsync();

            // Send notifications
            // TODO: Update to use proper employee lookup
            // await _notificationService.SendNotificationAsync(new HrNotification()
            // {
            //     RecipientId = employee.ManagerId ?? 0,
            //     Type = "onboarding_started",
            //     Title = "New Employee Onboarding Started",
            //     Message = $"Onboarding process has been initiated for {employee.Name}",
            //     Priority = "high"
            // });

            return new OnboardingWorkflowDto
            {
                WorkflowId = workflow.WorkflowId,
                EmployeeId = employeeId,
                Status = workflow.Status,
                Tasks = tasks.Select(t => new OnboardingTaskDto()
                {
                    TaskId = t.TaskId,
                    TaskName = t.TaskName,
                    AssignedTo = t.AssignedTo,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    Priority = t.Priority
                }).ToList(),
                Progress = 0
            };
        }

        public async Task<bool> CompleteOnboardingTask(long taskId, string completedBy, string notes)
        {
            var task = await Context.Queryable<HrWorkflowTask>()
                .FirstAsync(t => t.TaskId == taskId);

            if (task == null)
                return false;

            task.Status = "completed";
            task.CompletedBy = completedBy;
            task.CompletedDate = DateTime.Now;
            task.Notes = notes;
            task.Update_time = DateTime.Now;

            await Context.Updateable(task).ExecuteCommandAsync();

            // Check if all tasks are completed
            var workflow = await GetFirstAsync(w => w.WorkflowId == task.WorkflowId);
            var allTasks = await Context.Queryable<HrWorkflowTask>()
                .Where(t => t.WorkflowId == task.WorkflowId)
                .ToListAsync();

            if (allTasks.All(t => t.Status == "completed"))
            {
                workflow.Status = "completed";
                workflow.CompletedDate = DateTime.Now;
                workflow.Update_time = DateTime.Now;
                await Context.Updateable(workflow).ExecuteCommandAsync();

                // Send completion notification
                await _notificationService.SendNotificationAsync(new HrNotification()
                {
                    RecipientId = workflow.EntityId,
                    Type = "onboarding_completed",
                    Title = "Onboarding Completed",
                    Message = "Your onboarding process has been completed successfully!",
                    Priority = "high"
                });
            }
            else
            {
                // Move to next task
                var nextTask = allTasks
                    .Where(t => t.Status == "pending")
                    .OrderBy(t => t.Sequence)
                    .FirstOrDefault();

                if (nextTask != null)
                {
                    workflow.CurrentStep = nextTask.TaskName;
                    workflow.Update_time = DateTime.Now;
                    await Context.Updateable(workflow).ExecuteCommandAsync();

                    // Send notification for next task
                    await _notificationService.SendNotificationAsync(new HrNotification()
                    {
                        RecipientType = nextTask.AssignedTo,
                        Type = "task_assigned",
                        Title = "New Task Assigned",
                        Message = $"Task '{nextTask.TaskName}' has been assigned to you",
                        Priority = nextTask.Priority
                    });
                }
            }

            return true;
        }

        #endregion

        #region Leave Approval Workflow

        public async Task<LeaveWorkflowDto> InitiateLeaveApprovalWorkflow(long leaveRequestId)
        {
            var leaveRequest = await _leaveService.GetByIdAsync(leaveRequestId);
            if (leaveRequest == null)
                throw new Exception("Leave request not found");

            // TODO: Update to use EmployeeApiMappingService
            // var employee = await _employeeService.GetByIdAsync(leaveRequest.EmployeeId);

            // Create leave approval workflow
            var workflow = new HrWorkflow
            {
                WorkflowType = "leave_approval",
                EntityId = leaveRequestId,
                EntityType = "leave_request",
                Status = "pending_approval",
                CurrentStep = "manager_approval",
                InitiatedBy = "Employee", // TODO: Get from employee
                InitiatedDate = DateTime.Now,
                Create_time = DateTime.Now
            };

            await InsertAsync(workflow);

            // Create approval steps
            var approvalSteps = new List<HrWorkflowApproval>();

            // Manager approval
            // TODO: Get manager from employee assignment
            // if (employee.ManagerId.HasValue)
            // {
            //     approvalSteps.Add(new HrWorkflowApproval
            //     {
            //         WorkflowId = workflow.WorkflowId,
            //         ApproverId = employee.ManagerId.Value,
            //         ApprovalLevel = 1,
            //         Status = "pending",
            //         Create_time = DateTime.Now
            //     });
            // }

            // HR approval for leaves more than 5 days
            if (leaveRequest.Days > 5)
            {
                approvalSteps.Add(new HrWorkflowApproval
                {
                    WorkflowId = workflow.WorkflowId,
                    ApproverRole = "HR_Manager",
                    ApprovalLevel = 2,
                    Status = "pending",
                    Create_time = DateTime.Now
                });
            }

            await Context.Insertable(approvalSteps).ExecuteCommandAsync();

            // Send notification to first approver
            // TODO: Update to use proper employee lookup
            // if (approvalSteps.Any())
            // {
            //     var firstApprover = approvalSteps.First();
            //     await _notificationService.SendNotificationAsync(new HrNotification()
            //     {
            //         RecipientId = firstApprover.ApproverId ?? 0,
            //         RecipientRole = firstApprover.ApproverRole,
            //         Type = "leave_approval_request",
            //         Title = "Leave Approval Request",
            //         Message = $"{employee.Name} has requested leave from {leaveRequest.StartDate:dd-MMM} to {leaveRequest.EndDate:dd-MMM}",
            //         Priority = "high",
            //         ActionRequired = true,
            //         ActionUrl = $"/hr/leave/approve/{leaveRequestId}"
            //     });
            // }

            return new LeaveWorkflowDto
            {
                WorkflowId = workflow.WorkflowId,
                LeaveRequestId = leaveRequestId,
                Status = workflow.Status,
                CurrentApprover = approvalSteps.FirstOrDefault()?.ApproverId?.ToString(),
                ApprovalSteps = approvalSteps.Select(a => new ApprovalStepDto
                {
                    ApprovalId = a.ApprovalId,
                    ApproverId = a.ApproverId,
                    ApproverRole = a.ApproverRole,
                    Level = a.ApprovalLevel,
                    Status = a.Status
                }).ToList()
            };
        }

        public async Task<bool> ProcessLeaveApproval(long workflowId, long approverId, bool approved, string comments)
        {
            var workflow = await GetFirstAsync(w => w.WorkflowId == workflowId);
            if (workflow == null)
                return false;

            var currentApproval = await Context.Queryable<HrWorkflowApproval>()
                .FirstAsync(a => a.WorkflowId == workflowId && 
                               a.ApproverId == approverId && 
                               a.Status == "pending");

            if (currentApproval == null)
                return false;

            currentApproval.Status = approved ? "approved" : "rejected";
            currentApproval.Comments = comments;
            currentApproval.ApprovalDate = DateTime.Now;
            currentApproval.Update_time = DateTime.Now;

            await Context.Updateable(currentApproval).ExecuteCommandAsync();

            if (!approved)
            {
                // Reject the leave request
                workflow.Status = "rejected";
                workflow.CompletedDate = DateTime.Now;
                await Context.Updateable(workflow).ExecuteCommandAsync();

                await _leaveService.UpdateRequestStatus(workflow.EntityId, "rejected", comments);

                // Send rejection notification
                var leaveRequest = await _leaveService.GetByIdAsync(workflow.EntityId);
                await _notificationService.SendNotificationAsync(new HrNotification()
                {
                    RecipientId = leaveRequest.EmployeeId,
                    Type = "leave_rejected",
                    Title = "Leave Request Rejected",
                    Message = $"Your leave request has been rejected. Reason: {comments}",
                    Priority = "high"
                });
            }
            else
            {
                // Check for next approval level
                var nextApproval = await Context.Queryable<HrWorkflowApproval>()
                    .Where(a => a.WorkflowId == workflowId && 
                               a.ApprovalLevel > currentApproval.ApprovalLevel &&
                               a.Status == "pending")
                    .OrderBy(a => a.ApprovalLevel)
                    .FirstAsync();

                if (nextApproval != null)
                {
                    // Move to next approval level
                    workflow.CurrentStep = $"approval_level_{nextApproval.ApprovalLevel}";
                    await Context.Updateable(workflow).ExecuteCommandAsync();

                    // Send notification to next approver
                    await _notificationService.SendNotificationAsync(new HrNotification()
                    {
                        RecipientId = nextApproval.ApproverId ?? 0,
                        RecipientRole = nextApproval.ApproverRole,
                        Type = "leave_approval_request",
                        Title = "Leave Approval Request",
                        Message = "A leave request requires your approval",
                        Priority = "high",
                        ActionRequired = true,
                        ActionUrl = $"/hr/leave/approve/{workflow.EntityId}"
                    });
                }
                else
                {
                    // All approvals complete - approve the leave
                    workflow.Status = "approved";
                    workflow.CompletedDate = DateTime.Now;
                    await Context.Updateable(workflow).ExecuteCommandAsync();

                    await _leaveService.ApproveRequest(workflow.EntityId, approved, comments, approverId.ToString());

                    // Send approval notification
                    var leaveRequest = await _leaveService.GetByIdAsync(workflow.EntityId);
                    await _notificationService.SendNotificationAsync(new HrNotification()
                    {
                        RecipientId = leaveRequest.EmployeeId,
                        Type = "leave_approved",
                        Title = "Leave Request Approved",
                        Message = "Your leave request has been approved!",
                        Priority = "high"
                    });
                }
            }

            return true;
        }

        #endregion

        #region Performance Review Workflow

        public async Task<PerformanceWorkflowDto> InitiatePerformanceReviewWorkflow(int year, int quarter)
        {
            // Get active employees from HrEmployeeCore
            var employees = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.IsActive && e.TerminationDate == null)
                .ToListAsync();

            var workflows = new List<HrWorkflow>();
            var tasks = new List<HrWorkflowTask>();

            foreach (var employee in employees)
            {
                // Skip if workflow already exists for this employee/period
                var existingWorkflow = await Context.Queryable<HrWorkflow>()
                    .AnyAsync(w => w.WorkflowType == "performance_review" &&
                                   w.EntityId == employee.EmployeeId &&
                                   w.WorkflowData != null &&
                                   w.WorkflowData.Contains($"\"year\":{year}") &&
                                   w.WorkflowData.Contains($"\"quarter\":{quarter}"));

                if (existingWorkflow)
                    continue;

                var workflow = new HrWorkflow
                {
                    WorkflowType = "performance_review",
                    EntityId = employee.EmployeeId,
                    EntityType = "employee",
                    Status = "initiated",
                    CurrentStep = "self_assessment",
                    CurrentStepNumber = 1,
                    TotalSteps = 6,
                    InitiatedBy = "System",
                    InitiatedDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    WorkflowData = $"{{\"year\":{year},\"quarter\":{quarter}}}",
                    Create_time = DateTime.Now
                };

                workflows.Add(workflow);
            }

            if (workflows.Any())
            {
                await Context.Insertable(workflows).ExecuteCommandAsync();

                foreach (var workflow in workflows)
                {
                    tasks.AddRange(new[]
                    {
                        new HrWorkflowTask { WorkflowId = workflow.WorkflowId, TaskName = "Complete Self-Assessment", TaskType = "self_assessment", AssignedToId = workflow.EntityId, DueDate = DateTime.Now.AddDays(7), Priority = "high", Status = "pending", Sequence = 1, IsRequired = true, Create_time = DateTime.Now },
                        new HrWorkflowTask { WorkflowId = workflow.WorkflowId, TaskName = "Manager Evaluation", TaskType = "manager_evaluation", AssignedTo = "Manager", DueDate = DateTime.Now.AddDays(14), Priority = "high", Status = "pending", Sequence = 2, IsRequired = true, Create_time = DateTime.Now },
                        new HrWorkflowTask { WorkflowId = workflow.WorkflowId, TaskName = "Peer Feedback", TaskType = "peer_feedback", AssignedTo = "Peers", DueDate = DateTime.Now.AddDays(14), Priority = "medium", Status = "pending", Sequence = 3, IsRequired = false, IsParallel = true, Create_time = DateTime.Now },
                        new HrWorkflowTask { WorkflowId = workflow.WorkflowId, TaskName = "HR Review", TaskType = "hr_review", AssignedTo = "HR", DueDate = DateTime.Now.AddDays(21), Priority = "high", Status = "pending", Sequence = 4, IsRequired = true, Create_time = DateTime.Now },
                        new HrWorkflowTask { WorkflowId = workflow.WorkflowId, TaskName = "Calibration Meeting", TaskType = "calibration", AssignedTo = "HR", DueDate = DateTime.Now.AddDays(25), Priority = "high", Status = "pending", Sequence = 5, IsRequired = true, Create_time = DateTime.Now },
                        new HrWorkflowTask { WorkflowId = workflow.WorkflowId, TaskName = "Final Rating & Feedback", TaskType = "final_review", AssignedTo = "Manager", DueDate = DateTime.Now.AddDays(30), Priority = "high", Status = "pending", Sequence = 6, IsRequired = true, Create_time = DateTime.Now }
                    });
                }

                await Context.Insertable(tasks).ExecuteCommandAsync();

                // Send bulk notifications
                await _notificationService.SendBulkNotificationAsync(
                    workflows.Select(w => new HrNotification()
                    {
                        RecipientId = w.EntityId,
                        EmployeeId = w.EntityId,
                        Subject = "Performance Review Started",
                        Body = $"Your Q{quarter} {year} performance review has been initiated. Please complete your self-assessment by {DateTime.Now.AddDays(7):dd-MMM}",
                        NotificationType = "in_app",
                        Category = "performance",
                        Priority = "high",
                        ActionRequired = true,
                        ActionUrl = "/hr/performance/self-assessment"
                    }).ToList()
                );
            }

            return new PerformanceWorkflowDto
            {
                Year = year,
                Quarter = quarter,
                TotalEmployees = workflows.Count,
                WorkflowsCreated = workflows.Count,
                Status = "initiated",
                DueDate = DateTime.Now.AddDays(30)
            };
        }

        #endregion

        #region Workflow Monitoring

        public async Task<List<WorkflowStatusDto>> GetActiveWorkflows(string workflowType = null)
        {
            var query = Queryable()
                .Where(w => w.Status != "completed" && w.Status != "cancelled");

            if (!string.IsNullOrEmpty(workflowType))
                query = query.Where(w => w.WorkflowType == workflowType);

            var workflows = await query.ToListAsync();

            var result = new List<WorkflowStatusDto>();

            foreach (var workflow in workflows)
            {
                var tasks = await Context.Queryable<HrWorkflowTask>()
                    .Where(t => t.WorkflowId == workflow.WorkflowId)
                    .ToListAsync();

                result.Add(new WorkflowStatusDto
                {
                    WorkflowId = workflow.WorkflowId,
                    WorkflowType = workflow.WorkflowType,
                    EntityId = workflow.EntityId,
                    Status = workflow.Status,
                    CurrentStep = workflow.CurrentStep,
                    InitiatedDate = workflow.InitiatedDate,
                    DueDate = workflow.DueDate,
                    TotalTasks = tasks.Count,
                    CompletedTasks = tasks.Count(t => t.Status == "completed"),
                    Progress = tasks.Any() ? (decimal)tasks.Count(t => t.Status == "completed") / tasks.Count * 100 : 0,
                    IsOverdue = workflow.DueDate.HasValue && workflow.DueDate < DateTime.Now
                });
            }

            return result.OrderBy(w => w.DueDate).ToList();
        }

        public async Task<bool> SendWorkflowReminders()
        {
            // Get overdue tasks
            var overdueTasks = await Context.Queryable<HrWorkflowTask>()
                .Where(t => t.Status == "pending" && t.DueDate < DateTime.Now)
                .ToListAsync();

            var notifications = new List<HrNotification>();

            foreach (var task in overdueTasks)
            {
                notifications.Add(new HrNotification()
                {
                    RecipientId = task.AssignedToId  ,
                    RecipientRole = task.AssignedTo,
                    Type = "task_overdue",
                    Title = "Task Overdue",
                    Message = $"Task '{task.TaskName}' is overdue. Please complete it as soon as possible.",
                    Priority = "urgent"
                });
            }

            if (notifications.Any())
            {
                await _notificationService.SendBulkNotificationAsync(notifications);
            }

            return true;
        }

        #endregion
    }

}
