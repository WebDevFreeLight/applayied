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
    /// Risk &amp; Issue register business logic: scope-aware listing, validation, and server-side
    /// exposure scoring (score = probability × impact; never trusted from the client).
    /// </summary>
    [AppService(ServiceType = typeof(IPmoRiskService), ServiceLifetime = LifeTime.Transient)]
    public class PmoRiskService : BaseService<PmoRisk>, IPmoRiskService
    {
        private const int HighExposureThreshold = 12;

        public PagedInfo<PmoRisk> GetList(PmoRiskQueryDto parm, PmoActor actor)
        {
            var exp = Expressionable.Create<PmoRisk>();
            exp.AndIF(parm.ProjectId != null, r => r.ProjectId == parm.ProjectId);
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), r => r.Status == parm.Status);
            exp.AndIF(!string.IsNullOrEmpty(parm.Kind), r => r.Kind == parm.Kind);
            exp.AndIF(parm.MinScore != null, r => r.Score >= parm.MinScore);
            if (!string.IsNullOrWhiteSpace(parm.Keyword))
                exp.And(r => r.Title.Contains(parm.Keyword));

            // High-exposure open = Score >= 12 AND Status == "Open".
            if (parm.HighExposureOpenOnly == true)
            {
                exp.And(r => r.Score >= HighExposureThreshold && r.Status == "Open");
            }

            // Read scope: admins & PMs see everything; contributors only risks belonging to a
            // project they own or are a member of. This is server-side, not just a hidden menu.
            if (!actor.IsProjectManager)
            {
                long uid = actor.UserId;
                exp.And(r => SqlFunc.Subqueryable<PmoProject>()
                    .Where(p => p.ProjectId == r.ProjectId
                        && (p.OwnerUserId == uid
                            || SqlFunc.Subqueryable<PmoProjectMember>()
                                .Where(m => m.ProjectId == p.ProjectId && m.UserId == uid).Any()))
                    .Any());
            }

            var page = Queryable()
                .Where(exp.ToExpression())
                .OrderBy(r => r.Score, OrderByType.Desc)
                .ToPage(parm);

            Enrich(page.Result);
            return page;
        }

        public PmoRisk GetInfo(long riskId, PmoActor actor)
        {
            var risk = Queryable().First(r => r.RiskId == riskId)
                ?? throw new CustomException(ResultCode.FAIL, "Risk not found");

            if (!actor.IsProjectManager)
            {
                long uid = actor.UserId;
                bool inScope = Context.Queryable<PmoProject>().Any(p => p.ProjectId == risk.ProjectId
                    && (p.OwnerUserId == uid
                        || SqlFunc.Subqueryable<PmoProjectMember>()
                            .Where(m => m.ProjectId == p.ProjectId && m.UserId == uid).Any()));
                if (!inScope)
                    throw new CustomException(ResultCode.FORBIDDEN, "You do not have access to this risk");
            }

            Enrich(new List<PmoRisk> { risk });
            return risk;
        }

        public PmoRisk AddRisk(PmoRiskInputDto dto, PmoActor actor)
        {
            Validate(dto);
            if (!Context.Queryable<PmoProject>().Any(p => p.ProjectId == dto.ProjectId))
                throw new CustomException(ResultCode.FAIL, "Project not found");

            var kind = string.IsNullOrEmpty(dto.Kind) ? "Risk" : dto.Kind;
            var status = string.IsNullOrEmpty(dto.Status) ? "Open" : dto.Status;

            var entity = new PmoRisk
            {
                ProjectId = dto.ProjectId,
                Kind = kind,
                Title = dto.Title.Trim(),
                OwnerUserId = dto.OwnerUserId,
                Probability = dto.Probability,
                Impact = dto.Impact,
                Score = dto.Probability * dto.Impact,
                Mitigation = dto.Mitigation,
                DueDate = dto.DueDate,
                Status = status,
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            entity.RiskId = Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        public int UpdateRisk(PmoRiskInputDto dto, PmoActor actor)
        {
            var existing = Queryable().First(r => r.RiskId == dto.RiskId)
                ?? throw new CustomException(ResultCode.FAIL, "Risk not found");

            Validate(dto);
            if (!Context.Queryable<PmoProject>().Any(p => p.ProjectId == dto.ProjectId))
                throw new CustomException(ResultCode.FAIL, "Project not found");

            existing.ProjectId = dto.ProjectId;
            existing.Kind = string.IsNullOrEmpty(dto.Kind) ? "Risk" : dto.Kind;
            existing.Title = dto.Title.Trim();
            existing.OwnerUserId = dto.OwnerUserId;
            existing.Probability = dto.Probability;
            existing.Impact = dto.Impact;
            existing.Score = dto.Probability * dto.Impact;
            existing.Mitigation = dto.Mitigation;
            existing.DueDate = dto.DueDate;
            existing.Status = string.IsNullOrEmpty(dto.Status) ? "Open" : dto.Status;
            existing.Update_by = actor.UserName;
            existing.Update_time = DateTime.Now;

            return Update(existing, true, "Updated risk");
        }

        private static void Validate(PmoRiskInputDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new CustomException(ResultCode.FAIL, "Title is required");
            if (dto.Probability < 1 || dto.Probability > 5)
                throw new CustomException(ResultCode.FAIL, "Probability must be between 1 and 5");
            if (dto.Impact < 1 || dto.Impact > 5)
                throw new CustomException(ResultCode.FAIL, "Impact must be between 1 and 5");

            var kind = string.IsNullOrEmpty(dto.Kind) ? "Risk" : dto.Kind;
            if (kind != "Risk" && kind != "Issue")
                throw new CustomException(ResultCode.FAIL, "Kind must be 'Risk' or 'Issue'");

            var status = string.IsNullOrEmpty(dto.Status) ? "Open" : dto.Status;
            if (status != "Open" && status != "Mitigating" && status != "Closed")
                throw new CustomException(ResultCode.FAIL, "Status must be 'Open', 'Mitigating' or 'Closed'");
        }

        /// <summary>
        /// Enrich a page of risks with owner and project names. Names are loaded with separate IN
        /// queries resolved into dictionaries (no one-to-many JOIN fan-out).
        /// </summary>
        private void Enrich(List<PmoRisk> risks)
        {
            if (risks == null || risks.Count == 0) return;
            var ownerIds = risks.Where(r => r.OwnerUserId != null)
                .Select(r => r.OwnerUserId.Value).Distinct().ToList();
            var projectIds = risks.Select(r => r.ProjectId).Distinct().ToList();

            var owners = Context.Queryable<SysUser>().Where(u => ownerIds.Contains(u.UserId))
                .Select(u => new { u.UserId, u.NickName, u.UserName }).ToList()
                .ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);

            var projects = Context.Queryable<PmoProject>().Where(p => projectIds.Contains(p.ProjectId))
                .Select(p => new { p.ProjectId, p.Name }).ToList()
                .ToDictionary(p => p.ProjectId, p => p.Name);

            foreach (var r in risks)
            {
                if (r.OwnerUserId != null && owners.TryGetValue(r.OwnerUserId.Value, out var on))
                    r.OwnerName = on;
                r.ProjectName = projects.TryGetValue(r.ProjectId, out var pn) ? pn : null;
            }
        }
    }
}
