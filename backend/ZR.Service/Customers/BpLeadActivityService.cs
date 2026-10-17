using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Mapster;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Common;
using ZR.Model;
using ZR.Model.Customers.Dto;
using ZR.Model.Customers.Leads;
using ZR.Repository;
using ZR.Repository.Customers;
using ZR.Service.Customers.ICustomersService;

namespace ZR.Service.Customers
{
    /// <summary>
    /// Lead Activity Service - Track all lead interactions
    /// </summary>
    [AppService(ServiceType = typeof(IBpLeadActivityService), ServiceLifetime = LifeTime.Transient)]
    public class BpLeadActivityService : BaseService<BpLeadActivity>, IBpLeadActivityService
    {
        private readonly BpLeadActivityRepository _activityRepo;
        private readonly BpLeadRepository _leadRepo;

        public BpLeadActivityService(
            BpLeadActivityRepository activityRepo,
            BpLeadRepository leadRepo)
        {
            _activityRepo = activityRepo;
            _leadRepo = leadRepo;
        }

        public PagedInfo<LeadActivityDto> GetActivityList(LeadActivityQueryDto queryDto)
        {
            var predicate = Expressionable.Create<BpLeadActivity>();

            if (queryDto.LeadId.HasValue)
                predicate = predicate.And(a => a.LeadId == queryDto.LeadId.Value);
            if (queryDto.EmployeeId.HasValue)
                predicate = predicate.And(a => a.EmployeeId == queryDto.EmployeeId.Value);
            if (!string.IsNullOrEmpty(queryDto.ActivityType))
                predicate = predicate.And(a => a.ActivityType == queryDto.ActivityType);
            if (queryDto.StartDate.HasValue)
                predicate = predicate.And(a => a.ActivityDate >= queryDto.StartDate.Value);
            if (queryDto.EndDate.HasValue)
                predicate = predicate.And(a => a.ActivityDate <= queryDto.EndDate.Value);
            if (!string.IsNullOrEmpty(queryDto.Outcome))
                predicate = predicate.And(a => a.Outcome == queryDto.Outcome);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(a => a.ActivityDate, OrderByType.Desc)
                .ToPage<BpLeadActivity, LeadActivityDto>(queryDto);

            EnrichActivityDtos(response.Result);
            return response;
        }

        public LeadActivityDto GetActivityById(long activityId)
        {
            var activity = _activityRepo.GetId(activityId);
            if (activity == null) return null;

            var activityDto = activity.Adapt<LeadActivityDto>();
            EnrichActivityDto(activityDto, activity.LeadId, activity.EmployeeId);
            return activityDto;
        }

        public LeadActivityDto CreateActivity(CreateLeadActivityDto createDto)
        {
            var lead = _leadRepo.GetId(createDto.LeadId);
            if (lead == null)
                throw new Exception("Lead not found");

            var activity = createDto.Adapt<BpLeadActivity>();
            activity.ActivityId = SnowFlakeSingle.Instance.NextId();
            activity.Create_time = DateTime.Now;
            activity.IsAutomated = false;

            UseTran(() =>
            {
                _activityRepo.Insert(activity);

                if (!lead.FirstContactDate.HasValue)
                {
                    lead.FirstContactDate = activity.ActivityDate;
                    _leadRepo.Update(lead, true);
                }

                lead.LastContactDate = activity.ActivityDate;
                _leadRepo.Update(lead, true);
            });

            return GetActivityById(activity.ActivityId);
        }

        public List<LeadActivityDto> GetLeadActivityTimeline(long leadId)
        {
            var activities = Context.Queryable<BpLeadActivity>()
                .Where(a => a.LeadId == leadId)
                .OrderBy(a => a.ActivityDate, OrderByType.Desc)
                .ToList();

            var activityDtos = activities.Adapt<List<LeadActivityDto>>();
            EnrichActivityDtos(activityDtos);
            return activityDtos;
        }

        public List<LeadActivityDto> GetRecentActivitiesByEmployee(long employeeId, int limit = 10)
        {
            var activities = Context.Queryable<BpLeadActivity>()
                .Where(a => a.EmployeeId == employeeId)
                .OrderBy(a => a.ActivityDate, OrderByType.Desc)
                .Take(limit)
                .ToList();

            var activityDtos = activities.Adapt<List<LeadActivityDto>>();
            EnrichActivityDtos(activityDtos);
            return activityDtos;
        }

