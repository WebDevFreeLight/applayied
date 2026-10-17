using SqlSugar;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ZR.Model.PMO;
using ZR.Model.System;

namespace ZR.Service.PMO
{
    /// <summary>
    /// Imports the reproducible synthetic data package (../data/csv/*.csv) into the database
    /// through SqlSugar, so column mapping is always correct regardless of Postgres casing.
    ///
    /// Characteristics required by the assignment:
    ///  - Transactional: the whole import commits atomically or rolls back (no partial seed).
    ///  - Idempotent: a table that already contains rows is skipped, so data entered in the app
    ///    survives a restart and re-seeding never duplicates rows.
    ///  - Seeds identity too: 4 departments, 3 PMO roles and 15 demo users (+ user-role links)
    ///    so the three roles can sign in (password "123456", stored uppercase-MD5).
    /// </summary>
    public class PmoDataSeeder
    {
        // MD5("123456") in uppercase — the format the login path produces and compares against.
        private const string DemoPasswordMd5 = "E10ADC3949BA59ABBE56E057F20F883E";

        private readonly ISqlSugarClient _db;

        public PmoDataSeeder(ISqlSugarClient db = null)
        {
            _db = db ?? DbScoped.SugarScope.GetConnectionScope(0);
        }

        /// <summary>Run the full seed. Returns a human-readable log of what happened.</summary>
        public List<string> Seed(string csvDir)
        {
            var log = new List<string>();
            if (!Directory.Exists(csvDir))
            {
                log.Add($"[PMO seed] CSV directory not found: {csvDir}");
                return log;
            }

            // Make sure every pmo_* table exists (CodeFirst is non-destructive: it only creates
            // missing tables and adds missing columns). This lets PmoSeed work without a separate
            // InitDb pass. DDL runs outside the data transaction.
            EnsureTables(log);

            try
            {
                _db.Ado.BeginTran();

                // Identity first — they are FK targets for every pmo_* table.
                SeedDepartments(csvDir, log);
                SeedRolesAndUsers(csvDir, log);

                // pmo_* tables in dependency order.
                Import<PmoCostCategory>(csvDir, "cost_categories.csv", log);
                Import<PmoProject>(csvDir, "projects.csv", log);
                Import<PmoProjectMember>(csvDir, "project_members.csv", log);
                Import<PmoTask>(csvDir, "tasks.csv", log);
                Import<PmoMilestone>(csvDir, "milestones.csv", log);
                Import<PmoRisk>(csvDir, "risks.csv", log);
                Import<PmoChangeRequest>(csvDir, "change_requests.csv", log);
                Import<PmoChangeHistory>(csvDir, "change_history.csv", log);
                Import<PmoBudgetRevision>(csvDir, "budget_revisions.csv", log);
                Import<PmoCostTransaction>(csvDir, "cost_transactions.csv", log);
                Import<PmoFinancialForecast>(csvDir, "financial_forecasts.csv", log);
                Import<PmoProgressSnapshot>(csvDir, "progress_snapshots.csv", log);
                Import<PmoAutomationRun>(csvDir, "automation_runs.csv", log);
                Import<PmoOutboxNotification>(csvDir, "outbox_notifications.csv", log);
                Import<PmoAuditEvent>(csvDir, "audit_events.csv", log);

                _db.Ado.CommitTran();
                log.Add("[PMO seed] Committed successfully.");
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                log.Add($"[PMO seed] ROLLED BACK due to error: {ex.Message}");
                throw;
            }

            return log;
        }

        // ------------------------------------------------------------------ schema

        /// <summary>Create any missing pmo_* tables (non-destructive CodeFirst).</summary>
        private void EnsureTables(List<string> log)
        {
            try
            {
                // Align the reused identity tables with the current entities first — the legacy
                // MahamDB sys_user may predate columns the entity now defines (all added columns
                // are nullable; CodeFirst never drops or narrows existing columns).
                _db.CodeFirst.InitTables(
                    typeof(SysDept), typeof(SysRole), typeof(SysUserRole), typeof(SysUser));

                _db.CodeFirst.InitTables(
                    typeof(PmoCostCategory), typeof(PmoProject), typeof(PmoProjectMember),
                    typeof(PmoTask), typeof(PmoMilestone), typeof(PmoRisk),
                    typeof(PmoChangeRequest), typeof(PmoChangeHistory),
                    typeof(PmoBudgetRevision), typeof(PmoCostTransaction),
                    typeof(PmoFinancialForecast), typeof(PmoProgressSnapshot),
                    typeof(PmoAutomationRun), typeof(PmoOutboxNotification), typeof(PmoAuditEvent));
                log.Add("[PMO seed] ensured pmo_* tables exist");
            }
            catch (Exception ex)
            {
                log.Add($"[PMO seed] EnsureTables warning: {ex.Message}");
            }
        }

