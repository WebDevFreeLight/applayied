using System.Collections.Generic;
using FluentAssertions;
using Infrastructure.Model;
using Xunit;
using ZR.Model.PMO;
using ZR.ServiceCore.Localization;

namespace ZR.Service.Tests.PMO
{
    /// <summary>
    /// RBAC role-floor logic and bilingual (ar/en) response localization — pure unit tests,
    /// no database required. These mirror the server-side authorization the API enforces.
    /// </summary>
    public class PmoRbacAndI18nTests
    {
        private static PmoActor Actor(params string[] roles) =>
            new() { UserId = 10, UserName = "u", Roles = new List<string>(roles) };

        // ---------------- RBAC ----------------

        [Fact]
        public void Admin_outranks_pm_and_contributor()
        {
            var a = Actor(PmoRoleKeys.Admin);
            a.Level.Should().Be(3);
            a.IsAdmin.Should().BeTrue();
            a.IsProjectManager.Should().BeTrue();   // higher role satisfies lower floors
            a.IsContributor.Should().BeTrue();
        }

        [Fact]
        public void Pm_is_not_admin_but_satisfies_contributor_floor()
        {
            var pm = Actor(PmoRoleKeys.ProjectManager);
            pm.Level.Should().Be(2);
            pm.IsAdmin.Should().BeFalse();
            pm.IsProjectManager.Should().BeTrue();
            pm.IsContributor.Should().BeTrue();
        }

        [Fact]
        public void Contributor_cannot_satisfy_pm_or_admin_floor()
        {
            var c = Actor(PmoRoleKeys.Contributor);
            c.Level.Should().Be(1);
            c.IsProjectManager.Should().BeFalse();   // <-- Contributor calling a PM/finance route is denied
            c.IsAdmin.Should().BeFalse();
        }

        [Fact]
        public void No_roles_has_zero_level()
        {
            Actor().Level.Should().Be(0);
        }

        [Fact]
        public void System_super_admin_is_treated_as_pmo_admin()
        {
            var sa = new PmoActor { UserId = 1, IsSuperAdmin = true };
            sa.IsAdmin.Should().BeTrue();
            sa.Level.Should().Be(3);
        }

        [Theory]
        [InlineData("pmo_admin", 3)]
        [InlineData("pmo_pm", 2)]
        [InlineData("pmo_contributor", 1)]
        [InlineData("unknown", 0)]
        public void RankOf_maps_role_keys_to_floor(string key, int expected)
            => PmoActor.RankOf(key).Should().Be(expected);

        [Fact]
        public void From_token_copies_role_keys()
        {
            var token = new TokenModel { UserId = 5, UserName = "pm", RoleKeys = new() { PmoRoleKeys.ProjectManager } };
            var actor = PmoActor.From(token);
            actor.IsProjectManager.Should().BeTrue();
            actor.IsAdmin.Should().BeFalse();
        }

        // ---------------- i18n ----------------

        [Fact]
        public void Arabic_translates_known_rbac_message()
        {
            var ar = PmoMessageLocalizer.Translate("You do not have access to this project", "ar");
            ar.Should().NotBe("You do not have access to this project");
            ar.Should().Contain("المشروع");
        }

        [Fact]
        public void English_passes_through_unchanged()
        {
            const string msg = "Amount must be greater than zero";
            PmoMessageLocalizer.Translate(msg, "en").Should().Be(msg);
        }

        [Fact]
        public void Interpolated_message_is_translated_and_keeps_values()
        {
            var ar = PmoMessageLocalizer.Translate("Transition Draft → Approved is not permitted", "ar");
            ar.Should().Contain("الانتقال");
            ar.Should().Contain("Draft");     // concrete states preserved
            ar.Should().Contain("Approved");
        }

        [Fact]
        public void Unknown_message_falls_back_to_original()
        {
            const string msg = "Some brand new message not in the table";
            PmoMessageLocalizer.Translate(msg, "ar").Should().Be(msg);
        }

        [Theory]
        [InlineData("ar-SA,ar;q=0.9,en;q=0.8", "ar")]
        [InlineData("en-US,en;q=0.9", "en")]
        [InlineData("", "en")]
        public void LanguageFromHeader_parses_primary_language(string header, string expected)
            => PmoMessageLocalizer.LanguageFromHeader(header).Should().Be(expected);
    }
}
