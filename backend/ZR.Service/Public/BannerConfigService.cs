using Infrastructure.Attribute;
using Mapster;
using ZR.Model.System.Public;
using ZR.Model.System.Public.Dto;
using ZR.Repository;
using ZR.Service.Public.IPublicService;

namespace ZR.Service.Public
{
    /// <summary>
    /// Banner management service layer processing
    /// </summary>
    [AppService(ServiceType = typeof(IBannerConfigService), ServiceLifetime = LifeTime.Transient)]
    public class BannerConfigService : BaseService<BannerConfig>, IBannerConfigService
    {
        /// <summary>
        /// Query banner management list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BannerConfigDto> GetList(BannerConfigQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .ToPage<BannerConfig, BannerConfigDto>(parm);

            return response;
        }

        /// <summary>
        /// Get details
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public BannerConfig GetInfo(int Id)
        {
            var response = Queryable()
                .Where(x => x.Id == Id)
                .First();

            return response;
        }

        /// <summary>
        /// Add banner management
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public BannerConfig AddBannerConfig(BannerConfig model)
        {
            return Insertable(model).ExecuteReturnEntity();
        }

        /// <summary>
        /// Update banner management
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateBannerConfig(BannerConfig model)
        {
            return Update(model, false, "Update banner management");
        }

        /// <summary>
        /// Export banner management
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<BannerConfigDto> ExportList(BannerConfigQueryDto parm)
        {
            var predicate = QueryExp(parm);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new BannerConfigDto()
                {
                    ShowStatusLabel = it.ShowStatus.GetConfigValue<Model.System.SysDictData>("sys_common_status"),
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// Query export expression
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static Expressionable<BannerConfig> QueryExp(BannerConfigQueryDto parm)
        {
            var predicate = Expressionable.Create<BannerConfig>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Title), it => it.Title.Contains(parm.Title));
            predicate = predicate.AndIF(parm.JumpType != null, it => it.JumpType == parm.JumpType);
            predicate = predicate.AndIF(parm.ShowStatus != null, it => it.ShowStatus == parm.ShowStatus);
            predicate = predicate.AndIF(parm.AdType != null, it => it.AdType == parm.AdType);
            return predicate;
        }

        /// <summary>
        /// Query banner management list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public List<BannerConfigDto> GetBannerList(BannerConfigQueryDto parm)
        {
            var predicate = Expressionable.Create<BannerConfig>();
            var now = DateTime.Now;
            predicate = predicate.And(it => it.ShowStatus == 0);
            predicate = predicate.AndIF(parm.AdType != null, it => it.AdType == parm.AdType);
            predicate = predicate.And(it => it.BeginTime <= now && it.EndTime >= now);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .ToList();

            return response.Adapt<List<BannerConfigDto>>();
        }
    }
}
