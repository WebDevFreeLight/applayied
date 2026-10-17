using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;

namespace ZR.Service.Boulevard.IBoulevardService
{
    /// <summary>
    /// boulevard_season_zonesservice接口
    /// </summary>
    public interface IBlvBoulevardSeasonZonesService : IBaseService<BlvBoulevardSeasonZones>
    {
        PagedInfo<BlvBoulevardSeasonZonesDto> GetList(BlvBoulevardSeasonZonesQueryDto parm);
        public List<BlvBoulevardSeasonZonesDto> GetListAll(BlvBoulevardSeasonZonesQueryDto parm);
        BlvBoulevardSeasonZones GetInfo(long Id);


        BlvBoulevardSeasonZones AddBlvBoulevardSeasonZones(BlvBoulevardSeasonZones parm);
        int UpdateBlvBoulevardSeasonZones(BlvBoulevardSeasonZones parm);
        
        bool TruncateBlvBoulevardSeasonZones();

        (string, object, object) ImportBlvBoulevardSeasonZones(List<BlvBoulevardSeasonZones> list);

        PagedInfo<BlvBoulevardSeasonZonesDto> ExportList(BlvBoulevardSeasonZonesQueryDto parm);
    }
}

