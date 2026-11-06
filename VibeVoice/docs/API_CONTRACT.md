# VibeVoice — API contract

Single source of truth shared by `backend/` (ASP.NET Core), `frontend/` (React) and
`voice-service/` (FastAPI). JSON is **camelCase** for the .NET API and **snake_case** for the
voice service. All .NET endpoints are under `/api`. Enums are serialized as **strings**.
Dates are ISO-8601 UTC strings (`2026-11-07T10:20:30Z`); `memoryDate` is a date-only string (`2026-05-14`).

Errors use RFC 7807 ProblemDetails: `{ type, title, status, detail, errors?: { field: [msg] } }`.
Validation errors → 400 with `errors`. Missing / not-owned → 404. Voice service down → 503 with
`title: "Voice service unavailable"`.

## Auth & users

JWT bearer. Header `Authorization: Bearer <token>`. For `<audio>`/`<img>` tags, file URLs accept
`?access_token=<token>` instead (only on `/api/files/*`).

Roles: `Admin`, `User`. The **first registered user becomes Admin**. Dev seed user:
`admin@vibevoice.local` / `Admin123!` (Development only).

| Method | Path | Body | Returns |
|---|---|---|---|
| POST | `/api/auth/register` | `{ email, password, displayName, preferredLanguage?: "en"\|"ar" }` | `AuthResponse` |
| POST | `/api/auth/login` | `{ email, password }` | `AuthResponse` (401 on bad creds) |
| GET | `/api/auth/me` | – | `UserDto` |
| PUT | `/api/users/me` | `{ displayName, preferredLanguage, preferredVoice?, theme: "light"\|"dark"\|"system" }` | `UserDto` |
| PUT | `/api/users/me/password` | `{ currentPassword, newPassword }` | 204 |
| GET | `/api/users` *(Admin)* | – | `UserDto[]` |
| PUT | `/api/users/{id}/role` *(Admin)* | `{ role: "Admin"\|"User" }` | `UserDto` |
| PUT | `/api/users/{id}/active` *(Admin)* | `{ isActive: bool }` | `UserDto` |

```ts
AuthResponse = { token: string; expiresAt: string; user: UserDto }
UserDto = { id: string; email: string; displayName: string; role: "Admin"|"User";
  preferredLanguage: "en"|"ar"; preferredVoice: string|null; theme: "light"|"dark"|"system";
  isActive: boolean; createdAt: string }
```

Password rules: min 8 chars, at least one letter and one digit.

## Shared types

```ts
PagedResult<T> = { items: T[]; page: number; pageSize: number; total: number }

ImageDto = { id: string; ownerType: "Note"|"Memory"|"Library"; ownerId: string;
  filePath: string; thumbnailPath: string; mimeType: string; width: number; height: number;
  sizeBytes: number; caption: string|null; sortOrder: number; createdAt: string }

AudioDto = { id: string; ownerType: "Note"|"Memory"|"Studio"; ownerId: string;
  source: "Recorded"|"Uploaded"|"Tts"; filePath: string; mimeType: string;
  durationSeconds: number|null; sizeBytes: number; transcript: string|null;
  transcriptLanguage: string|null; voiceName: string|null; ttsEngine: string|null;
  text: string|null;            // the text a TTS clip was generated from
  isAiGenerated: boolean;       // true when source == "Tts" → UI must show an "AI-generated" label
  createdAt: string }
```

`filePath` / `thumbnailPath` are **relative** storage paths (`images/2026/11/<guid>.jpg`).
Build a URL with `GET /api/files/{path}?access_token=<jwt>`. Supports HTTP range requests.

## Notes — `/api/notes`

```ts
NoteDto = { id; title; body /* HTML from TipTap */; color: string|null; isPinned; isArchived;
  isDeleted; tags: string[]; images: ImageDto[]; audio: AudioDto[]; createdAt; updatedAt; deletedAt: string|null }
NoteListItemDto = { id; title; excerpt /* plain text, ≤ 200 chars */; color; isPinned; isArchived;
  isDeleted; tags: string[]; imageCount: number; audioCount: number;
  thumbnailPath: string|null /* first image thumb */; createdAt; updatedAt }
SaveNoteRequest = { title: string /* ≤ 200 */; body: string; color?: string|null; tags: string[] }
```

