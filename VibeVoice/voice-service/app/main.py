"""VibeVoice voice service - FastAPI wrapper around Microsoft VibeVoice (TTS / ASR).

Endpoints (snake_case JSON, see docs/API_CONTRACT.md "Voice service"):
  GET  /health      engine status
  GET  /voices      available voices (VibeVoice presets + Arabic fallback voices)
  POST /tts         {text, voice?, language?, format?} -> audio bytes (wav | mp3)
  POST /transcribe  multipart file (+ language?) -> {text, language, segments, engine}

Responsible use: only the official VibeVoice voice presets are used - never clone the voice of a
real person without their explicit consent. Every synthesized clip carries ``X-AI-Generated: true``
so callers can label it as AI-generated in the UI.

Run:  uvicorn app.main:app --host 0.0.0.0 --port 8001
"""

from __future__ import annotations

import logging
import os
import tempfile
import threading
import time
from contextlib import asynccontextmanager
from pathlib import Path
from typing import Literal

from fastapi import FastAPI, File, Form, HTTPException, Request, UploadFile
from fastapi.concurrency import run_in_threadpool
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import Response
from pydantic import BaseModel, Field

from app.audio import VIDEO_EXTENSIONS, extract_audio_to_wav, find_ffmpeg
from app.config import Settings, get_settings
from app.logging_config import configure_logging
from app.service import ASRDisabled, ASRManager, TTSFormatError, TTSInputError, TTSManager, TTSUnavailable

log = logging.getLogger("voice_service")

_ALLOWED_AUDIO_EXT = {".webm", ".ogg", ".oga", ".opus", ".wav", ".mp3", ".m4a", ".mp4", ".aac", ".flac"} | VIDEO_EXTENSIONS
_CT_EXT = {
    "audio/webm": ".webm", "audio/ogg": ".ogg", "audio/wav": ".wav", "audio/x-wav": ".wav",
    "audio/wave": ".wav", "audio/mpeg": ".mp3", "audio/mp3": ".mp3", "audio/mp4": ".m4a",
    "audio/x-m4a": ".m4a", "audio/aac": ".aac", "audio/flac": ".flac", "video/webm": ".webm",
    "video/mp4": ".mp4", "video/quicktime": ".mov", "video/x-matroska": ".mkv", "video/x-msvideo": ".avi",
    "video/3gpp": ".3gp",
}


class TTSRequest(BaseModel):
    text: str = Field(..., description="Plain text or HTML (tags are stripped).")
    voice: str | None = None
    language: str | None = Field(None, description="ISO code (en, ar, ...). Omitted -> detected from script.")
    format: Literal["wav", "mp3"] = "wav"