        public List<LeadActivityDto> GetActivitiesByType(string activityType, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = Context.Queryable<BpLeadActivity>()
                .Where(a => a.ActivityType == activityType);

            if (startDate.HasValue)
                query = query.Where(a => a.ActivityDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.ActivityDate <= endDate.Value);

            var activities = query.OrderBy(a => a.ActivityDate, OrderByType.Desc).ToList();
            var activityDtos = activities.Adapt<List<LeadActivityDto>>();
            EnrichActivityDtos(activityDtos);
            return activityDtos;
        }

        public List<LeadActivityDto> GetUpcomingFollowUps(long? employeeId = null, int daysAhead = 7)
        {
            var now = DateTime.Now;
            var endDate = now.AddDays(daysAhead);

            var query = Context.Queryable<BpLeadActivity>()
                .Where(a => a.NextFollowUpDate.HasValue)
                .Where(a => a.NextFollowUpDate.Value >= now && a.NextFollowUpDate.Value <= endDate);

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            var activities = query.OrderBy(a => a.NextFollowUpDate, OrderByType.Asc).ToList();
            var activityDtos = activities.Adapt<List<LeadActivityDto>>();
            EnrichActivityDtos(activityDtos);
            return activityDtos;
        }

        public List<LeadActivityDto> GetOverdueFollowUps(long? employeeId = null)
        {
            var now = DateTime.Now;
            var query = Context.Queryable<BpLeadActivity>()
                .Where(a => a.NextFollowUpDate.HasValue)
                .Where(a => a.NextFollowUpDate.Value < now);

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            var activities = query.OrderBy(a => a.NextFollowUpDate, OrderByType.Asc).ToList();
            var activityDtos = activities.Adapt<List<LeadActivityDto>>();
            EnrichActivityDtos(activityDtos);
            return activityDtos;
        }

        public ActivityStatisticsDto GetEmployeeActivityStatistics(long employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = Context.Queryable<BpLeadActivity>()
                .Where(a => a.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(a => a.ActivityDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.ActivityDate <= endDate.Value);

            var activities = query.ToList();
            var now = DateTime.Now;

            return new ActivityStatisticsDto
            {
                TotalActivities = activities.Count,
                TotalCalls = activities.Count(a => a.ActivityType == "Call"),
                TotalMeetings = activities.Count(a => a.ActivityType == "Meeting"),
                TotalEmails = activities.Count(a => a.ActivityType == "Email"),
                TotalSMS = activities.Count(a => a.ActivityType == "SMS"),
                TotalWhatsApp = activities.Count(a => a.ActivityType == "WhatsApp"),
                TotalNotes = activities.Count(a => a.ActivityType == "Note"),
                TotalFollowUps = activities.Count(a => a.ActivityType == "Follow-up"),
                SuccessfulContacts = activities.Count(a => a.Outcome == "Successful" || a.Outcome == "Scheduled Meeting"),
                FailedContacts = activities.Count(a => a.Outcome == "No Answer" || a.Outcome == "Not Interested"),
                AvgDurationMinutes = activities.Where(a => a.DurationMinutes.HasValue).Any()
                    ? (decimal)activities.Where(a => a.DurationMinutes.HasValue).Average(a => a.DurationMinutes.Value)
                    : 0,
                UpcomingFollowUps = activities.Count(a => a.NextFollowUpDate.HasValue && a.NextFollowUpDate.Value >= now),
                OverdueFollowUps = activities.Count(a => a.NextFollowUpDate.HasValue && a.NextFollowUpDate.Value < now)
            };
        }

        public int DeleteActivity(long activityId)
        {
            var result = _activityRepo.Delete(activityId);
            if (result == 0) throw new Exception("Activity not found");
            return result;
        }

        private void EnrichActivityDtos(List<LeadActivityDto> activityDtos)
        {
            foreach (var dto in activityDtos)
                EnrichActivityDto(dto, dto.LeadId, dto.EmployeeId);
        }

        private void EnrichActivityDto(LeadActivityDto dto, long leadId, long employeeId)
        {
            var lead = _leadRepo.GetId(leadId);
            dto.LeadName = lead?.BrandName ?? "Unknown Lead";
            dto.EmployeeName = $"Employee {employeeId}";
        }
    }
}
