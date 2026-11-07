"""Engine factories."""

from __future__ import annotations

from app.config import Settings
from app.engines.base import ASREngine, TTSEngine

TTS_ENGINE_NAMES = ("vibevoice", "edge", "dev")


def create_tts_engine(name: str, settings: Settings) -> TTSEngine:
    if name == "vibevoice":
        from app.engines.vibevoice_tts import VibeVoiceTTSEngine

        return VibeVoiceTTSEngine(settings)
    if name == "edge":
        from app.engines.edge_tts_engine import EdgeTTSEngine

        return EdgeTTSEngine(settings)
    if name == "dev":
        from app.engines.dev_tts import DevTTSEngine

        return DevTTSEngine()
    raise ValueError(f"unknown TTS engine {name!r}")


def create_asr_engine(name: str, settings: Settings) -> ASREngine:
    if name == "whisper":
        from app.engines.whisper_asr import WhisperASREngine

        return WhisperASREngine(settings)
    if name == "vibevoice":
        from app.engines.vibevoice_asr import VibeVoiceASREngine

        return VibeVoiceASREngine(settings)
    raise ValueError(f"unknown ASR engine {name!r}")
