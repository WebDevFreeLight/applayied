# CV Update Notes — Abdulqader Alshawea (October 2026)

What changed between the old CV (`Abdulqader_AlShawea_CV.pdf`) and the new one
(`Abdulqader_AlShawea_CV_2026.docx` / `.pdf`). All other content from the old CV was kept.

---

## 1. Header
- **New headline:** Full Stack Developer · .NET · React / Vue / Angular · SQL Server / PostgreSQL · AI Integration
- **Clickable links:** email (mailto), phone (tel: +966 50 227 6010), LinkedIn, GitHub, Portfolio
- **Location:** Al Khobar, Eastern Province, Saudi Arabia

## 2. Professional Summary (rewritten)
> Full stack developer with 7+ years building web, desktop, mobile and microservice systems end to end, from database design and query optimization, through secure .NET APIs, to React, Vue and Angular frontends. Recent work includes healthcare information systems (HIS, EMR, IAM) for Almana Hospitals Group, Saudi integrations (ZATCA e-invoicing, Elm Yakeen, Infobip SMS), and a full PMO portfolio and financial-control portal with server-side RBAC, SQL-computed KPIs, Power BI reporting, background automation, multi-provider AI and a bilingual Arabic/English RTL interface. Background in interior design brings a strong eye for UX.

## 3. NEW section — Core Tech Stack
- **Languages:** C#, TypeScript, JavaScript, SQL (T-SQL, PL/pgSQL), Python, Java, Dart, DAX, HTML5, CSS3
- **Backend:** ASP.NET Core 8 Web API & MVC, .NET microservices, SignalR, BackgroundService workers, Java Spring Boot, Node.js, gRPC, RabbitMQ, Apache Kafka, API Gateway, Redis
- **ORM & Data Access:** SqlSugar ORM (code-first), Entity Framework Core, Spring Data JPA, stored procedures, dynamic SQL / sp_executesql
- **Frontend:** React 18/19, Vue.js, Angular, Next.js, Blazor, Vite, TailwindCSS, ECharts, Recharts, i18next (EN/AR, RTL), axios, jQuery
- **Databases:** SQL Server, PostgreSQL, SQLite, Oracle, MongoDB; query optimization, profiling, stored procedures, backups & migrations
- **Security:** JWT authentication, role-based access control (RBAC), OAuth2 (Gmail API), Identity / IAM, SSO, audit logging
- **Reporting & BI:** Power BI (PBIP/TMDL, 70+ DAX measures, embedded reports via Azure AD), Crystal Reports, RDLC, PDF generation
- **AI:** Multi-provider LLM integration with automatic failover (OpenAI-compatible: Groq, OpenRouter, Gemini, DeepSeek, Mistral, Qwen and more), Ollama
- **Testing:** xUnit, FluentAssertions, in-memory SQLite integration tests, Playwright (end-to-end & scraping)
- **DevOps & Cloud:** Docker (multi-stage), Render, Netlify, Neon Postgres, Azure, GitHub, GitHub Actions, Nginx, PowerShell
- **Mobile & Desktop:** Flutter, .NET MAUI / MAUI Blazor, .NET desktop apps, Firebase, AppWrite

## 4. NEW section — Featured Project: PMO Portal (Live)
**PMO Portal — Project Portfolio & Financial Control Platform** (2026) — *Full Stack Developer & Architect*

Business application for managing a project portfolio: projects, tasks, milestones, risks, change requests, budgets, cost transactions and forecasts, with a live executive dashboard and Power BI reporting. Fully bilingual (English / Arabic with RTL) in both the UI and API responses.

**Links:**
- Live app: https://pompro.netlify.app
- API: https://applayied.onrender.com
- Investment pitch: https://abdulqader-pmo-pitch.netlify.app
- Product showcase: https://abdulqader-pmo-showcase.netlify.app

**Backend**
- .NET 8 Web API with SqlSugar ORM (code-first) on PostgreSQL: 15 pmo_* tables, 15 entities, 14 services, 17 controllers and 40 documented REST routes across 9 groups.
- Server-enforced RBAC: a custom PmoAuthorize action filter reads the JWT and applies a role floor (Admin > Project Manager > Contributor) to every route, plus in-service rules for project scope, no self-approval and draft→posted control.
- Finance engine computed in SQL at query time: approved budget (initial + approved revisions), actual spend to a cutoff date, remaining budget, utilization, EAC = actual + forecast-to-complete, and projected variance.
- Change-request state machine (Draft → Submitted → Under Review → Approved / Rejected / Returned) that blocks illegal transitions; each decision and its history row commit in one transaction.
- Optimistic concurrency (row version) and double-submission guards to prevent lost updates.
- Background automation worker (BackgroundService + scheduled job) running idempotent rules that write to an outbox with unique dedupe keys; real-time notifications pushed to the browser over SignalR.
- AI assistant with a fallback chain across 12 OpenAI-compatible providers (30 s timeout each), also used to draft notification text.
- Power BI "embed for your organization" via an Azure AD service principal (client credentials → embed URL → view token).
- Localized API errors and validation messages (Accept-Language or ?lang=ar).
- Reproducible Python data generator (fixed seed, 17 CSVs, data dictionary and KPI reconciliation) imported by a transactional, idempotent C# seeder.
- 30 automated test cases (xUnit + FluentAssertions + in-memory SQLite) covering finance calculations, workflow, RBAC/i18n and concurrency.
- Containerised with a multi-stage Dockerfile; Render Blueprint (render.yaml) for API + Postgres; environment-driven config with no secrets in Git.

