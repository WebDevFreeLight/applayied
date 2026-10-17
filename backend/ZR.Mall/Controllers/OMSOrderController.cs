using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using ZR.Common;
using ZR.Mall.Model;
using ZR.Mall.Model.Dto;
using ZR.Mall.Service.IService;

//创建时间：2025-05-30
namespace ZR.Mall.Controllers
{
    /// <summary>
    /// 订单管理
    /// </summary>
    [Route("shopping/Order")]
    [ApiExplorerSettings(GroupName = "shopping")]
    public class OMSOrderController : BaseController
    {
        private NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 订单管理接口
        /// </summary>
        private readonly IOMSOrderService _OMSOrderService;

        public OMSOrderController(IOMSOrderService OMSOrderService)
        {
            _OMSOrderService = OMSOrderService;
        }

        /// <summary>
        /// 查询订单管理列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "oms:order:list")]
        public IActionResult QueryOMSOrder([FromQuery] OMSOrderQueryDto parm)
        {
            var response = _OMSOrderService.GetList(parm);
            response.Extra.Add("NotDelivereOrder", _OMSOrderService.NotDelivereOrder());
            return SUCCESS(response);
        }

        /// <summary>
        /// 查询订单管理详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "oms:order:query")]
        public IActionResult GetOMSOrder(long Id)
        {
            var response = _OMSOrderService.GetInfo(Id);

            var info = response.Adapt<OMSOrderDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 更新订单管理
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Log(Title = "订单管理", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateOMSOrder([FromBody] OMSOrderDto parm)
        {
            var modal = parm.Adapt<OMSOrder>().ToUpdate(HttpContext);
            var response = _OMSOrderService.UpdateOMSOrder(parm.OperType, modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除订单管理
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "oms:order:delete")]
        [Log(Title = "订单管理", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteOMSOrder([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_OMSOrderService.Deleteable()
                .Where(f => f.IsDelete == 0)
                .In(idArr)
                .IsLogic()
                .ExecuteCommand());
        }

        /// <summary>
        /// 订单退款
        /// </summary>
        /// <returns></returns>
        [HttpPost("refund")]
        [Log(Title = "订单退款", BusinessType = BusinessType.UPDATE)]
        public IActionResult RefundOrder([FromBody] OMSOrderDto parm)
        {
            var modal = parm.Adapt<OMSOrder>().ToUpdate(HttpContext);
            var response = _OMSOrderService.UpdateOMSOrder(4, modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 导出订单管理
        /// </summary>
        /// <returns></returns>
        [Log(Title = "订单管理", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "oms:order:export")]
        public IActionResult Export([FromQuery] OMSOrderQueryDto parm)
        {
            var list = _OMSOrderService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "订单管理", "订单管理");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 订单发货
        /// </summary>
        /// <returns></returns>
        [HttpPut("delivery")]
        [Log(Title = "订单发货", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Delivery([FromBody] OMSOrderDto parm)
        {
            var modal = parm.Adapt<OMSOrder>().ToUpdate(HttpContext);
            var response = await _OMSOrderService.OrderDelivery(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 导出待发货订单
        /// </summary>
        /// <returns></returns>
        [Log(Title = "导出待发货订单", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("exportDelivery")]
        [ActionPermissionFilter(Permission = "oms:order:ship")]
        public async Task<IActionResult> ExportExpress([FromQuery] OMSOrderQueryDto parm)
        {
            if (parm == null || parm.BeginCreate_time == null)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "请选择时间");
            }
            var list = await _OMSOrderService.ExportWaitDeliveryList(parm);
            var result = await ExportExcelMiniAsync(list, "待发货", "待发货订单");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 批量发货
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "批量发货", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false, IsSaveResponseData = true)]
        [ActionPermissionFilter(Permission = "oms:order:ship")]
        public async Task<IActionResult> ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            if (formFile == null || formFile.Length <= 0)
            {
                return ToResponse(ResultCode.FAIL, "请选择要导入的文件");
            }
            var resultList = new List<DeliveryExpressDto>();
            using var stream = formFile.OpenReadStream();
            var rows = await stream.QueryAsync<DeliveryExpressDto>(startCell: "A1");

            var orderNos = rows.Select(x => x.OrderNo).Distinct().ToList();
            var allOrders = await _OMSOrderService.Queryable().In(x => x.OrderNo, orderNos).ToListAsync();

            foreach (var item in rows)
            {
                if (string.IsNullOrWhiteSpace(item.DeliveryCompany) || string.IsNullOrWhiteSpace(item.DeliveryNo))
                {
                    item.Status = "缺少快递信息"; 
                    resultList.Add(item);
                    continue;
                }
                var orderInfo = allOrders.FirstOrDefault(f => f.OrderNo == item.OrderNo);
                if (orderInfo == null)
                {
                    item.Status = "订单号不存在";
                    resultList.Add(item);
                    continue;
                }
                if (orderInfo.DeliveryStatus != Enum.DeliveryStatusEnum.NotDelivered)
                {
                    item.Status = "已发货";
                    resultList.Add(item);
                    continue;
                }
                var order = item.Adapt<OMSOrder>();
                
                var result =  await _OMSOrderService.OrderDelivery(order);
                item.Status = result > 0 ? "发货成功" : "发货失败";
                resultList.Add(item);
            }

            return SUCCESS(new
            {
                total = resultList.Count,
                successCount = resultList.Count(x => x.Status == "发货成功"),
                failCount = resultList.Count(x => x.Status != "发货成功"),
                result = resultList
            });
        }

        /// <summary>
        /// 查询销售总
        /// </summary>
        /// <returns></returns>
        [HttpGet("getSales")]
        [ActionPermissionFilter(Permission = "oms:sale:query")]
        public async Task<IActionResult> GetSales(OMSOrderQueryDto dto)
        {
            var response = await _OMSOrderService.GetTotalSales(dto);

            return SUCCESS(response);
        }

        /// <summary>
        /// 查询销售趋势
        /// </summary>
        /// <returns></returns>
        [HttpGet("getSalesTrade")]
        [ActionPermissionFilter(Permission = "oms:sale:query")]
        public async Task<IActionResult> GetSalesTrade(OMSOrderQueryDto dto)
        {
            var response = await _OMSOrderService.GetSaleTreandByDay(dto);

            return SUCCESS(response);
        }

        /// <summary>
        /// 查询销售前10的商品
        /// </summary>
        /// <returns></returns>
        [HttpGet("getSaleTopProduct")]
        [ActionPermissionFilter(Permission = "oms:sale:query")]
        public async Task<IActionResult> GetSaleTopProduct(OMSOrderQueryDto dto)
        {
            var response = await _OMSOrderService.GetSaleTopProduct(dto);

            return SUCCESS(response);
        }
    }
}