        // ------------------------------------------------------------------ identity

        private void SeedDepartments(string dir, List<string> log)
        {
            var rows = ReadCsv(Path.Combine(dir, "departments.csv"));
            var depts = new List<SysDept>();
            foreach (var r in rows)
            {
                long id = long.Parse(r["DeptId"]);
                if (_db.Queryable<SysDept>().Any(d => d.DeptId == id)) continue;
                depts.Add(new SysDept
                {
                    DeptId = id,
                    ParentId = 0,
                    Ancestors = "0",
                    DeptName = r["DeptName"],
                    OrderNum = 1,
                    Leader = r.GetValueOrDefault("Leader"),
                    Status = 0,
                    DelFlag = 0,
                    Create_by = "seed",
                    Create_time = DateTime.Now
                });
            }
            if (depts.Count > 0) _db.Insertable(depts).ExecuteCommand();
            log.Add($"[PMO seed] departments: inserted {depts.Count}");
        }

        private void SeedRolesAndUsers(string dir, List<string> log)
        {
            // 3 PMO roles (role_key carried into the JWT as RoleKeys → drives RBAC). sys_role.RoleId
            // is an IDENTITY column, so a provided id is ignored — we insert by key, then read back the
            // DB-assigned ids to wire the user-role links (never assume hardcoded ids).
            var roles = new List<(int Sort, string Key, string Name, string NameAr)>
            {
                (1, PmoRoleKeys.Admin,          "PMO Administrator",   "مدير مكتب المشاريع"),
                (2, PmoRoleKeys.ProjectManager, "PMO Project Manager", "مدير مشروع"),
                (3, PmoRoleKeys.Contributor,    "PMO Contributor",     "مساهم"),
            };
            var newRoles = new List<SysRole>();
            foreach (var (sort, key, name, _) in roles)
            {
                if (_db.Queryable<SysRole>().Any(x => x.RoleKey == key)) continue;
                newRoles.Add(new SysRole
                {
                    RoleName = name, RoleKey = key, RoleSort = sort,
                    Status = 0, DelFlag = 0,
                    DataScope = 1, // all-data scope; PMO scoping is enforced in services, not sys dataScope
                    Create_by = "seed", Create_time = DateTime.Now
                });
            }
            if (newRoles.Count > 0) _db.Insertable(newRoles).ExecuteCommand();
            log.Add($"[PMO seed] roles: inserted {newRoles.Count}");

            // Resolve the ACTUAL (DB-assigned) role ids by key.
            var keys = roles.Select(x => x.Key).ToList();
            var roleIdByKey = _db.Queryable<SysRole>().Where(x => keys.Contains(x.RoleKey))
                .ToList().ToDictionary(x => x.RoleKey, x => x.RoleId);

            // Repair: drop any seeded-user links that point to a role which no longer exists
            // (e.g. earlier runs wrote links to ids that identity re-assigned).
            var existingRoleIds = _db.Queryable<SysRole>().Select(x => x.RoleId).ToList();
            _db.Deleteable<SysUserRole>()
                .Where(x => x.UserId >= 1001 && x.UserId <= 1015 && !existingRoleIds.Contains(x.RoleId))
                .ExecuteCommand();

            var users = ReadCsv(Path.Combine(dir, "users.csv"));
            var newUsers = new List<SysUser>();
            var newLinks = new List<SysUserRole>();
            foreach (var r in users)
            {
                long uid = long.Parse(r["UserId"]);
                if (!_db.Queryable<SysUser>().Any(u => u.UserId == uid))
                {
                    newUsers.Add(new SysUser
                    {
                        UserId = uid,
                        UserName = r["UserName"],
                        NickName = r["NickName"],
                        DeptId = long.Parse(r["DeptId"]),
                        Sex = int.TryParse(r.GetValueOrDefault("Sex"), out var s) ? s : 0,
                        Email = r["UserName"] + "@pmo.local",
                        UserType = "00",
                        Password = DemoPasswordMd5,
                        Status = 0,
                        DelFlag = 0,
                        IsActivated = 1,
                        EmailVerified = 1,
                        Remark = "PMO demo user",
                        Create_by = "seed",
                        Create_time = DateTime.Now
                    });
                }
                var key = r.GetValueOrDefault("RoleKey");
                if (key != null && roleIdByKey.TryGetValue(key, out var rid)
                    && !_db.Queryable<SysUserRole>().Any(x => x.UserId == uid && x.RoleId == rid))
                {
                    newLinks.Add(new SysUserRole { UserId = uid, RoleId = rid });
                }
            }
            if (newUsers.Count > 0) _db.Insertable(newUsers).ExecuteCommand();
            if (newLinks.Count > 0) _db.Insertable(newLinks).ExecuteCommand();
            log.Add($"[PMO seed] users: inserted {newUsers.Count}, user-role links: {newLinks.Count}");

            // PMO controllers are guarded by [ActionPermissionFilter(Permission="pmo:*")]; seed those
            // permissions as sys_menu rows and grant them to the three roles by tier so Contributor/PM/
            // Admin keep exactly the access they had under the old role floors.
            SeedPmoPermissions(roleIdByKey, log);
        }

