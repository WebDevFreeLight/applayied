using ZR.Model.System.Public.Dto;
using ZR.Model.System.Public;

namespace ZR.Service.Public.IPublicService
{
    /// <summary>
    /// Banner management service interface
    /// </summary>
    public interface IBannerConfigService : IBaseService<BannerConfig>
    {
        PagedInfo<BannerConfigDto> GetList(BannerConfigQueryDto parm);

        BannerConfig GetInfo(int Id);

        BannerConfig AddBannerConfig(BannerConfig parm);
        int UpdateBannerConfig(BannerConfig parm);

        List<BannerConfigDto> GetBannerList(BannerConfigQueryDto parm);
        PagedInfo<BannerConfigDto> ExportList(BannerConfigQueryDto parm);
    }
}

