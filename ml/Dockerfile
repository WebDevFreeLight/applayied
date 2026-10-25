# PMO ML service image (FastAPI + scikit-learn).
#   docker build -t pmo-ml ml
#   docker run -p 8001:8001 -e DATABASE_URL=postgres://user:pass@host:5432/db pmo-ml
# In a container there is no appsettings.json next to it, so give the database through
# DATABASE_URL (Render / Neon style) or PMO_DB_CONN (Npgsql style). Models are trained on
# first start (api.train_on_startup_if_missing) and then by the built-in scheduler.
FROM python:3.13-slim
WORKDIR /app
ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1 MPLBACKEND=Agg
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY pyproject.toml README.md ./
COPY config ./config
COPY src ./src
RUN pip install --no-cache-dir -e .
EXPOSE 8001
# PORT is set by Render/Railway/Fly; default 8001
CMD ["sh", "-c", "python -m pmo_ml serve --host 0.0.0.0 --port ${PORT:-8001}"]