        // (perm, display name, tier) — ALL = contributor+pm+admin, PM = pm+admin, ADMIN = admin only.
        private static readonly (string Perm, string Name, string Tier)[] PmoPermissionCatalog =
        {
            ("pmo:project:list", "PMO Project List", "ALL"), ("pmo:project:query", "PMO Project View", "ALL"),
            ("pmo:project:add", "PMO Project Add", "PM"), ("pmo:project:edit", "PMO Project Edit", "PM"),
            ("pmo:task:list", "PMO Task List", "ALL"), ("pmo:task:query", "PMO Task View", "ALL"), ("pmo:task:progress", "PMO Task Progress", "ALL"),
            ("pmo:task:add", "PMO Task Add", "PM"), ("pmo:task:edit", "PMO Task Edit", "PM"),
            ("pmo:milestone:list", "PMO Milestone List", "ALL"), ("pmo:milestone:query", "PMO Milestone View", "ALL"),
            ("pmo:milestone:add", "PMO Milestone Add", "PM"), ("pmo:milestone:edit", "PMO Milestone Edit", "PM"),
            ("pmo:risk:list", "PMO Risk List", "ALL"), ("pmo:risk:query", "PMO Risk View", "ALL"),
            ("pmo:risk:add", "PMO Risk Add", "PM"), ("pmo:risk:edit", "PMO Risk Edit", "PM"),
            ("pmo:changerequest:list", "PMO CR List", "ALL"), ("pmo:changerequest:query", "PMO CR View", "ALL"),
            ("pmo:changerequest:add", "PMO CR Add", "ALL"), ("pmo:changerequest:edit", "PMO CR Edit", "ALL"), ("pmo:changerequest:submit", "PMO CR Submit", "ALL"),
            ("pmo:changerequest:decision", "PMO CR Decision", "PM"),
            ("pmo:finance:list", "PMO Finance View", "PM"), ("pmo:finance:add", "PMO Finance Add", "PM"), ("pmo:finance:edit", "PMO Finance Edit", "PM"),
            ("pmo:finance:post", "PMO Finance Post", "PM"), ("pmo:finance:revision", "PMO Finance Revision", "PM"),
            ("pmo:finance:approve", "PMO Finance Approve", "PM"), ("pmo:finance:forecast", "PMO Finance Forecast", "PM"),
            ("pmo:dashboard:view", "PMO Dashboard", "PM"),
            ("pmo:lookup:list", "PMO Lookups", "ALL"),
            ("pmo:category:list", "PMO Category List", "ALL"), ("pmo:category:add", "PMO Category Add", "ADMIN"), ("pmo:category:edit", "PMO Category Edit", "ADMIN"),
        };

