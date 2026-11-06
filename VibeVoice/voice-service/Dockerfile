# VibeVoice voice service
#   Light image (edge-tts + faster-whisper):   docker build -t vibevoice-voice .
#   With Microsoft VibeVoice (large, torch):   docker build --build-arg INSTALL_VIBEVOICE=true -t vibevoice-voice:full .
# For GPU inference use an NVIDIA CUDA base image / torch CUDA wheels and run with --gpus all.
FROM python:3.11-slim

ARG INSTALL_VIBEVOICE=false
ARG VIBEVOICE_REPO=https://github.com/microsoft/VibeVoice.git

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1 \
    HF_HOME=/data/hf \
    VIBEVOICE_VOICES_DIR=/app/voices/streaming_model

RUN apt-get update \
 && apt-get install -y --no-install-recommends ffmpeg libsndfile1 git ca-certificates curl \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY requirements.txt requirements-vibevoice.txt ./
RUN pip install -r requirements.txt

# Optional VibeVoice install, straight from upstream (official voice presets only).
RUN if [ "$INSTALL_VIBEVOICE" = "true" ]; then \
      git clone --depth 1 "$VIBEVOICE_REPO" /opt/VibeVoice \
      && pip install -e "/opt/VibeVoice[streamingtts]" \
      && mkdir -p /app/voices \
      && cp -r /opt/VibeVoice/demo/voices/streaming_model /app/voices/streaming_model ; \
    else echo "Skipping VibeVoice install (INSTALL_VIBEVOICE=$INSTALL_VIBEVOICE)"; fi

COPY app ./app

RUN useradd --create-home --uid 1000 voice && mkdir -p /data/hf && chown -R voice /app /data
USER voice
VOLUME ["/data"]
EXPOSE 8001

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD curl -fsS http://127.0.0.1:8001/health || exit 1

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8001", "--no-access-log"]
