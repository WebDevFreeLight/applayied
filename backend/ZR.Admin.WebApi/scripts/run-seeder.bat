@echo off
REM Finance Data Seeder - Batch Wrapper
REM This batch file simplifies running the PowerShell seeder script

echo ========================================
echo Finance Data Seeder
echo ========================================
echo.

REM Check if PowerShell is available
powershell -Command "exit 0" >nul 2>&1
if errorlevel 1 (
    echo ERROR: PowerShell is not available or not in PATH
    echo Please install PowerShell or add it to your PATH
    pause
    exit /b 1
)

REM Get the directory where this batch file is located
set SCRIPT_DIR=%~dp0

REM Run the PowerShell script with all arguments passed through
powershell.exe -ExecutionPolicy Bypass -File "%SCRIPT_DIR%seed-finance-data.ps1" %*

echo.
echo Press any key to exit...
pause >nul





