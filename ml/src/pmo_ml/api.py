"""Step 9 - Deployment: the PMO ML REST API (FastAPI).

Run:   python -m pmo_ml serve            (or: uvicorn pmo_ml.api:app --port 8001)
Docs:  http://localhost:8001/docs        (interactive Swagger UI)

The .NET API calls this service (ZR.Service/PMO/PmoMlService.cs); the browser never calls it
directly. Every endpoint takes ?lang=en|ar. If `api.api_key` is set, callers must send it in
the X-ML-Key header.

Built-in automation: a background thread runs the monitoring check every
`api.auto_retrain_hours` and retrains when drift / new data / an accuracy drop is found
(or when no model exists yet).
"""

from __future__ import annotations

import logging
import threading
import time
from contextlib import asynccontextmanager

from fastapi import Depends, FastAPI, Header, HTTPException, Query
from fastapi.staticfiles import StaticFiles
from pydantic import BaseModel, Field

from . import __version__, registry
from . import predict as pr
from .config import load_settings
from .db import describe_target
from .features import BUILDERS
from .monitor import monitoring_report
from .pipeline import history, run_pipeline
from .problems import PROBLEMS, localized

log = logging.getLogger("pmo_ml")
settings = load_settings()
data = pr.LiveData(settings)
models = pr.Models(settings)
state = {"last_check": None, "last_auto_run": None, "scheduler": "off"}


def _auto_loop(stop: threading.Event):
    hours = float(settings.section("api").get("auto_retrain_hours", 24) or 0)
    if hours <= 0:
        return
    state["scheduler"] = f"every {hours:g} h"
    while not stop.wait(hours * 3600):
        try:
            rep = monitoring_report(data.frames(refresh=True), settings)
            state["last_check"] = rep["checked_at"]
            if rep["retrain_recommended"]:
                targets = sorted({r["problem"] for r in rep["reasons"]})
                state["last_auto_run"] = run_pipeline(settings, targets, eda=False, reason="auto:" + ",".join(
                    sorted({r["reason"] for r in rep["reasons"]})))
                models.reload()
        except Exception:  # noqa: BLE001 - the scheduler must never die
            log.exception("auto-retrain check failed")


@asynccontextmanager
async def lifespan(_app: FastAPI):
    stop = threading.Event()
    api = settings.section("api")
    if api.get("train_on_startup_if_missing", True) and any(registry.current_version(settings, p) is None
                                                            for p in BUILDERS):
        def first_train():
            try:
                run_pipeline(settings, reason="startup:no_models")
                models.reload()
            except Exception:  # noqa: BLE001
                log.exception("startup training failed")
        threading.Thread(target=first_train, daemon=True).start()
    threading.Thread(target=_auto_loop, args=(stop,), daemon=True).start()
    yield
    stop.set()


app = FastAPI(title="PMO ML API", version=__version__, lifespan=lifespan,
              description="Task delay, risk severity, spend forecast, cost anomalies and portfolio segments "
                          "for the PMO Portal (scikit-learn).")
reports = settings.path("reports_dir")
reports.mkdir(parents=True, exist_ok=True)
app.mount("/reports", StaticFiles(directory=str(reports)), name="reports")   # EDA + evaluation charts


def require_key(x_ml_key: str | None = Header(default=None)):
    expected = settings.section("api").get("api_key")
    if expected and x_ml_key != expected:
        raise HTTPException(status_code=401, detail="Invalid or missing X-ML-Key")


Lang = Query("en", pattern="^(en|ar)$", description="Response language")


class RiskIn(BaseModel):
    text: str = Field(..., min_length=3, description="Risk title and/or mitigation text")
    kind: str = Field("Risk", description="Risk or Issue")
    project_id: int | None = None
    lang: str = "en"


class TrainIn(BaseModel):
    problems: list[str] | None = None
    source: str | None = Field(None, description="db or csv")
    eda: bool = True


