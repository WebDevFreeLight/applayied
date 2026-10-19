# Free deployment — PMO Portal (DB + API + Frontend)

Everything below uses **free tiers** and wires the three pieces together. You create the free
accounts and click deploy; the repo already contains all the config and is env-driven (no secrets
committed). Recommended free stack:

| Piece | Service (free) | What it hosts |
|-------|----------------|---------------|
| Database | **Neon** (or Render Postgres) | PostgreSQL `MahamDB` |
| API | **Render** (Docker web service) | `ZR.Admin.WebApi` (.NET 8) |
| Frontend | **Netlify** (or Vercel / Cloudflare Pages) | React/Vite static site |

The app is **env-driven**: the API reads `DATABASE_URL` + `PORT` + `UseHttps` + `corsUrls__0`; the
frontend reads `VITE_API_BASE`. Local dev still works unchanged (Vite proxies `/api` and `/msgHub`).

---

## 0) Push this repo to GitHub
Render and Netlify deploy from a Git repo.
```bash
git add -A && git commit -m "Add free-deploy config (Docker, Render blueprint, Netlify)"
git push        # to a GitHub repo you own
```

## 1) Database — Neon (longest-lived free Postgres)
1. Create a project at **neon.tech** → it gives a connection string like
   `postgresql://user:pass@ep-xxx.aws.neon.tech/mahamdb?sslmode=require`.
2. Keep it — you'll paste it as `DATABASE_URL` on the API in step 2.

*(Alternative: skip Neon and let the Render Blueprint create `pmo-db`; it auto-injects `DATABASE_URL`.
Render's free Postgres is time-limited, Neon isn't.)*

The API **creates all tables and seeds the synthetic PMO data on first boot** (SqlSugar code-first +
`data/csv`, which ships inside the image). No manual migration needed.

## 2) API — Render (Docker)
1. **render.com** → New ＋ → **Blueprint** → pick this repo. It reads [`render.yaml`](render.yaml)
   and creates `pmo-db` + `pmo-api`.
2. On **pmo-api → Environment**, set:
   - `DATABASE_URL` = your Neon string *(skip if you kept Render's `pmo-db` — it's auto-wired)*
   - `corsUrls__0` = your frontend URL from step 3 (come back after it exists), e.g.
     `https://pmo-portal.netlify.app` — **exact origin, no trailing slash**.
   - *(optional)* `AI__Providers__0__ApiKey`, `PowerBI__ClientSecret`, etc. to enable AI / Power BI.
3. Deploy. Your API is at `https://pmo-api.onrender.com` (note the exact name Render gives you).
   - First boot seeds the DB (can take a minute). Free web services **sleep after ~15 min idle** —
     the first request wakes it (~30–60s).

The container runs **HTTP only** behind Render's TLS (`UseHttps=false`, binds `$PORT`).

## 3) Frontend — Netlify
1. **netlify.com** → Add new site → import this repo. Netlify reads [`frontend/netlify.toml`](frontend/netlify.toml)
   (base = `frontend`, build `npm ci && npm run build`, publish `dist`, SPA redirect included).
2. **Site settings → Environment variables**, add:
   - `VITE_API_BASE` = your API URL from step 2, e.g. `https://pmo-api.onrender.com`
     *(the full https URL, no trailing slash — Vite inlines it at build time, so redeploy after setting it)*.
3. Deploy → your app is at e.g. `https://pmo-portal.netlify.app`.

## 4) Connect them (the integration)
- **Frontend → API:** `VITE_API_BASE` (step 3) makes axios **and** the SignalR hub (`/msgHub`) call
  the API host directly.
- **API → DB:** `DATABASE_URL` (step 2) overrides every `DbConfigs` connection to your Postgres.
- **CORS:** `corsUrls__0` on the API must equal the frontend origin, or the browser blocks calls.
  (The API already allows `localhost`/`127.0.0.1`; add your Netlify origin for production.)
- After setting `corsUrls__0`, **redeploy pmo-api**; after setting `VITE_API_BASE`, **redeploy the
  frontend**. Then open the Netlify URL and sign in.

---

## Test the API image locally (optional, needs Docker)
```bash
# from the repo root (context must be root so data/ is included)
docker build -f backend/Dockerfile -t pmo-api .
docker run --rm -p 8080:8080 \
  -e "DATABASE_URL=postgresql://user:pass@host/mahamdb?sslmode=require" \
  -e "corsUrls__0=http://localhost:5173" \
  pmo-api
# API on http://localhost:8080  (e.g. http://localhost:8080/pmo/powerbi/status)
```

## Notes & gotchas
- **Cold starts:** Render free API sleeps when idle; the first call after a nap is slow. Fine for demos.
- **`PdfiumViewer`** (a Windows-targeted PDF dependency) restores with a NU1701 warning; it's only used
  by optional PDF features. If a Linux container ever errors on it at runtime, that feature is the cause —
  the PMO endpoints don't need it.
- **Power BI** still needs real Azure AD creds (`PowerBI__*` env) to show a live report; otherwise the
  `/reports` page shows the preview + setup panel (by design).
- **Secrets stay out of Git:** real DB/AI/Power BI values live only in the host's env vars, never in
  `appsettings.json` (which keeps `YOUR_*` placeholders).
