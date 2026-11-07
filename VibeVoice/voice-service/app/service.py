"""Engine routing, fallback and the TTS / ASR pipelines (blocking; call from a threadpool)."""

from __future__ import annotations

import logging
from dataclasses import dataclass, field

from app import audio as audio_mod
from app.audio import AudioResult
from app.config import Settings
from app.engines import TTS_ENGINE_NAMES, create_asr_engine, create_tts_engine
from app.engines.base import ASREngine, EngineUnavailable, TTSEngine, Transcription, Voice
from app.text import chunk_text, clean_text, detect_language, normalize_language

log = logging.getLogger(__name__)


class TTSInputError(ValueError):
    """Invalid TTS input (-> HTTP 422)."""


class TTSFormatError(ValueError):
    """Requested output format not producible on this machine (-> HTTP 400)."""


class TTSUnavailable(RuntimeError):
    """No engine could synthesize the request (-> HTTP 503)."""


@dataclass
class TTSOutput:
    data: bytes
    media_type: str
    audio_format: str
    voice: str
    engine: str
    language: str
    duration: float
    sample_rate: int
    chunks: int


@dataclass
class TTSManager:
    settings: Settings
    notes: list[str] = field(default_factory=list)

    def __post_init__(self) -> None:
        self._engines: dict[str, TTSEngine] = {}
        s = self.settings
        self.primary_chain = self._build_chain([s.tts_engine, s.fallback_tts_engine], "TTS_ENGINE")
        self.arabic_chain = self._build_chain([s.arabic_tts_engine, s.fallback_tts_engine], "ARABIC_TTS_ENGINE")

    # -- engines ---------------------------------------------------------------
    def engine(self, name: str) -> TTSEngine:
        if name not in self._engines:
            self._engines[name] = create_tts_engine(name, self.settings)
        return self._engines[name]

    def _usable(self, name: str) -> tuple[bool, str | None]:
        eng = self.engine(name)
        if eng.failed_reason:
            return False, eng.failed_reason
        return eng.availability()

    def _build_chain(self, names: list[str], label: str) -> list[str]:
        chain: list[str] = []
        for i, name in enumerate(dict.fromkeys(names)):
            ok, reason = self._usable(name)
            if ok:
                chain.append(name)
            else:
                msg = f"{label}: engine '{name}' unavailable ({reason})"
                if i == 0:
                    msg += " - falling back"
                log.warning(msg)
                self.notes.append(msg)
        if not chain:
            msg = f"{label}: no configured engine available - using synthetic 'dev' engine"
            log.warning(msg)
            self.notes.append(msg)
            chain.append("dev")
        log.info("%s chain: %s", label, " -> ".join(chain))
        return chain

    def _active(self, chain: list[str]) -> str:
        for name in chain:
            if not self.engine(name).failed_reason:
                return name
        return chain[-1]

    @property
    def active_engine(self) -> TTSEngine:
        return self.engine(self._active(self.primary_chain))

    @property
    def active_arabic_engine(self) -> TTSEngine:
        return self.engine(self._active(self.arabic_chain))

    def _mark_failed(self, eng: TTSEngine, exc: Exception) -> None:
        eng.failed_reason = f"load failed: {exc}"
        msg = f"TTS engine '{eng.name}' disabled after load failure: {exc}"
        log.error(msg)
        self.notes.append(msg)

    def preload(self) -> None:
        for name in dict.fromkeys([self._active(self.primary_chain), self._active(self.arabic_chain)]):
            eng = self.engine(name)
            try:
                eng.load()
            except Exception as exc:  # noqa: BLE001
                self._mark_failed(eng, exc)

    # -- voices -------------------------------------------------------------------
    def list_voices(self) -> list[Voice]:
        seen: dict[str, Voice] = {}
        for name in dict.fromkeys(self.primary_chain + self.arabic_chain):
            eng = self.engine(name)
            if eng.failed_reason:
                continue
            for v in eng.list_voices():
                seen.setdefault(v.id, v)
        return list(seen.values())

    def _voice_owner(self, voice: str) -> TTSEngine | None:
        candidates = list(dict.fromkeys(self.primary_chain + self.arabic_chain + list(TTS_ENGINE_NAMES)))
        for name in candidates:
            ok, _ = self._usable(name)
            if ok and self.engine(name).owns_voice(voice):
                return self.engine(name)
        return None

    def plan(self, voice: str | None, language: str) -> list[tuple[TTSEngine, str]]:
        """Ordered (engine, voice) attempts for a request."""
        chain = self.arabic_chain if language == "ar" else self.primary_chain
        attempts: list[tuple[TTSEngine, str]] = []
        if voice:
            owner = self._voice_owner(voice)
            if owner is not None:
                attempts.append((owner, voice))
            else:
                log.warning("Unknown or unavailable voice %r - using default voice for language %s", voice, language)
        for name in chain:
            eng = self.engine(name)
            if eng.failed_reason or any(e is eng for e, _ in attempts):
                continue
            attempts.append((eng, eng.default_voice(language)))
        return attempts

    # -- pipeline -------------------------------------------------------------------
    def synthesize(self, text: str, voice: str | None, language: str | None, fmt: str = "wav") -> TTSOutput:
        s = self.settings
        cleaned = clean_text(text)
        if not cleaned:
            raise TTSInputError("Text is empty.")
        if len(cleaned) > s.max_tts_chars:
            raise TTSInputError(f"Text is too long ({len(cleaned)} chars, max {s.max_tts_chars}).")
        fmt = (fmt or "wav").lower()
        ffmpeg = audio_mod.find_ffmpeg(s.ffmpeg_binary)
        if fmt == "mp3" and not ffmpeg:
            raise TTSFormatError("format=mp3 requires ffmpeg on the voice-service host; install ffmpeg or use format=wav.")

        lang = normalize_language(language) or detect_language(cleaned)
        chunks = chunk_text(cleaned, s.max_chunk_chars)
        errors: list[str] = []
        for eng, voice_id in self.plan(voice, lang):
            try:
                eng.load()
            except Exception as exc:  # noqa: BLE001
                self._mark_failed(eng, exc)
                errors.append(f"{eng.name}: {exc}")
                continue
            try:
                results = [eng.synthesize(chunk, voice_id, lang) for chunk in chunks]
            except Exception as exc:  # noqa: BLE001 - try next engine
                log.exception("TTS engine %s failed (voice=%s)", eng.name, voice_id)
                errors.append(f"{eng.name}: {exc}")
                continue
            if errors:
                log.warning("TTS served by fallback %s/%s after failures: %s", eng.name, voice_id, errors)
            return self._encode(results, fmt, ffmpeg, eng.name, voice_id, lang, len(chunks))
        raise TTSUnavailable("No TTS engine could synthesize the text: " + "; ".join(errors or ["no engine"]))

    def _encode(self, results: list[AudioResult], fmt: str, ffmpeg: str | None, engine: str, voice: str,
                lang: str, n_chunks: int) -> TTSOutput:
        s = self.settings
        if any(r.pcm is None for r in results):
            # edge-tts without any decoder: MP3 frames can be concatenated as-is.
            data = b"".join(r.encoded or b"" for r in results)
            duration = len(data) * 8 / 48_000  # edge-tts streams 48 kbit/s mono MP3
            return TTSOutput(data, "audio/mpeg", "mp3", voice, engine, lang, duration,
                             results[0].sample_rate, n_chunks)
        rate = results[0].sample_rate
        pcm = audio_mod.concat_with_silence(
            [audio_mod.resample(audio_mod.to_float32(r.pcm), r.sample_rate, rate) for r in results],
            rate, s.chunk_silence_ms,
        )
        duration = audio_mod.duration_seconds(pcm, rate)
        if fmt == "mp3":
            data = audio_mod.pcm_to_mp3_bytes(pcm, rate, ffmpeg)
            return TTSOutput(data, "audio/mpeg", "mp3", voice, engine, lang, duration, rate, n_chunks)
        return TTSOutput(audio_mod.pcm_to_wav_bytes(pcm, rate), "audio/wav", "wav", voice, engine, lang,
                         duration, rate, n_chunks)


