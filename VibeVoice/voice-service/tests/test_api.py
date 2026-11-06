import io
import wave

from fastapi.testclient import TestClient

from app.main import create_app
from tests.conftest import make_settings


def _wav_info(data: bytes) -> tuple[int, float]:
    assert data[:4] == b"RIFF" and data[8:12] == b"WAVE"
    with wave.open(io.BytesIO(data)) as w:
        assert w.getnchannels() == 1 and w.getsampwidth() == 2
        return w.getframerate(), w.getnframes() / w.getframerate()


def test_health(client):
    r = client.get("/health")
    assert r.status_code == 200
    body = r.json()
    assert body["status"] == "ok"
    assert body["tts_engine"] == "dev"
    assert body["arabic_tts_engine"] == "dev"
    assert body["asr_engine"] == "none"
    assert body["model_loaded"] is True
    assert "device" in body


def test_voices(client):
    r = client.get("/voices")
    assert r.status_code == 200
    voices = r.json()
    ids = {v["id"] for v in voices}
    assert {"dev-en", "dev-ar"} <= ids
    for v in voices:
        assert set(v) == {"id", "name", "language", "gender", "engine", "is_fallback"}
    assert any(v["language"] == "ar" for v in voices)


def test_tts_english(client):
    r = client.post("/tts", json={"text": "Hello world. This is a test."})
    assert r.status_code == 200
    assert r.headers["content-type"] == "audio/wav"
    assert r.headers["x-ai-generated"] == "true"
    assert r.headers["x-engine"] == "dev"
    assert r.headers["x-language"] == "en"
    assert r.headers["x-voice"] == "dev-en"
    rate, dur = _wav_info(r.content)
    assert rate == 24000
    assert abs(dur - float(r.headers["x-duration-seconds"])) < 0.02


def test_tts_arabic_detected(client):
    r = client.post("/tts", json={"text": "مرحبا بكم في تطبيق فايب فويس. كيف حالك؟"})
    assert r.status_code == 200
    assert r.headers["x-language"] == "ar"
    assert r.headers["x-voice"] == "dev-ar"
    _wav_info(r.content)


def test_tts_explicit_voice_and_html(client):
    r = client.post("/tts", json={"text": "<p>Hello <b>there</b></p>", "voice": "dev-ar", "language": "en"})
    assert r.status_code == 200
    assert r.headers["x-voice"] == "dev-ar"
    assert r.headers["x-language"] == "en"


def test_longer_text_gives_longer_audio(client):
    short = client.post("/tts", json={"text": "Short sentence."})
    long = client.post("/tts", json={"text": "This is a much longer sentence. " * 10})
    assert float(long.headers["x-duration-seconds"]) > float(short.headers["x-duration-seconds"]) * 3
    assert _wav_info(long.content)[1] > _wav_info(short.content)[1]


def test_tts_deterministic(client):
    a = client.post("/tts", json={"text": "same input"}).content
    b = client.post("/tts", json={"text": "same input"}).content
    assert a == b


def test_tts_empty_text_422(client):
    assert client.post("/tts", json={"text": "   "}).status_code == 422
    assert client.post("/tts", json={"text": "<p> </p>"}).status_code == 422
    assert client.post("/tts", json={}).status_code == 422


def test_tts_too_long_422():
    with TestClient(create_app(make_settings(max_tts_chars=20))) as c:
        r = c.post("/tts", json={"text": "x" * 21})
        assert r.status_code == 422
        assert "too long" in r.json()["detail"]


def test_tts_bad_format_422(client):
    assert client.post("/tts", json={"text": "hi", "format": "ogg"}).status_code == 422


def test_tts_mp3_without_ffmpeg_400():
    with TestClient(create_app(make_settings(ffmpeg_binary="definitely-not-ffmpeg-xyz"))) as c:
        r = c.post("/tts", json={"text": "hello", "format": "mp3"})
        assert r.status_code == 400
        assert "ffmpeg" in r.json()["detail"]


def test_unknown_voice_falls_back_to_default(client):
    r = client.post("/tts", json={"text": "hello", "voice": "no-such-voice"})
    assert r.status_code == 200
    assert r.headers["x-voice"] == "dev-en"


def test_transcribe_disabled_503(client):
    r = client.post("/transcribe", files={"file": ("a.wav", b"RIFF0000WAVE", "audio/wav")})
    assert r.status_code == 503
    assert r.json() == {"detail": "ASR disabled (ASR_ENGINE=none)"}


def test_vibevoice_missing_falls_back():
    """TTS_ENGINE=vibevoice without the package/presets must fall back (here to dev) and say so."""
    s = make_settings(tts_engine="vibevoice", fallback_tts_engine="dev", vibevoice_voices_dir="does-not-exist")
    with TestClient(create_app(s)) as c:
        body = c.get("/health").json()
        assert body["tts_engine"] == "dev"
        assert body["requested_tts_engine"] == "vibevoice"
        assert any("vibevoice" in n for n in body["notes"])
        assert c.post("/tts", json={"text": "fallback works"}).status_code == 200


def test_vibevoice_asr_missing_falls_back_to_whisper():
    import importlib.util

    import pytest

    if importlib.util.find_spec("vibevoice") is not None or importlib.util.find_spec("faster_whisper") is None:
        pytest.skip("needs faster-whisper installed and vibevoice absent")
    with TestClient(create_app(make_settings(asr_engine="vibevoice"))) as c:
        body = c.get("/health").json()
        assert body["asr_engine"] == "whisper"
        assert body["asr_model_loaded"] is False  # lazy: nothing loaded until /transcribe
