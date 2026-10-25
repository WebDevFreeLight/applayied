import pandas as pd

from pmo_ml.features import BUILDERS, severity_band, task_outcomes

LEAKY = {"status", "progress_pct", "days_overdue", "score", "probability", "impact"}


def test_no_leaky_features(frames, settings):
    for name in ("task_delay", "risk_severity"):
        ds = BUILDERS[name](frames, settings.as_of)
        assert not LEAKY & set(ds.features), f"{name} uses a label-defining column"


def test_task_outcome_rules():
    t = pd.DataFrame({
        "task_id": [1, 2, 3, 4, 5],
        "status": ["ToDo", "Done", "Done", "InProgress", "Done"],
        "due_date": pd.to_datetime(["2026-09-01", "2026-09-01", "2026-09-01", "2026-12-01", "2026-12-01"]),
        "update_time": pd.to_datetime([None, "2026-08-30", "2026-09-10", None, None]),
    })
    out = task_outcomes(t, "2026-09-30").set_index("task_id")
    assert out.loc[1, "late"] == 1 and out.loc[1, "resolved"]          # open & overdue
    assert out.loc[2, "late"] == 0                                     # done on time
    assert out.loc[3, "late"] == 1                                     # done after due date
    assert not out.loc[4, "resolved"]                                  # future -> prediction target
    assert out.loc[5, "resolved"] and out.loc[5, "late"] == 0          # done early


def test_severity_bands():
    assert [severity_band(s) for s in (1, 6, 7, 11, 12, 25)] == ["Low", "Low", "Medium", "Medium", "High", "High"]


def test_every_builder_runs_on_csv(frames, settings):
    for name, build in BUILDERS.items():
        ds = build(frames, settings.as_of)
        assert len(ds.X) > 0, name
        assert set(ds.features) <= set(ds.X.columns), name


def test_spend_forecast_trains_only_on_past_months(frames, settings):
    ds = BUILDERS["spend_forecast"](frames, settings.as_of)
    current = pd.Timestamp(settings.as_of).to_period("M").to_timestamp()
    assert (ds.extra["months"] < current).all()
