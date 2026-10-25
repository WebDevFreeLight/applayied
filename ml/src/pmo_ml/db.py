"""Database access: reuse the .NET API's PostgreSQL connection string.

Resolution order (first match wins):
  1. PMO_DB_CONN   - Npgsql-style "Host=..;Port=..;Database=..;Username=..;Password=.."
  2. DATABASE_URL  - postgres://user:pass@host:port/db   (what Render/Neon provide)
  3. appsettings.json -> dbConfigs[<index>].Conn          (the default: same DB as the portal)
"""

from __future__ import annotations

import json
import os
import re
from pathlib import Path
from urllib.parse import unquote, urlparse

from sqlalchemy import URL, create_engine
from sqlalchemy.engine import Engine

from .config import Settings, resolve


def strip_json_comments(text: str) -> str:
    """Remove // and /* */ comments and trailing commas (appsettings.json allows them)."""
    out: list[str] = []
    i, n, in_str = 0, len(text), False
    while i < n:
        c = text[i]
        if in_str:
            out.append(c)
            if c == "\\" and i + 1 < n:
                out.append(text[i + 1])
                i += 2
                continue
            if c == '"':
                in_str = False
        elif c == '"':
            in_str = True
            out.append(c)
        elif text.startswith("//", i):
            while i < n and text[i] != "\n":
                i += 1
            continue
        elif text.startswith("/*", i):
            end = text.find("*/", i + 2)
            i = n if end < 0 else end + 2
            continue
        else:
            out.append(c)
        i += 1
    return re.sub(r",(\s*[}\]])", r"\1", "".join(out))


def parse_npgsql(conn: str) -> dict[str, str]:
    """'Host=a;Port=5432;Database=b;Username=u;Password=p' -> lower-cased dict."""
    pairs = (part.split("=", 1) for part in conn.split(";") if "=" in part)
    return {k.strip().lower().replace(" ", ""): v.strip() for k, v in pairs}


def conn_from_appsettings(path: Path, index: int = 0) -> str:
    cfg = json.loads(strip_json_comments(path.read_text(encoding="utf-8-sig")))
    configs = next((v for k, v in cfg.items() if k.lower() == "dbconfigs"), None)
    if not configs:
        raise ValueError(f"No dbConfigs in {path}")
    return configs[index]["Conn"]


def sqlalchemy_url(settings: Settings) -> URL:
    if os.environ.get("PMO_DB_CONN"):
        kv = parse_npgsql(os.environ["PMO_DB_CONN"])
    elif os.environ.get("DATABASE_URL"):
        u = urlparse(os.environ["DATABASE_URL"])
        return URL.create(
            "postgresql+psycopg2",
            username=unquote(u.username or ""),
            password=unquote(u.password or ""),
            host=u.hostname,
            port=u.port or 5432,
            database=u.path.lstrip("/"),
            query={"sslmode": "require"},
        )
    else:
        data = settings.section("data")
        kv = parse_npgsql(conn_from_appsettings(resolve(data["appsettings_path"]),
                                                int(data.get("db_config_index", 0))))
    query = {}
    if kv.get("sslmode"):
        query["sslmode"] = kv["sslmode"].lower()
    return URL.create(
        "postgresql+psycopg2",
        username=kv.get("username") or kv.get("userid") or kv.get("user"),
        password=kv.get("password"),
        host=kv.get("host") or kv.get("server"),
        port=int(kv.get("port", 5432)),
        database=kv.get("database"),
        query=query,
    )


def describe_target(settings: Settings) -> str:
    """host:port/db as user — never the password (safe for logs and /health)."""
    url = sqlalchemy_url(settings)
    return f"{url.host}:{url.port}/{url.database} as {url.username}"


def get_engine(settings: Settings) -> Engine:
    return create_engine(sqlalchemy_url(settings), pool_pre_ping=True)
