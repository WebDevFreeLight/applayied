# VibeVoice — Notes & Memories (text · audio · images · AI voice)

A full-stack, bilingual (**English / العربية**) personal app for **Notes** and **Memories** that supports
text and audio side by side: rich-text notes, voice recordings, uploaded audio, image attachments,
searchable transcripts, and AI speech generated with **[Microsoft VibeVoice](https://github.com/microsoft/VibeVoice)**.

| Module | What it does |
|---|---|
| **Dashboard** | Stats, recent notes & memories, "On this day", recent activity, voice-service status |
| **Memories** | Story, date, location, mood, people, tags, cover image; timeline grouped by month/year |
| **Notes** | Rich text (TipTap), tags, colors, pin, archive, grid/list, autosave (1.5 s) + Ctrl/Cmd+S |
| **Audio Notes** | Record in the browser (MediaRecorder + level meter), upload, waveform playback, transcription |
| **VibeVoice Studio** | Text → speech (VibeVoice voices, Arabic fallback voices) and speech → text |
| **Images** | Drag & drop / paste / multi-upload, thumbnails, lightbox, reorder, captions, attach to notes/memories |
| **Search** | One box over notes, memories and audio transcripts — Arabic-normalized (أ/ا, ة/ه, ى/ي, harakat) |
| **Users** | JWT auth, profile & preferences, Admin role (manage roles / deactivate users) |
| **Trash** | Soft delete, restore, delete permanently (files removed too) |

---

## Architecture

```
            ┌──────────────────────┐   REST/JSON + JWT    ┌───────────────────────────┐   HTTP (snake_case)   ┌──────────────────────────┐
 Browser ──►│ frontend (React/Vite)│ ───────────────────► │ backend (ASP.NET Core 10) │ ────────────────────► │ voice-service (FastAPI)  │
            │ :5173 dev / :8080    │ ◄─── audio/images ── │ :5080                     │ ◄──── wav / text ──── │ :8001                    │
            └──────────────────────┘    (range requests)  │  SqlSugar → SQLite/PG/... │                       │  VibeVoice-Realtime-0.5B │
                                                          │  storage/ (local files)   │                       │  edge-tts (Arabic)       │
                                                          └───────────────────────────┘                       │  faster-whisper / VV-ASR │
                                                                                                              └──────────────────────────┘
```

```
VibeVoice/
  backend/
    VibeVoice.Api              Minimal APIs, JWT, ProblemDetails, Swagger, Serilog
    VibeVoice.Application      Services, DTOs, FluentValidation, abstractions
    VibeVoice.Domain           Entities + enums (SqlSugar attributes)
    VibeVoice.Infrastructure   SqlSugar repositories, CodeFirst, local storage, ImageSharp, voice HTTP client
    VibeVoice.Tests            xUnit: unit, SQLite service tests, WebApplicationFactory integration tests
  frontend/                    React 19 + TypeScript + Vite + Tailwind v4 + TanStack Query + i18next (en/ar, RTL)
  voice-service/               Python FastAPI wrapper around Microsoft VibeVoice (TTS + optional ASR)
  docs/API_CONTRACT.md         The contract all three parts implement
  docker-compose.yml           api + frontend + voice-service (+ postgres profile)
```

**Design notes**

- **Clean architecture**: the Application layer owns the interfaces (`IRepository<T>`, `IFileStorage`,
  `IVoiceServiceClient`, …); Infrastructure implements them. Async all the way, `CancellationToken` everywhere.
- **Any database**: SqlSugar with provider-neutral queries only. Search uses a pre-normalized `SearchText`
  column + `LIKE`, so it behaves identically on SQLite, PostgreSQL, SQL Server, MySQL and Oracle.
- **Files**: stored as `{type}/{yyyy}/{MM}/{guid}.{ext}`, only relative paths in the DB, served through an
  owner-checked endpoint with HTTP range support (audio seeking). Uploads are validated by **magic bytes**;
  images are auto-oriented and **EXIF/GPS stripped**; thumbnails are WebP.
- **Resilience**: the voice client uses retry + timeout + circuit breaker. If the voice service is down, only
  TTS/transcription return **503** — notes, memories, images and recordings keep working.

---

## Important: what VibeVoice provides today

Microsoft's repository currently ships **VibeVoice-Realtime-0.5B** (TTS) and **VibeVoice-ASR** (speech-to-text).
The original long-form **VibeVoice-TTS** inference code was **removed in September 2025**, so this app does not
depend on it (the community fork `vibevoice-community/VibeVoice` is mentioned in `voice-service/README.md`).

- **TTS** → `microsoft/VibeVoice-Realtime-0.5B` with the official voice presets
  (`en-Carter_man`, `en-Emma_woman`, … de/fr/it/jp/kr/nl/pl/pt/sp).
- **Arabic TTS** → VibeVoice-Realtime has **no Arabic voice preset**, so Arabic text is routed to a clearly
  labeled fallback engine (Microsoft Edge neural voices via `edge-tts`, e.g. `ar-SA-HamedNeural`).
  Configure with `ARABIC_TTS_ENGINE`.
- **ASR** → VibeVoice-ASR is a 7B model (needs a large GPU), so the default is **faster-whisper** (Arabic + English).
  Set `ASR_ENGINE=vibevoice` on a big GPU.
- Without the VibeVoice package installed, the service **falls back automatically** (edge-tts → dev tone) and
  `/api/voice/status` tells you which engine is active.

---

## Quick start (local, SQLite — zero setup)

Prerequisites: **.NET SDK 10**, **Node 20.19+**, **Python 3.11–3.13**, optional **ffmpeg** (mp3 output / webm decoding).

```powershell
cd VibeVoice
./dev.ps1            # Windows: opens voice-service, api and frontend
# or
./dev.sh             # macOS / Linux
```

Open **http://localhost:5173** and sign in with the demo account **`admin@vibevoice.local` / `Admin123!`**
(seeded in Development). Swagger: http://localhost:5080/swagger.

Manual start, if you prefer:

```bash
# 1) voice service
cd voice-service && python -m venv .venv && .venv/Scripts/pip install -r requirements.txt   # (bin/ on macOS/Linux)
.venv/Scripts/python -m uvicorn app.main:app --port 8001
# 2) API
cd backend && dotnet run --project VibeVoice.Api --launch-profile http
# 3) frontend
cd frontend && npm install && npm run dev
```

### Enable real VibeVoice speech (GPU recommended)

```powershell
cd voice-service
./scripts/setup_vibevoice.ps1     # clones microsoft/VibeVoice, pip install -e .[streamingtts], copies voice presets
hf download microsoft/VibeVoice-Realtime-0.5B --local-dir models/VibeVoice-Realtime-0.5B   # optional pre-download
```

Restart the voice service with `TTS_ENGINE=vibevoice` (default). It detects CUDA → MPS → CPU (CPU works but is slow).
With Docker: `docker compose -f docker-compose.yml -f docker-compose.gpu.yml up --build`.

---

## Docker

```bash
cp .env.example .env          # set JWT_KEY
docker compose up --build     # app: http://localhost:8080   api: http://localhost:5080
```

## Switching the database

Set two values (appsettings, environment variables or `.env`):

| Provider | `Database__Provider` | `Database__ConnectionString` example |
|---|---|---|
| SQLite (default) | `Sqlite` | `Data Source=storage/vibevoice.db` |
| PostgreSQL | `PostgreSQL` | `Host=localhost;Port=5432;Database=vibevoice;Username=vibevoice;Password=vibevoice` |
| SQL Server | `SqlServer` | `Server=localhost;Database=VibeVoice;User Id=sa;Password=...;TrustServerCertificate=True` |
| MySQL | `MySql` | `Server=localhost;Database=vibevoice;Uid=root;Pwd=...` |

Tables are created/updated on startup with SqlSugar **CodeFirst**. With Docker:
`docker compose --profile postgres up` plus `DB_PROVIDER=PostgreSQL` and the `DB_CONNECTION` line from `.env.example`.

## Configuration (backend)

| Key | Default | Notes |
|---|---|---|
| `Jwt__Key` | *(dev key in appsettings.Development.json)* | **Required** outside Development, 32+ chars |
| `Database__Seed` | `true` in Development | Demo user + bilingual sample notes/memories |
| `Storage__RootPath` | `storage` | Uploaded audio/images/TTS files |
| `VoiceService__BaseUrl` | `http://localhost:8001` | Python service |
| `Uploads__MaxImageBytes` / `MaxAudioBytes` | 10 MB / 50 MB | Per file |
| `Cors__Origins__0` | `http://localhost:5173` | Frontend origin |

## API overview

Full contract: [`docs/API_CONTRACT.md`](docs/API_CONTRACT.md) · live docs at `/swagger`.

```
POST /api/auth/register | /api/auth/login        GET /api/auth/me        PUT /api/users/me[/password]
GET|POST /api/notes   GET|PUT /api/notes/{id}   PATCH …/pin|archive   DELETE …   POST …/restore   DELETE …/permanent
POST /api/notes/{id}/images | /audio | /tts       POST /api/notes/{id}/audio/{audioId}/transcribe
(same shape under /api/memories, plus PUT /api/memories/{id}/cover)
GET /api/search?q=   GET /api/dashboard   GET /api/tags   GET /api/people   GET /api/images   GET /api/audio
GET /api/voices   GET /api/voice/status   POST /api/voice/tts   POST /api/voice/transcribe   GET /api/voice/history
GET /api/files/{path}   GET /api/health
```

## Tests

```bash
cd backend && dotnet test                         # xUnit (unit + SQLite + WebApplicationFactory)
cd frontend && npm run lint && npm test -- --run && npm run build
cd voice-service && .venv/Scripts/python -m pytest -q
```

## Responsible use

- Only the **official** VibeVoice voice presets are shipped. Do not clone a real person's voice without their consent.
- Every generated clip is stored with `source = Tts` and shown with an **"AI-generated"** label in the UI;
  the voice service also sets `X-AI-Generated: true`.

## Screenshots

_Add screenshots of the Dashboard, Note editor, Memories timeline and VibeVoice Studio here._
