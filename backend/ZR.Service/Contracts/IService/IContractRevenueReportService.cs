using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;

namespace ZR.Service.Contracts.IService
{
    public interface IContractRevenueReportService
    {
        /// <summary>
        /// Get revenue reports with pagination and filtering
        /// </summary>
        Task<PagedInfo<RevenueReportResponseDto>> GetListAsync(RevenueReportQueryDto query);

        /// <summary>
        /// Get revenue report by ID
        /// </summary>
        Task<ErpContractRevenueReport> GetByIdAsync(Guid reportId);

        /// <summary>
        /// Get all reports for a contract
        /// </summary>
        Task<List<RevenueReportResponseDto>> GetByContractIdAsync(Guid contractId);

        /// <summary>
        /// Create a revenue report and calculate invoice amount deterministically.
        /// Fixed → fixed_rent_amount
        /// RevenueShare → MAX(reported_revenue × percentage, minimum_guarantee)
        /// </summary>
        Task<ErpContractRevenueReport> CreateAsync(CreateRevenueReportDto dto, string createdBy);

        /// <summary>
        /// Bulk create revenue reports for multiple units in one period
        /// </summary>
        Task<List<ErpContractRevenueReport>> BulkCreateAsync(BulkCreateRevenueReportDto dto, string createdBy);

        /// <summary>
        /// Update reported revenue (recalculates invoice amount). Only if status = Draft.
        /// </summary>
        Task<int> UpdateAsync(UpdateRevenueReportDto dto, string updatedBy);

        /// <summary>
        /// Submit a draft report (status → Submitted)
        /// </summary>
        Task<int> SubmitAsync(Guid reportId, string submittedBy);

        /// <summary>
        /// Delete a draft report
        /// </summary>
        Task<int> DeleteAsync(Guid reportId);

        /// <summary>
        /// Get summary statistics for a contract
        /// </summary>
        Task<RevenueReportSummaryDto> GetSummaryAsync(Guid contractId);
    }
}
