using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Customers.Dto;
using ZR.Model.Customers.Leads;

namespace ZR.Service.Customers.ICustomersService
{
    /// <summary>
    /// Lead Assignment Service Interface
    /// </summary>
    public interface IBpLeadAssignmentService : IBaseService<BpLeadAssignmentLog>
    {
        PagedInfo<LeadAssignmentLogDto> GetAssignmentHistory(LeadAssignmentQueryDto queryDto);
        List<LeadAssignmentLogDto> GetLeadAssignmentHistory(long leadId);
        LeadAssignmentLogDto AssignLead(AssignLeadDto assignDto);
        int BulkAssignLeads(BulkAssignLeadsDto bulkAssignDto);
        Dictionary<long, int> GetWorkloadDistribution(string leadStatus = null);
        int GetEmployeeLeadCount(long employeeId, string leadStatus = null);
        List<LeadAssignmentLogDto> GetRecentReassignments(int days = 7);
        AssignmentStatisticsDto GetAssignmentStatistics(DateTime? startDate = null, DateTime? endDate = null);
        LeadAssignmentLogDto AutoAssignLead(long leadId, string reason = "Auto-Assignment");
    }

    /// <summary>
    /// Assignment Statistics DTO
    /// </summary>
    public class AssignmentStatisticsDto
    {
        public int TotalAssignments { get; set; }
        public int InitialAssignments { get; set; }
        public int Reassignments { get; set; }
        public int AutoAssignments { get; set; }
        public int ManualAssignments { get; set; }
        public Dictionary<long, int> AssignmentsByEmployee { get; set; }
        public Dictionary<string, int> AssignmentsByType { get; set; }
    }
}
