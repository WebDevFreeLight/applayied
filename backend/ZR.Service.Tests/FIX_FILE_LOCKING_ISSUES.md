# How to Fix File Locking Issues - حل مشاكل قفل الملفات

## Problem - المشكلة

When building, you see errors like:
```
error MSB3027: Could not copy "...ZR.Infrastructure.pdb" to "...ZR.Infrastructure.pdb". 
The file is locked by: "Microsoft Visual Studio 2022 (10912), ZR.Admin.WebApi (23844)"
```

This happens when:
- ✅ Visual Studio is running the application
- ✅ The application (ZR.Admin.WebApi) is still running
- ✅ Another process is using the files

## Solutions - الحلول

### Solution 1: Stop Running Application (Quick Fix)

**In PowerShell:**
```powershell
# Find and stop ZR.Admin.WebApi
Get-Process | Where-Object {$_.ProcessName -like "*ZR.Admin*"} | Stop-Process -Force

# Or stop by process ID
Stop-Process -Id 23844 -Force
```

**In Visual Studio:**
1. Click the **Stop** button (red square) in the toolbar
2. Or press `Shift + F5` to stop debugging

### Solution 2: Close Visual Studio

1. Close Visual Studio completely
2. Build from command line: `dotnet build`
3. Reopen Visual Studio if needed

### Solution 3: Kill All Related Processes

```powershell
# Stop all dotnet processes (be careful - this stops ALL dotnet processes)
Get-Process dotnet | Stop-Process -Force

# Stop ZR.Admin.WebApi specifically
Get-Process | Where-Object {$_.ProcessName -like "*ZR.Admin*"} | Stop-Process -Force
```

### Solution 4: Restart Visual Studio

1. Close Visual Studio
2. Wait a few seconds
3. Reopen Visual Studio
4. Try building again

### Solution 5: Clean and Rebuild

```powershell
# Clean solution
dotnet clean

# Stop any running processes
Get-Process | Where-Object {$_.ProcessName -like "*ZR.Admin*"} | Stop-Process -Force

# Rebuild
dotnet build
```

## Quick PowerShell Script

Save this as `fix-locks.ps1`:

```powershell
# Stop ZR.Admin.WebApi if running
$processes = Get-Process | Where-Object {$_.ProcessName -like "*ZR.Admin*"}
if ($processes) {
    Write-Host "Stopping ZR.Admin.WebApi processes..." -ForegroundColor Yellow
    $processes | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "Stopped!" -ForegroundColor Green
} else {
    Write-Host "No ZR.Admin.WebApi processes found." -ForegroundColor Green
}

# Clean and rebuild
Write-Host "Cleaning solution..." -ForegroundColor Yellow
dotnet clean

Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build
```

Run it:
```powershell
.\fix-locks.ps1
```

## Prevention - الوقاية

### Before Building:
1. ✅ Stop the running application (Shift + F5 in VS)
2. ✅ Check if application is running in Task Manager
3. ✅ Close any debug sessions

### In Visual Studio:
- Use **Stop Debugging** (Shift + F5) before rebuilding
- Don't run the application while building

## Check What's Locking Files

```powershell
# Find processes using specific files
Get-Process | Where-Object {
    $_.Modules | Where-Object {
        $_.FileName -like "*ZR.Admin*" -or 
        $_.FileName -like "*ZR.Infrastructure*"
    }
} | Select-Object ProcessName, Id
```

## Summary

**Quick Fix:**
```powershell
Get-Process | Where-Object {$_.ProcessName -like "*ZR.Admin*"} | Stop-Process -Force
dotnet build
```

**If that doesn't work:**
1. Close Visual Studio
2. Wait 5 seconds
3. Reopen and try again





























