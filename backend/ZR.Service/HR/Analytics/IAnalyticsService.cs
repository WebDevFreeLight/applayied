using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Comprehensive Analytics Service Interface
    /// </summary>
    public interface IAnalyticsService
    {
        #region Analytics Reports
        
        // Analytics Report Management
        Task<PagedInfo<HrAnalyticsReport>> GetAnalyticsReportsAsync(Expression<Func<HrAnalyticsReport, bool>> predicate, PagerInfo pager);
        Task<HrAnalyticsReport> GenerateReportAsync(string reportType, Dictionary<string, object> parameters);
        Task<int> SaveReportAsync(HrAnalyticsReport report);
        Task<List<HrAnalyticsReport>> GetReportsAsync(string reportType = null, DateTime? startDate = null, DateTime? endDate = null);
        
        #endregion

        #region Workforce Analytics
        
        // Workforce Metrics
        Task<HrWorkforceMetrics> GetWorkforceMetricsAsync(DateTime? asOfDate = null);
        Task<HrWorkforceMetrics> GetWorkforceMetricsAsync(DateTime date);
        Task<HrWorkforceMetrics> CalculateWorkforceMetricsAsync(DateTime date);
        Task<List<HrWorkforceMetrics>> GetWorkforceMetricsTrendAsync(DateTime startDate, DateTime endDate);
        
        // Turnover Analytics
        Task<decimal> CalculateTurnoverRateAsync(DateTime startDate, DateTime endDate);
        Task<object> GetTurnoverAnalysisAsync(int year);
        
        // Absenteeism & Overtime
        Task<decimal> CalculateAbsenteeismRateAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateOvertimeCostAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateAverageTenureAsync();
        
        // Retention Analysis
        Task<object> GetRetentionAnalysisAsync(int year);
        
        // Flight Risk Analysis
        Task<List<HrEmployeeCore>> GetFlightRiskEmployeesAsync();
        
        #endregion

        #region Performance Analytics
        
        // Performance Metrics
        Task<HrPerformanceMetrics> GetPerformanceMetricsAsync(int year);
        Task<HrPerformanceMetrics> GetPerformanceMetricsAsync(int year, DateTime date);
        Task<HrPerformanceMetrics> CalculatePerformanceMetricsAsync(DateTime date);
        Task<List<HrEmployeeCore>> GetHighPerformersAsync(int topN = 10);
        
        #endregion

        #region Compensation Analytics
        
        // Compensation Metrics
        Task<HrCompensationMetrics> GetCompensationMetricsAsync(DateTime periodStart, DateTime periodEnd);
        Task<HrCompensationMetrics> CalculateCompensationMetricsAsync(DateTime date);
        Task<decimal> CalculateCompaRatioAsync(long employeeId);
        Task<object> GetCostAnalysisAsync(int year);
        
        #endregion

        #region Leave Analytics
        
        // Leave Metrics
        Task<HrLeaveMetrics> GetLeaveMetricsAsync(int year);
        Task<HrLeaveMetrics> CalculateLeaveMetricsAsync(int year);
        Task<object> GetLeaveAnalyticsAsync(int year);
        
        #endregion

        #region Training Analytics
        
        // Training Metrics
        Task<HrTrainingMetrics> GetTrainingMetricsAsync(DateTime startDate, DateTime endDate);
        Task<HrTrainingMetrics> CalculateTrainingMetricsAsync(DateTime periodStart, DateTime periodEnd);
        Task<object> GetTrainingAnalyticsAsync(DateTime startDate, DateTime endDate);
        
        #endregion

        #region Attendance Analytics d
        
        Task<object> GetAttendanceAnalyticsAsync(DateTime startDate, DateTime endDate);
        
        #endregion

        #region Recruitment Analytics
        
        Task<object> GetRecruitmentAnalyticsAsync(int year);
        
        #endregion

        #region Predictive Analytics
        
        // Predictive Insights
        Task<HrPredictiveInsights> GeneratePredictiveInsightsAsync();
        Task<HrPredictiveInsights> GeneratePredictiveInsightsAsync(DateTime date);
        Task<List<HrPredictiveInsights>> GetPredictiveInsightsHistoryAsync(int months);
        
        #endregion

        #region KPI & Dashboard
        
        // KPI Dashboard
        Task<List<HrKpiDashboard>> GetKpiDashboardAsync(int year, int? month, int? quarter);
        Task<object> GetKpiDashboardAsync();
        Task<int> UpdateKpiDashboardAsync(HrKpiDashboard kpi);
        
        // Dashboard Metrics
        Task<List<HrDashboardMetrics>> GetDashboardMetricsAsync(string dashboardType, long? userId);
        Task<int> UpdateDashboardMetricsAsync(List<HrDashboardMetrics> metrics);
        
        #endregion

        #region Job Position Analytics
        
        Task<List<HrJobPosition>> GetJobPositionAnalyticsAsync();
        Task<HrJobPosition> GetJobPositionByIdAsync(long positionId);
        Task<int> UpdateJobPositionAsync(HrJobPosition position);
        
        #endregion

        #region Payroll Analytics
        
        Task<List<HrPayrollHistory>> GetPayrollHistoryAsync(long employeeId);
        Task<object> GetPayrollTrendsAsync(long employeeId, int months);
        
        #endregion

        #region Diversity & Inclusion
        
        Task<object> GetDiversityMetricsAsync();
        Task<object> GetEmployeeSatisfactionMetricsAsync(int year);
        
        #endregion

        #region Department Analytics
        
        Task<object> GetDepartmentComparisonAsync();
        
        #endregion

        #region Trend Analysis
        
        Task<object> GetTrendAnalysisAsync(string metric, int months);
        Task<object> GetBenchmarkComparisonAsync();
        
        #endregion

        #region Export & Reporting
        
        // Export Reports
        Task<byte[]> ExportAnalyticsReportAsync(string reportType, string format, Dictionary<string, object> parameters);
        Task<byte[]> ExportDashboardAsync(string format, DateTime startDate, DateTime endDate);
        
        #endregion
    }
}
