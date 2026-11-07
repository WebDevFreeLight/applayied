"""Microsoft VibeVoice-Realtime-0.5B text-to-speech engine.

Responsible use: this engine only uses the *official* pre-computed voice presets shipped in the
upstream repository (``demo/voices/streaming_model/*.pt``). Do not add presets cloned from real
people without their explicit consent. Every response is flagged ``X-AI-Generated: true``.

``torch`` and ``vibevoice`` are imported lazily so the service can start without them; the model
is loaded on the first request (thread-safe) and kept in memory. Output is 24 kHz mono.
"""

from __future__ import annotations

import copy
import importlib.util
import logging
import threading
from pathlib import Path
from typing import Any

import numpy as np

from app.audio import AudioResult, to_float32
from app.config import Settings
from app.engines.base import INFERENCE_LOCK, EngineUnavailable, TTSEngine, Voice
from app.text import normalize_quotes

log = logging.getLogger(__name__)

SAMPLE_RATE = 24_000
_LANG_MAP = {"sp": "es", "jp": "ja", "kr": "ko", "in": "en"}  # "in" = Indian English
_GENDER_MAP = {"man": "male", "woman": "female"}


def parse_voice_id(voice_id: str) -> Voice:
    """``en-Carter_man`` -> Voice(name="Carter (en, male)", language="en", gender="male")."""
    prefix, _, rest = voice_id.partition("-")
    speaker, _, gender_raw = rest.rpartition("_")
    if not speaker:  # no gender suffix
        speaker, gender_raw = rest, ""
    lang = _LANG_MAP.get(prefix.lower(), prefix.lower())
    gender = _GENDER_MAP.get(gender_raw.lower())
    display = speaker
    if speaker.lower().startswith("spk") and speaker[3:].isdigit():
        display = f"Speaker {speaker[3:]}"
    tag = ", ".join(x for x in (lang, gender) if x)
    return Voice(voice_id, f"{display} ({tag})", lang, gender, "vibevoice", False)


def detect_device(preference: str = "auto") -> str:
    import torch

    pref = (preference or "auto").lower()
    if pref != "auto":
        return pref
    if torch.cuda.is_available():
        return "cuda"
    mps = getattr(torch.backends, "mps", None)
    if mps is not None and mps.is_available():
        return "mps"
    return "cpu"


