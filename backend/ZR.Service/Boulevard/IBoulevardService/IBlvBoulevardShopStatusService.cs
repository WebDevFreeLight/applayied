using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;

namespace ZR.Service.Boulevard.IBoulevardService
{
    /// <summary>
    /// shop_statusservice接口
    /// </summary>
    public interface IBlvBoulevardShopStatusService : IBaseService<BlvBoulevardShopStatus>
    {
        PagedInfo<BlvBoulevardShopStatusDto> GetList(BlvBoulevardShopStatusQueryDto parm);

        BlvBoulevardShopStatus GetInfo(long Id);


        BlvBoulevardShopStatus AddBlvBoulevardShopStatus(BlvBoulevardShopStatus parm);
        int UpdateBlvBoulevardShopStatus(BlvBoulevardShopStatus parm);
        
        bool TruncateBlvBoulevardShopStatus();

        (string, object, object) ImportBlvBoulevardShopStatus(List<BlvBoulevardShopStatus> list);

        PagedInfo<BlvBoulevardShopStatusDto> ExportList(BlvBoulevardShopStatusQueryDto parm);
    }
}