class ASRDisabled(RuntimeError):
    """ASR_ENGINE=none or no ASR engine available (-> HTTP 503)."""


@dataclass
class ASRManager:
    settings: Settings
    notes: list[str] = field(default_factory=list)

    def __post_init__(self) -> None:
        self.engine: ASREngine | None = None
        self.disabled_reason: str | None = None
        requested = self.settings.asr_engine
        if requested == "none":
            self.disabled_reason = "ASR disabled (ASR_ENGINE=none)"
            return
        order = [requested] + (["whisper"] if requested == "vibevoice" else [])
        for name in order:
            eng = create_asr_engine(name, self.settings)
            ok, reason = eng.availability()
            if ok:
                self.engine = eng
                break
            msg = f"ASR_ENGINE: engine '{name}' unavailable ({reason})"
            log.warning(msg)
            self.notes.append(msg)
        if self.engine is None:
            self.disabled_reason = f"ASR unavailable (ASR_ENGINE={requested}): " + "; ".join(self.notes)

    @property
    def name(self) -> str:
        return self.engine.name if self.engine else "none"

    @property
    def loaded(self) -> bool:
        return bool(self.engine is not None and getattr(self.engine, "model_loaded", False))

    def unload(self) -> None:
        unload = getattr(self.engine, "unload", None)
        if callable(unload):
            unload()

    def transcribe(self, path: str, language: str | None) -> Transcription:
        if self.engine is None:
            raise ASRDisabled(self.disabled_reason or "ASR disabled")
        try:
            return self.engine.transcribe(path, normalize_language(language))
        except EngineUnavailable as exc:
            raise ASRDisabled(str(exc)) from exc
