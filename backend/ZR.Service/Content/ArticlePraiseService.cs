using Infrastructure.Attribute;
using ZR.Model.Content;
using ZR.Model.System.Enums;
using ZR.Service.Content.IService;
using ZR.ServiceCore.Services;

namespace ZR.Service.Content
{
    [AppService(ServiceType = typeof(IArticlePraiseService), ServiceLifetime = LifeTime.Transient)]
    public class ArticlePraiseService : BaseService<ArticlePraise>, IArticlePraiseService
    {
        private IArticleService _articleService;
        private ISysUserMsgService _sysUserMsgService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleService"></param>
        /// <param name="userMsgService"></param>
        public ArticlePraiseService(IArticleService articleService, ISysUserMsgService userMsgService)
        {
            _articleService = articleService;
            _sysUserMsgService = userMsgService;
        }

        /// <summary>
        /// ??
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public int Praise(ArticlePraise dto)
        {
            int result = 0;
            var praiseInfo = GetFirst(f => f.UserId == dto.UserId && f.ArticleId == dto.ArticleId);
            var isPraise = 0;
            var r = UseTran(() =>
            {
                //?????????
                if (praiseInfo == null)
                {
                    Insertable(dto).ExecuteReturnSnowflakeId();
                    result = _articleService.PraiseArticle(dto.ArticleId);
                    isPraise = 1;

                    if (dto.UserId != dto.ToUserId)
                    {
                        _sysUserMsgService.AddSysUserMsg(new SysUserMsg()
                        {
                            FromUserid = dto.UserId,
                            UserId = dto.ToUserId,
                            Content = "??????",
                            MsgType = UserMsgType.PRAISE,
                            TargetId = dto.ArticleId,
                        });
                    }
                }
                else
                {
                    if (praiseInfo.IsDelete == 0)
                    {
                        result = CanclePraise(dto.UserId, dto.ArticleId);
                        //????-1
                        _articleService.CancelPraise(dto.ArticleId);
                    }
                    else
                    {
                        //????+1
                        _articleService.PraiseArticle(dto.ArticleId);
                        //??????????
                        PlusPraise(praiseInfo.PId);
                        isPraise = 1;
                    }
                }
            });
            return r.IsSuccess && isPraise == 1 ? 1 : 0;
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public int CanclePraise(long userid, long articleId)
        {
            return Deleteable()
                .Where(f => f.ArticleId == articleId && f.UserId == userid)
                .IsLogic()
                .ExecuteCommand(deleteValue: 1);
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public int PlusPraise(long pid)
        {
            return Deleteable()
                .Where(f => f.PId == pid)
                .IsLogic()
                .ExecuteCommand(deleteValue: 0);
        }
    }
}

