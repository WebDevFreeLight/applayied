using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;

namespace ZR.Service.Boulevard.IBoulevardService
{
    /// <summary>
    /// boulevard_seasons_Shopsservice接口
    /// </summary>
    public interface IBlvBoulevardSeasonsShopsService : IBaseService<BlvBoulevardSeasonsShops>
    {
        PagedInfo<BlvBoulevardSeasonsShopsDto> GetList(BlvBoulevardSeasonsShopsQueryDto parm);


        public List<BlvBoulevardSeasonsShopsDto> GetListAll(BlvBoulevardSeasonsShopsQueryDto parm);


        BlvBoulevardSeasonsShops GetInfo(long Id);


        BlvBoulevardSeasonsShops AddBlvBoulevardSeasonsShops(BlvBoulevardSeasonsShops parm);
        int UpdateBlvBoulevardSeasonsShops(BlvBoulevardSeasonsShops parm);
        
        bool TruncateBlvBoulevardSeasonsShops();

        (string, object, object) ImportBlvBoulevardSeasonsShops(List<BlvBoulevardSeasonsShops> list);

        PagedInfo<BlvBoulevardSeasonsShopsDto> ExportList(BlvBoulevardSeasonsShopsQueryDto parm);
    }
}

