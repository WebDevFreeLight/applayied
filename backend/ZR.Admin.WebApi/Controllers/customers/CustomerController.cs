using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Customers.Dto;
using ZR.Service.IService.Customers;

namespace ZR.Admin.WebApi.Controllers.Customers
{
    /// <summary>
    /// Customer Controller - متحكم العملاء
    /// </summary>
    [Verify]
    [Route("finance/arap/customers")]
    [ApiController]
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Get list of customers - الحصول على قائمة العملاء
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:arap:customer:list")]
        public IActionResult GetList([FromQuery] CustomerQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_customerService.GetList(query, pager));
        }

        /// <summary>
        /// Get customer by ID - الحصول على العميل بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:arap:customer:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _customerService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Customer not found - العميل غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create customer - إنشاء عميل
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:arap:customer:add")]
        [Log(Title = "Customer", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            if (_customerService.CodeExists(dto.CustomerCode))
                return ToResponse(ResultCode.FAIL, "Customer code already exists - رمز العميل موجود مسبقاً");

            var result = _customerService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Update customer - تحديث عميل
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:arap:customer:edit")]
        [Log(Title = "Customer", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            if (_customerService.CodeExists(dto.CustomerCode, dto.Id))
                return ToResponse(ResultCode.FAIL, "Customer code already exists - رمز العميل موجود مسبقاً");

            return ToResponse(_customerService.Update(dto, HttpContext.GetName()));
        }

        /// <summary>
        /// Delete customers - حذف عملاء
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:arap:customer:delete")]
        [Log(Title = "Customer", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
            if (idsArr.Length <= 0)
                return ToResponse(ResultCode.FAIL);

            var result = _customerService.Delete(idsArr);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Cannot delete customers with invoices - لا يمكن حذف عملاء لديهم فواتير");

            return ToResponse(result);
        }

        /// <summary>
        /// Check if code exists - التحقق من وجود رمز العميل
        /// </summary>
        [HttpGet("check-code")]
        [ActionPermissionFilter(Permission = "finance:arap:customer:query")]
        public IActionResult CheckCodeExists([FromQuery] string code, [FromQuery] Guid? excludeId)
        {
            var exists = _customerService.CodeExists(code, excludeId);
            return SUCCESS(new { Exists = exists });
        }

        /// <summary>
        /// Get active customers for dropdown - الحصول على العملاء النشطين للقائمة المنسدلة
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "finance:arap:customer:query")]
        public IActionResult GetActiveCustomers()
        {
            var result = _customerService.GetActiveCustomers();
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate next customer code - توليد رمز العميل التالي
        /// </summary>
        [HttpGet("next-code")]
        [ActionPermissionFilter(Permission = "finance:arap:customer:query")]
        public IActionResult GetNextCustomerCode()
        {
            var code = _customerService.GenerateNextCustomerCode();
            return SUCCESS(new { CustomerCode = code });
        }
    }
}
