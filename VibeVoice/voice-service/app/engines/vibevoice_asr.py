"""Microsoft VibeVoice-ASR (7B, built on Qwen2.5-7B) speech-to-text with speaker labels.

GPU only (bf16, ~20 GB+ VRAM). Enable with ``ASR_ENGINE=vibevoice`` after installing the upstream
package (scripts/setup_vibevoice). If it is unavailable the service falls back to Whisper.
"""

from __future__ import annotations

import importlib.util
import logging
import threading
from typing import Any

from app.config import Settings
from app.engines.base import INFERENCE_LOCK, ASREngine, EngineUnavailable, Segment, Transcription
from app.engines.vibevoice_tts import detect_device
from app.text import detect_language

log = logging.getLogger(__name__)


def _first(d: dict[str, Any], *keys: str, default: Any = None) -> Any:
    for k in keys:
        if k in d and d[k] is not None:
            return d[k]
    return default


def _to_float(v: Any) -> float:
    try:
        return round(float(v), 2)
    except (TypeError, ValueError):
        return 0.0


def map_segments(raw: Any) -> list[Segment]:
    """Map upstream ``post_process_transcription`` output defensively to ``Segment``s."""
    if not isinstance(raw, (list, tuple)):
        return []
    out: list[Segment] = []
    for item in raw:
        if isinstance(item, str):
            out.append(Segment(0.0, 0.0, item.strip()))
            continue
        if not isinstance(item, dict):
            continue
        text = str(_first(item, "text", "content", "transcript", default="")).strip()
        speaker = _first(item, "speaker", "speaker_id", "spk")
        out.append(Segment(
            start=_to_float(_first(item, "start", "start_time", "begin", default=0.0)),
            end=_to_float(_first(item, "end", "end_time", "stop", default=0.0)),
            text=text,
            speaker=None if speaker is None else str(speaker),
        ))
    return out


class VibeVoiceASREngine(ASREngine):
    name = "vibevoice"

    def __init__(self, settings: Settings) -> None:
        self.settings = settings
        self._model: Any = None
        self._processor: Any = None
        self._device: str | None = None
        self._load_lock = threading.Lock()

    def availability(self) -> tuple[bool, str | None]:
        if importlib.util.find_spec("torch") is None:
            return False, "torch not installed"
        if importlib.util.find_spec("vibevoice") is None:
            return False, "vibevoice package not installed"
        return True, None

    @property
    def model_loaded(self) -> bool:
        return self._model is not None

    @property
    def device(self) -> str:
        if self._device is None:
            try:
                self._device = detect_device(self.settings.vibevoice_device)
            except Exception:  # noqa: BLE001
                return "unknown"
        return self._device

    def _load(self) -> None:
        if self._model is not None:
            return
        with self._load_lock:
            if self._model is not None:
                return
            ok, reason = self.availability()
            if not ok:
                raise EngineUnavailable(reason)
            import torch
            from vibevoice.modular.modeling_vibevoice_asr import VibeVoiceASRForConditionalGeneration
            from vibevoice.processor.vibevoice_asr_processor import VibeVoiceASRProcessor

            if self.device == "cpu":
                log.warning("VibeVoice-ASR (7B) on CPU is impractically slow; use a large GPU or ASR_ENGINE=whisper")
            path = self.settings.vibevoice_asr_model_path
            log.info("Loading VibeVoice-ASR %s on %s ...", path, self.device)
            processor = VibeVoiceASRProcessor.from_pretrained(
                path, language_model_pretrained_name=self.settings.vibevoice_asr_language_model)
            model = VibeVoiceASRForConditionalGeneration.from_pretrained(
                path, dtype=torch.bfloat16, device_map=None, attn_implementation="sdpa", trust_remote_code=True,
            ).to(self.device).eval()
            self._processor, self._model = processor, model

    def transcribe(self, path: str, language: str | None) -> Transcription:
        self._load()
        import torch

        processor, model = self._processor, self._model
        with INFERENCE_LOCK, torch.no_grad():
            inputs = processor(audio=[path], sampling_rate=None, return_tensors="pt", padding=True,
                               add_generation_prompt=True)
            inputs = {k: v.to(self.device) if isinstance(v, torch.Tensor) else v for k, v in inputs.items()}
            out = model.generate(
                **inputs, max_new_tokens=self.settings.vibevoice_asr_max_new_tokens,
                pad_token_id=processor.pad_id, eos_token_id=processor.tokenizer.eos_token_id, do_sample=False,
            )
        ids = out[0, inputs["input_ids"].shape[1]:].tolist()
        eos = processor.tokenizer.eos_token_id
        if eos in ids:
            ids = ids[: ids.index(eos)]
        raw_text = processor.decode(ids, skip_special_tokens=True)
        try:
            segments = map_segments(processor.post_process_transcription(raw_text))
        except Exception as exc:  # noqa: BLE001 - keep the raw text if parsing fails
            log.warning("post_process_transcription failed: %s", exc)
            segments = []
        text = " ".join(s.text for s in segments if s.text).strip() or raw_text.strip()
        duration = max((s.end for s in segments), default=0.0) or None
        return Transcription(text=text, language=language or detect_language(text), segments=segments,
                             duration=duration)
