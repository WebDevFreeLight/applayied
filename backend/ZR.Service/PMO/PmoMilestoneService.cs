using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Repository;
using ZR.ServiceCore;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>
    /// Milestone business logic: filtered listing with a due-within window, validation, and
    /// project-name enrichment. Dashboard surfaces milestones due soon and missed ones.
    /// </summary>
    [AppService(ServiceType = typeof(IPmoMilestoneService), ServiceLifetime = LifeTime.Transient)]
    public class PmoMilestoneService : BaseService<PmoMilestone>, IPmoMilestoneService
    {
        private static readonly string[] AllowedStatuses = { "Pending", "Achieved", "Missed" };

        public PagedInfo<PmoMilestone> GetList(PmoMilestoneQueryDto parm, PmoActor actor)
        {
            var cutoff = parm.Cutoff ?? DateTime.Today;

            var exp = Expressionable.Create<PmoMilestone>();
            exp.AndIF(parm.ProjectId != null, m => m.ProjectId == parm.ProjectId);
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), m => m.Status == parm.Status);

            // Due-within window: milestones due between the cutoff (today) and cutoff + N days.
            if (parm.DueWithinDays != null)
            {
                var upper = cutoff.AddDays(parm.DueWithinDays.Value);
                exp.And(m => m.DueDate != null && m.DueDate >= cutoff && m.DueDate <= upper);
            }

            var page = Queryable()
                .Where(exp.ToExpression())
                .OrderBy(m => m.DueDate == null)
                .OrderBy(m => m.DueDate, OrderByType.Asc)
                .OrderBy(m => m.MilestoneId, OrderByType.Desc)
                .ToPage(parm);

            Enrich(page.Result);
            return page;
        }

        public PmoMilestone GetInfo(long milestoneId, PmoActor actor)
        {
            var milestone = Queryable().First(m => m.MilestoneId == milestoneId)
                ?? throw new CustomException(ResultCode.FAIL, "Milestone not found");

            Enrich(new List<PmoMilestone> { milestone });
            return milestone;
        }

        public PmoMilestone AddMilestone(PmoMilestoneInputDto dto, PmoActor actor)
        {
            Validate(dto);

            var entity = new PmoMilestone
            {
                ProjectId = dto.ProjectId,
                Name = dto.Name.Trim(),
                DueDate = dto.DueDate,
                Status = string.IsNullOrEmpty(dto.Status) ? "Pending" : dto.Status,
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            entity.MilestoneId = Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        public int UpdateMilestone(PmoMilestoneInputDto dto, PmoActor actor)
        {
            var existing = Queryable().First(m => m.MilestoneId == dto.MilestoneId)
                ?? throw new CustomException(ResultCode.FAIL, "Milestone not found");

            Validate(dto);

            existing.ProjectId = dto.ProjectId;
            existing.Name = dto.Name.Trim();
            existing.DueDate = dto.DueDate;
            existing.Status = string.IsNullOrEmpty(dto.Status) ? "Pending" : dto.Status;
            existing.Update_by = actor.UserName;
            existing.Update_time = DateTime.Now;

            return Update(existing, true, "Updated milestone");
        }

        private static void Validate(PmoMilestoneInputDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new CustomException(ResultCode.FAIL, "Milestone name is required");
            if (!string.IsNullOrEmpty(dto.Status) && !AllowedStatuses.Contains(dto.Status))
                throw new CustomException(ResultCode.FAIL, "Status must be Pending, Achieved or Missed");
        }

        /// <summary>Enrich milestones with project names via a single IN query (no JOIN fan-out).</summary>
        private void Enrich(List<PmoMilestone> milestones)
        {
            if (milestones == null || milestones.Count == 0) return;
            var projectIds = milestones.Select(m => m.ProjectId).Distinct().ToList();

            var projects = Context.Queryable<PmoProject>().Where(p => projectIds.Contains(p.ProjectId))
                .Select(p => new { p.ProjectId, p.Name }).ToList()
                .ToDictionary(p => p.ProjectId, p => p.Name);

            foreach (var m in milestones)
                m.ProjectName = projects.TryGetValue(m.ProjectId, out var pn) ? pn : null;
        }
    }
}
