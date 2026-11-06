#!/usr/bin/env bash
# Installs Microsoft VibeVoice into voice-service/.venv and copies the official voice presets.
# Usage: bash voice-service/scripts/setup_vibevoice.sh [--cuda cu124] [--download-model]
set -euo pipefail

CUDA=""
DOWNLOAD_MODEL=0
while [[ $# -gt 0 ]]; do
  case "$1" in
    --cuda) CUDA="$2"; shift 2 ;;
    --download-model) DOWNLOAD_MODEL=1; shift ;;
    *) echo "unknown arg: $1" >&2; exit 2 ;;
  esac
done

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"   # voice-service/
REPO="$ROOT/third_party/VibeVoice"
if [[ -x "$ROOT/.venv/bin/python" ]]; then PY="$ROOT/.venv/bin/python";
elif [[ -x "$ROOT/.venv/Scripts/python.exe" ]]; then PY="$ROOT/.venv/Scripts/python.exe";
else
  echo "Creating venv at $ROOT/.venv"
  python3 -m venv "$ROOT/.venv"
  PY="$ROOT/.venv/bin/python"
  "$PY" -m pip install --upgrade pip
  "$PY" -m pip install -r "$ROOT/requirements.txt"
fi

mkdir -p "$ROOT/third_party"
if [[ -d "$REPO/.git" ]]; then
  git -C "$REPO" pull --ff-only
else
  git clone --depth 1 https://github.com/microsoft/VibeVoice.git "$REPO"
fi

if [[ -n "$CUDA" ]]; then
  "$PY" -m pip install torch --index-url "https://download.pytorch.org/whl/$CUDA"
fi
"$PY" -m pip install -e "$REPO[streamingtts]"

mkdir -p "$ROOT/voices/streaming_model"
cp -f "$REPO"/demo/voices/streaming_model/*.pt "$ROOT/voices/streaming_model/"
echo "Copied $(ls "$ROOT/voices/streaming_model"/*.pt | wc -l) official voice presets"

if [[ "$DOWNLOAD_MODEL" == 1 ]]; then
  BIN="$(dirname "$PY")"
  # huggingface_hub >= 1.0 ships the `hf` CLI; older versions only have `huggingface-cli`.
  if [[ -x "$BIN/hf" || -x "$BIN/hf.exe" ]]; then HF="$BIN/hf"; else HF="huggingface-cli"; fi
  "$HF" download microsoft/VibeVoice-Realtime-0.5B --local-dir "$ROOT/models/VibeVoice-Realtime-0.5B"
  echo "Set VIBEVOICE_MODEL_PATH=models/VibeVoice-Realtime-0.5B in .env"
fi
echo "Done. Start with: TTS_ENGINE=vibevoice .venv/bin/python -m uvicorn app.main:app --port 8001"
