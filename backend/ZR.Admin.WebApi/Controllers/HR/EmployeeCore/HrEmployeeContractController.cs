using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;
using ZR.Admin.WebApi.Extensions;
using System;
namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Employee Core - Contract Controller
    /// Handles: Contract type, dates, probation period
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/contract")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrEmployeeContractController : BaseController
    {
        private readonly IHrEmployeeContractService _contractService;

        public HrEmployeeContractController(IHrEmployeeContractService contractService)
        {
            _contractService = contractService;
        }

        /// <summary>
        /// Get current contract for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Current contract record with calculated fields</returns>
        [HttpGet("{employeeId:long}/current")]
        [ActionPermissionFilter("hr:employeecore:contract:query")]
        public async Task<IActionResult> GetCurrentContract(long employeeId)
        {
            var contracts = await _contractService.GetListAsync(x => x.EmployeeId == employeeId);

            // Get current contract (IsCurrent flag or effective dating)
            var currentContract = contracts
                .Where(c => c.IsCurrent)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefault();

            if (currentContract == null)
            {
                // Fallback to effective dating
                var now =DateTime.Now;
                currentContract = contracts
                    .Where(c => c.EffectiveFrom <= now && (c.EffectiveTo == null || c.EffectiveTo > now))
                    .OrderByDescending(c => c.EffectiveFrom)
                    .FirstOrDefault();
            }

            if (currentContract == null)
            {
                return SUCCESS(new
                {
                    EmployeeId = employeeId,
                    HasContract = false,
                    Contract = (object)null
                });
            }

            // Calculate additional fields
            int? durationDays = null;
            if (currentContract.ContractStartDate.HasValue && currentContract.ContractEndDate.HasValue)
            {
                durationDays = (currentContract.ContractEndDate.Value - currentContract.ContractStartDate.Value).Days;
            }

            bool isProbationActive = false;
            if (currentContract.ProbationEndDate.HasValue)
            {
                isProbationActive = currentContract.ProbationEndDate.Value >= DateTime.Now;
            }

            return SUCCESS(new
            {
                EmployeeId = employeeId,
                HasContract = true,
                Contract = currentContract,
                ContractDurationDays = durationDays,
                IsProbationActive = isProbationActive
            });
        }

        /// <summary>
        /// Get contract history for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>All contract records ordered by effective date</returns>
        [HttpGet("{employeeId:long}/history")]
        [ActionPermissionFilter("hr:employeecore:contract:query")]
        public async Task<IActionResult> GetContractHistory(long employeeId)
        {
            var contracts = await _contractService.GetListAsync(x => x.EmployeeId == employeeId);
            var orderedContracts = contracts.OrderByDescending(c => c.EffectiveFrom).ToList();

            return SUCCESS(orderedContracts);
        }

        /// <summary>
        /// Create new contract (marks previous as not current)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Contract data</param>
        /// <returns>Created contract ID</returns>
        [HttpPost("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:contract:add")]
        [Log(Title = "Create Contract", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateContract(long employeeId, [FromBody] CreateContractDto dto)
        {
            // Mark existing contracts as not current
            var existingContracts = await _contractService.GetListAsync(c => c.EmployeeId == employeeId && c.IsCurrent);

            DateTime effectiveFrom = dto.EffectiveFrom ?? DateTime.Now;

            foreach (var existing in existingContracts)
            {
                existing.IsCurrent = false;
                existing.EffectiveTo = effectiveFrom.AddDays(-1);
                existing.Update_by = HttpContext.GetName();
                existing.Update_time = DateTime.Now;
                await _contractService.UpdateAsync(existing);
            }

            // Create new contract
            var newContract = new Model.HR.EmployeeCore.HrEmployeeContract
            {
                EmployeeId = employeeId,
                ContractTypeDictCode = dto.ContractTypeDictCode,
                ContractStartDate = dto.ContractStartDate,
                ContractEndDate = dto.ContractEndDate,
                ProbationEndDate = dto.ProbationEndDate,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = null,
                IsCurrent = true,
                Create_by = HttpContext.GetName(),
                Create_time = DateTime.Now
            };

            var contractId = await _contractService.AddAsync(newContract);

            return SUCCESS(new { EmployeeContractId = contractId });
        }

        /// <summary>
        /// Update contract
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="contractId">Contract ID</param>
        /// <param name="dto">Updated data</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/{contractId:long}")]
        [ActionPermissionFilter("hr:employeecore:contract:edit")]
        [Log(Title = "Update Contract", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateContract(long employeeId, long contractId, [FromBody] UpdateContractDto dto)
        {
            var contract = await _contractService.GetByIdAsync(contractId);
            if (contract == null || contract.EmployeeId != employeeId)
            {
                return ToResponse(ApiResult.Error("Contract not found"));
            }

            if (dto.ContractTypeDictCode.HasValue)
                contract.ContractTypeDictCode = dto.ContractTypeDictCode;

            if (dto.ContractStartDate.HasValue)
                contract.ContractStartDate = dto.ContractStartDate;

            if (dto.ContractEndDate.HasValue)
                contract.ContractEndDate = dto.ContractEndDate;

            if (dto.ProbationEndDate.HasValue)
                contract.ProbationEndDate = dto.ProbationEndDate;

            if (dto.EffectiveTo.HasValue)
                contract.EffectiveTo = dto.EffectiveTo;

            contract.Update_by = HttpContext.GetName();
            contract.Update_time = DateTime.Now;

            await _contractService.UpdateAsync(contract);

            return SUCCESS(contract);
        }

        /// <summary>
        /// Close/deactivate current contract
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="contractId">Contract ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("{employeeId:long}/{contractId:long}")]
        [ActionPermissionFilter("hr:employeecore:contract:delete")]
        [Log(Title = "Close Contract", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CloseContract(long employeeId, long contractId)
        {
            var contract = await _contractService.GetByIdAsync(contractId);
            if (contract == null || contract.EmployeeId != employeeId)
            {
                return ToResponse(ApiResult.Error("Contract not found"));
            }

            // Close the contract
            contract.IsCurrent = false;
            contract.EffectiveTo = DateTime.Now;
            contract.Update_by = HttpContext.GetName();
            contract.Update_time =DateTime.Now;

            await _contractService.UpdateAsync(contract);

            return SUCCESS(contract);
        }
    }
}
