using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Customers.Dto;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;
using ZR.Service.Contracts.IService;
using Infrastructure;
using Infrastructure.Model;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Operations Customer Controller - Read-only access to active customers
    /// Shows customers transferred from sales (deal Won) and other active customers
    /// </summary>
    [Verify]
    [Route("api/operations/customers")]
    public class OperationsCustomerController : BaseController
    {
        private readonly ICustomerService _customerService;
        private readonly IContractService _contractService;

        public OperationsCustomerController(ICustomerService customerService, IContractService contractService)
        {
            _customerService = customerService;
            _contractService = contractService;
        }

        /// <summary>
        /// Get customers list (active customers including those converted from leads)
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("operations:customers:list")]
        public IActionResult GetCustomersList([FromQuery] CustomerQueryDto query, [FromQuery] PagerInfo pager)
        {
            // Default to showing only active customers for operations
            if (!query.IsActive.HasValue)
            {
                query.IsActive = true;
            }

            var result = _customerService.GetList(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter("operations:customers:list")]
        public IActionResult GetCustomer(Guid id)
        {
            var customer = _customerService.GetById(id);
            if (customer == null)
                return ToResponse(ResultCode.FAIL, "Customer not found");

            return SUCCESS(customer);
        }

        /// <summary>
        /// Get active customers for dropdown
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter("operations:customers:list")]
        public IActionResult GetActiveCustomers()
        {
            return SUCCESS(_customerService.GetActiveCustomers());
        }

        /// <summary>
        /// Update customer profile (operations can complete missing fields)
        /// تحديث بيانات العميل - إكمال الملف الشخصي
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter("operations:customers:edit")]
        public IActionResult UpdateCustomer([FromBody] UpdateCustomerDto dto)
        {
            var existing = _customerService.GetById(dto.Id);
            if (existing == null)
                return ToResponse(ResultCode.FAIL, "Customer not found");

            var currentUser = JwtUtil.GetLoginUser(HttpContext);
            var result = _customerService.Update(dto, currentUser?.UserName);

            return result > 0
                ? SUCCESS(result)
                : ToResponse(ResultCode.FAIL, "Failed to update customer");
        }

        /// <summary>
        /// Delete customer - only allowed if customer has no contracts
        /// حذف عميل - مسموح فقط إذا لم يكن لديه عقود
        /// </summary>
        [HttpDelete("{id}")]
        [ActionPermissionFilter("operations:customers:delete")]
        public IActionResult DeleteCustomer(Guid id)
        {
            var existing = _customerService.GetById(id);
            if (existing == null)
                return ToResponse(ResultCode.FAIL, "Customer not found");

            // Check if customer has any contracts
            var contracts = _contractService.GetByCustomerId(id);
            if (contracts != null && contracts.Count > 0)
                return ToResponse(ResultCode.FAIL, "لا يمكن حذف العميل لأنه مرتبط بعقود - Cannot delete customer with existing contracts");

            var result = _customerService.Delete(new[] { id });
            return result > 0
                ? SUCCESS(result)
                : ToResponse(ResultCode.FAIL, "Failed to delete customer");
        }
    }
}
