using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Infrastructure;
using SqlSugar;
using Xunit;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO;

namespace ZR.Service.Tests.PMO
{
    /// <summary>
    /// Concurrency &amp; double-submission tests against the REAL services on in-memory SQLite.
    ///
    /// Two independent protections are proven here:
    ///   1. Optimistic version checking on PmoTask — a stale write (based on an out-of-date row
    ///      version) is rejected instead of silently clobbering the newer value (lost-update).
    ///   2. Double-submission prevention on the change-request workflow — re-submitting the same
    ///      decision is refused because the state machine has no edge out of the terminal state.
    /// </summary>
    public class PmoConcurrencyTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqlSugarScope _db;

        public PmoConcurrencyTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"pmo_cc_{Guid.NewGuid():N}.db");
            _db = new SqlSugarScope(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={_dbPath}",
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
            });
            _db.CodeFirst.InitTables(typeof(PmoTask), typeof(PmoChangeRequest), typeof(PmoChangeHistory));
        }

        private static PmoActor Pm(long id) => new()
        {
            UserId = id,
            UserName = "pm" + id,
            Roles = new List<string> { PmoRoleKeys.ProjectManager }
        };

        private PmoTaskService TaskSvc()
        {
            var svc = (PmoTaskService)RuntimeHelpers.GetUninitializedObject(typeof(PmoTaskService));
            svc.Context = _db;
            return svc;
        }

        private PmoChangeRequestService CrSvc()
        {
            var svc = (PmoChangeRequestService)RuntimeHelpers.GetUninitializedObject(typeof(PmoChangeRequestService));
            svc.Context = _db;
            return svc;
        }

        // ---------- 1. Optimistic version checking (lost-update prevention) ----------

        [Fact]
        public void First_writer_wins_and_increments_the_row_version()
        {
            _db.Insertable(new PmoTask { TaskId = 10, ProjectId = 1, Title = "T", AssigneeUserId = 7,
                Status = "ToDo", Priority = "Medium", Weight = 1, ProgressPct = 0, Version = 1 }).ExecuteCommand();
            var svc = TaskSvc();

            // Client read version 1, submits an update carrying version 1 -> accepted.
            svc.UpdateProgress(new PmoTaskProgressDto { TaskId = 10, ProgressPct = 40, Version = 1 }, Pm(7));

            var row = _db.Queryable<PmoTask>().First(t => t.TaskId == 10);
            row.ProgressPct.Should().Be(40);
            row.Version.Should().BeGreaterThan(1);   // SqlSugar bumped the version on a successful write
        }

        [Fact]
        public void Stale_second_writer_is_rejected_not_silently_overwritten()
        {
            _db.Insertable(new PmoTask { TaskId = 11, ProjectId = 1, Title = "T", AssigneeUserId = 7,
                Status = "ToDo", Priority = "Medium", Weight = 1, ProgressPct = 0, Version = 1 }).ExecuteCommand();
            var svc = TaskSvc();

            // Two clients both read the row at version 1. Client A commits first (version -> 2).
            svc.UpdateProgress(new PmoTaskProgressDto { TaskId = 11, ProgressPct = 50, Version = 1 }, Pm(7));

            // Client B commits with the now-stale version 1 -> conflict, surfaced as a friendly error.
            var stale = () => svc.UpdateProgress(new PmoTaskProgressDto { TaskId = 11, ProgressPct = 90, Version = 1 }, Pm(7));
            stale.Should().Throw<CustomException>()
                 .Where(e => e.Message.Contains("changed by someone else", StringComparison.OrdinalIgnoreCase));

            // Client A's value survived; the stale write did NOT clobber it (no lost update).
            _db.Queryable<PmoTask>().First(t => t.TaskId == 11).ProgressPct.Should().Be(50);
        }

        // ---------- 2. Double-submission prevention (idempotent workflow) ----------

        [Fact]
        public void Approving_twice_is_refused_the_second_time()
        {
            _db.Insertable(new PmoChangeRequest { ChangeRequestId = 20, ProjectId = 1, Title = "CR",
                RequestedByUserId = 5, Status = "Submitted", Version = 1 }).ExecuteCommand();
            var svc = CrSvc();

            // First approval succeeds: Submitted -> Approved.
            svc.Decide(new PmoChangeRequestDecisionDto { ChangeRequestId = 20, Action = "Approve" }, Pm(7));
            _db.Queryable<PmoChangeRequest>().First(c => c.ChangeRequestId == 20).Status.Should().Be("Approved");

            // A duplicate submission of the same decision is refused — Approved is terminal, so there
            // is no Approved -> Approved edge. Exactly one history row exists (no double booking).
            var again = () => svc.Decide(new PmoChangeRequestDecisionDto { ChangeRequestId = 20, Action = "Approve" }, Pm(7));
            again.Should().Throw<CustomException>().Where(e => e.Message.Contains("not permitted"));

            _db.Queryable<PmoChangeHistory>().Where(h => h.ChangeRequestId == 20).ToList().Should().HaveCount(1);
        }

        public void Dispose()
        {
            _db?.Dispose();
            try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* temp file */ }
        }
    }
}
