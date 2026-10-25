# Start the whole local stack, one window per service, in dependency order:
#   1. ML service (:8001)  2. .NET API (:63873)  3. React frontend (:5173)
# PostgreSQL must already be running. Run from the repository folder:
#   powershell -ExecutionPolicy Bypass -File ml\scripts\start_all.ps1 [-Train] [-SkipMl]
param(
    [switch]$Train,        # retrain all models before starting the ML service
    [switch]$SkipMl,       # start only the API and the frontend
    [int]$ApiPort = 63873
)
$ErrorActionPreference = "Stop"
$ml = Split-Path -Parent $PSScriptRoot
$root = Split-Path -Parent $ml

if (-not (Get-NetTCPConnection -State Listen -LocalPort 5432 -ErrorAction SilentlyContinue)) {
    throw "PostgreSQL is not listening on 5432 - start it first."
}

function Start-Window([string]$title, [string]$dir, [string]$command) {
    $cmd = "`$Host.UI.RawUI.WindowTitle = '$title'; Set-Location '$dir'; $command"
    Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-Command", $cmd | Out-Null
    Write-Host "started: $title"
}

if (-not $SkipMl) {
    if (-not (Test-Path (Join-Path $ml ".venv"))) { & (Join-Path $PSScriptRoot "setup.ps1") }
    $mlCmd = ".\.venv\Scripts\python -m pmo_ml serve"
    if ($Train) { $mlCmd = ".\.venv\Scripts\python -m pmo_ml train; $mlCmd" }
    Start-Window "PMO ML :8001" $ml $mlCmd
}

# Development = local DB from appsettings.json (+ git-ignored appsettings.Development.json).
# Without it, dotnet would load appsettings.Production.json (remote DB, HTTPS certificate).
Start-Window "PMO API :$ApiPort" $root `
    "`$env:ASPNETCORE_ENVIRONMENT='Development'; `$env:ASPNETCORE_URLS='http://localhost:$ApiPort'; dotnet run --project backend\ZR.Admin.WebApi"

$web = Join-Path $root "frontend"
$webCmd = if (Test-Path (Join-Path $web "node_modules")) { "npm run dev" } else { "npm install; npm run dev" }
Start-Window "PMO Web :5173" $web $webCmd

Write-Host ""
Write-Host "Give the API ~30-60 s, then open http://localhost:5173 and sign in (pmo.admin / 123456)."
Write-Host "Check everything with: powershell -File ml\scripts\check_stack.ps1"
