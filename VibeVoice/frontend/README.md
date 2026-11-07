# VibeVoice — frontend

Bilingual (English / العربية, full RTL) React app for **Notes** and **Memories** with text, audio
and images, using Microsoft VibeVoice (through the .NET API) for AI speech and transcription.

## Stack

React 19 · TypeScript (strict) · Vite · React Router (data router) · TanStack Query ·
React Hook Form + Zod · Tailwind CSS v4 · TipTap · wavesurfer.js · react-i18next · sonner ·
date-fns · lucide-react. Tests: Vitest + React Testing Library + jsdom.

## Run

```bash
cp .env.example .env          # VITE_API_URL=http://localhost:5080
npm install
npm run dev                   # http://localhost:5173
```

The API (`../backend`) must allow the `http://localhost:5173` origin. In Development the API
seeds `admin@vibevoice.local` / `Admin123!`.

| Script | What it does |
|---|---|
| `npm run dev` | Vite dev server on port 5173 |
| `npm run build` | Type-check (`tsc -b`) and build to `dist/` |
| `npm run lint` | ESLint (flat config, typescript-eslint, react-hooks, react-refresh) |
| `npm run test -- --run` | Vitest once (omit `--run` for watch mode) |

## Docker

```bash
docker build -t vibevoice-frontend --build-arg VITE_API_URL=http://localhost:5080 .
docker run -p 5173:80 vibevoice-frontend
```

`VITE_API_URL` is baked in at build time. nginx serves `dist/` with an SPA fallback (`nginx.conf`).

## Structure

```
src/
  api/          client.ts (fetch wrapper, ApiError, XHR upload with progress, fileUrl),
                types.ts (mirrors docs/API_CONTRACT.md), keys.ts (query-key factory), per-module APIs
  auth/         AuthProvider (JWT in localStorage, 401 → sign out), route guards
  components/   layout (sidebar / bottom bar / ⌘K quick search), editor (TipTap, autosave chrome),
                audio (AudioRecorder, AudioPlayer, AudioList, ReadAloudButton), images
                (ImageUploader, ImageGallery, Lightbox), TagInput, ui primitives
  hooks/        useAutosave, useUnsavedGuard, misc (debounce, hotkeys, …)
  i18n/         en.json, ar.json, setup (sets <html lang dir>)
  pages/        one file per route
```

Notes on behaviour:

- **New note/memory** first POSTs an "Untitled" item (attachments need an id), then opens the editor.
- **Autosave** is debounced (1.5 s), never overlaps saves, and shows *Saved / Saving… / Unsaved changes*;
  `Ctrl/⌘+S` saves immediately; leaving with unsaved changes asks for confirmation.
- `<img>`/`<audio>` load files via `/api/files/{path}?access_token=…` (see `fileUrl`).
- AI (TTS) clips always show an **AI-generated** label. Arabic voices are marked as a fallback engine.
