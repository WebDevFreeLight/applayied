"""Shared fixtures: an isolated config that trains on the CSV seed into a temp folder,
so tests never touch the real database or the real models/ directory."""

from __future__ import annotations

import os
from pathlib import Path

import pytest
import yaml

from pmo_ml.config import DEFAULT_CONFIG, ML_ROOT, load_settings


@pytest.fixture(scope="session")
def test_config(tmp_path_factory) -> Path:
    tmp = tmp_path_factory.mktemp("pmo_ml")
    raw = yaml.safe_load(DEFAULT_CONFIG.read_text(encoding="utf-8"))
    raw["data"]["source"] = "csv"
    raw["data"]["csv_dir"] = str((ML_ROOT / ".." / "data" / "csv").resolve())
    raw["data"]["as_of_date"] = "2026-09-30"
    raw["paths"] = {k: str(tmp / k) for k in ("models_dir", "reports_dir", "raw_dir")}
    raw["api"]["auto_retrain_hours"] = 0
    raw["api"]["train_on_startup_if_missing"] = False
    cfg = tmp / "config.yaml"
    cfg.write_text(yaml.safe_dump(raw), encoding="utf-8")
    os.environ["PMO_ML_CONFIG"] = str(cfg)
    return cfg


@pytest.fixture(scope="session")
def settings(test_config):
    return load_settings(test_config)


@pytest.fixture(scope="session")
def frames(settings):
    from pmo_ml.clean import clean
    from pmo_ml.collect import collect
    return clean(collect(settings, "csv"))[0]


@pytest.fixture(scope="session")
def trained(settings):
    from pmo_ml.pipeline import run_pipeline
    run = run_pipeline(settings, source="csv", eda=True, reason="test")
    assert run["status"] == "ok", run
    return run
