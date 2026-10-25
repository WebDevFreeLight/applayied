import importlib

import pytest

from pmo_ml import registry
from pmo_ml.problems import PROBLEMS


def test_pipeline_trains_every_problem(trained, settings):
    for problem in PROBLEMS:
        res = trained["results"][problem]
        assert res["status"] == "trained", res
        model, meta = registry.load(settings, problem)
        assert model is not None and meta["version"] == res["version"]
        assert meta["candidates"], problem
    assert (settings.path("reports_dir") / "eda" / "EDA.md").exists()


def test_supervised_models_report_baseline(trained, settings):
    for problem in ("task_delay", "risk_severity", "spend_forecast"):
        meta = registry.load_metadata(settings, problem)
        assert meta["baseline"]["metrics"], problem
        assert any(c["name"].startswith("baseline") for c in meta["candidates"])


@pytest.fixture(scope="module")
def client(trained):
    from fastapi.testclient import TestClient
    import pmo_ml.api as api
    importlib.reload(api)                       # pick up the test config
    with TestClient(api.app) as c:
        yield c


def test_health_never_leaks_password(client):
    body = client.get("/health").json()
    assert body["status"] == "ok"
    assert "password" not in str(body).lower()


@pytest.mark.parametrize("url", ["/models", "/predict/task-delay", "/forecast/spend", "/anomalies/costs?top=3",
                                 "/segments", "/monitoring", "/training/history"])
def test_get_endpoints(client, url):
    r = client.get(url)
    assert r.status_code == 200, r.text


def test_arabic_labels(client):
    names = [m["name"] for m in client.get("/models?lang=ar").json()["models"]]
    assert "خطر تأخر المهمة" in names
    seg = client.get("/segments?lang=ar").json()
    assert all(any("؀" <= ch <= "ۿ" for ch in i["segment_label"]) for i in seg["items"])


def test_risk_severity_suggestion(client):
    r = client.post("/predict/risk-severity", json={"text": "Vendor may miss the hardware delivery date"}).json()
    assert r["severity"] in {"Low", "Medium", "High"}
    assert abs(sum(r["probabilities"].values()) - 1) < 0.01      # each value is rounded to 3 dp


def test_ids_are_strings(client):
    items = client.get("/predict/task-delay").json()["items"]
    assert items and all(isinstance(i["task_id"], str) for i in items)


def test_api_key_enforced(client, monkeypatch):
    import pmo_ml.api as api
    monkeypatch.setitem(api.settings.raw["api"], "api_key", "secret")
    assert client.get("/models").status_code == 401
    assert client.get("/models", headers={"X-ML-Key": "secret"}).status_code == 200
    assert client.get("/health").status_code == 200          # health stays open for probes
