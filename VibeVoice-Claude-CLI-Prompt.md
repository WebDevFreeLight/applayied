# Prompt for Claude Code (CLI) — VibeVoice Notes & Memories

> Paste everything below the line into Claude Code, run from the root of your git repo
> (or an empty folder — Claude will `git init` if needed).

---

You are building a new full-stack project called **VibeVoice** in this repository. Work step by step, commit as you go, and do not stop until the app builds and runs end-to-end.

## 0. Git setup (do this first)
1. If this folder is not a git repo, run `git init` and make an initial commit.
2. Create and switch to a new branch: `git checkout -b feature/vibevoice-notes-memories`.
3. Add a proper `.gitignore` for .NET, Node/React, Python, and the local storage folders (`/storage`, `*.db`, `models/`, `.env`).
4. Commit after each major step below with clear conventional-commit messages (`feat:`, `chore:`, `fix:`). Never commit secrets, model weights, databases, or uploaded files.

## 1. What the app does
A personal app for **Notes** and **Memories**, each supporting **text and audio** content, plus **image attachments**:
- **Notes** — quick, working notes: title, rich text body, tags, pinned flag, color.
- **Memories** — personal moments to keep: title, story text, date of the memory, location (optional), mood/emotion tag, people tags, cover image.
- Both can have:
  - **Text** content.
  - **Recorded audio** (record in the browser with the microphone) or **uploaded audio** files.
  - **Generated speech (TTS)** from the text using **Microsoft VibeVoice**.
  - Optional **transcription** of recorded audio into text (speech-to-text).
  - **Multiple images** (upload, paste from clipboard, drag & drop) with thumbnails.
- Full CRUD, search (title/body/tags), filter by type/tag/date, sort, pin, archive, soft delete with a Trash view and restore.
- **Autosave** while editing (debounced ~1.5s) plus an explicit Save button, with a "Saved / Saving… / Unsaved changes" indicator.

## 2. Architecture
```
/VibeVoice
  /backend
    /VibeVoice.Api            ASP.NET Core Web API (.NET 10 LTS; fall back to the newest installed SDK)
    /VibeVoice.Application    services, DTOs, validation (FluentValidation)
    /VibeVoice.Domain         entities, enums
    /VibeVoice.Infrastructure SqlSugar data access, file storage, VibeVoice HTTP client
    /VibeVoice.Tests          xUnit tests
  /frontend                   React + TypeScript + Vite
  /voice-service              Python FastAPI wrapper around VibeVoice (TTS + optional ASR)
  /storage                    local files (gitignored): audio/, images/, tts/
  docker-compose.yml
  README.md
```
Clean architecture, dependency injection everywhere, async all the way, `CancellationToken` on every endpoint.

## 3. Database — SqlSugar, any database
- Use **SqlSugarCore** as the ORM (no EF Core).
- The DB provider must be **configurable** via `appsettings.json` so the app runs on any SqlSugar-supported database:
  ```json
  "Database": {
    "Provider": "Sqlite",            // Sqlite | PostgreSQL | SqlServer | MySql | Oracle
    "ConnectionString": "Data Source=storage/vibevoice.db"
  }
  ```
- **Default: SQLite + local files** (zero setup — must run on a fresh machine with no DB installed).
- **Second supported option: PostgreSQL + local files** — include a ready connection-string example and a `postgres` service in docker-compose.
- Map `Provider` → `SqlSugar.DbType` in one place (`DbProviderFactory`). Register `ISqlSugarClient` as scoped (or `SqlSugarScope` singleton) with `IsAutoCloseConnection = true`.
- Use **CodeFirst** (`db.CodeFirst.InitTables(...)`) on startup to create/update tables, and seed a few sample notes and memories in Development.
- Use only provider-neutral SqlSugar features (no raw SQL tied to one engine). Use `[SugarColumn]` attributes for lengths, nullability, and indexes.
- Repository pattern on top of SqlSugar with a generic `IRepository<T>` + specific repositories where needed. Use SqlSugar transactions (`db.Ado.BeginTran`/`UseTranAsync`) when saving an item with its attachments.
- Add a short README section showing how to switch to PostgreSQL / SQL Server.

