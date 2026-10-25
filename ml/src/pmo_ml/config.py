"""Load config/config.yaml and apply environment-variable overrides."""

from __future__ import annotations

import os
from dataclasses import dataclass, field
from datetime import date, datetime
from pathlib import Path
from typing import Any

import yaml

ML_ROOT = Path(__file__).resolve().parents[2]          # .../ml
DEFAULT_CONFIG = ML_ROOT / "config" / "config.yaml"

# env var -> (section, key, caster)
_ENV_OVERRIDES = {
    "PMO_ML_SOURCE": ("data", "source", str),
    "PMO_ML_APPSETTINGS": ("data", "appsettings_path", str),
    "PMO_ML_AS_OF": ("data", "as_of_date", str),
    "PMO_ML_HOST": ("api", "host", str),
    "PMO_ML_PORT": ("api", "port", int),
    "PMO_ML_API_KEY": ("api", "api_key", str),
    "PMO_ML_AUTO_RETRAIN_HOURS": ("api", "auto_retrain_hours", float),
}


@dataclass
class Settings:
    raw: dict[str, Any] = field(default_factory=dict)

    def section(self, name: str) -> dict[str, Any]:
        return self.raw.get(name) or {}

    def path(self, key: str) -> Path:
        """A paths.* entry resolved relative to the ml/ folder."""
        return resolve(self.section("paths")[key])

    @property
    def as_of(self) -> date:
        value = self.section("data").get("as_of_date")
        if not value:
            return date.today()
        if isinstance(value, date):
            return value
        return datetime.strptime(str(value), "%Y-%m-%d").date()

    @property
    def random_state(self) -> int:
        return int(self.section("training").get("random_state", 42))


def resolve(p: str | Path) -> Path:
    p = Path(p)
    return p if p.is_absolute() else (ML_ROOT / p).resolve()


def load_settings(path: str | Path | None = None) -> Settings:
    cfg_path = Path(path or os.environ.get("PMO_ML_CONFIG") or DEFAULT_CONFIG)
    with open(cfg_path, encoding="utf-8") as f:
        raw = yaml.safe_load(f) or {}
    for env, (section, key, cast) in _ENV_OVERRIDES.items():
        value = os.environ.get(env)
        if value not in (None, ""):
            raw.setdefault(section, {})[key] = cast(value)
    return Settings(raw)
