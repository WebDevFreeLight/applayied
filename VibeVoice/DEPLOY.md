# Deploy VibeVoice for free — Render + Neon (mirrors `docker-compose.yml`)

One Render Blueprint (`VibeVoice/render.yaml`) creates the same services as `docker-compose.yml`:

| `docker-compose.yml` | Render service | Type (free) | URL |
|---|---|---|---|
| `api` | `vibevoice-api` | Docker web service | `https://vibevoice-api.onrender.com` |
| `voice-service` | `vibevoice-voice-service` | Docker web service | `https://vibevoice-voice-service.onrender.com` |
| `frontend` | `vibevoice-frontend` | Static site | `https://vibevoice-frontend.onrender.com` |
| `postgres` (profile) | **Neon** Postgres — database **and uploaded files** | Free (0.5 GB) | — |

**Differences from running compose on your PC**
- Render can't read a compose file: each compose service is a Render service, wired with public `https` URLs
  (free services can't use Render's private network).
- Render services **sleep after ~15 min** without traffic. The first visit after that takes **30–60 s** to wake up.
  The static frontend never sleeps.
- Render's disk is wiped on every restart, so notes **and** audio/video/photos are stored in Neon
  (`Storage__Provider=Database`) instead of the compose volumes.
- Uploads are capped: photos 5 MB, audio 10 MB, video 25 MB. Transcription uses Whisper *tiny* (fits 512 MB RAM).
- Render has **no GPUs**, so the Microsoft VibeVoice model can't run there. Speech uses the Edge neural voices
  fallback (Arabic + English), labelled "AI-generated" in the app as always.

Everything deploys from the branch **`feature/vibevoice-notes-memories`** (already set in `render.yaml`).

---

## 1. Neon — database (≈ 3 min)

1. Sign up at **https://neon.tech** (GitHub login works) ▸ **Create project**.
   Name `vibevoice`, Postgres 17, region **AWS US East 2 (Ohio)** — `render.yaml` puts the Render services
   in `ohio` so every query stays in the same data centre area. (If you pick another Neon region, change
   `region:` in `render.yaml` to the nearest Render region: `oregon`, `ohio`, `virginia`, `frankfurt`, `singapore`.)
2. On the project dashboard ▸ **Connect** ▸ copy the connection string. It looks like
   `postgresql://neondb_owner:xxxx@ep-xxx-123-pooler.c-8.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require`
   Paste it into Render exactly as Neon shows it — the API converts the URL form itself.
   Keep it private — it's the password to your data. **Never commit it to the repo.**

## 2. Render — all three services (≈ 10 min, mostly waiting for builds)

1. Sign in at **https://dashboard.render.com** with GitHub and allow access to `WebDevFreeLight/applayied`.
2. **New ▸ Blueprint** ▸ pick the repo ▸ branch **`feature/vibevoice-notes-memories`** ▸
   **Blueprint path: `VibeVoice/render.yaml`** ▸ name it `vibevoice-compose`.
   (Do **not** use the repo-root `render.yaml` — that one is the PMO portal.)
3. Render asks only for the two values marked `sync: false` (service `vibevoice-api`):

   | Key | Value |
   |---|---|
   | `Database__ConnectionString` | the Neon string from step 1 |
   | `Auth__BootstrapAdminEmail` | **your** email — only this address becomes Admin |

   Everything else is preset: the voice-service key is shared automatically and `Jwt__Key` is generated.
4. **Apply**. Three services build (frontend ~2 min, API ~4 min, voice service ~8 min — it installs Whisper + OCR).
5. **Check the URLs Render gave.** If any service got a suffix (e.g. `vibevoice-api-x1y2.onrender.com`), fix the
   matching value and save (it redeploys):

   | If this URL differs… | …update this |
   |---|---|
   | `vibevoice-frontend` | `vibevoice-api` ▸ Environment ▸ `Cors__Origins__0` |
   | `vibevoice-voice-service` | `vibevoice-api` ▸ Environment ▸ `VoiceService__BaseUrl` |
   | `vibevoice-api` | `vibevoice-frontend` ▸ Environment ▸ `VITE_API_URL` (then **Manual Deploy** — it's baked in at build) |

6. Check: open `https://vibevoice-api.onrender.com/api/health` →
   `{"status":"Healthy","database":"up","voiceService":"up"}` (`voiceService` may say `down` for a minute while it wakes).

## 3. First login

1. Open `https://vibevoice-frontend.onrender.com` ▸ **Register** with the **same email** you put in
   `Auth__BootstrapAdminEmail` → you're Admin.
2. Settings ▸ language, theme and visual identity. Users ▸ manage other accounts.
3. The microphone works because Render serves HTTPS.

## Updating

Push to `feature/vibevoice-notes-memories` → Render redeploys automatically
(only the service whose folder changed rebuilds, thanks to `buildFilter`).

## Optional: frontend on Vercel instead

`VibeVoice/frontend/vercel.json` still works: import the repo on Vercel with **Root Directory `VibeVoice/frontend`**,
set `VITE_API_URL` to the API URL and the three `VITE_MAX_*_MB` values (5 / 10 / 25), production branch
`feature/vibevoice-notes-memories`. Then set `Cors__Origins__0` on `vibevoice-api` to the Vercel URL.

## Troubleshooting

| Symptom | Fix |
|---|---|
| Login shows a network/CORS error | `Cors__Origins__0` must equal the frontend URL exactly (https, no trailing `/`). |
| Frontend calls the wrong API | `VITE_API_URL` is baked in at build: fix it, then **Manual Deploy** the static site. |
| First request hangs ~1 min | The free API was asleep — normal. |
| "Voice service unavailable" | It's waking up; retry after a minute. Check `VoiceService__BaseUrl` is the voice service's real URL. |
| Voice service logs `401 Missing or invalid X-Api-Key` | `VoiceService__ApiKey` on the API must equal `API_KEY` on the voice service (the Blueprint links them). |
| `413` / "too large" on upload | Free limits: photo 5 MB, audio 10 MB, video 25 MB. |
| API fails at start with a database error | Re-copy the Neon string (`sslmode=require` must be in it). |
