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
    /// Employee Core - Emergency Contact Controller
    /// Handles ONLY: EmployeeEmergencyContact (emergency contact person)
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/emergencycontact")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrEmployeeEmergencyContactController : BaseController
    {
        private readonly IHrEmployeeEmergencyContactService _emergencyContactService;

        public HrEmployeeEmergencyContactController(IHrEmployeeEmergencyContactService emergencyContactService)
        {
            _emergencyContactService = emergencyContactService;
        }

        /// <summary>
        /// Get all emergency contacts for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>List of all emergency contacts</returns>
        [HttpGet("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:emergencycontact:query")]
        public async Task<IActionResult> GetEmergencyContacts(long employeeId)
        {
            var contacts = await _emergencyContactService.GetListAsync(x => x.EmployeeId == employeeId && x.IsActive);
            var contactsList = contacts.OrderByDescending(c => c.IsPrimary).ToList();

            return SUCCESS(contactsList);
        }

        /// <summary>
        /// Get primary emergency contact for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Primary emergency contact record</returns>
        [HttpGet("{employeeId:long}/primary")]
        [ActionPermissionFilter("hr:employeecore:emergencycontact:query")]
        public async Task<IActionResult> GetPrimaryEmergencyContact(long employeeId)
        {
            var contacts = await _emergencyContactService.GetListAsync(x => x.EmployeeId == employeeId && x.IsActive);
            var primaryContact = contacts.FirstOrDefault(c => c.IsPrimary);

            if (primaryContact == null)
            {
                return SUCCESS(new { HasEmergencyContact = false, Contact = (object)null });
            }

            return SUCCESS(new { HasEmergencyContact = true, Contact = primaryContact });
        }

        /// <summary>
        /// Create new emergency contact
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Emergency contact data</param>
        /// <returns>Created contact ID</returns>
        [HttpPost("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:emergencycontact:add")]
        [Log(Title = "Create Emergency Contact", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateEmergencyContact(long employeeId, [FromBody] CreateEmergencyContactDto dto)
        {
            // If this is primary, unset other primary contacts
            if (dto.IsPrimary)
            {
                var existingContacts = await _emergencyContactService.GetListAsync(x => x.EmployeeId == employeeId && x.IsPrimary && x.IsActive);
                foreach (var contact in existingContacts)
                {
                    contact.IsPrimary = false;
                    contact.Update_by = HttpContext.GetName();
                    contact.Update_time = DateTime.Now;
                    await _emergencyContactService.UpdateAsync(contact);
                }
            }

            var emergencyContact = new Model.HR.EmployeeCore.HrEmployeeEmergencyContact
            {
                EmployeeId = employeeId,
                ContactName = dto.ContactName,
                RelationshipDictCode = dto.RelationshipDictCode,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                IsPrimary = dto.IsPrimary,
                IsActive = true,
                Create_by = HttpContext.GetName(),
                Create_time = DateTime.Now
            };

            var contactId = await _emergencyContactService.AddAsync(emergencyContact);

            return SUCCESS(new { EmployeeEmergencyContactId = contactId });
        }

        /// <summary>
        /// Update emergency contact
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="dto">Updated data</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/{contactId:long}")]
        [ActionPermissionFilter("hr:employeecore:emergencycontact:edit")]
        [Log(Title = "Update Emergency Contact", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateEmergencyContact(long employeeId, long contactId, [FromBody] UpdateEmergencyContactDto dto)
        {
            var contact = await _emergencyContactService.GetByIdAsync(contactId);
            if (contact == null || contact.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.FAIL, "Emergency contact not found");
            }

            // If setting as primary, unset other primary contacts
            if (dto.IsPrimary.HasValue && dto.IsPrimary.Value)
            {
                var existingPrimary = await _emergencyContactService.GetListAsync(x => x.EmployeeId == employeeId && x.IsPrimary && x.IsActive && x.EmergencyContactId != contactId);
                foreach (var ec in existingPrimary)
                {
                    ec.IsPrimary = false;
                    ec.Update_by = HttpContext.GetName();
                    ec.Update_time = DateTime.Now;
                    await _emergencyContactService.UpdateAsync(ec);
                }
            }

            if (dto.ContactName != null)
                contact.ContactName = dto.ContactName;

            if (dto.RelationshipDictCode.HasValue)
                contact.RelationshipDictCode = dto.RelationshipDictCode;

            if (dto.Phone != null)
                contact.Phone = dto.Phone;

            if (dto.Email != null)
                contact.Email = dto.Email;

            if (dto.Address != null)
                contact.Address = dto.Address;

            if (dto.IsPrimary.HasValue)
                contact.IsPrimary = dto.IsPrimary.Value;

            contact.Update_by = HttpContext.GetName();
            contact.Update_time = DateTime.Now;

            await _emergencyContactService.UpdateAsync(contact);

            return SUCCESS(contact);
        }

        /// <summary>
        /// Update primary emergency contact (simple update)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Simple emergency contact update</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/primary")]
        [ActionPermissionFilter("hr:employeecore:emergencycontact:edit")]
        [Log(Title = "Update Primary Emergency Contact", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdatePrimaryEmergencyContact(long employeeId, [FromBody] UpdatePrimaryEmergencyContactDto dto)
        {
            var contacts = await _emergencyContactService.GetListAsync(x => x.EmployeeId == employeeId && x.IsActive);
            var primaryContact = contacts.FirstOrDefault(c => c.IsPrimary);

            var now = DateTime.Now;
            var userName = HttpContext.GetName();

            if (primaryContact != null)
            {
                // Update existing primary contact
                primaryContact.ContactName = dto.EmergencyContactName;
                primaryContact.Phone = dto.EmergencyContactPhone;
                primaryContact.Update_by = userName;
                primaryContact.Update_time = now;
                await _emergencyContactService.UpdateAsync(primaryContact);
            }
            else
            {
                // Create new primary contact
                var newContact = new Model.HR.EmployeeCore.HrEmployeeEmergencyContact
                {
                    EmployeeId = employeeId,
                    ContactName = dto.EmergencyContactName,
                    Phone = dto.EmergencyContactPhone,
                    IsPrimary = true,
                    IsActive = true,
                    Create_by = userName,
                    Create_time = now
                };
                await _emergencyContactService.AddAsync(newContact);
            }

            return SUCCESS(new { message = "Emergency contact updated successfully" });
        }

        /// <summary>
        /// Delete (deactivate) emergency contact
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("{employeeId:long}/{contactId:long}")]
        [ActionPermissionFilter("hr:employeecore:emergencycontact:delete")]
        [Log(Title = "Delete Emergency Contact", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteEmergencyContact(long employeeId, long contactId)
        {
            var contact = await _emergencyContactService.GetByIdAsync(contactId);
            if (contact == null || contact.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.FAIL, "Emergency contact not found");
            }

            // Soft delete - mark as inactive
            contact.IsActive = false;
            contact.Update_by = HttpContext.GetName();
            contact.Update_time = DateTime.Now;

            await _emergencyContactService.UpdateAsync(contact);

            return SUCCESS(new { message = "Emergency contact deleted successfully" });
        }
    }
}
