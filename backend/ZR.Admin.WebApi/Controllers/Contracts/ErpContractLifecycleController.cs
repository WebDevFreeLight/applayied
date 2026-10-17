using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Service.IService.Contracts.Lifecycle;

namespace ZR.Admin.WebApi.Controllers.Contracts
{
    /// <summary>
    /// Contract Lifecycle Management Controller - إدارة دورة حياة العقد
    /// Handles advanced lifecycle operations: renewals, terminations, amendments, suspensions, deposits
    /// </summary>
    [Authorize]
    [Route("erp/contracts/lifecycle")]
    [ApiController]
    public class ErpContractLifecycleController : BaseController
    {
        private readonly IContractLifecycleManager _lifecycleManager;

        public ErpContractLifecycleController(IContractLifecycleManager lifecycleManager)
        {
            _lifecycleManager = lifecycleManager;
        }

        #region Contract Renewal Endpoints

        /// <summary>
        /// Initiate contract renewal
        /// بدء تجديد العقد
        /// </summary>
        [HttpPost("renewal/initiate")]
        [Log(Title = "Contract Renewal", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> InitiateRenewal([FromBody] RenewalRequest request)
        {
            try
            {
                request.UserName = HttpContext.GetName();
                var result = await _lifecycleManager.InitiateRenewalAsync(request);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Calculate renewal terms for a contract
        /// حساب شروط التجديد للعقد
        /// </summary>
        [HttpPost("renewal/calculate-terms/{contractId}")]
        public async Task<IActionResult> CalculateRenewalTerms(Guid contractId, [FromBody] RenewalOptions options)
        {
            try
            {
                var result = await _lifecycleManager.CalculateRenewalTermsAsync(contractId, options ?? new RenewalOptions());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Apply renewal to contract
        /// تطبيق التجديد على العقد
        /// </summary>
        [HttpPost("renewal/apply/{contractId}")]
        [Log(Title = "Contract Renewal", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApplyRenewal(Guid contractId, [FromBody] RenewalTerms terms)
        {
            try
            {
                var result = await _lifecycleManager.ApplyRenewalAsync(contractId, terms, HttpContext.GetName());
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get contracts eligible for renewal
        /// الحصول على العقود المؤهلة للتجديد
        /// </summary>
        [HttpGet("renewal/eligible")]
        public async Task<IActionResult> GetRenewalEligibleContracts([FromQuery] int daysBeforeExpiry = 90)
        {
            try
            {
                var result = await _lifecycleManager.GetRenewalEligibleContractsAsync(daysBeforeExpiry);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Process auto-renewals
        /// معالجة التجديدات التلقائية
        /// </summary>
        [HttpPost("renewal/process-auto")]
        [Log(Title = "Contract Auto Renewal", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> ProcessAutoRenewals([FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var result = await _lifecycleManager.ProcessAutoRenewalsAsync(asOfDate ?? DateTime.Today);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Contract Termination Endpoints

        /// <summary>
        /// Initiate contract termination
        /// بدء إنهاء العقد
        /// </summary>
        [HttpPost("termination/initiate")]
        [Log(Title = "Contract Termination", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> InitiateTermination([FromBody] TerminationRequest request)
        {
            try
            {
                request.UserName = HttpContext.GetName();
                var result = await _lifecycleManager.InitiateTerminationAsync(request);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Calculate termination financials
        /// حساب التسويات المالية للإنهاء
        /// </summary>
        [HttpGet("termination/calculate/{contractId}")]
        public async Task<IActionResult> CalculateTerminationFinancials(
            Guid contractId,
            [FromQuery] DateTime terminationDate,
            [FromQuery] Model.Contracts.TerminationType terminationType)
        {
            try
            {
                var result = await _lifecycleManager.CalculateTerminationFinancialsAsync(contractId, terminationDate, terminationType);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Process contract termination
        /// معالجة إنهاء العقد
        /// </summary>
        [HttpPost("termination/process/{contractId}")]
        [Log(Title = "Contract Termination", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProcessTermination(Guid contractId)
        {
            try
            {
                var result = await _lifecycleManager.ProcessTerminationAsync(contractId, HttpContext.GetName());
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Calculate deposit settlement for termination
        /// حساب تسوية الضمان عند الإنهاء
        /// </summary>
        [HttpGet("termination/deposit-settlement/{contractId}")]
        public async Task<IActionResult> CalculateDepositSettlement(Guid contractId, [FromQuery] DateTime terminationDate)
        {
            try
            {
                var result = await _lifecycleManager.CalculateDepositSettlementAsync(contractId, terminationDate);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get termination checklist
        /// الحصول على قائمة فحص الإنهاء
        /// </summary>
        [HttpGet("termination/checklist/{contractId}")]
        public async Task<IActionResult> GetTerminationChecklist(Guid contractId)
        {
            try
            {
                var result = await _lifecycleManager.GetTerminationChecklistAsync(contractId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Contract Amendment Endpoints

        /// <summary>
        /// Create new amendment
        /// إنشاء تعديل جديد
        /// </summary>
        [HttpPost("amendment/create")]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateAmendment([FromBody] AmendmentRequest request)
        {
            try
            {
                request.UserName = HttpContext.GetName();
                var result = await _lifecycleManager.CreateAmendmentAsync(request);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Apply approved amendment to contract
        /// تطبيق التعديل المعتمد على العقد
        /// </summary>
        [HttpPost("amendment/apply/{amendmentId}")]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApplyAmendment(Guid amendmentId)
        {
            try
            {
                var result = await _lifecycleManager.ApplyAmendmentAsync(amendmentId, HttpContext.GetName());
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Reverse applied amendment
        /// التراجع عن التعديل المطبق
        /// </summary>
        [HttpPost("amendment/reverse/{amendmentId}")]
        [Log(Title = "Contract Amendment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ReverseAmendment(Guid amendmentId, [FromQuery] string reason)
        {
            try
            {
                var result = await _lifecycleManager.ReverseAmendmentAsync(amendmentId, HttpContext.GetName(), reason);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Calculate amendment impact
        /// حساب تأثير التعديل
        /// </summary>
        [HttpPost("amendment/impact/{contractId}")]
        public async Task<IActionResult> CalculateAmendmentImpact(Guid contractId, [FromBody] AmendmentRequest request)
        {
            try
            {
                request.ContractId = contractId;
                var result = await _lifecycleManager.CalculateAmendmentImpactAsync(contractId, request);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get amendment history for a contract
        /// الحصول على سجل التعديلات للعقد
        /// </summary>
        [HttpGet("amendment/history/{contractId}")]
        public async Task<IActionResult> GetAmendmentHistory(Guid contractId)
        {
            try
            {
                var result = await _lifecycleManager.GetAmendmentHistoryAsync(contractId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Contract Suspension Endpoints

        /// <summary>
        /// Suspend contract
        /// تعليق العقد
        /// </summary>
        [HttpPost("suspension/suspend")]
        [Log(Title = "Contract Suspension", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> SuspendContract([FromBody] SuspensionRequest request)
        {
            try
            {
                request.UserName = HttpContext.GetName();
                var result = await _lifecycleManager.SuspendContractAsync(request);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Resume suspended contract
        /// استئناف العقد المعلق
        /// </summary>
        [HttpPost("suspension/resume/{contractId}")]
        [Log(Title = "Contract Resume", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ResumeContract(Guid contractId, [FromQuery] DateTime? resumeDate = null)
        {
            try
            {
                var result = await _lifecycleManager.ResumeContractAsync(contractId, HttpContext.GetName(), resumeDate);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Calculate suspension billing impact
        /// حساب تأثير التعليق على الفوترة
        /// </summary>
        [HttpGet("suspension/impact/{contractId}")]
        public async Task<IActionResult> CalculateSuspensionImpact(
            Guid contractId,
            [FromQuery] DateTime suspensionStart,
            [FromQuery] DateTime? suspensionEnd = null)
        {
            try
            {
                var result = await _lifecycleManager.CalculateSuspensionImpactAsync(contractId, suspensionStart, suspensionEnd);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Deposit Management Endpoints

        /// <summary>
        /// Record security deposit
        /// تسجيل الضمان
        /// </summary>
        [HttpPost("deposit/record")]
        [Log(Title = "Contract Deposit", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> RecordDeposit([FromBody] DepositRequest request)
        {
            try
            {
                request.UserName = HttpContext.GetName();
                var result = await _lifecycleManager.RecordDepositAsync(request);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Process deposit refund
        /// معالجة استرداد الضمان
        /// </summary>
        [HttpPost("deposit/refund/{contractId}")]
        [Log(Title = "Contract Deposit", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProcessDepositRefund(Guid contractId, [FromBody] DepositRefundRequest request)
        {
            try
            {
                request.UserName = HttpContext.GetName();
                var result = await _lifecycleManager.ProcessDepositRefundAsync(contractId, request);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Apply deposit deduction
        /// تطبيق خصم من الضمان
        /// </summary>
        [HttpPost("deposit/deduction/{contractId}")]
        [Log(Title = "Contract Deposit", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApplyDepositDeduction(Guid contractId, [FromBody] DepositDeductionRequest request)
        {
            try
            {
                request.UserName = HttpContext.GetName();
                var result = await _lifecycleManager.ApplyDepositDeductionAsync(contractId, request);
                if (result.Success)
                {
                    return SUCCESS(result);
                }
                return ToResponse(ResultCode.FAIL, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get deposit balance for contract
        /// الحصول على رصيد الضمان للعقد
        /// </summary>
        [HttpGet("deposit/balance/{contractId}")]
        public async Task<IActionResult> GetDepositBalance(Guid contractId)
        {
            try
            {
                var result = await _lifecycleManager.GetDepositBalanceAsync(contractId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion
    }
}