### Entities (suggested)
- `Note` — Id (Guid), Title, Body (text, stores rich-text HTML or Markdown), Color, IsPinned, IsArchived, IsDeleted, CreatedAt, UpdatedAt.
- `Memory` — Id, Title, Story, MemoryDate, Location, Mood, CoverImageId, IsPinned, IsArchived, IsDeleted, CreatedAt, UpdatedAt.
- `Tag` — Id, Name (unique); join tables `NoteTag`, `MemoryTag`.
- `Person` (for memories) — Id, Name; join `MemoryPerson`.
- `AudioClip` — Id, OwnerType (Note|Memory), OwnerId, Source (Recorded|Uploaded|Tts), FilePath, MimeType, DurationSeconds, SizeBytes, Transcript (nullable), VoiceName (for TTS), CreatedAt.
- `ImageAttachment` — Id, OwnerType, OwnerId, FilePath, ThumbnailPath, MimeType, Width, Height, SizeBytes, Caption, SortOrder, CreatedAt.

## 4. File storage
- `IFileStorage` abstraction with a `LocalFileStorage` implementation saving under `storage/` (path configurable). Design the interface so S3/Azure Blob can be added later.
- Files saved as `{type}/{yyyy}/{MM}/{guid}.{ext}`; only relative paths stored in the DB.
- Validate uploads: allowed MIME types (images: jpeg/png/webp/gif; audio: webm/ogg/wav/mp3/m4a), max sizes from config, check file signatures not just extensions.
- Generate image thumbnails with **SixLabors.ImageSharp** (also strip EXIF location data).
- Serve files through API endpoints with range-request support (`enableRangeProcessing: true`) so audio seeking works.

