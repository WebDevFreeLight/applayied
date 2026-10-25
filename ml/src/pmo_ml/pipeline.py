"""End-to-end pipeline used by the CLI, the API's /train endpoint and the auto-retrain scheduler."""

from __future__ import annotations

import json
import threading
import time
from datetime import datetime

from .clean import clean
from .collect import collect, snapshot
from .config import Settings
from .eda import run_eda
from .train import train_all

_LOCK = threading.Lock()          # one training run at a time


def run_pipeline(settings: Settings, problems: list[str] | None = None, source: str | None = None,
                 eda: bool = True, keep_snapshot: bool = True, reason: str = "manual") -> dict:
    if not _LOCK.acquire(blocking=False):
        return {"status": "busy", "message": "A training run is already in progress."}
    started = time.time()
    try:
        src = source or settings.section("data").get("source", "db")
        raw = collect(settings, src)
        frames, cleaning = clean(raw)
        snap = str(snapshot(raw, settings)) if keep_snapshot else None
        if eda:
            run_eda(frames, settings, problems)
        results = train_all(frames, settings, problems, data_info={"source": src, "snapshot": snap})
        run = {"status": "ok", "reason": reason, "started_at": datetime.fromtimestamp(started).isoformat(timespec="seconds"),
               "seconds": round(time.time() - started, 1), "as_of": str(settings.as_of), "source": src,
               "snapshot": snap, "cleaning": cleaning, "results": results}
        _append_history(settings, run)
        return run
    finally:
        _LOCK.release()


def _append_history(settings: Settings, run: dict) -> None:
    path = settings.path("reports_dir") / "training_history.jsonl"
    path.parent.mkdir(parents=True, exist_ok=True)
    slim = {k: v for k, v in run.items() if k != "cleaning"}
    with open(path, "a", encoding="utf-8") as f:
        f.write(json.dumps(slim, default=str, ensure_ascii=False) + "\n")


def history(settings: Settings, limit: int = 20) -> list[dict]:
    path = settings.path("reports_dir") / "training_history.jsonl"
    if not path.exists():
        return []
    lines = path.read_text(encoding="utf-8").strip().splitlines()[-limit:]
    return [json.loads(x) for x in reversed(lines)]
