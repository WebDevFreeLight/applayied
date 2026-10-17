@echo off
REM Deploy to Synology NAS - Windows Batch Script
REM This script helps deploy the Docker setup to your Synology NAS

setlocal enabledelayedexpansion

echo ============================================
echo   Deploy to Synology NAS - Setup Assistant
echo ============================================
echo.

REM Get Synology IP
set /p SYNOLOGY_IP="Enter your Synology NAS IP address: "
set /p SYNOLOGY_USER="Enter your Synology username (admin): "
if "%SYNOLOGY_USER%"=="" set SYNOLOGY_USER=admin

echo.
echo Checking connection to %SYNOLOGY_IP%...
ping -n 1 %SYNOLOGY_IP% >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Cannot reach Synology at %SYNOLOGY_IP%
    echo Please check the IP address and network connection.
    pause
    exit /b 1
)

echo Connection successful!
echo.

REM Check if SSH is available
echo Testing SSH connection...
echo.
echo NOTE: If SSH fails, please enable it in:
echo   DSM Control Panel -^> Terminal ^& SNMP -^> Enable SSH service
echo.

REM Create deployment package
echo Creating deployment package...

REM Create temp directory
set TEMP_DIR=%TEMP%\synology-deploy-%RANDOM%
mkdir "%TEMP_DIR%" 2>nul

REM Copy necessary files
copy synology-setup.sh "%TEMP_DIR%\" >nul 2>&1
copy docker-compose.yml "%TEMP_DIR%\docker-compose.yml" >nul 2>&1
copy appsettings.Docker.json "%TEMP_DIR%\appsettings.json" >nul 2>&1
copy ZR.Admin.WebApi\Dockerfile "%TEMP_DIR%\Dockerfile" >nul 2>&1

REM Create deployment info
(
echo Deployment Information
echo ======================
echo Date: %DATE% %TIME%
echo Synology IP: %SYNOLOGY_IP%
echo User: %SYNOLOGY_USER%
echo.
echo Files included:
echo - synology-setup.sh
echo - docker-compose.yml
echo - appsettings.json
echo - Dockerfile
) > "%TEMP_DIR%\deployment-info.txt"

echo.
echo ============================================
echo   Deployment Options
echo ============================================
echo.
echo 1. Automatic deployment (requires SSH)
echo 2. Manual deployment instructions
echo 3. Generate SCP commands only
echo 4. Exit
echo.
set /p OPTION="Select option (1-4): "

if "%OPTION%"=="1" goto :automatic
if "%OPTION%"=="2" goto :manual
if "%OPTION%"=="3" goto :scp_only
if "%OPTION%"=="4" goto :end

:automatic
echo.
echo Starting automatic deployment...
echo.

REM Check for SSH client
where ssh >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: SSH client not found. 
    echo Please install OpenSSH or use manual deployment.
    echo.
    echo You can enable OpenSSH client on Windows 10/11:
    echo   Settings -^> Apps -^> Optional Features -^> Add Feature -^> OpenSSH Client
    echo.
    pause
    goto :manual
)

echo Uploading files to Synology...
echo.

REM Create directory on Synology
ssh %SYNOLOGY_USER%@%SYNOLOGY_IP% "mkdir -p /volume1/docker/maham" 2>nul

REM Upload files
scp "%TEMP_DIR%\synology-setup.sh" %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/
scp "%TEMP_DIR%\docker-compose.yml" %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/
scp "%TEMP_DIR%\appsettings.json" %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/
scp "%TEMP_DIR%\Dockerfile" %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/

echo.
echo Running setup script on Synology...
ssh %SYNOLOGY_USER%@%SYNOLOGY_IP% "cd /volume1/docker/maham && sudo bash synology-setup.sh"

echo.
echo Deployment complete!
goto :show_summary

:manual
echo.
echo ============================================
echo   Manual Deployment Instructions
echo ============================================
echo.
echo 1. Enable SSH on your Synology:
echo    DSM Control Panel -^> Terminal ^& SNMP -^> Enable SSH service
echo.
echo 2. Connect to your Synology via SSH:
echo    ssh %SYNOLOGY_USER%@%SYNOLOGY_IP%
echo.
echo 3. Create the directory:
echo    sudo mkdir -p /volume1/docker/maham
echo.
echo 4. Upload these files to /volume1/docker/maham/:
echo    - synology-setup.sh
echo    - docker-compose.yml
echo    - appsettings.json
echo    - Your Dockerfile and source code
echo.
echo    You can use DSM File Station or SCP:
echo    scp synology-setup.sh %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/
echo.
echo 5. Run the setup script:
echo    cd /volume1/docker/maham
echo    sudo bash synology-setup.sh
echo.
echo 6. Configure firewall in DSM:
echo    Control Panel -^> Security -^> Firewall
echo    Add rules for ports: 5433 (PostgreSQL) and 8888 (API)
echo.
pause
goto :show_summary

:scp_only
echo.
echo ============================================
echo   SCP Commands for File Transfer
echo ============================================
echo.
echo Copy and run these commands:
echo.
echo REM Create directory:
echo ssh %SYNOLOGY_USER%@%SYNOLOGY_IP% "sudo mkdir -p /volume1/docker/maham"
echo.
echo REM Upload setup script:
echo scp synology-setup.sh %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/
echo.
echo REM Upload Docker Compose:
echo scp docker-compose.yml %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/
echo.
echo REM Upload settings:
echo scp appsettings.Docker.json %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/appsettings.json
echo.
echo REM Upload Dockerfile:
echo scp ZR.Admin.WebApi\Dockerfile %SYNOLOGY_USER%@%SYNOLOGY_IP%:/volume1/docker/maham/
echo.
echo REM Run setup:
echo ssh %SYNOLOGY_USER%@%SYNOLOGY_IP% "cd /volume1/docker/maham && sudo bash synology-setup.sh"
echo.
pause
goto :show_summary

:show_summary
echo.
echo ============================================
echo   Post-Deployment Configuration
echo ============================================
echo.
echo After deployment, access your services at:
echo.
echo   PostgreSQL: %SYNOLOGY_IP%:5433
echo   API: http://%SYNOLOGY_IP%:8888
echo.
echo Default PostgreSQL credentials:
echo   Username: postgres
echo   Password: postgres123
echo   Database: MahamDB
echo.
echo Connection string for your application:
echo   Host=%SYNOLOGY_IP%;Port=5433;Database=MahamDB;Username=postgres;Password=postgres123;
echo.
echo Configure auto-start in DSM:
echo   1. Control Panel -^> Task Scheduler
echo   2. Create -^> Triggered Task -^> User-defined script
echo   3. Event: Boot-up
echo   4. Script: bash /volume1/docker/maham/scripts/synology-task.sh
echo.
echo Monitor your containers:
echo   ssh %SYNOLOGY_USER%@%SYNOLOGY_IP% "bash /volume1/docker/maham/scripts/monitor.sh"
echo.

REM Clean up temp files
if exist "%TEMP_DIR%" rd /s /q "%TEMP_DIR%" 2>nul

:end
echo.
echo Press any key to exit...
pause >nul
exit /b 0