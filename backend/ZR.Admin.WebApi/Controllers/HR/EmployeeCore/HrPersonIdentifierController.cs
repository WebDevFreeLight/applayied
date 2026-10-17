using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.EmployeeCore;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Person Identifier Controller - CRUD operations for identifiers
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/person/{personId}/identifier")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrPersonIdentifierController : BaseController
    {
        private readonly IHrPersonIdentifierService _identifierService;

        public HrPersonIdentifierController(IHrPersonIdentifierService identifierService)
        {
            _identifierService = identifierService;
        }

        /// <summary>
        /// Get all identifiers for a person
        /// </summary>
        [HttpGet("")]
        [ActionPermissionFilter("hr:employeecore:person:query")]
        public async Task<IActionResult> GetIdentifiers(long personId)
        {
            try
            {
                var identifiers = await _identifierService.GetListAsync(i => i.PersonId == personId);
                return SUCCESS(identifiers);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get identifiers: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new identifier
        /// </summary>
        [HttpPost("")]
        [ActionPermissionFilter("hr:employeecore:person:edit")]
        [Log(Title = "Person Identifier", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateIdentifier(long personId, [FromBody] HrPersonIdentifier identifier)
        {
            try
            {
                identifier.PersonId = personId;
                identifier.Create_by = HttpContext.GetName();
                identifier.Create_time = DateTime.Now;

                var result = await _identifierService.AddAsync(identifier);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to create identifier: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an identifier
        /// </summary>
        [HttpPut("{identifierId}")]
        [ActionPermissionFilter("hr:employeecore:person:edit")]
        [Log(Title = "Person Identifier", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateIdentifier(long personId, long identifierId, [FromBody] HrPersonIdentifier identifier)
        {
            try
            {
                var existing = await _identifierService.GetByIdAsync(identifierId);
                if (existing == null || existing.PersonId != personId)
                {
                    return ToResponse(ResultCode.FAIL, "Identifier not found");
                }

                identifier.PersonIdentifierId = identifierId;
                identifier.PersonId = personId;
                identifier.Update_by = HttpContext.GetName();
                identifier.Update_time = DateTime.Now;

                var result = await _identifierService.UpdateAsync(identifier);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to update identifier: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete an identifier
        /// </summary>
        [HttpDelete("{identifierId}")]
        [ActionPermissionFilter("hr:employeecore:person:edit")]
        [Log(Title = "Person Identifier", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteIdentifier(long personId, long identifierId)
        {
            try
            {
                var existing = await _identifierService.GetByIdAsync(identifierId);
                if (existing == null || existing.PersonId != personId)
                {
                    return ToResponse(ResultCode.FAIL, "Identifier not found");
                }

                var result = await _identifierService.DeleteAsync(new long[] { identifierId });
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to delete identifier: {ex.Message}");
            }
        }

        /// <summary>
        /// Verify an identifier
        /// </summary>
        [HttpPost("{identifierId}/verify")]
        [ActionPermissionFilter("hr:employeecore:person:edit")]
        [Log(Title = "Person Identifier", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> VerifyIdentifier(long personId, long identifierId)
        {
            try
            {
                var identifier = await _identifierService.GetByIdAsync(identifierId);
                if (identifier == null || identifier.PersonId != personId)
                {
                    return ToResponse(ResultCode.FAIL, "Identifier not found");
                }

                identifier.IsVerified = true;
                identifier.VerifiedTime = DateTime.Now;
                identifier.Update_by = HttpContext.GetName();
                identifier.Update_time = DateTime.Now;

                var result = await _identifierService.UpdateAsync(identifier);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to verify identifier: {ex.Message}");
            }
        }
    }
}