        private void SeedPmoPermissions(Dictionary<string, long> roleIdByKey, List<string> log)
        {
            if (!roleIdByKey.TryGetValue(PmoRoleKeys.Contributor, out var cId)
                || !roleIdByKey.TryGetValue(PmoRoleKeys.ProjectManager, out var pId)
                || !roleIdByKey.TryGetValue(PmoRoleKeys.Admin, out var aId))
            {
                log.Add("[PMO seed] pmo permissions: roles not resolved, skipped");
                return;
            }
            long[] Tier(string t) => t == "ALL" ? new[] { cId, pId, aId } : t == "PM" ? new[] { pId, aId } : new[] { aId };

            int menus = 0, links = 0;
            foreach (var (perm, name, tier) in PmoPermissionCatalog)
            {
                var existing = _db.Queryable<SysMenu>().First(m => m.Perms == perm);
                long menuId;
                if (existing == null)
                {
                    var m = new SysMenu
                    {
                        MenuName = name, Perms = perm, MenuType = "F", ParentId = 0, OrderNum = 1,
                        Path = "", Component = "", Icon = "", Visible = "1", Status = "0",
                        Create_by = "seed", Create_time = DateTime.Now
                    };
                    menuId = _db.Insertable(m).ExecuteReturnSnowflakeId();
                    menus++;
                }
                else menuId = existing.MenuId;

                foreach (var rid in Tier(tier))
                {
                    if (!_db.Queryable<SysRoleMenu>().Any(x => x.Role_id == rid && x.Menu_id == menuId))
                    {
                        _db.Insertable(new SysRoleMenu { Role_id = rid, Menu_id = menuId }).ExecuteCommand();
                        links++;
                    }
                }
            }
            log.Add($"[PMO seed] pmo permissions: {menus} menus created, {links} role grants added");
        }

        // ------------------------------------------------------------------ generic import

        /// <summary>Insert every row of <paramref name="file"/> as a <typeparamref name="T"/>, unless the table already has rows.</summary>
        private void Import<T>(string dir, string file, List<string> log) where T : class, new()
        {
            if (_db.Queryable<T>().Any())
            {
                log.Add($"[PMO seed] {file}: skipped (table already has data)");
                return;
            }
            var rows = ReadCsv(Path.Combine(dir, file));
            var list = rows.Select(MapRow<T>).ToList();
            var now = DateTime.Now;
            foreach (var e in list)
            {
                if (e is SysBase sb)
                {
                    sb.Create_by = "seed";
                    sb.Create_time = now;
                }
            }
            if (list.Count > 0) _db.Insertable(list).ExecuteCommand();
            log.Add($"[PMO seed] {file}: inserted {list.Count}");
        }

        /// <summary>Build an entity from a CSV row, matching headers to same-named properties with type conversion.</summary>
        private static T MapRow<T>(Dictionary<string, string> row) where T : new()
        {
            var entity = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                if (!p.CanWrite) continue;
                if (!row.TryGetValue(p.Name, out var raw)) continue;
                if (string.IsNullOrEmpty(raw)) continue; // leave default/null
                p.SetValue(entity, Convert(raw, p.PropertyType));
            }
            return entity;
        }

        private static object Convert(string raw, Type target)
        {
            var t = Nullable.GetUnderlyingType(target) ?? target;
            if (t == typeof(string)) return raw;
            if (t == typeof(long)) return long.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(decimal)) return decimal.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(double)) return double.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(bool)) return raw == "1" || raw.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (t == typeof(DateTime)) return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            return System.Convert.ChangeType(raw, t, CultureInfo.InvariantCulture);
        }

        // ------------------------------------------------------------------ minimal RFC-4180 CSV reader

        private static List<Dictionary<string, string>> ReadCsv(string path)
        {
            var result = new List<Dictionary<string, string>>();
            if (!File.Exists(path)) return result;

            var text = File.ReadAllText(path, Encoding.UTF8);
            if (text.Length > 0 && text[0] == '﻿') text = text.Substring(1); // strip BOM

            var records = ParseCsv(text);
            if (records.Count == 0) return result;

            var headers = records[0];
            for (int i = 1; i < records.Count; i++)
            {
                var fields = records[i];
                var dict = new Dictionary<string, string>();
                for (int c = 0; c < headers.Count; c++)
                {
                    dict[headers[c]] = c < fields.Count ? fields[c] : "";
                }
                result.Add(dict);
            }
            return result;
        }

        /// <summary>Parse CSV text into rows of fields, honouring quoted fields, escaped quotes and embedded commas/newlines.</summary>
        private static List<List<string>> ParseCsv(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else field.Append(ch);
                }
                else
                {
                    switch (ch)
                    {
                        case '"': inQuotes = true; break;
                        case ',': row.Add(field.ToString()); field.Clear(); break;
                        case '\r': break;
                        case '\n':
                            row.Add(field.ToString()); field.Clear();
                            rows.Add(row); row = new List<string>();
                            break;
                        default: field.Append(ch); break;
                    }
                }
            }
            // flush trailing field/row (file may not end with newline)
            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row);
            }
            return rows;
        }
    }
}
