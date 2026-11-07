"""Text helpers: HTML stripping, language detection and sentence chunking (Arabic + Latin)."""

from __future__ import annotations

import html
import re

_ARABIC_RE = re.compile(r"[؀-ۿݐ-ݿࢠ-ࣿﭐ-﷿ﹰ-﻿]")

_BLOCK_TAG_RE = re.compile(r"<\s*(br|/p|/div|/li|/h[1-6]|/blockquote|/tr|p|div|li|h[1-6])\b[^>]*>", re.I)
_DROP_BLOCK_RE = re.compile(r"<\s*(script|style)\b[^>]*>.*?<\s*/\s*\1\s*>", re.I | re.S)
_TAG_RE = re.compile(r"<[^>]+>")
_SPACES_RE = re.compile(r"[ \t ​]+")

# Hard sentence ends: Latin . ! ?, ellipsis, Arabic question mark ؟ and semicolon ؛, CJK 。！？
_SENTENCE_END_RE = re.compile(r"(?<=[.!?…؟؛。！？])[\"'”’»)\]]*\s+")
# Soft break points used only when a sentence is longer than the chunk limit.
_SOFT_BREAK_RE = re.compile(r"(?<=[,،;:،、])\s+")


def strip_html(text: str) -> str:
    """Remove HTML tags (TipTap output etc.), keeping block boundaries as newlines."""
    if "<" not in text and "&" not in text:
        return text
    text = _DROP_BLOCK_RE.sub(" ", text)
    text = _BLOCK_TAG_RE.sub("\n", text)
    text = _TAG_RE.sub("", text)
    return html.unescape(text)


def normalize_whitespace(text: str) -> str:
    lines = [_SPACES_RE.sub(" ", ln).strip() for ln in text.replace("\r\n", "\n").replace("\r", "\n").split("\n")]
    # blank lines carry no meaning for speech (each line is already a sentence boundary)
    return "\n".join(ln for ln in lines if ln)


def clean_text(text: str) -> str:
    return normalize_whitespace(strip_html(text or ""))


def normalize_quotes(text: str) -> str:
    """Upstream VibeVoice normalisation: curly quotes -> straight quotes."""
    return text.replace("’", "'").replace("‘", "'").replace("“", '"').replace("”", '"')


def arabic_ratio(text: str) -> float:
    letters = [c for c in text if c.isalpha()]
    if not letters:
        return 0.0
    return sum(1 for c in letters if _ARABIC_RE.match(c)) / len(letters)


def detect_language(text: str) -> str:
    """Script-based detection: Arabic-script letters > 30% -> ``ar`` else ``en``."""
    return "ar" if arabic_ratio(text) > 0.3 else "en"


def normalize_language(language: str | None) -> str | None:
    """``"ar-SA"`` -> ``"ar"``; ``""``/``"auto"`` -> ``None``."""
    if not language:
        return None
    lang = language.strip().lower().replace("_", "-").split("-")[0]
    return None if lang in ("", "auto") else lang


def _split_long(sentence: str, max_chars: int) -> list[str]:
    """Split an over-long sentence on soft punctuation, then whitespace, then hard-cut."""
    if len(sentence) <= max_chars:
        return [sentence]
    parts: list[str] = []
    for piece in _SOFT_BREAK_RE.split(sentence):
        if len(piece) <= max_chars:
            parts.append(piece)
            continue
        current = ""
        for word in piece.split(" "):
            while len(word) > max_chars:  # pathological token without spaces
                if current:
                    parts.append(current)
                    current = ""
                parts.append(word[:max_chars])
                word = word[max_chars:]
            if current and len(current) + 1 + len(word) > max_chars:
                parts.append(current)
                current = word
            else:
                current = f"{current} {word}" if current else word
        if current:
            parts.append(current)
    return _merge(parts, max_chars)


def _merge(parts: list[str], max_chars: int) -> list[str]:
    out: list[str] = []
    for p in (p.strip() for p in parts):
        if not p:
            continue
        if out and len(out[-1]) + 1 + len(p) <= max_chars:
            out[-1] = f"{out[-1]} {p}"
        else:
            out.append(p)
    return out


def split_sentences(text: str, max_chars: int = 400) -> list[str]:
    """Split into sentences (newline and . ! ? … ؟ ؛ boundaries); no piece exceeds ``max_chars``."""
    sentences: list[str] = []
    for line in text.split("\n"):
        line = line.strip()
        if not line:
            continue
        for s in _SENTENCE_END_RE.split(line):
            s = s.strip()
            if s:
                sentences.extend(_split_long(s, max_chars))
    return sentences


def chunk_text(text: str, max_chars: int = 400) -> list[str]:
    """Sentences greedily packed into chunks of at most ``max_chars`` characters."""
    return _merge(split_sentences(text, max_chars), max_chars)
