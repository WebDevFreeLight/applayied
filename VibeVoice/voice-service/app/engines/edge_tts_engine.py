"""Microsoft Edge neural voices via the ``edge-tts`` package (needs internet).

Used as the Arabic engine (VibeVoice-Realtime ships no Arabic voice preset) and as the general
fallback when VibeVoice is not installed. All voices are marked ``is_fallback=true``.
edge-tts returns MP3; it is decoded to PCM with ffmpeg (or PyAV). If neither is available the
MP3 bytes are passed through unchanged and the response is served as ``audio/mpeg``.
"""

from __future__ import annotations

import asyncio
import importlib.util
import logging
import re

from app.audio import DEFAULT_SAMPLE_RATE, AudioResult, decode_to_pcm, find_ffmpeg
from app.config import Settings
from app.engines.base import TTSEngine, Voice

log = logging.getLogger(__name__)

_VOICES = [
    Voice("ar-SA-HamedNeural", "Hamed (ar-SA, male)", "ar", "male", "edge", True),
    Voice("ar-SA-ZariyahNeural", "Zariyah (ar-SA, female)", "ar", "female", "edge", True),
    Voice("ar-EG-SalmaNeural", "Salma (ar-EG, female)", "ar", "female", "edge", True),
    Voice("ar-EG-ShakirNeural", "Shakir (ar-EG, male)", "ar", "male", "edge", True),
    Voice("en-US-AriaNeural", "Aria (en-US, female)", "en", "female", "edge", True),
    Voice("en-US-GuyNeural", "Guy (en-US, male)", "en", "male", "edge", True),
    Voice("en-GB-SoniaNeural", "Sonia (en-GB, female)", "en", "female", "edge", True),
]
# Any Edge neural voice id (e.g. "fr-FR-DeniseNeural") is accepted even if not listed above.
_EDGE_ID_RE = re.compile(r"^[a-z]{2,3}-[A-Z]{2}(-[A-Za-z]+)?-[A-Za-z]+Neural$")


class EdgeTTSEngine(TTSEngine):
    name = "edge"
    is_fallback = True

    def __init__(self, settings: Settings) -> None:
        super().__init__()
        self.settings = settings
        self.sample_rate = DEFAULT_SAMPLE_RATE

    def availability(self) -> tuple[bool, str | None]:
        if importlib.util.find_spec("edge_tts") is None:
            return False, "edge-tts package not installed"
        return True, None

    @property
    def device(self) -> str:
        return "remote"

    def list_voices(self) -> list[Voice]:
        return list(_VOICES)

    def owns_voice(self, voice_id: str) -> bool:
        return any(v.id == voice_id for v in _VOICES) or bool(_EDGE_ID_RE.match(voice_id))

    def default_voice(self, language: str) -> str:
        if language == "ar":
            return self.settings.edge_voice_ar
        if language == "en":
            return self.settings.edge_voice_en
        for v in _VOICES:
            if v.language == language:
                return v.id
        return self.settings.edge_voice_en

    async def _fetch(self, text: str, voice: str) -> bytes:
        import edge_tts

        communicate = edge_tts.Communicate(text, voice)
        data = bytearray()
        async for chunk in communicate.stream():
            if chunk.get("type") == "audio" and chunk.get("data"):
                data.extend(chunk["data"])
        return bytes(data)

    def synthesize(self, text: str, voice: str, language: str) -> AudioResult:
        # Runs in a worker thread (no running event loop) -> own loop per call.
        mp3 = asyncio.run(asyncio.wait_for(self._fetch(text, voice), self.settings.edge_timeout_seconds))
        if not mp3:
            raise RuntimeError(f"edge-tts returned no audio for voice {voice!r}")
        pcm = decode_to_pcm(mp3, self.sample_rate, find_ffmpeg(self.settings.ffmpeg_binary))
        if pcm is None:
            log.warning("No ffmpeg/PyAV available: passing edge-tts MP3 through undecoded")
            return AudioResult(pcm=None, sample_rate=self.sample_rate, encoded=mp3, encoded_format="mp3")
        return AudioResult(pcm=pcm, sample_rate=self.sample_rate)
