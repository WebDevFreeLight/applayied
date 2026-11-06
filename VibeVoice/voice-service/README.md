# VibeVoice voice service

FastAPI service that gives the VibeVoice Notes & Memories app its voice:

- **Text-to-speech** with [Microsoft VibeVoice](https://github.com/microsoft/VibeVoice)
  (`microsoft/VibeVoice-Realtime-0.5B`, 24 kHz), with Microsoft Edge neural voices (`edge-tts`) for
  Arabic and as a fallback, plus a synthetic `dev` engine for tests and CI.
- **Speech-to-text** with `faster-whisper` (default) or `microsoft/VibeVoice-ASR` (7B, large GPU).

Only the .NET API calls this service. The contract is in `../docs/API_CONTRACT.md` under "Voice service".

| Method | Path | Body | Returns |
|---|---|---|---|
| GET | `/health` | – | `{ status, tts_engine, asr_engine, device, model_loaded, arabic_tts_engine, ... }` |
| GET | `/voices` | – | `[{ id, name, language, gender, engine, is_fallback }]` |
| POST | `/tts` | `{ text, voice?, language?, format?: "wav"\|"mp3" }` | audio bytes + `X-Voice`, `X-Engine`, `X-Language`, `X-Duration-Seconds`, `X-AI-Generated: true` |
| POST | `/transcribe` | multipart `file`, `language?` | `{ text, language, segments: [{ start, end, text, speaker }], engine }` |

`/health` also returns `requested_tts_engine`, `requested_asr_engine`, `asr_device`, `asr_model_loaded`,
`ffmpeg` and `notes` (the reasons for any fallback). Interactive docs: http://localhost:8001/docs

## Quick start (no GPU, no VibeVoice)

```powershell
cd voice-service
python -m venv .venv
.venv\Scripts\python -m pip install -r requirements.txt     # Linux/macOS: .venv/bin/python
copy .env.example .env                                     # optional
.venv\Scripts\python -m uvicorn app.main:app --host 0.0.0.0 --port 8001 --no-access-log
```

If VibeVoice isn't installed, `TTS_ENGINE=vibevoice` (the default) falls back to `edge` and logs
the reason, and `/health` reports `"tts_engine": "edge"`. Edge voices need internet access. To run
with no network and no models (CI, offline demos), use synthetic tones:

```powershell
$env:TTS_ENGINE="dev"; $env:ARABIC_TTS_ENGINE="dev"; $env:ASR_ENGINE="none"
.venv\Scripts\python -m uvicorn app.main:app --port 8001
```

Install **ffmpeg** and keep it on `PATH`. It is needed for `format=mp3` and for decoding edge-tts output
and uploaded recordings. Without the ffmpeg CLI the service falls back to PyAV (installed with
faster-whisper) for decoding. If neither can decode, edge-tts MP3 is passed through as `audio/mpeg`.

### Tests

```powershell
.venv\Scripts\python -m pytest -q
```

The tests use `TTS_ENGINE=dev ARABIC_TTS_ENGINE=dev ASR_ENGINE=none`, so they need no network or models.

## Enabling Microsoft VibeVoice TTS

1. Install the upstream package and copy the official voice presets (clones into the gitignored
   `third_party/VibeVoice` and copies `demo/voices/streaming_model/*.pt` to `voices/streaming_model/`):

   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts\setup_vibevoice.ps1 -Cuda cu124 -DownloadModel
   ```
   ```bash
   bash scripts/setup_vibevoice.sh --cuda cu124 --download-model
   ```
   Leave out `-Cuda`/`--cuda` to install CPU or MPS torch. The scripts run
   `pip install -e third_party/VibeVoice[streamingtts]`. That extra pins `transformers==4.51.3`.
   `requirements-vibevoice.txt` lists the upstream dependencies for reference.

2. Model weights. The model downloads from Hugging Face on first use. To pre-download it:

   ```bash
   hf download microsoft/VibeVoice-Realtime-0.5B --local-dir models/VibeVoice-Realtime-0.5B
   # older huggingface_hub (< 1.0):
   huggingface-cli download microsoft/VibeVoice-Realtime-0.5B --local-dir models/VibeVoice-Realtime-0.5B
   ```
   Then set `VIBEVOICE_MODEL_PATH=models/VibeVoice-Realtime-0.5B`.

3. Start with `TTS_ENGINE=vibevoice`. The model loads on the first `/tts` request, or at startup
   in the background if `PRELOAD_MODELS=true`.

**Devices.** The service checks CUDA first (bf16, `flash_attention_2` with `sdpa` fallback), then
Apple MPS (fp32), then CPU (fp32, **slow**, and it logs a warning). Set `VIBEVOICE_DEVICE` to override.
For a GPU, install a CUDA build of torch *before* the upstream package
(`pip install torch --index-url https://download.pytorch.org/whl/cu124`). If you want flash-attention,
install `flash-attn` separately.

**Voices.** Voice ids are the preset file names, for example `en-Carter_man` ("Carter (en, male)"),
`de-Spk1_woman` ("Speaker 1 (de, female)"), and `in-Samuel_man` (Indian English). Language codes map
as follows: `sp`→`es`, `jp`→`ja`, `kr`→`ko`, `in`→`en`. Languages: en, de, fr, it, ja, ko, nl, pl, pt, es.
**There is no Arabic preset.**

**Long-form 1.5B model.** Microsoft removed long-form VibeVoice-TTS-1.5B inference from the official
repo in September 2025. Only the realtime 0.5B model and ASR remain. If you need 1.5B, the community
fork [`vibevoice-community/VibeVoice`](https://github.com/vibevoice-community/VibeVoice) keeps that
code. It is not integrated here.

## Why Arabic goes to edge-tts

VibeVoice-Realtime-0.5B ships pre-computed voice prompts only, and none of them is Arabic. Making a
new one would mean cloning a voice. So Arabic text, and any text whose letters are more than 30%
Arabic script, is routed to `ARABIC_TTS_ENGINE` (default `edge`). Its voices are `ar-SA-HamedNeural`,
`ar-SA-ZariyahNeural`, `ar-EG-SalmaNeural` and `ar-EG-ShakirNeural`, all marked `is_fallback: true`.
Edge voices are Microsoft's cloud neural voices, so Arabic TTS needs internet access. Set
`ARABIC_TTS_ENGINE=vibevoice` if you want VibeVoice to try Arabic with an English preset anyway.

**Routing rules**
- If `language` is omitted, it is detected from the script. Arabic letters above 30% → `ar`,
  otherwise `en`.
- If `voice` belongs to a known engine (a VibeVoice preset, any `xx-YY-…Neural` edge voice, or
  `dev-*`), that engine is used. Unknown voices fall back to the default voice for the language.
- If an engine fails, the next engine in its chain is tried: `TTS_ENGINE` → `FALLBACK_TTS_ENGINE`.
  The `dev` engine is used only if nothing else is available at startup. If every engine fails, the
  service returns 503.
- HTML is stripped. Text is split into sentences on `. ! ? … ؟ ؛` and newlines, and long sentences
  are also split at `, ، ; :`. Sentences are packed into chunks of ≤ `MAX_CHUNK_CHARS` (400). Each
  chunk is synthesized, and the chunks are joined with 150 ms of silence.
- Empty text → 422. Text longer than `MAX_TTS_CHARS` (5000) → 422. `format=mp3` without ffmpeg → 400.

## Speech-to-text

- `ASR_ENGINE=whisper` (default) uses `faster-whisper` with `WHISPER_MODEL=small`. It handles Arabic,
  English and the other Whisper languages, and auto-detects the language when `language` is omitted.
  `WHISPER_DEVICE=auto` probes CUDA with a tiny test inference. If the CUDA 12 runtime libraries
  (cuBLAS/cuDNN) are missing, it falls back to CPU `int8` and logs a warning. Uploads (webm/ogg/wav/
  mp3/m4a) go to a temporary file, which is always deleted afterwards. The service decodes them
  itself with ffmpeg/PyAV, because faster-whisper 1.2's built-in decoder is incompatible with
  PyAV ≥ 19.
- `ASR_ENGINE=none` → `/transcribe` returns 503 `{"detail": "ASR disabled (ASR_ENGINE=none)"}`.
- `ASR_ENGINE=vibevoice` uses `microsoft/VibeVoice-ASR`, a 7B model on Qwen2.5-7B with bf16 weights
  and speaker labels. It needs a large GPU (about 24 GB VRAM recommended) and the upstream package
  (run the setup script). Set `VIBEVOICE_ASR_MODEL_PATH` if you downloaded the model locally. If the
  package is missing, the service falls back to Whisper.

## Responsible use

- The service ships and uses **only the official VibeVoice voice presets** and Microsoft's published
  Edge voices. **Do not clone or imitate the voice of a real person without their explicit,
  informed consent.** If a custom-voice feature is ever added, it must require a consent
  confirmation and label the output.
- Every `/tts` response has `X-AI-Generated: true`. The app stores this as `isAiGenerated` and shows
  an "AI-generated" label next to the clip.
- Don't use generated speech to deceive, impersonate, or spread disinformation. VibeVoice is
  released for research and development. Read Microsoft's model card before any production use.

## Docker

```bash
docker build -t vibevoice-voice .                                        # edge + whisper (small image)
docker build --build-arg INSTALL_VIBEVOICE=true -t vibevoice-voice:full . # + upstream VibeVoice & presets
docker run -p 8001:8001 -v vv-data:/data -e ASR_ENGINE=whisper vibevoice-voice
```

The image is based on `python:3.11-slim` and includes ffmpeg. Hugging Face models are cached in
`/data/hf`. For GPU inference, base the image on an NVIDIA CUDA runtime, install CUDA torch, and run
it with `--gpus all`.

## Environment variables

| Variable | Default | Meaning |
|---|---|---|
| `TTS_ENGINE` | `vibevoice` | `vibevoice` \| `edge` \| `dev` – primary engine (non-Arabic text) |
| `ARABIC_TTS_ENGINE` | `edge` | Engine for Arabic text |
| `FALLBACK_TTS_ENGINE` | `edge` | Used when the primary engine is unavailable or fails (`dev` is the last resort) |
| `ASR_ENGINE` | `whisper` | `none` \| `whisper` \| `vibevoice` |
| `VIBEVOICE_MODEL_PATH` | `microsoft/VibeVoice-Realtime-0.5B` | HF id or local folder |
| `VIBEVOICE_VOICES_DIR` | `voices/streaming_model` | Folder with official `*.pt` presets (relative to voice-service/) |
| `VIBEVOICE_DEFAULT_VOICE` | `en-Carter_man` | Default preset |
| `VIBEVOICE_DEVICE` | `auto` | `auto` \| `cuda` \| `mps` \| `cpu` |
| `VIBEVOICE_CFG_SCALE` | `1.5` | Classifier-free guidance |
| `VIBEVOICE_DDPM_STEPS` | `5` | Diffusion steps |
| `VIBEVOICE_ASR_MODEL_PATH` | `microsoft/VibeVoice-ASR` | ASR model |
| `VIBEVOICE_ASR_LANGUAGE_MODEL` | `Qwen/Qwen2.5-7B` | Tokenizer base for the ASR processor |
| `VIBEVOICE_ASR_MAX_NEW_TOKENS` | `4096` | Generation cap for ASR |
| `EDGE_VOICE_EN` / `EDGE_VOICE_AR` | `en-US-AriaNeural` / `ar-SA-HamedNeural` | Default edge voices |
| `EDGE_TIMEOUT_SECONDS` | `60` | Per-chunk edge-tts timeout |
| `WHISPER_MODEL` | `small` | `tiny` … `large-v3` |
| `WHISPER_DEVICE` | `auto` | `auto` \| `cpu` \| `cuda` |
| `WHISPER_COMPUTE_TYPE` | `auto` | `int8` on cpu, `float16` on cuda |
| `WHISPER_BEAM_SIZE` | `5` | Beam size |
| `WHISPER_VAD` | `true` | Silero VAD filter |
| `MAX_TTS_CHARS` | `5000` | Max cleaned text length |
| `MAX_CHUNK_CHARS` | `400` | Max characters per synthesis chunk |
| `CHUNK_SILENCE_MS` | `150` | Silence between chunks |
| `MAX_UPLOAD_MB` | `50` | Max `/transcribe` upload |
| `PRELOAD_MODELS` | `false` | Load the TTS model at startup (background) |
| `LOG_LEVEL` / `LOG_FORMAT` | `INFO` / `text` | `LOG_FORMAT=json` for one JSON object per line |
| `CORS_ORIGINS` | `*` | Comma-separated |
| `FFMPEG_BINARY` | `ffmpeg` | ffmpeg executable name/path |

## Layout

```
app/main.py              FastAPI app & endpoints (create_app factory)
app/service.py           engine routing, fallback chains, TTS/ASR pipelines
app/config.py            pydantic-settings
app/text.py              HTML stripping, language detection, sentence chunking
app/audio.py             WAV/MP3 encoding, ffmpeg/PyAV decoding, concatenation
app/engines/             base, vibevoice_tts, edge_tts_engine, dev_tts, whisper_asr, vibevoice_asr
scripts/setup_vibevoice.(ps1|sh)
tests/                   pytest (dev engine, no network)
```
