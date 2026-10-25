# Full ML pipeline: collect -> clean -> EDA -> features -> train/select -> evaluate -> registry.
#   powershell -ExecutionPolicy Bypass -File ml\scripts\run_pipeline.ps1            # all models
#   powershell -ExecutionPolicy Bypass -File ml\scripts\run_pipeline.ps1 -IfNeeded  # only when monitoring says so
#   ... -Source csv                                                                 # train on the CSV seed
param(
    [switch]$IfNeeded,
    [ValidateSet("db", "csv")][string]$Source = "db"
)
$ErrorActionPreference = "Stop"
$ml = Split-Path -Parent $PSScriptRoot
Set-Location $ml
$py = Join-Path $ml ".venv\Scripts\python.exe"
if (-not (Test-Path $py)) { throw "Run scripts\setup.ps1 first." }

$log = Join-Path $ml "reports\pipeline.log"
New-Item -ItemType Directory -Force (Split-Path $log) | Out-Null
"[$(Get-Date -Format s)] start (IfNeeded=$IfNeeded, Source=$Source)" | Add-Content $log

if ($IfNeeded) {
    New-Item -ItemType Directory -Force (Join-Path $ml "reports\monitoring") | Out-Null
    & $py -m pmo_ml monitor | Out-File (Join-Path $ml "reports\monitoring\last_check.json") -Encoding utf8
    if ($LASTEXITCODE -ne 3) {
        "[$(Get-Date -Format s)] monitoring: no retraining needed" | Add-Content $log
        Write-Host "No retraining needed."
        exit 0
    }
    "[$(Get-Date -Format s)] monitoring: retraining recommended" | Add-Content $log
}

& $py -m pmo_ml train --source $Source
$code = $LASTEXITCODE
"[$(Get-Date -Format s)] train exit code $code" | Add-Content $log
exit $code
