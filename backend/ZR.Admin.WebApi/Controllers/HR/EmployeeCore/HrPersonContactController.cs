using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
    /// Employee Core - Person Contact Controller
    /// Handles ONLY: PersonContact (work email, personal email, phone, mobile)
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/contact")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrPersonContactController : BaseController
    {
        private readonly IHrPersonContactService _contactService;
        private readonly IHrEmployeeCoreService _employeeService;

        public HrPersonContactController(
            IHrPersonContactService contactService,
            IHrEmployeeCoreService employeeService)
        {
            _contactService = contactService;
            _employeeService = employeeService;
        }

        /// <summary>
        /// Get all contacts for an employee (by employee ID)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>List of all contact records for this employee's person</returns>
        [HttpGet("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:contact:query")]
        public async Task<IActionResult> GetContacts(long employeeId)
        {
            // Get employee to find person_id
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            var contacts = await _contactService.GetListAsync(x => x.PersonId == employee.PersonId);
            var contactsList = contacts.OrderByDescending(c => c.IsPrimary).ThenBy(c => c.ContactTypeDictCode).ToList();

            return SUCCESS(contactsList);
        }

        /// <summary>
        /// Get contacts grouped by type (work email, personal email, work phone, mobile)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Contacts grouped by type</returns>
        [HttpGet("{employeeId:long}/grouped")]
        [ActionPermissionFilter("hr:employeecore:contact:query")]
        public async Task<IActionResult> GetContactsGrouped(long employeeId)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            var contacts = await _contactService.GetListAsync(x => x.PersonId == employee.PersonId);
            var contactsList = contacts.ToList();

            // Group by contact type (1=Work Email, 2=Personal Email, 3=Work Phone, 4=Mobile)
            var grouped = new
            {
                WorkEmail = contactsList.FirstOrDefault(c => c.ContactTypeDictCode == 1)?.ContactValue,
                PersonalEmail = contactsList.FirstOrDefault(c => c.ContactTypeDictCode == 2)?.ContactValue,
                WorkPhone = contactsList.FirstOrDefault(c => c.ContactTypeDictCode == 3)?.ContactValue,
                Mobile = contactsList.FirstOrDefault(c => c.ContactTypeDictCode == 4)?.ContactValue,
                AllContacts = contactsList
            };

            return SUCCESS(grouped);
        }

        /// <summary>
        /// Create new contact for employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Contact data</param>
        /// <returns>Created contact ID</returns>
        [HttpPost("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:contact:add")]
        [Log(Title = "Create Contact", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateContact(long employeeId, [FromBody] CreateContactDto dto)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            var contact = new Model.HR.EmployeeCore.HrPersonContact
            {
                PersonId = employee.PersonId,
                ContactTypeDictCode = dto.ContactTypeDictCode,
                ContactValue = dto.ContactValue,
                IsPrimary = dto.IsPrimary,
                IsVerified = dto.IsVerified ?? false,
                EffectiveFrom = dto.EffectiveFrom ?? DateTime.Now,
                EffectiveTo = dto.EffectiveTo,
                Create_by = HttpContext.GetName(),
                Create_time = DateTime.Now
            };

            var contactId = await _contactService.AddAsync(contact);

            return SUCCESS(new { PersonContactId = contactId });
        }

        /// <summary>
        /// Update contact
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="dto">Updated contact data</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/{contactId:long}")]
        [ActionPermissionFilter("hr:employeecore:contact:edit")]
        [Log(Title = "Update Contact", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateContact(long employeeId, long contactId, [FromBody] UpdateContactDto dto)
        {
            var contact = await _contactService.GetByIdAsync(contactId);
            if (contact == null)
            {
                return ToResponse(ResultCode.FAIL, "Contact not found");
            }

            // Verify this contact belongs to this employee's person
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null || contact.PersonId != employee.PersonId)
            {
                return ToResponse(ResultCode.FAIL, "Contact does not belong to this employee");
            }

            if (dto.ContactTypeDictCode.HasValue)
                contact.ContactTypeDictCode = dto.ContactTypeDictCode.Value;

            if (dto.ContactValue != null)
                contact.ContactValue = dto.ContactValue;

            if (dto.IsPrimary.HasValue)
                contact.IsPrimary = dto.IsPrimary.Value;

            if (dto.IsVerified.HasValue)
                contact.IsVerified = dto.IsVerified.Value;

            if (dto.EffectiveTo.HasValue)
                contact.EffectiveTo = dto.EffectiveTo;

            contact.Update_by = HttpContext.GetName();
            contact.Update_time = DateTime.Now;

            await _contactService.UpdateAsync(contact);

            return SUCCESS(contact);
        }

        /// <summary>
        /// Update multiple contacts at once (work email, personal email, phone, mobile)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Batch contact updates</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/batch")]
        [ActionPermissionFilter("hr:employeecore:contact:edit")]
        [Log(Title = "Update Contacts Batch", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateContactsBatch(long employeeId, [FromBody] UpdateContactsBatchDto dto)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            var contacts = await _contactService.GetListAsync(x => x.PersonId == employee.PersonId);
            var contactsList = contacts.ToList();

            var now = DateTime.Now;
            var userName = HttpContext.GetName();

            // Update or create Work Email (Type 1)
            if (dto.Email != null)
            {
                var workEmail = contactsList.FirstOrDefault(c => c.ContactTypeDictCode == 1);
                if (workEmail != null)
                {
                    workEmail.ContactValue = dto.Email;
                    workEmail.Update_by = userName;
                    workEmail.Update_time = now;
                    await _contactService.UpdateAsync(workEmail);
                }
                else
                {
                    var newContact = new Model.HR.EmployeeCore.HrPersonContact
                    {
                        PersonId = employee.PersonId,
                        ContactTypeDictCode = 1,
                        ContactValue = dto.Email,
                        IsPrimary = true,
                        IsVerified = false,
                        EffectiveFrom = now,
                        Create_by = userName,
                        Create_time = now
                    };
                    await _contactService.AddAsync(newContact);
                }
            }

            // Update or create Personal Email (Type 2)
            if (dto.PersonalEmail != null)
            {
                var personalEmail = contactsList.FirstOrDefault(c => c.ContactTypeDictCode == 2);
                if (personalEmail != null)
                {
                    personalEmail.ContactValue = dto.PersonalEmail;
                    personalEmail.Update_by = userName;
                    personalEmail.Update_time = now;
                    await _contactService.UpdateAsync(personalEmail);
                }
                else
                {
                    var newContact = new Model.HR.EmployeeCore.HrPersonContact
                    {
                        PersonId = employee.PersonId,
                        ContactTypeDictCode = 2,
                        ContactValue = dto.PersonalEmail,
                        IsPrimary = false,
                        IsVerified = false,
                        EffectiveFrom = now,
                        Create_by = userName,
                        Create_time = now
                    };
                    await _contactService.AddAsync(newContact);
                }
            }

            // Update or create Work Phone (Type 3)
            if (dto.Phone != null)
            {
                var workPhone = contactsList.FirstOrDefault(c => c.ContactTypeDictCode == 3);
                if (workPhone != null)
                {
                    workPhone.ContactValue = dto.Phone;
                    workPhone.Update_by = userName;
                    workPhone.Update_time = now;
                    await _contactService.UpdateAsync(workPhone);
                }
                else
                {
                    var newContact = new Model.HR.EmployeeCore.HrPersonContact
                    {
                        PersonId = employee.PersonId,
                        ContactTypeDictCode = 3,
                        ContactValue = dto.Phone,
                        IsPrimary = true,
                        IsVerified = false,
                        EffectiveFrom = now,
                        Create_by = userName,
                        Create_time = now
                    };
                    await _contactService.AddAsync(newContact);
                }
            }

            // Update or create Mobile (Type 4)
            if (dto.MobileNumber != null)
            {
                var mobile = contactsList.FirstOrDefault(c => c.ContactTypeDictCode == 4);
                if (mobile != null)
                {
                    mobile.ContactValue = dto.MobileNumber;
                    mobile.Update_by = userName;
                    mobile.Update_time = now;
                    await _contactService.UpdateAsync(mobile);
                }
                else
                {
                    var newContact = new Model.HR.EmployeeCore.HrPersonContact
                    {
                        PersonId = employee.PersonId,
                        ContactTypeDictCode = 4,
                        ContactValue = dto.MobileNumber,
                        IsPrimary = false,
                        IsVerified = false,
                        EffectiveFrom = now,
                        Create_by = userName,
                        Create_time = now
                    };
                    await _contactService.AddAsync(newContact);
                }
            }

            return SUCCESS(new { message = "Contacts updated successfully" });
        }

        /// <summary>
        /// Delete contact
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("{employeeId:long}/{contactId:long}")]
        [ActionPermissionFilter("hr:employeecore:contact:delete")]
        [Log(Title = "Delete Contact", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteContact(long employeeId, long contactId)
        {
            var contact = await _contactService.GetByIdAsync(contactId);
            if (contact == null)
            {
                return ToResponse(ResultCode.FAIL, "Contact not found");
            }

            // Verify this contact belongs to this employee's person
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null || contact.PersonId != employee.PersonId)
            {
                return ToResponse(ResultCode.FAIL, "Contact does not belong to this employee");
            }

            // Soft delete - set EffectiveTo
            contact.EffectiveTo = DateTime.Now;
            contact.Update_by = HttpContext.GetName();
            contact.Update_time = DateTime.Now;

            await _contactService.UpdateAsync(contact);

            return SUCCESS(new { message = "Contact deleted successfully" });
        }
    }
}
