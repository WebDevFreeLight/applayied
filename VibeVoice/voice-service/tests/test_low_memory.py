"""LOW_MEMORY mode: heavy requests (transcription / OCR) never overlap.

Two phone recordings transcribed side by side pushed a 512 MB host past its limit (OOM-killed); the
service now runs one at a time, decoding included.
"""

import asyncio
import threading
import time

import httpx

from app.engines.base import Segment, Transcription
from app.main import create_app
from tests.conftest import make_settings


class SlowASR:
    """Stands in for Whisper: records how many transcriptions run at the same time."""

    name = "fake"
    model_loaded = True

    def __init__(self) -> None:
        self.active = 0
        self.peak = 0
        self._lock = threading.Lock()

    def availability(self):
        return True, None

    def transcribe(self, path, language):
        with self._lock:
            self.active += 1
            self.peak = max(self.peak, self.active)
        time.sleep(0.2)
        with self._lock:
            self.active -= 1
        return Transcription(text="مرحبا", language="ar", segments=[Segment(0.0, 1.0, "مرحبا")], duration=1.0)


async def _transcribe_twice(low_memory: bool) -> int:
    app = create_app(make_settings(asr_engine="whisper", ocr_engine="none", low_memory=low_memory))
    fake = SlowASR()
    app.state.asr.engine = fake
    transport = httpx.ASGITransport(app=app)
    async with httpx.AsyncClient(transport=transport, base_url="http://test") as client:
        async def send(i: int):
            files = {"file": (f"recording-{i}.webm", b"\x1aE\xdf\xa3" + b"\0" * 64, "audio/webm")}
            return await client.post("/transcribe", files=files)

        responses = await asyncio.gather(send(1), send(2))
    assert [r.status_code for r in responses] == [200, 200]
    assert all(r.json()["text"] == "مرحبا" for r in responses)
    return fake.peak


def test_low_memory_runs_one_transcription_at_a_time():
    assert asyncio.run(_transcribe_twice(low_memory=True)) == 1


def test_without_low_memory_transcriptions_may_overlap():
    assert asyncio.run(_transcribe_twice(low_memory=False)) == 2