## 5. Backend API (ASP.NET Core)
- Minimal APIs or controllers — your choice, but consistent. Swagger/OpenAPI enabled in Development.
- Endpoints (both `/api/notes` and `/api/memories` follow the same shape):
  - `GET /` (paging, search, tag filter, date range, sort, includeArchived)
  - `GET /{id}`, `POST /`, `PUT /{id}`, `PATCH /{id}/pin`, `PATCH /{id}/archive`, `DELETE /{id}` (soft), `POST /{id}/restore`, `DELETE /{id}/permanent`
  - `POST /{id}/images` (multipart, multiple files), `DELETE /{id}/images/{imageId}`, `PUT /{id}/images/order`
  - `POST /{id}/audio` (multipart — recorded or uploaded), `DELETE /{id}/audio/{audioId}`
  - `POST /{id}/tts` → body `{ voice, text? }` (defaults to the item's text) → calls voice-service, saves the result as an `AudioClip` with Source = Tts
  - `POST /{id}/audio/{audioId}/transcribe` → calls voice-service ASR, stores the transcript, optional flag to append it to the body
- `GET /api/files/{**path}` to stream stored files.
- `GET /api/tags`, `GET /api/voices` (proxied from voice-service), `GET /api/health` (checks DB + voice-service).
- Global exception handling with ProblemDetails, FluentValidation for DTOs, Serilog logging, CORS for the Vite dev origin, request size limits configured for audio uploads.
- `VoiceServiceClient` via typed `HttpClient` + Polly retry/timeout. If the voice service is down, the API must still work for everything else and return a clear 503 for TTS/ASR only.

## 6. Voice service (Python, FastAPI) — Microsoft VibeVoice
- Wrap **Microsoft VibeVoice** for TTS. Default to **`microsoft/VibeVoice-Realtime-0.5B`** (smallest, low latency, single speaker, uses pre-computed voice presets). Make the model path configurable (`VIBEVOICE_MODEL_PATH`) so `VibeVoice-1.5B` can be used for long-form / multi-speaker.
- Note: Microsoft removed the long-form TTS inference code from its official repo in Sept 2025; the Realtime-0.5B model and ASR remain available. If you need 1.5B inference code, use the **vibevoice-community/VibeVoice** fork. Check the current READMEs before writing the integration code instead of guessing APIs.
- Endpoints: `GET /health`, `GET /voices`, `POST /tts` (`{ text, voice, format: "wav"|"mp3" }` → audio bytes), `POST /transcribe` (audio file → `{ text, segments }`).
- ASR: make it **optional and pluggable** (`ASR_ENGINE=none|vibevoice|whisper`). VibeVoice-ASR is a large 7B model, so default to `none` or a small Whisper model (faster-whisper) and document how to enable VibeVoice-ASR on a GPU machine.
- Device auto-detect: CUDA → MPS → CPU (warn that CPU is slow).
- Long text: split into sentence chunks, synthesize, and concatenate.
- **Responsible use**: no voice cloning of real people without consent. Only ship the official voice presets; if a custom voice upload feature is added, require a consent checkbox and label generated audio as AI-generated in the UI.
- Include `requirements.txt`, `Dockerfile`, `.env.example`, and a `README` with model download steps (`huggingface-cli download …`).

## 7. Frontend (React + TypeScript + Vite)
- Stack: React 18+, TypeScript strict, Vite, React Router, **TanStack Query** for server state, React Hook Form + Zod, Tailwind CSS (or MUI — pick one and be consistent), a rich-text editor (TipTap), `lucide-react` icons.
- Pages: Dashboard (recent notes + memories), Notes list (grid/list toggle, pinned on top), Note editor, Memories timeline (grouped by month/year, cover images), Memory editor, Search, Trash, Settings (voice choice, theme).
- Editor features:
  - Title + rich text, tags input with autocomplete.
  - **Audio recorder** component using `MediaRecorder` (start/pause/stop, live timer, level meter, preview before saving, discard).
  - Audio upload + **audio player list** with waveform (wavesurfer.js), duration, delete, download, "Transcribe" button, and source badge (Recorded / Uploaded / AI voice).
  - **"Read aloud with VibeVoice"** button → voice picker → generates TTS, shows progress, then adds it to the audio list.
  - **Image gallery**: drag & drop, paste from clipboard, multi-upload with progress, reorder, captions, lightbox viewer, set as memory cover.
  - **Autosave** + manual Save (Ctrl/Cmd+S), dirty-state guard when leaving the page.
- Light/dark theme, responsive down to mobile, accessible (labels, keyboard navigation, focus states), loading skeletons, empty states, toast notifications.
- Support **Arabic and English** text in notes (set `dir="auto"` on text fields so RTL text displays correctly).
- API base URL from `VITE_API_URL`; typed API client generated by hand or from the OpenAPI spec.

## 8. Run & DevOps
- `docker-compose.yml` with services: `api`, `frontend`, `voice-service`, and an optional `postgres` (profile `postgres`). Default compose run uses SQLite.
- Root scripts: `dev.ps1` and `dev.sh` to start api + frontend + voice-service locally.
- `.env.example` files for each part.
- README: prerequisites, quick start (SQLite), switching to PostgreSQL, enabling GPU for VibeVoice, project structure, API overview, screenshots section placeholder.

## 9. Quality
- xUnit tests for services and repositories (run against SQLite in-memory/temp file), plus a couple of API integration tests with `WebApplicationFactory`.
- Frontend: Vitest + React Testing Library for the recorder, autosave hook, and image uploader.
- `dotnet build`, `dotnet test`, `npm run build`, `npm run lint` must all pass before you finish.

## 10. Order of work
1. Git branch + scaffolding + .gitignore → commit
2. Domain + SqlSugar setup (SQLite default, provider switch) + CodeFirst + seed → commit
3. File storage + Notes API → commit
4. Memories API + tags/people → commit
5. Voice service (TTS first, ASR optional) + API integration → commit
6. Frontend: layout, routing, notes list/editor → commit
7. Frontend: audio recorder, player, TTS, transcription → commit
8. Frontend: images, memories timeline, search, trash, settings → commit
9. Tests, docker-compose, README → commit
10. Final check: run everything, fix issues, then print a summary of what was built and how to run it.

Ask me before any destructive git action (force push, reset, deleting branches). Do not push to a remote unless I say so.