**Frontend**
- React 18 + TypeScript + Vite SPA with react-router, axios and a SignalR client; deployed on Netlify with SPA routing.
- Executive dashboard built with ECharts: 7 visuals, 6 filters and 2 drill-downs, recalculated from the API after every task update or posted cost.
- Modules: Dashboard, Projects (members, forecasts, snapshots), Tasks, Milestones, Risks, Change Requests, Finance, Reports (Power BI), Employees, Activity, plus 17 admin pages (users, roles, permission tree, menus, jobs, logs, sessions, server monitor).
- Live English/Arabic switching with full RTL layout using i18next.

**Reporting**
- Power BI project (PBIP / TMDL) connected directly to PostgreSQL: 72 DAX measures, Arabic (ar-SA) translations, 5 validated SQL queries (overdue tasks, financial performance, high-exposure risks, monthly spend vs forecast, milestone status) and a Power BI training course.

**Tech Stack:** ASP.NET Core 8, C#, SqlSugar ORM, PostgreSQL, JWT, SignalR, BackgroundService, xUnit, FluentAssertions, SQLite, Python, React 18, TypeScript, Vite, ECharts, i18next, Power BI, DAX, TMDL, Azure AD, Docker, Render, Netlify

## 5. NEW section — Interactive Presentations (Live)
- **PMO Portal — Investment Pitch** (2026) — https://abdulqader-pmo-pitch.netlify.app
  Offline single-file, 10-scene interactive pitch: architecture request trace, branching change-request use case, dashboard, business-case simulator (36-month cash curve, payback) and AI provider-failover demo. Vanilla JS sources inlined by a Python build script; companion PowerPoint generated from the same data model with Node.js. Verified with Playwright in headless Chromium at multiple resolutions.
- **PMO Portal — Product Showcase** (2026) — https://abdulqader-pmo-showcase.netlify.app
  Sales walkthrough of the PMO Portal (about 16 sections) with real English and Arabic screenshots of the dashboard, finance, risks, change requests, automation, RBAC and AI assistant.
- **AeroBridge — Autonomous Medical-Supply Logistics Pitch** (2026) — https://abdulqader-aerobridge.netlify.app
  9-scene cinematic pitch with self-drawing drone routes and a live fleet / hub / distance simulator.
- **MetroPulse — City Digital-Twin Command Centre Pitch** (2026) — https://abdulqader-metropulse.netlify.app
  9-scene pitch with a layered city map, live layer toggles, incident-response dispatch and an operations simulator.

**Tech Stack:** HTML5, CSS3, vanilla JavaScript, SVG path animation, Canvas particle fields, kinetic typography, parallax, reduced-motion accessibility, Python build tooling, Playwright

## 6. Work Experience — changes
- Order is now **newest first** (the old CV went oldest first).
- Every role keeps a **Tech Stack** line.
- **Almana Hospitals Group** — new bullet:
  - Wrote cross-location T-SQL stored procedures (dynamic SQL over every hospital location database) for patient search by ID, name, mobile and insurance.
  - Tech stack now also lists C# and T-SQL.
- **AI & Full Stack Developer (Freelance)** — now mentions the PMO Portal first (see Featured Project) and adds PostgreSQL to the tech stack.
- **Microservices Developer** — Redis caching added to the bullet (it was already in the old project list); tech stack expanded to .NET Core, Docker, gRPC, RabbitMQ, API Gateway, Redis, EF Core, JWT, Swagger/OpenAPI, DevOps.
- **Outsourcing Developer — Datatrans** — SQL Server added to the tech stack.
- **Web Applications Developer** — SQL Server added to the tech stack.
- **Mobile Applications Developer** — Dart and .NET MAUI added to the tech stack.
- **Desktop Applications Developer (2018–2019 and Freelance 2024–2025)** — C# / .NET added to the tech stack.
- Bullets for the smaller roles were merged into one line each to save space (no facts removed).

## 7. Selected Projects — changes
- Every project now has a **Backend: … Frontend: …** tech line.
- The ZATCA, Elm Yakeen and Infobip integrations were merged into one entry.
- The Mourjan scraper, Text Difference Checker, Dynamic Excel tool and PDF report engine were merged into one **Data Tools** entry.
- The order is now: HIS, EMR, IAM, Contract Management, Saudi integrations, VoiceOfJobs, Microservices CRM Suite, SQL Deployment Manager, Invoice Builder, CV Creator, E-commerce, MAUI Blazor Water App, Data Tools.

## 8. Skills — reorganised
The long "Professional & Soft Skills" lists were grouped into 4 lines:
- **Engineering:** System architecture & design, microservices, REST API design, database design & administration, query optimization, legacy system modernization, performance tuning, debugging & troubleshooting, code quality & testing, technical documentation, version control
- **Domain:** Healthcare IT (HIS, EMR, IAM), project portfolio management (PMO), finance & budgeting, recruitment & marketplaces, e-invoicing & billing, Saudi government integrations
- **Delivery:** Requirement analysis, project management, rapid prototyping, cross-functional collaboration, technical mentoring, process improvement, localization (i18n / RTL), executive presentations
- **Soft skills:** Problem solving, critical thinking, adaptability, working under pressure, teamwork, attention to detail, time management, self-motivation, UX thinking, bilingual communication (Arabic / English)

## 9. Unchanged
Education, Certifications (newest first now) and Languages (Arabic native, English fluent).

## 10. Please double-check
- "Full Stack Developer & Architect" as the PMO Portal role
- "7+ years" in the summary (counted from 2018)
- Removed from the skills list: "Social Media Marketing & Management" and several items that repeated tech already listed in Core Tech Stack
