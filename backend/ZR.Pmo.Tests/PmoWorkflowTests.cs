using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    /// Change-request workflow tests against the REAL PmoChangeRequestService on in-memory SQLite:
    /// permitted transitions, self-approval prevention, reason enforcement, and the atomic
    /// decision + single history row.
    /// </summary>
    public class PmoWorkflowTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqlSugarScope _db;
        private readonly PmoChangeRequestService _svc;

        public PmoWorkflowTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"pmo_wf_{Guid.NewGuid():N}.db");
            _db = new SqlSugarScope(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={_dbPath}",
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
            });
            _db.CodeFirst.InitTables(typeof(PmoChangeRequest), typeof(PmoChangeHistory));

            // CR #1 submitted by user 5; CR #2 still a draft.
            _db.Insertable(new PmoChangeRequest { ChangeRequestId = 1, ProjectId = 1, Title = "CR1",
                RequestedByUserId = 5, Status = "Submitted", Version = 1 }).ExecuteCommand();
            _db.Insertable(new PmoChangeRequest { ChangeRequestId = 2, ProjectId = 1, Title = "CR2",
                RequestedByUserId = 5, Status = "Draft", Version = 1 }).ExecuteCommand();

            _svc = (PmoChangeRequestService)RuntimeHelpers.GetUninitializedObject(typeof(PmoChangeRequestService));
            _svc.Context = _db;
        }

        private static PmoActor Pm(long id) => new() { UserId = id, UserName = "pm" + id,
            Roles = new List<string> { PmoRoleKeys.ProjectManager } };

        [Fact]
        public void Cannot_decide_your_own_change_request()
        {
            var act = () => _svc.Decide(new PmoChangeRequestDecisionDto { ChangeRequestId = 1, Action = "Approve" }, Pm(5));
            act.Should().Throw<CustomException>().Where(e => e.Message.Contains("your own"));
        }

        [Fact]
        public void Valid_decision_commits_status_and_exactly_one_history_row()
        {
            _svc.Decide(new PmoChangeRequestDecisionDto { ChangeRequestId = 1, Action = "Approve" }, Pm(7));

            var cr = _db.Queryable<PmoChangeRequest>().First(c => c.ChangeRequestId == 1);
            cr.Status.Should().Be("Approved");
            cr.DecisionByUserId.Should().Be(7);

            var history = _db.Queryable<PmoChangeHistory>().Where(h => h.ChangeRequestId == 1).ToList();
            history.Should().HaveCount(1);                 // atomic: one decision → one history entry
            history[0].FromStatus.Should().Be("Submitted");
            history[0].ToStatus.Should().Be("Approved");
        }

        [Fact]
        public void Reject_requires_a_reason()
        {
            var act = () => _svc.Decide(new PmoChangeRequestDecisionDto { ChangeRequestId = 1, Action = "Reject" }, Pm(7));
            act.Should().Throw<CustomException>().Where(e => e.Message.Contains("reason"));
        }

        [Fact]
        public void Illegal_transition_is_rejected()
        {
            // Draft → Approved is not a permitted edge.
            var act = () => _svc.Decide(new PmoChangeRequestDecisionDto { ChangeRequestId = 2, Action = "Approve" }, Pm(7));
            act.Should().Throw<CustomException>().Where(e => e.Message.Contains("not permitted"));
        }

        [Fact]
        public void Allowed_transition_table_matches_documented_state_machine()
        {
            var field = typeof(PmoChangeRequestService).GetField("Allowed", BindingFlags.NonPublic | BindingFlags.Static);
            var allowed = (HashSet<(string, string)>)field!.GetValue(null)!;

            allowed.Should().Contain(("Draft", "Submitted"));
            allowed.Should().Contain(("Returned", "Submitted"));
            allowed.Should().Contain(("Submitted", "UnderReview"));
            allowed.Should().Contain(("UnderReview", "Approved"));
            allowed.Should().Contain(("UnderReview", "Rejected"));

            allowed.Should().NotContain(("Draft", "Approved"));   // cannot skip submission
            allowed.Should().NotContain(("Approved", "Rejected")); // terminal state
        }

        public void Dispose()
        {
            _db?.Dispose();
            try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* temp file */ }
        }
    }
}
