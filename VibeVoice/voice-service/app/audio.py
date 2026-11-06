"""Audio helpers: WAV encoding (stdlib), concatenation, resampling and ffmpeg / PyAV transcoding."""

from __future__ import annotations

import io
import logging
import shutil
import subprocess
import wave
from dataclasses import dataclass
from functools import lru_cache

import numpy as np

log = logging.getLogger(__name__)

DEFAULT_SAMPLE_RATE = 24_000


@dataclass
class AudioResult:
    """Output of a TTS engine for one chunk.

    Normally ``pcm`` holds mono float32 samples in [-1, 1]. Engines that can only produce an
    encoded stream (edge-tts without ffmpeg/PyAV) set ``encoded`` + ``encoded_format`` instead.
    """

    pcm: np.ndarray | None
    sample_rate: int
    encoded: bytes | None = None
    encoded_format: str | None = None  # e.g. "mp3"


@lru_cache
def find_ffmpeg(binary: str = "ffmpeg") -> str | None:
    return shutil.which(binary)


def to_float32(samples: np.ndarray) -> np.ndarray:
    arr = np.asarray(samples)
    if arr.dtype == np.int16:
        arr = arr.astype(np.float32) / 32768.0
    arr = arr.astype(np.float32, copy=False).reshape(-1)
    return np.clip(np.nan_to_num(arr), -1.0, 1.0)


def resample(pcm: np.ndarray, src_rate: int, dst_rate: int) -> np.ndarray:
    """Linear-interpolation resample (only used if engines disagree on sample rate)."""
    if src_rate == dst_rate or pcm.size == 0:
        return pcm
    n_out = int(round(pcm.size * dst_rate / src_rate))
    x_old = np.linspace(0.0, 1.0, num=pcm.size, endpoint=False)
    x_new = np.linspace(0.0, 1.0, num=n_out, endpoint=False)
    return np.interp(x_new, x_old, pcm).astype(np.float32)


def concat_with_silence(chunks: list[np.ndarray], sample_rate: int, silence_ms: int) -> np.ndarray:
    if not chunks:
        return np.zeros(0, dtype=np.float32)
    gap = np.zeros(int(sample_rate * silence_ms / 1000), dtype=np.float32)
    parts: list[np.ndarray] = []
    for i, c in enumerate(chunks):
        if i:
            parts.append(gap)
        parts.append(c)
    return np.concatenate(parts).astype(np.float32)


def pcm_to_wav_bytes(pcm: np.ndarray, sample_rate: int) -> bytes:
    """Mono 16-bit PCM WAV."""
    ints = (np.clip(pcm, -1.0, 1.0) * 32767.0).astype("<i2")
    buf = io.BytesIO()
    with wave.open(buf, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(sample_rate)
        w.writeframes(ints.tobytes())
    return buf.getvalue()


def duration_seconds(pcm: np.ndarray, sample_rate: int) -> float:
    return float(pcm.size) / float(sample_rate) if sample_rate else 0.0


def pcm_to_mp3_bytes(pcm: np.ndarray, sample_rate: int, ffmpeg: str) -> bytes:
    wav = pcm_to_wav_bytes(pcm, sample_rate)
    proc = subprocess.run(
        [ffmpeg, "-hide_banner", "-loglevel", "error", "-f", "wav", "-i", "pipe:0",
         "-codec:a", "libmp3lame", "-b:a", "96k", "-f", "mp3", "pipe:1"],
        input=wav, capture_output=True, check=False, timeout=300,
    )
    if proc.returncode != 0 or not proc.stdout:
        raise RuntimeError(f"ffmpeg mp3 encode failed: {proc.stderr.decode(errors='replace')[:300]}")
    return proc.stdout


def decode_to_pcm(data: bytes, sample_rate: int, ffmpeg: str | None) -> np.ndarray | None:
    """Decode any compressed audio (mp3, ...) to mono float32 at ``sample_rate``.

    Tries the ffmpeg CLI first, then PyAV (shipped with faster-whisper). Returns ``None`` if
    neither is available or decoding fails, so callers can fall back to passing bytes through.
    """
    if ffmpeg:
        proc = subprocess.run(
            [ffmpeg, "-hide_banner", "-loglevel", "error", "-i", "pipe:0",
             "-f", "s16le", "-acodec", "pcm_s16le", "-ac", "1", "-ar", str(sample_rate), "pipe:1"],
            input=data, capture_output=True, check=False, timeout=300,
        )
        if proc.returncode == 0 and proc.stdout:
            return np.frombuffer(proc.stdout, dtype="<i2").astype(np.float32) / 32768.0
        log.warning("ffmpeg decode failed: %s", proc.stderr.decode(errors="replace")[:300])
    try:
        import av  # type: ignore[import-not-found]
    except ImportError:
        return None
    try:
        with av.open(io.BytesIO(data)) as container:
            resampler = av.AudioResampler(format="s16", layout="mono", rate=sample_rate)
            frames: list[np.ndarray] = []
            for frame in container.decode(audio=0):
                for out in resampler.resample(frame):
                    frames.append(out.to_ndarray().reshape(-1))
            for out in resampler.resample(None):  # flush
                frames.append(out.to_ndarray().reshape(-1))
        if not frames:
            return None
        return np.concatenate(frames).astype(np.float32) / 32768.0
    except Exception as exc:  # noqa: BLE001 - any decoder error -> pass-through fallback
        log.warning("PyAV decode failed: %s", exc)
        return None
