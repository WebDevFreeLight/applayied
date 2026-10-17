using ZR.Model.Boulevard.Dto;
using ZR.Model.Boulevard;

namespace ZR.Service.Boulevard.IBoulevardService
{
    /// <summary>
    /// boulevard_seasonsservice接口
    /// </summary>
    public interface IBlvBoulevardSeasonsService : IBaseService<BlvBoulevardSeasons>
    {
        PagedInfo<BlvBoulevardSeasonsDto> GetList(BlvBoulevardSeasonsQueryDto parm);

        BlvBoulevardSeasons GetInfo(long Id);


        BlvBoulevardSeasons AddBlvBoulevardSeasons(BlvBoulevardSeasons parm);
        int UpdateBlvBoulevardSeasons(BlvBoulevardSeasons parm);
        
        bool TruncateBlvBoulevardSeasons();

        (string, object, object) ImportBlvBoulevardSeasons(List<BlvBoulevardSeasons> list);

        PagedInfo<BlvBoulevardSeasonsDto> ExportList(BlvBoulevardSeasonsQueryDto parm);
    }
}

