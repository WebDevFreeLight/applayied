"""Common engine interfaces.

TTS engines implement :class:`TTSEngine` (``synthesize`` / ``list_voices``); ASR engines
implement :class:`ASREngine` (``transcribe``). Heavy dependencies (torch, vibevoice,
faster-whisper) are imported lazily inside the engines so the service starts without them.
"""

from __future__ import annotations

import threading
from abc import ABC, abstractmethod
from dataclasses import asdict, dataclass, field
from typing import Any

from app.audio import AudioResult

# One lock serialises all local model inference (single GPU / avoid CPU oversubscription).
INFERENCE_LOCK = threading.Lock()


class EngineUnavailable(RuntimeError):
    """The engine cannot be used on this machine (missing package, model, voices...)."""


@dataclass
class Voice:
    id: str
    name: str
    language: str
    gender: str | None
    engine: str
    is_fallback: bool

    def to_dict(self) -> dict[str, Any]:
        return asdict(self)


@dataclass
class Segment:
    start: float
    end: float
    text: str
    speaker: str | None = None

    def to_dict(self) -> dict[str, Any]:
        return asdict(self)


@dataclass
class Transcription:
    text: str
    language: str | None
    segments: list[Segment] = field(default_factory=list)
    duration: float | None = None


class TTSEngine(ABC):
    name: str = "base"
    is_fallback: bool = False

    def __init__(self) -> None:
        self.failed_reason: str | None = None  # set when loading failed permanently

    # -- availability / lifecycle -----------------------------------------
    def availability(self) -> tuple[bool, str | None]:
        """Cheap check (no model load): ``(available, reason_if_not)``."""
        return True, None

    def load(self) -> None:  # noqa: B027 - optional hook
        """Load models (lazy; called before the first synthesis)."""

    @property
    def model_loaded(self) -> bool:
        return True

    @property
    def device(self) -> str:
        return "cpu"

    # -- voices -------------------------------------------------------------
    @abstractmethod
    def list_voices(self) -> list[Voice]: ...

    def owns_voice(self, voice_id: str) -> bool:
        return any(v.id == voice_id for v in self.list_voices())

    @abstractmethod
    def default_voice(self, language: str) -> str: ...

    # -- inference ------------------------------------------------------------
    @abstractmethod
    def synthesize(self, text: str, voice: str, language: str) -> AudioResult:
        """Synthesize one chunk of text (already cleaned & chunked)."""


class ASREngine(ABC):
    name: str = "base"

    def availability(self) -> tuple[bool, str | None]:
        return True, None

    @property
    def model_loaded(self) -> bool:
        return False

    @property
    def device(self) -> str:
        return "cpu"

    @abstractmethod
    def transcribe(self, path: str, language: str | None) -> Transcription: ...
