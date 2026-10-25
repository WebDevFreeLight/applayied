"""Model registry: models/<problem>/<version>/{model.joblib, metadata.json} + current.json pointer.

Versions are timestamps. `current.json` names the version the API serves, so a rollback is
`python -m pmo_ml promote <problem> <version>`.
"""

from __future__ import annotations

import json
from datetime import datetime
from pathlib import Path

import joblib

from .config import Settings


def _root(settings: Settings, problem: str) -> Path:
    return settings.path("models_dir") / problem


def new_version() -> str:
    return datetime.now().strftime("%Y%m%d-%H%M%S")


def save(settings: Settings, problem: str, model, metadata: dict, version: str) -> Path:
    vdir = _root(settings, problem) / version
    vdir.mkdir(parents=True, exist_ok=True)
    joblib.dump(model, vdir / "model.joblib")
    (vdir / "metadata.json").write_text(json.dumps(metadata, indent=2, ensure_ascii=False, default=str),
                                        encoding="utf-8")
    promote(settings, problem, version)
    return vdir


def promote(settings: Settings, problem: str, version: str) -> None:
    vdir = _root(settings, problem) / version
    if not (vdir / "model.joblib").exists():
        raise FileNotFoundError(f"No model {problem}/{version}")
    (_root(settings, problem) / "current.json").write_text(json.dumps({"version": version}), encoding="utf-8")


def current_version(settings: Settings, problem: str) -> str | None:
    p = _root(settings, problem) / "current.json"
    return json.loads(p.read_text(encoding="utf-8"))["version"] if p.exists() else None


def load_metadata(settings: Settings, problem: str, version: str | None = None) -> dict | None:
    version = version or current_version(settings, problem)
    if not version:
        return None
    p = _root(settings, problem) / version / "metadata.json"
    return json.loads(p.read_text(encoding="utf-8")) if p.exists() else None


def load(settings: Settings, problem: str, version: str | None = None):
    """(model, metadata) for the current (or given) version, or (None, None)."""
    version = version or current_version(settings, problem)
    if not version:
        return None, None
    vdir = _root(settings, problem) / version
    if not (vdir / "model.joblib").exists():
        return None, None
    return joblib.load(vdir / "model.joblib"), load_metadata(settings, problem, version)


def versions(settings: Settings, problem: str) -> list[str]:
    root = _root(settings, problem)
    return sorted((p.name for p in root.iterdir() if p.is_dir()), reverse=True) if root.exists() else []
