using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Performance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// KPI Service Interface
    /// </summary>
    public interface IKpiService
    {
        // KPI Management
        Task<PagedInfo<HrKpi>> GetKpisAsync(Expression<Func<HrKpi, bool>> predicate, PagerInfo pager);
        Task<List<HrKpi>> GetActiveKpisAsync();
        Task<HrKpi> GetKpiByIdAsync(long kpiId);
        Task<int> AddKpiAsync(HrKpi kpi);
        Task<int> UpdateKpiAsync(HrKpi kpi);
        Task<int> DeleteKpiAsync(long kpiId);
        Task<List<HrKpi>> GetKpisByCategoryAsync(string category);
        Task<List<HrKpi>> GetDepartmentKpisAsync(long departmentId);
        Task<List<HrKpi>> GetPositionKpisAsync(long positionId);
        
        // Employee KPI Assignment
        Task<List<HrEmployeeKpi>> GetEmployeeKpisAsync(long employeeId);
        Task<int> AssignKpiToEmployeeAsync(HrEmployeeKpi assignment);
        Task<int> UpdateEmployeeKpiAsync(HrEmployeeKpi assignment);
        Task<int> RemoveEmployeeKpiAsync(long assignmentId);
        Task<List<HrEmployeeKpi>> GetKpiAssignmentsByPeriodAsync(int year, int? quarter);
        
        // KPI Achievement
        Task<List<HrKpiAchievement>> GetKpiAchievementsAsync(long employeeId, DateTime periodDate);
        Task<int> RecordKpiAchievementAsync(HrKpiAchievement achievement);
        Task<int> UpdateKpiAchievementAsync(HrKpiAchievement achievement);
        Task<decimal> CalculateKpiScoreAsync(long employeeId, DateTime periodDate);
        Task<int> VerifyKpiAchievementAsync(long achievementId, long verifiedBy);
        
        // KPI Metrics (Legacy Support)
        Task<List<HrKpiMetric>> GetKpiMetricsAsync(long employeeId);
        Task<int> AddKpiMetricAsync(HrKpiMetric metric);
        Task<int> UpdateKpiMetricAsync(HrKpiMetric metric);
        
        // KPI Records
        Task<List<HrKpiRecord>> GetKpiRecordsAsync(long employeeId, DateTime startDate, DateTime endDate);
        Task<int> AddKpiRecordAsync(HrKpiRecord record);
        
        // KPI Analytics
        Task<object> GetKpiDashboardAsync(long? employeeId, int year);
        Task<object> GetTeamKpiPerformanceAsync(long managerId, DateTime periodDate);
        Task<object> GetDepartmentKpiAnalyticsAsync(long departmentId, int year);
        Task<object> GetKpiTrendsAsync(long employeeId, int months);
        Task<List<object>> GetTopPerformersAsync(DateTime periodDate, int topN);
        Task<List<object>> GetUnderperformersAsync(DateTime periodDate, int bottomN);
        
        // KPI Calculation
        Task<decimal> CalculateWeightedScoreAsync(long employeeId, DateTime periodDate);
        Task<int> ProcessMonthlyKpisAsync(DateTime month);
        Task<int> ProcessQuarterlyKpisAsync(int year, int quarter);
        Task<int> ProcessAnnualKpisAsync(int year);
        
        // KPI Reports
        Task<byte[]> GenerateKpiReportAsync(long employeeId, DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateTeamKpiReportAsync(long managerId, DateTime periodDate);
        Task<byte[]> ExportKpiDataAsync(Expression<Func<HrKpi, bool>> predicate, string format);
        
        // KPI Templates
        Task<List<object>> GetKpiTemplatesAsync(string category);
        Task<int> CreateKpiFromTemplateAsync(long templateId, long employeeId);
        Task<int> BulkAssignKpisAsync(List<long> employeeIds, List<long> kpiIds);
    }
}