class VibeVoiceTTSEngine(TTSEngine):
    name = "vibevoice"
    is_fallback = False

    def __init__(self, settings: Settings) -> None:
        super().__init__()
        self.settings = settings
        self.voices_dir: Path = settings.resolve_path(settings.vibevoice_voices_dir)
        self._load_lock = threading.Lock()
        self._model: Any = None
        self._processor: Any = None
        self._device: str | None = None
        self._voice_cache: dict[str, Any] = {}

    # -- availability -----------------------------------------------------------
    def _preset_files(self) -> dict[str, Path]:
        if not self.voices_dir.is_dir():
            return {}
        return {p.stem: p for p in sorted(self.voices_dir.glob("*.pt"))}

    def availability(self) -> tuple[bool, str | None]:
        if importlib.util.find_spec("torch") is None:
            return False, "torch not installed (see requirements-vibevoice.txt)"
        if importlib.util.find_spec("vibevoice") is None:
            return False, "vibevoice package not installed (run scripts/setup_vibevoice)"
        if not self._preset_files():
            return False, f"no voice presets (*.pt) found in {self.voices_dir}"
        return True, None

    @property
    def model_loaded(self) -> bool:
        return self._model is not None

    @property
    def device(self) -> str:
        if self._device is None:
            try:
                self._device = detect_device(self.settings.vibevoice_device)
            except Exception:  # noqa: BLE001 - torch missing
                return "unknown"
        return self._device

    # -- voices -------------------------------------------------------------------
    def list_voices(self) -> list[Voice]:
        return [parse_voice_id(vid) for vid in self._preset_files()]

    def owns_voice(self, voice_id: str) -> bool:
        return voice_id in self._preset_files()

    def default_voice(self, language: str) -> str:
        presets = list(self._preset_files())
        default = self.settings.vibevoice_default_voice
        if not presets:
            return default
        if default in presets and parse_voice_id(default).language == language:
            return default
        for vid in presets:
            if parse_voice_id(vid).language == language:
                return vid
        return default if default in presets else presets[0]

    # -- loading ------------------------------------------------------------------
    def load(self) -> None:
        if self._model is not None:
            return
        with self._load_lock:
            if self._model is not None:
                return
            ok, reason = self.availability()
            if not ok:
                raise EngineUnavailable(reason)
            import torch
            from vibevoice.modular.modeling_vibevoice_streaming_inference import (
                VibeVoiceStreamingForConditionalGenerationInference,
            )
            from vibevoice.processor.vibevoice_streaming_processor import VibeVoiceStreamingProcessor

            device = self.device
            if device == "cpu":
                log.warning("VibeVoice is running on CPU - synthesis will be SLOW. Use a CUDA GPU for realtime speed.")
            path = self.settings.vibevoice_model_path
            log.info("Loading VibeVoice model %s on %s ...", path, device)
            processor = VibeVoiceStreamingProcessor.from_pretrained(path)
            cls = VibeVoiceStreamingForConditionalGenerationInference
            if device == "cuda":
                try:
                    model = cls.from_pretrained(path, torch_dtype=torch.bfloat16, device_map="cuda",
                                                attn_implementation="flash_attention_2")
                except Exception as exc:  # noqa: BLE001 - flash-attn missing / unsupported GPU
                    log.warning("flash_attention_2 unavailable (%s); falling back to sdpa", exc)
                    model = cls.from_pretrained(path, torch_dtype=torch.bfloat16, device_map="cuda",
                                                attn_implementation="sdpa")
            elif device == "mps":
                model = cls.from_pretrained(path, torch_dtype=torch.float32, device_map=None,
                                            attn_implementation="sdpa")
                model.to("mps")
            else:
                model = cls.from_pretrained(path, torch_dtype=torch.float32, device_map="cpu",
                                            attn_implementation="sdpa")
            model.eval()
            model.set_ddpm_inference_steps(num_steps=self.settings.vibevoice_ddpm_steps)
            self._processor = processor
            self._model = model
            log.info("VibeVoice model loaded (device=%s)", device)

    def _voice_prompt(self, voice_id: str) -> Any:
        if voice_id in self._voice_cache:
            return self._voice_cache[voice_id]
        path = self._preset_files().get(voice_id)
        if path is None:
            raise EngineUnavailable(f"unknown VibeVoice preset {voice_id!r}")
        import torch
        from transformers.cache_utils import DynamicCache
        from transformers.modeling_outputs import BaseModelOutputWithPast

        with torch.serialization.safe_globals([BaseModelOutputWithPast, DynamicCache]):
            prefilled = torch.load(path, map_location=self.device, weights_only=True)
        self._voice_cache[voice_id] = prefilled
        return prefilled

    # -- inference -------------------------------------------------------------------
    def synthesize(self, text: str, voice: str, language: str) -> AudioResult:
        self.load()
        import torch

        text = normalize_quotes(text)
        with INFERENCE_LOCK:
            prefilled = self._voice_prompt(voice)
            inputs = self._processor.process_input_with_cached_prompt(
                text=text, cached_prompt=prefilled, padding=True, return_tensors="pt",
                return_attention_mask=True,
            )
            inputs = {k: (v.to(self.device) if torch.is_tensor(v) else v) for k, v in inputs.items()}
            with torch.no_grad():
                outputs = self._model.generate(
                    **inputs, max_new_tokens=None, cfg_scale=self.settings.vibevoice_cfg_scale,
                    tokenizer=self._processor.tokenizer, generation_config={"do_sample": False},
                    verbose=False, all_prefilled_outputs=copy.deepcopy(prefilled),
                )
        speech = getattr(outputs, "speech_outputs", None)
        if not speech or speech[0] is None:
            raise RuntimeError("VibeVoice produced no audio")
        audio = speech[0]
        if torch.is_tensor(audio):
            audio = audio.detach().to(torch.float32).cpu().numpy()
        return AudioResult(pcm=to_float32(np.asarray(audio)), sample_rate=SAMPLE_RATE)
