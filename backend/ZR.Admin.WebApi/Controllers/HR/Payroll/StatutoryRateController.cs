using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.Payroll;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Payroll
{
    /// <summary>
    /// Statutory Rate Management (GOSI rates, etc.)
    /// </summary>
    [Verify]
    [Route("api/hr/statutory-rates")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class StatutoryRateController : BaseController
    {
        private readonly IHrStatutoryRateService _service;

        public StatutoryRateController(IHrStatutoryRateService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all active statutory rates
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:statutory:list")]
        public async Task<IActionResult> GetList()
        {
            var result = await _service.GetAllActiveRatesAsync();
            return SUCCESS(result);
        }

        /// <summary>
        /// Get statutory rate by ID
        /// </summary>
        [HttpGet("{rateId}")]
        [ActionPermissionFilter("hr:statutory:query")]
        public async Task<IActionResult> GetById(long rateId)
        {
            var result = await _service.GetRateAsync(rateId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get current GOSI rates
        /// </summary>
        [HttpGet("gosi")]
        [ActionPermissionFilter("hr:statutory:query")]
        public async Task<IActionResult> GetCurrentGOSIRates([FromQuery] bool isSaudi = true)
        {
            var (employeeRate, employerRate) = await _service.GetCurrentGOSIRatesAsync(isSaudi);
            return SUCCESS(new
            {
                IsSaudi = isSaudi,
                EmployeeRate = employeeRate,
                EmployerRate = employerRate,
                EmployeePercentage = $"{employeeRate * 100}%",
                EmployerPercentage = $"{employerRate * 100}%"
            });
        }

        /// <summary>
        /// Add statutory rate
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("hr:statutory:add")]
        [Log(Title = "Add Statutory Rate", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> Add([FromBody] HrStatutoryRate rate)
        {
            var result = await _service.AddRateAsync(rate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update statutory rate
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter("hr:statutory:edit")]
        [Log(Title = "Update Statutory Rate", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Update([FromBody] HrStatutoryRate rate)
        {
            var result = await _service.UpdateRateAsync(rate);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete (deactivate) statutory rate
        /// </summary>
        [HttpDelete("{rateId}")]
        [ActionPermissionFilter("hr:statutory:delete")]
        [Log(Title = "Delete Statutory Rate", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> Delete(long rateId)
        {
            var result = await _service.DeleteRateAsync(rateId);
            return ToResponse(result);
        }
    }
}
