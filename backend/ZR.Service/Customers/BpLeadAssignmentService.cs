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
    /// Lead Assignment Service
    /// </summary>
    [AppService(ServiceType = typeof(IBpLeadAssignmentService), ServiceLifetime = LifeTime.Transient)]
    public class BpLeadAssignmentService : BaseService<BpLeadAssignmentLog>, IBpLeadAssignmentService
    {
        private readonly BpLeadAssignmentLogRepository _assignmentRepo;
        private readonly BpLeadRepository _leadRepo;

        public BpLeadAssignmentService(
            BpLeadAssignmentLogRepository assignmentRepo,
            BpLeadRepository leadRepo)
        {
            _assignmentRepo = assignmentRepo;
            _leadRepo = leadRepo;
        }

        public PagedInfo<LeadAssignmentLogDto> GetAssignmentHistory(LeadAssignmentQueryDto queryDto)
        {
            var predicate = Expressionable.Create<BpLeadAssignmentLog>();

            if (queryDto.LeadId.HasValue)
                predicate = predicate.And(a => a.LeadId == queryDto.LeadId.Value);
            if (queryDto.FromEmployeeId.HasValue)
                predicate = predicate.And(a => a.FromEmployeeId == queryDto.FromEmployeeId.Value);
            if (queryDto.ToEmployeeId.HasValue)
                predicate = predicate.And(a => a.ToEmployeeId == queryDto.ToEmployeeId.Value);
            if (queryDto.StartDate.HasValue)
                predicate = predicate.And(a => a.AssignmentDate >= queryDto.StartDate.Value);
            if (queryDto.EndDate.HasValue)
                predicate = predicate.And(a => a.AssignmentDate <= queryDto.EndDate.Value);
            if (!string.IsNullOrEmpty(queryDto.ReassignmentType))
                predicate = predicate.And(a => a.ReassignmentType == queryDto.ReassignmentType);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(a => a.AssignmentDate, OrderByType.Desc)
                .ToPage<BpLeadAssignmentLog, LeadAssignmentLogDto>(queryDto);

            EnrichAssignmentDtos(response.Result);
            return response;
        }

        public List<LeadAssignmentLogDto> GetLeadAssignmentHistory(long leadId)
        {
            var assignments = Context.Queryable<BpLeadAssignmentLog>()
                .Where(a => a.LeadId == leadId)
                .OrderBy(a => a.AssignmentDate, OrderByType.Desc)
                .ToList();

            var assignmentDtos = assignments.Adapt<List<LeadAssignmentLogDto>>();
            EnrichAssignmentDtos(assignmentDtos);
            return assignmentDtos;
        }

        public LeadAssignmentLogDto AssignLead(AssignLeadDto assignDto)
        {
            var lead = _leadRepo.GetId(assignDto.LeadId);
            if (lead == null) throw new Exception("Lead not found");

            var isInitialAssignment = lead.AssignedTo == 0;

            var assignment = new BpLeadAssignmentLog
            {
                AssignmentLogId = SnowFlakeSingle.Instance.NextId(),
                LeadId = assignDto.LeadId,
                FromEmployeeId = lead.AssignedTo > 0 ? lead.AssignedTo : null,
                ToEmployeeId = assignDto.ToEmployeeId,
                AssignmentDate = DateTime.Now,
                Reason = assignDto.Reason,
                ReassignmentType = assignDto.ReassignmentType ?? (isInitialAssignment ? "Initial" : "Manual"),
                Create_time = DateTime.Now
            };

            UseTran(() =>
            {
                _assignmentRepo.Insert(assignment);
                lead.AssignedTo = assignDto.ToEmployeeId;
                _leadRepo.Update(lead, true);
            });

            var assignmentDto = assignment.Adapt<LeadAssignmentLogDto>();
            EnrichAssignmentDto(assignmentDto, assignment.LeadId, assignment.FromEmployeeId, assignment.ToEmployeeId);
            return assignmentDto;
        }

        public int BulkAssignLeads(BulkAssignLeadsDto bulkAssignDto)
        {
            int successCount = 0;

            UseTran(() =>
            {
                foreach (var leadId in bulkAssignDto.LeadIds)
                {
                    var lead = _leadRepo.GetId(leadId);
                    if (lead == null) continue;

                    var assignment = new BpLeadAssignmentLog
                    {
                        AssignmentLogId = SnowFlakeSingle.Instance.NextId(),
                        LeadId = leadId,
                        FromEmployeeId = lead.AssignedTo > 0 ? lead.AssignedTo : null,
                        ToEmployeeId = bulkAssignDto.ToEmployeeId,
                        AssignmentDate = DateTime.Now,
                        Reason = bulkAssignDto.Reason ?? "Bulk Assignment",
                        ReassignmentType = "Manual",
                        Create_time = DateTime.Now
                    };

                    _assignmentRepo.Insert(assignment);
                    lead.AssignedTo = bulkAssignDto.ToEmployeeId;
                    _leadRepo.Update(lead, true);
                    successCount++;
                }
            });

            return successCount;
        }

        public Dictionary<long, int> GetWorkloadDistribution(string leadStatus = null)
        {
            var query = Context.Queryable<BpLead>().Where(l => l.AssignedTo > 0);

            if (!string.IsNullOrEmpty(leadStatus))
                query = query.Where(l => l.LeadStatus == leadStatus);

            var distribution = query
                .GroupBy(l => l.AssignedTo)
                .Select(g => new { EmployeeId = g.AssignedTo, Count = SqlFunc.AggregateCount(g.LeadId) })
                .ToList();

            return distribution.ToDictionary(d => d.EmployeeId, d => d.Count);
        }

        public int GetEmployeeLeadCount(long employeeId, string leadStatus = null)
        {
            var query = Context.Queryable<BpLead>().Where(l => l.AssignedTo == employeeId);

            if (!string.IsNullOrEmpty(leadStatus))
                query = query.Where(l => l.LeadStatus == leadStatus);

            return query.Count();
        }

        public List<LeadAssignmentLogDto> GetRecentReassignments(int days = 7)
        {
            var startDate = DateTime.Now.AddDays(-days);

            var assignments = Context.Queryable<BpLeadAssignmentLog>()
                .Where(a => a.AssignmentDate >= startDate)
                .Where(a => a.FromEmployeeId.HasValue)
                .OrderBy(a => a.AssignmentDate, OrderByType.Desc)
                .ToList();

            var assignmentDtos = assignments.Adapt<List<LeadAssignmentLogDto>>();
            EnrichAssignmentDtos(assignmentDtos);
            return assignmentDtos;
        }

        public AssignmentStatisticsDto GetAssignmentStatistics(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = Context.Queryable<BpLeadAssignmentLog>();

            if (startDate.HasValue)
                query = query.Where(a => a.AssignmentDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.AssignmentDate <= endDate.Value);

            var assignments = query.ToList();

            var assignmentsByEmployee = assignments
                .GroupBy(a => a.ToEmployeeId)
                .ToDictionary(g => g.Key, g => g.Count());

            var assignmentsByType = assignments
                .Where(a => !string.IsNullOrEmpty(a.ReassignmentType))
                .GroupBy(a => a.ReassignmentType)
                .ToDictionary(g => g.Key, g => g.Count());

            return new AssignmentStatisticsDto
            {
                TotalAssignments = assignments.Count,
                InitialAssignments = assignments.Count(a => !a.FromEmployeeId.HasValue),
                Reassignments = assignments.Count(a => a.FromEmployeeId.HasValue),
                AutoAssignments = assignments.Count(a => a.ReassignmentType == "Auto"),
                ManualAssignments = assignments.Count(a => a.ReassignmentType == "Manual"),
                AssignmentsByEmployee = assignmentsByEmployee,
                AssignmentsByType = assignmentsByType
            };
        }

        public LeadAssignmentLogDto AutoAssignLead(long leadId, string reason = "Auto-Assignment")
        {
            var workload = GetWorkloadDistribution("New");

            if (!workload.Any())
                throw new Exception("No employees available for assignment");

            var employeeWithLowestLoad = workload.OrderBy(w => w.Value).First().Key;

            var assignDto = new AssignLeadDto
            {
                LeadId = leadId,
                ToEmployeeId = employeeWithLowestLoad,
                Reason = reason,
                ReassignmentType = "Auto"
            };

            return AssignLead(assignDto);
        }

        private void EnrichAssignmentDtos(List<LeadAssignmentLogDto> assignmentDtos)
        {
            foreach (var dto in assignmentDtos)
                EnrichAssignmentDto(dto, dto.LeadId, dto.FromEmployeeId, dto.ToEmployeeId);
        }

        private void EnrichAssignmentDto(LeadAssignmentLogDto dto, long leadId, long? fromEmployeeId, long toEmployeeId)
        {
            var lead = _leadRepo.GetId(leadId);
            dto.LeadName = lead?.BrandName ?? "Unknown Lead";
            dto.FromEmployeeName = fromEmployeeId.HasValue ? $"Employee {fromEmployeeId.Value}" : "Unassigned";
            dto.ToEmployeeName = $"Employee {toEmployeeId}";
        }
    }
}
