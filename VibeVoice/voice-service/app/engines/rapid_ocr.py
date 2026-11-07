"""Image → text (OCR) with RapidOCR (PaddleOCR PP-OCRv5/v6 models on onnxruntime).

The Docker image downloads the models at build time (scripts/fetch_models.py): at runtime the service
runs as a non-root user and cannot write into the rapidocr package folder.

The PP-OCRv5 *Arabic* recognition model also covers Latin letters and digits, so it reads Arabic,
English and mixed lines in one pass; it is the default for ``language`` auto/"ar". ``language="en"``
uses the dedicated English model. Arabic needs ``python-bidi`` (``pip install "rapidocr[rtl]"``) so
characters come back in logical (reading) order. Models download on first use (~15 MB each).
"""

from __future__ import annotations

import importlib.util
import logging
import re
import threading
from dataclasses import dataclass, field
from typing import Any

from app.config import Settings
from app.engines.base import INFERENCE_LOCK

log = logging.getLogger(__name__)

_ARABIC = re.compile(r"[؀-ۿݐ-ݿࢠ-ࣿ]")


class OCRDisabled(RuntimeError):
    """OCR is switched off or its dependencies are missing."""


@dataclass
class OCRLine:
    text: str
    confidence: float
    box: list[list[float]]

    def to_dict(self) -> dict:
        return {"text": self.text, "confidence": round(self.confidence, 3), "box": self.box}


@dataclass
class OCRResult:
    text: str
    language: str | None
    lines: list[OCRLine] = field(default_factory=list)


def guess_language(text: str) -> str | None:
    if not text.strip():
        return None
    return "ar" if _ARABIC.search(text) else "en"


def reading_order(lines: list[OCRLine]) -> list[OCRLine]:
    """Top-to-bottom; lines whose vertical centres are within half a line height count as one row."""
    def centre_y(line: OCRLine) -> float:
        return sum(p[1] for p in line.box) / 4 if line.box else 0.0

    def height(line: OCRLine) -> float:
        ys = [p[1] for p in line.box] or [0.0]
        return max(ys) - min(ys)

    ordered = sorted(lines, key=centre_y)
    rows: list[list[OCRLine]] = []
    for line in ordered:
        if rows and abs(centre_y(line) - centre_y(rows[-1][0])) < max(height(rows[-1][0]), 1) / 2:
            rows[-1].append(line)
        else:
            rows.append([line])
    # Within a row keep the detector's left-to-right order; mixed Arabic/English rows are rare.
    return [ln for row in rows for ln in sorted(row, key=lambda ln: min((p[0] for p in ln.box), default=0))]


class OCRManager:
    def __init__(self, settings: Settings) -> None:
        self.settings = settings
        self._engines: dict[str, Any] = {}
        self._lock = threading.Lock()
        self.disabled_reason: str | None = None
        if settings.ocr_engine == "none":
            self.disabled_reason = "OCR disabled (OCR_ENGINE=none)"
        elif importlib.util.find_spec("rapidocr") is None:
            self.disabled_reason = "rapidocr not installed (pip install \"rapidocr[rtl]\")"
            log.warning(self.disabled_reason)

    @property
    def name(self) -> str:
        return "none" if self.disabled_reason else "rapidocr"

    def base_params(self) -> dict[str, Any]:
        """Memory-bounded detector settings shared by every recognition model."""
        params: dict[str, Any] = {
            # Cap the LONGER side; the library default ("min") upscales wide images to 3000+ px.
            "Det.limit_type": "max",
            "Det.limit_side_len": self.settings.ocr_max_side,
            # Photos of notes/documents are upright; the 180-degree classifier only costs time + RAM.
            "Global.use_cls": False,
        }
        if self.settings.ocr_threads > 0:
            params["EngineConfig.onnxruntime.intra_op_num_threads"] = self.settings.ocr_threads
        return params

    @property
    def loaded(self) -> bool:
        return bool(self._engines)

    def unload(self) -> None:
        """Free the OCR models (low-memory mode, before Whisper loads)."""
        with self._lock:
            if self._engines:
                log.info("Unloading RapidOCR models to free memory")
                self._engines.clear()

    def _engine(self, model: str) -> Any:
        if model in self._engines:
            return self._engines[model]
        with self._lock:
            if model not in self._engines:
                from rapidocr import LangRec, ModelType, OCRVersion, RapidOCR

                params = self.base_params()
                if model == "arabic":
                    params |= {"Rec.lang_type": LangRec.ARABIC, "Rec.ocr_version": OCRVersion.PPOCRV5,
                               "Rec.model_type": ModelType.MOBILE}
                else:
                    params |= {"Rec.lang_type": LangRec.EN}
                log.info("Loading RapidOCR recognition model '%s'", model)
                try:
                    self._engines[model] = RapidOCR(params=params)
                except ModuleNotFoundError as exc:  # python-bidi missing for RTL
                    raise OCRDisabled(str(exc)) from exc
        return self._engines[model]

    def recognize(self, image: bytes, language: str | None) -> OCRResult:
        if self.disabled_reason:
            raise OCRDisabled(self.disabled_reason)
        model = "en" if (language or "").lower().startswith("en") else "arabic"
        engine = self._engine(model)
        with INFERENCE_LOCK:
            out = engine(image)
        txts = list(getattr(out, "txts", None) or [])
        scores = list(getattr(out, "scores", None) or [])
        boxes = getattr(out, "boxes", None)
        lines = [
            OCRLine(text=t.strip(), confidence=float(scores[i]) if i < len(scores) else 0.0,
                    box=boxes[i].tolist() if boxes is not None and i < len(boxes) else [])
            for i, t in enumerate(txts) if t and t.strip()
        ]
        lines = [ln for ln in reading_order(lines) if ln.confidence >= self.settings.ocr_min_confidence]
        text = "\n".join(ln.text for ln in lines)
        return OCRResult(text=text, language=guess_language(text) if not language else language, lines=lines)
