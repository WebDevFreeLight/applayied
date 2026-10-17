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
    /// Project portfolio business logic: scope-aware listing, validation, and derived delivery status.
    /// </summary>
    [AppService(ServiceType = typeof(IPmoProjectService), ServiceLifetime = LifeTime.Transient)]
    public class PmoProjectService : BaseService<PmoProject>, IPmoProjectService
    {
        public PagedInfo<PmoProject> GetList(PmoProjectQueryDto parm, PmoActor actor)
        {
            var exp = Expressionable.Create<PmoProject>();
            if (!string.IsNullOrWhiteSpace(parm.Keyword))
                exp.And(p => p.Code.Contains(parm.Keyword) || p.Name.Contains(parm.Keyword));
            exp.AndIF(parm.DeptId != null, p => p.DeptId == parm.DeptId);
            exp.AndIF(parm.OwnerUserId != null, p => p.OwnerUserId == parm.OwnerUserId);
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), p => p.Status == parm.Status);
            exp.AndIF(!string.IsNullOrEmpty(parm.Priority), p => p.Priority == parm.Priority);
            exp.AndIF(parm.BeginStartDate != null, p => p.StartDate >= parm.BeginStartDate);
            exp.AndIF(parm.EndStartDate != null, p => p.StartDate <= parm.EndStartDate);

            // Read scope: admins & PMs see the whole portfolio; contributors only projects they
            // own or are members of. This is server-side — it is not just a hidden menu.
            if (!actor.IsProjectManager)
            {
                long uid = actor.UserId;
                exp.And(p => p.OwnerUserId == uid
                    || SqlFunc.Subqueryable<PmoProjectMember>().Where(m => m.ProjectId == p.ProjectId && m.UserId == uid).Any());
            }

            var page = Queryable()
                .Where(exp.ToExpression())
                .OrderBy(p => p.ProjectId, OrderByType.Desc)
                .ToPage(parm);

            Enrich(page.Result);
            return page;
        }

        public PmoProject GetInfo(long projectId, PmoActor actor)
        {
            var project = Queryable().First(p => p.ProjectId == projectId)
                ?? throw new CustomException(ResultCode.FAIL, "Project not found");

            if (!actor.IsProjectManager && project.OwnerUserId != actor.UserId
                && !Context.Queryable<PmoProjectMember>().Any(m => m.ProjectId == projectId && m.UserId == actor.UserId))
            {
                throw new CustomException(ResultCode.FORBIDDEN, "You do not have access to this project");
            }

            Enrich(new List<PmoProject> { project });
            return project;
        }

        public PmoProject AddProject(PmoProjectInputDto dto, PmoActor actor)
        {
            ValidateCommon(dto);
            if (Queryable().Any(p => p.Code == dto.Code))
                throw new CustomException(ResultCode.FAIL, $"Project code '{dto.Code}' already exists");

            var entity = new PmoProject
            {
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                DeptId = dto.DeptId,
                OwnerUserId = dto.OwnerUserId,
                Status = string.IsNullOrEmpty(dto.Status) ? "Planning" : dto.Status,
                Priority = string.IsNullOrEmpty(dto.Priority) ? "Medium" : dto.Priority,
                StartDate = dto.StartDate,
                PlannedEndDate = dto.PlannedEndDate,
                ActualEndDate = dto.ActualEndDate,
                Description = dto.Description,
                InitialApprovedBudget = dto.InitialApprovedBudget,
                Currency = string.IsNullOrEmpty(dto.Currency) ? "AED" : dto.Currency,
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            entity.ProjectId = Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        public int UpdateProject(PmoProjectInputDto dto, PmoActor actor)
        {
            var existing = Queryable().First(p => p.ProjectId == dto.ProjectId)
                ?? throw new CustomException(ResultCode.FAIL, "Project not found");

            if (!actor.IsAdmin && existing.OwnerUserId != actor.UserId)
                throw new CustomException(ResultCode.FORBIDDEN, "Only the project owner or an administrator may edit this project");

            ValidateCommon(dto);
            if (Queryable().Any(p => p.Code == dto.Code && p.ProjectId != dto.ProjectId))
                throw new CustomException(ResultCode.FAIL, $"Project code '{dto.Code}' already exists");

            existing.Code = dto.Code.Trim();
            existing.Name = dto.Name.Trim();
            existing.DeptId = dto.DeptId;
            existing.OwnerUserId = dto.OwnerUserId;
            existing.Status = dto.Status;
            existing.Priority = dto.Priority;
            existing.StartDate = dto.StartDate;
            existing.PlannedEndDate = dto.PlannedEndDate;
            existing.ActualEndDate = dto.ActualEndDate;
            existing.Description = dto.Description;
            existing.InitialApprovedBudget = dto.InitialApprovedBudget;
            existing.Currency = dto.Currency;
            existing.Update_by = actor.UserName;
            existing.Update_time = DateTime.Now;

            return Update(existing, true, "Updated project");
        }

        private static void ValidateCommon(PmoProjectInputDto dto)
        {
            if (dto.InitialApprovedBudget < 0)
                throw new CustomException(ResultCode.FAIL, "Initial approved budget cannot be negative");
            if (dto.StartDate != null && dto.PlannedEndDate != null && dto.PlannedEndDate < dto.StartDate)
                throw new CustomException(ResultCode.FAIL, "Planned end date cannot be before the start date");
        }

        /// <summary>
        /// Enrich a page of projects with owner/department names and weighted delivery progress.
        /// Names and task aggregates are loaded with separate IN queries (no one-to-many JOIN fan-out).
        /// Weighted progress = Σ(weight × progress) ÷ Σ(weight) — a non-trivial SQL aggregation.
        /// </summary>
        private void Enrich(List<PmoProject> projects)
        {
            if (projects == null || projects.Count == 0) return;
            var projectIds = projects.Select(p => p.ProjectId).ToList();
            var ownerIds = projects.Select(p => p.OwnerUserId).Distinct().ToList();
            var deptIds = projects.Select(p => p.DeptId).Distinct().ToList();

            var owners = Context.Queryable<SysUser>().Where(u => ownerIds.Contains(u.UserId))
                .Select(u => new { u.UserId, u.NickName, u.UserName }).ToList()
                .ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);

            var depts = Context.Queryable<SysDept>().Where(d => deptIds.Contains(d.DeptId))
                .Select(d => new { d.DeptId, d.DeptName }).ToList()
                .ToDictionary(d => d.DeptId, d => d.DeptName);

            var taskAgg = Context.Queryable<PmoTask>()
                .Where(t => projectIds.Contains(t.ProjectId))
                .GroupBy(t => t.ProjectId)
                .Select(t => new
                {
                    t.ProjectId,
                    Cnt = SqlFunc.AggregateCount(t.TaskId),
                    SumW = SqlFunc.AggregateSum(t.Weight),
                    SumWp = SqlFunc.AggregateSum(t.Weight * t.ProgressPct)
                }).ToList().ToDictionary(x => x.ProjectId, x => x);

            foreach (var p in projects)
            {
                p.OwnerName = owners.TryGetValue(p.OwnerUserId, out var on) ? on : null;
                p.DeptName = depts.TryGetValue(p.DeptId, out var dn) ? dn : null;
                if (taskAgg.TryGetValue(p.ProjectId, out var agg))
                {
                    p.TaskCount = agg.Cnt;
                    p.ProgressPct = agg.SumW > 0 ? Math.Round(agg.SumWp / agg.SumW, 1) : 0;
                }
            }
        }
    }
}
