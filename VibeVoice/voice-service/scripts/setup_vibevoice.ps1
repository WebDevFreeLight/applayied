# Installs Microsoft VibeVoice into voice-service/.venv and copies the official voice presets.
# Usage (from anywhere):  powershell -ExecutionPolicy Bypass -File voice-service/scripts/setup_vibevoice.ps1 [-Cuda cu124] [-DownloadModel]
param(
    [string]$Cuda = "",          # e.g. "cu124" to install a CUDA build of torch first
    [switch]$DownloadModel       # also download microsoft/VibeVoice-Realtime-0.5B into models/
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot           # voice-service/
$third = Join-Path $root "third_party"
$repo = Join-Path $third "VibeVoice"
$py = Join-Path $root ".venv\Scripts\python.exe"

if (-not (Test-Path $py)) {
    Write-Host "Creating venv at $root\.venv"
    python -m venv (Join-Path $root ".venv")
    & $py -m pip install --upgrade pip
    & $py -m pip install -r (Join-Path $root "requirements.txt")
}

New-Item -ItemType Directory -Force $third | Out-Null
if (Test-Path (Join-Path $repo ".git")) {
    Write-Host "Updating $repo"
    git -C $repo pull --ff-only
} else {
    git clone --depth 1 https://github.com/microsoft/VibeVoice.git $repo
}

if ($Cuda) {
    Write-Host "Installing torch for $Cuda"
    & $py -m pip install torch --index-url "https://download.pytorch.org/whl/$Cuda"
}
& $py -m pip install -e "$repo[streamingtts]"

$src = Join-Path $repo "demo\voices\streaming_model"
$dst = Join-Path $root "voices\streaming_model"
New-Item -ItemType Directory -Force $dst | Out-Null
Copy-Item -Path (Join-Path $src "*.pt") -Destination $dst -Force
Write-Host "Copied $((Get-ChildItem $dst -Filter *.pt).Count) official voice presets to $dst"

if ($DownloadModel) {
    # huggingface_hub >= 1.0 ships the `hf` CLI; older versions only have `huggingface-cli`.
    $hf = Join-Path $root ".venv\Scripts\hf.exe"
    if (-not (Test-Path $hf)) { $hf = Join-Path $root ".venv\Scripts\huggingface-cli.exe" }
    & $hf download microsoft/VibeVoice-Realtime-0.5B --local-dir (Join-Path $root "models\VibeVoice-Realtime-0.5B")
    Write-Host "Set VIBEVOICE_MODEL_PATH=models/VibeVoice-Realtime-0.5B in .env"
}
Write-Host "Done. Start with: TTS_ENGINE=vibevoice  .venv\Scripts\python -m uvicorn app.main:app --port 8001"
