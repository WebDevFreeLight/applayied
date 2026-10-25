import json

from pmo_ml.config import Settings
from pmo_ml.db import conn_from_appsettings, parse_npgsql, sqlalchemy_url, strip_json_comments


def test_strip_comments_keeps_urls_and_strings():
    text = '{\n  // comment\n  "a": "http://x//y", /* block */ "b": [1, 2,],\n}'
    assert json.loads(strip_json_comments(text)) == {"a": "http://x//y", "b": [1, 2]}


def test_parse_npgsql():
    kv = parse_npgsql("Host=db;Port=5433;Database=MahamDB;Username=u;Password=p=1;SSL Mode=Require;")
    assert kv == {"host": "db", "port": "5433", "database": "MahamDB", "username": "u", "password": "p=1",
                  "sslmode": "Require"}


def test_conn_from_appsettings(tmp_path):
    f = tmp_path / "appsettings.json"
    f.write_text('{ // c\n "dbConfigs": [ { "Conn": "Host=h;Database=d;Username=u;Password=p", } ] }',
                 encoding="utf-8")
    assert conn_from_appsettings(f).startswith("Host=h")


def test_env_overrides(monkeypatch):
    s = Settings({"data": {"appsettings_path": "missing.json"}})
    monkeypatch.setenv("DATABASE_URL", "postgres://user:p%40ss@example.com:6543/pmo")
    url = sqlalchemy_url(s)
    assert (url.host, url.port, url.database, url.username, url.password) == \
        ("example.com", 6543, "pmo", "user", "p@ss")
    monkeypatch.delenv("DATABASE_URL")
    monkeypatch.setenv("PMO_DB_CONN", "Host=a;Port=5432;Database=b;Username=c;Password=d")
    assert sqlalchemy_url(s).host == "a"
