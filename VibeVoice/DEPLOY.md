# Deploy VibeVoice for free — Vercel + Render + Neon

| Part | Host | Plan | URL (example) |
|---|---|---|---|
| Frontend (React) | **Vercel** | Hobby (free) | `https://vibevoice.vercel.app` |
| API (.NET) | **Render** | Free | `https://vibevoice-api.onrender.com` |
| Voice service (TTS / transcription / OCR) | **Render** | Free | `https://vibevoice-voice.onrender.com` |
| Database **and uploaded files** | **Neon** Postgres | Free (0.5 GB) | — |

**What to expect on free plans**
- Render services **sleep after ~15 min** without traffic. The first visit after that takes **30–60 s** to wake up.
- Render's disk is wiped on every restart, so notes **and** audio/video/photos are stored in Neon (`Storage__Provider=Database`).
- Uploads are capped: photos 5 MB, audio 10 MB, video 25 MB. Transcription uses Whisper *tiny* (fits 512 MB RAM).
- Render has **no GPUs**, so the Microsoft VibeVoice model can't run there. Speech uses the Edge neural voices
  fallback (Arabic + English), labelled "AI-generated" in the app as always.

Everything deploys from the branch **`feature/vibevoice-notes-memories`** (already set in `render.yaml`).

---

## 1. Neon — database (≈ 3 min)

1. Sign up at **https://neon.tech** (GitHub login works) ▸ **Create project**.
   Name `vibevoice`, Postgres 17, region **AWS US East 2 (Ohio)** — `render.yaml` puts both Render services
   in `ohio` so every query stays in the same data centre area. (If you pick another Neon region, change
   `region:` in `render.yaml` to the nearest Render region: `oregon`, `ohio`, `virginia`, `frankfurt`, `singapore`.)
2. On the project dashboard ▸ **Connect** ▸ copy the connection string. It looks like
   `postgresql://neondb_owner:xxxx@ep-xxx-123-pooler.c-8.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require`
   Paste it into Render exactly as Neon shows it — the API converts the URL form itself.
   Keep it private — it's the password to your data. **Never commit it to the repo.**

## 2. Render — API + voice service (≈ 10 min, mostly waiting for builds)

1. Sign in at **https://dashboard.render.com** with GitHub and allow access to `WebDevFreeLight/applayied`.
2. **New ▸ Blueprint** ▸ pick the repo ▸ **Blueprint path: `VibeVoice/render.yaml`** ▸ name it `vibevoice`.
3. Render asks for the values marked `sync: false`. Fill in:

   | Key (service `vibevoice-api`) | Value |
   |---|---|
   | `Database__ConnectionString` | the Neon string from step 1 |
   | `Auth__BootstrapAdminEmail` | **your** email — only this address becomes Admin |
   | `Cors__Origins__0` | `https://vibevoice.vercel.app` (fix it after step 3 if Vercel gives another name) |
   | `VoiceService__BaseUrl` | `https://vibevoice-voice.onrender.com` (check the real URL of the voice service after it's created) |

4. **Apply**. Two services build (API ~4 min, voice service ~8 min — it installs Whisper + OCR).
5. Check: open `https://vibevoice-api.onrender.com/api/health` →
   `{"status":"Healthy","database":"up","voiceService":"up"}` (`voiceService` may say `down` for a minute while it wakes).

## 3. Vercel — frontend (≈ 3 min)

1. Sign in at **https://vercel.com** with GitHub ▸ **Add New ▸ Project** ▸ import `WebDevFreeLight/applayied`.
2. **Root Directory: `VibeVoice/frontend`** (framework Vite is detected; build settings come from `vercel.json`).
3. **Environment Variables**:

   | Name | Value |
   |---|---|
   | `VITE_API_URL` | `https://vibevoice-api.onrender.com` (your API URL, no trailing `/`) |
   | `VITE_MAX_IMAGE_MB` | `5` |
   | `VITE_MAX_AUDIO_MB` | `10` |
   | `VITE_MAX_VIDEO_MB` | `25` |

4. Under **Settings ▸ Git ▸ Production Branch** choose `feature/vibevoice-notes-memories` (or merge it to `main`).
5. **Deploy**. Note the URL Vercel gives you. If it isn't `https://vibevoice.vercel.app`, go back to Render ▸
   `vibevoice-api` ▸ **Environment** ▸ set `Cors__Origins__0` to the real URL ▸ **Save** (it redeploys).

## 4. First login

1. Open your Vercel URL ▸ **Register** with the **same email** you put in `Auth__BootstrapAdminEmail` → you're Admin.
2. Settings ▸ language, theme and visual identity. Users ▸ manage other accounts.
3. The microphone works because Vercel serves HTTPS.

## Updating

Push to `feature/vibevoice-notes-memories` → Vercel and Render redeploy automatically
(Render only rebuilds the service whose folder changed, thanks to `buildFilter`).

## Troubleshooting

| Symptom | Fix |
|---|---|
| Login shows a network/CORS error | `Cors__Origins__0` must equal the Vercel URL exactly (https, no trailing `/`). |
| First request hangs ~1 min | The free API was asleep — normal. |
| "Voice service unavailable" | It's waking up; retry after a minute. Check `VoiceService__BaseUrl` is the voice service's real URL. |
| Voice service logs `401 Missing or invalid X-Api-Key` | `VoiceService__ApiKey` on the API must equal `API_KEY` on the voice service (the Blueprint links them). |
| `413` / "too large" on upload | Free limits: photo 5 MB, audio 10 MB, video 25 MB. |
| API fails at start with a database error | Re-copy the Neon string (`sslmode=require` must be in it). |
