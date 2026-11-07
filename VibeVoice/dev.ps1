<#
  Starts the VibeVoice stack locally in three windows:
    voice-service  http://localhost:8001   (FastAPI, Python venv in voice-service/.venv)
    api            http://localhost:5080   (ASP.NET Core, Swagger at /swagger)
    frontend       http://localhost:5173   (Vite)
  Usage:  ./dev.ps1            # all three
          ./dev.ps1 -NoVoice   # skip the Python service (TTS/ASR return 503, everything else works)
#>
param([switch]$NoVoice)
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

if (-not $NoVoice) {
    $voice = Join-Path $root "voice-service"
    $venvPython = Join-Path $voice ".venv\Scripts\python.exe"
    if (-not (Test-Path $venvPython)) {
        Write-Host "Creating Python venv for the voice service..." -ForegroundColor Cyan
        python -m venv (Join-Path $voice ".venv")
        & $venvPython -m pip install -r (Join-Path $voice "requirements.txt")
    }
    Start-Process pwsh -WorkingDirectory $voice -ArgumentList "-NoExit", "-Command",
        "`$host.UI.RawUI.WindowTitle='voice-service'; & '$venvPython' -m uvicorn app.main:app --host 127.0.0.1 --port 8001"
}

$api = Join-Path $root "backend\VibeVoice.Api"
Start-Process pwsh -WorkingDirectory $api -ArgumentList "-NoExit", "-Command",
    "`$host.UI.RawUI.WindowTitle='api'; dotnet run --launch-profile http"

$web = Join-Path $root "frontend"
if (-not (Test-Path (Join-Path $web "node_modules"))) {
    Write-Host "Installing frontend packages..." -ForegroundColor Cyan
    Push-Location $web; npm install; Pop-Location
}
Start-Process pwsh -WorkingDirectory $web -ArgumentList "-NoExit", "-Command",
    "`$host.UI.RawUI.WindowTitle='frontend'; npm run dev:lan"

Write-Host ""
Write-Host "VibeVoice is starting:" -ForegroundColor Green
Write-Host "  App      http://localhost:5173   (demo: admin@vibevoice.local / Admin123!)"
Write-Host "  API      http://localhost:5080/swagger"
if (-not $NoVoice) { Write-Host "  Voice    http://localhost:8001/docs" }
