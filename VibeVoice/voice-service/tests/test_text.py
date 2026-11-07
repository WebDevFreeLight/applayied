from app.engines.vibevoice_asr import map_segments
from app.engines.vibevoice_tts import parse_voice_id
from app.text import chunk_text, clean_text, detect_language, normalize_language, split_sentences


def test_detect_language():
    assert detect_language("Hello there, how are you?") == "en"
    assert detect_language("مرحبا بكم في تطبيق فايب فويس") == "ar"
    assert detect_language("Meeting with أحمد tomorrow at the office about the project") == "en"
    assert detect_language("اجتماع مع Ahmed غدا في المكتب") == "ar"
    assert detect_language("") == "en"
    assert detect_language("12345 !!!") == "en"


def test_normalize_language():
    assert normalize_language("ar-SA") == "ar"
    assert normalize_language("EN") == "en"
    assert normalize_language("auto") is None
    assert normalize_language(None) is None


def test_split_english():
    s = split_sentences("Hello world. How are you? I'm fine!\nNew line here")
    assert s == ["Hello world.", "How are you?", "I'm fine!", "New line here"]


def test_split_keeps_decimals():
    assert split_sentences("Pi is 3.14 roughly. Yes.") == ["Pi is 3.14 roughly.", "Yes."]


def test_split_arabic():
    s = split_sentences("مرحبا بكم. كيف حالك؟ أنا بخير، شكرا؛ إلى اللقاء!")
    assert s == ["مرحبا بكم.", "كيف حالك؟", "أنا بخير، شكرا؛", "إلى اللقاء!"]


def test_long_sentence_split_on_arabic_comma_and_limit():
    text = "، ".join(["كلمة عربية طويلة جدا"] * 40)  # one sentence, ~800 chars
    parts = split_sentences(text, max_chars=100)
    assert len(parts) > 1
    assert all(len(p) <= 100 for p in parts)
    assert "".join(p.replace(" ", "") for p in parts) == text.replace(" ", "")


def test_word_without_spaces_hard_cut():
    parts = split_sentences("a" * 950, max_chars=400)
    assert [len(p) for p in parts] == [400, 400, 150]


def test_chunk_text_packs_sentences():
    text = " ".join(f"Sentence number {i}." for i in range(50))
    chunks = chunk_text(text, max_chars=120)
    assert all(len(c) <= 120 for c in chunks)
    assert len(chunks) < 50
    assert " ".join(chunks) == text


def test_clean_text_strips_html():
    html = "<h1>Title</h1><p>Hello <b>world</b> &amp; friends</p><script>alert(1)</script><ul><li>One</li></ul>"
    assert clean_text(html) == "Title\nHello world & friends\nOne"


def test_parse_voice_id():
    v = parse_voice_id("en-Carter_man")
    assert (v.name, v.language, v.gender, v.engine, v.is_fallback) == ("Carter (en, male)", "en", "male", "vibevoice", False)
    assert parse_voice_id("sp-Spk0_woman").language == "es"
    assert parse_voice_id("sp-Spk0_woman").name == "Speaker 0 (es, female)"
    assert parse_voice_id("jp-Spk1_woman").language == "ja"
    assert parse_voice_id("kr-Spk1_man").language == "ko"
    assert parse_voice_id("in-Samuel_man").language == "en"


def test_map_asr_segments_defensively():
    segs = map_segments([
        {"start_time": "0.5", "end_time": 1.25, "content": " hi ", "speaker_id": 0},
        {"start": 2, "end": 3, "text": "there"},
        "loose text",
        42,
    ])
    assert [(s.start, s.end, s.text, s.speaker) for s in segs] == [
        (0.5, 1.25, "hi", "0"), (2.0, 3.0, "there", None), (0.0, 0.0, "loose text", None)]
    assert map_segments(None) == []


def test_extract_audio_to_wav_round_trips_a_file(tmp_path):
    """Video uploads go through this helper; a WAV input must survive it with the same length."""
    import wave

    import numpy as np

    from app.audio import extract_audio_to_wav, find_ffmpeg, pcm_to_wav_bytes

    src = tmp_path / "in.wav"
    src.write_bytes(pcm_to_wav_bytes(np.zeros(16_000, dtype=np.float32), 16_000))
    dst = tmp_path / "out.wav"
    assert extract_audio_to_wav(str(src), str(dst), 16_000, find_ffmpeg())
    with wave.open(str(dst)) as w:
        assert w.getframerate() == 16_000
        assert abs(w.getnframes() - 16_000) < 200


def test_extract_audio_to_wav_rejects_non_media(tmp_path):
    from app.audio import extract_audio_to_wav, find_ffmpeg

    src = tmp_path / "x.mp4"
    src.write_bytes(b"definitely not a video")
    assert not extract_audio_to_wav(str(src), str(tmp_path / "o.wav"), 16_000, find_ffmpeg())
