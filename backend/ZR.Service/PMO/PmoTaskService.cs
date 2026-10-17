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
    /// Delivery task business logic: scope-aware listing, validation, progress coupling,
    /// optimistic-concurrency-safe contributor updates, and the weighted project-progress helper.
    /// </summary>
    [AppService(ServiceType = typeof(IPmoTaskService), ServiceLifetime = LifeTime.Transient)]
    public class PmoTaskService : BaseService<PmoTask>, IPmoTaskService
    {
        public PagedInfo<PmoTask> GetList(PmoTaskQueryDto parm, PmoActor actor)
        {
            // Reference date for the overdue filter and IsOverdue enrichment.
            var cutoff = parm.Cutoff ?? DateTime.Today;

            var exp = Expressionable.Create<PmoTask>();
            exp.AndIF(parm.ProjectId != null, t => t.ProjectId == parm.ProjectId);
            exp.AndIF(parm.AssigneeUserId != null, t => t.AssigneeUserId == parm.AssigneeUserId);
            exp.AndIF(!string.IsNullOrEmpty(parm.Status), t => t.Status == parm.Status);
            exp.AndIF(!string.IsNullOrEmpty(parm.Priority), t => t.Priority == parm.Priority);
            if (!string.IsNullOrWhiteSpace(parm.Keyword))
                exp.And(t => t.Title.Contains(parm.Keyword));

            // Overdue = past due and not yet Done.
            if (parm.OverdueOnly == true)
                exp.And(t => t.DueDate != null && t.DueDate < cutoff && t.Status != "Done");

            // Read scope: admins & PMs see all tasks; contributors only tasks they are assigned
            // or belong to the project as a member. Server-side — not just a hidden menu.
            if (!actor.IsProjectManager)
            {
                long uid = actor.UserId;
                exp.And(t => t.AssigneeUserId == uid
                    || SqlFunc.Subqueryable<PmoProjectMember>().Where(m => m.ProjectId == t.ProjectId && m.UserId == uid).Any());
            }

            // Order by due date ascending with nulls last (undated tasks sink to the bottom),
            // then newest first as a stable tie-breaker.
            var page = Queryable()
                .Where(exp.ToExpression())
                .OrderBy(t => t.DueDate == null)
                .OrderBy(t => t.DueDate, OrderByType.Asc)
                .OrderBy(t => t.TaskId, OrderByType.Desc)
                .ToPage(parm);

            Enrich(page.Result, cutoff);
            return page;
        }

        public PmoTask GetInfo(long taskId, PmoActor actor)
        {
            var task = Queryable().First(t => t.TaskId == taskId)
                ?? throw new CustomException(ResultCode.FAIL, "Task not found");

            if (!actor.IsProjectManager && task.AssigneeUserId != actor.UserId
                && !Context.Queryable<PmoProjectMember>().Any(m => m.ProjectId == task.ProjectId && m.UserId == actor.UserId))
            {
                throw new CustomException(ResultCode.FORBIDDEN, "You do not have access to this task");
            }

            Enrich(new List<PmoTask> { task }, DateTime.Today);
            return task;
        }

        public PmoTask AddTask(PmoTaskInputDto dto, PmoActor actor)
        {
            Normalize(dto);

            var entity = new PmoTask
            {
                ProjectId = dto.ProjectId,
                Title = dto.Title.Trim(),
                AssigneeUserId = dto.AssigneeUserId,
                Status = string.IsNullOrEmpty(dto.Status) ? "ToDo" : dto.Status,
                Priority = string.IsNullOrEmpty(dto.Priority) ? "Medium" : dto.Priority,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                Weight = dto.Weight,
                ProgressPct = dto.ProgressPct,
                Create_by = actor.UserName,
                Create_time = DateTime.Now
            };
            // Apply the Done/100 coupling after defaults are resolved.
            CoupleStatusAndProgress(entity);

            entity.TaskId = Insertable(entity).ExecuteReturnSnowflakeId();
            return entity;
        }

        public int UpdateTask(PmoTaskInputDto dto, PmoActor actor)
        {
            var existing = Queryable().First(t => t.TaskId == dto.TaskId)
                ?? throw new CustomException(ResultCode.FAIL, "Task not found");

            Normalize(dto);

            existing.ProjectId = dto.ProjectId;
            existing.Title = dto.Title.Trim();
            existing.AssigneeUserId = dto.AssigneeUserId;
            existing.Status = string.IsNullOrEmpty(dto.Status) ? "ToDo" : dto.Status;
            existing.Priority = string.IsNullOrEmpty(dto.Priority) ? "Medium" : dto.Priority;
            existing.StartDate = dto.StartDate;
            existing.DueDate = dto.DueDate;
            existing.Weight = dto.Weight;
            existing.ProgressPct = dto.ProgressPct;
            existing.Version = dto.Version;
            existing.Update_by = actor.UserName;
            existing.Update_time = DateTime.Now;
            CoupleStatusAndProgress(existing);

            return Update(existing, true, "Updated task");
        }

        public int UpdateProgress(PmoTaskProgressDto dto, PmoActor actor)
        {
            var task = Queryable().First(t => t.TaskId == dto.TaskId)
                ?? throw new CustomException(ResultCode.FAIL, "Task not found");

            // Contributor rule: non-PMs may only update their own assigned tasks.
            if (!actor.IsProjectManager && task.AssigneeUserId != actor.UserId)
                throw new CustomException(ResultCode.FORBIDDEN, "You can only update your own assigned tasks");

            if (dto.ProgressPct < 0 || dto.ProgressPct > 100)
                throw new CustomException(ResultCode.FAIL, "Progress must be between 0 and 100");

            task.ProgressPct = dto.ProgressPct;
            if (!string.IsNullOrEmpty(dto.Status))
                task.Status = dto.Status;
            CoupleStatusAndProgress(task);

            // Carry the client-supplied version for optimistic concurrency validation.
            task.Version = dto.Version;
            task.Update_by = actor.UserName;
            task.Update_time = DateTime.Now;

            try
            {
                return Update(task, true, "Updated task progress");
            }
            catch (Exception ex) when (
                ex is VersionExceptions
                || (ex.Message != null && ex.Message.Contains("version", StringComparison.OrdinalIgnoreCase)))
            {
                // SqlSugar raises a version conflict when the row changed since it was read.
                throw new CustomException(ResultCode.FAIL, "This task was changed by someone else — reload and retry.");
            }
        }

        public decimal GetProjectProgress(long projectId)
        {
            // Weighted progress = Σ(weight × progress) ÷ Σ(weight), computed in a single aggregate query.
            var agg = Context.Queryable<PmoTask>()
                .Where(t => t.ProjectId == projectId)
                .GroupBy(t => t.ProjectId)
                .Select(t => new
                {
                    SumW = SqlFunc.AggregateSum(t.Weight),
                    SumWp = SqlFunc.AggregateSum(t.Weight * t.ProgressPct)
                }).First();

            if (agg == null || agg.SumW <= 0) return 0;
            return Math.Round(agg.SumWp / agg.SumW, 1);
        }

        /// <summary>Validate and normalize task input shared by add/update.</summary>
        private static void Normalize(PmoTaskInputDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new CustomException(ResultCode.FAIL, "Task title is required");
            if (dto.ProgressPct < 0 || dto.ProgressPct > 100)
                throw new CustomException(ResultCode.FAIL, "Progress must be between 0 and 100");
            if (dto.StartDate != null && dto.DueDate != null && dto.DueDate < dto.StartDate)
                throw new CustomException(ResultCode.FAIL, "Due date cannot be before the start date");
            // Weight must be positive; fall back to 1 so the weighted average stays well-defined.
            if (dto.Weight <= 0) dto.Weight = 1;
        }

        /// <summary>Keep status and progress consistent: Done ⇔ 100%.</summary>
        private static void CoupleStatusAndProgress(PmoTask task)
        {
            if (task.Status == "Done") task.ProgressPct = 100;
            else if (task.ProgressPct == 100) task.Status = "Done";
        }

        /// <summary>
        /// Enrich tasks with assignee/project names and the derived IsOverdue flag.
        /// Names are loaded with separate IN queries keyed into dictionaries (no JOIN fan-out),
        /// mirroring PmoProjectService.Enrich.
        /// </summary>
        private void Enrich(List<PmoTask> tasks, DateTime cutoff)
        {
            if (tasks == null || tasks.Count == 0) return;

            var assigneeIds = tasks.Where(t => t.AssigneeUserId != null)
                .Select(t => t.AssigneeUserId.Value).Distinct().ToList();
            var projectIds = tasks.Select(t => t.ProjectId).Distinct().ToList();

            var assignees = assigneeIds.Count == 0
                ? new Dictionary<long, string>()
                : Context.Queryable<SysUser>().Where(u => assigneeIds.Contains(u.UserId))
                    .Select(u => new { u.UserId, u.NickName, u.UserName }).ToList()
                    .ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);

            var projects = Context.Queryable<PmoProject>().Where(p => projectIds.Contains(p.ProjectId))
                .Select(p => new { p.ProjectId, p.Name }).ToList()
                .ToDictionary(p => p.ProjectId, p => p.Name);

            foreach (var t in tasks)
            {
                if (t.AssigneeUserId != null && assignees.TryGetValue(t.AssigneeUserId.Value, out var an))
                    t.AssigneeName = an;
                t.ProjectName = projects.TryGetValue(t.ProjectId, out var pn) ? pn : null;
                t.IsOverdue = t.DueDate != null && t.DueDate < cutoff && t.Status != "Done";
            }
        }
    }
}
