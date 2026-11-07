#!/usr/bin/env bash
# Starts voice-service (:8001), api (:5080) and frontend (:5173). Ctrl+C stops all three.
#   ./dev.sh            all three
#   ./dev.sh --no-voice skip the Python service (TTS/ASR return 503, everything else works)
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
pids=()
cleanup() { kill "${pids[@]}" 2>/dev/null || true; }
trap cleanup EXIT INT TERM

if [[ "${1:-}" != "--no-voice" ]]; then
  cd "$ROOT/voice-service"
  if [[ ! -d .venv ]]; then
    python3 -m venv .venv
    .venv/bin/pip install -r requirements.txt
  fi
  PY=.venv/bin/python; [[ -x $PY ]] || PY=.venv/Scripts/python.exe
  "$PY" -m uvicorn app.main:app --host 127.0.0.1 --port 8001 &
  pids+=($!)
fi

(cd "$ROOT/backend/VibeVoice.Api" && dotnet run --launch-profile http) &
pids+=($!)

cd "$ROOT/frontend"
[[ -d node_modules ]] || npm install
npm run dev &
pids+=($!)

echo
echo "VibeVoice is starting:"
echo "  App   http://localhost:5173   (demo: admin@vibevoice.local / Admin123!)"
echo "  API   http://localhost:5080/swagger"
wait
