using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Contracts.Dto;
using ZR.Service.Contracts.IService;
using Infrastructure;
using Infrastructure.Model;
using System;

namespace ZR.Admin.WebApi.Controllers.Contracts
{
    /// <summary>
    /// Contract Revenue Reports - تقارير إيرادات العقود
    /// Revenue share calculation engine.
    /// Fixed → invoice = fixed_rent_amount
    /// RevenueShare → invoice = MAX(reported_revenue × percentage, minimum_guarantee)
    /// </summary>
    [Verify]
    [Route("api/erp/contracts/revenue-reports")]
    public class ContractRevenueReportController : BaseController
    {
        private readonly IContractRevenueReportService _revenueReportService;

        public ContractRevenueReportController(IContractRevenueReportService revenueReportService)
        {
            _revenueReportService = revenueReportService;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        /// <summary>
        /// Get revenue reports with pagination and filtering
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("erp:contracts:list")]
        public async Task<IActionResult> GetList([FromQuery] RevenueReportQueryDto dto)
        {
            var result = await _revenueReportService.GetListAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get single revenue report by ID
        /// </summary>
        [HttpGet("{reportId}")]
        [ActionPermissionFilter("erp:contracts:view")]
        public async Task<IActionResult> GetById(Guid reportId)
        {
            return SUCCESS(await _revenueReportService.GetByIdAsync(reportId));
        }

        /// <summary>
        /// Get all revenue reports for a contract
        /// </summary>
        [HttpGet("contract/{contractId}")]
        [ActionPermissionFilter("erp:contracts:view")]
        public async Task<IActionResult> GetByContractId(Guid contractId)
        {
            return SUCCESS(await _revenueReportService.GetByContractIdAsync(contractId));
        }

        /// <summary>
        /// Submit a revenue report for a contract unit.
        /// Automatically calculates invoice amount based on pricing type.
        /// </summary>
        [HttpPost("add")]
        [ActionPermissionFilter("erp:contracts:edit")]
        public async Task<IActionResult> Create([FromBody] CreateRevenueReportDto dto)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var result = await _revenueReportService.CreateAsync(dto, currentUser?.UserName);
                return SUCCESS(result, "Revenue report created successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Bulk submit revenue reports for multiple units in one period
        /// </summary>
        [HttpPost("bulk-add")]
        [ActionPermissionFilter("erp:contracts:edit")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateRevenueReportDto dto)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var result = await _revenueReportService.BulkCreateAsync(dto, currentUser?.UserName);
                return SUCCESS(result, $"{result.Count} revenue reports created successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update reported revenue (recalculates invoice amount). Only for draft reports.
        /// </summary>
        [HttpPost("update")]
        [ActionPermissionFilter("erp:contracts:edit")]
        public async Task<IActionResult> Update([FromBody] UpdateRevenueReportDto dto)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var result = await _revenueReportService.UpdateAsync(dto, currentUser?.UserName);
                return SUCCESS(result, "Revenue report updated successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Submit a draft report (transitions to Submitted status)
        /// </summary>
        [HttpPost("submit/{reportId}")]
        [ActionPermissionFilter("erp:contracts:edit")]
        public async Task<IActionResult> Submit(Guid reportId)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var result = await _revenueReportService.SubmitAsync(reportId, currentUser?.UserName);
                return SUCCESS(result, "Revenue report submitted successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete a draft report
        /// </summary>
        [HttpPost("delete/{reportId}")]
        [ActionPermissionFilter("erp:contracts:edit")]
        public async Task<IActionResult> Delete(Guid reportId)
        {
            try
            {
                var result = await _revenueReportService.DeleteAsync(reportId);
                return SUCCESS(result, "Revenue report deleted successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get summary statistics for a contract's revenue reports
        /// </summary>
        [HttpGet("summary/{contractId}")]
        [ActionPermissionFilter("erp:contracts:view")]
        public async Task<IActionResult> GetSummary(Guid contractId)
        {
            return SUCCESS(await _revenueReportService.GetSummaryAsync(contractId));
        }
    }
}
