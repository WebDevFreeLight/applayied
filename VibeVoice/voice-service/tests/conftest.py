import pytest
from fastapi.testclient import TestClient

from app.config import Settings
from app.main import create_app


def make_settings(**overrides) -> Settings:
    base = dict(tts_engine="dev", arabic_tts_engine="dev", fallback_tts_engine="dev", asr_engine="none",
                preload_models=False, log_level="WARNING")
    base.update(overrides)
    return Settings(_env_file=None, **base)


@pytest.fixture()
def client():
    with TestClient(create_app(make_settings())) as c:
        yield c
