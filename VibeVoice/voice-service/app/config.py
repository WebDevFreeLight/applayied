"""Service configuration (environment variables / optional ``.env`` file).

Every setting maps 1:1 to an upper-case environment variable, e.g.
``tts_engine`` <- ``TTS_ENGINE``. See ``.env.example`` and README for the full table.
"""

from __future__ import annotations

from functools import lru_cache
from pathlib import Path
from typing import Literal

from pydantic import field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict

SERVICE_ROOT = Path(__file__).resolve().parent.parent

TTSEngineName = Literal["vibevoice", "edge", "dev"]
ASREngineName = Literal["none", "whisper", "vibevoice"]


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    # --- engine routing -------------------------------------------------
    tts_engine: TTSEngineName = "vibevoice"
    arabic_tts_engine: TTSEngineName = "edge"
    fallback_tts_engine: TTSEngineName = "edge"
    asr_engine: ASREngineName = "whisper"
    # Image -> text. rapidocr = PaddleOCR PP-OCRv5 models on onnxruntime (Arabic + English).
    ocr_engine: Literal["rapidocr", "none"] = "rapidocr"
    ocr_min_confidence: float = 0.5
    ocr_max_upload_mb: int = 15

    # --- VibeVoice realtime TTS ----------------------------------------
    vibevoice_model_path: str = "microsoft/VibeVoice-Realtime-0.5B"
    vibevoice_voices_dir: str = "voices/streaming_model"
    vibevoice_default_voice: str = "en-Carter_man"
    vibevoice_device: str = "auto"  # auto | cuda | mps | cpu
    vibevoice_cfg_scale: float = 1.5
    vibevoice_ddpm_steps: int = 5

    # --- VibeVoice ASR (7B, GPU only) ----------------------------------
    vibevoice_asr_model_path: str = "microsoft/VibeVoice-ASR"
    vibevoice_asr_language_model: str = "Qwen/Qwen2.5-7B"
    vibevoice_asr_max_new_tokens: int = 4096

    # --- edge-tts (Microsoft neural voices, needs internet) ------------
    edge_voice_en: str = "en-US-AriaNeural"
    edge_voice_ar: str = "ar-SA-HamedNeural"
    edge_timeout_seconds: float = 60.0

    # --- faster-whisper ASR --------------------------------------------
    whisper_model: str = "small"
    whisper_device: str = "auto"  # auto | cpu | cuda
    whisper_compute_type: str = "auto"  # auto -> int8 on cpu, float16 on cuda
    whisper_beam_size: int = 5
    whisper_vad: bool = True

    # --- limits / text handling ----------------------------------------
    max_tts_chars: int = 5000
    max_chunk_chars: int = 400
    chunk_silence_ms: int = 150
    max_upload_mb: int = 512

    # --- misc -----------------------------------------------------------
    preload_models: bool = False
    log_level: str = "INFO"
    log_format: Literal["text", "json"] = "text"
    cors_origins: str = "*"
    # When set, every endpoint except /health requires the header X-Api-Key with this value
    # (set it whenever the service is reachable from the internet, e.g. on Render).
    api_key: str | None = None
    ffmpeg_binary: str = "ffmpeg"

    @field_validator(
        "tts_engine", "arabic_tts_engine", "fallback_tts_engine", "asr_engine", "ocr_engine", mode="before"
    )
    @classmethod
    def _lower(cls, v: object) -> object:
        return v.strip().lower() if isinstance(v, str) else v

    def resolve_path(self, value: str) -> Path:
        """Relative paths are resolved against the voice-service folder, not the CWD."""
        p = Path(value)
        return p if p.is_absolute() else SERVICE_ROOT / p


@lru_cache
def get_settings() -> Settings:
    return Settings()
