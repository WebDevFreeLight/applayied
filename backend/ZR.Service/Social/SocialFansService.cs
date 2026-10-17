using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using ZR.Model.Content.Dto;
using ZR.Model.social;
using ZR.Model.System;
using ZR.Repository;
using ZR.Service.Social.IService;

namespace ZR.Service.Social
{
    /// <summary>
    /// Fans
    /// </summary>
    [AppService(ServiceType = typeof(ISocialFansService))]
    public class SocialFansService : BaseService<SocialFans>, ISocialFansService, IDynamicApi
    {
        /// <summary>
        /// Query follow/fans list
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiResult FollowList([FromQuery] FansQueryDto dto)
        {
            var userid = (int)HttpContextExtension.GetUId(App.HttpContext);
            PagedInfo<SocialFansDto> list = dto.SelectType == 1 ? GetFollow(dto, userid) : GetFans(dto, userid);

            return ApiResult.Success(list);
        }

        /// <summary>
        /// Whether following
        /// </summary>
        /// <param name="toUserid"></param>
        /// <returns></returns>
        public ApiResult IsFollow([FromQuery] int toUserid)
        {
            var userid = (int)HttpContextExtension.GetUId(App.HttpContext);
            if (userid == toUserid || toUserid <= 0)
            {
                return ApiResult.Success("");
            }
            var isFollow = Any(f => f.Userid == userid && f.ToUserid == toUserid);

            return ApiResult.Success(isFollow);
        }

        /// <summary>
        /// Follow
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Follow([FromBody] SocialFansDto dto)
        {
            dto.Userid = (int)HttpContextExtension.GetUId(App.HttpContext);
            if (dto.Userid == dto.ToUserid)
            {
                throw new CustomException("Cannot follow yourself");
            };
            if (dto.ToUserid <= 0)
            {
                return ApiResult.Error("Follow failed");
            }
            //TODO Check if user exists

            var isFollow = GetFirst(x => x.Userid == dto.Userid && x.ToUserid == dto.ToUserid);
            if (isFollow != null)
            {
                throw new CustomException("Already followed");
            }
            var fans = new SocialFans
            {
                Userid = dto.Userid,
                ToUserid = dto.ToUserid,
                FollowTime = DateTime.Now,
                UserIP = HttpContextExtension.GetClientUserIp(App.HttpContext)
            };
            var result = UseTran2(() =>
            {
                //Add fan
                InsertReturnSnowflakeId(fans);

                UpdateFollowInfo(dto.Userid, isFollowNum: true);
                UpdateFollowInfo(dto.ToUserid, isFollowNum: false);
            });

            if (!result)
            {
                return ApiResult.Error("Follow failed");
            }
            var data = Context.Queryable<SocialFansInfo>()
                    .Where(x => x.Userid == dto.Userid)
                    .First();
            return ApiResult.Success("Follow successful", data);
        }

        /// <summary>
        /// Cancel follow
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult CancelFollow([FromBody] SocialFansDto dto)
        {
            dto.Userid = (int)HttpContextExtension.GetUId(App.HttpContext);
            var result = UseTran(() =>
            {
                //Delete fan
                Delete(f => f.Userid == dto.Userid && f.ToUserid == dto.ToUserid);

                Context.Updateable<SocialFansInfo>()
                .SetColumns(x => x.FollowNum == x.FollowNum - 1)
                .Where(x => x.Userid == dto.Userid)
                .ExecuteCommand();

                Context.Updateable<SocialFansInfo>()
                .SetColumns(x => x.FansNum == x.FansNum - 1)
                .Where(x => x.Userid == dto.ToUserid)
                .ExecuteCommand();
            });

            if (!result.IsSuccess)
            {
                return ApiResult.Error("Cancel follow failed");
            }
            var data = Context.Queryable<SocialFansInfo>()
                    .Where(x => x.Userid == dto.Userid)
                    .First();
            return ApiResult.Success("Cancel follow successful", data);
        }

        private void UpdateFollowInfo(long userId, bool isFollowNum)
        {
            var count = Context.Updateable<SocialFansInfo>()
                .SetColumnsIF(isFollowNum, x => x.FollowNum == x.FollowNum + 1)
                .SetColumnsIF(!isFollowNum, x => x.FansNum == x.FansNum + 1)
                .Where(x => x.Userid == userId)
                .ExecuteCommand();

            if (count > 0) return;

            Context.Insertable(new SocialFansInfo
            {
                Userid = userId,
                FollowNum = isFollowNum ? 1 : 0,
                FansNum = isFollowNum ? 0 : 1,
                Update_time = DateTime.Now
            }).ExecuteCommand();
        }

        private PagedInfo<SocialFansDto> GetFollow(FansQueryDto dto, int userid)
        {
            return Queryable()
                            .LeftJoin<SysUser>((it, u) => it.ToUserid == u.UserId)
                            .Where(it => it.Userid == userid)
                            .Select((it, u) => new SocialFansDto()
                            {
                                User = new ArticleUser()
                                {
                                    Avatar = u.Avatar,
                                    NickName = u.NickName,
                                    Sex = u.Sex,
                                },
                                Status = 1,
                                ToUserid = it.ToUserid,
                            })
                            .ToPage(dto);
        }
        private PagedInfo<SocialFansDto> GetFans(FansQueryDto dto, int userid)
        {
            return Queryable()
                            .LeftJoin<SysUser>((it, u) => it.Userid == u.UserId)
                            .Where(it => it.ToUserid == userid)
                            .Select((it, u) => new SocialFansDto()
                            {
                                User = new ArticleUser()
                                {
                                    Avatar = u.Avatar,
                                    NickName = u.NickName,
                                    Sex = u.Sex,
                                },
                                Userid = it.Userid,
                            })
                            .ToPage(dto);
        }
    }
}

