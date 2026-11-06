"""Speech-to-text with faster-whisper (CTranslate2). Arabic + English (+ any Whisper language).

Audio is decoded with the ffmpeg CLI (or PyAV if ffmpeg is absent), so webm/ogg/wav/mp3/m4a
uploads all work. The model is downloaded
from Hugging Face and loaded on the first request.
"""

from __future__ import annotations

import importlib.util
import logging
import threading
from typing import Any

import numpy as np

from app.audio import decode_to_pcm, find_ffmpeg
from app.config import Settings
from app.engines.base import INFERENCE_LOCK, ASREngine, EngineUnavailable, Segment, Transcription

log = logging.getLogger(__name__)


class WhisperASREngine(ASREngine):
    name = "whisper"

    def __init__(self, settings: Settings) -> None:
        self.settings = settings
        self._model: Any = None
        self._device: str | None = None
        self._load_lock = threading.Lock()

    def availability(self) -> tuple[bool, str | None]:
        if importlib.util.find_spec("faster_whisper") is None:
            return False, "faster-whisper not installed"
        return True, None

    @property
    def model_loaded(self) -> bool:
        return self._model is not None

    @property
    def device(self) -> str:
        if self._device is None:
            pref = self.settings.whisper_device.lower()
            if pref != "auto":
                self._device = pref
            else:
                try:
                    import ctranslate2

                    self._device = "cuda" if ctranslate2.get_cuda_device_count() > 0 else "cpu"
                except Exception:  # noqa: BLE001
                    self._device = "cpu"
        return self._device

    def _load(self) -> Any:
        if self._model is not None:
            return self._model
        with self._load_lock:
            if self._model is None:
                ok, reason = self.availability()
                if not ok:
                    raise EngineUnavailable(reason)
                device = self.device
                try:
                    model = self._create(device)
                    if device == "cuda":
                        self._probe(model)
                except Exception as exc:  # noqa: BLE001
                    if device != "cuda":
                        raise
                    # A GPU is visible but the CUDA runtime (cuBLAS 12 / cuDNN 9) is missing or broken.
                    log.warning("faster-whisper cannot use CUDA (%s) - falling back to CPU int8. "
                                "Install the CUDA 12 cuBLAS/cuDNN libraries or set WHISPER_DEVICE=cpu.", exc)
                    self._device = "cpu"
                    model = self._create("cpu")
                self._model = model
        return self._model

    def _create(self, device: str) -> Any:
        from faster_whisper import WhisperModel

        compute = self.settings.whisper_compute_type
        if compute == "auto" or (device == "cpu" and compute == "float16"):
            compute = "float16" if device == "cuda" else "int8"
        log.info("Loading faster-whisper model=%s device=%s compute_type=%s",
                 self.settings.whisper_model, device, compute)
        return WhisperModel(self.settings.whisper_model, device=device, compute_type=compute)

    @staticmethod
    def _probe(model: Any) -> None:
        """Run one tiny inference so missing CUDA DLLs fail here (cleanly) instead of mid-request."""
        segments, _ = model.transcribe(np.zeros(16_000, dtype=np.float32), language="en", beam_size=1,
                                       vad_filter=False)
        list(segments)

    def transcribe(self, path: str, language: str | None) -> Transcription:
        model = self._load()
        # Decode ourselves (ffmpeg CLI, then PyAV) to 16 kHz mono float32. This avoids
        # faster-whisper's own decoder, which breaks with PyAV >= 19 ("metadata_errors").
        with open(path, "rb") as fh:
            pcm = decode_to_pcm(fh.read(), 16_000, find_ffmpeg(self.settings.ffmpeg_binary))
        if pcm is None or pcm.size == 0:
            raise ValueError("could not decode the audio file (unsupported or corrupt format)")
        with INFERENCE_LOCK:
            segments_iter, info = model.transcribe(
                pcm, language=language, beam_size=self.settings.whisper_beam_size,
                vad_filter=self.settings.whisper_vad,
            )
            segments = [
                Segment(start=round(float(s.start), 2), end=round(float(s.end), 2), text=s.text.strip())
                for s in segments_iter  # generator: decoding happens here, keep inside the lock
            ]
        text = " ".join(s.text for s in segments if s.text).strip()
        return Transcription(
            text=text, language=language or getattr(info, "language", None), segments=segments,
            duration=float(getattr(info, "duration", 0.0) or 0.0) or None,
        )
