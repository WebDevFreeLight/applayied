# OPTIONAL: register a Windows scheduled task that checks the models every night and retrains
# only when monitoring recommends it (drift, new data, live-accuracy drop).
# You do not need this when the API service runs: it already has a built-in scheduler
# (config.yaml -> api.auto_retrain_hours). Use it when you train without running the API.
#
#   powershell -ExecutionPolicy Bypass -File ml\scripts\register_retrain_task.ps1 [-At "02:30"]
#   Remove later with:  Unregister-ScheduledTask -TaskName "PMO ML retrain" -Confirm:$false
param([string]$At = "02:30")
$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "run_pipeline.ps1"
$action = New-ScheduledTaskAction -Execute "powershell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$script`" -IfNeeded"
$trigger = New-ScheduledTaskTrigger -Daily -At $At
Register-ScheduledTask -TaskName "PMO ML retrain" -Action $action -Trigger $trigger `
    -Description "PMO Portal: retrain ML models when monitoring recommends it" -Force | Out-Null
Write-Host "Registered 'PMO ML retrain' daily at $At."