def create_app(settings: Settings | None = None) -> FastAPI:
    settings = settings or get_settings()
    configure_logging(settings.log_level, settings.log_format)
    tts = TTSManager(settings)
    asr = ASRManager(settings)

    @asynccontextmanager
    async def lifespan(_: FastAPI):
        log.info("Voice service ready: tts=%s arabic_tts=%s asr=%s ffmpeg=%s",
                 tts.active_engine.name, tts.active_arabic_engine.name, asr.name,
                 bool(find_ffmpeg(settings.ffmpeg_binary)))
        if settings.preload_models:
            threading.Thread(target=tts.preload, name="tts-preload", daemon=True).start()
        yield

    app = FastAPI(title="VibeVoice voice service", version="1.0.0", lifespan=lifespan)
    app.state.settings, app.state.tts, app.state.asr = settings, tts, asr
    origins = [o.strip() for o in settings.cors_origins.split(",") if o.strip()]
    app.add_middleware(CORSMiddleware, allow_origins=origins or ["*"], allow_methods=["*"], allow_headers=["*"],
                       expose_headers=["X-Voice", "X-Engine", "X-Language", "X-Duration-Seconds", "X-AI-Generated"])

    @app.middleware("http")
    async def access_log(request: Request, call_next):
        start = time.perf_counter()
        response = await call_next(request)
        ms = round((time.perf_counter() - start) * 1000, 1)
        log.info("%s %s -> %s (%.1f ms)", request.method, request.url.path, response.status_code, ms,
                 extra={"method": request.method, "path": request.url.path,
                        "status": response.status_code, "duration_ms": ms})
        return response

    @app.get("/health")
    def health() -> dict:
        eng = tts.active_engine
        return {
            "status": "ok",
            "tts_engine": eng.name,
            "asr_engine": asr.name,
            "device": eng.device,
            "model_loaded": eng.model_loaded,
            "arabic_tts_engine": tts.active_arabic_engine.name,
            # extras (not in the minimal contract, useful for /api/voice/status messages)
            "requested_tts_engine": settings.tts_engine,
            "requested_asr_engine": settings.asr_engine,
            "asr_model_loaded": bool(asr.engine and asr.engine.model_loaded),
            "asr_device": asr.engine.device if asr.engine else None,
            "ffmpeg": bool(find_ffmpeg(settings.ffmpeg_binary)),
            "notes": tts.notes + asr.notes,
        }

    @app.get("/voices")
    def voices() -> list[dict]:
        return [v.to_dict() for v in tts.list_voices()]

    @app.post("/tts")
    async def synthesize(req: TTSRequest) -> Response:
        try:
            out = await run_in_threadpool(tts.synthesize, req.text, req.voice, req.language, req.format)
        except TTSInputError as exc:
            raise HTTPException(status_code=422, detail=str(exc)) from exc
        except TTSFormatError as exc:
            raise HTTPException(status_code=400, detail=str(exc)) from exc
        except TTSUnavailable as exc:
            raise HTTPException(status_code=503, detail=str(exc)) from exc
        log.info("tts engine=%s voice=%s lang=%s chars=%d chunks=%d dur=%.2fs", out.engine, out.voice,
                 out.language, len(req.text), out.chunks, out.duration,
                 extra={"engine": out.engine, "voice": out.voice, "language": out.language, "chars": len(req.text)})
        return Response(content=out.data, media_type=out.media_type, headers={
            "X-Voice": out.voice,
            "X-Engine": out.engine,
            "X-Language": out.language,
            "X-Duration-Seconds": f"{out.duration:.2f}",
            "X-AI-Generated": "true",
            "X-Sample-Rate": str(out.sample_rate),
            "X-Audio-Format": out.audio_format,
            "Content-Disposition": f'inline; filename="tts.{out.audio_format}"',
        })

    @app.post("/transcribe")
    async def transcribe(file: UploadFile = File(...), language: str | None = Form(None)) -> dict:
        if asr.engine is None:
            raise HTTPException(status_code=503, detail=asr.disabled_reason or "ASR disabled")
        ext = Path(file.filename or "").suffix.lower()
        if ext not in _ALLOWED_AUDIO_EXT:
            ext = _CT_EXT.get((file.content_type or "").split(";")[0].strip().lower(), ".bin")
        limit = settings.max_upload_mb * 1024 * 1024
        fd, path = tempfile.mkstemp(prefix="vv-asr-", suffix=ext)
        try:
            size = 0
            with os.fdopen(fd, "wb") as fh:
                while chunk := await file.read(1024 * 1024):
                    size += len(chunk)
                    if size > limit:
                        raise HTTPException(status_code=413, detail=f"Media file exceeds {settings.max_upload_mb} MB.")
                    fh.write(chunk)
            if size == 0:
                raise HTTPException(status_code=400, detail="Uploaded audio file is empty.")
            asr_input = path
            if ext in VIDEO_EXTENSIONS:
                # Engines expect audio; pull the soundtrack out of the video first.
                wav_path = path + ".wav"
                ok = await run_in_threadpool(extract_audio_to_wav, path, wav_path, 16_000,
                                             find_ffmpeg(settings.ffmpeg_binary))
                if not ok:
                    raise HTTPException(status_code=422, detail="The video has no audio track that could be decoded.")
                asr_input = wav_path
            try:
                result = await run_in_threadpool(asr.transcribe, asr_input, language)
            except ASRDisabled as exc:
                raise HTTPException(status_code=503, detail=str(exc)) from exc
            except Exception as exc:  # noqa: BLE001 - decode errors etc.
                log.exception("Transcription failed")
                raise HTTPException(status_code=422, detail=f"Could not transcribe audio: {exc}") from exc
        finally:
            await file.close()
            for leftover in (path, path + ".wav"):
                try:
                    os.remove(leftover)
                except OSError:
                    pass
        return {
            "text": result.text,
            "language": result.language,
            "segments": [s.to_dict() for s in result.segments],
            "engine": asr.name,
            "duration": result.duration,
        }

    return app


app = create_app()
