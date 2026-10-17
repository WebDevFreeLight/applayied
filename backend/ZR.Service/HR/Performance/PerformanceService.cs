using ZR.Model.HR.EmployeeCore;
using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Model.HR.Performance;
using ZR.Model.System;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// Performance Management Service Implementation
    /// </summary>


    [AppService(ServiceType = typeof(IPerformanceService), ServiceLifetime = LifeTime.Transient)]
    public class PerformanceService : BaseService<HrPerformanceReview>, IPerformanceService
    {
        private readonly IEmployeeDataService _employeeDataService;

        public PerformanceService(IEmployeeDataService employeeDataService) : base()
        {
            _employeeDataService = employeeDataService;
        }

        #region Base CRUD Operations

        public async Task<PagedInfo<HrPerformanceReview>> GetPageAsync(Expression<Func<HrPerformanceReview, bool>> predicate, PagerInfo pager)
        {
            var source = Queryable();
            if (predicate != null)
            {
                source = source.Where(predicate);
            }
            var pagedInfo = source.ToPage(pager);
            return pagedInfo;
        }

        public async Task<HrPerformanceReview> GetByIdAsync(long id)
        {
            return await GetFirstAsync(r => r.ReviewId == id);
        }

        public async Task<int> AddAsync(HrPerformanceReview review)
        {
            return await InsertAsync(review) > 0 ? 1 : 0;
        }

        public new async Task<int> UpdateAsync(HrPerformanceReview review)
        {
            return await Context.Updateable(review).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] ids)
        {
            return await Context.Deleteable<HrPerformanceReview>()
                .In(ids)
                .ExecuteCommandAsync();
        }

        public new async Task<List<HrPerformanceReview>> GetListAsync(Expression<Func<HrPerformanceReview, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> SubmitReviewAsync(long reviewId)
        {
            return await Context.Updateable<HrPerformanceReview>()
                .SetColumns(r => r.Status == "submitted")
                .SetColumns(r => r.SubmittedDate == DateTime.Now)
                .Where(r => r.ReviewId == reviewId)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> ApproveReviewAsync(long reviewId, bool approved, string comments, string approvedBy)
        {
            var status = approved ? "approved" : "rejected";
            return await Context.Updateable<HrPerformanceReview>()
                .SetColumns(r => r.Status == status)
                .SetColumns(r => r.ApprovedDate == DateTime.Now)
                .SetColumns(r => r.ApprovedBy == approvedBy)
                .SetColumns(r => r.ManagerComments == comments)
                .Where(r => r.ReviewId == reviewId)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<List<HrPerformanceReview>> GetEmployeeHistoryAsync(long employeeId)
        {
            return await Queryable()
                .Where(r => r.EmployeeId == employeeId)
                .OrderBy(r => r.ReviewDate, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<List<HrPerformanceReviewGoal>> GetEmployeeGoalsAsync(long employeeId)
        {
            return await Context.Queryable<HrPerformanceReviewGoal>()
                .Where(g => g.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<int> SetGoalAsync(HrPerformanceReviewGoal goal)
        {
            var id = await Context.Insertable(goal).ExecuteReturnBigIdentityAsync();
            return id > 0 ? 1 : 0;
        }

        public async Task<bool> UpdateGoalProgressAsync(long goalId, decimal progress, string notes)
        {
            return await Context.Updateable<HrPerformanceReviewGoal>()
                .SetColumns(g => g.CurrentProgress == progress)
                .SetColumns(g => g.Update_time == DateTime.Now)
                .SetColumns(g => g.Comments == notes)
                .Where(g => g.GoalId == goalId)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<object> GetPerformanceMetricsAsync(long employeeId, string period)
        {
            var reviews = await Queryable()
                .Where(r => r.EmployeeId == employeeId && r.ReviewPeriod == period)
                .ToListAsync();

            return new
            {
                EmployeeId = employeeId,
                Period = period,
                AverageRating = reviews.Average(r => r.OverallRating ?? 0),
                ReviewCount = reviews.Count,
                Reviews = reviews
            };
        }

        public async Task<object> GetTeamPerformanceAsync(long managerId)
        {
            var teamMembersData = await _employeeDataService.GetTeamMembersAsync(managerId);
            var teamMembers = teamMembersData.Select(e => e.EmployeeId).ToList();

            var reviews = await Queryable()
                .Where(r => teamMembers.Contains(r.EmployeeId))
                .ToListAsync();

            return new
            {
                ManagerId = managerId,
                TeamSize = teamMembers.Count,
                AverageRating = reviews.Average(r => r.OverallRating ?? 0),
                ReviewsCompleted = reviews.Count,
                TeamReviews = reviews
            };
        }

        #endregion

        #region Performance Reviews

        /// <summary>
        /// Get performance reviews with filtering
        /// </summary>
        public async Task<PagedInfo<PerformanceReviewDto>> GetReviewsAsync(PerformanceQueryDto query)
        {
            var predicate = Expressionable.Create<HrPerformanceReview>();
            
            if (query.EmployeeId.HasValue)
                predicate = predicate.And(r => r.EmployeeId == query.EmployeeId.Value);
            
            if (query.ReviewerId.HasValue)
                predicate = predicate.And(r => r.ReviewerId == query.ReviewerId.Value);
            
            if (query.ReviewYear.HasValue)
                predicate = predicate.And(r => r.ReviewYear == query.ReviewYear.Value);
            
            if (query.ReviewQuarter.HasValue)
                predicate = predicate.And(r => r.ReviewQuarter == query.ReviewQuarter.Value);
            
            if (!string.IsNullOrEmpty(query.Status))
                predicate = predicate.And(r => r.Status == query.Status);
            
            if (!string.IsNullOrEmpty(query.ReviewType))
                predicate = predicate.And(r => r.ReviewType == query.ReviewType);
            
            if (query.DepartmentId.HasValue)
            {
                var now = DateTime.Now;
                predicate = predicate.And(r => SqlFunc.Subqueryable<HrEmployeeAssignment>()
                    .Where(a => a.EmployeeId == r.EmployeeId
                        && a.DepartmentId == query.DepartmentId.Value
                        && a.EffectiveFrom <= now
                        && (a.EffectiveTo == null || a.EffectiveTo > now)
                        && a.IsPrimary)
                    .Any());
            }

            var list = await Context.Queryable<HrPerformanceReview>()
                .Where(predicate.ToExpression())
                .OrderBy(r => r.ReviewId, OrderByType.Desc)
                .ToPageAsync(query);

            var result = new PagedInfo<PerformanceReviewDto>
            {
                TotalNum = list.TotalNum,
                PageIndex = list.PageIndex,
                PageSize = list.PageSize,
                Result = list.Result.Select(MapReviewToDto).ToList()
            };

            return result;
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        public async Task<PerformanceReviewDto> GetReviewByIdAsync(long reviewId)
        {
            var review = await Context.Queryable<HrPerformanceReview>()
                .FirstAsync(r => r.ReviewId == reviewId);
            
            return review != null ? MapReviewToDto(review) : null;
        }

        /// <summary>
        /// Create performance review
        /// </summary>
        public async Task<PerformanceReviewDto> CreateReviewAsync(PerformanceReviewDto reviewDto)
        {
            var review = new HrPerformanceReview
            {
                EmployeeId = reviewDto.EmployeeId,
                ReviewerId = reviewDto.ReviewerId,
                ReviewPeriod = reviewDto.ReviewPeriod,
                ReviewType = reviewDto.ReviewType,
                ReviewDate = reviewDto.ReviewDate,
                ReviewYear = DateTime.Now.Year,
                ReviewQuarter = (DateTime.Now.Month - 1) / 3 + 1,
                QualityRating = reviewDto.QualityRating,
                ProductivityRating = reviewDto.ProductivityRating,
                KnowledgeRating = reviewDto.KnowledgeRating,
                CommunicationRating = reviewDto.CommunicationRating,
                TeamworkRating = reviewDto.TeamworkRating,
                InitiativeRating = reviewDto.InitiativeRating,
                AttendanceRating = reviewDto.AttendanceRating,
                OverallRating = CalculateOverallRating(reviewDto),
                Strengths = reviewDto.Strengths,
                AreasForImprovement = reviewDto.AreasForImprovement,
                GoalsAchieved = reviewDto.GoalsAchieved,
                TrainingNeeds = reviewDto.TrainingNeeds,
                CareerAspirations = reviewDto.CareerAspirations,
                ManagerComments = reviewDto.ManagerComments,
                EmployeeComments = reviewDto.EmployeeComments,
                Status = "draft",
                Create_time = DateTime.Now,
                Create_by = "System"
            };

            review.ReviewId = await Context.Insertable(review).ExecuteReturnIdentityAsync();
            reviewDto.ReviewId = review.ReviewId;
            reviewDto.OverallRating = review.OverallRating ?? 0;
            reviewDto.Status = review.Status;

            return reviewDto;
        }

        /// <summary>
        /// Update performance review
        /// </summary>
        public async Task<bool> UpdateReviewAsync(PerformanceReviewDto reviewDto)
        {
            var review = await Context.Queryable<HrPerformanceReview>()
                .FirstAsync(r => r.ReviewId == reviewDto.ReviewId);
            
            if (review == null || review.Status == "approved")
                return false;

            review.QualityRating = reviewDto.QualityRating;
            review.ProductivityRating = reviewDto.ProductivityRating;
            review.KnowledgeRating = reviewDto.KnowledgeRating;
            review.CommunicationRating = reviewDto.CommunicationRating;
            review.TeamworkRating = reviewDto.TeamworkRating;
            review.InitiativeRating = reviewDto.InitiativeRating;
            review.AttendanceRating = reviewDto.AttendanceRating;
            review.OverallRating = CalculateOverallRating(reviewDto);
            review.Strengths = reviewDto.Strengths;
            review.AreasForImprovement = reviewDto.AreasForImprovement;
            review.GoalsAchieved = reviewDto.GoalsAchieved;
            review.TrainingNeeds = reviewDto.TrainingNeeds;
            review.CareerAspirations = reviewDto.CareerAspirations;
            review.ManagerComments = reviewDto.ManagerComments;
            review.EmployeeComments = reviewDto.EmployeeComments;
            review.Update_time = DateTime.Now;
            review.Update_by = "System";

            return await Context.Updateable(review).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Submit review for approval
        /// </summary>
        public async Task<bool> SubmitReviewAsync(long reviewId, string submittedBy)
        {
            var review = await Context.Queryable<HrPerformanceReview>()
                .FirstAsync(r => r.ReviewId == reviewId);
            
            if (review == null || review.Status != "draft")
                return false;

            review.Status = "submitted";
            review.SubmittedDate = DateTime.Now;
            review.Update_time = DateTime.Now;
            review.Update_by = submittedBy;

            return await Context.Updateable(review)
                .UpdateColumns(r => new { r.Status, r.SubmittedDate, r.Update_time, r.Update_by })
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Approve performance review
        /// </summary>
        public async Task<bool> ApproveReviewAsync(long reviewId, string approvedBy, string comments)
        {
            var review = await Context.Queryable<HrPerformanceReview>()
                .FirstAsync(r => r.ReviewId == reviewId);
            
            if (review == null || review.Status != "submitted")
                return false;

            review.Status = "approved";
            review.ApprovedDate = DateTime.Now;
            review.ApprovedBy = approvedBy;
            if (!string.IsNullOrEmpty(comments))
                review.ManagerComments = review.ManagerComments + "\n[Approval Comments]: " + comments;
            review.Update_time = DateTime.Now;
            review.Update_by = approvedBy;

            return await Context.Updateable(review)
                .UpdateColumns(r => new { r.Status, r.ApprovedDate, r.ApprovedBy, r.ManagerComments, r.Update_time, r.Update_by })
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Reject performance review
        /// </summary>
        public async Task<bool> RejectReviewAsync(long reviewId, string rejectedBy, string reason)
        {
            var review = await Context.Queryable<HrPerformanceReview>()
                .FirstAsync(r => r.ReviewId == reviewId);
            
            if (review == null || review.Status != "submitted")
                return false;

            review.Status = "rejected";
            review.ManagerComments = review.ManagerComments + "\n[Rejection Reason]: " + reason;
            review.Update_time = DateTime.Now;
            review.Update_by = rejectedBy;

            return await Context.Updateable(review)
                .UpdateColumns(r => new { r.Status, r.ManagerComments, r.Update_time, r.Update_by })
                .ExecuteCommandAsync() > 0;
        }

        #endregion

        #region Goals Management

        /// <summary>
        /// Get employee goals
        /// </summary>
        public async Task<List<PerformanceGoalDto>> GetEmployeeGoalsAsync(long employeeId, int? year = null)
        {
            var query = Context.Queryable<HrPerformanceGoal>()
                .Where(g => g.EmployeeId == employeeId);
            
            if (year.HasValue)
                query = query.Where(g => g.Year == year.Value);
            
            var goals = await query.OrderBy(g => g.TargetDate).ToListAsync();
            
            return goals.Select(MapGoalToDto).ToList();
        }

        /// <summary>
        /// Create goal
        /// </summary>
        public async Task<PerformanceGoalDto> CreateGoalAsync(PerformanceGoalDto goalDto)
        {
            var goal = new HrPerformanceGoal
            {
                EmployeeId = goalDto.EmployeeId,
                GoalTitle = goalDto.GoalTitle,
                Description = goalDto.Description,
                Category = goalDto.Category,
                Year = goalDto.Year,
                Quarter = goalDto.Quarter,
                TargetDate = goalDto.TargetDate,
                TargetValue = goalDto.TargetValue,
                TargetUnit = goalDto.TargetUnit,
                CurrentProgress = 0,
                Weight = goalDto.Weight,
                Status = "pending",
                Create_time = DateTime.Now,
                Create_by = "System"
            };

            goal.GoalId = await Context.Insertable(goal).ExecuteReturnIdentityAsync();
            goalDto.GoalId = goal.GoalId;
            goalDto.Status = goal.Status;

            return goalDto;
        }

        /// <summary>
        /// Update goal
        /// </summary>
        public async Task<bool> UpdateGoalAsync(PerformanceGoalDto goalDto)
        {
            var goal = await Context.Queryable<HrPerformanceGoal>()
                .FirstAsync(g => g.GoalId == goalDto.GoalId);
            
            if (goal == null || goal.Status == "completed")
                return false;

            goal.GoalTitle = goalDto.GoalTitle;
            goal.Description = goalDto.Description;
            goal.Category = goalDto.Category;
            goal.TargetDate = goalDto.TargetDate;
            goal.TargetValue = goalDto.TargetValue;
            goal.TargetUnit = goalDto.TargetUnit;
            goal.Weight = goalDto.Weight;
            goal.Update_time = DateTime.Now;
            goal.Update_by = "System";

            return await Context.Updateable(goal).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Update goal progress with user
        /// </summary>
        public async Task<bool> UpdateGoalProgressAsync2(long goalId, decimal progress, string Update_by)
        {
            var goal = await Context.Queryable<HrPerformanceGoal>()
                .FirstAsync(g => g.GoalId == goalId);
            
            if (goal == null)
                return false;

            goal.CurrentProgress =(int) progress;
            goal.Status = progress >= 100 ? "completed" : "in_progress";
            if (progress >= 100)
            {
                goal.CompletedDate = DateTime.Now;
                goal.AchievedValue = goal.TargetValue;
            }
            goal.Update_time = DateTime.Now;
            goal.Update_by = Update_by;

            return await Context.Updateable(goal)
                .UpdateColumns(g => new { g.CurrentProgress, g.Status, g.CompletedDate, g.AchievedValue, g.Update_time, g.Update_by })
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Complete goal
        /// </summary>
        public async Task<bool> CompleteGoalAsync(long goalId, decimal achievementScore, string comments)
        {
            var goal = await Context.Queryable<HrPerformanceGoal>()
                .FirstAsync(g => g.GoalId == goalId);
            
            if (goal == null)
                return false;

            goal.Status = "completed";
            goal.CompletedDate = DateTime.Now;
            goal.AchievementScore = achievementScore;
            goal.Comments = comments;
            goal.CurrentProgress = 100;
            goal.Update_time = DateTime.Now;
            goal.Update_by = "System";

            return await Context.Updateable(goal).ExecuteCommandAsync() > 0;
        }

        #endregion

        #region KPI Management

        /// <summary>
        /// Get KPI metrics
        /// </summary>
        public async Task<List<KpiMetricDto>> GetKpiMetricsAsync(long? departmentId = null, long? positionId = null)
        {
            var query = Context.Queryable<HrKpiMetric>()
                .Where(m => m.IsActive);
            
            if (departmentId.HasValue)
                query = query.Where(m => m.DepartmentId == departmentId.Value || m.DepartmentId == null);
            
            if (positionId.HasValue)
                query = query.Where(m => m.PositionId == positionId.Value || m.PositionId == null);
            
            var metrics = await query.ToListAsync();
            
            return metrics.Select(MapKpiToDto).ToList();
        }

        /// <summary>
        /// Create KPI metric
        /// </summary>
        public async Task<KpiMetricDto> CreateKpiMetricAsync(KpiMetricDto metricDto)
        {
            var metric = new HrKpiMetric
            {
                MetricName = metricDto.MetricName,
                MetricNameAr = metricDto.MetricNameAr,
                Description = metricDto.Description,
                Category = metricDto.Category,
                DepartmentId = metricDto.DepartmentId??0,
                PositionId = metricDto.PositionId??0,
                MeasurementUnit = metricDto.MeasurementUnit,
                Frequency = metricDto.Frequency,
                TargetValue = metricDto.TargetValue,
                MinValue = metricDto.MinValue,
                MaxValue = metricDto.MaxValue,
                Weight = metricDto.Weight,
                IsActive = true,
                Create_time = DateTime.Now,
                Create_by = "System"
            };

            metric.MetricId = await Context.Insertable(metric).ExecuteReturnIdentityAsync();
            metricDto.MetricId = metric.MetricId;

            return metricDto;
        }

        /// <summary>
        /// Update KPI metric
        /// </summary>
        public async Task<bool> UpdateKpiMetricAsync(KpiMetricDto metricDto)
        {
            var metric = await Context.Queryable<HrKpiMetric>()
                .FirstAsync(m => m.MetricId == metricDto.MetricId);
            
            if (metric == null)
                return false;

            metric.MetricName = metricDto.MetricName;
            metric.MetricNameAr = metricDto.MetricNameAr;
            metric.Description = metricDto.Description;
            metric.Category = metricDto.Category;
            metric.DepartmentId = metricDto.DepartmentId ?? 0   ;
            metric.PositionId = metricDto.PositionId ?? 0;
            metric.MeasurementUnit = metricDto.MeasurementUnit;
            metric.Frequency = metricDto.Frequency;
            metric.TargetValue = metricDto.TargetValue;
            metric.MinValue = metricDto.MinValue;
            metric.MaxValue = metricDto.MaxValue;
            metric.Weight = metricDto.Weight;
            metric.IsActive = metricDto.IsActive;
            metric.Update_time = DateTime.Now;
            metric.Update_by = "System";

            bool isUpdated = await Context.Updateable(metric).ExecuteCommandAsync() > 0;
            return isUpdated;
        }

        /// <summary>
        /// Record KPI value
        /// </summary>
        public async Task<bool> RecordKpiValueAsync(long employeeId, long metricId, decimal value, DateTime period)
        {
            // Check if record exists
            var existing = await Context.Queryable<HrKpiRecord>()
                .FirstAsync(r => r.EmployeeId == employeeId && r.MetricId == metricId && r.Period == period);
            
            if (existing != null)
            {
                existing.Value = value;
                existing.Update_time = DateTime.Now;
                return await Context.Updateable(existing).ExecuteCommandAsync() > 0;
            }
            else
            {
                var record = new HrKpiRecord
                {
                    EmployeeId = employeeId,
                    MetricId = metricId,
                    Period = period,
                    Value = value,
                    Create_time = DateTime.Now,
                    Create_by = "System"
                };
                return await Context.Insertable(record).ExecuteCommandAsync() > 0;
            }
        }

        #endregion

        #region 360 Feedback

        /// <summary>
        /// Get pending feedback requests
        /// </summary>
        public async Task<List<FeedbackRequestDto>> GetPendingFeedbacksAsync(long reviewerId)
        {
            var feedbacks = await Context.Queryable<Hr360Feedback>()
                .Where(f => f.ReviewerId == reviewerId && f.Status == "pending")
                .Select(f => new { f.FeedbackId, f.EmployeeId, f.ReviewerId, f.ReviewPeriod, f.RequestDate, f.DueDate, f.Status })
                .ToListAsync();

            var employeeIds = feedbacks.Select(f => f.EmployeeId).ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            var requests = feedbacks.Select(f =>
            {
                var emp = employeeDict.GetValueOrDefault(f.EmployeeId);
                return new FeedbackRequestDto
                {
                    RequestId = f.FeedbackId,
                    EmployeeId = f.EmployeeId,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "",
                    ReviewerId = f.ReviewerId,
                    ReviewPeriod = f.ReviewPeriod,
                    RequestDate = f.RequestDate,
                    DueDate = (DateTime)f.DueDate,
                    Status = f.Status
                };
            }).ToList();

            return requests;
        }

        /// <summary>
        /// Submit 360 feedback
        /// </summary>
        public async Task<bool> SubmitFeedbackAsync(long requestId, FeedbackResponseDto feedback)
        {
            var request = await Context.Queryable<Hr360Feedback>()
                .FirstAsync(f => f.FeedbackId == requestId);
            
            if (request == null || request.Status != "pending")
                return false;

            request.OverallRating = feedback.OverallRating;
            request.TechnicalSkillsRating = feedback.TechnicalSkillsRating;
            request.CommunicationRating = feedback.CommunicationRating;
            request.TeamworkRating = feedback.TeamworkRating;
            request.LeadershipRating = feedback.LeadershipRating;
            request.ProblemSolvingRating = feedback.ProblemSolvingRating;
            request.Strengths = feedback.Strengths;
            request.ImprovementAreas = feedback.ImprovementAreas;
            request.Comments = feedback.AdditionalComments;
            request.Relationship = feedback.Relationship;
            request.Status = "completed";
            request.SubmittedDate = DateTime.Now;
            request.Update_time = DateTime.Now;

            return await Context.Updateable(request).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Request 360 feedback
        /// </summary>
        public async Task<bool> Request360FeedbackAsync(long employeeId, List<long> reviewerIds, string reviewPeriod)
        {
            var feedbacks = reviewerIds.Select(reviewerId => new Hr360Feedback
            {
                EmployeeId = employeeId,
                ReviewerId = reviewerId,
                ReviewPeriod = reviewPeriod,
                RequestDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Status = "pending",
                Create_time = DateTime.Now,
                Create_by = "System"
            }).ToList();

            return await Context.Insertable(feedbacks).ExecuteCommandAsync() > 0;
        }

        #endregion

        #region Performance Analytics

        /// <summary>
        /// Get performance analytics for an employee
        /// </summary>
        public async Task<PerformanceAnalyticsDto> GetPerformanceAnalyticsAsync(long employeeId, int year)
        {
            // Get employee data using EmployeeDataService
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(new List<long> { employeeId });
            var employee = employees.FirstOrDefault();

            if (employee == null)
                return null;

            // Get reviews for the year
            var reviews = await Context.Queryable<HrPerformanceReview>()
                .Where(r => r.EmployeeId == employeeId && r.ReviewYear == year && r.Status == "approved")
                .ToListAsync();

            // Get goals for the year
            var goals = await Context.Queryable<HrPerformanceGoal>()
                .Where(g => g.EmployeeId == employeeId && g.Year == year)
                .ToListAsync();

            // Get KPI records
            var kpiRecords = await Context.Queryable<HrKpiRecord>()
                .LeftJoin<HrKpiMetric>((r, m) => r.MetricId == m.MetricId)
                .Where((r, m) => r.EmployeeId == employeeId && r.Period.Year == year)
                .Select((r, m) => new
                {
                    MetricName = m.MetricName,
                    Target = m.TargetValue,
                    Value = r.Value
                })
                .ToListAsync();

            var analytics = new PerformanceAnalyticsDto
            {
                EmployeeId = employeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Year = year,
                AverageRating = reviews.Any() ? reviews.Average(r => r.OverallRating ?? 0) : 0,
                GoalsCompleted = goals.Count(g => g.Status == "completed"),
                TotalGoals = goals.Count,
                GoalCompletionRate = goals.Any() ? (decimal)goals.Count(g => g.Status == "completed") / goals.Count * 100 : 0,
                QuarterlyRatings = GetQuarterlyRatings(reviews),
                KpiScores = kpiRecords.Select(k => new KpiScore
                {
                    MetricName = k.MetricName,
                    Target = k.Target,
                    Achieved = k.Value,
                    Score = k.Target > 0 ? (k.Value / k.Target * 100) : 0
                }).ToList(),
                PerformanceLevel = DeterminePerformanceLevel(reviews.Any() ? reviews.Average(r => r.OverallRating ?? 0) : 0),
                RatingsByCategory = GetRatingsByCategory(reviews)
            };

            // Calculate percentile rank
            analytics.PercentileRank = await CalculatePercentileRank(employeeId, year, analytics.AverageRating);

            // Calculate rating trend
            var previousYearAvg = await GetPreviousYearAverageRating(employeeId, year - 1);
            if (previousYearAvg > 0)
            {
                analytics.RatingTrend = ((analytics.AverageRating - previousYearAvg) / previousYearAvg) * 100;
            }

            return analytics;
        }

        /// <summary>
        /// Get department performance
        /// </summary>
        public async Task<DepartmentPerformanceDto> GetDepartmentPerformanceAsync(long departmentId, int year, int? quarter = null)
        {
            var department = await Context.Queryable<SysDept>()
                .FirstAsync(d => d.DeptId == departmentId);
            
            if (department == null)
                return null;

            // Get employees in department
            var departmentEmployees = await _employeeDataService.GetEmployeesByDepartmentAsync(departmentId);
            var employeeIds = departmentEmployees.Select(e => e.EmployeeId).ToList();

            // Get reviews
            var reviewsQuery = Context.Queryable<HrPerformanceReview>()
                .Where(r => employeeIds.Contains(r.EmployeeId) && r.ReviewYear == year && r.Status == "approved");
            
            if (quarter.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.ReviewQuarter == quarter.Value);
            
            var reviews = await reviewsQuery.ToListAsync();

            // Get goals
            var goals = await Context.Queryable<HrPerformanceGoal>()
                .Where(g => employeeIds.Contains(g.EmployeeId) && g.Year == year)
                .ToListAsync();

            var performance = new DepartmentPerformanceDto
            {
                DepartmentId = departmentId,
                DepartmentName = department.DeptName,
                Year = year,
                Quarter = quarter,
                TotalEmployees = employeeIds.Count,
                ReviewsCompleted = reviews.Select(r => r.EmployeeId).Distinct().Count(),
                AverageRating = reviews.Any() ? reviews.Average(r => r.OverallRating ?? 0) : 0,
                GoalCompletionRate = goals.Any() ? (decimal)goals.Count(g => g.Status == "completed") / goals.Count * 100 : 0,
                RatingDistribution = GetRatingDistribution(reviews),
                TopPerformers = await GetTopPerformersInDepartment(departmentId, year, 5),
                CommonStrengths = ExtractCommonThemes(reviews.Select(r => r.Strengths).ToList()),
                CommonImprovementAreas = ExtractCommonThemes(reviews.Select(r => r.AreasForImprovement).ToList())
            };

            return performance;
        }

        /// <summary>
        /// Get top performers
        /// </summary>
        public async Task<List<TopPerformerDto>> GetTopPerformersAsync(int year, int? quarter = null, int topCount = 10)
        {
            // Get all reviews and calculate in memory
            var reviewsQuery = Context.Queryable<HrPerformanceReview>()
                .Where(r => r.ReviewYear == year && r.Status == "approved");

            if (quarter.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.ReviewQuarter == quarter.Value);

            var allReviews = await reviewsQuery.ToListAsync();

            var topPerformerData = allReviews
                .GroupBy(r => r.EmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    AverageRating = g.Average(r => r.OverallRating)
                })
                .OrderByDescending(x => x.AverageRating)
                .Take(topCount)
                .ToList();

            // Get employee data
            var employeeIds = topPerformerData.Select(x => x.EmployeeId).ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            // Get departments and positions
            var departmentIds = employees.Where(e => e.DepartmentId.HasValue).Select(e => e.DepartmentId.Value).Distinct().ToList();
            var positionIds = employees.Where(e => e.PositionId.HasValue).Select(e => e.PositionId.Value).Distinct().ToList();

            var departmentsList = await Context.Queryable<SysDept>()
                .Where(d => departmentIds.Contains(d.DeptId))
                .ToListAsync();
            var departments = departmentsList.ToDictionary(d => d.DeptId, d => d.DeptName);

            var positionsList = await Context.Queryable<SysPost>()
                .Where(p => positionIds.Contains(p.PostId))
                .ToListAsync();
            var positions = positionsList.ToDictionary(p => p.PostId, p => p.PostName);

            // Combine data
            var topPerformers = topPerformerData.Select(tp =>
            {
                var emp = employeeDict.GetValueOrDefault(tp.EmployeeId);
                return new TopPerformerDto
                {
                    EmployeeId = tp.EmployeeId,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "",
                    Department = emp?.DepartmentId != null && departments.ContainsKey(emp.DepartmentId.Value) ? departments[emp.DepartmentId.Value] : "",
                    Position = emp?.PositionId != null && positions.ContainsKey(emp.PositionId.Value) ? positions[emp.PositionId.Value] : "",
                    Rating = tp.AverageRating ?? 0
                };
            }).ToList();

            // Calculate goal completion rates
            for (int i = 0; i < topPerformers.Count; i++)
            {
                var performer = topPerformers[i];
                performer.Rank = i + 1;
                
                var goals = await Context.Queryable<HrPerformanceGoal>()
                    .Where(g => g.EmployeeId == performer.EmployeeId && g.Year == year)
                    .ToListAsync();
                
                performer.GoalCompletionRate = goals.Any() ? 
                    (decimal)goals.Count(g => g.Status == "completed") / goals.Count * 100 : 0;
            }

            return topPerformers;
        }

        /// <summary>
        /// Compare performance between employees
        /// </summary>
        public async Task<PerformanceComparisonDto> ComparePerformanceAsync(List<long> employeeIds, int year)
        {
            var comparison = new PerformanceComparisonDto
            {
                Year = year,
                Employees = new List<EmployeeComparison>(),
                CategoryComparisons = new Dictionary<string, List<decimal>>()
            };

            // Get employee data using EmployeeDataService
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            foreach (var employeeId in employeeIds)
            {
                var employee = employeeDict.GetValueOrDefault(employeeId);

                if (employee == null)
                    continue;

                var reviews = await Context.Queryable<HrPerformanceReview>()
                    .Where(r => r.EmployeeId == employeeId && r.ReviewYear == year && r.Status == "approved")
                    .ToListAsync();

                if (!reviews.Any())
                    continue;

                var avgRatings = new Dictionary<string, decimal>
                {
                    ["Quality"] = reviews.Average(r => r.QualityRating ?? 0),
                    ["Productivity"] = reviews.Average(r => r.ProductivityRating ?? 0),
                    ["Knowledge"] = reviews.Average(r => r.KnowledgeRating ?? 0),
                    ["Communication"] = reviews.Average(r => r.CommunicationRating ?? 0),
                    ["Teamwork"] = reviews.Average(r => r.TeamworkRating ?? 0),
                    ["Initiative"] = reviews.Average(r => r.InitiativeRating ?? 0),
                    ["Attendance"] = reviews.Average(r => r.AttendanceRating ?? 0)
                };

                comparison.Employees.Add(new EmployeeComparison
                {
                    EmployeeId = employeeId,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    OverallRating = reviews.Average(r => r.OverallRating ?? 0),
                    CategoryRatings = avgRatings
                });

                // Build category comparisons
                foreach (var category in avgRatings.Keys)
                {
                    if (!comparison.CategoryComparisons.ContainsKey(category))
                        comparison.CategoryComparisons[category] = new List<decimal>();
                    
                    comparison.CategoryComparisons[category].Add(avgRatings[category]);
                }
            }

            // Calculate averages
            if (comparison.Employees.Any())
            {
                comparison.TeamAverage = comparison.Employees.Average(e => e.OverallRating);
                
                // Calculate company average
                var allReviews = await Context.Queryable<HrPerformanceReview>()
                    .Where(r => r.ReviewYear == year && r.Status == "approved")
                    .ToListAsync();
                
                comparison.CompanyAverage = allReviews.Any() ? allReviews.Average(r => r.OverallRating ?? 0) : 0;
            }

            return comparison;
        }

        #endregion

        #region Performance Improvement Plans

        /// <summary>
        /// Get active PIPs
        /// </summary>
        public async Task<List<PerformanceImprovementPlanDto>> GetActivePIPsAsync(long? employeeId = null)
        {
            var query = Context.Queryable<HrPerformanceImprovementPlan>()
                .Where(p => p.Status == "active");
            
            if (employeeId.HasValue)
                query = query.Where(p => p.EmployeeId == employeeId.Value);
            
            var pips = await query.ToListAsync();
            
            return pips.Select(MapPIPToDto).ToList();
        }

        /// <summary>
        /// Create PIP
        /// </summary>
        public async Task<PerformanceImprovementPlanDto> CreatePIPAsync(PerformanceImprovementPlanDto pipDto)
        {
            var pip = new HrPerformanceImprovementPlan
            {
                EmployeeId = pipDto.EmployeeId,
                StartDate = pipDto.StartDate,
                EndDate = pipDto.EndDate,
                Reason = pipDto.Reason,
                ImprovementAreas = pipDto.ImprovementAreas,
                SpecificGoals = pipDto.SpecificGoals,
                SupportProvided = pipDto.SupportProvided,
                MilestoneChecks = pipDto.MilestoneChecks,
                ConsequencesOfFailure = pipDto.ConsequencesOfFailure,
                CompletionPercentage = 0,
                Status = "active",
                Create_time = DateTime.Now,
                Create_by = "System"
            };

            pip.PipId = await Context.Insertable(pip).ExecuteReturnIdentityAsync();
            pipDto.PipId = pip.PipId;
            pipDto.Status = pip.Status;

            return pipDto;
        }

        /// <summary>
        /// Update PIP progress
        /// </summary>
        public async Task<bool> UpdatePIPProgressAsync(long pipId, string progressUpdate, decimal completionPercentage)
        {
            var pip = await Context.Queryable<HrPerformanceImprovementPlan>()
                .FirstAsync(p => p.PipId == pipId);
            
            if (pip == null || pip.Status != "active")
                return false;

            pip.ProgressUpdates = string.IsNullOrEmpty(pip.ProgressUpdates) ? 
                progressUpdate : 
                pip.ProgressUpdates + $"\n[{DateTime.Now:yyyy-MM-dd}]: {progressUpdate}";
            pip.CompletionPercentage = completionPercentage;
            pip.Update_time = DateTime.Now;
            pip.Update_by = "System";

            return await Context.Updateable(pip)
                .UpdateColumns(p => new { p.ProgressUpdates, p.CompletionPercentage, p.Update_time, p.Update_by })
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Complete PIP
        /// </summary>
        public async Task<bool> CompletePIPAsync(long pipId, bool successful, string finalComments)
        {
            var pip = await Context.Queryable<HrPerformanceImprovementPlan>()
                .FirstAsync(p => p.PipId == pipId);
            
            if (pip == null || pip.Status != "active")
                return false;

            pip.Status = successful ? "completed_success" : "completed_failure";
            pip.CompletedDate = DateTime.Now;
            pip.FinalOutcome = finalComments;
            pip.CompletionPercentage = successful ? 100 : pip.CompletionPercentage;
            pip.Update_time = DateTime.Now;
            pip.Update_by = "System";

            return await Context.Updateable(pip).ExecuteCommandAsync() > 0;
        }

        #endregion

        #region Calibration Sessions

        /// <summary>
        /// Create calibration session
        /// </summary>
        public async Task<CalibrationSessionDto> CreateCalibrationSessionAsync(CalibrationSessionDto sessionDto)
        {
            // Implementation would involve creating session and adding participants
            // This is a simplified version
            return sessionDto;
        }

        /// <summary>
        /// Update employee rating in calibration
        /// </summary>
        public async Task<bool> UpdateEmployeeRatingAsync(long sessionId, long employeeId, decimal rating, string justification)
        {
            // Implementation would update the calibrated rating for an employee in a session
            return true;
        }

        /// <summary>
        /// Finalize calibration session
        /// </summary>
        public async Task<bool> FinalizeCalibrationAsync(long sessionId, string finalizedBy)
        {
            // Implementation would finalize the calibration session and update all employee ratings
            return true;
        }

        #endregion

        #region Reports

        /// <summary>
        /// Generate performance report for an employee
        /// </summary>
        public async Task<byte[]> GeneratePerformanceReportAsync(long employeeId, int year)
        {
            // Implementation would generate a PDF/Excel report
            // This is a placeholder
            return System.Text.Encoding.UTF8.GetBytes($"Performance Report for Employee {employeeId} - Year {year}");
        }

        /// <summary>
        /// Generate team performance report
        /// </summary>
        public async Task<byte[]> GenerateTeamPerformanceReportAsync(long managerId, int year, int? quarter = null)
        {
            // Implementation would generate a PDF/Excel report for the manager's team
            return System.Text.Encoding.UTF8.GetBytes($"Team Performance Report for Manager {managerId} - Year {year}");
        }

        /// <summary>
        /// Get review cycle status
        /// </summary>
        public async Task<ReviewCycleStatusDto> GetReviewCycleStatusAsync(int year, int? quarter = null)
        {
            // Get all active employees using EmployeeDataService
            var allEmployees = await _employeeDataService.GetAllActiveEmployeesAsync();

            var reviewsQuery = Context.Queryable<HrPerformanceReview>()
                .Where(r => r.ReviewYear == year);

            if (quarter.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.ReviewQuarter == quarter.Value);

            var reviews = await reviewsQuery.ToListAsync();

            var status = new ReviewCycleStatusDto
            {
                Year = year,
                Quarter = quarter,
                TotalEmployees = allEmployees.Count,
                ReviewsScheduled = allEmployees.Count, // Assuming all should have reviews
                ReviewsCompleted = reviews.Count(r => r.Status == "approved"),
                ReviewsPending = reviews.Count(r => r.Status == "submitted" || r.Status == "draft"),
                ReviewsOverdue = 0, // Would need to calculate based on due dates
                DepartmentStatuses = new List<DepartmentReviewStatus>()
            };

            status.CompletionPercentage = status.ReviewsScheduled > 0 ? 
                (decimal)status.ReviewsCompleted / status.ReviewsScheduled * 100 : 0;

            // Get department statuses
            var departments = await Context.Queryable<SysDept>()
                .Where(d => d.Status == 1)
                .ToListAsync();

            foreach (var dept in departments)
            {
                var deptEmployees = allEmployees.Where(e => e.DepartmentId == dept.DeptId).ToList();
                var deptReviews = reviews.Where(r => deptEmployees.Any(e => e.EmployeeId == r.EmployeeId)).ToList();
                
                if (deptEmployees.Any())
                {
                    status.DepartmentStatuses.Add(new DepartmentReviewStatus
                    {
                        DepartmentId = dept.DeptId,
                        DepartmentName = dept.DeptName,
                        TotalReviews = deptEmployees.Count,
                        Completed = deptReviews.Count(r => r.Status == "approved"),
                        CompletionRate = (decimal)deptReviews.Count(r => r.Status == "approved") / deptEmployees.Count * 100
                    });
                }
            }

            return status;
        }

        #endregion

        #region Helper Methods

        private PerformanceReviewDto MapReviewToDto(HrPerformanceReview review)
        {
            return new PerformanceReviewDto
            {
                ReviewId = review.ReviewId,
                EmployeeId = review.EmployeeId,
                ReviewerId = review.ReviewerId,
                ReviewPeriod = review.ReviewPeriod,
                ReviewType = review.ReviewType,
                ReviewDate = (DateTime)review.ReviewDate,
                QualityRating = review.QualityRating,
                ProductivityRating = review.ProductivityRating,
                KnowledgeRating = review.KnowledgeRating,
                CommunicationRating = review.CommunicationRating,
                TeamworkRating = review.TeamworkRating,
                InitiativeRating = review.InitiativeRating,
                AttendanceRating = review.AttendanceRating,
                OverallRating = (decimal)review.OverallRating,
                Strengths = review.Strengths,
                AreasForImprovement = review.AreasForImprovement,
                GoalsAchieved = review.GoalsAchieved,
                TrainingNeeds = review.TrainingNeeds,
                CareerAspirations = review.CareerAspirations,
                ManagerComments = review.ManagerComments,
                EmployeeComments = review.EmployeeComments,
                Status = review.Status,
                SubmittedDate = review.SubmittedDate,
                ApprovedDate = review.ApprovedDate,
                ApprovedBy = review.ApprovedBy
            };
        }

        private PerformanceGoalDto MapGoalToDto(HrPerformanceGoal goal)
        {
            return new PerformanceGoalDto
            {
                GoalId = goal.GoalId,
                EmployeeId = goal.EmployeeId,
                GoalTitle = goal.GoalTitle,
                Description = goal.Description,
                Category = goal.Category,
                Year = goal.Year,
                Quarter = goal.Quarter,
                TargetDate = goal.TargetDate,
                TargetValue = goal.TargetValue,
                TargetUnit = goal.TargetUnit,
                CurrentProgress = goal.CurrentProgress,
                AchievedValue = goal.AchievedValue,
                Weight = goal.Weight,
                Status = goal.Status,
                CompletedDate = goal.CompletedDate,
                AchievementScore = goal.AchievementScore,
                Comments = goal.Comments
            };
        }

        private KpiMetricDto MapKpiToDto(HrKpiMetric metric)
        {
            return new KpiMetricDto
            {
                MetricId = metric.MetricId,
                MetricName = metric.MetricName,
                MetricNameAr = metric.MetricNameAr,
                Description = metric.Description,
                Category = metric.Category,
                DepartmentId = metric.DepartmentId,
                PositionId = metric.PositionId,
                MeasurementUnit = metric.MeasurementUnit,
                Frequency = metric.Frequency,
                TargetValue = metric.TargetValue,
                MinValue = metric.MinValue,
                MaxValue = metric.MaxValue,
                Weight = metric.Weight,
                IsActive = metric.IsActive
            };
        }

        private PerformanceImprovementPlanDto MapPIPToDto(HrPerformanceImprovementPlan pip)
        {
            return new PerformanceImprovementPlanDto
            {
                PipId = pip.PipId,
                EmployeeId = pip.EmployeeId,
                StartDate = pip.StartDate,
                EndDate = pip.EndDate,
                Reason = pip.Reason,
                ImprovementAreas = pip.ImprovementAreas,
                SpecificGoals = pip.SpecificGoals,
                SupportProvided = pip.SupportProvided,
                MilestoneChecks = pip.MilestoneChecks,
                ConsequencesOfFailure = pip.ConsequencesOfFailure,
                CompletionPercentage = pip.CompletionPercentage,
                ProgressUpdates = pip.ProgressUpdates,
                Status = pip.Status,
                CompletedDate = pip.CompletedDate,
                FinalOutcome = pip.FinalOutcome
            };
        }

        private decimal CalculateOverallRating(PerformanceReviewDto review)
        {
            var ratings = new List<decimal?> 
            { 
                review.QualityRating,
                review.ProductivityRating,
                review.KnowledgeRating,
                review.CommunicationRating,
                review.TeamworkRating,
                review.InitiativeRating,
                review.AttendanceRating
            };

            var validRatings = ratings.Where(r => r.HasValue).Select(r => r.Value).ToList();
            return validRatings.Any() ? validRatings.Average() : 0;
        }

        private string DeterminePerformanceLevel(decimal rating)
        {
            if (rating >= 4.5m) return "exceptional";
            if (rating >= 3.5m) return "exceeds";
            if (rating >= 2.5m) return "meets";
            return "needs_improvement";
        }

        private List<decimal> GetQuarterlyRatings(List<HrPerformanceReview> reviews)
        {
            var quarterlyRatings = new List<decimal>();
            for (int q = 1; q <= 4; q++)
            {
                var quarterReviews = reviews.Where(r => r.ReviewQuarter == q).ToList();
                quarterlyRatings.Add(quarterReviews.Any() ? quarterReviews.Average(r => r.OverallRating ?? 0) : 0);
            }
            return quarterlyRatings;
        }

        private Dictionary<string, decimal> GetRatingsByCategory(List<HrPerformanceReview> reviews)
        {
            if (!reviews.Any())
                return new Dictionary<string, decimal>();

            return new Dictionary<string, decimal>
            {
                ["Quality"] = reviews.Average(r => r.QualityRating ?? 0),
                ["Productivity"] = reviews.Average(r => r.ProductivityRating ?? 0),
                ["Knowledge"] = reviews.Average(r => r.KnowledgeRating ?? 0),
                ["Communication"] = reviews.Average(r => r.CommunicationRating ?? 0),
                ["Teamwork"] = reviews.Average(r => r.TeamworkRating ?? 0),
                ["Initiative"] = reviews.Average(r => r.InitiativeRating ?? 0),
                ["Attendance"] = reviews.Average(r => r.AttendanceRating ?? 0)
            };
        }

        private Dictionary<string, int> GetRatingDistribution(List<HrPerformanceReview> reviews)
        {
            return new Dictionary<string, int>
            {
                ["Exceptional (4.5+)"] = reviews.Count(r => r.OverallRating >= 4.5m),
                ["Exceeds (3.5-4.5)"] = reviews.Count(r => r.OverallRating >= 3.5m && r.OverallRating < 4.5m),
                ["Meets (2.5-3.5)"] = reviews.Count(r => r.OverallRating >= 2.5m && r.OverallRating < 3.5m),
                ["Needs Improvement (<2.5)"] = reviews.Count(r => r.OverallRating < 2.5m)
            };
        }

        private async Task<decimal> GetPreviousYearAverageRating(long employeeId, int year)
        {
            var reviews = await Context.Queryable<HrPerformanceReview>()
                .Where(r => r.EmployeeId == employeeId && r.ReviewYear == year && r.Status == "approved")
                .ToListAsync();

            
            return reviews.Any() ? reviews.Average(r => r.OverallRating ?? 0) : 0;
        }

        private async Task<decimal> CalculatePercentileRank(long employeeId, int year, decimal rating)
        {
            var allRatings = await Context.Queryable<HrPerformanceReview>()
                .Where(r => r.ReviewYear == year && r.Status == "approved")
                .Select(r => r.OverallRating)
                .ToListAsync();
            
            if (!allRatings.Any())
                return 0;

            var belowCount = allRatings.Count(r => r < rating);
            return (decimal)belowCount / allRatings.Count * 100;
        }

        private async Task<List<TopPerformerDto>> GetTopPerformersInDepartment(long departmentId, int year, int topCount)
        {
            var departmentEmployees = await _employeeDataService.GetEmployeesByDepartmentAsync(departmentId);
            var employeeIds = departmentEmployees.Select(e => e.EmployeeId).ToList();
            var employeeDict = departmentEmployees.ToDictionary(e => e.EmployeeId);

            // Get all reviews for department employees
            var reviews = await Context.Queryable<HrPerformanceReview>()
                .Where(r => employeeIds.Contains(r.EmployeeId) && r.ReviewYear == year && r.Status == "approved")
                .ToListAsync();

            // Calculate average ratings in memory
            var topPerformers = reviews
                .GroupBy(r => r.EmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    AverageRating = g.Average(r => r.OverallRating ?? 0),
                    ReviewCount = g.Count()
                })
                .OrderByDescending(x => x.AverageRating)
                .Take(topCount)
                .ToList();

            // Get position data for these employees
            var topEmployeeIds = topPerformers.Select(tp => tp.EmployeeId).ToList();
            var positionIds = departmentEmployees
                .Where(e => topEmployeeIds.Contains(e.EmployeeId) && e.PositionId.HasValue)
                .Select(e => e.PositionId.Value)
                .Distinct()
                .ToList();

            var positionsList = await Context.Queryable<SysPost>()
                .Where(p => positionIds.Contains(p.PostId))
                .ToListAsync();
            var positions = positionsList.ToDictionary(p => p.PostId, p => p.PostName);

            return topPerformers.Select(tp =>
            {
                var emp = employeeDict.GetValueOrDefault(tp.EmployeeId);
                string positionName = "";
                if (emp?.PositionId != null && positions.ContainsKey(emp.PositionId.Value))
                {
                    positionName = positions[emp.PositionId.Value];
                }

                return new TopPerformerDto
                {
                    EmployeeId = tp.EmployeeId,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "",
                    Position = positionName,
                    Rating = tp.AverageRating
                };
            }).ToList();
        }

        private List<string> ExtractCommonThemes(List<string> texts)
        {
            // This is a simplified version - in production you might use NLP
            var commonThemes = new List<string>();
            
            if (texts == null || !texts.Any())
                return commonThemes;

            // Simple word frequency analysis
            var words = new Dictionary<string, int>();
            foreach (var text in texts.Where(t => !string.IsNullOrEmpty(t)))
            {
                var textWords = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in textWords.Where(w => w.Length > 4)) // Skip short words
                {
                    if (words.ContainsKey(word))
                        words[word]++;
                    else
                        words[word] = 1;
                }
            }

            // Return top 5 most common words/themes
            return words.OrderByDescending(w => w.Value)
                       .Take(5)
                       .Select(w => w.Key)
                       .ToList();
        }

        #endregion
    }
}
