using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Model.System;
using ZR.Repository;
using ZR.ServiceCore;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>
    /// Project membership: who works on a project. Drives the Contributor's scoped access and the
    /// project workspace "Members" tab. Enriched with the user's display name via a single IN query.
    /// </summary>
    [AppService(ServiceType = typeof(IPmoProjectMemberService), ServiceLifetime = LifeTime.Transient)]
    public class PmoProjectMemberService : BaseService<PmoProjectMember>, IPmoProjectMemberService
    {
        public PagedInfo<PmoProjectMember> GetList(PmoMemberQueryDto parm, PmoActor actor)
        {
            var exp = Expressionable.Create<PmoProjectMember>();
            exp.AndIF(parm.ProjectId != null, m => m.ProjectId == parm.ProjectId);

            var page = Queryable().Where(exp.ToExpression())
                .OrderBy(m => m.MemberId, OrderByType.Desc).ToPage(parm);
            Enrich(page.Result);
            return page;
        }

        public PmoProjectMember AddMember(PmoMemberInputDto dto, PmoActor actor)
        {
            if (dto.ProjectId <= 0 || dto.UserId <= 0)
                throw new CustomException(ResultCode.FAIL, "Project and user are required");
            if (Queryable().Any(m => m.ProjectId == dto.ProjectId && m.UserId == dto.UserId))
                throw new CustomException(ResultCode.FAIL, "User is already a member of this project");

            var entity = new PmoProjectMember
            {
                ProjectId = dto.ProjectId,
                UserId = dto.UserId,
                RoleInProject = string.IsNullOrWhiteSpace(dto.RoleInProject) ? "Member" : dto.RoleInProject.Trim(),
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            entity.MemberId = Insertable(entity).ExecuteReturnSnowflakeId();
            Enrich(new List<PmoProjectMember> { entity });
            return entity;
        }

        public int RemoveMember(long memberId, PmoActor actor)
        {
            var existing = Queryable().First(m => m.MemberId == memberId)
                ?? throw new CustomException(ResultCode.FAIL, "Member not found");
            return Context.Deleteable<PmoProjectMember>().In(existing.MemberId).ExecuteCommand();
        }

        private void Enrich(List<PmoProjectMember> members)
        {
            if (members == null || members.Count == 0) return;
            var userIds = members.Select(m => m.UserId).Distinct().ToList();
            var users = Context.Queryable<SysUser>().Where(u => userIds.Contains(u.UserId))
                .Select(u => new { u.UserId, u.NickName, u.UserName }).ToList()
                .ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);
            foreach (var m in members)
                m.UserName = users.TryGetValue(m.UserId, out var n) ? n : null;
        }
    }
}
