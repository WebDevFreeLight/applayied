using Infrastructure.Extensions;
using ZR.Mall.Enum;
using ZR.Mall.Model;
using ZR.Mall.Model.Dto;
using ZR.Mall.Service.IService;

namespace ZR.Mall.Service
{
    /// <summary>
    /// 订单管理Service业务层处理
    /// </summary>
    [AppService(ServiceType = typeof(IOMSOrderService))]
    public class OMSOrderService : BaseService<OMSOrder>, IOMSOrderService
    {
        private ISkusService _shopSkusService;

        public OMSOrderService(ISkusService shopSkusService)
        {
            _shopSkusService = shopSkusService;
        }

        /// <summary>
        /// 查询订单管理列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<OMSOrderDto> GetList(OMSOrderQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Includes(x => x.Items)
                .Where(predicate.ToExpression())
                .ToPage<OMSOrder, OMSOrderDto>(parm);

            return response;
        }

        /// <summary>
        /// 查询未发货的订单数
        /// </summary>
        /// <returns></returns>
        public int NotDelivereOrder()
        {
            return Queryable()
                .Where(f => f.OrderStatus == Enum.OrderStatusEnum.Completed && f.DeliveryStatus == Enum.DeliveryStatusEnum.NotDelivered)
                .Count();
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OMSOrder GetInfo(long Id)
        {
            var response = Queryable()
                .Includes(x => x.Items)
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// 修改订单管理
        /// </summary>
        /// <param name="operType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateOMSOrder(int operType, OMSOrder model)
        {
            //修改商家备注
            if (operType == 2)
            {
                return UpdateMerchantNote(model);
            }
            //修改地址
            if (operType == 3)
            {
                var result = Update(w => w.Id == model.Id, it => new OMSOrder()
                {
                    AddressSnapshot = model.AddressSnapshot,
                });
                return result;
            }
            //订单退款
            if (operType == 4)
            {
                //TODO 退款逻辑

                throw new CustomException("还未实现");
            }
            return Update(model, true);
        }

        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> OrderDelivery(OMSOrder model)
        {
            var dbDate = Context.GetDate();
            var result = await UpdateAsync(w => w.OrderNo == model.OrderNo, it => new OMSOrder()
            {
                DeliveryCompany = model.DeliveryCompany,
                DeliveryNo = model.DeliveryNo,
                OrderStatus = Enum.OrderStatusEnum.Shipped, // 已发货
                DeliveryStatus = Enum.DeliveryStatusEnum.Delivering, // 已发货
                ShipTime = dbDate
            });
            //TODO 发送消息通知用户,订单完整日志
            return result;
        }

        /// <summary>
        /// 修改平台备注
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateMerchantNote(OMSOrder model)
        {
            var result = Update(w => w.OrderNo == model.OrderNo, it => new OMSOrder()
            {
                MerchantNote = model.MerchantNote,
            });
            return result;
        }

        /// <summary>
        /// 导出订单管理
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<OMSOrderDto> ExportList(OMSOrderQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new OMSOrderDto()
                {
                }, true)
                .Mapper(it =>
                {
                    it.User = $"{it.AddressSnapshot?.UserName} {it.AddressSnapshot?.Phone}"; 
                })
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// 导出代发货订单
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public async Task<List<DeliveryExpressDto>> ExportWaitDeliveryList(OMSOrderQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            var predicate = Expressionable.Create<OMSOrder>();

            predicate = predicate.And(it => it.OrderStatus == Enum.OrderStatusEnum.TobeShipped);
            predicate = predicate.And(it => it.DeliveryStatus == Enum.DeliveryStatusEnum.NotDelivered);
            predicate = predicate.And(it => it.PayTime >= parm.BeginCreate_time && it.PayTime <= parm.EndCreate_time);

            var response = await Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new DeliveryExpressDto()
                {
                    DeliveryCompany = it.DeliveryCompany,
                    DeliveryNo = it.DeliveryNo,
                    OrderNo = it.OrderNo,
                })
                .ToListAsync();

            return response;
        }

        /// <summary>
        /// 总销售额和订单数
        /// </summary>
        /// <returns></returns>
        public async Task<dynamic> GetTotalSales(OMSOrderQueryDto dto)
        {
            var result = await Queryable()
                //.Where(o => o.OrderStatus == Enum.OrderStatusEnum.Completed)
                .WhereIF(dto.BeginCreate_time != null, o => o.PayTime >= dto.BeginCreate_time)
                .WhereIF(dto.BeginCreate_time != null, o => o.PayTime <= dto.EndCreate_time)
                .Select(o => new
                {
                    TotalSales = SqlFunc.AggregateSum(o.PayAmount),
                    OrderCount = SqlFunc.AggregateCount(o.Id)
                })
                .FirstAsync();
            return result;
        }

        /// <summary>
        /// 销售趋势（按天）
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<dynamic> GetSaleTreandByDay(OMSOrderQueryDto dto)
        {
            var trend = await Queryable()
            .WhereIF(dto.BeginCreate_time == null, o => o.PayTime >= DateTime.Now.AddDays(-7))
            .WhereIF(dto.BeginCreate_time != null, o => o.PayTime >= dto.BeginCreate_time)
            .WhereIF(dto.BeginCreate_time != null, o => o.PayTime <= dto.EndCreate_time)
            .GroupBy(o => SqlFunc.ToDateShort(o.PayTime))
            .OrderBy(o => SqlFunc.ToDateShort(o.PayTime))
            .Select(o => new
            {
                Date = SqlFunc.ToDateShort(o.PayTime),
                TotalSales = SqlFunc.AggregateSum(o.PayAmount),
                OrderCount = SqlFunc.AggregateCount(o.Id)
            })
            .ToListAsync();

            return trend;
        }

        /// <summary>
        /// 销售排名前10的商品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<dynamic> GetSaleTopProduct(OMSOrderQueryDto dto)
        {
            var topProducts = await Context.Queryable<OMSOrderItem>()
                .InnerJoin<OMSOrder>((oi, o) => oi.OrderId == o.Id)
                .WhereIF(dto.BeginCreate_time != null, (oi, o) => o.PayTime >= dto.BeginCreate_time)
                .WhereIF(dto.BeginCreate_time != null, (oi, o) => o.PayTime <= dto.EndCreate_time)
                .GroupBy((oi, o) => oi.ProductId)
                .OrderBy((oi, o) => SqlFunc.AggregateSum(oi.Quantity), OrderByType.Desc)
                .Select((oi, o) => new
                {
                    oi.ProductId,
                    TotalSold = SqlFunc.AggregateSum(oi.Quantity),
                    TotalSales = SqlFunc.AggregateSum(oi.Quantity * oi.TotalPrice)
                })
                .Take(10)
                .MergeTable()
                .LeftJoin<Product>((it, p) => it.ProductId == p.ProductId)
                .Select((it, p) => new
                {
                    p.ProductName,
                    it.TotalSold,
                    it.TotalSales
                })
                .ToListAsync();

            return topProducts;
        }

        /// <summary>
        /// 查询导出表达式
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<OMSOrder> QueryExp(OMSOrderQueryDto parm)
        {
            var predicate = Expressionable.Create<OMSOrder>();
            if (parm.OrderStatus == null && parm.BeginCreate_time == null)
            {
                predicate = predicate.And(it => it.Create_time >= DateTime.Now.AddDays(-7).ToShortDateString().ParseToDateTime());
            }
            else
            {
                predicate = predicate.AndIF(parm.EndCreate_time != null, it => it.Create_time >= parm.BeginCreate_time && it.Create_time <= parm.EndCreate_time);
            }
            predicate = predicate.AndIF(parm.OrderNo.IsNotEmpty(), it => it.OrderNo == parm.OrderNo);
            predicate = predicate.AndIF(parm.UserId != null, it => it.UserId == parm.UserId);
            predicate = predicate.AndIF(parm.OrderStatus != null, it => it.OrderStatus == parm.OrderStatus);

            //predicate = predicate.AndIF(parm.ConfirmStatus != null, it => it.ConfirmStatus == parm.ConfirmStatus);
            predicate = predicate.AndIF(parm.DeliveryNo.IsNotEmpty(), it => it.DeliveryNo == parm.DeliveryNo);
            predicate = predicate.And(it => it.IsDelete == 0);
            //待发货双条件查询
            predicate = predicate.AndIF(parm.OrderStatus == Enum.OrderStatusEnum.TobeShipped, it => it.DeliveryStatus == DeliveryStatusEnum.NotDelivered);

            return predicate;
        }
    }
}