@app.get("/health")
def health():
    try:
        target = describe_target(settings)
    except Exception as e:  # noqa: BLE001
        target = f"unavailable: {type(e).__name__}"
    return {"status": "ok", "version": __version__, "as_of": str(settings.as_of), "database": target,
            "models": {p: registry.current_version(settings, p) for p in BUILDERS},
            "scheduler": state["scheduler"], "last_check": state["last_check"]}


@app.get("/models", dependencies=[Depends(require_key)])
def list_models(lang: str = Lang):
    out = []
    for p in PROBLEMS:
        meta = registry.load_metadata(settings, p)
        item = {**localized(p, lang), "trained": meta is not None}
        if meta:
            item.update({"version": meta["version"], "trained_at": meta["trained_at"], "rows": meta["data"]["rows"],
                         "best_model": meta["best_model"], "beats_baseline": meta["beats_baseline"],
                         "metrics": {k: v for k, v in meta["metrics"].items() if k != "confusion_matrix"},
                         "baseline": meta.get("baseline"), "evaluation_strategy": meta.get("evaluation_strategy"),
                         "charts": [f"/reports/{meta['charts_dir']}/{c}" for c in meta.get("charts", {}).values()],
                         "versions": registry.versions(settings, p)[:10]})
        out.append(item)
    return {"models": out}


@app.get("/models/{problem}", dependencies=[Depends(require_key)])
def model_detail(problem: str, lang: str = Lang):
    if problem not in PROBLEMS:
        raise HTTPException(404, "Unknown problem")
    meta = registry.load_metadata(settings, problem)
    return {"definition": localized(problem, lang), "metadata": meta}


@app.get("/predict/task-delay", dependencies=[Depends(require_key)])
def predict_task_delay(project_id: int | None = None, lang: str = Lang):
    return pr.task_delay(models, data, project_id, lang)


@app.post("/predict/risk-severity", dependencies=[Depends(require_key)])
def predict_risk(body: RiskIn):
    return pr.risk_severity(models, data, body.text, body.kind, body.project_id, body.lang)


@app.get("/forecast/spend", dependencies=[Depends(require_key)])
def forecast_spend(project_id: int | None = None, lang: str = Lang):
    return pr.spend_forecast(models, data, project_id, lang)


@app.get("/anomalies/costs", dependencies=[Depends(require_key)])
def anomalies(project_id: int | None = None, top: int = Query(10, ge=1, le=100), lang: str = Lang):
    return pr.cost_anomalies(models, data, project_id, top, lang)


@app.get("/segments", dependencies=[Depends(require_key)])
def get_segments(lang: str = Lang):
    return pr.segments(models, data, lang)


@app.get("/monitoring", dependencies=[Depends(require_key)])
def monitoring(lang: str = Lang):
    rep = monitoring_report(data.frames(refresh=True), settings)
    state["last_check"] = rep["checked_at"]
    return rep


@app.get("/training/history", dependencies=[Depends(require_key)])
def training_history(limit: int = Query(20, ge=1, le=200)):
    return {"runs": history(settings, limit)}


@app.post("/train", dependencies=[Depends(require_key)])
def train(body: TrainIn | None = None):
    body = body or TrainIn()
    unknown = [p for p in body.problems or [] if p not in PROBLEMS]
    if unknown:
        raise HTTPException(400, f"Unknown problems: {unknown}")
    t = time.time()
    run = run_pipeline(settings, body.problems, body.source, eda=body.eda, reason="api")
    data.frames(refresh=True)
    models.reload()
    run["api_seconds"] = round(time.time() - t, 1)
    return run


@app.post("/models/{problem}/promote/{version}", dependencies=[Depends(require_key)])
def promote(problem: str, version: str):
    try:
        registry.promote(settings, problem, version)
    except FileNotFoundError as e:
        raise HTTPException(404, str(e)) from e
    models.reload()
    return {"problem": problem, "version": version}
