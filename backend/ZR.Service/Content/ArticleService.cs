using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Mapster;
using ZR.Common;
using ZR.Model.Content;
using ZR.Model.Content.Dto;
using ZR.Model.Content.Enums;
using ZR.Model.System.Enums;
using ZR.Model.System;
using ZR.Repository;
using ZR.Service.Content.IService;
using ZR.ServiceCore.Services;

namespace ZR.Service.Content
{
    /// <summary>
    /// 
    /// </summary>
    [AppService(ServiceType = typeof(IArticleService), ServiceLifetime = LifeTime.Transient)]
    public class ArticleService : BaseService<Article>, IArticleService
    {
        private readonly IArticleCategoryService _categoryService;
        private readonly IArticleTopicService _topicService;
        private readonly ISysUserMsgService _userMsgService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryService"></param>
        /// <param name="topicService"></param>
        /// <param name="userMsgService"></param>
        public ArticleService(
            IArticleCategoryService categoryService,
            IArticleTopicService topicService,
            ISysUserMsgService userMsgService)
        {
            _categoryService = categoryService;
            _topicService = topicService;
            _userMsgService = userMsgService;
        }

        /// <summary>
        /// Query article management list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<ArticleDto> GetList(ArticleQueryDto parm)
        {
            var predicate = Expressionable.Create<Article>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Title), m => m.Title.Contains(parm.Title));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AbstractText), m => m.AbstractText.Contains(parm.AbstractText));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), m => m.Status == parm.Status);
            predicate = predicate.AndIF(parm.IsPublic != null, m => m.IsPublic == parm.IsPublic);
            predicate = predicate.AndIF(parm.IsTop != null, m => m.IsTop == parm.IsTop);
            predicate = predicate.AndIF(parm.ArticleType != null, m => (int)m.ArticleType == parm.ArticleType);
            predicate = predicate.AndIF(parm.TopicId != null, m => m.TopicId == parm.TopicId);
            predicate = predicate.AndIF(parm.AuditStatus != null, m => m.AuditStatus == parm.AuditStatus);

            if (parm.CategoryId != null)
            {
                var allChildCategory = Context.Queryable<ArticleCategory>()
                    .ToChildList(m => m.ParentId, parm.CategoryId);
                var categoryIdList = allChildCategory.Select(x => x.CategoryId).ToArray();
                predicate = predicate.And(m => categoryIdList.Contains(m.CategoryId));
            }

            var response = Queryable()
                .WithCache(60 * 24)
                .IgnoreColumns(x => new { x.Content })
                .Includes(x => x.CategoryNav) //Fill child objects
                .Where(predicate.ToExpression())
                //.OrderBy(x => x.Create_time, OrderByType.Desc)
                .ToPage<Article, ArticleDto>(parm);

            return response;
        }

        /// <summary>
        /// Frontend query article list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<ArticleDto> GetArticleList(ArticleQueryDto parm)
        {
            var predicate = Expressionable.Create<Article>();
            predicate = predicate.And(m => m.Status == "1");
            predicate = predicate.And(m => m.IsPublic == 1);
            predicate = predicate.And(m => m.AuditStatus == AuditStatusEnum.Passed);
            predicate = predicate.And(m => m.ArticleType == ArticleTypeEnum.Article);
            predicate = predicate.AndIF(parm.SearchText.IsNotEmpty(), m => m.Title.Contains(parm.SearchText) || m.AbstractText.Contains(parm.SearchText));
            predicate = predicate.AndIF(parm.TopicId != null, m => m.TopicId == parm.TopicId);

            if (parm.CategoryId != null)
            {
                var allChildCategory = Context.Queryable<ArticleCategory>()
                    .ToChildList(m => m.ParentId, parm.CategoryId);
                var categoryIdList = allChildCategory.Select(x => x.CategoryId).ToArray();
                predicate = predicate.And(m => categoryIdList.Contains(m.CategoryId));
            }

            var response = Queryable()
                .WithCache(60 * 30)
                .Includes(x => x.CategoryNav)
                .LeftJoin<SysUser>((m, u) => m.UserId == u.UserId).Filter(null, true)
                .Where(predicate.ToExpression())
                .OrderByDescending(m => m.Cid)
                .Select((m, u) => new ArticleDto()
                {
                    User = new ArticleUser()
                    {
                        Avatar = u.Avatar,
                        NickName = u.NickName,
                        Sex = u.Sex,
                    },
                    Content = string.Empty,
                    UserIP = string.Empty
                }, true)
                .ToPage(parm);

            if (parm.UserId > 0)
            {
                Context.ThenMapper(response.Result, item =>
                {
                    item.IsPraise = Context.Queryable<ArticlePraise>()
                    .Where(f => f.UserId == parm.UserId && f.IsDelete == 0)
                    .SetContext(scl => scl.ArticleId, () => item.Cid, item).Any() ? 1 : 0;
                });
            }

            return response;
        }

        /// <summary>
        /// Query latest article list
        /// </summary>
        /// <returns></returns>
        public List<ArticleDto> GetNewArticleList()
        {
            var predicate = Expressionable.Create<Article>();
            predicate = predicate.And(m => m.Status == "1");
            predicate = predicate.And(m => m.IsPublic == 1);
            predicate = predicate.And(m => m.ArticleType == ArticleTypeEnum.Article);
            predicate = predicate.And(m => m.AuditStatus == AuditStatusEnum.Passed);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Includes(x => x.CategoryNav) //Fill child objects
                .Take(10)
                .OrderBy(f => f.Create_time, OrderByType.Desc)
                .ToList();
            return response.Adapt<List<ArticleDto>>();
        }

        /// <summary>
        /// Frontend query moments list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<ArticleDto> GetMonentList(ArticleQueryDto parm)
        {
            var predicate = Expressionable.Create<Article>();
            predicate = predicate.And(m => m.Status == "1");
            predicate = predicate.And(m => m.IsPublic == 1);
            predicate = predicate.And(m => m.ArticleType == ArticleTypeEnum.Monent);
            predicate = predicate.And(m => m.AuditStatus == AuditStatusEnum.Passed);
            predicate = predicate.AndIF(parm.TopicId != null, m => m.TopicId == parm.TopicId);
            predicate = predicate.AndIF(parm.CategoryId != null, m => m.CategoryId == parm.CategoryId);

            //Get my circles
            if (parm.QueryMyJoin)
            {
                var myJoin = Context.Queryable<ArticleUserCircles>()
                    .Where(m => m.UserId == parm.UserId)
                    .Select(m => m.CategoryId)
                    .ToList();
                predicate = predicate.And(m => myJoin.Contains(m.CategoryId));
            }
            var response = Queryable()
                .Includes(x => x.CategoryNav) //Fill child objects
                .LeftJoin<SysUser>((m, u) => m.UserId == u.UserId).Filter(null, true)
                .Where(predicate.ToExpression())
                .OrderByIF(parm.OrderBy == 1, m => new { m.PraiseNum, m.CommentNum }, OrderByType.Desc)
                .OrderByIF(parm.OrderBy == 2, m => new { m.Cid }, OrderByType.Desc)
                .OrderBy(m => m.Cid, OrderByType.Desc)
                .Select((m, u) => new ArticleDto()
                {
                    User = new ArticleUser()
                    {
                        Avatar = u.Avatar,
                        NickName = u.NickName,
                        Sex = u.Sex,
                    },
                }, true)
                .ToPage(parm);

            if (parm.UserId > 0)
            {
                Context.ThenMapper(response.Result, item =>
                {
                    item.IsPraise = Context.Queryable<ArticlePraise>()
                    .Where(f => f.UserId == parm.UserId && f.IsDelete == 0)
                    .SetContext(scl => scl.ArticleId, () => item.Cid, item).Any() ? 1 : 0;
                });
            }

            return response;
        }

        /// <summary>
        /// Query my article list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public PagedInfo<ArticleDto> GetMyList(ArticleQueryDto parm)
        {
            var predicate = Expressionable.Create<Article>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Title), m => m.Title.Contains(parm.Title));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Status), m => m.Status == parm.Status);
            predicate = predicate.AndIF(parm.BeginTime != null, m => m.Create_time >= parm.BeginTime);
            predicate = predicate.AndIF(parm.EndTime != null, m => m.Create_time <= parm.EndTime);
            predicate = predicate.And(m => m.UserId == parm.UserId);
            predicate = predicate.AndIF(parm.ArticleType != null, m => (int)m.ArticleType == parm.ArticleType);
            predicate = predicate.AndIF(parm.TabId == 2, m => m.IsPublic == 0 && m.UserId == parm.UserId);

            var response = Queryable()
                //.IgnoreColumns(x => new { x.Content })
                .Includes(x => x.CategoryNav)
                .Where(predicate.ToExpression())
                .OrderByIF(parm.TabId == 3, m => new { m.IsTop, m.PraiseNum }, OrderByType.Desc)
                .OrderByDescending(m => new { m.IsTop, m.Cid })
                .Select((x) => new ArticleDto()
                {
                    Content = x.ArticleType == 0 ? string.Empty : x.Content,
                }, true)
                .ToPage(parm);

            return response;
        }

        /// <summary>
        /// Update article management
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateArticle(Article model)
        {
            var response = Update(w => w.Cid == model.Cid, it => new Article()
            {
                Title = model.Title,
                Content = model.Content,
                Status = model.Status,
                Tags = model.Tags,
                Update_time = DateTime.Now,
                CoverUrl = model.CoverUrl,
                CategoryId = model.CategoryId,
                IsPublic = model.IsPublic,
                AbstractText = model.AbstractText,
            });
            return response;
        }

        /// <summary>
        /// Pin article to top
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int TopArticle(Article model)
        {
            var response = Update(w => w.Cid == model.Cid, it => new Article()
            {
                IsTop = model.IsTop,
            });
            return response;
        }

        /// <summary>
        /// Comment permission
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int ChangeComment(Article model)
        {
            var response = Update(w => w.Cid == model.Cid, it => new Article()
            {
                CommentSwitch = model.CommentSwitch,
            });
            return response;
        }

        /// <summary>
        /// Whether public
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int ChangeArticlePublic(Article model)
        {
            var response = Update(w => w.Cid == model.Cid, it => new Article()
            {
                IsPublic = model.IsPublic,
            });
            return response;
        }

        /// <summary>
        /// Update article view count
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public int UpdateArticleHit(long cid)
        {
            var response = Update(w => w.Cid == cid, it => new Article() { Hits = it.Hits + 1 });
            return response;
        }

        /// <summary>
        /// Like/Praise article
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public int PraiseArticle(long cid)
        {
            return Update(w => w.Cid == cid, it => new Article() { PraiseNum = it.PraiseNum + 1 });
        }
        public int CancelPraise(long cid)
        {
            return Update(w => w.Cid == cid, it => new Article() { PraiseNum = it.PraiseNum - 1 });
        }

        public Article PublishArticle(Article article)
        {
            return Publish(article);
        }

        /// <summary>
        /// Frontend - Publish moments, articles
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        public Article Publish(Article article)
        {
            var httpContext = App.HttpContext;
            article.AuthorName = HttpContextExtension.GetName(httpContext);
            article.UserId = HttpContextExtension.GetUId(httpContext);
            article.UserIP = HttpContextExtension.GetClientUserIp(httpContext);
            article.Location = HttpContextExtension.GetIpInfo(article.UserIP);
            article.AuditStatus = AuditStatusEnum.Passed;

            article = InsertReturnEntity(article);
            if (article.Status == "1")
            {
                //Update topic join count
                if (article.Cid > 0 && article.TopicId > 0)
                {
                    _topicService.Update(w => w.TopicId == article.TopicId, it => new ArticleTopic() { JoinNum = it.JoinNum + 1 });
                }
                //Update published article count
                if (article.Cid > 0 && article.CategoryId > 0)
                {
                    _categoryService.Update(w => w.CategoryId == article.CategoryId, it => new ArticleCategory() { ArticleNum = it.ArticleNum + 1 });
                }
            }
            return article;
        }

        /// <summary>
        /// Get article details
        /// </summary>
        /// <param name="cid">Content id</param>
        /// <param name="userId">Current user id</param>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        public ArticleDto GetArticle(long cid, long userId)
        {
            var response = GetId(cid);
            var model = response.Adapt<ArticleDto>() ?? throw new CustomException(ResultCode.FAIL, "Content does not exist");
            if (model.IsPublic == 0 && userId != model.UserId)
            {
                throw new CustomException(ResultCode.CUSTOM_ERROR, "Access failed");
            }
            if (model != null)
            {
                model.CategoryNav = _categoryService.GetById(model.CategoryId).Adapt<ArticleCategoryDto>();
            }

            var webContext = App.HttpContext;
            var CK = "ARTICLE_DETAILS_" + userId + HttpContextExtension.GetClientUserIp(webContext);
            if (!CacheHelper.Exists(CK))
            {
                UpdateArticleHit(cid);
                var userIP = HttpContextExtension.GetClientUserIp(webContext);
                var location = HttpContextExtension.GetIpInfo(userIP);
                itenant.InsertableWithAttr(new ArticleBrowsingLog()
                {
                    ArticleId = cid,
                    UserId = userId,
                    Location = location,
                    UserIP = userIP,
                    AddTime = DateTime.Now,
                }).ExecuteReturnSnowflakeId();
            }
            CacheHelper.SetCache(CK, 1, 10);
            if (userId > 0)
            {
                model.IsPraise = Context.Queryable<ArticlePraise>()
               .Where(f => f.UserId == userId && f.ArticleId == cid && f.IsDelete == 0)
               .Any() ? 1 : 0;
            }

            return model;
        }

        /// <summary>
        /// Audit passed
        /// </summary>
        /// <param name="idsArr"></param>
        /// <returns></returns>
        public int Passed(long[] idsArr)
        {
            int result = 0;
            List<Article> monents = [];
            foreach (var item in idsArr)
            {
                var model = GetFirst(x => x.Cid == item && x.AuditStatus == 0);
                if (model == null) continue;

                //model.Auditor = HttpContext.GetName();
                model.AuditStatus = AuditStatusEnum.Passed;
                var response = Update(w => w.Cid == model.Cid, it => new Article()
                {
                    AuditStatus = model.AuditStatus,
                    //Auditor = model.Auditor,
                });
                result += response;
                monents.Add(model);
            }
            var data = from a in monents
                       group a by new { a.UserId } into grp
                       select new
                       {
                           userid = grp.Key.UserId,
                           num = grp.Count()
                       };
            foreach (var item in data)
            {
                _userMsgService.AddSysUserMsg(item.userid, "Your published content has passed the review", UserMsgType.SYSTEM);
            }

            return result;
        }

        /// <summary>
        /// Audit not passed
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="idsArr"></param>
        /// <returns></returns>
        public int Reject(string reason, long[] idsArr)
        {
            int result = 0;
            List<Article> monents = new();
            foreach (var item in idsArr)
            {
                var model = GetFirst(x => x.Cid == item && x.AuditStatus == 0);
                if (model == null) continue;

                //model.Auditor = HttpContext.GetName();
                model.AuditStatus = AuditStatusEnum.Reject;
                var response = Update(w => w.Cid == model.Cid, it => new Article()
                {
                    AuditStatus = model.AuditStatus,
                    //Auditor = model.Auditor,
                });
                result += response;
                monents.Add(model);
            }
            var data = from a in monents
                       group a by new { a.UserId } into grp
                       select new
                       {
                           userid = grp.Key.UserId,
                           num = grp.Count()
                       };
            foreach (var item in data)
            {
                //Console.WriteLine(item.useridx +"," + item.num);
                string content = $"Your published content has not passed the review.";
                if (!string.IsNullOrEmpty(reason))
                {
                    content += $"Reason: {reason}";
                }
                _userMsgService.AddSysUserMsg(item.userid, content, UserMsgType.SYSTEM);
            }

            return result;
        }

    }
}