| Method | Path | Notes |
|---|---|---|
| GET | `/api/notes` | query: `page=1, pageSize=20 (max 100), search, tag, from, to (ISO dates on updatedAt), sort=updated_desc\|updated_asc\|created_desc\|created_asc\|title_asc, includeArchived=false, archivedOnly=false, deleted=false (true = Trash), hasAudio, hasImages` → `PagedResult<NoteListItemDto>`; pinned items always first |
| GET | `/api/notes/{id}` | `NoteDto` |
| POST | `/api/notes` | `SaveNoteRequest` → 201 `NoteDto` |
| PUT | `/api/notes/{id}` | `SaveNoteRequest` → `NoteDto` (autosave target) |
| PATCH | `/api/notes/{id}/pin` | `{ value: bool }` → `NoteDto` |
| PATCH | `/api/notes/{id}/archive` | `{ value: bool }` → `NoteDto` |
| DELETE | `/api/notes/{id}` | soft delete → 204 |
| POST | `/api/notes/{id}/restore` | → `NoteDto` |
| DELETE | `/api/notes/{id}/permanent` | deletes row, attachments and files → 204 |
| POST | `/api/notes/{id}/images` | multipart, field `files` (1..n) → `ImageDto[]` |
| PATCH | `/api/notes/{id}/images/{imageId}` | `{ caption }` → `ImageDto` |
| DELETE | `/api/notes/{id}/images/{imageId}` | 204 |
| PUT | `/api/notes/{id}/images/order` | `{ imageIds: string[] }` → `ImageDto[]` |
| POST | `/api/notes/{id}/audio` | multipart: `file`, `source` = `Recorded`\|`Uploaded`, optional `durationSeconds` → `AudioDto` |
| DELETE | `/api/notes/{id}/audio/{audioId}` | 204 |
| POST | `/api/notes/{id}/tts` | `{ voice?: string, text?: string, language?: "en"\|"ar"\|... }` (text defaults to title + plain body) → `AudioDto` (503 if voice service down) |
| POST | `/api/notes/{id}/audio/{audioId}/transcribe` | `{ appendToBody?: bool, language?: string }` → `AudioDto` (with transcript) |

## Memories — `/api/memories`

Same endpoint shape as notes (replace `notes` with `memories`), plus:

```ts
MemoryDto = { id; title; story /* HTML */; memoryDate: string /* yyyy-MM-dd */; location: string|null;
  mood: Mood|null; coverImageId: string|null; isPinned; isArchived; isDeleted; tags: string[];
  people: string[]; images: ImageDto[]; audio: AudioDto[]; createdAt; updatedAt; deletedAt }
MemoryListItemDto = { id; title; excerpt; memoryDate; location; mood; isPinned; isArchived; isDeleted;
  tags; people; imageCount; audioCount; coverThumbnailPath: string|null; createdAt; updatedAt }
SaveMemoryRequest = { title; story; memoryDate; location?; mood?; tags: string[]; people: string[] }
Mood = "Happy"|"Grateful"|"Calm"|"Excited"|"Nostalgic"|"Proud"|"Sad"|"Loved"
```

- List query adds `mood`, `person`; `from`/`to` filter on `memoryDate`; default sort `date_desc` (`date_desc|date_asc|updated_desc|title_asc`).
- `PUT /api/memories/{id}/cover` `{ imageId: string|null }` → `MemoryDto`.
- If a memory has no cover, the first image is used as `coverThumbnailPath`.

## Library, audio, voice studio

| Method | Path | Notes |
|---|---|---|
| GET | `/api/images` | query `page, pageSize, ownerType?` → `PagedResult<ImageWithOwnerDto>` (`ImageDto & { ownerTitle: string\|null }`) |
| POST | `/api/images` | multipart `files` → `ImageDto[]` stored in the Library |
| POST | `/api/images/{id}/attach` | `{ ownerType: "Note"\|"Memory", ownerId }` → `ImageDto` (moves it) |
| DELETE | `/api/images/{id}` | 204 |
| GET | `/api/audio` | query `page, pageSize, source?, ownerType?, search?` → `PagedResult<AudioWithOwnerDto>` (`AudioDto & { ownerTitle: string\|null }`) |
| GET | `/api/voices` | `VoiceDto[]` (proxied; `[]` + header `X-Voice-Service: down` if unavailable) |
| GET | `/api/voice/status` | `{ available: bool; ttsEngine: string\|null; asrEngine: string\|null; device: string\|null; message: string\|null }` |
| POST | `/api/voice/tts` | `{ text, voice?, language? }` → `AudioDto` (ownerType `Studio`) |
| POST | `/api/voice/transcribe` | multipart `file`, `language?`, `save?=true` → `{ text, language, segments: Segment[], audio: AudioDto\|null }` |
| GET | `/api/voice/history` | `PagedResult<AudioDto>` of Studio clips |
| DELETE | `/api/voice/history/{id}` | 204 |

```ts
VoiceDto = { id: string; name: string; language: string /* "en","ar",... */; gender: "male"|"female"|null;
  engine: "vibevoice"|"edge"|"dev"; isFallback: boolean }
Segment = { start: number; end: number; text: string; speaker: string|null }
```

## Search, tags, dashboard, health

- `GET /api/search?q=&types=notes,memories,audio&page=1&pageSize=20` →
  `PagedResult<SearchResultDto>` where
  `SearchResultDto = { kind: "note"|"memory"|"audio"; id; ownerId: string|null; ownerKind: "note"|"memory"|"studio"|null; title; snippet; matchedIn: "title"|"body"|"tag"|"transcript"|"person"|"location"; date: string }`.
  Search is case-insensitive and **Arabic-normalized** (diacritics/tatweel removed, أ/إ/آ→ا, ى→ي, ة→ه).
