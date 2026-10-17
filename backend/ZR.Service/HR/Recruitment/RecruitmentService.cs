using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Recruitment;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// Recruitment Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IRecruitmentService), ServiceLifetime = LifeTime.Transient)]
    public class RecruitmentService : BaseService<HrRecruitment>, IRecruitmentService
    {
        #region Job Requisitions

        public async Task<PagedInfo<JobRequisitionDto>> GetRequisitionsAsync(RequisitionQueryDto query)
        {
            var predicate = Expressionable.Create<HrRecruitment>();
            
            if (query.DepartmentId.HasValue)
                predicate = predicate.And(r => r.DepartmentId == query.DepartmentId.Value);
            
            if (!string.IsNullOrEmpty(query.Status))
                predicate = predicate.And(r => r.Status == query.Status);
            
            if (!string.IsNullOrEmpty(query.JobTitle))
                predicate = predicate.And(r => r.JobTitle.Contains(query.JobTitle));

            var list = await Context.Queryable<HrRecruitment>()
                .Where(predicate.ToExpression())
                .OrderBy(r => r.Create_time, OrderByType.Desc)
                .ToPageAsync(query);

            var result = new PagedInfo<JobRequisitionDto>
            {
                TotalNum = list.TotalNum,
                PageIndex = list.PageIndex,
                PageSize = list.PageSize,
                Result = list.Result.Select(MapToRequisitionDto).ToList()
            };

            return result;
        }

        public async Task<JobRequisitionDto> GetRequisitionByIdAsync(long requisitionId)
        {
            var requisition = await GetFirstAsync(r => r.RecruitmentId == requisitionId);
            return requisition != null ? MapToRequisitionDto(requisition) : null;
        }

        public async Task<JobRequisitionDto> CreateRequisitionAsync(JobRequisitionDto requisition)
        {
            var entity = new HrRecruitment
            {
                JobTitle = requisition.JobTitle,
                JobCode = requisition.JobCode,
                DepartmentId = requisition.DepartmentId,
                PositionId = requisition.PositionId,
                ReportingTo = requisition.ReportingTo,
                NumberOfPositions = requisition.NumberOfPositions,
                JobDescription = requisition.JobDescription,
                Requirements = requisition.Requirements,
                SalaryMin = requisition.SalaryMin,
                SalaryMax = requisition.SalaryMax,
                EmploymentType = requisition.EmploymentType,
                Location = requisition.Location,
                ExperienceRequired = requisition.ExperienceRequired,
                EducationLevel = requisition.EducationLevel,
                Skills = requisition.Skills,
                Status = "draft",
                Priority = requisition.Priority,
                TargetStartDate = requisition.TargetStartDate,
                Create_time = DateTime.Now,
                Create_by = requisition.Create_by,
            };

            var result = await InsertAsync(entity);
            if (result>0)
            {
                requisition.RequisitionId = entity.RecruitmentId;
                return requisition;
            }
            return null;
        }

        public async Task<bool> UpdateRequisitionAsync(JobRequisitionDto requisition)
        {
            var entity = await GetFirstAsync(r => r.RecruitmentId == requisition.RequisitionId);
            if (entity == null) return false;

            entity.JobTitle = requisition.JobTitle;
            entity.JobDescription = requisition.JobDescription;
            entity.Requirements = requisition.Requirements;
            entity.SalaryMin = requisition.SalaryMin;
            entity.SalaryMax = requisition.SalaryMax;
            entity.NumberOfPositions = requisition.NumberOfPositions;
            entity.Update_time = DateTime.Now;

            return await Context.Updateable(entity)
                .IgnoreColumns(r => new { r.Create_by, r.Create_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> ApproveRequisitionAsync(long requisitionId, string approvedBy)
        {
            return await Context.Updateable<HrRecruitment>()
                .SetColumns(r => r.Status == "approved")
                .SetColumns(r => r.ApprovedBy == approvedBy)
                .SetColumns(r => r.ApprovedDate == DateTime.Now)
                .SetColumns(r => r.Update_time == DateTime.Now)
                .Where(r => r.RecruitmentId == requisitionId)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> PublishJobAsync(long requisitionId, List<string> jobBoards)
        {
            var requisition = await GetFirstAsync(r => r.RecruitmentId == requisitionId);
            if (requisition == null || requisition.Status != "approved")
                return false;

            requisition.Status = "published";
            requisition.PublishedDate = DateTime.Now;
            requisition.JobBoards = string.Join(",", jobBoards);
            requisition.Update_time = DateTime.Now;

            return await Context.Updateable(requisition)
                .UpdateColumns(r => new { r.Status, r.PublishedDate, r.JobBoards, r.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> CloseRequisitionAsync(long requisitionId, string reason)
        {
            return await Context.Updateable<HrRecruitment>()
                .SetColumns(r => r.Status == "closed")
                .SetColumns(r => r.ClosedDate == DateTime.Now)
                .SetColumns(r => r.ClosedReason == reason)
                .SetColumns(r => r.Update_time == DateTime.Now)
                .Where(r => r.RecruitmentId == requisitionId)
                .ExecuteCommandAsync() > 0;
        }

        #endregion

        #region Candidate Management

        public async Task<PagedInfo<CandidateDto>> GetCandidatesAsync(CandidateQueryDto query)
        {
            var predicate = Expressionable.Create<HrCandidate>();
            
            if (!string.IsNullOrEmpty(query.Name))
                predicate = predicate.And(c => c.FirstName.Contains(query.Name) || c.LastName.Contains(query.Name));
            
            if (!string.IsNullOrEmpty(query.Status))
                predicate = predicate.And(c => c.Status == query.Status);
            
            if (query.RequisitionId.HasValue)
                predicate = predicate.And(c => c.RequisitionId == query.RequisitionId.Value);

            var list = await Context.Queryable<HrCandidate>()
                .Where(predicate.ToExpression())
                .OrderBy(c => c.Create_time, OrderByType.Desc)
                .ToPageAsync(query);

            var result = new PagedInfo<CandidateDto>
            {
                TotalNum = list.TotalNum,
                PageIndex = list.PageIndex,
                PageSize = list.PageSize,
                Result = list.Result.Select(MapToCandidateDto).ToList()
            };

            return result;
        }

        public async Task<CandidateDto> GetCandidateByIdAsync(long candidateId)
        {
            var candidate = await Context.Queryable<HrCandidate>()
                .FirstAsync(c => c.CandidateId == candidateId);
            return candidate != null ? MapToCandidateDto(candidate) : null;
        }

        public async Task<CandidateDto> CreateCandidateAsync(CandidateDto candidate)
        {
            var entity = new HrCandidate
            {
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Email = candidate.Email,
                Phone = candidate.Phone,
                RequisitionId = candidate.RequisitionId,
                Source = candidate.Source,
                CurrentCompany = candidate.CurrentCompany,
                CurrentPosition = candidate.CurrentPosition,
                TotalExperience = candidate.TotalExperience,
                CurrentSalary = candidate.CurrentSalary,
                ExpectedSalary = candidate.ExpectedSalary,
                NoticePeriod = candidate.NoticePeriod,
                Skills = candidate.Skills,
                Status = "new",
                Stage = "screening",
                Create_time = DateTime.Now
            };

            var result = await Context.Insertable(entity).ExecuteCommandAsync();
            if (result > 0)
            {
                candidate.CandidateId = entity.CandidateId;
                return candidate;
            }
            return null;
        }

        public async Task<bool> UpdateCandidateAsync(CandidateDto candidate)
        {
            var entity = await Context.Queryable<HrCandidate>()
                .FirstAsync(c => c.CandidateId == candidate.CandidateId);
            
            if (entity == null) return false;

            entity.FirstName = candidate.FirstName;
            entity.LastName = candidate.LastName;
            entity.Email = candidate.Email;
            entity.Phone = candidate.Phone;
            entity.Update_time = DateTime.Now;

            return await Context.Updateable(entity)
                .IgnoreColumns(c => new { c.Create_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> UploadResumeAsync(long candidateId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var candidate = await Context.Queryable<HrCandidate>()
                .FirstAsync(c => c.CandidateId == candidateId);
            
            if (candidate == null) return false;

            // Save file to disk
            var fileName = $"{candidateId}_{DateTime.Now:yyyyMMddHHmmss}_{file.FileName}";
            var filePath = Path.Combine("uploads", "resumes", fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            candidate.ResumePath = filePath;
            candidate.Update_time = DateTime.Now;

            return await Context.Updateable(candidate)
                .UpdateColumns(c => new { c.ResumePath, c.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> UpdateCandidateStatusAsync(long candidateId, string status, string notes)
        {
            var candidate = await Context.Queryable<HrCandidate>()
                .FirstAsync(c => c.CandidateId == candidateId);
            
            if (candidate == null) return false;

            // Save status history
            var history = new HrCandidateHistory
            {
                CandidateId = candidateId,
                Stage = candidate.Stage,
                Status = status,
                Notes = notes,
                Create_time = DateTime.Now
            };
            await Context.Insertable(history).ExecuteCommandAsync();

            // Update candidate status
            candidate.Status = status;
            candidate.Update_time = DateTime.Now;

            return await Context.Updateable(candidate)
                .UpdateColumns(c => new { c.Status, c.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<List<CandidateDto>> SearchCandidatesAsync(string skills, string experience)
        {
            var query = Context.Queryable<HrCandidate>();

            if (!string.IsNullOrEmpty(skills))
            {
                var skillList = skills.Split(',').Select(s => s.Trim()).ToList();
                query = query.Where(c => skillList.Any(s => c.Skills.Contains(s)));
            }

            if (!string.IsNullOrEmpty(experience))
            {
                if (int.TryParse(experience, out int years))
                {
                    query = query.Where(c => c.TotalExperience >= years);
                }
            }

            var candidates = await query.ToListAsync();
            return candidates.Select(MapToCandidateDto).ToList();
        }

        #endregion

        #region Interview Management

        public async Task<List<InterviewDto>> GetInterviewsAsync(InterviewQueryDto query)
        {
            var predicate = Expressionable.Create<HrInterview>();
            
            if (query.CandidateId.HasValue)
                predicate = predicate.And(i => i.CandidateId == query.CandidateId.Value);
            
            if (query.InterviewerId.HasValue)
                predicate = predicate.And(i => i.InterviewerId == query.InterviewerId.Value);
            
            if (!string.IsNullOrEmpty(query.Status))
                predicate = predicate.And(i => i.Status == query.Status);
            
            if (query.StartDate.HasValue)
                predicate = predicate.And(i => i.InterviewDate >= query.StartDate.Value);
            
            if (query.EndDate.HasValue)
                predicate = predicate.And(i => i.InterviewDate <= query.EndDate.Value);

            var interviews = await Context.Queryable<HrInterview>()
                .Where(predicate.ToExpression())
                .OrderBy(i => i.InterviewDate)
                .ToListAsync();

            return interviews.Select(MapToInterviewDto).ToList();
        }

        public async Task<InterviewDto> ScheduleInterviewAsync(InterviewDto interview)
        {
            var entity = new HrInterview
            {
                CandidateId = interview.CandidateId,
                RequisitionId = interview.RequisitionId,
                InterviewerId = interview.InterviewerId,
                InterviewDate = interview.InterviewDate,
                InterviewType = interview.InterviewType,
                InterviewRound = interview.InterviewRound,
                Location = interview.Location,
                Duration = interview.Duration,
                Status = "scheduled",
                Create_time = DateTime.Now
            };

            var result = await Context.Insertable(entity).ExecuteCommandAsync();
            if (result > 0)
            {
                interview.InterviewId = entity.InterviewId;
                
                // Send notifications to candidate and interviewer
                await SendInterviewNotifications(entity);
                
                return interview;
            }
            return null;
        }

        public async Task<bool> UpdateInterviewAsync(InterviewDto interview)
        {
            var entity = await Context.Queryable<HrInterview>()
                .FirstAsync(i => i.InterviewId == interview.InterviewId);
            
            if (entity == null) return false;

            entity.InterviewDate = interview.InterviewDate;
            entity.Location = interview.Location;
            entity.Duration = interview.Duration;
            entity.Update_time = DateTime.Now;

            return await Context.Updateable(entity)
                .IgnoreColumns(i => new { i.Create_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> CancelInterviewAsync(long interviewId, string reason)
        {
            return await Context.Updateable<HrInterview>()
                .SetColumns(i => i.Status == "cancelled")
                .SetColumns(i => i.CancelReason == reason)
                .SetColumns(i => i.Update_time == DateTime.Now)
                .Where(i => i.InterviewId == interviewId)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> SubmitInterviewFeedbackAsync(long interviewId, InterviewFeedbackDto feedback)
        {
            var interview = await Context.Queryable<HrInterview>()
                .FirstAsync(i => i.InterviewId == interviewId);
            
            if (interview == null) return false;

            interview.Feedback = feedback.Feedback;
            interview.Rating = feedback.Rating;
            interview.Recommendation = feedback.Recommendation;
            interview.Status = "completed";
            interview.Update_time = DateTime.Now;

            return await Context.Updateable(interview)
                .UpdateColumns(i => new { i.Feedback, i.Rating, i.Recommendation, i.Status, i.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<List<InterviewDto>> GetUpcomingInterviewsAsync(long? interviewerId = null, int days = 7)
        {
            var endDate = DateTime.Now.AddDays(days);
            var query = Context.Queryable<HrInterview>()
                .Where(i => i.InterviewDate >= DateTime.Now && i.InterviewDate <= endDate && i.Status == "scheduled");

            if (interviewerId.HasValue)
            {
                query = query.Where(i => i.InterviewerId == interviewerId.Value);
            }

            var interviews = await query.OrderBy(i => i.InterviewDate).ToListAsync();
            return interviews.Select(MapToInterviewDto).ToList();
        }

        #endregion

        #region Offer Management

        public async Task<List<JobOfferDto>> GetOffersAsync(long? candidateId = null, string status = null)
        {
            var query = Context.Queryable<HrJobOffer>();

            if (candidateId.HasValue)
                query = query.Where(o => o.CandidateId == candidateId.Value);
            
            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            var offers = await query.OrderBy(o => o.Create_time, OrderByType.Desc).ToListAsync();
            return offers.Select(MapToOfferDto).ToList();
        }

        public async Task<JobOfferDto> CreateOfferAsync(JobOfferDto offer)
        {
            var entity = new HrJobOffer
            {
                CandidateId = offer.CandidateId,
                RequisitionId = offer.RequisitionId,
                PositionTitle = offer.PositionTitle,
                Department = offer.Department,
                BasicSalary = offer.BasicSalary,
                HousingAllowance = (decimal)offer.HousingAllowance,
                TransportAllowance = offer.TransportAllowance,
                OtherAllowances = offer.OtherAllowances,
                TotalPackage = offer.TotalPackage,
                JoiningDate = offer.JoiningDate,
                ValidUntil = offer.ValidUntil,
                Status = "draft",
                Create_time = DateTime.Now
            };

            var result = await Context.Insertable(entity).ExecuteCommandAsync();
            if (result > 0)
            {
                offer.OfferId = entity.OfferId;
                return offer;
            }
            return null;
        }

        public async Task<bool> UpdateOfferAsync(JobOfferDto offer)
        {
            var entity = await Context.Queryable<HrJobOffer>()
                .FirstAsync(o => o.OfferId == offer.OfferId);
            
            if (entity == null) return false;

            entity.BasicSalary = offer.BasicSalary;
            entity.HousingAllowance = offer.HousingAllowance;
            entity.TransportAllowance = offer.TransportAllowance;
            entity.OtherAllowances = offer.OtherAllowances;
            entity.TotalPackage = offer.TotalPackage;
            entity.JoiningDate = offer.JoiningDate;
            entity.Update_time = DateTime.Now;

            return await Context.Updateable(entity)
                .IgnoreColumns(o => new { o.Create_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> SendOfferAsync(long offerId)
        {
            var offer = await Context.Queryable<HrJobOffer>()
                .FirstAsync(o => o.OfferId == offerId);
            
            if (offer == null) return false;

            offer.Status = "sent";
            offer.SentDate = DateTime.Now;
            offer.Update_time = DateTime.Now;

            // Generate offer letter and send email
            // await GenerateAndSendOfferLetter(offer);

            return await Context.Updateable(offer)
                .UpdateColumns(o => new { o.Status, o.SentDate, o.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> AcceptOfferAsync(long offerId, DateTime joiningDate)
        {
            var offer = await Context.Queryable<HrJobOffer>()
                .FirstAsync(o => o.OfferId == offerId);
            
            if (offer == null) return false;

            offer.Status = "accepted";
            offer.AcceptedDate = DateTime.Now;
            offer.JoiningDate = joiningDate;
            offer.Update_time = DateTime.Now;

            return await Context.Updateable(offer)
                .UpdateColumns(o => new { o.Status, o.AcceptedDate, o.JoiningDate, o.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> RejectOfferAsync(long offerId, string reason)
        {
            return await Context.Updateable<HrJobOffer>()
                .SetColumns(o => o.Status == "rejected")
                .SetColumns(o => o.RejectedDate == DateTime.Now)
                .SetColumns(o => o.RejectionReason == reason)
                .SetColumns(o => o.Update_time == DateTime.Now)
                .Where(o => o.OfferId == offerId)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> NegotiateOfferAsync(long offerId, decimal proposedSalary, string comments)
        {
            var offer = await Context.Queryable<HrJobOffer>()
                .FirstAsync(o => o.OfferId == offerId);
            
            if (offer == null) return false;

            // Create negotiation record
            var negotiation = new HrOfferNegotiation
            {
                OfferId = offerId,
                ProposedSalary = proposedSalary,
                Comments = comments,
                Create_time = DateTime.Now
            };
            await Context.Insertable(negotiation).ExecuteCommandAsync();

            offer.Status = "negotiating";
            offer.Update_time = DateTime.Now;

            return await Context.Updateable(offer)
                .UpdateColumns(o => new { o.Status, o.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        #endregion

        #region Pipeline Management

        public async Task<bool> MoveCandidateToStageAsync(long candidateId, long requisitionId, string stage)
        {
            var candidate = await Context.Queryable<HrCandidate>()
                .FirstAsync(c => c.CandidateId == candidateId);
            
            if (candidate == null) return false;

            // Save stage history
            var history = new HrCandidateHistory
            {
                CandidateId = candidateId,
                Stage = stage,
                Status = candidate.Status,
                Create_time = DateTime.Now
            };
            await Context.Insertable(history).ExecuteCommandAsync();

            // Update candidate stage
            candidate.Stage = stage;
            candidate.Update_time = DateTime.Now;

            return await Context.Updateable(candidate)
                .UpdateColumns(c => new { c.Stage, c.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        public async Task<RecruitmentPipelineDto> GetPipelineStatusAsync(long requisitionId)
        {
            var candidates = await Context.Queryable<HrCandidate>()
                .Where(c => c.RequisitionId == requisitionId)
                .ToListAsync();

            var pipeline = new RecruitmentPipelineDto
            {
                RequisitionId = requisitionId,
                TotalCandidates = candidates.Count,
                
                //Stages = new List<PipelineStageDto>
                //{
                //    new PipelineStageDto { Stage = "screening", Count = candidates.Count(c => c.Stage == "screening") },
                //    new PipelineStageDto { Stage = "phone_interview", Count = candidates.Count(c => c.Stage == "phone_interview") },
                //    new PipelineStageDto { Stage = "technical_interview", Count = candidates.Count(c => c.Stage == "technical_interview") },
                //    new PipelineStageDto { Stage = "hr_interview", Count = candidates.Count(c => c.Stage == "hr_interview") },
                //    new PipelineStageDto { Stage = "offer", Count = candidates.Count(c => c.Stage == "offer") },
                //    new PipelineStageDto { Stage = "hired", Count = candidates.Count(c => c.Stage == "hired") }
                //}
            };

            return pipeline;
        }

        public async Task<List<CandidateStageHistoryDto>> GetCandidateHistoryAsync(long candidateId)
        {
            var history = await Context.Queryable<HrCandidateHistory>()
                .Where(h => h.CandidateId == candidateId)
                .OrderBy(h => h.Create_time)
                .ToListAsync();

            return history.Select(h => new CandidateStageHistoryDto
            {
                Stage = h.Stage,
                Status = h.Status,
                Notes = h.Notes,
                Date = h.Create_time
            }).ToList();
        }

        #endregion

        #region Analytics and Reporting

        public async Task<RecruitmentMetricsDto> GetRecruitmentMetricsAsync(DateTime startDate, DateTime endDate)
        {
            var requisitions = await Context.Queryable<HrRecruitment>()
                .Where(r => r.Create_time >= startDate && r.Create_time <= endDate)
                .ToListAsync();

            var candidates = await Context.Queryable<HrCandidate>()
                .Where(c => c.Create_time >= startDate && c.Create_time <= endDate)
                .ToListAsync();

            var offers = await Context.Queryable<HrJobOffer>()
                .Where(o => o.Create_time >= startDate && o.Create_time <= endDate)
                .ToListAsync();

            return new RecruitmentMetricsDto
            {
                TotalRequisitions = requisitions.Count,
                OpenPositions = requisitions.Where(r => r.Status == "published").Sum(r => r.NumberOfPositions),
                TotalCandidates = candidates.Count,
                TotalOffers = offers.Count,
                AcceptedOffers = offers.Count(o => o.Status == "accepted"),
                RejectedOffers = offers.Count(o => o.Status == "rejected"),
                OfferAcceptanceRate = offers.Any() ? (decimal)offers.Count(o => o.Status == "accepted") / offers.Count * 100 : 0,
                AverageTimeToHire = CalculateAverageTimeToHire(requisitions, offers)
            };
        }

        public async Task<SourceAnalyticsDto> GetSourceAnalyticsAsync(int year, int? quarter = null)
        {
            var query = Context.Queryable<HrCandidate>()
                .Where(c => c.Create_time.Year == year);

            if (quarter.HasValue)
            {
                var startMonth = (quarter.Value - 1) * 3 + 1;
                var endMonth = startMonth + 2;
                query = query.Where(c => c.Create_time.Month >= startMonth && c.Create_time.Month <= endMonth);
            }

            var candidates = await query.ToListAsync();
            var sources = candidates.GroupBy(c => c.Source ?? "Unknown")
                .Select(g => new SourcePerformance
                {
                    Source = g.Key,
                    Applications = g.Count(),
                    Interviews = g.Count(c => c.Stage == "phone_interview" || c.Stage == "technical_interview" || c.Stage == "hr_interview"),
                    Offers = g.Count(c => c.Stage == "offer"),
                    Hires = g.Count(c => c.Stage == "hired"),
                    ConversionRate = g.Any() ? (decimal)g.Count(c => c.Stage == "hired") / g.Count() * 100 : 0,
                    QualityScore = 0 // You can implement your own logic for quality score
                })
                .ToList();

            var bestSource = sources.OrderByDescending(s => s.ConversionRate).FirstOrDefault();

            return new SourceAnalyticsDto
            {
                Year = year,
                Quarter = quarter,
                Sources = sources,
                BestSource = bestSource?.Source,
                BestSourceConversionRate = bestSource?.ConversionRate ?? 0
            };
        }

        public async Task<TimeToHireDto> GetTimeToHireReportAsync(long? departmentId = null, int months = 6)
        {
            var startDate = DateTime.Now.AddMonths(-months);
            
            var query = Context.Queryable<HrRecruitment>()
                .Where(r => r.Create_time >= startDate && r.Status == "closed");

            if (departmentId.HasValue)
            {
                query = query.Where(r => r.DepartmentId == departmentId.Value);
            }

            var requisitions = await query.ToListAsync();
            
            var report = new TimeToHireDto
            {
                AverageDays = CalculateAverageTimeToHire(requisitions, null),
                MinDays = 0,
                MaxDays = 0,
                MedianDays = 0
            };

            return report;
        }

        public async Task<InterviewerPerformanceDto> GetInterviewerPerformanceAsync(long interviewerId, int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-months);
            
            var interviews = await Context.Queryable<HrInterview>()
                .Where(i => i.InterviewerId == interviewerId && i.InterviewDate >= startDate)
                .ToListAsync();

            return new InterviewerPerformanceDto
            {
                InterviewerId = interviewerId,
                TotalInterviews = interviews.Count,
                CompletedInterviews = interviews.Count(i => i.Status == "completed"),
                //AverageRating = interviews.Where(i => i.Rating.HasValue).Any() ? 
                //    interviews.Where(i => i.Rating.HasValue).Average(i => i.Rating.Value) : 0,
                //AverageRating = interviews.Where(i => i.Rating.HasValue).Any() ?
                //    (decimal)interviews.Where(i => i.Rating.HasValue).Average(i => i.Rating.Value) : 0,
                RecommendedHires = interviews.Count(i => i.Recommendation == "hire"),
                ActualHires = 0 // Would need to join with candidate data
            };
        }

        #endregion

        #region Onboarding

        public async Task<OnboardingChecklistDto> GetOnboardingChecklistAsync(long candidateId)
        {
            var tasks = await Context.Queryable<HrOnboardingTask>()
                .Where(t => t.CandidateId == candidateId)
                .OrderBy(t => t.Sequence)
                .ToListAsync();

            return new OnboardingChecklistDto
            {
                CandidateId = candidateId,
                //Tasks = tasks.Select(t => new OnboardingTaskDto
                //{
                //    TaskId = t.TaskId,
                //    TaskName = t.TaskName,
                //    Description = t.Description,
                //    AssignedTo = t.AssignedTo,
                //    DueDate = t.DueDate,
                //    Completed = t.Completed,
                //    CompletedDate = t.CompletedDate,
                //    Notes = t.Notes
                //}).ToList(),
                CompletionRate = tasks.Any() ? (decimal)tasks.Count(t => t.Completed) / tasks.Count * 100 : 0
            };
        }

        public async Task<bool> UpdateOnboardingTaskAsync(long taskId, bool completed, string notes)
        {
            return await Context.Updateable<HrOnboardingTask>()
                .SetColumns(t => t.Completed == completed)
                //.SetColumns(t => t.CompletedDate == completed ? DateTime.Now : (DateTime?)null)
                .SetColumns(t => t.Notes == notes)
                .SetColumns(t => t.Update_time == DateTime.Now)
                .Where(t => t.TaskId == taskId)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> CompleteOnboardingAsync(long candidateId, long employeeId)
        {
            // Update candidate status
            await Context.Updateable<HrCandidate>()
                .SetColumns(c => c.Stage == "hired")
                .SetColumns(c => c.Status == "converted")
                .SetColumns(c => c.EmployeeId == employeeId)
                .SetColumns(c => c.Update_time == DateTime.Now)
                .Where(c => c.CandidateId == candidateId)
                .ExecuteCommandAsync();

            // Complete all onboarding tasks
            await Context.Updateable<HrOnboardingTask>()
                .SetColumns(t => t.Completed == true)
                .SetColumns(t => t.CompletedDate == DateTime.Now)
                .Where(t => t.CandidateId == candidateId && !t.Completed)
                .ExecuteCommandAsync();

            return true;
        }

        #endregion

        #region Talent Pool

        public async Task<bool> AddToTalentPoolAsync(long candidateId, string category, string notes)
        {
            var talentPool = new HrTalentPool
            {
                CandidateId = candidateId,
                Category = category,
                Notes = notes,
                AddedDate = DateTime.Now,
                IsActive = true
            };

            return await Context.Insertable(talentPool).ExecuteCommandAsync() > 0;
        }

        public async Task<List<CandidateDto>> GetTalentPoolAsync(string category = null, string skills = null)
        {
            var query = Context.Queryable<HrTalentPool>()
                .LeftJoin<HrCandidate>((tp, c) => tp.CandidateId == c.CandidateId)
                .Where((tp, c) => tp.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where((tp, c) => tp.Category == category);
            }

            if (!string.IsNullOrEmpty(skills))
            {
                var skillList = skills.Split(',').Select(s => s.Trim()).ToList();
                query = query.Where((tp, c) => skillList.Any(s => c.Skills.Contains(s)));
            }

            var candidates = await query.Select((tp, c) => c).ToListAsync();
            return candidates.Select(MapToCandidateDto).ToList();
        }

        public async Task<bool> ReachOutToCandidateAsync(long candidateId, string subject, string message)
        {
            var candidate = await Context.Queryable<HrCandidate>()
                .FirstAsync(c => c.CandidateId == candidateId);
            
            if (candidate == null || string.IsNullOrEmpty(candidate.Email))
                return false;

            // Send email to candidate
            // await _emailService.SendAsync(candidate.Email, subject, message);

            // Log the communication
            var communication = new HrCandidateCommunication
            {
                CandidateId = candidateId,
                Type = "email",
                Subject = subject,
                Message = message,
                SentDate = DateTime.Now
            };
            await Context.Insertable(communication).ExecuteCommandAsync();

            return true;
        }

        #endregion

        #region Helper Methods

        private JobRequisitionDto MapToRequisitionDto(HrRecruitment requisition)
        {
            return new JobRequisitionDto
            {
                RequisitionId = requisition.RecruitmentId,
                JobTitle = requisition.JobTitle,
                JobCode = requisition.JobCode,
                DepartmentId = requisition.DepartmentId,
                PositionId = requisition.PositionId,
                ReportingTo = requisition.ReportingTo,
                NumberOfPositions = requisition.NumberOfPositions,
                JobDescription = requisition.JobDescription,
                Requirements = requisition.Requirements,
                SalaryMin = requisition.SalaryMin,
                SalaryMax = requisition.SalaryMax,
                EmploymentType = requisition.EmploymentType,
                Location = requisition.Location,
                ExperienceRequired = requisition.ExperienceRequired,
                EducationLevel = requisition.EducationLevel,
                Skills = requisition.Skills,
                Status = requisition.Status,
                Priority = requisition.Priority,
                TargetStartDate = requisition.TargetStartDate
            };
        }

        private CandidateDto MapToCandidateDto(HrCandidate candidate)
        {
            return new CandidateDto
            {
                CandidateId = candidate.CandidateId,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Email = candidate.Email,
                Phone = candidate.Phone,
                RequisitionId = candidate.RequisitionId,
                Source = candidate.Source,
                CurrentCompany = candidate.CurrentCompany,
                CurrentPosition = candidate.CurrentPosition,
                TotalExperience = candidate.TotalExperience,
                CurrentSalary = candidate.CurrentSalary,
                ExpectedSalary = candidate.ExpectedSalary,
                NoticePeriod = candidate.NoticePeriod,
                Skills = candidate.Skills,
                Status = candidate.Status,
                Stage = candidate.Stage
            };
        }

        private InterviewDto MapToInterviewDto(HrInterview interview)
        {
            return new InterviewDto
            {
                InterviewId = interview.InterviewId,
                CandidateId = interview.CandidateId,
                RequisitionId = interview.RequisitionId,
                InterviewerId = interview.InterviewerId,
                InterviewDate = interview.InterviewDate,
                InterviewType = interview.InterviewType,
                InterviewRound = interview.InterviewRound,
                Location = interview.Location,
                Duration = interview.Duration,
                Status = interview.Status,
                Feedback = interview.Feedback,
                Rating = interview.Rating,
                Recommendation = interview.Recommendation
            };
        }

        private JobOfferDto MapToOfferDto(HrJobOffer offer)
        {
            return new JobOfferDto
            {
                OfferId = offer.OfferId,
                CandidateId = offer.CandidateId,
                RequisitionId = offer.RequisitionId,
                PositionTitle = offer.PositionTitle,
                Department = offer.Department,
                BasicSalary = offer.BasicSalary,
                HousingAllowance = offer.HousingAllowance,
                TransportAllowance = offer.TransportAllowance,
                OtherAllowances = offer.OtherAllowances,
                TotalPackage = offer.TotalPackage,
                JoiningDate = offer.JoiningDate,
                ValidUntil = offer.ValidUntil,
                Status = offer.Status
            };
        }

        private decimal CalculateAverageTimeToHire(List<HrRecruitment> requisitions, List<HrJobOffer> offers)
        {
            var completedRequisitions = requisitions.Where(r => r.ClosedDate.HasValue).ToList();
            if (!completedRequisitions.Any()) return 0;

            var totalDays = completedRequisitions.Sum(r => (r.ClosedDate.Value - r.Create_time).Days);
            return totalDays / completedRequisitions.Count;
        }

        private async Task SendInterviewNotifications(HrInterview interview)
        {
            // Implementation for sending email/SMS notifications
            // This would integrate with your notification service
        }

        #endregion
    }
}
