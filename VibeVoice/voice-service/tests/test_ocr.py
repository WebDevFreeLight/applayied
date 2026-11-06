import importlib.util
import io

import pytest
from fastapi.testclient import TestClient

from app.engines.rapid_ocr import OCRLine, guess_language, reading_order
from app.main import create_app
from tests.conftest import make_settings


def _box(x: float, y: float, w: float = 100, h: float = 20) -> list[list[float]]:
    return [[x, y], [x + w, y], [x + w, y + h], [x, y + h]]


def test_reading_order_is_top_to_bottom_then_left_to_right():
    lines = [OCRLine("third", 0.9, _box(0, 100)), OCRLine("second-b", 0.9, _box(200, 52)),
             OCRLine("first", 0.9, _box(0, 0)), OCRLine("second-a", 0.9, _box(0, 50))]
    assert [ln.text for ln in reading_order(lines)] == ["first", "second-a", "second-b", "third"]


def test_guess_language():
    assert guess_language("مرحبا Coffee") == "ar"
    assert guess_language("Hello") == "en"
    assert guess_language("   ") is None


def test_ocr_disabled_returns_503():
    with TestClient(create_app(make_settings(ocr_engine="none"))) as c:
        assert c.get("/health").json()["ocr_engine"] == "none"
        r = c.post("/ocr", files={"file": ("x.png", b"\x89PNG", "image/png")})
        assert r.status_code == 503


@pytest.mark.skipif(importlib.util.find_spec("rapidocr") is None, reason="rapidocr not installed")
def test_ocr_reads_english_text_from_a_rendered_image():
    from PIL import Image, ImageDraw, ImageFont

    img = Image.new("RGB", (640, 120), "white")
    try:
        font = ImageFont.truetype("arial.ttf", 40)
    except OSError:
        font = ImageFont.load_default(size=40)
    ImageDraw.Draw(img).text((20, 30), "Hello VibeVoice 2026", fill="black", font=font)
    buf = io.BytesIO()
    img.save(buf, format="PNG")

    with TestClient(create_app(make_settings(ocr_engine="rapidocr"))) as c:
        r = c.post("/ocr", files={"file": ("t.png", buf.getvalue(), "image/png")}, data={"language": "en"})
    assert r.status_code == 200, r.text
    body = r.json()
    assert "VibeVoice" in body["text"]
    assert body["lines"] and body["lines"][0]["confidence"] > 0.5


def test_ocr_rejects_garbage_with_422():
    if importlib.util.find_spec("rapidocr") is None:
        pytest.skip("rapidocr not installed")
    with TestClient(create_app(make_settings(ocr_engine="rapidocr"))) as c:
        r = c.post("/ocr", files={"file": ("x.png", b"not an image at all", "image/png")}, data={"language": "en"})
    assert r.status_code == 422
