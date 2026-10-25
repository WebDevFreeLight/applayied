# One-time setup: virtual environment + dependencies + the pmo_ml package (editable).
# Run from anywhere:  powershell -ExecutionPolicy Bypass -File ml\scripts\setup.ps1
$ErrorActionPreference = "Stop"
$ml = Split-Path -Parent $PSScriptRoot
Set-Location $ml

if (-not (Test-Path ".venv")) {
    Write-Host "Creating virtual environment in $ml\.venv"
    python -m venv .venv
}
$py = Join-Path $ml ".venv\Scripts\python.exe"
& $py -m pip install --upgrade pip
& $py -m pip install -r requirements.txt
& $py -m pip install -e .
& $py -c "import sklearn, fastapi; print('pmo_ml ready - scikit-learn', sklearn.__version__)"
Write-Host "Next: .\scripts\run_pipeline.ps1   then   .venv\Scripts\python -m pmo_ml serve"
