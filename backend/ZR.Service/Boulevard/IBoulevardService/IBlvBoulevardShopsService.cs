using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;

namespace ZR.Service.Boulevard.IBoulevardService
{
    /// <summary>
    /// shopsservice接口
    /// </summary>
    public interface IBlvBoulevardShopsService : IBaseService<BlvBoulevardShops>
    {
        PagedInfo<BlvBoulevardShopsDto> GetList(BlvBoulevardShopsQueryDto parm);

        BlvBoulevardShops GetInfo(long Id);


        BlvBoulevardShops AddBlvBoulevardShops(BlvBoulevardShops parm);
        int UpdateBlvBoulevardShops(BlvBoulevardShops parm);
        
        bool TruncateBlvBoulevardShops();

        (string, object, object) ImportBlvBoulevardShops(List<BlvBoulevardShops> list);

        PagedInfo<BlvBoulevardShopsDto> ExportList(BlvBoulevardShopsQueryDto parm);
    }
}

