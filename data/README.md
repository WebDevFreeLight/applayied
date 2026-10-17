# PMO Portal — Synthetic Data Package

Reproducible fictional data for the Project Portfolio & Financial Control Portal.
No real people, customers, vendors, credentials or company data are used.

## Contents

| File | Purpose |
|------|---------|
| `generate.py` | Deterministic generator (fixed seed `20260930`). Writes all CSVs + reports. |
| `csv/*.csv` | One file per table. Headers are the backend **entity property names**. |
| `data_dictionary.md` | Column-level documentation of every generated table. |
| `reconciliation.md` | KPI values at the baseline snapshot, for cross-system reconciliation. |

## How to (re)generate

```bash
cd data
python generate.py          # Python 3.8+, standard library only
```

Output is byte-for-byte identical on every run (seeded RNG), satisfying the
"repeatable output" requirement.

## How it is imported into PostgreSQL

Column casing in PostgreSQL is case-sensitive and the backend's ORM (SqlSugar) maps
each C# property to a same-cased quoted column. To avoid brittle hand-written SQL, the
CSVs are imported through the backend's **`PmoDataSeeder`** (C#), which reads these files
and inserts rows via the ORM — so the column mapping is always correct and the insert is
transactional and idempotent.

1. Point `backend/ZR.Admin.WebApi/appsettings.json` → `dbConfigs[0].Conn` at your Postgres DB
   (`DbType: 4`). Set `"InitDb": true` once so SqlSugar creates every `pmo_*` table, then
   set it back to `false`.
2. Set `"PmoSeed": true` (and optional `"PmoSeedPath"`) in `appsettings.json`, or call the
   admin-only endpoint `POST /pmo/admin/seed`. The seeder:
   - seeds 4 departments, 3 PMO roles (`pmo_admin`/`pmo_pm`/`pmo_contributor`) and 15 demo
     users with `sys_user_role` mappings (password `123456`, stored uppercase-MD5);
   - imports all `pmo_*` rows from `csv/`.
3. The seeder is **idempotent** — it skips tables that already contain seeded rows, so a
   restart will not duplicate data (changes entered in the app survive a restart).

## Demo logins

| Username | Role | Password |
|----------|------|----------|
| `pmo.admin` | Administrator (`pmo_admin`) | `123456` |
| `pm.hamdan` | Project Manager (`pmo_pm`) | `123456` |
| `con.mariam` | Contributor (`pmo_contributor`) | `123456` |

(15 users total — see `csv/users.csv` for the full list and role distribution.)

## Baseline snapshot

All baseline demonstrations use the fixed reporting cutoff **30 September 2026**.
In production the same aggregation endpoints receive "today" or a user-selected date —
the cutoff is a query parameter, never a stored constant.
