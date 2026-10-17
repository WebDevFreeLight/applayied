using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;
using ZR.Admin.WebApi.Extensions;

namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Employee Core - Statutory/Compliance Controller
    /// Handles: GOSI, Medical Insurance, Iqama, and other statutory requirements
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/statutory")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrEmployeeStatutoryController : BaseController
    {
        private readonly IHrEmployeeStatutoryService _statutoryService;

        public HrEmployeeStatutoryController(IHrEmployeeStatutoryService statutoryService)
        {
            _statutoryService = statutoryService;
        }

        /// <summary>
        /// Get current statutory information for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Current statutory record</returns>
        [HttpGet("{employeeId:long}/current")]
        [ActionPermissionFilter("hr:employeecore:statutory:query")]
        public async Task<IActionResult> GetCurrentStatutory(long employeeId)
        {
            var records = await _statutoryService.GetListAsync(x => x.EmployeeId == employeeId);

            // Get current record (effective now, no end date or end date in future)
            var now = DateTime.Now;
            var currentRecord = records
                .Where(s => s.EffectiveFrom <= now && (s.EffectiveTo == null || s.EffectiveTo > now))
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstOrDefault();

            if (currentRecord == null)
            {
                return SUCCESS(new
                {
                    EmployeeId = employeeId,
                    HasRecord = false,
                    Record = (object)null
                });
            }

            return SUCCESS(new
            {
                EmployeeId = employeeId,
                HasRecord = true,
                Record = currentRecord
            });
        }

        /// <summary>
        /// Get statutory history for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>All statutory records ordered by effective date</returns>
        [HttpGet("{employeeId:long}/history")]
        [ActionPermissionFilter("hr:employeecore:statutory:query")]
        public async Task<IActionResult> GetStatutoryHistory(long employeeId)
        {
            var records = await _statutoryService.GetListAsync(x => x.EmployeeId == employeeId);
            var orderedRecords = records.OrderByDescending(s => s.EffectiveFrom).ToList();

            return SUCCESS(orderedRecords);
        }

        /// <summary>
        /// Create new statutory record (closes existing current record)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Statutory data</param>
        /// <returns>Created record ID</returns>
        [HttpPost("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:statutory:add")]
        [Log(Title = "Create Statutory Record", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateStatutoryRecord(long employeeId, [FromBody] CreateStatutoryDto dto)
        {
            // Close existing current records
            var existingRecords = await _statutoryService.GetListAsync(s =>
                s.EmployeeId == employeeId &&
                (s.EffectiveTo == null || s.EffectiveTo > DateTime.Now));

            DateTime effectiveFrom = dto.EffectiveFrom ?? DateTime.Now;

            foreach (var existing in existingRecords)
            {
                existing.EffectiveTo = effectiveFrom.AddDays(-1);
                existing.Update_by = HttpContext.GetName();
                existing.Update_time = DateTime.Now;
                await _statutoryService.UpdateAsync(existing);
            }

            // Create new record
            var newRecord = new Model.HR.EmployeeCore.HrEmployeeStatutory
            {
                EmployeeId = employeeId,
                CountryDictCode = dto.CountryDictCode,
                HasGosi = dto.HasGosi,
                GosiNumber = dto.GosiNumber,
                IsSaudi = dto.IsSaudi,
                MedicalInsuranceNumber = dto.MedicalInsuranceNumber,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = null,
                Create_by = HttpContext.GetName(),
                Create_time = DateTime.Now
            };

            var recordId = await _statutoryService.AddAsync(newRecord);

            return SUCCESS(new { EmployeeStatutoryId = recordId });
        }

        /// <summary>
        /// Update statutory record
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="statutoryId">Statutory Record ID</param>
        /// <param name="dto">Updated data</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/{statutoryId:long}")]
        [ActionPermissionFilter("hr:employeecore:statutory:edit")]
        [Log(Title = "Update Statutory Record", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateStatutoryRecord(long employeeId, long statutoryId, [FromBody] UpdateStatutoryDto dto)
        {
            var record = await _statutoryService.GetByIdAsync(statutoryId);
            if (record == null || record.EmployeeId != employeeId)
            {
                return ToResponse(ApiResult.Error("Statutory record not found"));
            }

            if (dto.CountryDictCode.HasValue)
                record.CountryDictCode = dto.CountryDictCode;

            if (dto.HasGosi.HasValue)
                record.HasGosi = dto.HasGosi.Value;

            if (dto.GosiNumber != null)
                record.GosiNumber = dto.GosiNumber;

            if (dto.IsSaudi.HasValue)
                record.IsSaudi = dto.IsSaudi.Value;

            if (dto.MedicalInsuranceNumber != null)
                record.MedicalInsuranceNumber = dto.MedicalInsuranceNumber;

            if (dto.EffectiveTo.HasValue)
                record.EffectiveTo = dto.EffectiveTo;

            record.Update_by = HttpContext.GetName();
            record.Update_time = DateTime.Now;

            await _statutoryService.UpdateAsync(record);

            return SUCCESS(record);
        }

        /// <summary>
        /// Close/deactivate current statutory record
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="statutoryId">Statutory Record ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("{employeeId:long}/{statutoryId:long}")]
        [ActionPermissionFilter("hr:employeecore:statutory:delete")]
        [Log(Title = "Close Statutory Record", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CloseStatutoryRecord(long employeeId, long statutoryId)
        {
            var record = await _statutoryService.GetByIdAsync(statutoryId);
            if (record == null || record.EmployeeId != employeeId)
            {
                return ToResponse(ApiResult.Error("Statutory record not found"));
            }

            // Close the record
            record.EffectiveTo = DateTime.Now;
            record.Update_by = HttpContext.GetName();
            record.Update_time = DateTime.Now;

            await _statutoryService.UpdateAsync(record);

            return SUCCESS(record);
        }
    }
}
