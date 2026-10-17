using Microsoft.AspNetCore.Mvc;
using ZR.Model.Sales;
using ZR.Model.Sales.Dto;
using ZR.Service.Sales.IService;
using ZR.Admin.WebApi.Filters;
using Mapster;

namespace ZR.Admin.WebApi.Controllers.Sales
{
    /// <summary>
    /// Customer Controller - متحكم العملاء
    /// </summary>
    [Verify]
    [Route("api/erp/customers")]
    [ApiController]
    public class ErpCustomerController : BaseController
    {
        private readonly IErpCustomerService _customerService;

        public ErpCustomerController(IErpCustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Get customer list - قائمة العملاء
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:customer:list")]
        public IActionResult GetList([FromQuery] ErpCustomerQueryDto parm)
        {
            var response = _customerService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get customer by ID - الحصول على عميل
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:customer:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _customerService.GetInfo(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Customer not found - العميل غير موجود");
            }
            var dto = response.Adapt<ErpCustomerDto>();
            return SUCCESS(dto);
        }

        /// <summary>
        /// Add customer - إضافة عميل
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:customer:add")]
        [Log(Title = "Customer", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] ErpCustomerDto dto)
        {
            if (_customerService.IsCustomerCodeExists(dto.CustomerCode))
            {
                return ToResponse(ResultCode.FAIL, "Customer code already exists - كود العميل موجود مسبقاً");
            }

            var model = dto.Adapt<ErpCustomer>();
       

            var result = _customerService.AddCustomer(model);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update customer - تعديل عميل
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:customer:edit")]
        [Log(Title = "Customer", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpCustomerDto dto)
        {
            if (_customerService.IsCustomerCodeExists(dto.CustomerCode, dto.CustomerId))
            {
                return ToResponse(ResultCode.FAIL, "Customer code already exists - كود العميل موجود مسبقاً");
            }

            var model = dto.Adapt<ErpCustomer>();
     

            var result = _customerService.UpdateCustomer(model);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete customers - حذف عملاء
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:customer:delete")]
        [Log(Title = "Customer", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ResultCode.FAIL); }

            var result = _customerService.Delete(idsArr);
            return ToResponse(result);
        }

        /// <summary>
        /// Get customer balance summary - ملخص رصيد العميل
        /// </summary>
        [HttpGet("{id}/balance")]
        [ActionPermissionFilter(Permission = "erp:customer:query")]
        public IActionResult GetBalanceSummary(long id)
        {
            var response = _customerService.GetBalanceSummary(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Customer not found - العميل غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Get customer statement - كشف حساب العميل
        /// </summary>
        [HttpGet("{id}/statement")]
        [ActionPermissionFilter(Permission = "erp:customer:statement")]
        public IActionResult GetStatement(long id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] Model.PagerInfo pager)
        {
            var response = _customerService.GetStatement(id, fromDate, toDate, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get customers with due balances - العملاء بأرصدة مستحقة
        /// </summary>
        [HttpGet("due-balances")]
        [ActionPermissionFilter(Permission = "erp:customer:list")]
        public IActionResult GetCustomersWithDueBalances()
        {
            var response = _customerService.GetCustomersWithDueBalances();
            return SUCCESS(response);
        }

        /// <summary>
        /// AI: Analyze customer patterns - تحليل أنماط العميل
        /// </summary>
        [HttpGet("{id}/ai/analyze")]
        [ActionPermissionFilter(Permission = "erp:customer:ai")]
        public async Task<IActionResult> AnalyzePatterns(long id)
        {
            var response = await _customerService.AnalyzeCustomerPatternsAsync(id);
            return SUCCESS(response);
        }

        /// <summary>
        /// AI: Predict churn risk - توقع خطر فقدان العميل
        /// </summary>
        [HttpGet("{id}/ai/churn-risk")]
        [ActionPermissionFilter(Permission = "erp:customer:ai")]
        public async Task<IActionResult> PredictChurnRisk(long id)
        {
            var risk = await _customerService.PredictChurnRiskAsync(id);
            return SUCCESS(new { CustomerId = id, ChurnRisk = risk, RiskLevel = risk > 0.6m ? "High" : risk > 0.3m ? "Medium" : "Low" });
        }

        /// <summary>
        /// AI: Get recommendations - الحصول على توصيات
        /// </summary>
        [HttpGet("{id}/ai/recommendations")]
        [ActionPermissionFilter(Permission = "erp:customer:ai")]
        public async Task<IActionResult> GetRecommendations(long id)
        {
            var recommendations = await _customerService.GetRecommendationsAsync(id);
            return SUCCESS(recommendations);
        }

        /// <summary>
        /// Export customers - تصدير العملاء
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:customer:export")]
        [Log(Title = "Customer", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpCustomerQueryDto parm)
        {
            parm.PageSize = 10000;
            var list = _customerService.GetList(parm).Result;
            var result = ExportExcelMini(list, "Customers", "Customers");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

