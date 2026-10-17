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
    /// Employee Core - Person Address Controller
    /// Handles ONLY: PersonAddress (residential address)
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/address")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrPersonAddressController : BaseController
    {
        private readonly IHrPersonAddressService _addressService;
        private readonly IHrEmployeeCoreService _employeeService;

        public HrPersonAddressController(
            IHrPersonAddressService addressService,
            IHrEmployeeCoreService employeeService)
        {
            _addressService = addressService;
            _employeeService = employeeService;
        }

        /// <summary>
        /// Get all addresses for an employee (by employee ID)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>List of all address records for this employee's person</returns>
        [HttpGet("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:address:query")]
        public async Task<IActionResult> GetAddresses(long employeeId)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            var addresses = await _addressService.GetListAsync(x => x.PersonId == employee.PersonId);
            var addressesList = addresses.OrderByDescending(a => a.IsPrimary).ThenBy(a => a.AddressTypeDictCode).ToList();

            return SUCCESS(addressesList);
        }

        /// <summary>
        /// Get primary address for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Primary address record</returns>
        [HttpGet("{employeeId:long}/primary")]
        [ActionPermissionFilter("hr:employeecore:address:query")]
        public async Task<IActionResult> GetPrimaryAddress(long employeeId)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            var addresses = await _addressService.GetListAsync(x => x.PersonId == employee.PersonId);
            var primaryAddress = addresses.FirstOrDefault(a => a.IsPrimary);

            if (primaryAddress == null)
            {
                return SUCCESS(new { HasAddress = false, Address = (object)null });
            }

            return SUCCESS(new { HasAddress = true, Address = primaryAddress });
        }

        /// <summary>
        /// Create new address for employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Address data</param>
        /// <returns>Created address ID</returns>
        [HttpPost("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:address:add")]
        [Log(Title = "Create Address", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateAddress(long employeeId, [FromBody] CreateAddressDto dto)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            // If this is primary, unset other primary addresses
            if (dto.IsPrimary)
            {
                var existingAddresses = await _addressService.GetListAsync(x => x.PersonId == employee.PersonId && x.IsPrimary);
                foreach (var addr in existingAddresses)
                {
                    addr.IsPrimary = false;
                    addr.Update_by = HttpContext.GetName();
                    addr.Update_time = DateTime.Now;
                    await _addressService.UpdateAsync(addr);
                }
            }

            var address = new Model.HR.EmployeeCore.HrPersonAddress
            {
                PersonId = employee.PersonId,
                AddressTypeDictCode = dto.AddressTypeDictCode ?? 1, // Default to Home Address
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                PostalCode = dto.PostalCode,
                CountryDictCode = dto.CountryDictCode,
                IsPrimary = dto.IsPrimary,
                EffectiveFrom = dto.EffectiveFrom ?? DateTime.Now,
                EffectiveTo = dto.EffectiveTo,
                Create_by = HttpContext.GetName(),
                Create_time = DateTime.Now
            };

            var addressId = await _addressService.AddAsync(address);

            return SUCCESS(new { PersonAddressId = addressId });
        }

        /// <summary>
        /// Update address
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="addressId">Address ID</param>
        /// <param name="dto">Updated address data</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/{addressId:long}")]
        [ActionPermissionFilter("hr:employeecore:address:edit")]
        [Log(Title = "Update Address", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateAddress(long employeeId, long addressId, [FromBody] UpdateAddressDto dto)
        {
            var address = await _addressService.GetByIdAsync(addressId);
            if (address == null)
            {
                return ToResponse(ResultCode.FAIL, "Address not found");
            }

            // Verify this address belongs to this employee's person
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null || address.PersonId != employee.PersonId)
            {
                return ToResponse(ResultCode.FAIL, "Address does not belong to this employee");
            }

            // If setting as primary, unset other primary addresses
            if (dto.IsPrimary.HasValue && dto.IsPrimary.Value)
            {
                var existingPrimary = await _addressService.GetListAsync(x => x.PersonId == employee.PersonId && x.IsPrimary && x.PersonAddressId != addressId);
                foreach (var addr in existingPrimary)
                {
                    addr.IsPrimary = false;
                    addr.Update_by = HttpContext.GetName();
                    addr.Update_time = DateTime.Now;
                    await _addressService.UpdateAsync(addr);
                }
            }

            if (dto.AddressTypeDictCode.HasValue)
                address.AddressTypeDictCode = dto.AddressTypeDictCode.Value;

            if (dto.AddressLine1 != null)
                address.AddressLine1 = dto.AddressLine1;

            if (dto.AddressLine2 != null)
                address.AddressLine2 = dto.AddressLine2;

            if (dto.City != null)
                address.City = dto.City;

            if (dto.PostalCode != null)
                address.PostalCode = dto.PostalCode;

            if (dto.CountryDictCode.HasValue)
                address.CountryDictCode = dto.CountryDictCode;

            if (dto.IsPrimary.HasValue)
                address.IsPrimary = dto.IsPrimary.Value;

            if (dto.EffectiveTo.HasValue)
                address.EffectiveTo = dto.EffectiveTo;

            address.Update_by = HttpContext.GetName();
            address.Update_time = DateTime.Now;

            await _addressService.UpdateAsync(address);

            return SUCCESS(address);
        }

        /// <summary>
        /// Update primary address (simple update for single address field)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Simple address update</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/primary")]
        [ActionPermissionFilter("hr:employeecore:address:edit")]
        [Log(Title = "Update Primary Address", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdatePrimaryAddress(long employeeId, [FromBody] UpdatePrimaryAddressDto dto)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            var addresses = await _addressService.GetListAsync(x => x.PersonId == employee.PersonId);
            var primaryAddress = addresses.FirstOrDefault(a => a.IsPrimary);

            var now = DateTime.Now;
            var userName = HttpContext.GetName();

            if (primaryAddress != null)
            {
                // Update existing primary address
                primaryAddress.AddressLine1 = dto.Address;
                primaryAddress.Update_by = userName;
                primaryAddress.Update_time = now;
                await _addressService.UpdateAsync(primaryAddress);
            }
            else
            {
                // Create new primary address
                var newAddress = new Model.HR.EmployeeCore.HrPersonAddress
                {
                    PersonId = employee.PersonId,
                    AddressTypeDictCode = 1, // Home Address
                    AddressLine1 = dto.Address,
                    IsPrimary = true,
                    EffectiveFrom = now,
                    Create_by = userName,
                    Create_time = now
                };
                await _addressService.AddAsync(newAddress);
            }

            return SUCCESS(new { message = "Address updated successfully" });
        }

        /// <summary>
        /// Delete address
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="addressId">Address ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("{employeeId:long}/{addressId:long}")]
        [ActionPermissionFilter("hr:employeecore:address:delete")]
        [Log(Title = "Delete Address", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteAddress(long employeeId, long addressId)
        {
            var address = await _addressService.GetByIdAsync(addressId);
            if (address == null)
            {
                return ToResponse(ResultCode.FAIL, "Address not found");
            }

            // Verify this address belongs to this employee's person
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null || address.PersonId != employee.PersonId)
            {
                return ToResponse(ResultCode.FAIL, "Address does not belong to this employee");
            }

            // Soft delete - set EffectiveTo
            address.EffectiveTo = DateTime.Now;
            address.Update_by = HttpContext.GetName();
            address.Update_time = DateTime.Now;

            await _addressService.UpdateAsync(address);

            return SUCCESS(new { message = "Address deleted successfully" });
        }
    }
}
