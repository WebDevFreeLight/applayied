"""Deterministic synthetic "speech" (soft beeps) for tests, CI and machines without models.

It does not speak: each word becomes a short soft tone whose pitch is derived from the word,
so the output length scales with the text length and is identical for identical input.
"""

from __future__ import annotations

import zlib

import numpy as np

from app.audio import DEFAULT_SAMPLE_RATE, AudioResult
from app.engines.base import TTSEngine, Voice

_VOICES = [
    Voice("dev-en", "Dev tone (en)", "en", None, "dev", True),
    Voice("dev-ar", "Dev tone (ar)", "ar", None, "dev", True),
]


class DevTTSEngine(TTSEngine):
    name = "dev"
    is_fallback = True
    sample_rate = DEFAULT_SAMPLE_RATE

    def list_voices(self) -> list[Voice]:
        return list(_VOICES)

    def owns_voice(self, voice_id: str) -> bool:
        return voice_id.startswith("dev-")

    def default_voice(self, language: str) -> str:
        return "dev-ar" if language == "ar" else "dev-en"

    def synthesize(self, text: str, voice: str, language: str) -> AudioResult:
        sr = self.sample_rate
        base = 180.0 if voice == "dev-ar" else 220.0
        words = text.split() or [text]
        pieces: list[np.ndarray] = []
        for word in words:
            n_chars = max(1, len(word))
            dur = min(0.06 * n_chars + 0.08, 0.9)
            n = int(sr * dur)
            t = np.arange(n, dtype=np.float32) / sr
            freq = base + (zlib.crc32(word.encode("utf-8")) % 12) * 20.0
            tone = 0.25 * np.sin(2 * np.pi * freq * t) + 0.08 * np.sin(2 * np.pi * 2 * freq * t)
            env = np.sin(np.pi * np.arange(n, dtype=np.float32) / max(n - 1, 1)) ** 2  # soft fade in/out
            pieces.append((tone * env).astype(np.float32))
            pieces.append(np.zeros(int(sr * 0.05), dtype=np.float32))
        return AudioResult(pcm=np.concatenate(pieces), sample_rate=sr)
