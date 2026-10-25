"""Command line: python -m pmo_ml <command>

  collect    snapshot the PMO tables to data/raw/
  eda        write EDA charts + EDA.md to reports/eda/
  train      run the full pipeline and register new models   [--problem X ...] [--source db|csv]
  evaluate   print the metrics of the current models
  predict    print live predictions for one problem           <problem> [--project ID] [--lang ar]
  monitor    drift / new-data / live-accuracy report (exit code 3 = retrain recommended)
  promote    serve an older version again                     <problem> <version>
  serve      start the REST API (FastAPI + uvicorn)
"""

from __future__ import annotations

import argparse
import json
import sys

from .config import load_settings

PROBLEM_CHOICES = ["task_delay", "risk_severity", "spend_forecast", "cost_anomaly", "portfolio_segments"]


def _print(obj) -> None:
    sys.stdout.reconfigure(encoding="utf-8")
    print(json.dumps(obj, indent=2, ensure_ascii=False, default=str))


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(prog="pmo_ml", description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--config", help="path to config.yaml")
    sub = ap.add_subparsers(dest="cmd", required=True)
    sub.add_parser("collect").add_argument("--source", choices=["db", "csv"])
    sub.add_parser("eda").add_argument("--source", choices=["db", "csv"])
    t = sub.add_parser("train")
    t.add_argument("--problem", action="append", choices=PROBLEM_CHOICES)
    t.add_argument("--source", choices=["db", "csv"])
    t.add_argument("--no-eda", action="store_true")
    sub.add_parser("evaluate")
    p = sub.add_parser("predict")
    p.add_argument("problem", choices=PROBLEM_CHOICES)
    p.add_argument("--project", type=int)
    p.add_argument("--text", help="risk text (risk_severity)")
    p.add_argument("--lang", default="en", choices=["en", "ar"])
    sub.add_parser("monitor")
    pm = sub.add_parser("promote")
    pm.add_argument("problem", choices=PROBLEM_CHOICES)
    pm.add_argument("version")
    s = sub.add_parser("serve")
    s.add_argument("--host")
    s.add_argument("--port", type=int)
    s.add_argument("--reload", action="store_true")
    args = ap.parse_args(argv)
    settings = load_settings(args.config)

    if args.cmd == "collect":
        from .collect import collect, snapshot
        frames = collect(settings, args.source)
        _print({"snapshot": str(snapshot(frames, settings)), "rows": {k: len(v) for k, v in frames.items()}})
    elif args.cmd == "eda":
        from .clean import clean
        from .collect import collect
        from .eda import run_eda
        frames, _ = clean(collect(settings, args.source))
        res = run_eda(frames, settings)
        _print({k: (v.get("findings") if isinstance(v, dict) else v) for k, v in res.items()})
        print(f"\nReport: {settings.path('reports_dir') / 'eda' / 'EDA.md'}")
    elif args.cmd == "train":
        from .pipeline import run_pipeline
        run = run_pipeline(settings, args.problem, args.source, eda=not args.no_eda, reason="cli")
        _print({k: v for k, v in run.items() if k != "cleaning"})
        return 0 if run.get("status") == "ok" else 1
    elif args.cmd == "evaluate":
        from . import registry
        out = {}
        for prob in PROBLEM_CHOICES:
            m = registry.load_metadata(settings, prob)
            out[prob] = None if not m else {"version": m["version"], "best_model": m["best_model"],
                                            "strategy": m.get("evaluation_strategy"), "beats_baseline": m["beats_baseline"],
                                            "metrics": {k: v for k, v in m["metrics"].items() if k != "confusion_matrix"},
                                            "baseline": m.get("baseline", {}).get("metrics")}
        _print(out)
    elif args.cmd == "predict":
        from . import predict as pr
        models, data = pr.Models(settings), pr.LiveData(settings)
        fn = {"task_delay": lambda: pr.task_delay(models, data, args.project, args.lang, log=False),
              "risk_severity": lambda: pr.risk_severity(models, data, args.text or "Vendor delay on hardware delivery",
                                                        "Risk", args.project, args.lang),
              "spend_forecast": lambda: pr.spend_forecast(models, data, args.project, args.lang),
              "cost_anomaly": lambda: pr.cost_anomalies(models, data, args.project, 10, args.lang),
              "portfolio_segments": lambda: pr.segments(models, data, args.lang)}[args.problem]
        _print(fn())
    elif args.cmd == "monitor":
        from .clean import clean
        from .collect import collect
        from .monitor import monitoring_report
        rep = monitoring_report(clean(collect(settings))[0], settings)
        _print(rep)
        return 3 if rep["retrain_recommended"] else 0
    elif args.cmd == "promote":
        from . import registry
        registry.promote(settings, args.problem, args.version)
        _print({"problem": args.problem, "serving": args.version})
    elif args.cmd == "serve":
        import uvicorn
        api = settings.section("api")
        uvicorn.run("pmo_ml.api:app", host=args.host or api.get("host", "0.0.0.0"),
                    port=args.port or int(api.get("port", 8001)), reload=args.reload)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
