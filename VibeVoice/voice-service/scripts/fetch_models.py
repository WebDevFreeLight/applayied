"""Download the OCR and Whisper models into the image at build time (run as root by the Dockerfile).

At runtime the service runs as a non-root user: RapidOCR cannot write into its package folder, so a
model it has to fetch on first use (the Arabic recognizer) fails with PermissionError. Baking the
models in also saves a slow first request on a freshly woken free-tier host.

    python scripts/fetch_models.py [whisper-model-name ...]
"""

from __future__ import annotations

import io
import sys

from PIL import Image, ImageDraw


def fetch_ocr() -> None:
    from app.config import Settings
    from app.engines.rapid_ocr import OCRManager

    # The recognizer is loaded lazily, only once the detector finds text, so read a real line.
    img = Image.new("RGB", (480, 120), "white")
    ImageDraw.Draw(img).text((20, 40), "VibeVoice 2026", fill="black", font_size=40)
    buf = io.BytesIO()
    img.save(buf, format="PNG")

    manager = OCRManager(Settings(ocr_engine="rapidocr"))
    for language in ("ar", "en"):  # Arabic model (reads Arabic + Latin) and the English model
        result = manager.recognize(buf.getvalue(), language)
        print(f"OCR model '{language}' ready - sample read: {result.text!r}")


def fetch_whisper(names: list[str]) -> None:
    from faster_whisper import WhisperModel

    for name in names:
        WhisperModel(name, device="cpu", compute_type="int8")
        print(f"Whisper model '{name}' ready")


if __name__ == "__main__":
    fetch_ocr()
    fetch_whisper([n for n in sys.argv[1:] if n])
