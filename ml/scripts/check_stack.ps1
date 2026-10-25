# Read-only health check of the whole local stack. Run it whenever something "doesn't load".
#   powershell -ExecutionPolicy Bypass -File ml\scripts\check_stack.ps1
param(
    [int]$ApiPort = 63873,
    [int]$MlPort = 8001,
    [int]$WebPort = 5173
)
$ml = Split-Path -Parent $PSScriptRoot
$py = Join-Path $ml ".venv\Scripts\python.exe"

function Test-Port([int]$port) {
    [bool](Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue)
}
function Show([string]$name, [bool]$ok, [string]$detail, [string]$fix) {
    $mark = if ($ok) { "[ OK ]" } else { "[FAIL]" }
    $color = if ($ok) { "Green" } else { "Red" }
    Write-Host ("{0} {1,-22} {2}" -f $mark, $name, $detail) -ForegroundColor $color
    if (-not $ok -and $fix) { Write-Host "       fix: $fix" -ForegroundColor Yellow }
}

Write-Host "PMO stack check" -ForegroundColor Cyan

# 1. PostgreSQL
Show "PostgreSQL :5432" (Test-Port 5432) "" "start the PostgreSQL service (services.msc) or your Docker container"

# 2. Python env + DB access from the ML project (reads appsettings.json, never prints the password)
if (Test-Path $py) {
    $db = & $py -c "from pmo_ml.config import load_settings; from pmo_ml.db import get_engine, describe_target; from sqlalchemy import text; s=load_settings(); e=get_engine(s); c=e.connect(); n=c.execute(text('select count(*) from pmo_task')).scalar(); print(describe_target(s), '-', n, 'tasks')" 2>&1
    Show "ML -> database" ($LASTEXITCODE -eq 0) ($db | Select-Object -Last 1) "check dbConfigs[0].Conn in backend\ZR.Admin.WebApi\appsettings.json"
    $models = & $py -c "from pmo_ml.config import load_settings; from pmo_ml import registry; from pmo_ml.features import BUILDERS; s=load_settings(); v=[p for p in BUILDERS if registry.current_version(s,p)]; print(len(v),'of',len(BUILDERS),'models trained')" 2>&1
    Show "ML models" ($models -match "^5 of 5") ($models | Select-Object -Last 1) "powershell -File ml\scripts\run_pipeline.ps1"
} else {
    Show "ML virtualenv" $false "ml\.venv missing" "powershell -File ml\scripts\setup.ps1"
}

# 3. ML service
try {
    $h = Invoke-RestMethod "http://localhost:$MlPort/health" -TimeoutSec 5
    Show "ML service :$MlPort" ($h.status -eq "ok") "db=$($h.database), scheduler=$($h.scheduler)" ""
} catch {
    Show "ML service :$MlPort" $false "not answering" "ml\.venv\Scripts\python -m pmo_ml serve"
}

# 4. .NET API (401 without a token = it is up and protecting /pmo/ml)
try {
    $r = Invoke-WebRequest "http://localhost:$ApiPort/pmo/ml/status" -TimeoutSec 10 -UseBasicParsing
    Show ".NET API :$ApiPort" ($r.Content -match '"code":\s*401|"code":\s*200') "answers /pmo/ml/status" ""
} catch {
    Show ".NET API :$ApiPort" $false "not answering" "see step 3 of ml\docs\13_RUN_AND_DEBUG.md (set ASPNETCORE_ENVIRONMENT=Development)"
}

# 5. Frontend dev server
Show "Frontend :$WebPort" (Test-Port $WebPort) "http://localhost:$WebPort" "cd frontend; npm run dev"