- `GET /api/tags` → `{ name: string; count: number }[]`
- `GET /api/people` → `{ name: string; count: number }[]`
- `GET /api/dashboard` →
  ```ts
  { stats: { notes; memories; audioClips; images; aiVoiceClips; transcripts; pinned; trash };
    recentNotes: NoteListItemDto[]; recentMemories: MemoryListItemDto[];
    onThisDay: MemoryListItemDto[];   // same month/day, earlier years
    activity: { kind: "note"|"memory"|"audio"|"image"; action: "created"|"updated"; id; ownerId: string|null; title; at }[] }
  ```
- `GET /api/health` (anonymous) → `{ status: "Healthy"|"Degraded"; database: "up"|"down"; voiceService: "up"|"down" }`
- `GET /api/files/{**path}` → file stream (owner-checked, range enabled).

## Video + transcripts (v2)

Audio clips are now **media clips**: the same `AudioDto` / endpoints carry audio **and video**.

```ts
AudioDto += {
  mediaKind: "Audio" | "Video";          // render <video> for Video, waveform/audio player for Audio
  segments: Segment[];                   // timestamped transcript ([] until transcribed)
}
```

- `POST /api/{notes|memories}/{id}/audio` and `POST /api/voice/transcribe` accept **video** files
  (mp4, mov, webm, mkv, avi) as well as audio. Extra optional form fields on the notes/memories upload:
  `transcribe=true` (transcribe right after upload) and `language` (`en`, `ar`, …; omit = auto-detect).
  If auto-transcription fails the clip is still saved and returned (200) with header
  `X-Transcription-Error: <message>` — show it as a warning toast.
- `GET /api/audio` adds query `kind=Audio|Video`.
- `GET /api/audio/{id}/transcript?format=txt|srt|vtt` → downloadable transcript file
  (`Content-Disposition: attachment`). 404 if not transcribed yet. Accepts `?access_token=` like `/api/files`.
- Limits: audio ≤ 50 MB, **video ≤ 500 MB** (configurable `Uploads:MaxVideoBytes`).

## Image → text / OCR (v3)

Pictures can be turned into text (Arabic, English and mixed) with RapidOCR (PP-OCRv5) in the voice service.

```ts
ImageDto += { ocrText: string | null; ocrLanguage: string | null }   // null until extracted
OcrResultDto = { text: string; language: string | null; lines: OcrLine[]; image: ImageDto | null }
OcrLine = { text: string; confidence: number }                        // top-to-bottom reading order
```

- Image uploads (`POST /api/{notes|memories}/{id}/images`, `POST /api/images`) accept optional form fields
  `ocr=true` and `language` (`ar`, `en`; omit = auto, which reads Arabic + English). Extraction runs per image
  after upload; failures never fail the upload — the header `X-Ocr-Error: <message>` is set instead.
- `POST /api/{notes|memories}/{id}/images/{imageId}/ocr` body `{ appendToBody?: bool, language?: string }`
  → `ImageDto` (with `ocrText`). With `appendToBody` the text is appended to the note body / memory story.
- `POST /api/images/{id}/ocr` body `{ language?: string }` → `ImageDto` (any of the user's images, incl. library).
- `POST /api/voice/ocr` multipart `file` (jpeg/png/webp/gif ≤ 10 MB), `language?`, `save?=true`
  → `OcrResultDto`; with `save=true` the picture is kept in the Images library and returned as `image`.
- Search: `GET /api/search` gains kind `"image"` (`types=…,images`), `matchedIn: "imageText"`, `ownerId/ownerKind`
  of the note/memory the picture belongs to (`"library"` for library images).
- `GET /api/voice/status` adds `ocrEngine: string | null`.
- Voice service: `POST /ocr` multipart `file`, `language?` → `{ text, language, lines: [{ text, confidence, box }], engine }`;
  `/health` adds `ocr_engine`. `OCR_ENGINE=rapidocr|none`.

## Upload limits (configurable)

Images: jpeg/png/webp/gif, ≤ 10 MB each. Audio: webm/ogg/wav/mp3/m4a(mp4), ≤ 50 MB.
Validation checks magic bytes, not just extension.

---

## Voice service (FastAPI, default `http://localhost:8001`) — snake_case

| Method | Path | Body | Returns |
|---|---|---|---|
| GET | `/health` | – | `{ status: "ok", tts_engine, asr_engine, device, model_loaded: bool, arabic_tts_engine }` |
| GET | `/voices` | – | `[{ id, name, language, gender, engine, is_fallback }]` |
| POST | `/tts` | `{ text, voice?, language?, format?: "wav"\|"mp3" }` | audio bytes; headers `X-Voice`, `X-Engine`, `X-Language`, `X-Duration-Seconds`, `X-AI-Generated: true` |
| POST | `/transcribe` | multipart `file`, `language?` | `{ text, language, segments: [{ start, end, text, speaker }], engine }` |

Engines: `TTS_ENGINE=vibevoice|edge|dev` (dev = synthetic tone, for tests/CI),
`ARABIC_TTS_ENGINE=edge|dev|vibevoice` (VibeVoice-Realtime ships no Arabic preset),
`ASR_ENGINE=none|whisper|vibevoice`. `language` omitted → auto-detected from the text script.
